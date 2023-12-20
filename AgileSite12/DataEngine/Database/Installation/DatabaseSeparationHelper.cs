using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Sets of methods used for database separation
    /// </summary>
    public class DatabaseSeparationHelper
    {
        #region "Variables"

        /// <summary>
        /// On-line marketing connection string name
        /// </summary>
        public const string OM_CONNECTION_STRING = "CMSOMConnectionString";

        private SeparatedTables separatedTables;
        private SqlConnectionStringBuilder connectionStringSource;
        private SqlConnectionStringBuilder connectionStringTarget;
        private bool isSeparation;
        private ISqlServerCapabilities mSqlServerCapabilities;
        private SqlInstallationHelper.LogProgress mLogMessage = (m, t, e) => { };

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if DB separation is in progress.
        /// </summary>
        public static bool SeparationInProgress
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue("CMSApplicationState") == "DBSeparationInProgress";
            }
            set
            {
                if (value && (SettingsKeyInfoProvider.GetValue("CMSApplicationState") != "DBSeparationInProgress"))
                {
                    SettingsKeyInfoProvider.SetGlobalValue("CMSApplicationState", "DBSeparationInProgress");
                }
                else if (!value && !String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSApplicationState")))
                {
                    SettingsKeyInfoProvider.SetGlobalValue("CMSApplicationState", null);
                }
            }
        }


        /// <summary>
        /// Indicates what web farm server started DB separation.
        /// </summary>
        public static string SeparationStartedByServer
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue("CMSDBSeparationStartedByServer");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSDBSeparationStartedByServer", value);
            }
        }


        /// <summary>
        /// Folder where SQL separation scripts are placed.
        /// </summary>
        public string ScriptsFolder
        {
            get;
            set;
        }


        /// <summary>
        /// Folder where SQL installation scripts are placed.
        /// </summary>
        public string InstallScriptsFolder
        {
            get;
            set;
        }


        /// <summary>
        /// Installation connection string for separated DB.
        /// </summary>
        public string InstallationConnStringSeparate
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string for separated DB.
        /// </summary>
        public static string ConnStringSeparate
        {
            get
            {
                return ConnectionHelper.GetConnectionString(ConnStringSeparateName, true);
            }
        }


        /// <summary>
        /// Connection string name for separated DB.
        /// </summary>
        public static string ConnStringSeparateName
        {
            get
            {
                return ConnectionHelper.GetConnectionStringNameWithPrefix(OM_CONNECTION_STRING);
            }
        }


        /// <summary>
        /// Method for logging.
        /// </summary>
        public SqlInstallationHelper.LogProgress LogMessage
        {
            get
            {
                return mLogMessage;
            }
            set
            {
                mLogMessage = value;
            }
        }


        /// <summary>
        /// Returns SQL server capabilities.
        /// </summary>
        internal ISqlServerCapabilities SqlServerCapabilities
        {
            get
            {
                return mSqlServerCapabilities ?? (mSqlServerCapabilities = SqlServerCapabilitiesFactory.GetSqlServerCapabilities(ConnectionHelper.GetSqlConnectionString()));
            }
            set
            {
                mSqlServerCapabilities = value;
            }
        }


        /// <summary>
        /// If true, the helper is allowed to copy the database data through the application instance during separation if linked server feature is not available.
        /// </summary>
        public bool AllowCopyDataThroughApplication
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Check that DB separation is successfully finished. Returns false, and sets up connection error message in case it isn't
        /// </summary>
        public static bool CheckDBSeparation()
        {
            // If DB separation was in progress then after application restart it is not in progress
            if (SeparationInProgress)
            {
                SeparationInProgress = false;
            }

            // Check that after DB join all web farm servers have proper connection strings set
            string sepConnString = ConnStringSeparate;
            bool isSepConnString = !String.IsNullOrEmpty(sepConnString);

            string error = null;

            if (isSepConnString)
            {
                // Check separated database
                if (!CheckCMDatabase(OM_CONNECTION_STRING))
                {
                    error = "The separated database was joined back to main database. Please remove connection string CMSOMConnectionString from the web.config.";
                }
            }
            // Check that after DB separation all web farm servers have proper connection string set
            else if (!CheckCMDatabase(null))
            {
                // Check standard database
                error = "The contact management database is separated. Please add connection string CMSOMConnectionString to the web.config.";
            }

            if (error != null)
            {
                CMSApplication.ApplicationErrorMessage = error;

                return false;
            }

            return true;
        }


        /// <summary>
        /// Separates Database.
        /// </summary>
        /// <returns>Returns TRUE if all scripts proceeded without error</returns>
        public void SeparateDatabase()
        {
            isSeparation = true;
            PrepareConnectionStrings();
            MoveDB("[SeparateDatabase]: Error during database separation.");
        }


        /// <summary>
        /// Joins databases.
        /// </summary>
        public void JoinDatabase()
        {
            PrepareConnectionStrings();
            MoveDB("[DatabaseJoin]: Error during database join.");
        }


        /// <summary>
        /// Checks if database is installed with contact management tables.
        /// </summary>
        /// <param name="connectionString">Connection string to database</param>
        /// <returns>Returns true if database contains contact management tables</returns>
        public static bool CheckCMDatabase(string connectionString)
        {
            try
            {
                using (new CMSConnectionScope(connectionString))
                {
                    var tm = new TableManager(null);

                    return tm.TableExists("OM_Contact");
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Modifies old database.
        /// </summary>
        /// <param name="deleteDB">Indicates if complete database should be deleted.</param>
        /// <param name="separation">Indicates if current process is separation</param>
        /// <returns>Returns error if any occurred.</returns>
        public string DeleteSourceTables(bool deleteDB, bool separation)
        {
            isSeparation = separation;
            PrepareConnectionStrings();
            Initialize();

            try
            {
                if (deleteDB)
                {
                    DeleteDB();
                }
                else
                {
                    DeleteTables();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return null;
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Sets connection strings.
        /// </summary>
        private void PrepareConnectionStrings()
        {
            if (isSeparation)
            {
                connectionStringSource = new SqlConnectionStringBuilder(ConnectionHelper.ConnectionString);
                connectionStringTarget = new SqlConnectionStringBuilder(InstallationConnStringSeparate);
            }
            else
            {
                if (!String.IsNullOrEmpty(ConnStringSeparate))
                {
                    connectionStringSource = new SqlConnectionStringBuilder(ConnStringSeparate);
                }
                else if (!String.IsNullOrEmpty(InstallationConnStringSeparate))
                {
                    connectionStringSource = new SqlConnectionStringBuilder(InstallationConnStringSeparate);
                }
                connectionStringTarget = new SqlConnectionStringBuilder(ConnectionHelper.ConnectionString);
            }
        }


        /// <summary>
        /// Runs  DB separation process.
        /// </summary>
        private void MoveDB(string exceptionText)
        {
            SeparationInProgress = true;
            SeparationStartedByServer = SystemContext.MachineName;

            try
            {
                Initialize();
                TargetDBModification();
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message, MessageTypeEnum.Error, true);

                throw new Exception(exceptionText, ex);
            }
            finally
            {
                SeparationInProgress = false;
            }
        }


        /// <summary>
        /// Initializes classes used in separation or join.
        /// </summary>
        private void Initialize()
        {
            separatedTables = new SeparatedTables(ScriptsFolder);
        }

        #endregion


        #region "Target DB methods"

        /// <summary>
        /// Creates data on new database.
        /// </summary>
        private void TargetDBModification()
        {
            var tableSchemas = separatedTables.GetTablesAndSchemas();

            using (var scope = new CMSConnectionScope(connectionStringTarget.ConnectionString, false))
            {
                scope.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                LinkedServer();
                ModifyContent(tableSchemas);
            }
        }


        /// <summary>
        /// Modify content.
        /// </summary>
        private void ModifyContent(List<TableAndClass> tableSchemas)
        {
            var temporaryTables = new List<string>();
            LogMessage(CoreServices.Localization.GetString("separationDB.tablescopy"), MessageTypeEnum.Info, false);

            if (isSeparation)
            {
                temporaryTables = TemporaryTables.CreateTemporaryTables(ScriptsFolder);
            }

            CreateSeparatedTables(tableSchemas);

            if (isSeparation)
            {
                TemporaryTables.RemoveTemporaryTables(temporaryTables);
            }
            else
            {
                TemporaryTables.CreateAdditionalConstraints();
            }

            CreateProceduresViews();
            CopyData(tableSchemas);
        }


        /// <summary>
        /// Create procedures functions and views
        /// </summary>
        private void CreateProceduresViews()
        {
            LogMessage(CoreServices.Localization.GetString("separationDB.proceduresviews"), MessageTypeEnum.Info, false);
            ProcsFuncViewsTypes pfv = new ProcsFuncViewsTypes();
            pfv.InstallScriptsFolder = InstallScriptsFolder;
            pfv.ScriptsFolder = ScriptsFolder;
            pfv.LogMessage = LogMessage;
            pfv.CreateProceduresViews();
        }


        /// <summary>
        /// Creates all required tables.
        /// </summary>
        private void CreateSeparatedTables(IEnumerable<TableAndClass> tableList)
        {
            var tm = new TableManager(null);

            // True is needed here because user could have changed default DB schema
            tm.UpdateSystemFields = true;

            // Process all tables
            IDataConnection conn = ConnectionHelper.GetConnection();
            string currentSchema = SqlInstallationHelper.GetCurrentDefaultSchema(conn);
            foreach (var tableSchema in tableList)
            {
                DataSet result = ConnectionHelper.ExecuteQuery("SELECT OBJECT_ID('" + tableSchema.TableName + "','U') ", null, QueryTypeEnum.SQLQuery);
                if ((result == null) || DataHelper.DataSourceIsEmpty(result) || (result.Tables[0].Rows[0][0] == DBNull.Value))
                {
                    string scriptFileName = Path.Combine(InstallScriptsFolder, tableSchema.TableName + SqlInstallationHelper.SQL_EXTENSION);
                    SqlInstallationHelper.RunSQLScript(scriptFileName, conn, currentSchema);
                    var classInfo = tableSchema.ClassInfo;
                    if (classInfo != null)
                    {
                        // Update the table by custom schema
                        // Data class info from source database is provided together with form definition to reflect custom table fields
                        // Changes are compared to the plain information from target database table
                        var parameters = new UpdateTableParameters
                        {
                            ClassInfo = classInfo,
                            UseOriginalDefinition = false
                        };

                        tm.UpdateTableByDefinition(parameters);
                    }
                }
            }
        }


        /// <summary>
        /// Copies content of all tables.
        /// </summary>
        private void CopyData(IEnumerable<TableAndClass> tableSchemas)
        {
            if (SqlServerCapabilities.SupportsOpenQueryCommand)
            {
                foreach (var tableSchema in tableSchemas)
                {
                    LinkedCopyTableData(tableSchema.TableName);
                }
            }
            else if (AllowCopyDataThroughApplication)
            {
                foreach (var tableSchema in tableSchemas)
                {
                    BulkCopyTableData(tableSchema.TableName);
                }
            }
        }


        /// <summary>
        /// Prepares and test linked server connection.
        /// </summary>
        private void LinkedServer()
        {
            if (SqlServerCapabilities.SupportsLinkedServer)
            {
                PrepareLinkedServer();
                TestLinkedServer();
            }
        }


        /// <summary>
        /// Get linked server SQL script.
        /// </summary>
        private void PrepareLinkedServer()
        {
            string script = File.ReadAllText(Path.Combine(ScriptsFolder, "linked_server.txt"));

            // Replace all values
            script = script.Replace("##BASESERVER##", connectionStringSource.DataSource);
            script = script.Replace("##BASEDATABASENAME##", connectionStringSource.InitialCatalog);
            script = script.Replace("##SERVER##", connectionStringTarget.DataSource);
            script = script.Replace("##DATABASENAME##", connectionStringTarget.InitialCatalog);
            script = script.Replace("##BASEUSERNAME##", connectionStringSource.UserID);
            script = script.Replace("##BASEUSERPASS##", connectionStringSource.Password);

            ConnectionHelper.ExecuteQuery(script, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Test linked servers.
        /// </summary>
        private void TestLinkedServer()
        {
            try
            {
                ConnectionHelper.ExecuteQuery("EXEC sp_testlinkedserver @servername  = [" + connectionStringSource.DataSource + "]", null, QueryTypeEnum.SQLQuery);
            }
            catch
            {
                throw new Exception(CoreServices.Localization.GetAPIString("separationDB.errorLinkedServer", null, "Creating link between SQL Servers failed. Please make sure that SQL Server enables remote connections, that specified login has appropriate permissions and that SQL connections are not blocked."));
            }
        }


        /// <summary>
        /// Copy content of single table to different DB via bulk insert.
        /// </summary>
        private void BulkCopyTableData(string tableName)
        {
            var data = 
                new DataQuery()
                {
                    ConnectionStringName = ConnectionHelper.ConnectionStringName
                }
                .From(tableName)
                .Result;

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                using (var cs = new CMSConnectionScope(connectionStringTarget.ConnectionString, false))
                {
                    ConnectionHelper.BulkInsert(
                        data.Tables[0], 
                        tableName, 
                        new BulkInsertSettings { KeepIdentity = true, BulkCopyTimeout = 180 }
                    );

                    SqlInstallationHelper.CheckAllConstraints(cs.Connection, tableName);
                }
            }
        }


        /// <summary>
        /// Copy content of single table to different DB.
        /// </summary>
        private void LinkedCopyTableData(string tableName)
        {
            string tableColumns = GetTableColumns(tableName);
            string queryText = String.Empty;
            bool hasIdentity = HasIdentityColumn(tableName);
            if (hasIdentity)
            {
                queryText += "SET IDENTITY_INSERT [" + tableName + @"] ON";
            }

            queryText += @"
INSERT INTO [" + tableName + @"] (" + tableColumns + @")
SELECT " + tableColumns + @" 
FROM OPENQUERY([" + connectionStringSource.DataSource + "], 'SELECT " + tableColumns + @" FROM [" + connectionStringSource.InitialCatalog + "]..[" + tableName + @"]')";

            if (hasIdentity)
            {
                queryText += "SET IDENTITY_INSERT  [" + tableName + @"] OFF";
            }

            LogMessage(CoreServices.Localization.GetString("separationDB.contentcopy") + tableName, MessageTypeEnum.Info, false);
            ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Indicates if table has identity column defined
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Returns TRUE if table has identity column</returns>
        private bool HasIdentityColumn(string tableName)
        {
            string queryText = String.Format(@"SELECT COLUMN_NAME, TABLE_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_Name = '{0}'
AND COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1
ORDER BY TABLE_NAME", tableName);
            DataSet result = ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
            return !DataHelper.DataSourceIsEmpty(result);
        }


        /// <summary>
        /// Get table columns.
        /// </summary>
        private string GetTableColumns(string tableName)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'", null, QueryTypeEnum.SQLQuery);
            string columns = String.Empty;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    columns += row[0] + ",";
                }
            }
            return columns.Substring(0, columns.Length - 1);
        }

        #endregion


        #region "Source DB methods"

        /// <summary>
        /// Delete complete database.
        /// </summary>
        private void DeleteDB()
        {
            string dbName = connectionStringSource.InitialCatalog;
            connectionStringSource.InitialCatalog = String.Empty;

            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionStringSource.ConnectionString);
            
            using (var scope = new CMSConnectionScope(conn).Open())
            {
                scope.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                DropDB(dbName);
            }
        }


        /// <summary>
        /// Delete tables.
        /// </summary>
        private void DeleteTables()
        {
            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionStringSource.ConnectionString);
            
            using (var tr = new CMSTransactionScope(conn))
            {
                DeleteOldTables();
                DropProceduresFunctionsViews();
                tr.Commit();
            }
        }


        /// <summary>
        /// Drops all old tables.
        /// </summary>
        private void DeleteOldTables()
        {
            foreach (var table in separatedTables.GetTableNames(true))
            {
                string queryText = @"IF OBJECT_ID('" + table.TableName + @"','U') IS NOT NULL
                                   DROP  TABLE  [" + table.TableName + "]";

                ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
            }
        }


        /// <summary>
        /// Drops old procedures functions and views.
        /// </summary>
        private void DropProceduresFunctionsViews()
        {
            ProcsFuncViewsTypes pfv = new ProcsFuncViewsTypes();
            pfv.ScriptsFolder = ScriptsFolder;
            pfv.IsSeparation = isSeparation;
            pfv.DropProceduresFunctionsViews();
        }


        /// <summary>
        /// Drops separated DB.
        /// </summary>
        private void DropDB(string dbName)
        {
            ConnectionHelper.ExecuteQuery(String.Format(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dbName), null, QueryTypeEnum.SQLQuery);
            ConnectionHelper.ExecuteQuery(String.Format(@"DROP DATABASE [{0}]", dbName), null, QueryTypeEnum.SQLQuery);
        }

        #endregion
    }
}

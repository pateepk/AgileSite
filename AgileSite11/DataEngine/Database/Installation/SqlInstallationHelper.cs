using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Text;
using System.Xml.Serialization;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

using SystemIO = System.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Sets of methods for creating database during installation process.
    /// </summary>
    public static class SqlInstallationHelper
    {
        #region "Constants"

        internal const string SQL_EXTENSION = ".sql";

        private const string XML_EXTENSION = ".xml";

        /// <summary>
        /// Settings keys table name.
        /// </summary>
        private const string SETTINGS_KEY_TABLE_NAME = "CMS_SettingsKey";

        /// <summary>
        /// Default SQL connection timeout.
        /// </summary>
        public const int DB_CONNECTION_TIMEOUT = 60;

        /// <summary>
        /// DB Owner schema
        /// </summary>
        public const string DBO_SCHEMA = "dbo";

        #endregion


        #region "Variables"

        private static Regex mDBORegEx;

        /// <summary>
        /// Executes when SQL script is run by the installer
        /// </summary>
        public static QueryHandler RunQuery = new QueryHandler();

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for removing dbo. and [dbo]. from install queries.
        /// </summary>
        private static Regex DBORegEx
        {
            get
            {
                return mDBORegEx ?? (mDBORegEx = RegexHelper.GetRegex("(\\s|\\()(\\[dbo\\]|dbo)\\.", true));
            }
        }

        #endregion


        #region "Delegates and events"

        /// <summary>
        /// Delegate of event fired when message logging is required.
        /// </summary>
        /// <param name="message">Text of the message</param>
        /// <param name="type">Type of the message</param>
        /// <param name="logToEventLog">Log to event log</param>
        public delegate void LogProgress(string message, MessageTypeEnum type, bool logToEventLog);


        /// <summary>
        /// Delegate of event fired when message logging is required.
        /// </summary>
        /// <param name="message">Text of the message</param>
        /// <param name="type">Type of the message</param>
        public delegate void LogMessage(string message, MessageTypeEnum type);


        /// <summary>
        /// Fired after data getting is finished.
        /// </summary>
        public static event EventHandler<DataSetPostProcessingEventArgs> AfterDataGet;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Test connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static Exception TestConnection(string connectionString)
        {
            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionString);
            try
            {
                conn.Open();
                conn.Close();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }


        /// <summary>
        /// Returns connection string for the given parameters.
        /// </summary>
        /// <param name="winAuth">Whether to use windows authentication</param>
        /// <param name="serverName">Server</param>
        /// <param name="database">Name of the database</param>
        /// <param name="userName">SQL user name</param>
        /// <param name="pass">SQL password</param>
        public static string GetConnectionString(bool winAuth, string serverName, string database, string userName, string pass)
        {
            if (String.IsNullOrEmpty(serverName))
            {
                return null;
            }

            SQLServerAuthenticationModeEnum authEnum = SQLServerAuthenticationModeEnum.SQLServerAuthentication;
            if (winAuth)
            {
                authEnum = SQLServerAuthenticationModeEnum.WindowsAuthentication;
            }

            if ((authEnum == SQLServerAuthenticationModeEnum.SQLServerAuthentication) &&
                (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(pass)))
            {
                return null;
            }

            return ConnectionHelper.BuildConnectionString(authEnum, serverName, database, userName, pass, 240);
        }


        /// <summary>
        /// Check if DB schema exists.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="dbSchema">Schema</param>
        public static bool CheckIfSchemaExist(string connectionString, string dbSchema)
        {
            if (!String.IsNullOrEmpty(dbSchema))
            {
                try
                {
                    // Try to find DB schema in system schemas
                    IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionString);
                    DataSet ds = conn.ExecuteQuery("SELECT * FROM sys.schemas WHERE name = N'" + SqlHelper.GetSafeQueryString(dbSchema, false) + "'", null, QueryTypeEnum.SQLQuery, false);
                    if (DataHelper.DataSourceIsEmpty(ds))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns current default schema
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static string GetCurrentDefaultSchema(string connectionString)
        {
            // Try to find DB schema in system schemas
            var conn = DataConnectionFactory.GetNativeConnection(connectionString);

            return GetCurrentDefaultSchema(conn);
        }


        /// <summary>
        /// Returns current default schema
        /// </summary>
        /// <param name="conn">Connection</param>
        public static string GetCurrentDefaultSchema(IDataConnection conn)
        {
            try
            {
                return ValidationHelper.GetString(conn.ExecuteScalar("SELECT SCHEMA_NAME()", null, QueryTypeEnum.SQLQuery, false), null);
            }
            catch
            {
                // Suppress error and return no schema
            }

            return null;
        }


        /// <summary>
        /// Creates database.
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="connectionString">Connection string</param>
        /// <param name="databaseCollation">Collation of database</param>
        public static void CreateDatabase(string databaseName, string connectionString, string databaseCollation)
        {
            // Use default collation if not specified
            if (String.IsNullOrEmpty(databaseCollation))
            {
                databaseCollation = DatabaseHelper.DatabaseCollation;
            }

            // Create the query
            string query = "CREATE DATABASE [" + databaseName + "] COLLATE " + databaseCollation;

            // Create the database
            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionString);
            conn.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, false);
        }


        /// <summary>
        /// Gets the folder in which the SQL install scripts are located.
        /// </summary>
        public static string GetSQLInstallPath()
        {
            return GetSQLInstallPath(null);
        }


        /// <summary>
        /// Gets the folder in which the SQL install scripts for DB objects are located.
        /// </summary>
        public static string GetSQLInstallPathToObjects()
        {
            return GetSQLInstallPath("Objects");
        }


        /// <summary>
        /// Returns database version from settings key.
        /// </summary>
        /// <param name="connString">Connection string</param>
        public static string GetDatabaseVersion(string connString)
        {
            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connString);
            return ValidationHelper.GetString(conn.ExecuteScalar("SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName='CMSDBVersion'", null, QueryTypeEnum.SQLQuery, false), null);
        }


        /// <summary>
        /// Returns true if database exists.
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <param name="connectionString">Connection string to DB server</param>
        public static bool DatabaseExists(string databaseName, string connectionString)
        {
            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionString);

            string query = "SELECT * FROM sys.databases WHERE Name = '" + SqlHelper.GetSafeQueryString(databaseName, false) + "'";
            DataSet ds = conn.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, false);

            return (ds != null) && (ds.Tables.Count >= 1) && (ds.Tables[0].Rows.Count >= 1);
        }


        /// <summary>
        /// Returns database engine edition of the instance of SQL Server installed on the server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static SQLEngineEditionEnum GetEngineEdition(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                return SQLEngineEditionEnum.Unknown;
            }

            IDataConnection conn = DataConnectionFactory.GetNativeConnection(connectionString);

            int engine = ValidationHelper.GetInteger(conn.ExecuteScalar("SELECT SERVERPROPERTY('EngineEdition')", null, QueryTypeEnum.SQLQuery, false), 0);
            return (SQLEngineEditionEnum)engine;
        }


        /// <summary>
        /// Changes the database schema to the given schema
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="newSchema">New schema</param>
        /// <param name="log">Log</param>
        public static bool ChangeDefaultSchema(IDataConnection conn, string newSchema, LogMessage log)
        {
            try
            {
                conn.ExecuteNonQuery(
@"
CREATE TABLE [dbo].[Schema_Temp] (
	[ID] [int] IDENTITY(1, 1) NOT NULL,
)

DECLARE @SQL_SCRIPT varchar(450)
SET @SQL_SCRIPT = 'ALTER USER [' + USER_NAME() + '] WITH DEFAULT_SCHEMA = [" + newSchema + @"]'

EXECUTE (@SQL_SCRIPT)

DROP TABLE [dbo].[Schema_Temp]
",
                    null,
                    QueryTypeEnum.SQLQuery,
                    true
                );

                return true;
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    HandleError("Failed to change DB schema", ex, log);
                }
                else
                {
                    // Re-throw the exception if there is no function to report error
                    throw;
                }

                return false;
            }
        }


        /// <summary>
        /// Create a copy of database. This feature is available only in SQL Azure databases.
        /// </summary>
        /// <param name="connectionString">Connection string to master database.</param>
        /// <param name="databaseName">Name of database to copy.</param>
        /// <param name="copyName">Name of new database.</param>
        public static void CopyDatabase(string connectionString, string databaseName, string copyName)
        {
            using (IDataConnection sqlConn = DataConnectionFactory.GetNativeConnection(connectionString))
            {
                sqlConn.ExecuteQuery("CREATE DATABASE [" + SqlHelper.GetSafeQueryString(copyName, false) + "] AS COPY OF [" + SqlHelper.GetSafeQueryString(databaseName, false) + "]", new QueryDataParameters(), QueryTypeEnum.SQLQuery, false);
            }
        }


        /// <summary>
        /// Checks if given database is being copied.
        /// </summary>
        /// <param name="connectionString">Connection to master database.</param>
        /// <param name="databaseName">Name of database to check.</param>
        public static bool CopyingCompleted(string connectionString, string databaseName)
        {
            using (IDataConnection sqlConn = DataConnectionFactory.GetNativeConnection(connectionString))
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@Name", databaseName, typeof(string));

                return (int)sqlConn.ExecuteScalar("SELECT COUNT(*) FROM sys.dm_database_copies WHERE partner_database = @Name", parameters, QueryTypeEnum.SQLQuery, false) == 0;
            }
        }


        /// <summary>
        /// Retrieves SQL server default backup directory from it's registry.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Database default backup directory</returns>
        public static string GetDefaultBackupPath(string connectionString)
        {
            using (IDataConnection sqlConn = DataConnectionFactory.GetNativeConnection(connectionString))
            {
                DbDataReader reader = sqlConn.ExecuteReader(@"EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer',N'BackupDirectory'", new QueryDataParameters(), QueryTypeEnum.SQLQuery, CommandBehavior.SingleResult);

                if (reader.Read())
                {
                    return reader["Data"] as string;
                }

                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
        }


        /// <summary>
        /// Backup given database to filesystem.
        /// </summary>
        /// <param name="connectionString">Connection string of database to backup</param>
        /// <param name="backupPath">Path to filesystem, where the database will be backuped on the database server.</param>
        public static void BackupDatabase(string connectionString, string backupPath)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(builder.InitialCatalog))
            {
                using (IDataConnection sqlConn = DataConnectionFactory.GetNativeConnection(connectionString))
                {
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@Name", builder.InitialCatalog);
                    parameters.Add("@Path", backupPath);

                    sqlConn.ExecuteNonQuery("BACKUP DATABASE @Name TO DISK = @Path", parameters, QueryTypeEnum.SQLQuery, false);
                }
            }
        }


        /// <summary>
        /// Deletes database.
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <param name="connectionString">Connection string to DB server</param>
        public static string DeleteDatabase(string databaseName, string connectionString)
        {
            try
            {
                databaseName = SqlHelper.GetSafeQueryString(databaseName, false);

                var conn = DataConnectionFactory.GetNativeConnection(connectionString);

                // Switching database to single user mode is not supported in Azure
                if (GetEngineEdition(connectionString) != SQLEngineEditionEnum.SQLAzure)
                {
                    conn.ExecuteNonQuery("ALTER DATABASE [" + databaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", null, QueryTypeEnum.SQLQuery, false);
                }
                conn.ExecuteNonQuery("DROP DATABASE [" + databaseName + "]", null, QueryTypeEnum.SQLQuery, false);

                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Returns unique database name.
        /// </summary>
        public static string GetUniqueDatabaseName(string dbName, string connString)
        {
            // Ensure uniqueness
            while (DatabaseExists(dbName, connString))
            {
                dbName = IncrementCounter(dbName);
            }
            return dbName;
        }


        /// <summary>
        /// Performs the database installation.
        /// </summary>
        /// <param name="connectionString">Connection string to the database</param>
        /// <param name="scriptsFolder">Folder with the database scripts</param>
        /// <param name="dbObjectsErrMessage">Error message when creation of DB objects fails</param>
        /// <param name="defaultDataErrMessage">Error message when creation data fails</param>
        /// <param name="log">Method for logging messages</param>
        public static bool InstallDatabase(string connectionString, string scriptsFolder, string dbObjectsErrMessage, string defaultDataErrMessage, LogMessage log)
        {
            bool success = true;

            var conn = DataConnectionFactory.GetNativeConnection(connectionString);

            var dbSchema = EnsureDefaultSchema(log, conn, ref success);

            // Create database objects 
            if (success)
            {
                success = ProceedSQLScripts("defaultDBObjects.txt", conn, scriptsFolder, log, dbSchema, dbObjectsErrMessage);
            }

            if (success)
            {
                success = ImportDefaultData(conn, scriptsFolder, log, defaultDataErrMessage);
            }

            // Insert default data with SQL scripts
            if (success)
            {
                success = ProceedSQLScripts("defaultData.txt", conn, scriptsFolder, log, dbSchema, defaultDataErrMessage);
            }

            // Apply hotfix scripts
            if (success)
            {
                success = ApplyHotfix(conn, scriptsFolder, dbSchema, log, defaultDataErrMessage);
            }

            // Dispose the zip storage provider to release memory and the file
            if (ZipStorageProvider.IsZipFolderPath(scriptsFolder))
            {
                ZipStorageProvider.Dispose(scriptsFolder);
            }

            // Set database version
            conn.ExecuteNonQuery("UPDATE [CMS_SettingsKey] SET KeyDefaultValue='" + CMSVersion.MainVersion + "', KeyValue='" + CMSVersion.MainVersion + "' WHERE KeyName='CMSDBVersion'", null, QueryTypeEnum.SQLQuery, false);

            return success;
        }


        /// <summary>
        /// Ensures the default schema for current user
        /// </summary>
        /// <param name="conn">Connection</param>
        public static void EnsureDefaultSchema(IDataConnection conn)
        {
            bool success = true;

            EnsureDefaultSchema(null, conn, ref success);
        }


        /// <summary>
        /// Performs (re)check operation of all constraints for specified table.
        /// If operation finish successfully untrusted constraints will be trusted.
        /// </summary>
        /// <param name="connection">Connection to database</param>
        /// <param name="tableName">Name of the table to check</param>
        public static void CheckAllConstraints(IDataConnection connection, string tableName)
        {
            var query = $"ALTER TABLE [{SqlHelper.EscapeQuotes(tableName)}] WITH CHECK CHECK CONSTRAINT ALL";
            connection.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, false);
        }


        /// <summary>
        /// Ensures the database schema for the current user. If the user does not have the schema set, sets the schema to dbo.
        /// </summary>
        /// <param name="log">Log message</param>
        /// <param name="conn">Data connection</param>
        /// <param name="success">Returns true, if changing the schema was successful</param>
        private static string EnsureDefaultSchema(LogMessage log, IDataConnection conn, ref bool success)
        {
            string dbSchema = GetCurrentDefaultSchema(conn);

            // Ensure the [dbo] schema if schema not found
            if (dbSchema == null)
            {
                success = ChangeDefaultSchema(conn, DBO_SCHEMA, log);

                dbSchema = DBO_SCHEMA;
            }

            return dbSchema;
        }


        /// <summary>
        /// Checks if database is separated or not.
        /// </summary>
        public static bool DatabaseIsSeparated()
        {
            return !String.IsNullOrEmpty(DatabaseSeparationHelper.ConnStringSeparate);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets the folder in which the SQL install scripts are located.
        /// </summary>
        /// <param name="relativePath">Additional path to folder or file</param>
        private static string GetSQLInstallPath(string relativePath)
        {
            // Prepare the scripts path
            string scriptsPath = "~/App_Data/Install/";
            if (StorageHelper.UseZippedResources && File.ExistsRelative(scriptsPath + "SQL.zip"))
            {
                scriptsPath = scriptsPath + ZipStorageProvider.GetZipFileName("SQL.zip");
            }
            else
            {
                scriptsPath += "SQL";
            }

            if (!String.IsNullOrEmpty(relativePath))
            {
                scriptsPath += "/" + Path.EnsureSlashes(relativePath, true);
            }

            return StorageHelper.GetFullFilePhysicalPath(scriptsPath);
        }


        private static void HandleError(string errorPrefix, string errorMessage, LogMessage log)
        {
            log?.Invoke($"{errorPrefix} {errorMessage}", MessageTypeEnum.Error);
        }


        private static void HandleError(string errorPrefix, Exception errorException, LogMessage log)
        {
            if (errorException != null)
            {
                log?.Invoke($"{errorPrefix} {errorException.Message}{Environment.NewLine}{errorException.StackTrace}", MessageTypeEnum.Error);
            }
        }


        private static void ReportProgress(string filePath, string fileExtension, LogMessage log)
        {
            if ((log != null) && !String.IsNullOrEmpty(filePath))
            {
                string fileWithoutExt = filePath.ToLowerInvariant();
                if (!String.IsNullOrEmpty(fileExtension) && fileWithoutExt.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    fileWithoutExt = fileWithoutExt.Remove(fileWithoutExt.Length - fileExtension.Length);
                }

                log(fileWithoutExt, MessageTypeEnum.Info);
            }
        }


        /// <summary>
        /// Executes SQL query.
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="conn">Connection</param>
        /// <param name="dbSchema">Schema</param>
        private static void RunSQLQuery(string query, IDataConnection conn, string dbSchema)
        {
            // Start run query handler
            using (var h = RunQuery.StartEvent(query))
            {
                if (h.CanContinue())
                {
                    query = h.EventArguments.Query;

                    // Change the schema
                    if (!String.IsNullOrEmpty(dbSchema))
                    {
                        query = DBORegEx.Replace(query, "$1[" + dbSchema + "].");
                    }

                    // Execute query
                    conn.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, false);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Executes SQL script. Method internally handles 'GO' commands.
        /// </summary>
        /// <param name="fileName">SQL script</param>
        /// <param name="conn">Connection</param>
        /// <param name="dbSchema">DB schema</param>
        internal static Exception RunSQLScript(string fileName, IDataConnection conn, string dbSchema)
        {
            if (!File.Exists(fileName))
            {
                return new SystemIO.FileNotFoundException($"SQL script file '{fileName}' does not exist.");
            }

            try
            {
                StringBuilder sb = new StringBuilder(65536);
                using (StreamReader stream = File.OpenText(fileName))
                {
                    while (!stream.EndOfStream)
                    {
                        string line = stream.ReadLine();

                        if ((line != null) && line.Trim().Equals("go", StringComparison.OrdinalIgnoreCase))
                        {
                            RunSQLQuery(sb.ToString(), conn, dbSchema);
                            sb.Length = 0;
                            sb.Capacity = 65536;
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
                if (sb.Length != 0)
                {
                    RunSQLQuery(sb.ToString(), conn, dbSchema);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }


        /// <summary>
        /// Runs the SQL scripts listed in the given file.
        /// </summary>
        /// <param name="fileName">FileName with the scripts to run</param>
        /// <param name="conn">SQL connection</param>
        /// <param name="scriptsFolder">Folder with the SQL scripts</param>
        /// <param name="log">Method for logging messages</param>
        /// <param name="dbSchema">Name of the schema (all "dbo." and "[dbo]." occurrences are replaced by this schema)</param>
        /// <param name="defaultErrorMessage">Default error message</param>
        private static bool ProceedSQLScripts(string fileName, IDataConnection conn, string scriptsFolder, LogMessage log, string dbSchema, string defaultErrorMessage)
        {
            List<string> scriptsList = null;
            try
            {
                // Load script names from file or folder
                if (!String.IsNullOrEmpty(fileName))
                {
                    scriptsList = LoadScriptsFromFile(DirectoryHelper.CombinePath(scriptsFolder, fileName));
                }
            }
            catch (Exception ex)
            {
                HandleError(defaultErrorMessage, ex, log);
                return false;
            }

            // Execute SQL scripts
            if (scriptsList != null)
            {
                foreach (string scriptName in scriptsList)
                {
                    ReportProgress(scriptName, SQL_EXTENSION, log);

                    // Execute script
                    Exception error = RunSQLScript(DirectoryHelper.CombinePath(scriptsFolder, Path.EnsureBackslashes(scriptName)), conn, dbSchema);
                    if (error != null)
                    {
                        HandleError(defaultErrorMessage, error, log);

                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Increments number (separated by "_") at the end of string.
        /// </summary>
        /// <param name="str">String</param>
        private static string IncrementCounter(string str)
        {
            if (str == null)
            {
                str = String.Empty;
            }

            int counter = 1;
            int index = str.LastIndexOf("_", StringComparison.Ordinal);
            if (index >= 0)
            {
                string stringCounter = str.Substring(index + 1);
                if (Int32.TryParse(stringCounter, out counter))
                {
                    counter++;
                    str = str.Remove(index);
                }
                else
                {
                    counter = 1;
                }
            }

            return str + "_" + counter;
        }



        /// <summary>
        /// Returns script names (files) from text file.
        /// </summary>
        /// <param name="path">Path to file</param>
        private static List<string> LoadScriptsFromFile(string path)
        {
            List<string> result = new List<string>();

            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            line = line.Trim();
                            if (!String.IsNullOrEmpty(line) && !line.StartsWith("//", StringComparison.Ordinal))
                            {
                                result.Add(line);
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Get list of tables ordered by dependency from least dependent to most dependent.
        /// </summary>
        /// <param name="connection">Connection to database.</param>
        private static IEnumerable<string> GetOrderedTables(IDataConnection connection)
        {
            string script =
@"
WITH ForeignKeys
AS (
	SELECT DISTINCT OnTable = OnTable.Name
		,AgainstTable = AgainstTable.Name
	FROM SYS.FOREIGN_KEY_COLUMNS
	INNER JOIN sysobjects OnTable ON SYS.FOREIGN_KEY_COLUMNS.parent_object_id = OnTable.id
	INNER JOIN sysobjects AgainstTable ON SYS.FOREIGN_KEY_COLUMNS.referenced_object_id = AgainstTable.id
	WHERE AgainstTable.TYPE = 'U'
		AND OnTable.TYPE = 'U'
		-- Avoid recursion
		AND OnTable.Name <> AgainstTable.Name
	)
	,AllTables
AS (
	-- Make sure all tables are included
	SELECT OnTable = sys.objects.NAME
		,AgainstTable = ForeignKeys.AgainstTable
	FROM sys.objects
	LEFT JOIN ForeignKeys ON sys.objects.NAME = ForeignKeys.onTable
	WHERE sys.objects.type = 'U'
		AND sys.objects.NAME NOT LIKE 'sys%'
	)
	,ClassifiedTables
AS (
	-- Starndard dependency
	SELECT TableName = OnTable
		,TableLevel = 1
	FROM AllTables
	WHERE AgainstTable IS NULL
	
	UNION ALL
	
	-- Recursive dependency
	SELECT TableName = OnTable
		,TableLevel = ClassifiedTables.TableLevel + 1
	FROM AllTables
	INNER JOIN ClassifiedTables ON AllTables.AgainstTable = ClassifiedTables.TableName
	)
SELECT TableName
FROM ClassifiedTables
GROUP BY TableName
ORDER BY MAX(TableLevel) ASC
	,TableName ASC
";

            DataSet result = connection.ExecuteQuery(script, null, QueryTypeEnum.SQLQuery, false);

            return result.Tables[0].Rows.Cast<DataRow>().Select(row => row["TableName"].ToString()).ToList();
        }


        /// <summary>
        /// Import default data to database.
        /// </summary>
        /// <param name="connection">Connection to database.</param>
        /// <param name="dataFolder">Folder containing default data serialized in XML.</param>
        /// <param name="log">Method for logging messages</param>
        /// <param name="defaultErrorMessage">Default error message</param>
        private static bool ImportDefaultData(IDataConnection connection, string dataFolder, LogMessage log, string defaultErrorMessage)
        {
            if (connection == null)
            {
                HandleError(defaultErrorMessage, "Connection must not be null", log);
                return false;
            }

            if (String.IsNullOrEmpty(dataFolder) || !Directory.Exists(dataFolder))
            {
                HandleError(defaultErrorMessage, "Folder with default data must exist.", log);
                return false;
            }

            try
            {
                var orderedTables = GetOrderedTables(connection);

                // Insert default data in given order
                foreach (var tableName in orderedTables)
                {
                    string shortFilePath = Path.Combine("Data", tableName + XML_EXTENSION);
                    string filePath = Path.Combine(dataFolder, shortFilePath);

                    if (!File.Exists(filePath))
                    {
                        // Not all database tables have default data
                        continue;
                    }

                    ReportProgress(shortFilePath, XML_EXTENSION, log);

                    DataSet data = new DataSet();

                    // ReadXml does not support zip provider
                    SystemIO.Stream fileStream = File.OpenRead(filePath);
                    try
                    {
                        data.ReadXml(fileStream, XmlReadMode.ReadSchema);
                    }
                    finally
                    {
                        fileStream.Close();
                    }

                    InitSettingsKeyTimestamp(data, tableName);

                    // Post process the data
                    AfterDataGet?.Invoke(null, new DataSetPostProcessingEventArgs(data, tableName));

                    foreach (DataTable table in data.Tables)
                    {
                        var settings = new BulkInsertSettings
                        {
                            Mappings = table.Columns.Cast<DataColumn>().ToDictionary(x => x.ColumnName.ToString(), y => y.ColumnName.ToString()),
                            KeepIdentity = true,
                            BulkCopyTimeout = 180
                        };

                        connection.BulkInsert(table, tableName, settings);

                        CheckAllConstraints(connection, tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(defaultErrorMessage, ex, log);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Initialize settings key timestamp.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="tableName">Table name</param>
        private static void InitSettingsKeyTimestamp(DataSet data, string tableName)
        {
            if ((tableName != null) && tableName.Equals(SETTINGS_KEY_TABLE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                if (DataHelper.DataSourceIsEmpty(data))
                {
                    throw new InvalidOperationException($"{SETTINGS_KEY_TABLE_NAME} dataset must not be empty.");
                }

                // Create required column if does not exist
                DataHelper.EnsureColumn(data.Tables[0], SettingsKeyInfo.TYPEINFO.TimeStampColumn, typeof(DateTime));

                // Update column values
                DataHelper.SetColumnValues(data.Tables[0], SettingsKeyInfo.TYPEINFO.TimeStampColumn, DateTime.Now);
            }
        }


        /// <summary>
        /// Import default data to database.
        /// </summary>
        /// <param name="connection">Connection to database.</param>
        /// <param name="scriptsFolder">Folder containing default data serialized in XML.</param>
        /// <param name="dbSchema">Name of the schema (all "dbo." and "[dbo]." occurrences are replaced by this schema)</param>
        /// <param name="log">Method for logging messages</param>
        /// <param name="defaultErrorMessage">Default error message</param>
        private static bool ApplyHotfix(IDataConnection connection, string scriptsFolder, string dbSchema, LogMessage log, string defaultErrorMessage)
        {
            const string HOTFIX_FOLDER_NAME = "Hotfix";
            string hotfixFolderPath = Path.Combine(scriptsFolder, HOTFIX_FOLDER_NAME);
            const string HOTFIX_FILE_NAME = "Hotfix" + SQL_EXTENSION;
            string hotfixSql = Path.Combine(hotfixFolderPath, HOTFIX_FILE_NAME);
            Exception error = null;

            if (Directory.Exists(hotfixFolderPath))
            {
                // Get scripts to run
                SQLSettings settings = null;
                string pathToHotfixXML = Path.Combine(hotfixFolderPath, "Hotfix.xml");
                if (File.Exists(pathToHotfixXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SQLSettings));
                    try
                    {
                        using (FileStream stream = FileStream.New(pathToHotfixXML, FileMode.Open))
                        {
                            settings = (SQLSettings)serializer.Deserialize(stream);
                        }

                        foreach (SQLScript sqlScript in settings.Scripts)
                        {
                            sqlScript.SQLFilePath = Path.Combine(hotfixFolderPath, sqlScript.SQLFileName);
                        }
                        settings.Scripts = settings.Scripts.OrderBy(s => s.SQLLaunchOrder).ToList();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                }
                else if (File.Exists(hotfixSql))
                {
                    settings = new SQLSettings();
                    settings.Scripts.Add(new SQLScript
                    {
                        SQLFileName = HOTFIX_FILE_NAME,
                        SQLFilePath = hotfixSql
                    });
                }

                if (error == null)
                {
                    if (settings != null)
                    {
                        // Run scripts one by one
                        foreach (SQLScript sqlScript in settings.Scripts)
                        {
                            log(Path.Combine(HOTFIX_FOLDER_NAME, sqlScript.SQLFileName), MessageTypeEnum.Info);
                            error = RunSQLScript(sqlScript.SQLFilePath, connection, dbSchema);
                            if (error != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (error != null)
                {
                    HandleError(defaultErrorMessage, error, log);
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
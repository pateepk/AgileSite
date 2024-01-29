using System;
using System.Data;

using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Separation class for manipulation with procedures, types, functions and views.
    /// </summary>
    internal class ProcsFuncViewsTypes
    {
        #region "Properties"

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
        /// Indicates if current process is separation of databases. If FALSE then current process is joining of databases.
        /// </summary>
        public bool IsSeparation
        {
            get;
            set;
        }


        /// <summary>
        /// Method for logging.
        /// </summary>
        public SqlInstallationHelper.LogProgress LogMessage
        {
            get;
            set;
        }

        #endregion


        #region "Methods for creating"

        /// <summary>
        /// Creates all required procedures functions and views.
        /// </summary>
        public void CreateProceduresViews()
        {
            IDataConnection conn = ConnectionHelper.GetConnection();
            string currentSchema = SqlInstallationHelper.GetCurrentDefaultSchema(conn);

            foreach (var fileName in new[]
            {
                "copy_types.txt",
                "procedures_functions_views.txt",
                "copy_functions.txt"
            })
            {
                using (StreamReader stream = File.OpenText(Path.Combine(ScriptsFolder, fileName)))
                {
                    while (!stream.EndOfStream)
                    {
                        string dbObjectName = stream.ReadLine()?.Trim();
                        string scriptFileName = Path.Combine(InstallScriptsFolder, dbObjectName + SqlInstallationHelper.SQL_EXTENSION);
                        try
                        {
                            if (!CheckIfItemExistsInDB(dbObjectName))
                            {
                                SqlInstallationHelper.RunSQLScript(scriptFileName, conn, currentSchema);
                            }
                        }
                        catch(Exception ex)
                        {
                            var connectionStringSource = new System.Data.SqlClient.SqlConnectionStringBuilder(ConnectionHelper.ConnectionString);
                            using (new CMSConnectionScope(connectionStringSource.ConnectionString, false))
                            {
                                string message = "Could not create '" + dbObjectName + "' on the database from '" + scriptFileName + "'." + Environment.NewLine + " Original message:" + ex.Message + Environment.NewLine + " Stack trace: " + ex.StackTrace;
                                LogMessage(message, MessageTypeEnum.Warning, true);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Checks existence of specified View, Procedure, Function or Type in DB.
        /// </summary>
        private bool CheckIfItemExistsInDB(string item)
        {
            string query = null;
            if (item.StartsWith("Func", StringComparison.Ordinal))
            {
                query = "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@objectName) AND type IN ('FN', 'IF', 'TF')";
            }
            else if (item.StartsWith("Proc", StringComparison.Ordinal))
            {
                query = "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@objectName) AND type IN ('P', 'PC')";
            }
            else if (item.StartsWith("View", StringComparison.Ordinal))
            {
                query = "SELECT OBJECT_ID(@objectName, 'V')";
            }
            else if (item.StartsWith("Type", StringComparison.Ordinal))
            {
                query = "SELECT TYPE_ID(@objectName)";
            }

            DataSet result = null;
            if (query != null)
            {
                var parameters = new QueryDataParameters();
                parameters.Add("@objectName", item, typeof(string));

                result = ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
            }

            return !DataHelper.DataSourceIsEmpty(result) && (result.Tables[0].Rows[0][0] != DBNull.Value);
        }


        #endregion


        #region "Methods for deleting"

        /// <summary>
        /// Drops functions, procedures and views.
        /// </summary>
        public void DropProceduresFunctionsViews()
        {
            foreach (var item in FileReader.ReadAndSplit(ScriptsFolder, "procedures_functions_views.txt"))
            {
                DropItem(item);
            }

            if (!IsSeparation)
            {
                foreach (var item in FileReader.ReadAndSplit(ScriptsFolder, "copy_functions.txt"))
                {
                    DropItem(item);
                }
            }
        }


        /// <summary>
        /// Deletes one procedure or function or view on old DB.
        /// </summary>
        private void DropItem(string item)
        {
            string query = null;
            if (item.StartsWith("Func", StringComparison.Ordinal))
            {
                query = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@objectName) AND type IN ('FN', 'IF', 'TF')) DROP FUNCTION {0}";
            }
            else if (item.StartsWith("Proc", StringComparison.Ordinal))
            {
                query = @"IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@objectName) AND type IN ('P', 'PC')) DROP PROCEDURE {0}";
            }
            else if (item.StartsWith("View", StringComparison.Ordinal))
            {
                query = @"IF OBJECT_ID(@objectName, 'V') IS NOT NULL DROP VIEW {0}";
            }

            if (query != null)
            {
                var parameters = new QueryDataParameters();
                parameters.Add("@objectName", item, typeof(string));

                ConnectionHelper.ExecuteQuery(string.Format(query, item), parameters, QueryTypeEnum.SQLQuery);
            }
        }

        #endregion
    }
}

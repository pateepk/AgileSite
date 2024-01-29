using System;
using System.Data;
using System.Collections.Generic;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides database operations
    /// </summary>
    public static class DatabaseHelper
    {
        #region "Constants"

        /// <summary>
        /// Default database collation
        /// </summary>
        public const string DEFAULT_DB_COLLATION = "Latin1_General_CI_AS";


        /// <summary>
        /// All supported database collations (separated by semicolon)
        /// </summary>
        private static readonly Lazy<HashSet<string>> SUPPORTED_DB_COLLATIONS = new Lazy<HashSet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            DEFAULT_DB_COLLATION,
            "SQL_Latin1_General_CP1_CI_AS"
        });

        #endregion


        #region "Variables"

        private static string mDatabaseCollation = DEFAULT_DB_COLLATION;

        /// <summary>
        /// Defines the version of the current database
        /// </summary>
        private static readonly CMSStatic<string> mDatabaseVersion = new CMSStatic<string>();

        /// <summary>
        /// Object for locking the thread context
        /// </summary>
        private static readonly object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Database collation.
        /// </summary>
        public static string DatabaseCollation
        {
            get
            {
                return mDatabaseCollation;
            }
            set
            {
                mDatabaseCollation = value;
            }
        }


        /// <summary>
        /// Indicates whether connection string is set and objects exist in database.
        /// </summary>
        public static bool IsDatabaseAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(DatabaseVersion);
            }
        }


        /// <summary>
        /// Returns the version of the database that the application uses. Returns null if connection string is not initialized.
        /// </summary>
        public static string DatabaseVersion
        {
            get
            {
                if (mDatabaseVersion.Value == null)
                {
                    // Do not get version if database not connected
                    if (!ConnectionHelper.IsConnectionStringInitialized)
                    {
                        return null;
                    }

                    lock (lockObject)
                    {
                        if (mDatabaseVersion.Value == null)
                        {
                            var version = TableManager.DefaultSystemTableManagerObject.DatabaseVersion;

                            if (String.IsNullOrEmpty(version))
                            {
                                // Do not cache version if not available
                                return null;
                            }

                            // Get the database version
                            mDatabaseVersion.Value = version;
                        }
                    }
                }

                return mDatabaseVersion.Value;
            }
            internal set
            {
                mDatabaseVersion.Value = value;
            }
        }


        /// <summary>
        /// Returns whether database version is correct and matches the application version.   
        /// </summary>
        public static bool IsCorrectDatabaseVersion
        {
            get
            {
                return (DatabaseVersion == CMSVersion.MainVersion);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists.
        /// </summary>
        /// <param name="connectionString">Connection string to the database</param>
        public static bool DatabaseExists(string connectionString)
        {
            // Try to open the connection
            IDataConnection conn = null;
            try
            {
                conn = ConnectionHelper.GetConnection(connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }


        /// <summary>
        /// Returns database collation.
        /// </summary>
        /// <param name="connectionString">Connection string to the database</param>
        public static string GetDatabaseCollation(string connectionString)
        {
            string collation = null;

            // Get current database collation
            IDataConnection conn = ConnectionHelper.GetConnection(connectionString);

            DataSet dsColl = conn.ExecuteQuery("SELECT convert(sysname,DatabasePropertyEx(db_name(),'Collation'))", null, QueryTypeEnum.SQLQuery, false);
            if (!DataHelper.DataSourceIsEmpty(dsColl))
            {
                // Check the collation
                collation = ValidationHelper.GetString(dsColl.Tables[0].Rows[0][0], "");
                conn.Close();
            }
            return collation;
        }


        /// <summary>
        /// Checks if SQL account is granted with specific permission.
        /// </summary>
        /// <param name="permission">SQL permission to check</param>
        /// <param name="authenticationMode">Authentication type</param>
        /// <param name="serverName">Server name</param>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <returns>Returns <c>true</c> if SQL account is granted permission.</returns>
        public static bool CheckDBPermission(string permission, SQLServerAuthenticationModeEnum authenticationMode, string serverName, string userName, string password)
        {
            if (String.IsNullOrEmpty(serverName))
            {
                // no server to connect to, no granted permission
                return false;
            }

            string connectionString = ConnectionHelper.BuildConnectionString(authenticationMode, serverName, null, userName, password, 10);
            using (new CMSConnectionScope(connectionString))
            {
                try
                {
                    DataSet result = ConnectionHelper.ExecuteQuery(String.Format("SELECT permission_name FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name LIKE '{0}'", permission), null, QueryTypeEnum.SQLQuery);

                    return !DataHelper.DataSourceIsEmpty(result);
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Checks if the specified collation is supported.
        /// </summary>
        /// <param name="collation">Collation to check.</param>
        public static bool IsSupportedDatabaseCollation(string collation)
        {
            return SUPPORTED_DB_COLLATIONS.Value.Contains(collation);
        }


        /// <summary>
        /// Returns the database name.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static string GetDatabaseName(string connectionString)
        {
            // Get current database collation
            IDataConnection conn = ConnectionHelper.GetConnection(connectionString);

            try
            {
                // Prepare the database name
                string query = "SELECT db_name()";

                return ValidationHelper.GetString(conn.ExecuteScalar(query, null, QueryTypeEnum.SQLQuery, true), null);
            }
            catch
            {
                // Exception is expected
            }

            return null;
        }


        /// <summary>
        /// Change the collation of specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to the database</param>
        /// <param name="databaseName">Database name (if NULL current database is used)</param>
        /// <param name="collation">Collation</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collation"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collation"/> is empty string</exception>
        public static void ChangeDatabaseCollation(string connectionString, string databaseName, string collation)
        {
            if (collation == null)
            {
                throw new ArgumentNullException("collation");
            }

            if (collation == "")
            {
                throw new ArgumentException("Argument must not be the empty string.", "collation");
            }

            // Get current database collation
            IDataConnection conn = ConnectionHelper.GetConnection(connectionString);

            try
            {
                // Prepare the database name
                if (String.IsNullOrEmpty(databaseName))
                {
                    databaseName = "db_name()";
                }
                else if (!databaseName.StartsWith("[", StringComparison.Ordinal))
                {
                    databaseName = "[" + databaseName + "]";
                }

                string query = "ALTER DATABASE " + databaseName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ";
                query += "ALTER DATABASE " + databaseName + " COLLATE " + collation + "; ";
                query += "ALTER DATABASE " + databaseName + " SET MULTI_USER";

                conn.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery, true);

                // Dummy query
                conn.ExecuteQuery("SELECT TOP 1 ClassID FROM CMS_Class", null, QueryTypeEnum.SQLQuery, false);
            }
            catch
            {
                // Exception is expected
            }
            finally
            {
                conn.Close();
            }
        }


        /// <summary>
        /// Checks the database version, returns true if the version is correct, reports error in case the version is wrong
        /// </summary>
        public static bool CheckDatabaseVersion()
        {
            // Check the version
            if (!IsCorrectDatabaseVersion)
            {
                // Report error about not being able to connect
                string version = DatabaseVersion;

                var error = "The database version '" + version + "' does not match the project version '" + CMSVersion.MainVersion + "', please check your connection string.";
                
                CMSApplication.ApplicationErrorMessage = error;

                return false;
            }

            return true;
        }


        /// <summary>
        /// Clears the internal cached values
        /// </summary>
        internal static void Clear()
        {
            mDatabaseVersion.Value = null;
        }

        #endregion
    }
}
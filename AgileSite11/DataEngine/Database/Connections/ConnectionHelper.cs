using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Connection helper class.
    /// </summary>
    public static class ConnectionHelper
    {
        #region "Variables"

        // Table of connection string prefixes per domain [domain -> connectionString prefix]
        private static readonly SafeDictionary<string, string> mDomainConnectionStringsPrefixes = new SafeDictionary<string, string>();

        // Current connection string.
        private static readonly CMSStatic<string> mConnectionString = new CMSStatic<string>();

        // Current connection string name.
        private static readonly CMSStatic<string> mConnectionStringName = new CMSStatic<string>();

        // Current connection string prefix.
        private static readonly CMSStatic<string> mConnectionStringPrefix = new CMSStatic<string>();

        // Dictionary of connection strings
        private static readonly Dictionary<string, string> mConnectionStrings = new Dictionary<string, string>();


        /// <summary>
        /// Default connection string name
        /// </summary>
        public const string DEFAULT_CONNECTIONSTRING_NAME = "CMSConnectionString";

        /// <summary>
        /// If true, disposes connection after the connection is closed.
        /// </summary>
        internal static readonly BoolAppSetting DisposeConnectionAfterClose = new BoolAppSetting("CMSDisposeConnectionAfterClose");

        /// <summary>
        /// Isolation level for SQL operations with transactions.
        /// </summary>
        public static readonly AppSetting<IsolationLevel> TransactionIsolationLevel = new AppSetting<IsolationLevel>("CMSTransactionIsolationLevel", IsolationLevel.ReadCommitted, GetIsolationLevel);

        /// <summary>
        /// Command timeout for the SQL commands.
        /// </summary>
        public static readonly IntAppSetting DefaultCommandTimeout = new IntAppSetting("CMSSQLCommandTimeout", 0);

        /// <summary>
        /// Command timeout in seconds for the SQL queries which are known to possibly take more time than standard command timeout.
        /// Long running queries have to be explicitly wrapped with CMSConnectionScope with its CommandTimeout property set to this value.
        /// </summary>
        public static readonly IntAppSetting LongRunningCommandTimeout = new IntAppSetting("CMSSQLLongRunningCommandTimeout", 3600);

        /// <summary>
        /// If true, the thread safety of the connection access is checked (the connection must be used only in thread where it was originally created).
        /// </summary>
        internal static readonly BoolAppSetting CheckThreadSafety = new BoolAppSetting("CMSCheckConnectionThreadSafety");

        /// <summary>
        /// If true, single connection is used for the entire request.
        /// </summary>
        internal static readonly BoolAppSetting UseContextConnection = new BoolAppSetting("CMSUseContextConnection", true);

        /// <summary>
        /// If true, the connection is opened only once for the entire request and kept open until the end of the request.
        /// </summary>
        internal static readonly BoolAppSetting KeepContextConnectionOpen = new BoolAppSetting("CMSKeepContextConnectionOpen");

        #endregion


        #region "Properties"

        /// <summary>
        /// Global connection string name
        /// </summary>
        public static string ConnectionStringName
        {
            get
            {
                string name = mConnectionStringName;
                if (name == null)
                {
                    return DEFAULT_CONNECTIONSTRING_NAME;
                }
                else
                {
                    return name;
                }
            }
            set
            {
                mConnectionStringName.Value = value;
                ConnectionString = null;
            }
        }


        /// <summary>
        /// Global connection string prefix
        /// </summary>
        public static string ConnectionStringPrefix
        {
            get
            {
                return mConnectionStringPrefix;
            }
            set
            {
                mConnectionStringPrefix.Value = value;
            }
        }


        /// <summary>
        /// Global connection string.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (mConnectionString.Value == null)
                {
                    // Initialize the connection string
                    string connectionString = LoadConnectionString(ConnectionStringName);
                    if (!String.IsNullOrEmpty(connectionString))
                    {
                        mConnectionString.Value = connectionString;
                    }
                }

                return mConnectionString;
            }
            set
            {
                mConnectionString.Value = value;
                DatabaseHelper.DatabaseVersion = null;

                if (value != null)
                {
                    SettingsHelper.ConnectionStrings.SetConnectionString(ConnectionStringName, value);
                }
            }
        }


        /// <summary>
        /// Indicates whether connection string is initialized or not.
        /// </summary>
        public static bool IsConnectionStringInitialized
        {
            get
            {
                return !String.IsNullOrEmpty(ConnectionString);
            }
        }


        /// <summary>
        /// Returns true if the connection is available.
        /// </summary>
        public static bool ConnectionAvailable
        {
            get
            {
                return (CMSApplication.ApplicationInitialized == true) && IsConnectionStringInitialized;
            }
        }

        #endregion


        #region "Connection methods"

        /// <summary>
        /// Initializes the connection helper
        /// </summary>
        internal static void Init()
        {
            DataConnectionFactory.OnGetConnection += GetConnection;
        }


        /// <summary>
        /// Clears the connection helper cache
        /// </summary>
        public static void Clear()
        {
            mConnectionString.Value = null;
            DatabaseHelper.DatabaseVersion = null;

            mConnectionStringName.Value = null;
            mConnectionStringPrefix.Value = null;
        }


        /// <summary>
        /// Gets the connection string name for the given domain
        /// </summary>
        /// <param name="domain">Domain name</param>
        public static string GetConnectionStringPrefix(string domain)
        {
            domain = domain.ToLowerInvariant();

            // Try to get cached information
            string result = mDomainConnectionStringsPrefixes[domain];
            if (result == null)
            {
                string domainPrefix = $"({domain})";

                // Find the first one available by the prefix
                result = GetFirstFoundConnectionString(DEFAULT_CONNECTIONSTRING_NAME, domainPrefix, "");

                // Save to the cache
                mDomainConnectionStringsPrefixes[domain] = result;
            }

            return result;
        }


        /// <summary>
        /// Returns the connection string name with prefix set in <see cref="ConnectionStringPrefix" /> from the given <paramref name="connectionStringName"/>.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        internal static string GetConnectionStringNameWithPrefix(string connectionStringName)
        {
            if (!String.IsNullOrEmpty(ConnectionStringPrefix))
            {
                connectionStringName = ConnectionStringPrefix + (connectionStringName ?? DEFAULT_CONNECTIONSTRING_NAME);
            }

            return connectionStringName;
        }


        /// <summary>
        /// Returns the connection.
        /// </summary>
        /// <param name="connectionString">Connection string. If no connection string is provided, CMSConnectionString configuration value is used instead</param>
        public static GeneralConnection GetConnection(string connectionString = null)
        {
            return GeneralConnection.CreateInstance(connectionString);
        }


        /// <summary>
        /// Returns the connection.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <param name="defaultIfNotFound">If true, the default connection string is returned if the given connection string is not found</param>
        public static GeneralConnection GetConnectionByName(string connectionStringName, bool defaultIfNotFound = false)
        {
            connectionStringName = GetConnectionStringNameWithPrefix(connectionStringName);

            var connectionString = GetConnectionString(connectionStringName, defaultIfNotFound);

            return GetConnection(connectionString);
        }


        /// <summary>
        /// Gets the connection string of provided <paramref name="connectionStringName"/>.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <exception cref="ArgumentException">Thrown when there is no connection string with provided <paramref name="connectionStringName"/>.</exception>
        public static string GetConnectionString(string connectionStringName)
        {
            return GetConnectionString(connectionStringName, false);
        }


        /// <summary>
        /// Gets the connection string of provided <paramref name="connectionStringName"/>.
        /// </summary>
        /// <param name="connectionStringName">Connection string name.</param>
        /// <param name="nullIfNotFound">If <c>true</c>, the <c>null</c> is returned if the given connection string is not found.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="nullIfNotFound"/> is <c>false</c> and there is no connection string with provided <paramref name="connectionStringName"/>.</exception>
        public static string GetConnectionString(string connectionStringName, bool nullIfNotFound)
        {
            // Empty name is always a default connection string
            if (String.IsNullOrEmpty(connectionStringName))
            {
                return null;
            }

            // Get from the settings
            var connString = SettingsHelper.ConnectionStrings[connectionStringName];
            if (connString != null)
            {
                return connString.ConnectionString;
            }

            if (nullIfNotFound)
            {
                return null;
            }

            throw new ArgumentException($"Connection string '{connectionStringName}' not found, please check the web.config file.", nameof(connectionStringName));
        }


        /// <summary>
        /// Returns <c>true</c> if provided <paramref name="connectionStringName"/> belongs to a connection string present in <see cref="SettingsHelper.ConnectionStrings"/>.
        /// </summary>
        /// <param name="connectionStringName">Name of connection string that might or might not exist.</param>
        private static bool ConnectionStringExists(string connectionStringName)
        {
            return GetConnectionString(connectionStringName, true) != null;
        }


        /// <summary>
        /// Returns <paramref name="connectionStringName"/> if it is valid name of a connection string that exists in <see cref="SettingsHelper.ConnectionStrings"/>;
        /// returns <paramref name="defaultConnectionStringName"/> otherwise;
        /// </summary>
        /// <param name="connectionStringName">Name of connection string that might or might not exist.</param>
        /// <param name="defaultConnectionStringName">Name of default connection to return when <paramref name="connectionStringName"/> connection string does not exist (<see cref="DEFAULT_CONNECTIONSTRING_NAME"/> by default).</param>
        internal static string EnsureExistingConnectionStringName(string connectionStringName, string defaultConnectionStringName = DEFAULT_CONNECTIONSTRING_NAME)
        {
            return ConnectionStringExists(connectionStringName)
                ? connectionStringName
                : defaultConnectionStringName;
        }


        /// <summary>
        /// Gets the first found connection string by the prefix, if none found, returns null
        /// </summary>
        /// <param name="baseConnectionString">Base connection string</param>
        /// <param name="prefixes">Connection string prefixes to try</param>
        public static string GetFirstFoundConnectionString(string baseConnectionString, params string[] prefixes)
        {
            // Search by all names
            foreach (string prefix in prefixes)
            {
                string name = prefix + baseConnectionString;

                // Try to get the connection string with the given name
                if (GetConnectionString(name, true) != null)
                {
                    return prefix;
                }
            }

            return null;
        }

        #endregion


        #region "Execute methods"

        /// <summary>
        /// <para>
        /// Executes the query asynchronously and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="macros">Query expressions.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task<object> ExecuteScalarAsync(string queryName, CancellationToken cancellationToken, QueryDataParameters parameters = null, QueryMacros macros = null)
        {
            var conn = GetConnection();
            var qi = QueryInfoProvider.GetQueryInfo(queryName);
            var query = new QueryParameters(qi, parameters, macros);

            return conn.ExecuteScalarAsync(query, cancellationToken);
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="macros">Query expressions.</param>
        public static object ExecuteScalar(string queryName, QueryDataParameters parameters = null, QueryMacros macros = null)
        {
            var conn = GetConnection();
            var qi = QueryInfoProvider.GetQueryInfo(queryName);
            var query = new QueryParameters(qi, parameters, macros);

            return conn.ExecuteScalar(query);
        }


        /// <summary>
        /// <para>
        /// Executes the query asynchronously and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query text.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <param name="transaction">If true, connection uses transaction for the query.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task<object> ExecuteScalarAsync(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, CancellationToken cancellationToken, bool transaction = false)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType, transaction);

            return conn.ExecuteScalarAsync(query, cancellationToken);
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="queryText">Query text.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="transaction">If true, connection uses transaction for the query.</param>
        public static object ExecuteScalar(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, bool transaction = false)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType, transaction);

            return conn.ExecuteScalar(query);
        }


        /// <summary>
        /// <para>
        /// Executes the query asynchronously and returns the number of rows affected.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="macros">Query expressions.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task<int> ExecuteNonQueryAsync(string queryName, CancellationToken cancellationToken, QueryDataParameters parameters = null, QueryMacros macros = null)
        {
            var conn = GetConnection();
            var qi = QueryInfoProvider.GetQueryInfo(queryName);
            var query = new QueryParameters(qi, parameters, macros);

            return conn.ExecuteNonQueryAsync(query, cancellationToken);
        }


        /// <summary>
        /// Executes the query and returns the number of rows affected.
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="macros">Query expressions.</param>
        public static int ExecuteNonQuery(string queryName, QueryDataParameters parameters = null, QueryMacros macros = null)
        {
            var conn = GetConnection();
            var qi = QueryInfoProvider.GetQueryInfo(queryName);
            var query = new QueryParameters(qi, parameters, macros);

            return conn.ExecuteNonQuery(query);
        }


        /// <summary>
        /// <para>
        /// Executes the query asynchronously and returns the number of rows affected.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query text.</param>       
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <param name="transaction">If true, connection uses transaction for the query.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task<int> ExecuteNonQueryAsync(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, CancellationToken cancellationToken, bool transaction = false)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType, transaction);

            return conn.ExecuteNonQueryAsync(query, cancellationToken);
        }


        /// <summary>
        /// Executes the query and returns the number of rows affected.
        /// </summary>
        /// <param name="queryText">Query text.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="transaction">If true, connection uses transaction for the query.</param>
        public static int ExecuteNonQuery(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, bool transaction = false)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType, transaction);

            return conn.ExecuteNonQuery(query);
        }


        /// <summary>
        /// Executes query and returns result as a dataset.
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="where">WHERE expression</param>
        /// <param name="orderBy">Sort expression</param>
        /// <param name="topN">Top N expression</param>
        /// <param name="columns">Columns expression</param>
        public static DataSet ExecuteQuery(string queryName, QueryDataParameters parameters, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            int totalRecords = 0;

            return ExecuteQuery(queryName, parameters, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Executes query and returns result as a dataset.
        /// </summary>
        /// <param name="queryName">Name of the query in format application.class.queryname</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="where">WHERE expression</param>
        /// <param name="orderBy">Sort expression</param>
        /// <param name="topN">Top N expression</param>
        /// <param name="columns">Columns expression</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static DataSet ExecuteQuery(string queryName, QueryDataParameters parameters, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            var q = new DataQuery(queryName)
            {
                Parameters = parameters,
                WhereCondition = where,
                OrderByColumns = orderBy,
                TopNRecords = topN,
                Offset = offset,
                MaxRecords = maxRecords
            };

            q.Columns(columns);

            var res = q.Result;
            totalRecords = q.TotalRecords;

            return res;
        }


        /// <summary>
        /// <para>
        /// Executes the query asynchronously and returns result as a <see cref="DbDataReader"/>.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query text.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task<DbDataReader> ExecuteReaderAsync(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, CommandBehavior commandBehavior, CancellationToken cancellationToken)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType);

            return conn.ExecuteReaderAsync(query, commandBehavior, cancellationToken);
        }


        /// <summary>
        /// Executes the query and returns result of the query as a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="queryText">Query text.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <param name="queryType">Query type.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        public static DbDataReader ExecuteReader(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, CommandBehavior commandBehavior)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType);

            return conn.ExecuteReader(query, commandBehavior);
        }


        /// <summary>
        /// Performs a bulk insert of the data into a target database table
        /// </summary>
        /// <param name="sourceData">Source data table</param>
        /// <param name="targetTable">Name of the target DB table</param>
        /// <param name="insertSettings">Bulk insert configuration</param>
        public static void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings = null)
        {
            var conn = GetConnection();

            conn.BulkInsert(sourceData, targetTable, insertSettings);
        }


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="queryType">Query type</param>
        /// <param name="transaction">If true, connection uses transaction for the query</param>
        public static DataSet ExecuteQuery(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, bool transaction = false)
        {
            var conn = GetConnection();
            var query = new QueryParameters(queryText, parameters, queryType, transaction);

            return conn.ExecuteQuery(query);
        }


        /// <summary>
        /// Executes query and returns result as a dataset.
        /// </summary>
        /// <param name="query">Query to execute</param>
        public static DataSet ExecuteQuery(QueryParameters query)
        {
            var conn = GetConnection();
            var ds = conn.ExecuteQuery(query);

            return ds;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Builds a connection string based on specified parameters.
        /// </summary>
        /// <param name="authenticationMode">Authentication type</param>
        /// <param name="serverName">The name or network address of the instance of SQL Server to which to connect.</param>
        /// <param name="databaseName">The name of the database. Can be <c>null</c> or <c>String.Empty</c>.</param>
        /// <param name="userName">User name (used only with <see cref="SQLServerAuthenticationModeEnum.SQLServerAuthentication"/>)</param>
        /// <param name="password">User password (used only with <see cref="SQLServerAuthenticationModeEnum.SQLServerAuthentication"/>)</param>
        /// <param name="timeout">The length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error.</param>
        /// <param name="language">Connection language. If not provided, "English" is used.</param>
        /// <param name="isForAzure">If <c>true</c>, connection string is meant to be used with Microsoft Azure.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serverName"/> is <c>null</c> or <c>String.Empty</c>.</exception>
        public static string BuildConnectionString(SQLServerAuthenticationModeEnum authenticationMode, string serverName, string databaseName, string userName, string password, int timeout, string language = null, bool isForAzure = false)
        {
            if (String.IsNullOrEmpty(serverName))
            {
                throw new ArgumentException("Server name cannot be empty.", nameof(serverName));
            }

            const string DEFAULT_LANGUAGE = "English";

            var isAzureAvailable = isForAzure && AzureHelper.IsSQLAzureServer(serverName);
            var isSqlServerAuthentication = authenticationMode == SQLServerAuthenticationModeEnum.SQLServerAuthentication;

            if (isSqlServerAuthentication)
            {
                if (String.IsNullOrEmpty(userName))
                {
                    throw new ArgumentException("Username can't be empty when using SQL Server authentication.", nameof(userName));
                }

                if (String.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Password can't be empty when using SQL Server authentication.", nameof(password));
                }
            }

            var builder = new SqlConnectionStringBuilder
            {
                Encrypt = isAzureAvailable,
                PersistSecurityInfo = false,
                DataSource = GetConnectionServerName(serverName, isAzureAvailable),
                InitialCatalog = databaseName ?? String.Empty,
                CurrentLanguage = language ?? DEFAULT_LANGUAGE,
                IntegratedSecurity = !isSqlServerAuthentication,
            };

            if (isSqlServerAuthentication)
            {
                builder.UserID = userName;
                builder.Password = password;
            }

            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }

            // Returning the builder result as it is causes some unexpected problems.
            // E.g: database separation is unable to connect to the newly created db.
            // Probably due to connection string comparison somewhere deep in our db connection handling,
            // yet inexplicably the addition of something extra (the semicolon in this case) fixes the issue.
            return builder.ConnectionString + ";";
        }


        /// <summary>
        /// If <paramref name="isForAzure"/> set to <c>false</c>, returns unchanged <paramref name="serverName"/>.
        /// If <paramref name="isForAzure"/> set to <c>true</c>, returns <paramref name="serverName"/> prefixed with "tcp:".
        /// </summary>
        private static string GetConnectionServerName(string serverName, bool isForAzure)
        {
            const string AZURE_SERVER_NAME_PROTOCOL_PREFIX = "tcp:";

            var shouldPrependPrefix = isForAzure && !serverName.StartsWith(AZURE_SERVER_NAME_PROTOCOL_PREFIX, StringComparison.InvariantCultureIgnoreCase);

            return (shouldPrependPrefix ? AZURE_SERVER_NAME_PROTOCOL_PREFIX : String.Empty) + serverName;
        }


        /// <summary>
        /// Tests the given connection parameters.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static string TestConnection(string connectionString)
        {
            IDataConnection conn = null;
            try
            {
                // Get default connection string if not set
                if (connectionString == null)
                {
                    connectionString = ConnectionString;
                }

                conn = DataConnectionFactory.GetNativeConnection(connectionString);
                conn.Open();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                conn?.Close();
            }
        }


        /// <summary>
        /// Tests the given connection parameters.
        /// </summary>
        /// <param name="authenticationMode">Authentication type</param>
        /// <param name="serverName">Server name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        public static string TestConnection(SQLServerAuthenticationModeEnum authenticationMode, string serverName, string databaseName, string userName, string password)
        {
            string connectionString = BuildConnectionString(authenticationMode, serverName, databaseName, userName, password, 10);
            return TestConnection(connectionString);
        }


        /// <summary>
        /// Returns the connection string.
        /// </summary>
        public static string GetSqlConnectionString()
        {
            return ConnectionString;
        }


        /// <summary>
        /// Returns the connection string.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        private static string LoadConnectionString(string connectionStringName)
        {
            if (String.IsNullOrEmpty(connectionStringName))
            {
                connectionStringName = DEFAULT_CONNECTIONSTRING_NAME;
            }

            // Check the web.config
            var cs = SettingsHelper.ConnectionStrings[connectionStringName];

            string result = cs?.ConnectionString.Trim() ?? "";

            return result;
        }


        /// <summary>
        /// Returns the connection string.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        public static string GetSqlConnectionString(string connectionStringName)
        {
            // Handle the default connection string
            if (String.IsNullOrEmpty(connectionStringName) || connectionStringName.Equals(DEFAULT_CONNECTIONSTRING_NAME, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConnectionString;
            }

            // Try to get cached value
            string lowerName = connectionStringName.ToLowerInvariant();

            string result = mConnectionStrings[lowerName];
            if (result == null)
            {
                // Load and cache
                result = LoadConnectionString(connectionStringName);

                mConnectionStrings[lowerName] = result;
            }

            return result;
        }


        /// <summary>
        /// Returns the isolation level evaluated from the string representation.
        /// </summary>
        /// <param name="value">String value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public static IsolationLevel GetIsolationLevel(object value, IsolationLevel defaultValue)
        {
            // No value = default value
            if (value == null)
            {
                return defaultValue;
            }

            // Get the proper representation
            switch (ValidationHelper.GetString(value, string.Empty).ToLowerInvariant())
            {
                case "chaos":
                    return IsolationLevel.Chaos;

                case "readcommitted":
                    return IsolationLevel.ReadCommitted;

                case "readuncommitted":
                    return IsolationLevel.ReadUncommitted;

                case "repeatableread":
                    return IsolationLevel.RepeatableRead;

                case "serializable":
                    return IsolationLevel.Serializable;

                case "snapshot":
                    return IsolationLevel.Snapshot;

                default:
                    return defaultValue;
            }
        }


        /// <summary>
        /// Initializes the request context to use proper database based on current domain name
        /// </summary>
        public static void InitRequestContext()
        {
            // Ensure the default request context
            string domain = RequestContext.CurrentDomain;

            string connStringPrefix = GetConnectionStringPrefix(domain);

            if (!String.IsNullOrEmpty(connStringPrefix))
            {
                SetConnectionContext(connStringPrefix);
            }
        }


        /// <summary>
        /// Sets current context to the given connection string prefix, e.g. for prefix "External" the connection string "ExternalCMSConnectionString" will be used
        /// instead of the default "CMSConnectionString". To revert to default DB, use prefix null or empty string.
        /// </summary>
        /// <param name="connStringPrefix">Connection string prefix</param>
        internal static void SetConnectionContext(string connStringPrefix)
        {
            if (!String.IsNullOrEmpty(connStringPrefix))
            {
                string connString = connStringPrefix + DEFAULT_CONNECTIONSTRING_NAME;

                StaticContext.CurrentContextName = connString;
                CacheHelper.CurrentCachePrefix = connString;

                ConnectionStringName = connString;
                ConnectionStringPrefix = connStringPrefix;
            }
            else
            {
                StaticContext.CurrentContextName = null;
                CacheHelper.CurrentCachePrefix = null;
            }
        }

        #endregion


        /// <summary>
        /// Returns true, if the given connection string represents a connection to the local CMS database.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        internal static bool IsLocalCMSDatabase(string connectionString)
        {
            return String.IsNullOrEmpty(connectionString) 
                || connectionString.Equals(ConnectionString, StringComparison.OrdinalIgnoreCase) 
                || connectionString.Equals(DatabaseSeparationHelper.ConnStringSeparate, StringComparison.OrdinalIgnoreCase);
        }
    }
}
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a general database connection.
    /// </summary>
    [Serializable]
    public class GeneralConnection : ISerializable, IDataConnection
    {
        #region "Variables"

        private IDataConnection mDataConnection;

        #endregion


        #region "Properties"

        /// <summary>
        /// Data connection for specific provider.
        /// </summary>
        public IDataConnection DataConnection
        {
            get
            {
                return mDataConnection ?? (mDataConnection = DataConnectionFactory.GetNativeConnection(ConnectionString));
            }
            set
            {
                mDataConnection = value;
            }
        }


        /// <summary>
        /// Connection string for specific provider.
        /// </summary>
        public string ConnectionString
        {
            get;
            private set;
        }


        /// <summary>
        /// If true, the debug is disabled on this connection
        /// </summary>
        public bool DisableConnectionDebug
        {
            get
            {
                return DataConnection.DisableConnectionDebug;
            }
            set
            {
                DataConnection.DisableConnectionDebug = value;
            }
        }


        /// <summary>
        /// If true, the debug of the executed queries is disabled on this connection
        /// </summary>
        public bool DisableQueryDebug
        {
            get
            {
                return DataConnection.DisableQueryDebug;
            }
            set
            {
                DataConnection.DisableQueryDebug = value;
            }
        }


        /// <summary>
        /// If true, the connection stays open even if close is requested.
        /// </summary>
        public bool KeepOpen
        {
            get
            {
                return DataConnection.KeepOpen;
            }
            set
            {
                DataConnection.KeepOpen = value;
            }
        }


        /// <summary>
        /// If true, the connection uses the scope connection
        /// </summary>
        public bool UseScopeConnection
        {
            get
            {
                return DataConnection.UseScopeConnection;
            }
            set
            {
                DataConnection.UseScopeConnection = value;
            }
        }


        /// <summary>
        /// Command timeout.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return DataConnection.CommandTimeout;
            }
            set
            {
                DataConnection.CommandTimeout = value;
            }
        }

        #endregion


        #region "Serialization"

        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ConnectionString", ConnectionString);
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public GeneralConnection(SerializationInfo info, StreamingContext context)
        {
            ConnectionString = (string)info.GetValue("ConnectionString", typeof(string));
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">Connection string. If no connection string is provided, CMSConnectionString configuration value is used instead</param>
        protected GeneralConnection(string connectionString)
        {
            if (connectionString != null)
            {
                ConnectionString = connectionString;
            }
        }


        /// <summary>
        /// Creates new instance of the connection object.
        /// </summary>
        /// <param name="connectionString">Connection string. If no connection string is provided, CMSConnectionString configuration value is used instead</param>
        internal static GeneralConnection CreateInstance(string connectionString)
        {
            return new GeneralConnection(connectionString);
        }


        /// <summary>
        /// Disposes the connection
        /// </summary>
        public void Dispose()
        {
            if (mDataConnection != null)
            {
                mDataConnection.Dispose();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the connection is open.
        /// </summary>
        public bool IsOpen()
        {
            return DataConnection.IsOpen();
        }


        /// <summary>
        /// Opens the connection.
        /// </summary>
        public void Open()
        {
            DataConnection.Open();
        }


        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            DataConnection.Close();
        }


        /// <summary>
        /// Gets the connection that executes the given query
        /// </summary>
        /// <param name="query">Query parameters</param>
        private IDataConnection GetExecutingConnection(QueryParameters query)
        {
            if (query == null)
            {
                return GetExecutingConnection(null, false);
            }

            return GetExecutingConnection(query.ConnectionStringName, query.UseNewConnection);
        }


        /// <summary>
        /// Gets the connection that executes the given query
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <param name="newConnection">If true, a new connection instance is created</param>
        public IDataConnection GetExecutingConnection(string connectionStringName, bool newConnection = false)
        {
            return DataConnection.GetExecutingConnection(connectionStringName, newConnection);
        }

        #endregion


        #region "ExecuteQuery - Executing methods"

        /// <summary>
        /// Executes query and returns result as a dataset.
        /// </summary>
        /// <param name="query">Query parameters</param>
        [HideFromDebugContext]
        public virtual DataSet ExecuteQuery(QueryParameters query)
        {
            query.ExecutionType = QueryExecutionTypeEnum.ExecuteQuery;

            query.ResolveMacros();

            return RunQueryWithRetry(query);
        }


        /// <summary>
        /// Runs <paramref name="query"/>. When a deadlock occurs, the query is run again, up to <paramref name="retryCount"/> times.
        /// </summary>
        /// <param name="query">Query to be run</param>
        /// <param name="retryCount">Number of retries</param>
        /// <remarks>Queries in a transaction are never retried.</remarks>
        private DataSet RunQueryWithRetry(QueryParameters query, int retryCount = 3)
        {
            try
            {
                return RunQuery(query);
            }
            catch(Exception e)
            {
                var sqlException = e.InnerException as SqlException;
                
                if ((retryCount > 0) && !CMSTransactionScope.IsInTransaction && sqlException.HasDeadlockOccured())
                {
                    return RunQueryWithRetry(query, --retryCount);
                }
                throw;
            }
        }


        /// <summary>
        /// Executes query and returns result as a DataSet. Returns the total number of result items.
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <param name="totalRecords">Returns total records</param>
        [HideFromDebugContext]
        public virtual DataSet ExecuteQuery(QueryParameters query, ref int totalRecords)
        {
            // Finalize the query parameters
            query = query.GetParametersForExecution(totalRecords != SqlHelper.NO_TOTALRECORDS);

            // Get the data
            DataSet ds = ExecuteQuery(query);

            // Get total records from the result
            totalRecords = 0;

            SqlHelper.ProcessPagedResults(ds, ref totalRecords);

            return ds;
        }


        /// <summary>
        /// Runs the query against SQL DB.
        /// </summary>
        /// <param name="query">Query to run</param>
        [HideFromDebugContext]
        protected virtual DataSet RunQuery(QueryParameters query)
        {
            DataSet result;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteQuery.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == null))
                {
                    bool closeConnectionAtTheEnd = false;
                    bool commitTransactionAtTheEnd = false;

                    try
                    {
                        // Preprocess the query
                        SqlHelper.PreprocessQuery(query);

                        // Log the query start
                        LogQueryStart(query, conn);

                        // If connection is not open, open it
                        if (!conn.IsOpen())
                        {
                            conn.Open();
                            closeConnectionAtTheEnd = true;
                        }

                        // If transaction is required and no transaction is running, start it
                        if (query.RequiresTransaction && !conn.IsTransaction())
                        {
                            conn.BeginTransaction();
                            commitTransactionAtTheEnd = true;
                        }

                        // Execute query
                        result = conn.ExecuteQuery(query.Text, query.Params, query.Type, false);
                        e.Result = result;

                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.CommitTransaction();
                            commitTransactionAtTheEnd = false;
                        }
                    }
                    finally
                    {
                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.RollbackTransaction();
                        }

                        // If connection was opened automatically, close it
                        if (closeConnectionAtTheEnd)
                        {
                            conn.Close();
                        }

                        // Log the query end
                        LogQueryEnd(result);
                    }
                }

                // Finish execute query
                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }

        #endregion


        #region "ExecuteNonQuery - Execution methods"

        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteNonQuery"/> which executes the query asynchronously and returns the number of rows affected.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="query">Query parameters.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(QueryParameters query, CancellationToken cancellationToken)
        {
            int result;

            query.ExecutionType = QueryExecutionTypeEnum.ExecuteNonQuery;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteNonQuery.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == 0))
                {
                    // Resolve the macros
                    query.ResolveMacros();

                    bool closeConnectionAtTheEnd = false;
                    bool commitTransactionAtTheEnd = false;

                    try
                    {
                        // Log the query start
                        LogQueryStart(query, conn);

                        // If connection is not open, open it
                        if (!conn.IsOpen())
                        {
                            conn.Open();
                            closeConnectionAtTheEnd = true;
                        }

                        // If transaction is required and no transaction is running, start it
                        if (query.RequiresTransaction && !conn.IsTransaction())
                        {
                            conn.BeginTransaction();
                            commitTransactionAtTheEnd = true;
                        }

                        // Execute query
                        result = await conn.ExecuteNonQueryAsync(query.Text, query.Params, query.Type, false, cancellationToken);
                        e.Result = result;

                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.CommitTransaction();
                            commitTransactionAtTheEnd = false;
                        }
                    }
                    finally
                    {
                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.RollbackTransaction();
                        }

                        // If connection was opened automatically, close it
                        if (closeConnectionAtTheEnd)
                        {
                            conn.Close();
                        }

                        // Log the query end
                        LogQueryEnd(result);
                    }
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }


        /// <summary>
        /// Executes the query and returns the number of rows affected.
        /// </summary>
        /// <param name="query">Query parameters.</param>
        public virtual int ExecuteNonQuery(QueryParameters query)
        {
            int result;

            query.ExecutionType = QueryExecutionTypeEnum.ExecuteNonQuery;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteNonQuery.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == 0))
                {
                    // Resolve the macros
                    query.ResolveMacros();

                    bool closeConnectionAtTheEnd = false;
                    bool commitTransactionAtTheEnd = false;

                    try
                    {
                        // Log the query start
                        LogQueryStart(query, conn);

                        // If connection is not open, open it
                        if (!conn.IsOpen())
                        {
                            conn.Open();
                            closeConnectionAtTheEnd = true;
                        }

                        // If transaction is required and no transaction is running, start it
                        if (query.RequiresTransaction && !conn.IsTransaction())
                        {
                            conn.BeginTransaction();
                            commitTransactionAtTheEnd = true;
                        }

                        // Execute query
                        result = conn.ExecuteNonQuery(query.Text, query.Params, query.Type, false);
                        e.Result = result;

                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.CommitTransaction();
                            commitTransactionAtTheEnd = false;
                        }
                    }
                    finally
                    {
                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.RollbackTransaction();
                        }

                        // If connection was opened automatically, close it
                        if (closeConnectionAtTheEnd)
                        {
                            conn.Close();
                        }

                        // Log the query end
                        LogQueryEnd(result);
                    }
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }

        #endregion


        #region "ExecuteReader - Execution methods"

        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteReader"/> which executes the query asynchronously and returns result as a <see cref="DbDataReader"/>.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="query">Query parameters.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(QueryParameters query, CommandBehavior commandBehavior, CancellationToken cancellationToken)
        {
            query.ExecutionType = QueryExecutionTypeEnum.ExecuteReader;

            DbDataReader result;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteReader.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == null))
                {
                    // Resolve the query macros
                    query.ResolveMacros();

                    // Log the query start
                    LogQueryStart(query, conn);

                    // Execute reader
                    result = await conn.ExecuteReaderAsync(query.Text, query.Params, query.Type, commandBehavior, cancellationToken);
                    e.Result = result;

                    // Log the query end
                    LogQueryEnd(result);
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }


        /// <summary>
        /// Executes the query and returns result of the query as a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="query">Query parameters.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        public virtual DbDataReader ExecuteReader(QueryParameters query, CommandBehavior commandBehavior)
        {
            query.ExecutionType = QueryExecutionTypeEnum.ExecuteReader;

            DbDataReader result;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteReader.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == null))
                {
                    // Resolve the query macros
                    query.ResolveMacros();

                    // Log the query start
                    LogQueryStart(query, conn);

                    // Execute reader
                    result = conn.ExecuteReader(query.Text, query.Params, query.Type, commandBehavior);
                    e.Result = result;

                    // Log the query end
                    LogQueryEnd(result);
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }

        #endregion


        #region "ExecuteScalar - Execution methods"

        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteScalar"/>, which executes the query asynchronously
        /// and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryParameters">Query parameters.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<object> ExecuteScalarAsync(QueryParameters queryParameters, CancellationToken cancellationToken)
        {
            queryParameters.ExecutionType = QueryExecutionTypeEnum.ExecuteScalar;

            object result;

            var conn = GetExecutingConnection(queryParameters);

            // Start execute query event
            using (var h = SqlEvents.ExecuteScalar.StartEvent(queryParameters, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == null))
                {
                    // Resolve the macros
                    queryParameters.ResolveMacros();

                    bool closeConnectionAtTheEnd = false;
                    bool commitTransactionAtTheEnd = false;

                    try
                    {
                        // Log the query start
                        LogQueryStart(queryParameters, conn);

                        // If connection is not open, open it
                        if (!conn.IsOpen())
                        {
                            conn.Open();
                            closeConnectionAtTheEnd = true;
                        }

                        // If transaction is required and no transaction is running, start it
                        if (queryParameters.RequiresTransaction && !conn.IsTransaction())
                        {
                            conn.BeginTransaction();
                            commitTransactionAtTheEnd = true;
                        }

                        // Execute query
                        result = await conn.ExecuteScalarAsync(queryParameters.Text, queryParameters.Params, queryParameters.Type, false, cancellationToken);
                        e.Result = result;

                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.CommitTransaction();
                            commitTransactionAtTheEnd = false;
                        }
                    }
                    finally
                    {
                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.RollbackTransaction();
                        }

                        // If connection was opened automatically, close it
                        if (closeConnectionAtTheEnd)
                        {
                            conn.Close();
                        }

                        // Log the query end
                        LogQueryEnd(result);
                    }
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="query">Query parameters.</param>
        public virtual object ExecuteScalar(QueryParameters query)
        {
            query.ExecutionType = QueryExecutionTypeEnum.ExecuteScalar;

            object result;

            var conn = GetExecutingConnection(query);

            // Start execute query event
            using (var h = SqlEvents.ExecuteScalar.StartEvent(query, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                result = e.Result;

                if (h.CanContinue() && (result == null))
                {
                    // Resolve the macros
                    query.ResolveMacros();

                    bool closeConnectionAtTheEnd = false;
                    bool commitTransactionAtTheEnd = false;

                    try
                    {
                        // Log the query start
                        LogQueryStart(query, conn);

                        // If connection is not open, open it
                        if (!conn.IsOpen())
                        {
                            conn.Open();
                            closeConnectionAtTheEnd = true;
                        }

                        // If transaction is required and no transaction is running, start it
                        if (query.RequiresTransaction && !conn.IsTransaction())
                        {
                            conn.BeginTransaction();
                            commitTransactionAtTheEnd = true;
                        }

                        // Execute query
                        result = conn.ExecuteScalar(query.Text, query.Params, query.Type, false);
                        e.Result = result;

                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.CommitTransaction();
                            commitTransactionAtTheEnd = false;
                        }
                    }
                    finally
                    {
                        // If transaction was required and was started automatically, commit it
                        if (commitTransactionAtTheEnd)
                        {
                            conn.RollbackTransaction();
                        }

                        // If connection was opened automatically, close it
                        if (closeConnectionAtTheEnd)
                        {
                            conn.Close();
                        }

                        // Log the query end
                        LogQueryEnd(result);
                    }
                }

                h.FinishEvent();
                result = h.EventArguments.Result;
            }

            return result;
        }

        #endregion


        #region "Bulk insert"

        /// <summary>
        /// Performs a bulk insert of the data into a target database table
        /// </summary>
        /// <param name="sourceData">Source data table</param>
        /// <param name="targetTable">Name of the target DB table</param>
        /// <param name="insertSettings">Bulk insertion settings</param>
        public virtual void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings = null)
        {
            var conn = GetExecutingConnection(null);

            if (insertSettings == null)
            {
                insertSettings = new BulkInsertSettings();
            }

            using (var h = SqlEvents.BulkInsert.StartEvent(sourceData, targetTable, insertSettings, conn))
            {
                var e = h.EventArguments;

                conn = e.Connection;
                sourceData = e.SourceData;
                targetTable = e.TargetTable;
                insertSettings = e.InsertSettings;

                if (h.CanContinue())
                {
                    conn.BulkInsert(sourceData, targetTable, insertSettings);
                }

                h.FinishEvent();
            }
        }

        #endregion


        #region "Debug methods"

        /// <summary>
        /// Logs query start. Logs the query to the file and to current request log for debugging.
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="conn">Connection for the query execution</param>
        /// <returns>Returns the new log item</returns>
        public void LogQueryStart(QueryParameters query, IDataConnection conn)
        {
            if (IsQueryDebugEnabled())
            {
                var text = query.Type == QueryTypeEnum.StoredProcedure ? query.Text : query.GetFullQueryText();
                SqlDebug.LogQueryStart(query.Name, text, query.Params, conn);
            }
        }


        /// <summary>
        /// Logs the end of the query processing.
        /// </summary>
        /// <param name="result">Result</param>
        public void LogQueryEnd(object result)
        {
            if (IsQueryDebugEnabled())
            {
                SqlDebug.LogQueryEnd(result);
            }
        }


        private bool IsQueryDebugEnabled()
        {
            return !DisableQueryDebug && SqlDebug.DebugCurrentRequest;
        }

        #endregion


        #region "IDataConnection Members"

        // Native connection
        IDbConnection IDataConnection.NativeConnection
        {
            get
            {
                return DataConnection.NativeConnection;
            }
            set
            {
                DataConnection.NativeConnection = value;
            }
        }


        // Transaction
        IDbTransaction IDataConnection.Transaction
        {
            get
            {
                return DataConnection.Transaction;
            }
            set
            {
                DataConnection.Transaction = value;
            }
        }


        // Connection string name
        string IDataConnection.ConnectionStringName
        {
            get
            {
                return DataConnection.ConnectionStringName;
            }
        }


        // Connection string
        string IDataConnection.ConnectionString
        {
            get
            {
                return DataConnection.ConnectionString;
            }
        }


        // Execute query
        [HideFromDebugContext]
        DataSet IDataConnection.ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            return DataConnection.ExecuteQuery(queryText, queryParams, queryType, requiresTransaction);
        }


        Task<int> IDataConnection.ExecuteNonQueryAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken)
        {
            return DataConnection.ExecuteNonQueryAsync(queryText, queryParams, queryType, requiresTransaction, cancellationToken);
        }


        // Execute non query
        int IDataConnection.ExecuteNonQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            return DataConnection.ExecuteNonQuery(queryText, queryParams, queryType, requiresTransaction);
        }


        Task<DbDataReader> IDataConnection.ExecuteReaderAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior, CancellationToken cancellationToken)
        {
            return DataConnection.ExecuteReaderAsync(queryText, queryParams, queryType, commandBehavior, cancellationToken);
        }


        // Execute reader
        DbDataReader IDataConnection.ExecuteReader(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior)
        {
            return DataConnection.ExecuteReader(queryText, queryParams, queryType, commandBehavior);
        }


        Task<object> IDataConnection.ExecuteScalarAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken)
        {
            return DataConnection.ExecuteScalarAsync(queryText, queryParams, queryType, requiresTransaction, cancellationToken);
        }


        // Execute scalar
        object IDataConnection.ExecuteScalar(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            return DataConnection.ExecuteScalar(queryText, queryParams, queryType, requiresTransaction);
        }


        // Get XML schema
        string IDataConnection.GetXmlSchema(string tableName)
        {
            return DataConnection.GetXmlSchema(tableName);
        }


        // Native DB connection exists
        bool IDataConnection.NativeDBConnectionExists()
        {
            return DataConnection.NativeDBConnectionExists();
        }


        // Native connection exists
        bool IDataConnection.NativeConnectionExists()
        {
            return DataConnection.NativeConnectionExists();
        }


        // Is open
        bool IDataConnection.IsOpen()
        {
            return DataConnection.IsOpen();
        }


        // Open
        void IDataConnection.Open()
        {
            DataConnection.Open();
        }


        // Close
        void IDataConnection.Close()
        {
            DataConnection.Close();
        }


        // Begin transaction
        void IDataConnection.BeginTransaction()
        {
            DataConnection.BeginTransaction();
        }


        // Begin transaction
        void IDataConnection.BeginTransaction(IsolationLevel isolationLevel)
        {
            DataConnection.BeginTransaction(isolationLevel);
        }


        // Commit
        void IDataConnection.CommitTransaction()
        {
            DataConnection.CommitTransaction();
        }


        // Rollback
        void IDataConnection.RollbackTransaction()
        {
            DataConnection.RollbackTransaction();
        }


        // Is transaction
        bool IDataConnection.IsTransaction()
        {
            return DataConnection.IsTransaction();
        }

        #endregion
    }
}
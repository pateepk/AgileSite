using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents SQL Server data connection.
    /// </summary>
    public abstract class AbstractDataConnection : IDataConnection, INotCopyThreadItem
    {
        #region "Variables"

        /// <summary>
        /// Connection string name
        /// </summary>
        protected string mConnectionStringName;

        /// <summary>
        /// Connection string for specific provider.
        /// </summary>
        protected string mConnectionString;


        /// <summary>
        /// Native connection object. It depends on provider type.
        /// </summary>
        protected IDbConnection mNativeConnection;

        /// <summary>
        /// If true, the connection uses the scope connection
        /// </summary>
        protected bool mUseScopeConnection = true;

        /// <summary>
        /// Command timeout.
        /// </summary>
        protected int mCommandTimeout;
        

        /// <summary>
        /// List of nested connections
        /// </summary>
        protected List<IDataConnection> mNestedConnectionsList;


        /// <summary>
        /// List of nested connections that were automatically opened
        /// </summary>
        protected List<IDataConnection> mNestedOpenConnections = new List<IDataConnection>();


        /// <summary>
        /// List of nested connections that were automatically opened with transaction
        /// </summary>
        protected List<IDataConnection> mNestedOpenTransactions = new List<IDataConnection>();


        /// <summary>
        /// Nested connections indexed by the connection string [connectionString ->  IDbConnection]
        /// </summary>
        protected SafeDictionary<string, IDataConnection> mNestedConnections;


        /// <summary>
        /// Last connection error
        /// </summary>
        protected string mLastError;

        // Original thread ID of the connection.
        private int mOriginalThreadID;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the connection uses the scope connection
        /// </summary>
        public bool UseScopeConnection
        {
            get
            {
                return mUseScopeConnection;
            }
            set
            {
                mUseScopeConnection = value;
            }
        }


        /// <summary>
        /// Native connection object. It depends on provider type.
        /// </summary>
        public IDbConnection NativeConnection
        {
            get
            {
                if (!UseScopeConnection)
                {
                    return NativeDBConnection;
                }

                // Use connection from currently open scope
                var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                if (scopeConnection != null)
                {
                    return scopeConnection.NativeDBConnection;
                }

                return NativeDBConnection;
            }
            set
            {
                if (!UseScopeConnection)
                {
                    NativeDBConnection = value;
                }
                else
                {
                    // Use connection from currently open scope
                    var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                    if (scopeConnection != null)
                    {
                        scopeConnection.NativeDBConnection = value;
                    }
                    else
                    {
                        NativeDBConnection = value;
                    }
                }
            }
        }


        /// <summary>
        /// Native connection object. It depends on provider type.
        /// </summary>
        protected IDbConnection NativeDBConnection
        {
            get
            {
                // Ensure own connection
                if (mNativeConnection == null)
                {
                    LogConnectionOperation("new SqlConnection()", true, this);

                    mNativeConnection = CreateNativeConnection();
                }

                return mNativeConnection;
            }
            set
            {
                mNativeConnection = value;
            }
        }


        /// <summary>
        /// Transaction object.
        /// </summary>
        public IDbTransaction Transaction
        {
            get
            {
                if (!UseScopeConnection)
                {
                    return LocalTransaction;
                }

                // Use connection from currently open scope
                var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                if (scopeConnection != null)
                {
                    return scopeConnection.LocalTransaction;
                }

                return LocalTransaction;
            }
            set
            {
                if (!UseScopeConnection)
                {
                    LocalTransaction = value;
                }
                else
                {
                    // Use connection from currently open scope
                    var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                    if (scopeConnection != null)
                    {
                        scopeConnection.LocalTransaction = value;
                    }
                    else
                    {
                        LocalTransaction = value;
                    }
                }
            }
        }


        /// <summary>
        /// Transaction object of local instance.
        /// </summary>
        protected IDbTransaction LocalTransaction
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string name
        /// </summary>
        public string ConnectionStringName
        {
            get
            {
                return mConnectionStringName ?? (mConnectionStringName = SettingsHelper.ConnectionStrings.GetConnectionStringName(ConnectionString));
            }
            protected set
            {
                mConnectionStringName = value;
            }
        }


        /// <summary>
        /// Connection string for specific provider.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(mConnectionString))
                {
                    LoadDefaultConnectionString();
                }

                return mConnectionString;
            }
            protected set
            {
                mConnectionString = value;

                if (mNativeConnection != null)
                {
                    mNativeConnection.ConnectionString = mConnectionString;
                }
            }
        }


        /// <summary>
        /// Command timeout.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                if (mCommandTimeout > 0)
                {
                    return mCommandTimeout;
                }
                else if (ConnectionHelper.DefaultCommandTimeout > 0)
                {
                    return ConnectionHelper.DefaultCommandTimeout;
                }
                else
                {
                    return NativeConnection.ConnectionTimeout;
                }
            }
            set
            {
                mCommandTimeout = value;
            }
        }


        /// <summary>
        /// If true, the connection stays open even if close is requested.
        /// </summary>
        public bool KeepOpen
        {
            get
            {
                if (!UseScopeConnection)
                {
                    return LocalKeepOpen;
                }

                // Use connection from currently open scope
                var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                if (scopeConnection != null)
                {
                    return scopeConnection.LocalKeepOpen;
                }

                return LocalKeepOpen;
            }
            set
            {
                if (!UseScopeConnection)
                {
                    LocalKeepOpen = value;
                }
                else
                {
                    // Use connection from currently open scope
                    var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
                    if (scopeConnection != null)
                    {
                        scopeConnection.LocalKeepOpen = value;
                    }
                    else
                    {
                        LocalKeepOpen = value;
                    }
                }
            }
        }


        /// <summary>
        /// If true, the connection stays open even if close is requested.
        /// </summary>
        protected bool LocalKeepOpen
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the debug is disabled on this connection
        /// </summary>
        public bool DisableConnectionDebug
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the debug of queries is disabled on this connection
        /// </summary>
        public bool DisableQueryDebug
        {
            get;
            set;
        }

        #endregion


        #region "Execute methods"

        /// <summary>
        /// Performs a bulk insert of the data into a target database table
        /// </summary>
        /// <param name="sourceData">Source data table</param>
        /// <param name="targetTable">Name of the target DB table</param>
        /// <param name="insertSettings">Bulk insert configuration</param>
        public abstract void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings = null);


        /// <summary>
        /// Returns result of the query.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Indicates it query is a SQL query or stored procedure</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction</param>
        [HideFromDebugContext]
        public virtual DataSet ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;

            // Prepare the DataSet to be filled
            DataSet ds = null;
            if (queryParams != null)
            {
                ds = queryParams.FillDataSet;
            }

            if (ds == null)
            {
                ds = new DataSet();
            }
            else
            {
                // Make sure DataSet is empty
                ds.Clear();
            }

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, requiresTransaction, ref closeConnection, ref commitTransaction);

                // Prepare the adapter
                var da = CreateDataAdapter();
                da.SelectCommand = cmd;

                // Load the data
                da.Fill(ds);

                // Mark tables as originated from CMS database
                if (ConnectionHelper.IsLocalCMSDatabase(ConnectionString))
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        dt.IsFromCMSDatabase(true);
                        dt.TrackExternalData();
                    }
                }

                // Commit transaction is necessary
                if (commitTransaction)
                {
                    CommitTransaction();
                    commitTransaction = false;
                }
            }
            catch (Exception ex)
            {
                if (!HandleError(queryText, ex))
                {
                    throw;
                }
            }
            finally
            {
                // Rollback transaction if necessary
                if (commitTransaction)
                {
                    RollbackTransaction();
                }

                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }

            return ds;
        }


        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteNonQuery"/> which executes the query asynchronously and returns the number of rows affected.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;

            int result = 0;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, requiresTransaction, ref closeConnection, ref commitTransaction);

                // Execute
                result = await cmd.ExecuteNonQueryAsync(cancellationToken);

                // Commit transaction is necessary
                if (commitTransaction)
                {
                    CommitTransaction();
                    commitTransaction = false;
                }
            }
            catch (Exception ex)
            {
                if (!HandleError(queryText, ex))
                {
                    throw;
                }
            }
            finally
            {
                // Rollback transaction if necessary
                if (commitTransaction)
                {
                    RollbackTransaction();
                }

                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }

            return result;
        }


        /// <summary>
        /// Executes the query and returns the number of rows affected.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        public virtual int ExecuteNonQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;

            int result = 0;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, requiresTransaction, ref closeConnection, ref commitTransaction);

                // Execute
                result = cmd.ExecuteNonQuery();

                // Commit transaction is necessary
                if (commitTransaction)
                {
                    CommitTransaction();
                    commitTransaction = false;
                }
            }
            catch (Exception ex)
            {
                if (!HandleError(queryText, ex))
                {
                    throw;
                }
            }
            finally
            {
                // Rollback transaction if necessary
                if (commitTransaction)
                {
                    RollbackTransaction();
                }

                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }

            return result;
        }


        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteReader"/> which executes the query asynchronously and returns result as a <see cref="DbDataReader"/>.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The method executes the query with command behavior <see cref="CommandBehavior.CloseConnection"/> if passed <paramref name="commandBehavior"/> is set to <see cref="CommandBehavior.Default"/> and connection is not opened.
        /// </remarks>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior, CancellationToken cancellationToken)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, false, ref closeConnection, ref commitTransaction);

                // If the reader is supposed to close the connection, set the behavior
                if (closeConnection && (commandBehavior == CommandBehavior.Default))
                {
                    commandBehavior = CommandBehavior.CloseConnection;
                }

                // Execute the reader
                return await cmd.ExecuteReaderAsync(commandBehavior, cancellationToken);
            }
            catch (Exception ex)
            {
                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }

                if (!HandleError(queryText, ex))
                {
                    throw;
                }

                return null;
            }
        }


        /// <summary>
        /// Executes the query and returns result of the query as a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        /// <remarks>
        /// The method executes the query with command behavior <see cref="CommandBehavior.CloseConnection"/> if passed <paramref name="commandBehavior"/> is set to <see cref="CommandBehavior.Default"/> and connection is not opened.
        /// </remarks>
        public virtual DbDataReader ExecuteReader(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, false, ref closeConnection, ref commitTransaction);

                // If the reader is supposed to close the connection, set the behavior
                if (closeConnection && (commandBehavior == CommandBehavior.Default))
                {
                    commandBehavior = CommandBehavior.CloseConnection;
                }

                // Execute the reader
                return cmd.ExecuteReader(commandBehavior);
            }
            catch (Exception ex)
            {
                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }

                if (!HandleError(queryText, ex))
                {
                    throw;
                }

                return null;
            }
        }


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
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<object> ExecuteScalarAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;
            object result = null;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, requiresTransaction, ref closeConnection, ref commitTransaction);

                // Execute scalar
                result = await cmd.ExecuteScalarAsync(cancellationToken);

                // Commit transaction is necessary
                if (commitTransaction)
                {
                    CommitTransaction();
                    commitTransaction = false;
                }
            }
            catch (Exception ex)
            {
                if (!HandleError(queryText, ex))
                {
                    throw;
                }
            }
            finally
            {
                // Rollback transaction if necessary
                if (commitTransaction)
                {
                    RollbackTransaction();
                }

                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }

            return result;
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        public virtual object ExecuteScalar(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;
            bool commitTransaction = false;
            object result = null;

            try
            {
                // Prepare the command
                var cmd = PrepareCommand(queryText, queryParams, queryType, requiresTransaction, ref closeConnection, ref commitTransaction);

                // Execute scalar
                result = cmd.ExecuteScalar();

                // Commit transaction is necessary
                if (commitTransaction)
                {
                    CommitTransaction();
                    commitTransaction = false;
                }
            }
            catch (Exception ex)
            {
                if (!HandleError(queryText, ex))
                {
                    throw;
                }
            }
            finally
            {
                // Rollback transaction if necessary
                if (commitTransaction)
                {
                    RollbackTransaction();
                }

                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }

            return result;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        protected AbstractDataConnection(string connectionString)
        {
            if (!String.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            mOriginalThreadID = CMSThread.GetCurrentThreadId();
        }


        /// <summary>
        /// Checks the thread safety of current operation.
        /// </summary>
        protected void CheckThreadSafety()
        {
            if (ConnectionHelper.CheckThreadSafety && (mOriginalThreadID != CMSThread.GetCurrentThreadId()))
            {
                throw new Exception("Thread safety of the connection was violated. The connection must be used only in thread where it was opened.");
            }
        }


        /// <summary>
        /// Returns XML schema for specified table.
        /// </summary>
        /// <param name="tableName">Name of a table to get xml schema for</param>
        public virtual string GetXmlSchema(string tableName)
        {
            // Execute the command for getting the schema
            var cmd = CreateCommand("SELECT TOP 1 * FROM " + tableName);
            cmd.CommandTimeout = CommandTimeout;

            var da = CreateDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();

            // Get xml schema
            da.FillSchema(ds, SchemaType.Mapped, tableName);
            string xmlSchema = ds.GetXmlSchema();

            xmlSchema = !string.IsNullOrEmpty(xmlSchema) ? xmlSchema.Replace("utf-16", "utf-8") : "";

            return xmlSchema;
        }


        /// <summary>
        /// Returns true if the native connection exists.
        /// </summary>
        public virtual bool NativeDBConnectionExists()
        {
            return (mNativeConnection != null);
        }


        /// <summary>
        /// Returns true if the native connection exists.
        /// </summary>
        public virtual bool NativeConnectionExists()
        {
            if (!UseScopeConnection)
            {
                return NativeDBConnectionExists();
            }

            // Use connection from currently open scope
            IDataConnection scopeConnection = ConnectionContext.CurrentScopeConnection;
            if ((scopeConnection != null) && (scopeConnection != this))
            {
                return scopeConnection.NativeDBConnectionExists();
            }

            return NativeDBConnectionExists();
        }


        /// <summary>
        /// Returns true if connection to the database is open.
        /// </summary>
        public virtual bool IsOpen()
        {
            // Check if native connection exists
            if (!NativeConnectionExists())
            {
                return false;
            }

            return (NativeConnection.State == ConnectionState.Open);
        }


        /// <summary>
        /// Opens connection to the database.
        /// </summary>
        [HideFromDebugContext]
        public virtual void Open()
        {
            if (!IsOpen())
            {
                mOriginalThreadID = CMSThread.GetCurrentThreadId();

                // Open the connection
                NativeConnection.ConnectionString = ConnectionString;

                LogConnectionOperation("OpenConnection()", true, this);

                NativeConnection.Open();
            }
        }


        /// <summary>
        /// Closes connection to the database.
        /// </summary>
        [HideFromDebugContext]
        public virtual void Close()
        {
            if (!KeepOpen && IsOpen())
            {
                // Close nested connection first, in the opposite order than they were opened
                if (mNestedOpenConnections != null)
                {
                    for (int i = mNestedOpenConnections.Count - 1; i >= 0; i--)
                    {
                        mNestedOpenConnections[i].Close();
                    }
                }

                LogConnectionOperation("CloseConnection()", false, this);

                NativeConnection.Close();

                // Optionally dispose the connection
                if (ConnectionHelper.DisposeConnectionAfterClose)
                {
                    LogConnectionOperation("DisposeConnection()", false, this);

                    NativeConnection.Dispose();
                    NativeConnection = null;
                }
            }
        }


        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        [HideFromDebugContext]
        public virtual void BeginTransaction(IsolationLevel isolation)
        {
            if (!IsTransaction())
            {
                LogConnectionOperation("BeginTransaction(" + isolation + ")", true, this);

                Transaction = NativeConnection.BeginTransaction(isolation);
            }
        }


        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        [HideFromDebugContext]
        public virtual void BeginTransaction()
        {
            BeginTransaction(ConnectionHelper.TransactionIsolationLevel);
        }


        /// <summary>
        /// Commits current transaction.
        /// </summary>
        [HideFromDebugContext]
        public virtual void CommitTransaction()
        {
            IDbTransaction tr = Transaction;
            if (tr != null)
            {
                // Validate the transaction consistency
                ValidateTransaction(tr);

                // Close nested connection first, in the opposite order than they were opened
                if (mNestedOpenTransactions != null)
                {
                    for (int i = mNestedOpenTransactions.Count - 1; i >= 0; i--)
                    {
                        mNestedOpenTransactions[i].CommitTransaction();
                    }
                }

                LogConnectionOperation("CommitTransaction()", false, this);

                tr.Commit();
                Transaction = null;
            }
        }


        /// <summary>
        /// Rollbacks current transaction.
        /// </summary>
        [HideFromDebugContext]
        public virtual void RollbackTransaction()
        {
            IDbTransaction tr = Transaction;
            if (tr != null)
            {
                // Close nested connection first, in the opposite order than they were opened
                if (mNestedConnectionsList != null)
                {
                    for (int i = mNestedConnectionsList.Count - 1; i >= 0; i--)
                    {
                        mNestedConnectionsList[i].RollbackTransaction();
                    }
                }

                LogConnectionOperation("RollbackTransaction()", false, this);

                tr.Rollback();
                Transaction = null;
            }
        }


        /// <summary>
        /// Indicates if transaction is running.
        /// </summary>
        public virtual bool IsTransaction()
        {
            if (!UseScopeConnection)
            {
                return (LocalTransaction != null);
            }

            // Check the scope connection first
            var scopeConnection = AbstractDataProvider.CurrentScopeConnection;
            if (scopeConnection != null)
            {
                return (scopeConnection.LocalTransaction != null);
            }
            else
            {
                return (LocalTransaction != null);
            }
        }


        /// <summary>
        /// Disposes the connection object.
        /// </summary>
        public void Dispose()
        {
            // Dispose nested connection first, in the opposite order than they were opened
            if (mNestedConnectionsList != null)
            {
                for (int i = mNestedConnectionsList.Count - 1; i >= 0; i--)
                {
                    mNestedConnectionsList[i].Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Loads the default connection string
        /// </summary>
        protected void LoadDefaultConnectionString()
        {
            mConnectionString = ConnectionHelper.ConnectionString;
            mConnectionStringName = ConnectionHelper.ConnectionStringName;
        }


        /// <summary>
        /// Handles the error. Returns false, if the error was not handled
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="ex">Exception to handle</param>
        protected bool HandleError(string queryText, Exception ex)
        {
            var dbEx = ex as DbException;
            if (dbEx != null)
            {
                // Special treatment for SQL exception, include query
                string message = string.Format(
                    @"
[DataConnection.HandleError]: 

Query: 
{0}

Caused exception: 
{1}
"
                    , queryText, ex.Message);

                mLastError = message;

                // Handle 
                if (!HandleDbError(message, dbEx))
                {
                    throw new Exception(message, ex);
                }
            }

            return false;
        }


        /// <summary>
        /// Handles the database error. Returns false, if the error was not handled
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Database exception</param>
        protected virtual bool HandleDbError(string message, DbException ex)
        {
            return false;
        }


        /// <summary>
        /// Validates the transaction
        /// </summary>
        /// <param name="tr">Transaction to validate</param>
        protected void ValidateTransaction(IDbTransaction tr)
        {
            // Validate the transaction consistency
            if ((tr != null) && (tr.Connection == null) && (mLastError != null))
            {
                string message = "Transaction is no longer usable. Query execution failed during this transaction which is not permitted. Original error: " + mLastError;

                throw new Exception(message);
            }
        }

        #endregion


        #region "Command methods"

        /// <summary>
        /// Prepares the SQL command for the query.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Indicates it query is a SQL query or stored procedure</param>
        /// <param name="allowTransaction">Allow transaction for the command</param>
        /// <param name="closeConnection">Close connection</param>
        /// <param name="commitTransaction">Commit transaction</param>
        [HideFromDebugContext]
        protected virtual DbCommand PrepareCommand(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool allowTransaction, ref bool closeConnection, ref bool commitTransaction)
        {
            // Open the connection
            if (!IsOpen())
            {
                Open();
                closeConnection = true;
            }

            // Create new transaction with dirty read if necessary
            if (allowTransaction && (Transaction == null))
            {
                BeginTransaction(ConnectionHelper.TransactionIsolationLevel);
                commitTransaction = true;
            }

            // Prepare the command
            var cmd = CreateCommand(queryText);

            cmd.CommandType = (queryType == QueryTypeEnum.StoredProcedure) ? CommandType.StoredProcedure : CommandType.Text;
            cmd.CommandTimeout = CommandTimeout;

            // Add parameters
            AddParameters(cmd, queryParams);

            return cmd;
        }


        /// <summary>
        /// Adds the given parameters to the SQL command
        /// </summary>
        /// <param name="cmd">SQL command</param>
        /// <param name="queryParams">Parameters to add</param>
        protected void AddParameters(DbCommand cmd, IEnumerable<DataParameter> queryParams)
        {
            // Prepare the parameters
            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    if (!string.IsNullOrEmpty(param.Name))
                    {
                        // Add parameter
                        var par = CreateParameter(param);

                        cmd.Parameters.Add(par);
                    }
                }
            }
        }


        /// <summary>
        /// Creates a SQL parameter from the given data parameter
        /// </summary>
        /// <param name="param">Source parameter</param>
        protected DbParameter CreateParameter(DataParameter param)
        {
            var value = param.Value;

            var par = CreateParameter(param.Name, value);

            var type = param.Type;

            // Ensure type based on value
            if ((type == null) && (value != null))
            {
                type = value.GetType();
            }

            // Set the parameter type
            if (type != null)
            {
                SetParameterType(par, type);
            }

            return par;
        }


        /// <summary>
        /// Sets the parameter type to a destination type
        /// </summary>
        /// <param name="param">Parameter to set</param>
        /// <param name="type">Desired parameter type</param>
        protected virtual void SetParameterType(DbParameter param, Type type)
        {
            var sqlParam = param as SqlParameter;
            if (sqlParam != null)
            {
                // Get the type by data type
                DataType dataType = DataTypeManager.GetDataType(type);
                if ((dataType != null) && (dataType.DbType != null))
                {
                    sqlParam.SqlDbType = (SqlDbType)dataType.DbType;
                    if (!String.IsNullOrEmpty(dataType.TypeName) && (sqlParam.SqlDbType == SqlDbType.Structured))
                    {
                        sqlParam.TypeName = dataType.TypeName;
                    }
                }
            }
        }


        /// <summary>
        /// Creates a new native connection
        /// </summary>
        protected abstract IDbConnection CreateNativeConnection();


        /// <summary>
        /// Creates a new SQL command
        /// </summary>
        /// <param name="cmdText">Command text</param>
        protected abstract DbCommand CreateCommand(string cmdText);


        /// <summary>
        /// Creates a new command parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        protected abstract DbParameter CreateParameter(string name, object value);


        /// <summary>
        /// Creates a data adapter
        /// </summary>
        protected abstract DbDataAdapter CreateDataAdapter();

        #endregion


        #region "Nested connections"

        /// <summary>
        /// Gets the nested connection with the given connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="newConnection">If true, a new connection is created</param>
        public IDataConnection GetNestedConnection(string connectionString, bool newConnection = false)
        {
            IDataConnection result = null;

            if (mNestedConnections == null)
            {
                // Ensure the nested connection lists
                mNestedConnections = new SafeDictionary<string, IDataConnection>();
                mNestedConnectionsList = new List<IDataConnection>();
            }
            else if (!newConnection)
            {
                // Try to find existing
                result = mNestedConnections[connectionString];
            }

            // Ensure the nested connection
            if (result == null)
            {
                // Create new connection
                result = DataConnectionFactory.GetNativeConnection(connectionString, newConnection);
                result.CommandTimeout = CommandTimeout;
                result.UseScopeConnection = false;

                // Register within lists
                mNestedConnectionsList.Add(result);

                if (!newConnection)
                {
                    mNestedConnections[connectionString] = result;
                }
            }

            // Ensure the open state of the nested connection the same as current
            if (IsOpen())
            {
                result.Open();
                
                mNestedOpenConnections.Add(result);

                // Ensure the transaction state of the nested connection the same as current
                if (IsTransaction())
                {
                    result.BeginTransaction(Transaction.IsolationLevel);

                    mNestedOpenTransactions.Add(result);
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the connection that executes the given query
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <param name="newConnection">If true, a new connection instance is created</param>
        public IDataConnection GetExecutingConnection(string connectionStringName, bool newConnection = false)
        {
            IDataConnection useConn = this;

            if (newConnection || !String.IsNullOrEmpty(connectionStringName))
            {
                connectionStringName = ConnectionHelper.GetConnectionStringNameWithPrefix(connectionStringName);

                // Check if the connection strings match
                var connectionString = ConnectionHelper.GetConnectionString(connectionStringName, true);

                if (newConnection || ((connectionString != null) && (connectionString != ConnectionString)))
                {
                    // Get other executing connection if wanted connection is not the current one
                    useConn = GetNestedConnection(connectionString, newConnection);
                }
            }

            return useConn;
        }


        /// <summary>
        /// Logs the connection operation to the query log.
        /// </summary>
        /// <param name="operation">Connection operation</param>
        /// <param name="allowBeforeQuery">If true, the operation is allowed before the query when the query is open</param>
        /// <param name="conn">Connection around the operation</param>
        public void LogConnectionOperation(string operation, bool allowBeforeQuery, IDataConnection conn)
        {
            if (!DisableConnectionDebug)
            {
                SqlDebug.LogConnectionOperation(operation, allowBeforeQuery, conn);
            }
        }

        #endregion


        #region "IDataConnection Properties & Methods"

        // interface properties implemented by ConnectionString
        string IDataConnection.ConnectionString
        {
            get
            {
                return ConnectionString;
            }
        }


        // interface properties implemented by NativeConnection
        IDbConnection IDataConnection.NativeConnection
        {
            get
            {
                return NativeConnection;
            }
            set
            {
                NativeConnection = value;
            }
        }


        // interface methods implemented by ExecuteQuery
        [HideFromDebugContext]
        DataSet IDataConnection.ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            return ExecuteQuery(queryText, queryParams, queryType, false);
        }


        // interface methods implemented by ExecuteQuery
        DbDataReader IDataConnection.ExecuteReader(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior)
        {
            return ExecuteReader(queryText, queryParams, queryType, commandBehavior);
        }


        // interface methods implemented by IsOpen
        bool IDataConnection.IsOpen()
        {
            return IsOpen();
        }


        // interface methods implemented by Open
        [HideFromDebugContext]
        void IDataConnection.Open()
        {
            Open();
        }


        // interface methods implemented by Close
        [HideFromDebugContext]
        void IDataConnection.Close()
        {
            Close();
        }


        // interface methods implemented by BeginTransaction
        [HideFromDebugContext]
        void IDataConnection.BeginTransaction()
        {
            BeginTransaction();
        }


        // interface methods implemented by CommitTransaction
        [HideFromDebugContext]
        void IDataConnection.CommitTransaction()
        {
            CommitTransaction();
        }


        // interface methods implemented by RollbackTransaction
        [HideFromDebugContext]
        void IDataConnection.RollbackTransaction()
        {
            RollbackTransaction();
        }


        // interface methods implemented by IsTransaction
        bool IDataConnection.IsTransaction()
        {
            return IsTransaction();
        }

        #endregion
    }
}
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data connection interface that must be implemented by data providers.
    /// </summary>
    public interface IDataConnection : IDisposable
    {
        #region "Properties"

        /// <summary>
        /// Command timeout which will be set on this connection.
        /// </summary>
        /// <remarks>
        /// If a new <see cref="CMSConnectionScope"/> is created within this connection and a different <see cref="CMSConnectionScope.CommandTimeout"/> is set,
        /// the current connection timeout is not changed outside of the scope.
        /// </remarks>
        int CommandTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the connection uses the scope connection
        /// </summary>
        bool UseScopeConnection
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the connection stays open even if close is requested.
        /// </summary>
        bool KeepOpen
        {
            get;
            set;
        }


        /// <summary>
        /// Native connection object. It depends on provider type.
        /// </summary>
        IDbConnection NativeConnection
        {
            set;
            get;
        }


        /// <summary>
        /// Transaction object.
        /// </summary>
        IDbTransaction Transaction
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string for specific provider.
        /// </summary>
        string ConnectionStringName
        {
            get;
        }


        /// <summary>
        /// Connection string for specific provider.
        /// </summary>
        string ConnectionString
        {
            get;
        }


        /// <summary>
        /// If true, the debug is disabled on this connection
        /// </summary>
        bool DisableConnectionDebug
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the debug of queries is disabled on this connection
        /// </summary>
        bool DisableQueryDebug
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the connection that executes the given query
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <param name="newConnection">If true, a new connection instance is created</param>
        IDataConnection GetExecutingConnection(string connectionStringName, bool newConnection = false);


        /// <summary>
        /// Performs a bulk insert of the data into a target database table
        /// </summary>
        /// <param name="sourceData">Source data table</param>
        /// <param name="targetTable">Name of the target DB table</param>
        /// <param name="insertSettings">Bulk insert configuration</param>
        void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings = null);


        /// <summary>
        /// Returns result of the query.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run</param>
        /// <param name="queryParams">Array of query parameters</param>
        /// <param name="queryType">Indicates it query is a SQL query or stored procedure</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction</param>
        DataSet ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction);


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
        Task<int> ExecuteNonQueryAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken);


        /// <summary>
        /// Executes the query and returns the number of rows affected.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        int ExecuteNonQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction);


        /// <summary>
        /// <para>
        /// An asynchronous version of <see cref="ExecuteReader"/> which executes the query asynchronously and returns result as a <see cref="DbDataReader"/>.
        /// </para>
        /// <para>
        /// The cancellation token can be used to request that the operation be abandoned before the command timeout elapses.
        /// Exceptions will be reported via the returned Task object.
        /// </para>
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Indicates it query is a SQL query or stored procedure</param>
        /// <param name="commandBehavior">Command behavior</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<DbDataReader> ExecuteReaderAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior, CancellationToken cancellationToken);


        /// <summary>
        /// Executes the query and returns result as a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or stored procedure.</param>
        /// <param name="commandBehavior">Command behavior.</param>
        DbDataReader ExecuteReader(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, CommandBehavior commandBehavior);


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
        Task<object> ExecuteScalarAsync(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction, CancellationToken cancellationToken);


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="queryText">Query or stored procedure to be run.</param>
        /// <param name="queryParams">Query parameters.</param>
        /// <param name="queryType">Indicates if query is a SQL query or a stored procedure.</param>
        /// <param name="requiresTransaction">If true, the query should run within transaction.</param>
        object ExecuteScalar(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction);


        /// <summary>
        /// Returns XML schema for specified table.
        /// </summary>
        /// <param name="tableName">Name of a table to get xml schema for</param>
        string GetXmlSchema(string tableName);


        /// <summary>
        /// Returns true if the native connection exists.
        /// </summary>
        bool NativeDBConnectionExists();


        /// <summary>
        /// Returns true if the native connection exists.
        /// </summary>
        bool NativeConnectionExists();


        /// <summary>
        /// Returns true if connection to the database is open.
        /// </summary>
        bool IsOpen();


        /// <summary>
        /// Opens connection to the database.
        /// </summary>
        void Open();


        /// <summary>
        /// Closes connection to the database.
        /// </summary>
        void Close();


        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        void BeginTransaction();


        /// <summary>
        /// Begins a new transaction with specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Isolation level to use</param>
        void BeginTransaction(IsolationLevel isolationLevel);


        /// <summary>
        /// Commits current transaction.
        /// </summary>
        void CommitTransaction();


        /// <summary>
        /// Rollbacks current transaction.
        /// </summary>
        void RollbackTransaction();


        /// <summary>
        /// Indicates if transaction is running.
        /// </summary>
        bool IsTransaction();

        #endregion
    }
}
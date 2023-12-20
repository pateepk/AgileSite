using System;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Allows changing command timeout for a code block.
    /// Also allows specifying a connection that is used for executing all <see cref="ConnectionHelper"/> calls
    /// that are passed the query text directly within a code block.
    /// </summary>
    /// <remarks>
    /// To set the connection scope for the whole API (e.g. <see cref="DataQuery" />), use the <see cref="CMSConnectionContext"/> class.
    /// </remarks>
    public class CMSConnectionScope : Trackable<CMSConnectionScope>, INotCopyThreadItem
    {
        #region "Variables"

        private bool mRemoveScope;
        private bool mCloseConnection;

        private IDataConnection mConnection;
        private IDataConnection mOriginalConnection;
        private int? mOriginalCommandTimeout;

        #endregion


        #region "Properties"

        /// <summary>
        /// Connection string name that the thread should use to access the database
        /// </summary>
        public string ConnectionString
        {
            get;
            protected set;
        }


        /// <summary>
        /// Connection of the current scope.
        /// </summary>
        public IDataConnection Connection
        {
            get
            {
                return mConnection;
            }
        }


        /// <summary>
        /// Command timeout which will be set on the connection within the current connection scope.
        /// </summary>
        public int CommandTimeout
        {
            set
            {
                mOriginalCommandTimeout = Connection.CommandTimeout;
                Connection.CommandTimeout = value;
            }
        }


        /// <summary>
        /// If true, the debug is disabled on this connection
        /// </summary>
        public bool DisableConnectionDebug
        {
            get
            {
                return Connection.DisableConnectionDebug;
            }
            set
            {
                Connection.DisableConnectionDebug = value;
            }
        }


        /// <summary>
        /// If true, the debug of queries is disabled on this connection
        /// </summary>
        public bool DisableQueryDebug
        {
            get
            {
                return Connection.DisableQueryDebug;
            }
            set
            {
                Connection.DisableQueryDebug = value;
            }
        }


        /// <summary>
        /// If true, the debug of queries is disabled on this connection
        /// </summary>
        public bool DisableDebug
        {
            set
            {
                DisableQueryDebug = value;
                DisableConnectionDebug = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor. Creates a new connection and ensures that all database connections during the live of this object will use that connection.
        /// </summary>
        /// <param name="newConnection">If true, creates a new connection</param>
        public CMSConnectionScope(bool newConnection = false)
        {
            IDataConnection conn = null;
            string connString = null;

            if (newConnection)
            {
                // Get current connection
                var currentConnection = DataConnectionFactory.CurrentConnection;
                if (currentConnection != null)
                {
                    // Get new connection based on current
                    connString = currentConnection.ConnectionString;

                    conn = ConnectionHelper.GetConnection(connString);
                    conn.UseScopeConnection = false;
                }
            }

            Init(connString, conn);
        }


        /// <summary>
        /// Constructor. Ensures that all database connections during the live of this object will use the given connection.
        /// </summary>
        /// <param name="connectionString">Connection string to use within all underlying database operations. If null new connection is created</param>
        /// <param name="defaultIfNotFound">If true, the default connection string is used if the given connection string is not found</param>
        public CMSConnectionScope(string connectionString, bool defaultIfNotFound = false)
        {
            Init(connectionString, null, defaultIfNotFound);
        }


        /// <summary>
        /// Constructor. Ensures that all database connections during the live of this object will use the given connection.
        /// </summary>
        /// <param name="conn">Connection to use within all database operations. If null new connection is created</param>
        public CMSConnectionScope(IDataConnection conn)
        {
            Init(null, conn);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the context
        /// </summary>
        /// <param name="connectionStringName">Connection string or connection string name to use within all underlying database operations. If null, default connection is used.</param>
        /// <param name="conn">Connection to use</param>
        /// <param name="defaultIfNotFound">If true, the default connection string is used if the given connection string is not found</param>
        private void Init(string connectionStringName, IDataConnection conn, bool defaultIfNotFound = false)
        {
            ConnectionString = connectionStringName;

            string connectionString = connectionStringName;

            // Get the connection string by name if not set
            if (!String.IsNullOrEmpty(connectionString) && !connectionString.Contains(";"))
            {
                connectionStringName = ConnectionHelper.GetConnectionStringNameWithPrefix(connectionStringName);

                connectionString = ConnectionHelper.GetConnectionString(connectionStringName, defaultIfNotFound);
            }

            if (conn == null)
            {
                // Try to use current connection if explicit connection is not specified
                var currentConnection = DataConnectionFactory.CurrentConnection;
                if (currentConnection != null)
                {
                    mOriginalConnection = currentConnection;

                    // Keep current connection if no connection given, or connection strings match
                    if (String.IsNullOrEmpty(connectionString) || connectionString.Equals(currentConnection.ConnectionString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        mConnection = currentConnection;
                    }
                }
            }

            if (mConnection == null)
            {
                // Ensure the connection
                if (conn == null)
                {
                    conn = ConnectionHelper.GetConnection(connectionString);
                }

                // Take the data connection from the general connection
                var genConn = conn as GeneralConnection;

                DataConnectionFactory.CurrentConnection = (genConn != null) ? genConn.DataConnection : conn;

                // Set current connection scope
                mConnection = conn;
                mRemoveScope = true;
            }
        }


        /// <summary>
        /// Resets the connection to recover from error.
        /// </summary>
        public void ResetConnection()
        {
            // Create new connection
            var conn = ConnectionHelper.GetConnection();
            mConnection = conn;

            DataConnectionFactory.CurrentConnection = conn.DataConnection;

            // Open the connection
            if (mCloseConnection)
            {
                mConnection.Open();
            }
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public override void Dispose()
        {
            // Close the connection if needed
            if (mCloseConnection)
            {
                Close();
            }
            
            // Drop current connection scope
            if (mRemoveScope)
            {
                DataConnectionFactory.CurrentConnection = mOriginalConnection;
            }

            var currentConnection = DataConnectionFactory.CurrentConnection;

            // Restore original command timeout
            if ((currentConnection != null) && mOriginalCommandTimeout.HasValue)
            {
                currentConnection.CommandTimeout = mOriginalCommandTimeout.Value;
            }
            
            base.Dispose();
        }


        /// <summary>
        /// Opens the connection.
        /// </summary>
        public CMSConnectionScope Open()
        {
            if (!Connection.IsOpen())
            {
                Connection.Open();

                mCloseConnection = true;
            }

            return this;
        }


        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            if (Connection.IsOpen())
            {
                Connection.Close();

                mCloseConnection = false;
            }
        }

        #endregion
    }
}
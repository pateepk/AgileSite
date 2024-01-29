using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data provider class.
    /// </summary>
    public abstract class AbstractDataProvider : IDataProvider
    {
        #region "Properties"

        /// <summary>
        /// Connection string to use for the connections.
        /// </summary>
        public virtual string ConnectionString
        {
            get
            {
                return ConnectionHelper.ConnectionString;
            }
            set
            {
                ConnectionHelper.ConnectionString = value;
            }
        }


        /// <summary>
        /// Current DB connection to use within current connection scope.
        /// </summary>
        public virtual IDataConnection CurrentConnection
        {
            get
            {
                return ConnectionContext.CurrentScopeConnection;
            }
            set
            {
                ConnectionContext.CurrentScopeConnection = value;
            }
        }


        /// <summary>
        /// Current DB connection to use within current connection scope.
        /// </summary>
        public static AbstractDataConnection CurrentScopeConnection
        {
            get
            {
                return (AbstractDataConnection)ConnectionContext.CurrentScopeConnection;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns new data connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public virtual IDataConnection GetNewConnection(string connectionString)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sends the specific command with arguments to the provider.
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="commandArguments">Command arguments (parameters)</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public virtual object ProcessCommand(string commandName, object[] commandArguments)
        {
            return null;
        }

        #endregion
    }
}
using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data provider interface.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Returns new data connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        IDataConnection GetNewConnection(string connectionString);


        /// <summary>
        /// Sends the specific command with arguments to the provider.
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="commandArguments">Command arguments (parameters)</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        object ProcessCommand(string commandName, object[] commandArguments);


        /// <summary>
        /// Connection string.
        /// </summary>
        string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// Current DB connection to use within current connection scope.
        /// </summary>
        IDataConnection CurrentConnection
        {
            get;
            set;
        }
    }
}
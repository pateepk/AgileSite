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
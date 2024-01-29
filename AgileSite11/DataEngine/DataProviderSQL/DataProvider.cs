using CMS.DataEngine;

namespace CMS.DataProviderSQL
{
    /// <summary>
    /// Data provider class.
    /// </summary>
    public class DataProvider : AbstractDataProvider
    {
        /// <summary>
        /// Returns new data connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public override IDataConnection GetNewConnection(string connectionString)
        {
            return new DataConnection(connectionString);
        }
    }
}
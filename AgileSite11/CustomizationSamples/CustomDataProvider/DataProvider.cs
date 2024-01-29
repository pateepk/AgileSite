using System;

using CMS.DataEngine;

namespace CMS.CustomDataProvider
{
    /// <summary>
    /// Data provider class.
    /// </summary>
    public class DataProvider : AbstractDataProvider
    {
        #region "Methods"

        /// <summary>
        /// Returns new data connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public override IDataConnection GetNewConnection(string connectionString)
        {
            IDataConnection conn = new DataConnection(connectionString);

            return conn;
        }

        #endregion
    }
}
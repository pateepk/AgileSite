using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// ConnectionStrings service
    /// </summary>
    public class ConnectionStringService : IConnectionStringService
    {
        /// <summary>
        /// Default connection string
        /// </summary>
        public string DefaultConnectionString
        {
            get
            {
                return this["CMSConnectionString"];
            }
        }


        /// <summary>
        /// Gets the specific connection string from the app config
        /// </summary>
        public string this[string name]
        {
            get 
            {
                // Get connection string
                var connString = SettingsHelper.ConnectionStrings[name];
                if (connString == null)
                {
                    return null;
                }

                return connString.ConnectionString;    
            }
        }
    }
}

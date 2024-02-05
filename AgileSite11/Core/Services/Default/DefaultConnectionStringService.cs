using System.Configuration;

namespace CMS.Core
{
    /// <summary>
    /// Default service to provide app settings
    /// </summary>
    internal class DefaultConnectionStringService : IConnectionStringService
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
        /// Returns null
        /// </summary>
        /// <param name="name">Connection string name</param>
        public string this[string name]
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings[name];
                if (cs != null)
                {
                    return cs.ConnectionString;
                }

                return null;
            }
        }
    }
}

using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Default container for registering and retrieving module usage data sources.
    /// </summary>
    /// <remarks>
    /// This is default empty implementation which doesn't return any data source. 
    /// Module usage tracking module registers its own implementation of the interface when installed.
    /// </remarks>
    public class DefaultModuleUsageDataSourceContainer : IModuleUsageDataSourceContainer
    {
        /// <summary>
        /// Register module usage data source.
        /// </summary>
        /// <param name="registerAttribute">Attribute registering data source object for collecting data.</param>
        public void RegisterDataSource(RegisterModuleUsageDataSourceAttribute registerAttribute)
        {
        }


        /// <summary>
        /// Get all registered module usage data sources.
        /// </summary>
        public IEnumerable<IModuleUsageDataSource> GetDataSources()
        {
            yield break;
        }
    }
}

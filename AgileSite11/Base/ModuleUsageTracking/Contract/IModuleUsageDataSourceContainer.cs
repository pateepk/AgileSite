using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Container for registering and retrieving module usage data sources.
    /// </summary>
    public interface IModuleUsageDataSourceContainer
    {
        /// <summary>
        /// Register module usage data source.
        /// </summary>
        /// <param name="registerAttribute">Attribute registering data source object for collecting data.</param>
        void RegisterDataSource(RegisterModuleUsageDataSourceAttribute registerAttribute);


        /// <summary>
        /// Get all registered module usage data sources.
        /// </summary>
        IEnumerable<IModuleUsageDataSource> GetDataSources();
    }
}

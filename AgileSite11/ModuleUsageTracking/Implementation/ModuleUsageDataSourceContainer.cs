using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Default container for registering and retrieving module usage data sources.
    /// </summary>
    internal class ModuleUsageDataSourceContainer : IModuleUsageDataSourceContainer
    {
        private readonly ConcurrentBag<Type> mDataSourceTypes = new ConcurrentBag<Type>();


        /// <summary>
        /// Register module usage data source.
        /// </summary>
        /// <param name="registerAttribute">Attribute for registering data source object for collecting data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when register attribute is null</exception>
        public void RegisterDataSource(RegisterModuleUsageDataSourceAttribute registerAttribute)
        {
            if (registerAttribute == null)
            {
                throw new ArgumentNullException("registerAttribute");
            }

            mDataSourceTypes.Add(registerAttribute.MarkedType);
        }


        /// <summary>
        /// Get all registered module usage data sources.
        /// </summary>
        public IEnumerable<IModuleUsageDataSource> GetDataSources()
        {
            return mDataSourceTypes.Select(type => (IModuleUsageDataSource)ObjectFactory.New(type));
        }
    }
}

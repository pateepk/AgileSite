using CMS.Base;
using CMS.Core;
using CMS.ModuleUsageTracking;

[assembly: RegisterModuleUsageDataSource(typeof(CountersDataSource))]

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Class for retrieving module usage counters.
    /// </summary>
    internal class CountersDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Name of the global data source.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Counters";
            }
        }


        /// <summary>
        /// Get the module usage counters' data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            foreach (var counter in ModuleUsageCounterInfoProvider.GetModuleUsageCounters())
            {
                result.Add(counter.ModuleUsageCounterName, counter.ModuleUsageCounterValue);
            }

            return result;
        }
    }
}

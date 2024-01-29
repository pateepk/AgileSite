using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine.Query;
using CMS.WebFarmSync;

[assembly: RegisterModuleUsageDataSource(typeof(WebFarmSyncUsageDataSource))]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Module usage data for web farms.
    /// </summary>
    public class WebFarmSyncUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Web farms usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.WebFarmSync";
            }
        }


        /// <summary>
        /// Get web farms usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Get number of all registered web farm servers
            result.Add("WebFarmServers", WebFarmServerInfoProvider.GetWebFarmServers().Count);

            // Get number of all registered web farm servers which are currently enabled
            result.Add("WebFarmServersEnabled", WebFarmContext.EnabledServers.Count);

            // Get number of all unprocessed tasks
            result.Add("WebFarmTasksUnprocessed", WebFarmServerTaskInfoProvider.GetWebFarmServerTasks().WhereEmpty("ErrorMessage").GetCount());

            // Get number of all unprocessed tasks because server is disabled
            result.Add("WebFarmTasksUnprocessedServerInactive", WebFarmServerTaskInfoProvider.GetWebFarmServerTasksInternal()
                                                                                        .Columns("CMS_WebFarmServerTask.ServerID", "TaskID")
                                                                                        .Source(s => s.InnerJoin<WebFarmServerInfo>("ServerID", "ServerID"))
                                                                                        .WhereFalse("ServerEnabled")
                                                                                        .OrderBy("CMS_WebFarmServerTask.ServerID", "TaskID")
                                                                                        .Count);

            // Get number of all failed tasks
            result.Add("WebFarmTasksFailed", WebFarmServerTaskInfoProvider.GetWebFarmServerTasks().WhereNotEmpty("ErrorMessage").GetCount());

            var allServers = WebFarmServerInfoProvider.GetWebFarmServers();

            // Get count of healthy web farm servers
            result.Add("WebFarmHealthyServerCount", allServers.Count(s => s.Status == WebFarmServerStatusEnum.Healthy));

            // Get count of transitioning web farm servers
            result.Add("WebFarmTransitioningServerCount", allServers.Count(s => s.Status == WebFarmServerStatusEnum.Transitioning));

            // Get count of not responding web farm servers
            result.Add("WebFarmNotRespondingServerCount", allServers.Count(s => s.Status == WebFarmServerStatusEnum.NotResponding));

            // Get count of not responding web farm servers
            result.Add("WebFarmAutoDisabledServerCount", allServers.Count(s => s.Status == WebFarmServerStatusEnum.AutoDisabled));

            // Get web farm synchronization interval
            result.Add("WebFarmSynchronizationInterval", WebFarmContext.SyncInterval);
            
            // Get web farm support is enabled
            result.Add("WebFarmSupportIsEnabled", WebFarmContext.WebFarmEnabled);

            return result;
        }
    }
}

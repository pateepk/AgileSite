using CMS.Core;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Web farm task used to clear enabled status of health monitoring.
    /// </summary>
    internal class ClearHealthMonitoringEnabledStatusWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Clears the enabled status of the health monitoring so it can reload.
        /// </summary>
        public override void ExecuteTask()
        {
            HealthMonitoringHelper.Clear(false);
        }
    }
}

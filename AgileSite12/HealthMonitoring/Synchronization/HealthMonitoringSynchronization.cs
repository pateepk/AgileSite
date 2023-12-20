using CMS.Helpers;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Synchronization helper class for the health monitoring module.
    /// </summary>
    internal static class HealthMonitoringSynchronization
    {
        /// <summary>
        /// Initializes the tasks for clearing enabled status of health monitoring synchronization.
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<ClearHealthMonitoringEnabledStatusWebFarmTask>();
        }
    }
}

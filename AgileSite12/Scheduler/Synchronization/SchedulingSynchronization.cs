using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Synchronization helper class for the scheduler module.
    /// </summary>
    internal static class SchedulingSynchronization
    {
        /// <summary>
        /// Initializes the tasks for clearing enabled status of scheduling synchronization.
        /// </summary>
        public static void Init()
        {
            // Web farm synchronization
            WebFarmHelper.RegisterTask<ClearSchedulingEnabledStatusWebFarmTask>();
        }
    }
}

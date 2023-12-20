using CMS.Core;

namespace CMS.Scheduler
{
    /// <summary>
    /// Web farm task used to clear enabled status of scheduler.
    /// </summary>
    internal class ClearSchedulingEnabledStatusWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Clears the enabled status of the scheduler so it can reload.
        /// </summary>
        public override void ExecuteTask()
        {
            SchedulingHelper.Clear(false);
        }
    }
}

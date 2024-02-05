using CMS.Core;

namespace CMS.Scheduler
{
    /// <summary>
    /// Represents the Scheduler module metadata.
    /// </summary>
    public class SchedulerModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SchedulerModuleMetadata()
            : base(ModuleName.SCHEDULER)
        {
            RootPath = "~/CMSModules/Scheduler/";
        }
    }
}
using CMS;
using CMS.DataEngine;
using CMS.Scheduler;

[assembly: RegisterModule(typeof(SchedulerModule))]

namespace CMS.Scheduler
{
    /// <summary>
    /// Represents the Scheduler module.
    /// </summary>
    public class SchedulerModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SchedulerModule()
            : base(new SchedulerModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ImportSpecialActions.Init();
            TaskExport.Init();
            SchedulerHandlers.Init();

            SchedulerCounters.RegisterPerformanceCounters();
        }

        #endregion
    }
}
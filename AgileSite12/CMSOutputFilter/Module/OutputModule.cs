using System;

using CMS;
using CMS.DataEngine;
using CMS.Base;
using CMS.OutputFilter;
using CMS.CMSImportExport;
using CMS.Scheduler;

[assembly: RegisterModule(typeof(OutputModule))]

namespace CMS.OutputFilter
{
    /// <summary>
    /// Represents the Output filter module.
    /// </summary>
    public class OutputModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OutputModule()
            : base(new OutputModuleMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            if (!SystemContext.IsCMSRunningAsMainApplication)
            {
                return;
            }

            DebugHelper.RegisterDebug(OutputDebug.Settings);
        }


        /// <summary>
        /// Module init
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            if (!SystemContext.IsCMSRunningAsMainApplication)
            {
                return;
            }

            // Special case for output file system cache move
            SpecialActionsEvents.ProcessMainObject.Before += (s, e) =>
            {
                var scheduledTask = e.Object as TaskInfo;
                if (scheduledTask != null &&
                    e.Settings.IsLowerVersion("12.0") &&
                    scheduledTask.TaskAssemblyName.Equals("CMS.Scheduler", StringComparison.OrdinalIgnoreCase) &&
                    scheduledTask.TaskClass.Equals("CMS.Scheduler.FileSystemCacheCleaner", StringComparison.OrdinalIgnoreCase))
                {
                    scheduledTask.TaskAssemblyName = "CMS.OutputFilter";
                    scheduledTask.TaskClass = "CMS.OutputFilter.FileSystemCacheCleaner";
                }

            };

            OutputFilterHandlers.Init();
        }

        #endregion
    }
}

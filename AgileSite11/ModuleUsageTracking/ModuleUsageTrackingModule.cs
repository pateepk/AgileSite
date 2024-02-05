using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ModuleUsageTracking;
using CMS.Scheduler;

[assembly: RegisterModule(typeof(ModuleUsageTrackingModule))]

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Represents the Module usage tracking module.
    /// </summary>
    internal class ModuleUsageTrackingModule : Module
    {
        #region "Constants"

        internal const string MODULE_USAGE_EVENTS_SOURCE = "Module usage tracking";

        private const string MODULE_USAGE_TRACKING_MODULE = "CMS.ModuleUsageTracking";
        private const string IDENTITY_KEY = "CMSModuleUsageTrackingIdentity";
        private const string DISPATCHER_NAME = "ModuleUsageTrackingTask";
        private const int DISPATCHER_TASK_START_HOUR = 3;

        #endregion


        #region "Properties"

        /// <summary>
        /// Kentico installation identity
        /// </summary>
        internal static Guid Identity
        {
            get;
            set;
        }


        /// <summary>
        /// The module usage module has all prerequisites to process and send the data.
        /// </summary>
        internal static new bool Initialized
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ModuleUsageTrackingModule()
            : base(MODULE_USAGE_TRACKING_MODULE)
        {
        }


        protected override void OnPreInit()
        {
            ObjectFactory<IModuleUsageCounter>.SetObjectTypeTo<ModuleUsageCounter>(true);
            ObjectFactory<IModuleUsageDataSourceContainer>.SetObjectTypeTo<ModuleUsageDataSourceContainer>();
            ObjectFactory<IModuleUsageDataCollection>.SetObjectTypeTo<ModuleUsageDataCollection>();
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                // Move initialization to post start to minimize application start time
                ApplicationEvents.PostStart.Execute += (sender, arguments) =>
                {
                    // Disable logging on production environment. This module should not bother customers with logs.
                    using (new CMSActionContext { LogEvents = SystemContext.DevelopmentMode })
                    {
                        // Initialize the scheduled task execution time and the identity
                        Initialized = EnsureInitialization();
                    }
                };
            }
        }


        /// <summary>
        /// Ensures identity guid of this Kentico installation and schedule time of module usage dispatcher task.
        /// </summary>
        /// <returns>True on success</returns>
        private static bool EnsureInitialization()
        {
            // Check if identity is set
            var identity = SettingsKeyInfoProvider.GetSettingsKeyInfo(IDENTITY_KEY);

            if (identity == null)
            {
                CoreServices.EventLog.LogEvent("E", MODULE_USAGE_EVENTS_SOURCE, "INITIALIZE", "SettingKeyInfo representing the identity in module usage tracking was not found.");
                return false;
            }

            bool result = true;
            if (string.IsNullOrEmpty(identity.KeyValue))
            {
                // Generate the scheduled task start time the first time module initializes.
                result = InitializeDispatcher();
                if (result)
                {
                    // Create the identity
                    identity.KeyValue = Guid.NewGuid().ToString();
                    SettingsKeyInfoProvider.SetSettingsKeyInfo(identity);
                }
            }

            Identity = ValidationHelper.GetGuid(identity.KeyValue, Guid.Empty);
            return result;
        }


        /// <summary>
        /// Initializes schedule time of module usage data dispatcher task.
        /// </summary>
        /// <returns>True on success</returns>
        private static bool InitializeDispatcher()
        {
            var dispatcher = TaskInfoProvider.GetTaskInfo(DISPATCHER_NAME, null);

            if (dispatcher == null)
            {
                CoreServices.EventLog.LogEvent("E", MODULE_USAGE_EVENTS_SOURCE, "INITIALIZE", "Scheduled task for sending module usage data was not found.");
                return false;
            }

            // Set task interval to once a week somewhere between 3AM and 4AM.
            var newInterval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_WEEK,
                StartTime = DateTime.Today.AddHours(DISPATCHER_TASK_START_HOUR).AddMinutes(new Random().Next(0, 60)),
                Every = 1,
            };

            dispatcher.TaskInterval = SchedulingHelper.EncodeInterval(newInterval);
            TaskInfoProvider.SetTaskInfo(dispatcher);

            return true;
        }

        #endregion
    }
}

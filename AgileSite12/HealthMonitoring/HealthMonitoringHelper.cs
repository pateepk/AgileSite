using System;
using System.IO;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// The class that provides to helping methods to work with performance counters.
    /// </summary>
    public class HealthMonitoringHelper
    {
        #region "Constants

        /// <summary>
        /// Path of directory CMSModules.
        /// </summary>
        public const string SYSTEM_COUNTERS_DIRECTORY_PATH = @"App_Data\CMSModules";

        #endregion


        #region "Variables"

        private static string mCountersStartDirectoryPath = null;

#pragma warning disable BH1014 // Do not use System.IO
        private static FileSystemWatcher mWatcher = null;
#pragma warning restore BH1014 // Do not use System.IO

        private static bool mCanLog = true;
        internal static bool? mHealthMonitoringEnabled = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if Health monitoring is enabled.
        /// </summary>
        public static bool HealthMonitoringEnabled
        {
            get
            {
                if (mHealthMonitoringEnabled == null)
                {
                    mHealthMonitoringEnabled = SettingsKeyInfoProvider.GetBoolValue("CMSEnableHealthMonitoring");
                }

                return mHealthMonitoringEnabled.Value;
            }
        }


        /// <summary>
        /// Indicates if the counters should be logged.
        /// </summary>
        public static bool LogCounters
        {
            get
            {
                // Counter should be logged and Health monitoring is enabled
                return (HealthMonitoringEnabled && !HealthMonitoringManager.Error && CanLog);
            }
        }


        /// <summary>
        /// Indicates if Health monitoring is enabled for site level.
        /// </summary>
        public static bool SiteCountersEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSEnableSiteCounters");
            }
        }


        /// <summary>
        /// Web application logging interval (in seconds).
        /// </summary>
        public static int ApplicationMonitoringInterval
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue("CMSApplicationHealthMonitoringInterval");
            }
        }


        /// <summary>
        /// Windows service logging interval (in seconds).
        /// </summary>
        public static int ServiceMonitoringInterval
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue("CMSServiceHealthMonitoringInterval");
            }
        }


        /// <summary>
        /// Indicates if the windows service should be log to the counters.
        /// </summary>
        /// <returns>TRUE if the windows service should be log to the counters.</returns>
        public static bool UseExternalService
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseExternalService") || SystemHelper.WinServicesForceUsage;
            }
        }


        /// <summary>
        /// Gets or sets start physical path of directory that contains  subdirectories with counter files.
        /// </summary>
        public static string CountersStartDirectoryPath
        {
            get
            {
                if (mCountersStartDirectoryPath == null)
                {
                    mCountersStartDirectoryPath = IO.Path.Combine(SystemContext.WebApplicationPhysicalPath, SYSTEM_COUNTERS_DIRECTORY_PATH).TrimEnd('\\');
                }

                return mCountersStartDirectoryPath;
            }
            set
            {
                mCountersStartDirectoryPath = value;
            }
        }


        /// <summary>
        /// Indicates if web application can log to the counters.
        /// </summary>
        public static bool CanLog
        {
            get
            {
                InitializeWatcher();
                return mCanLog;
            }
            set
            {
                mCanLog = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears the enabled status of the health monitoring so it can reload
        /// </summary>
        /// <param name="logTask">Indicates whether web farm task should be logged.</param>
        public static void Clear(bool logTask)
        {
            mHealthMonitoringEnabled = null;

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearHealthMonitoringEnabledStatusWebFarmTask());
            }
        }


        /// <summary>
        /// Gets category name.
        /// </summary>
        /// <param name="instancePath">Instance path - value of key CMSHealthMonitoringInstancePath</param>
        /// <param name="type">Category type</param>
        public static string GetCategoryName(string instancePath, CategoryType type)
        {
            if (!String.IsNullOrEmpty(instancePath))
            {
                string formatString = null;
                string[] splits = instancePath.Split(new char[] { '/' }, 2);
                // Path and web site
                if (splits.Length == 2)
                {
                    formatString = splits[1].Trim('/') + "/" + splits[0];
                }
                // Only 'web site'
                else
                {
                    formatString = splits[0];
                }

                if (type == CategoryType.General)
                {
                    return String.Format("Kentico - General ({0})", formatString);
                }
                else if (type == CategoryType.Sites)
                {
                    return String.Format("Kentico - Sites ({0})", formatString);
                }
            }

            return null;
        }


        /// <summary>
        /// Initializes watcher to identify that windows service regenerates the counter categories.
        /// </summary>
        private static void InitializeWatcher()
        {
            if (mWatcher == null)
            {
#pragma warning disable BH1014 // Do not use System.IO
                mWatcher = new FileSystemWatcher(Path.Combine(CountersStartDirectoryPath, "HealthMonitoring"), "Counters.tmp");
#pragma warning restore BH1014 // Do not use System.IO
                mWatcher.EnableRaisingEvents = true;
                mWatcher.Created += mWatcher_Created;
                mWatcher.Deleted += mWatcher_Deleted;
            }
        }


        /// <summary>
        /// Handles OnDeleted event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">File system event argument</param>
        private static void mWatcher_Deleted(object sender, EventArgs e)
        {
            HealthMonitoringManager.ClearCounters();
            mCanLog = true;
        }


        /// <summary>
        /// Handles OnCreated event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">File system event argument</param>
        private static void mWatcher_Created(object sender, EventArgs e)
        {
            mCanLog = false;
        }


        /// <summary>
        /// Ensures performance counter timer.
        /// </summary>
        public static void EnsurePerformanceCounterTimer()
        {
            // Health monitoring is enabled
            if (LogCounters)
            {
                // Get passive timer and execute
                PerformanceCounterTimer timer = PerformanceCounterTimer.EnsureTimer();
                timer.EnsureRunTimerAsync();
            }
        }

        #endregion
    }
}
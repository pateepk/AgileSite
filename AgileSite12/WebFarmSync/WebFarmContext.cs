using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Context of web farms module.
    /// </summary>
    public class WebFarmContext
    {
        #region "Variables"

        private List<WebFarmServerInfo> mServersToUpdate;
        private List<WebFarmServerInfo> mEnabledServers;
        private bool? mWebFarmEnabled;
        private int mServerId;
        private long mMaxWebFarmFileSize = long.MaxValue;
        private static readonly CMSStatic<WebFarmContext> mCurrent = new CMSStatic<WebFarmContext>();
        private static WebFarmModeEnum? mWebFarmModeFromConfig;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether web farm for current server is enabled.
        /// Set value overrides only web config enable setting.
        /// </summary>
        public static bool WebFarmEnabled
        {
            get
            {
                // Hidden web farm server is always enabled
                if (InstanceIsHiddenWebFarmServer)
                {
                    return true;
                }

                var current = Current;

                if (!current.mWebFarmEnabled.HasValue)
                {
                    // If application runs on azure, or is enabled from settings and if server name is set and the web farm server is enabled
                    current.mWebFarmEnabled = !String.IsNullOrEmpty(ServerName) &&
                                              ((WebFarmMode == WebFarmModeEnum.Automatic) ||
                                               ((WebFarmMode == WebFarmModeEnum.Manual) && EnabledServers.Any(s => s.ServerName.EqualsCSafe(ServerName, true))));

                }

                return current.mWebFarmEnabled.Value;
            }
            set
            {
                Current.mWebFarmEnabled = value;
            }
        }


        /// <summary>
        /// Indicates that the instance is hidden part of the web farm.
        /// </summary>
        /// <remarks>This means that it produces web farm tasks but other web farms do not see this instance and do not generate web farm tasks for it.</remarks>
        public static bool InstanceIsHiddenWebFarmServer
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if synchronization tasks should be logged and processed for external applications.
        /// </summary>
        public static bool UseTasksForExternalApplication
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the collection of enabled servers.
        /// </summary>
        public static List<WebFarmServerInfo> EnabledServers
        {
            get
            {
                var current = Current;
                return current.mEnabledServers ?? (current.mEnabledServers = WebFarmServerInfoProvider.GetAllEnabledServers().ToList());
            }
            internal set
            {
                Current.mEnabledServers = value;
            }
        }


        /// <summary>
        /// Indicates if web farm functionality is enabled and set to be configured manually or automatically in application settings.
        /// </summary>
        public static WebFarmModeEnum WebFarmMode
        {
            get
            {
                // Force mode has highest priority
                if (ForceAutomaticWebFarmMode)
                {
                    return WebFarmModeEnum.Automatic;
                }

                // Try to get mode from application configuration
                if (!mWebFarmModeFromConfig.HasValue)
                {
                    WebFarmModeEnum mode;
                    if (Enum.TryParse(ValidationHelper.GetString(SettingsHelper.AppSettings["CMSWebFarmMode"], null), true, out mode))
                    {
                        mWebFarmModeFromConfig = mode;
                    }
                }

                return mWebFarmModeFromConfig.HasValue ? mWebFarmModeFromConfig.Value : (WebFarmModeEnum)SettingsKeyInfoProvider.GetIntValue("CMSWebFarmMode");
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Current cached container.
        /// </summary>
        internal static WebFarmContext Current
        {
            get
            {
                return mCurrent.Value ?? (mCurrent.Value = new WebFarmContext());
            }
            set
            {
                mCurrent.Value = value;
            }
        }


        /// <summary>
        /// Whether the system should be creating anonymous tasks.
        /// </summary>
        internal static bool CreateAnonymousTasks
        {
            get
            {
                return !WebFarmEnabled && !SystemContext.IsWebSite && UseTasksForExternalApplication;
            }
        }


        /// <summary>
        /// Whether the system should be creating web farm tasks for other servers.
        /// </summary>
        internal static bool CreateWebFarmTasks
        {
            get
            {
                using (new CMSActionContext { CheckLicense = false })
                {
                    return WebFarmEnabled && ServersToUpdate.Any();
                }
            }
        }


        /// <summary>
        /// Application setups for which web farm automatic mode should be forced.
        /// </summary>
        private static bool ForceAutomaticWebFarmMode
        {
            get
            {
                return SystemContext.IsRunningOnAzure || ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWWAGInstallation"], false);
            }
        }


        /// <summary>
        /// Gets the maximal file size which is allowed for web farm synchronization.
        /// </summary>
        internal static long MaxWebFarmFileSize
        {
            get
            {
                var current = Current;

                if (current.mMaxWebFarmFileSize == long.MaxValue)
                {
                    var settingsValue = SettingsKeyInfoProvider.GetIntValue("CMSWebFarmMaxFileSize", "CMSWebFarmMaxFileSize", Int32.MaxValue);
                    current.mMaxWebFarmFileSize = Convert.ToInt64(settingsValue) * 1024;
                }
                return current.mMaxWebFarmFileSize;
            }
            set
            {
                Current.mMaxWebFarmFileSize = value;
            }
        }


        /// <summary>
        /// Gets the interval in milliseconds for the workers.
        /// </summary>
        internal static int SyncInterval
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue("CMSWebFarmSyncInterval", "CMSWebFarmSyncInterval", 1000);
            }
        }


        /// <summary>
        /// Gets the collection of servers to be updated if needed. Current server is excluded.
        /// </summary>
        internal static List<WebFarmServerInfo> ServersToUpdate
        {
            get
            {
                var current = Current;
                return current.mServersToUpdate ?? (current.mServersToUpdate = WebFarmServerInfoProvider.GetWebFarmServers().ToList().Where(s => s.ServerName.ToLowerCSafe() != ServerName.ToLowerCSafe()).ToList());
            }
        }


        /// <summary>
        /// Server name.
        /// </summary>
        internal static string ServerName
        {
            get
            {
                if(String.IsNullOrEmpty(SystemContext.ServerName))
                {
                    SystemContext.ServerName = WebFarmServerInfoProvider.GetAutomaticServerName();
                }
                return SystemContext.ServerName;
            }
            set
            {
                SystemContext.ServerName = value;
                Clear(false);
            }
        }


        /// <summary>
        /// Server ID.
        /// </summary>
        internal static int ServerId
        {
            get
            {
                var current = Current;
                if (current.mServerId == 0)
                {
                    WebFarmServerInfo wfsi = WebFarmServerInfoProvider.GetWebFarmServerInfo(ServerName);
                    if (wfsi != null)
                    {
                        current.mServerId = wfsi.ServerID;
                    }
                }

                return current.mServerId;
            }
            set
            {
                Current.mServerId = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Clears the hashtables of web farm servers.
        /// </summary>
        /// <param name="logWebFarm">Enables or disables webfarm task logging, if false no task is logged</param>
        public static void Clear(bool logWebFarm = true)
        {
            // Clear hash tables
            ProviderHelper.ClearHashtables(WebFarmServerInfo.OBJECT_TYPE, false);
            Current = null;

            // Create webfarm task if needed
            if (logWebFarm)
            {
                WebFarmTaskCreator.CreateTask(new ClearWebFarmServerListWebFarmTask());
            }
        }


        /// <summary>
        /// Function for getting current datetime for purposes of this class.
        /// </summary>
        internal static Func<DateTime> GetDateTime = () => DateTime.Now;

        #endregion
    }
}
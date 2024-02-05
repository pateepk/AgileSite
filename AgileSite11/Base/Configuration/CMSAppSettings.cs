using System;
using System.Configuration;

namespace CMS.Base
{
    /// <summary>
    /// App settings.
    /// </summary>
    public class CMSAppSettings
    {
        #region "Variables"

        /// <summary>
        /// Lock object used for singleton
        /// </summary>
        internal readonly static object SingletonLock = new object();

        // Locker object for explicit settings
        private readonly static object explicitSettingsLocker = new object();

        // Explicitly set settings
        private SafeDictionary<string, string> mExplicitSettings;

        // Settings dictionary.
        private SafeDictionary<string, string> mSettings = new SafeDictionary<string, string>()
        {
            AllowNulls = true
        };

        #endregion


        #region "Events"

        /// <summary>
        /// Delegate for event for reading application's settings.
        /// </summary>
        public delegate string GetAppSettingsEventHandler(string key);


        /// <summary>
        /// Event which is trown when Azure application wants to read application settings.
        /// </summary>
        public static event GetAppSettingsEventHandler GetApplicationSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Settings dictionary.
        /// </summary>
        private SafeDictionary<string, string> Settings
        {
            get
            {
                return mSettings;
            }
        }


        /// <summary>
        /// Settings key indexer.
        /// </summary>
        /// <param name="key">Settings key</param>
        public string this[string key]
        {
            get
            {
                // Try to get by the given key
                string result;

                if (!Settings.TryGetValue(key, out result))
                {
                    // Not found as cached, load the settings value
                    if (!SettingsHelper.AllowLocalConfigKeys)
                    {
                        // Local keys not allowed
                        result = GetFromSettings(key);
                    }
                    else
                    {
                        // Get with trial of localized values
                        result = GetFromSettingsWithLocalizedValues(key);
                    }

                    Settings[key] = result;
                }

                return result;
            }
            set
            {
                EnsureExplicitSettings();

                mExplicitSettings[key] = value;

                Settings.Clear();
            }
        }


        /// <summary>
        /// Gets the key from settings with support for localized values. Values can be localized based on machine name and virtual directory (application path)
        /// Machine name can be localized based on application pool name
        /// 
        /// Examples of .config key names:
        /// 
        /// [AppPool 1]CMSMachineName
        /// 
        /// [Machine1]/VirtualDirectory1:CMSWebFarmEnabled
        /// [Machine1]CMSWebFarmEnabled
        /// /VirtualDirectory2:CMSWebFarmEnabled
        /// CMSWebFarmEnabled
        /// </summary>
        /// <param name="key"></param>
        private string GetFromSettingsWithLocalizedValues(string key)
        {
            // Try also local settings
            string appPath = SystemContext.ApplicationPath;

            // Exclude machine name key to prevent infinite loop
            var machineNameKey = key.Equals("CMSMachineName", StringComparison.InvariantCultureIgnoreCase);

            // Use application pool name for machine name key, for other keys use machine name. Example values
            //
            // <add key="[AppPool 1]CMSMachineName" value="Machine1" />
            //
            // <add key="[Machine1]/VirtualDirectory1:CMSWebFarmEnabled" value="true" />
            // <add key="[Machine1]CMSWebFarmEnabled" value="true" />
            // <add key="/VirtualDirectory2:CMSWebFarmEnabled" value="true" />
            // <add key="CMSWebFarmEnabled" value="true" />
            
            var serverName = machineNameKey ? SystemContext.IISWebSiteName : SystemContext.MachineName;
            var keys = GetKeys(key, serverName, appPath);

            // Get from AppSettings and cache
            string result = GetFromSettings(keys);

            return result;
        }


        private static string[] GetKeys(string key, string serverName, string appPath)
        {
            // Get by the all possible specific keys
            string[] keys =
            {
                String.Format("[{0}]{1}:{2}", serverName, appPath, key),
                String.Format("[{0}]{1}", serverName, key),
                String.Format("{0}:{1}", appPath, key),
                key
            };

            return keys;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the collection of explicit settings
        /// </summary>
        private void EnsureExplicitSettings()
        {
            // Set the value
            if (mExplicitSettings == null)
            {
                lock (explicitSettingsLocker)
                {
                    if (mExplicitSettings == null)
                    {
                        mExplicitSettings = new SafeDictionary<string, string>();
                        mExplicitSettings.AllowNulls = true;
                    }
                }
            }
        }


        /// <summary>
        /// Returns application settings from Azure service configuration file.
        /// </summary>
        /// <param name="key">Setting key</param>
        protected static string RaiseGetSettings(string key)
        {
            if (GetApplicationSettings != null)
            {
                return GetApplicationSettings(key);
            }

            return null;
        }


        /// <summary>
        /// Clear settings.
        /// </summary>
        public void Clear()
        {
            SettingsHelper.ApplicationConfiguration = null;

            mExplicitSettings = null;
            Settings.Clear();
        }


        /// <summary>
        /// Attempts to get the settings key value using the given list of keys.
        /// </summary>
        /// <param name="keys">List of keys to search</param>
        protected string GetFromSettings(string[] keys)
        {
            // Go through all possible given keys
            foreach (string key in keys)
            {
                var result = GetFromSettings(key);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the settings key from the settings
        /// </summary>
        /// <param name="key">Key name</param>
        internal string GetFromSettings(string key)
        {
            string result = null;

            // Try to get from explicit settings (highest priority)
            if (mExplicitSettings != null)
            {
                result = mExplicitSettings[key];
            }

            if (result == null)
            {
                // Try get value from Azure service configuration (middle priority)
                result = RaiseGetSettings(key);

                if (result == null)
                {
                    // If not found, try to get from AppSettings (lowest priority)
                    if (SystemContext.UseWebApplicationConfiguration)
                    {
                        var elem = SettingsHelper.ApplicationConfiguration.AppSettings.Settings[key];
                        if (elem != null)
                        {
                            result = elem.Value;
                        }
                    }
                    else
                    {
                        result = ConfigurationManager.AppSettings[key];
                    }
                }
            }

            return result;
        }

        #endregion
    }
}

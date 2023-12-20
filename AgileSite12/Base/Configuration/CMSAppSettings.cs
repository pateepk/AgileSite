using System;
using System.Configuration;
using System.Linq;

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
        internal static readonly object SingletonLock = new object();

        // Locker object for explicit settings
        private static readonly object explicitSettingsLocker = new object();

        // Explicitly set settings
        private SafeDictionary<string, string> mExplicitSettings;

        #endregion


        #region "Events"

        /// <summary>
        /// Delegate for event for reading application's settings.
        /// </summary>
        public delegate string GetAppSettingsEventHandler(string key);


        /// <summary>
        /// Event which is raised when external application wants to read application settings.
        /// </summary>
        public static event GetAppSettingsEventHandler GetApplicationSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Settings dictionary.
        /// </summary>
        private SafeDictionary<string, string> Settings
        {
            get;
        } = new SafeDictionary<string, string>
        {
            AllowNulls = true
        };


        /// <summary>
        /// Settings key indexer.
        /// </summary>
        /// <param name="key">Settings key</param>
        public string this[string key]
        {
            get
            {
                // Try to get by the given key
                if (!Settings.TryGetValue(key, out var result))
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
        /// Gets the key from settings with support for localized values. Values can be localized based on machine name and virtual directory (application path).
        /// 
        /// Examples of .config key names:
        /// 
        /// [Machine1]/VirtualDirectory1:CMSWebFarmEnabled
        /// [Machine1]CMSWebFarmEnabled
        /// /VirtualDirectory2:CMSWebFarmEnabled
        /// CMSWebFarmEnabled
        /// </summary>
        /// <param name="key">Setting key.</param>
        private string GetFromSettingsWithLocalizedValues(string key)
        {
            // Setting key CMSMachineName should not be localized using machine name, otherwise infinite loop will occur
            var keys = key.Equals("CMSMachineName", StringComparison.InvariantCultureIgnoreCase) 
                ? GetKeys(key, SystemContext.ApplicationPath) 
                : GetKeys(key, SystemContext.MachineName, SystemContext.ApplicationPath);

            // Get from AppSettings and cache
            return GetFromSettings(keys);
        }


        private static string[] GetKeys(string key, string serverName, string appPath)
        {
            string[] keys =
            {
                $"[{serverName}]{appPath}:{key}",
                $"[{serverName}]{key}"
            };

            return keys.Union(GetKeys(key, appPath)).ToArray();
        }


        private static string[] GetKeys(string key, string appPath)
        {
            string[] keys =
            {
                $"{appPath}:{key}",
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

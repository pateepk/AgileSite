using System.Configuration;
using System.IO;

namespace CMS.Base
{
    /// <summary>
    /// Settings helper.
    /// </summary>
    public static class SettingsHelper
    {
        #region "Variables"

        private static bool? mAllowLocalConfigKeys;
        private static CMSAppSettings mAppSettings;
        private static CMSConnectionStrings mConnectionStrings;
        private static Configuration mApplicationConfiguration;
        private static bool mAllowUpdateTimeStamps = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, local configuration keys are allowed within the application
        /// </summary>
        public static bool AllowLocalConfigKeys
        {
            get
            {
                if (mAllowLocalConfigKeys == null)
                {
                    mAllowLocalConfigKeys = AppSettings.GetFromSettings("CMSAllowLocalConfigKeys") == "true";
                }

                return mAllowLocalConfigKeys.Value;
            }
        }


        /// <summary>
        /// AppSettings collection.
        /// </summary>
        public static CMSAppSettings AppSettings
        {
            get
            {
                if (mAppSettings == null)
                {
                    lock (CMSAppSettings.SingletonLock)
                    {
                        if (mAppSettings == null)
                        {
                            mAppSettings = new CMSAppSettings();
                        }
                    }
                }
                return mAppSettings;
            }
        }


        /// <summary>
        /// ConnectionStrings collection.
        /// </summary>
        public static CMSConnectionStrings ConnectionStrings
        {
            get
            {
                return mConnectionStrings ?? (mConnectionStrings = new CMSConnectionStrings());
            }
        }


        /// <summary>
        /// Application configuration.
        /// </summary>
        internal static Configuration ApplicationConfiguration
        {
            get
            {
                return mApplicationConfiguration ?? (mApplicationConfiguration = OpenConfiguration(SystemContext.WebApplicationPhysicalPath));
            }
            set
            {
                mApplicationConfiguration = value;
            }
        }



        /// <summary>
        /// Opens the application configuration from application at the given path
        /// </summary>
        /// <param name="appPath">Application path</param>
        internal static Configuration OpenConfiguration(string appPath)
        {
            ExeConfigurationFileMap ecfm = new ExeConfigurationFileMap();

#pragma warning disable BH1014 // Do not use System.IO
            ecfm.ExeConfigFilename = Path.Combine(appPath, "web.config");
#pragma warning restore BH1014 // Do not use System.IO

            Configuration config;

            // Open web.config file from application path
            try
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(ecfm, ConfigurationUserLevel.None);
            }
            catch
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }

            return config;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets or adds the specified key to the AppSettings section for web application.
        /// </summary>
        /// <param name="key">Settings key</param>
        /// <param name="value">Settings value</param>
        public static bool SetConfigValue(string key, string value)
        {
            try
            {
                if (ApplicationConfiguration.AppSettings.Settings[key] != null)
                {
                    ApplicationConfiguration.AppSettings.Settings[key].Value = value;
                }
                else
                {
                    ApplicationConfiguration.AppSettings.Settings.Add(key, value);
                }

                ApplicationConfiguration.Save(ConfigurationSaveMode.Modified);

                // Clear application settings
                AppSettings.Clear();

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Remove the specified key from the AppSettings section for web application.
        /// </summary>
        /// <param name="key">Settings key</param>
        public static bool RemoveConfigValue(string key)
        {
            try
            {
                if (ApplicationConfiguration.AppSettings.Settings[key] != null)
                {
                    ApplicationConfiguration.AppSettings.Settings.Remove(key);
                    ApplicationConfiguration.Save(ConfigurationSaveMode.Modified);

                    // Clear application settings
                    AppSettings.Clear();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Sets or adds the specified connection string in the ConnectionStrings section for web application.
        /// </summary>
        /// <param name="name">ConnectionString name</param>
        /// <param name="connectionString">Connection string</param>
        public static bool SetConnectionString(string name, string connectionString)
        {
            try
            {
                if (ApplicationConfiguration.ConnectionStrings.ConnectionStrings[name] != null)
                {
                    ApplicationConfiguration.ConnectionStrings.ConnectionStrings[name].ConnectionString = connectionString;
                }
                else
                {
                    ApplicationConfiguration.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(name, connectionString));
                }

                ApplicationConfiguration.Save(ConfigurationSaveMode.Modified);

                // Clear connection strings
                ConnectionStrings.Clear();

                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Removes connection string from web.config.
        /// </summary>
        /// <param name="name">Name of the connection string</param>
        /// <returns>Returns true if removing was successful</returns>
        public static bool RemoveConnectionString(string name)
        {
            try
            {
                if (ApplicationConfiguration.ConnectionStrings.ConnectionStrings[name] != null)
                {
                    ApplicationConfiguration.ConnectionStrings.ConnectionStrings.Remove(name);
                    ApplicationConfiguration.Save(ConfigurationSaveMode.Modified);
                    ConnectionStrings.Clear();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion


        /// <summary>
        /// If true, time stamp update of the objects is allowed.
        /// </summary>
        public static bool AllowUpdateTimeStamps
        {
            get
            {
                return mAllowUpdateTimeStamps;
            }
            set
            {
                mAllowUpdateTimeStamps = value;
            }
        }
    }
}
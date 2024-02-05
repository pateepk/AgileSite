using System;
using System.Configuration;

namespace CMS.Base
{
    /// <summary>
    /// Connection strings
    /// </summary>
    public class CMSConnectionStrings
    {
        #region "Variables"

        /// <summary>
        /// Connection string names by their values
        /// </summary>
        public SafeDictionary<string, string> ConnectionStringNames = new StringSafeDictionary<string>(false);

        /// <summary>
        /// Explicitly set connection strings
        /// </summary>
        private SafeDictionary<string, ConnectionStringSettings> mExplicitSettings;

        /// <summary>
        /// Connection strings dictionary.
        /// </summary>
        private SafeDictionary<string, ConnectionStringSettings> mSettings;

        #endregion


        #region "Events"

        /// <summary>
        /// Delegate for event for reading application's connection strings.
        /// </summary>
        public delegate string GetConnectionStringEventHandler(string key);


        /// <summary>
        /// Event which is thrown when Azure application wants to read application connection strings.
        /// </summary>
        public static event GetConnectionStringEventHandler GetConnectionString;

        #endregion


        #region "Properties"

        /// <summary>
        /// Connection strings dictionary.
        /// </summary>
        private SafeDictionary<string, ConnectionStringSettings> Settings
        {
            get
            {
                if (mSettings == null)
                {
                    mSettings = new SafeDictionary<string, ConnectionStringSettings>();
                    mSettings.AllowNulls = false;
                }

                return mSettings;
            }
        }


        /// <summary>
        /// Gets number of connection strings.
        /// </summary>
        public int Count
        {
            get
            {
                return ConnectionStringNames.Count;
            }
        }


        /// <summary>
        /// Connection strings indexer.
        /// </summary>
        /// <param name="key">Settings key</param>
        public ConnectionStringSettings this[string key]
        {
            get
            {
                return GetItem(key);
            }
            set
            {
                if ((value != null) && (key != value.Name))
                {
                    throw new Exception("[CMSConnectionStrings]: Connection string name '" + value.Name + "' does not match the key indexer '" + key + "'.");
                }

                // Set the value
                EnsureExplicitSettings();

                mExplicitSettings[key] = value;

                Settings.Clear();
            }
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
                mExplicitSettings = new SafeDictionary<string, ConnectionStringSettings>();
                mExplicitSettings.AllowNulls = true;
            }
        }


        /// <summary>
        /// Returns application connection string from external source
        /// </summary>
        /// <param name="key">Setting key</param>
        protected static string RaiseGetConnectionString(string key)
        {
            if (GetConnectionString != null)
            {
                return GetConnectionString(key);
            }

            return null;
        }


        /// <summary>
        /// Clear connection strings table
        /// </summary>
        public void Clear()
        {
            mExplicitSettings = null;

            Settings.Clear();

            ConnectionStringNames.Clear();
        }
        

        /// <summary>
        /// Returns item by key.
        /// </summary>
        private ConnectionStringSettings GetItem(string key)
        {
            // Try to get by the given key
            ConnectionStringSettings result;

            if (!Settings.TryGetValue(key, out result))
            {
                // Not found as cached - Load
                if (!SettingsHelper.AllowLocalConfigKeys)
                {
                    result = GetFromSettings(key);
                }
                else
                {
                    string appPath = SystemContext.ApplicationPath;
                    string machineName = SystemContext.MachineName;

                    // Get by the all possible specific keys
                    var keys = new[]
                        {
                            String.Format("[{0}]{1}:{2}", machineName, appPath, key),
                            String.Format("[{0}]{1}", machineName, key),
                            String.Format("{0}:{1}", appPath, key),
                            key
                        };

                    // Get from AppSettings and cache
                    result = GetFromSettings(keys);

                    Settings[key] = result;
                }
            }

            return result;
        }



        /// <summary>
        /// Attempts to get the connection string using the given list of keys.
        /// </summary>
        /// <param name="keys">List of keys to search</param>
        protected ConnectionStringSettings GetFromSettings(string[] keys)
        {
            ConnectionStringSettings result = null;

            // Go through all possible given keys
            foreach (string key in keys)
            {
                result = GetFromSettings(key);

                if (result != null)
                {
                    break;
                }
            }

            // Store to connection strings by name
            if (result != null)
            {
                ConnectionStringNames[result.ConnectionString] = result.Name;
            }

            return result;
        }


        /// <summary>
        /// Attempts to get a connection string directly from settings
        /// </summary>
        /// <param name="key">Connection string key</param>
        private ConnectionStringSettings GetFromSettings(string key)
        {
            ConnectionStringSettings result = null;

            // Try to get from explicit settings (highest priority)
            if (mExplicitSettings != null)
            {
               result = mExplicitSettings[key];
            }

            if (result == null)
            {
                // Try to get connection string from azure (middle priority)
                if (SystemContext.IsRunningOnAzure)
                {
                    var connString = RaiseGetConnectionString(key);
                    if (!string.IsNullOrEmpty(connString))
                    {
                        result = new ConnectionStringSettings(key, connString);
                    }
                }

                if (result == null)
                {
                    // Get from standard configuration files (lowest priority)
                    if (SystemContext.UseWebApplicationConfiguration)
                    {
                        result = SettingsHelper.ApplicationConfiguration.ConnectionStrings.ConnectionStrings[key];
                    }
                    else
                    {
                        result = ConfigurationManager.ConnectionStrings[key];
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the connection string name by its value
        /// </summary>
        /// <param name="connectionString">Connection string value</param>
        public string GetConnectionStringName(string connectionString)
        {
            return ConnectionStringNames[connectionString];
        }


        /// <summary>
        /// Sets the specific connection string to a new value
        /// </summary>
        /// <param name="name">Connection string name</param>
        /// <param name="connectionString">Connection string value</param>
        public void SetConnectionString(string name, string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                this[name] = null;
            }
            else
            {
                this[name] = new ConnectionStringSettings(name, connectionString);
                ConnectionStringNames[connectionString] = name;
            }
        }

        #endregion
    }
}

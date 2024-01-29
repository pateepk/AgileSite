using System.Configuration;

namespace CMS.Core
{
    /// <summary>
    /// Default service to provide app settings
    /// </summary>
    internal class DefaultAppSettingsService : IAppSettingsService
    {
        /// <summary>
        /// Returns null
        /// </summary>
        /// <param name="keyName">Settings key</param>
        public string this[string keyName]
        {
            get
            {
                return GetSetting(keyName);
            }
        }
        

        /// <summary>
        /// Gets the setting value based on the given key
        /// </summary>
        /// <param name="keyName">Settings key name</param>
        internal static string GetSetting(string keyName)
        {
            return ConfigurationManager.AppSettings[keyName];
        }
    }
}

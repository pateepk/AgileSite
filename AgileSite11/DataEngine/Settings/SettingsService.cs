using System;

using CMS.Core;


namespace CMS.DataEngine
{
    /// <summary>
    /// Settings service
    /// </summary>
    public class SettingsService : ISettingsService
    {
        /// <summary>
        /// Returns true if the settings service's data is available.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return SettingsKeyInfoProvider.ProviderObject.IsDataAvailable;
            }
        }


        /// <summary>
        /// Gets the specific settings from the database.
        /// </summary>
        /// <param name="keyName">Settings key</param>
        public string this[string keyName]
        {
            get 
            {
                if (!IsAvailable)
                {
                    return String.Empty;
                }

                return SettingsKeyInfoProvider.GetValue(keyName);
            }
        }
    }
}

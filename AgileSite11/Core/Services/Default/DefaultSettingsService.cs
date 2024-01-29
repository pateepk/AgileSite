using System;

namespace CMS.Core
{
    /// <summary>
    /// Default service to provide settings
    /// </summary>
    internal class DefaultSettingsService : ISettingsService
    {
        /// <summary>
        /// Returns true if the settings service is available
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Returns null
        /// </summary>
        /// <param name="keyName">Settings key</param>
        public string this[string keyName]
        {
            get
            {
                return String.Empty;
            }
        }
    }
}

using System;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ISettingService"/> interface.
    /// </summary>
    internal class DefaultSettingService : ISettingService
    {
        private readonly SiteInfoIdentifier mIdentifier;


        /// <summary>
        /// Creates a new instance of <see cref="DefaultSettingService"/>.
        /// </summary>
        /// <param name="identifier">Site identifier.</param>
        public DefaultSettingService(SiteInfoIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            mIdentifier = identifier;
        }


        /// <summary>
        /// Returns boolean value of setting with given name.
        /// </summary>
        public bool GetBooleanValue(string key)
        {
            return SettingsKeyInfoProvider.GetBoolValue(key, mIdentifier);
        }


        /// <summary>
        /// Returns integer value of setting with given name.
        /// </summary>
        public int GetIntegerValue(string key)
        {
            return SettingsKeyInfoProvider.GetIntValue(key, mIdentifier);
        }


        /// <summary>
        /// Returns string value of setting with given name.
        /// </summary>
        public string GetStringValue(string key)
        {
            return SettingsKeyInfoProvider.GetValue(key, mIdentifier);
        }
    }
}
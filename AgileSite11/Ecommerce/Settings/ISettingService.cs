using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ISettingService), typeof(DefaultSettingService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for service providing setting values.
    /// </summary>
    /// <remarks>
    /// Interface is meant to access settings persisted via respective object type not on web.config settings.
    /// </remarks>
    /// <seealso cref="SettingsKeyInfo"/>
    /// <seealso cref="SettingsKeyInfoProvider"/>
    public interface ISettingService
    {
        /// <summary>
        /// Returns boolean value of setting with given name.
        /// </summary>
        bool GetBooleanValue(string key);


        /// <summary>
        /// Returns integer value of setting with given name.
        /// </summary>
        int GetIntegerValue(string key);


        /// <summary>
        /// Returns string value of setting with given name.
        /// </summary>
        string GetStringValue(string key);
    }
}
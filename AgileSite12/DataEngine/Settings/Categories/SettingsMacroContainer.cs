using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Macro container for all settings.
    /// </summary>
    /// <remarks>This API supports the framework infrastructure and is not intended to be used directly from your code.</remarks>
    public class SettingsMacroContainer : IDataContainer, IMacroSecurityCheckPermissions
    {
        // Backward compatibility for old setting macro style (Settings.Category.CMSSettingKey)
        private SettingsCategoryContainer settingsCategory;

        private readonly int siteId;


        /// <summary>
        /// Creates new instance of <see cref="SettingsMacroContainer"/>.
        /// </summary>
        /// <param name="siteId">Current site id</param>
        public SettingsMacroContainer(int siteId)
        {
            this.siteId = siteId;

        }


        /// <summary>
        /// Gets the setting key value based on settings key name.
        /// </summary>
        /// <param name="columnName">Setting key name.</param>
        /// <exception cref="NotImplementedException">Thrown when setter is used.</exception>
        public object this[string columnName]
        {
            get { return GetValue(columnName); }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        /// Gets collection of all available setting keys.
        /// </summary>
        public List<string> ColumnNames => GetSiteSettingKeys();


        /// <summary>
        /// Returns true when setting key is available by specified name.
        /// </summary>
        /// <param name="columnName">Setting key name</param>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName, StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Gets the setting key value.
        /// </summary>
        /// <param name="columnName">Setting key name</param>
        public object GetValue(string columnName)
        {
            object retval = null;
            TryGetValue(columnName, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented method. Throws exception in all cases.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets setting key value for specified setting key name.
        /// </summary>
        /// <param name="columnName">Setting key name</param>
        /// <param name="value">Setting key value</param>
        /// <returns><c>true</c>when setting key was found; otherwise <c>false</c>.</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = SettingsKeyInfoProvider.GetValue(columnName, siteId, true);

            // Backward compatibility for old setting macro style (Settings.Category.CMSSettingKey)
            if (value == null)
            {
                settingsCategory = settingsCategory ?? (settingsCategory = new SettingsCategoryContainer(SettingsCategoryInfoProvider.RootCategory, siteId, false));
                return settingsCategory.TryGetValue(columnName, out value);
            }

            return true;
        }


        private List<string> GetSiteSettingKeys()
        {
            return SettingsKeyInfoProvider.GetSettingsKeys()
                .Select(x => x.KeyName)
                .Distinct()
                .ToList();
        }


        /// <summary>
        /// Gets an object for which to perform the permissions check.
        /// </summary>
        public object GetObjectToCheck()
        {
            return SettingsCategoryInfoProvider.RootCategory;
        }
    }
}
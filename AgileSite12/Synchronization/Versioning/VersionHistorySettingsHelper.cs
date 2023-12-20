using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Helper class providing general functionality used by version history.
    /// </summary>
    public static class VersionHistorySettingsHelper
    {
        /// <summary>
        /// Returns a query allowing to select the IDs of sites that specify own value for given settings key.
        /// </summary>
        /// <param name="keyName">Settings key name to check.</param>
        public static IDataQuery GetSiteIDsWhereSettingsKeyIsOverridden(string keyName)
        {
            return SettingsKeyInfoProvider
                .GetSettingsKeys()
                .WhereEquals("KeyName", keyName)
                .WhereNotNull("SiteID")
                .Columns("SiteID")
                .AsSingleColumn();
        }


        /// <summary>
        /// Returns true if the original <paramref name="setting" /> value was greater than the current one.
        /// </summary>
        /// <remarks>
        /// Use when given <paramref name="setting"/> is being changed.
        /// When comparing the values, zero and less represent unlimited version history length.
        /// </remarks>
        /// <param name="setting">Settings key representing version history length limit.</param>
        public static bool IsLimitDecreased(SettingsKeyInfo setting)
        {
            int currentLimit = ValidationHelper.GetInteger(setting.KeyValue, 0);
            int previousLimit = ValidationHelper.GetInteger(setting.GetOriginalValue("KeyValue"), 0);

            return IsNewLimitLower(previousLimit, currentLimit);
        }


        /// <summary>
        /// Returns true if the global setting value was greater than given <paramref name="setting"/>.
        /// </summary>
        /// <remarks>
        /// Use when given <paramref name="setting"/> is being created and thus the global setting will not apply anymore.
        /// When comparing the values, zero and less represent unlimited version history length.
        /// </remarks>
        /// <param name="setting">Settings key representing version history length limit.</param>
        public static bool IsLimitLowerThanGlobal(SettingsKeyInfo setting)
        {
            // Global value that the site used to inherit
            int globalLimit = SettingsKeyInfoProvider.GetIntValue(setting.KeyName);

            // New site specific value being created
            int siteLimit = ValidationHelper.GetInteger(setting.KeyValue, 0);

            return IsNewLimitLower(globalLimit, siteLimit);
        }


        /// <summary>
        /// Returns true if given <paramref name="setting"/> was greater than the global setting value.
        /// </summary>
        /// <remarks>
        /// Use when given <paramref name="setting"/> is being removed and thus the global setting will apply from now on.
        /// When comparing the values, zero and less represent unlimited version history length.
        /// </remarks>
        /// <param name="setting">Settings key representing version history length limit.</param>
        public static bool IsLimitHigherThanGlobal(SettingsKeyInfo setting)
        {
            // Old site value being deleted
            int siteLimit = ValidationHelper.GetInteger(setting.GetOriginalValue("KeyValue"), 0);

            // Global value that will the site inherit
            int globalLimit = SettingsKeyInfoProvider.GetIntValue(setting.KeyName);

            return IsNewLimitLower(siteLimit, globalLimit);
        }


        private static bool IsNewLimitLower(int oldLimit, int newLimit)
        {
            if (newLimit <= 0)
            {
                // No version history limit
                return false;
            }

            if (oldLimit <= 0)
            {
                // Was unlimited, not anymore
                return true;
            }

            return oldLimit > newLimit;
        }
    }
}

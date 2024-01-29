using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Synchronization
{
    /// <summary>
    /// Handlers that trim object version history if the limit settings are changed.
    /// </summary>
    internal static class VersioningHandlers
    {
        public static void Init()
        {
            var settingsEvents = SettingsKeyInfo.TYPEINFO.Events;

            settingsEvents.Update.Before += HandleVersionHistoryLimitChange;
            settingsEvents.Insert.After += HandleSiteVersionHistoryLimitCreation;
            settingsEvents.Delete.After += HandleSiteVersionHistoryLimitDeletion;
        }


        private static void HandleSiteVersionHistoryLimitDeletion(object sender, ObjectEventArgs e)
        {
            var setting = e.Object as SettingsKeyInfo;

            if (IsVersionHistoryLimitSetting(setting) && VersionHistorySettingsHelper.IsLimitHigherThanGlobal(setting))
            {
                DeleteOlderVersionsAsync(setting);
            }
        }


        private static void HandleSiteVersionHistoryLimitCreation(object sender, ObjectEventArgs e)
        {
            var setting = e.Object as SettingsKeyInfo;

            if (IsVersionHistoryLimitSetting(setting) && VersionHistorySettingsHelper.IsLimitLowerThanGlobal(setting))
            {
                DeleteOlderVersionsAsync(setting);
            }
        }


        private static void HandleVersionHistoryLimitChange(object sender, ObjectEventArgs e)
        {
            var setting = e.Object as SettingsKeyInfo;

            if (IsVersionHistoryLimitSetting(setting) && VersionHistorySettingsHelper.IsLimitDecreased(setting))
            {
                e.CallWhenFinished(() => DeleteOlderVersionsAsync(setting));
            }
        }


        private static void DeleteOlderVersionsAsync(SettingsKeyInfo setting)
        {
            var thread = new CMSThread(() =>
            {
                try
                {
                    DeleteOlderVersions(setting.KeyName, setting.SiteID);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("ObjectVersioning", "DeleteOlderVersions", ex, setting.SiteID, "An error occurred while trimming the object version history according to the new length limit.");
                }
            });

            thread.Start();
        }


        /// <summary>
        /// Deletes excessive old object versions from site specified by given <paramref name="siteId"/>
        /// or global and all sites that inherit the global setting if the <paramref name="siteId"/> is 0 or less.
        /// </summary>
        /// <param name="settingsKeyName">Settings key that may be overridden on sites.</param>
        /// <param name="siteId">Settings key site ID</param>
        internal static void DeleteOlderVersions(string settingsKeyName, int siteId)
        {
            var versionsQuery = ObjectVersionHistoryInfoProvider
                .GetVersionHistories()
                .Columns("VersionObjectID", "VersionObjectType");

            string singleSiteName = null;
            if (siteId > 0)
            {
                // For site specific setting select versions only on this site
                singleSiteName = SiteInfoProvider.GetSiteName(siteId);
                versionsQuery.WhereEquals("VersionObjectSiteID", siteId);
            }
            else
            {
                // For global setting select versions from all sites that don't have overridden setting
                versionsQuery.AddColumn("VersionObjectSiteID")
                             .Where(new WhereCondition()
                                 .WhereNull("VersionObjectSiteID")
                                 .Or()
                                 .WhereNotIn("VersionObjectSiteID", VersionHistorySettingsHelper.GetSiteIDsWhereSettingsKeyIsOverridden(settingsKeyName)));
            }

            versionsQuery
                .Distinct()
                .ForEachRow(row =>
                {
                    string siteName = singleSiteName ?? SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(row["VersionObjectSiteID"], 0));
                    string objectType = ValidationHelper.GetString(row["VersionObjectType"], String.Empty);
                    int objectId = ValidationHelper.GetInteger(row["VersionObjectID"], 0);

                    ObjectVersionManager.DeleteOlderVersions(objectType, objectId, siteName);
                });
        }


        private static bool IsVersionHistoryLimitSetting(SettingsKeyInfo setting)
        {
            if (setting == null)
            {
                return false;
            }

            switch (setting.KeyName.ToLowerInvariant())
            {
                case "cmsobjectversionhistorylength":
                case "cmsobjectversionhistorymajorversionslength":
                    return true;

                default:
                    return false;
            }
        }
    }
}

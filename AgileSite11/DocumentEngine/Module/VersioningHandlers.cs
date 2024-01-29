using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.DocumentEngine
{
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
                    EventLogProvider.LogException("Content", "DeleteOlderVersions", ex, setting.SiteID, "An error occurred while trimming the version history according to the new length limit.");
                }
            });

            thread.Start();
        }


        /// <summary>
        /// Deletes excessive old page versions from site specified by given <paramref name="siteId"/>
        /// or global and all sites that inherit the global setting if the <paramref name="siteId"/> is 0 or less.
        /// </summary>
        /// <param name="settingsKeyName">Settings key that may be overridden on sites.</param>
        /// <param name="siteId">Settings key site ID</param>
        internal static void DeleteOlderVersions(string settingsKeyName, int siteId)
        {
            var versionsQuery = VersionHistoryInfoProvider
                .GetVersionHistories()
                .Columns("DocumentID");

            string singleSiteName = null;
            if (siteId > 0)
            {
                // For site specific setting select versions only on this site
                singleSiteName = SiteInfoProvider.GetSiteName(siteId);
                versionsQuery.WhereEquals("NodeSiteID", siteId);
            }
            else
            {
                // For global setting select versions from all sites that don't have overridden setting
                versionsQuery.AddColumn("NodeSiteID")
                             .WhereNotIn("NodeSiteID", VersionHistorySettingsHelper.GetSiteIDsWhereSettingsKeyIsOverridden(settingsKeyName));
            }

            var manager = VersionManager.GetInstance(null);
            versionsQuery
                .Distinct()
                .ForEachRow(row =>
                {
                    string siteName = singleSiteName ?? SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(row["NodeSiteID"], 0));
                    int documentId = ValidationHelper.GetInteger(row["DocumentID"], 0);

                    manager.DeleteOlderVersions(documentId, siteName);
                });
        }


        private static bool IsVersionHistoryLimitSetting(SettingsKeyInfo setting)
        {
            if (setting == null)
            {
                return false;
            }

            return setting.KeyName.Equals("CMSVersionHistoryLength", StringComparison.OrdinalIgnoreCase);
        }
    }
}

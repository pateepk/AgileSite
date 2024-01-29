using System;

using CMS.Base;
using CMS.Core;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.Activities
{
    /// <summary>
    /// Provides comfortable access to settings keys for activities.
    /// </summary>
    public static class ActivitySettingsHelper
    {
        /// <summary>
        /// Checks if specified activity logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="keyname">Settings key name of particular activity (e.g. "CMSCMPageVisits")</param>
        /// <param name="activityType">Activity type code name (e.g. PredefinedActivityType.PAGE_VISIT)</param>
        public static bool GetLoggingEnabled(string siteName, string keyname, string activityType)
        {
            if (GetBoolValue(siteName, keyname))
            {
                return ActivityTypeInfoProvider.GetActivityTypeEnabled(activityType);
            }

            return false;
        }


        /// <summary>
        /// Determines whether on-line marketing module is activated.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static bool OnlineMarketingEnabled(int siteId)
        {
            string siteName = SiteInfoProvider.GetSiteName(siteId);
            return OnlineMarketingEnabled(siteName);
        }


        /// <summary>
        /// Determines whether on-line marketing module is activated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool OnlineMarketingEnabled(string siteName)
        {
            return GetBoolValue(siteName, "CMSEnableOnlineMarketing");
        }


        /// <summary>
        /// Determines whether on-line marketing module is available.
        /// </summary>
        public static bool IsModuleLoaded()
        {
            return ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING);
        }


        /// <summary>
        /// Returns true if activities logging is enabled.
        /// </summary>
        public static bool ActivitiesEnabled(int siteId)
        {
            string siteName = SiteInfoProvider.GetSiteName(siteId);
            return ActivitiesEnabled(siteName);
        }


        /// <summary>
        /// Returns true if activities logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name (or NULL for global setting)</param>
        public static bool ActivitiesEnabled(string siteName)
        {
            return GetBoolValue(siteName, "CMSCMActivitiesEnabled");
        }


        /// <summary>
        /// Returns true if user registration logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name (or NULL for global setting)</param>
        public static bool UserRegistrationEnabled(string siteName)
        {
            return GetLoggingEnabled(siteName, "CMSCMUserRegistration", PredefinedActivityType.REGISTRATION);
        }


        /// <summary>
        /// Returns true if adding product to WL logging is enabled.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static bool AddingProductToWLEnabled(int siteId)
        {
            string siteName = SiteInfoProvider.GetSiteName(siteId);
            return AddingProductToWLEnabled(siteName);
        }


        /// <summary>
        /// Returns true if adding product to WL logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name (or NULL for global setting)</param>
        public static bool AddingProductToWLEnabled(string siteName)
        {
            return GetLoggingEnabled(siteName, "CMSCMAddingProductToWL", PredefinedActivityType.PRODUCT_ADDED_TO_WISHLIST);
        }


        /// <summary>
        /// Returns true if forum post subscription logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name (or NULL for global setting)</param>
        public static bool ForumPostSubscriptionEnabled(string siteName)
        {
            return GetLoggingEnabled(siteName, "CMSCMForumPostSubscription", PredefinedActivityType.SUBSCRIPTION_FORUM_POST);
        }


        /// <summary>
        /// Returns true if forum post logging is enabled.
        /// </summary>
        /// <param name="siteName">Site name (or NULL for global setting)</param>
        public static bool ForumPostsEnabled(string siteName)
        {
            return GetLoggingEnabled(siteName, "CMSCMForumPosts", PredefinedActivityType.FORUM_POST);
        }

        
        /// <summary>
        /// Returns true if global logging switch for the given site is enabled and online marketing module is loaded.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static bool ActivitiesEnabledAndModuleLoaded(int siteId)
        {
            string siteName = SiteInfoProvider.GetSiteName(siteId);
            return ActivitiesEnabledAndModuleLoaded(siteName);
        }


        /// <summary>
        /// Returns true if global logging switch for the given site is enabled and online marketing module is loaded.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool ActivitiesEnabledAndModuleLoaded(string siteName)
        {
            return OnlineMarketingEnabled(siteName) && ActivitiesEnabled(siteName) && IsModuleLoaded();
        }


        /// <summary>
        /// Returns true if logging activity is enabled for given user.
        /// </summary>
        /// <param name="ui">User info</param>
        public static bool ActivitiesEnabledForThisUser(UserInfo ui)
        {
            if (ui == null)
            {
                return false;
            }

            return ui.UserSettings.UserLogActivities;
        }


        /// <summary>
        /// Returns key value (bool).
        /// </summary>
        /// <param name="siteName">Site name (optional)</param>
        /// <param name="keyName">Key name</param>
        private static bool GetBoolValue(string siteName, string keyName)
        {
            if (!String.IsNullOrEmpty(siteName))
            {
                siteName += ".";
            }

            return CoreServices.Settings[siteName + keyName].ToBoolean(false);
        }
    }
}
﻿using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Class providing methods for web part partial cache items management.
    /// </summary>
    public static class PartialCacheItemsProvider
    {
        internal static CMSStatic<string> mCacheItems = new CMSStatic<string>();


        /// <summary>
        /// List of the cache key items for partial caching separated by semicolon.
        /// Default value is "username;sitename;lang".
        /// </summary>
        public static string GetEnabledCacheItems()
        {
            string items = mCacheItems.Value;

            if (items == null)
            {
                string appSettingsCacheItems = SettingsHelper.AppSettings["CMSPartialCacheItems"];
                if (!String.IsNullOrEmpty(appSettingsCacheItems))
                {
                    items = appSettingsCacheItems;
                }
                else
                {
                    // Get only enabled cache item names from the CMS settings
                    items = CacheHelper.GetCacheItemsString(GetCacheItemNames());
                }

                mCacheItems.Value = items;
            }

            return items;
        }


        /// <summary>
        /// Clears cached Enabled cache items and optionally logs a web farm task to propagate the change.
        /// </summary>
        /// <param name="logTask">A value indicating whether to log a web farm task.</param>
        internal static void ClearEnabledCacheItemsCache(bool logTask)
        {
            mCacheItems.Value = null;

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearPartialCacheItemsWebFarmTask());
            }
        }


        /// <summary>
        /// Gets the available cache item names with a boolean value indicating whether the cache item name is enabled or not.
        /// </summary>
        public static Dictionary<string, bool> GetCacheItemNames()
        {
            return CacheHelper.GetCombinedCacheItems(SettingsKeyInfoProvider.GetValue("CMSPartialCacheItems"), GetAvailableCacheItemNames());
        }


        /// <summary>
        /// Gets a collection of cache item names supported by the system with an indication whether they are enabled by default.
        /// </summary>
        private static Dictionary<string, bool> GetAvailableCacheItemNames()
        {
            Dictionary<string, bool> items = new Dictionary<string, bool>();

            // Items enabled by default
            items.Add("username", true);
            items.Add("sitename", true);
            items.Add("lang", true);

            // Items disabled by default
            items.Add("browser", false);

            return items;
        }
    }
}

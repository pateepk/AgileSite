using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides support for cache web farm synchronization
    /// </summary>
    public class CacheSynchronization
    {
        private static string mSynchronizeCache;


        /// <summary>
        /// Gets or sets value that indicates whether cache synchronization is enabled.
        /// </summary>
        public static bool SynchronizeCache
        {
            get
            {
                if (mSynchronizeCache == null)
                {
                    mSynchronizeCache = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSWebFarmSynchronizeCache"], string.Empty);
                }

                return ValidationHelper.GetBoolean(mSynchronizeCache, CoreServices.Settings["CMSWebFarmSynchronizeCache"].ToBoolean(true));
            }
            set
            {
                mSynchronizeCache = value.ToString();
            }
        }


        /// <summary>
        /// Logs the web farm synchronization task for the touch key operation
        /// </summary>
        /// <param name="keys">Keys to touch</param>
        public static void LogTouchKeysTask(params string[] keys)
        {
            if (SynchronizeCache)
            {
                WebFarmHelper.CreateTask(CacheTaskType.TouchCacheItem, null, keys.ToArray());
            }
        }


        /// <summary>
        /// Logs the web farm synchronization task for the touch key operation
        /// </summary>
        /// <param name="keys">Keys to touch</param>
        public static void LogTouchKeysTask(IEnumerable<string> keys)
        {
            LogTouchKeysTask(keys.ToArray());
        }


        /// <summary>
        /// Logs the web farm task for clear cache operation
        /// </summary>
        /// <param name="startsWith">Starts with parameter for the clear operation</param>
        /// <param name="caseSensitive">Case sensitive</param>
        public static void LogClearCacheTask(string startsWith, bool caseSensitive)
        {
            if (SynchronizeCache)
            {
                WebFarmHelper.CreateTask(CacheTaskType.ClearCacheItems, null, caseSensitive.ToString(), startsWith);
            }
        }


        /// <summary>
        /// Logs the web farm synchronization task for remove operation
        /// </summary>
        /// <param name="originalKey">Original key</param>
        /// <param name="caseSensitive">Case sensitive</param>
        public static void LogRemoveTask(string originalKey, bool caseSensitive)
        {
            if (SynchronizeCache)
            {
                WebFarmHelper.CreateTask(CacheTaskType.RemoveCacheItem, null, caseSensitive.ToString(), originalKey);
            }
        }


        /// <summary>
        /// Logs the web farm task for clear full page cache operation
        /// </summary>
        public static void LogClearFullPageCacheTask()
        {
            if (SynchronizeCache)
            {
                WebFarmHelper.CreateTask(CacheTaskType.ClearFullPageCache);
            }
        }
    }
}

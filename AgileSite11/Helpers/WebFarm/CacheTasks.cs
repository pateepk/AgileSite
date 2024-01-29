using System;
using System.Linq;

using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm synchronization for cache
    /// </summary>
    internal class CacheTasks
    {
        #region "Methods"

        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CacheTaskType.TouchCacheItem,
                Execute = TouchCacheItem,
                IsMemoryTask = true,
                OptimizationType = WebFarmTaskOptimizeActionEnum.GroupData
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CacheTaskType.RemoveCacheItem,
                Execute = RemoveCacheItem,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CacheTaskType.ClearCacheItems,
                Execute = ClearCacheItems,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CacheTaskType.ClearFullPageCache,
                Execute = ClearFullPageCache
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CacheTaskType.RemovePersistentStorageKey,
                Execute = RemovePersistentStorageCacheKey
            });
        }


        /// <summary>
        /// Clears the full page cache
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearFullPageCache(string target, string[] data, BinaryData binaryData)
        {
            CacheHelper.ClearFullPageCache(false);
        }


        /// <summary>
        /// Touches cache item
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void TouchCacheItem(string target, string[] data, BinaryData binaryData)
        {
            CacheHelper.TouchKeys(data, false, false);
        }


        /// <summary>
        /// Clears cache items
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearCacheItems(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException(String.Format("[CacheTasks.ClearCacheItems]: Expected 2 data arguments, but received {0}", data.Length));
            }

            bool caseSensitive = ValidationHelper.GetBoolean(data[0], false);
            string startsWith = data[1];
            CacheHelper.ClearCache(startsWith, caseSensitive, false);
        }


        /// <summary>
        /// Removes cache item
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveCacheItem(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException(String.Format("[CacheTasks.RemoveCacheItem]: Expected 2 data arguments, but received {0}", data.Length));
            }

            bool caseSensitive = ValidationHelper.GetBoolean(data[0], false);
            string key = data[1];
            CacheHelper.Remove(key, caseSensitive, true, false);
        }


        /// <summary>
        /// Removes key from <see cref="PersistentStorageHelper"/> internal cache, so the next request for value
        /// of given key will be taken from file system. 
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemovePersistentStorageCacheKey(string target, string[] data, BinaryData binaryData)
        {
            var cacheKeyToDelete = data[0];
            PersistentStorageHelper.RemoveKeyFromInternalCache(cacheKeyToDelete);
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;
using System.Linq;

using CMS.Core;
using CMS.IO;
using CMS.Base;

using IOExceptions = System.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides helper methods for caching.
    /// </summary>
    public class CacheHelper : AbstractHelper<CacheHelper>
    {
        #region "Constants"

        /// <summary>
        /// File node caching key.
        /// </summary>
        public const string FILENODE_KEY = "filenode";


        /// <summary>
        /// Full page caching key.
        /// </summary>
        public const string FULLPAGE_KEY = "fullpage";


        /// <summary>
        /// CSS caching key.
        /// </summary>
        public const string CSS_KEY = "css";


        /// <summary>
        /// Partial cache key.
        /// </summary>
        public const string PARTIAL_KEY = "partial";

        /// <summary>
        /// Macro cache key.
        /// </summary>
        public const string MACRO_KEY = "macrocachekey";

        /// <summary>
        /// No cache dependencies constant.
        /// </summary>
        public const string NO_CACHE_DEPENDENCIES = "##NONE##";


        /// <summary>
        /// Default cache dependencies constant.
        /// </summary>
        public const string DEFAULT_CACHE_DEPENDENCIES = "##DEFAULT##";


        /// <summary>
        /// Dummy item.
        /// </summary>
        public static object DUMMY_KEY = new DummyItem();


        /// <summary>
        /// Cache item separator.
        /// </summary>
        public static char SEPARATOR = '|';


        /// <summary>
        /// Identifies default cache dependencies macro in string of cache dependencies
        /// </summary>
        private static Regex mDefaultDependenciesRegex;

        #endregion


        #region "Variables"

        /// <summary>
        /// Cache minutes for caching the system data in API. 1 hour by default
        /// </summary>
        public static int API_CACHE_MINUTES = 60;

        /// <summary>
        /// Cache item priority.
        /// </summary>
        public static CacheItemPriority CacheItemPriority = CacheItemPriority.High;


        /// <summary>
        /// If true, progressive caching is enabled, meaning that two threads accessing the same code share the result of an internal operation
        /// </summary>
        protected static bool? mProgressiveCaching = null;

        // Track cache dependencies for output filter cache.
        private static bool? mTrackCacheDependencies;

        // Determines if files are always cached on client, even outside the live site.
        private static bool? mAlwaysCacheFiles;

        // Determines if resources are always cached on client, even outside the live site.
        private static bool? mAlwaysCacheResources;

        // Determines the expiration time for cached physical files (JS/CSS resources).
        private static int? mPhysicalFilesCacheMinutes;

        // If true, the cache allows separate keys by user name.
        private static bool? mAllowCacheByUserName;

        // If true, the cache allows separate keys by culture.
        private static bool? mAllowCacheByCulture;

        // File access lock object
        private static readonly object fileLock = new object();

        // Persistent data directory.
        private readonly StringAppSetting mPersistentDirectory = new StringAppSetting("CMSPersistentCacheDirectory", Path.Combine(PersistentStorageHelper.PersistentDirectory, "Cache"));

        private static readonly Lazy<IPerformanceCounter> mRemoved = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mExpired = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mDependencyChanged = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mUnderused = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);

        #endregion


        #region "Static properties"

        /// <summary>
        /// If true, progressive caching is enabled, meaning that two threads accessing the same code share the result of an internal operation
        /// </summary>
        public static bool ProgressiveCaching
        {
            get
            {
                if (mProgressiveCaching == null)
                {
                    mProgressiveCaching = CoreServices.Settings["CMSProgressiveCaching"].ToBoolean(false);
                }

                return mProgressiveCaching.Value;
            }
            set
            {
                mProgressiveCaching = value;
            }
        }


        /// <summary>
        /// If true, multiple cache prefixes were used
        /// </summary>
        public static bool MultiplePrefixesUsed
        {
            get;
            private set;
        }


        /// <summary>
        /// Current context name
        /// </summary>
        public static string CurrentCachePrefix
        {
            get
            {
                if (MultiplePrefixesUsed)
                {
                    // Get the current context
                    return (string)AbstractStockHelper<RequestStockHelper>.GetItem("CurrentCachePrefix", true);
                }

                // Automatically use only single context if no context was initialized
                return null;
            }
            set
            {
                // Set the engine to use multiple contexts
                if (!String.IsNullOrEmpty(value))
                {
                    MultiplePrefixesUsed = true;
                }

                if (MultiplePrefixesUsed)
                {
                    // Set the current context
                    AbstractStockHelper<RequestStockHelper>.Add("CurrentCachePrefix", (object)value, true);
                }
            }
        }


        /// <summary>
        /// Persistent data directory.
        /// </summary>
        public static string PersistentDirectory
        {
            get
            {
                return HelperObject.PersistentDirectoryInternal;
            }
            set
            {
                HelperObject.PersistentDirectoryInternal.Value = value;
            }
        }


        /// <summary>
        /// Removed items count (removed correctly by the system).
        /// </summary>
        public static IPerformanceCounter Removed
        {
            get
            {
                return mRemoved.Value;
            }
        }


        /// <summary>
        /// Underused items count (removed sooner for memory reasons).
        /// </summary>
        public static IPerformanceCounter Underused
        {
            get
            {
                return mUnderused.Value;
            }
        }


        /// <summary>
        /// DependencyChanged items count (removed by the dependency).
        /// </summary>
        public static IPerformanceCounter DependencyChanged
        {
            get
            {
                return mDependencyChanged.Value;
            }
        }


        /// <summary>
        /// Expired items count (removed by the system for expiration reasons).
        /// </summary>
        public static IPerformanceCounter Expired
        {
            get
            {
                return mExpired.Value;
            }
        }


        /// <summary>
        /// Current request dependency list, if set the list is used for the cache dependencies of the page output.
        /// </summary>
        public static CacheDependencyList CurrentRequestDependencyList
        {
            get
            {
                return (CacheDependencyList)AbstractStockHelper<RequestStockHelper>.GetItem("CurrentRequestDependencyList", true);
            }
            set
            {
                AbstractStockHelper<RequestStockHelper>.Add("CurrentRequestDependencyList", value, true);
            }
        }


        /// <summary>
        /// Returns true if the cache dependencies for the output cache are tracked.
        /// </summary>
        public static bool TrackCacheDependencies
        {
            get
            {
                if (mTrackCacheDependencies == null)
                {
                    mTrackCacheDependencies = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSOutputTrackCacheDependencies"], false);
                }

                return mTrackCacheDependencies.Value;
            }
            set
            {
                mTrackCacheDependencies = value;
            }
        }


        /// <summary>
        /// If true, the cache allows separate keys by user name.
        /// </summary>
        public static bool AllowCacheByUserName
        {
            get
            {
                if (mAllowCacheByUserName == null)
                {
                    mAllowCacheByUserName = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowCacheByUserName"], true);
                }

                return mAllowCacheByUserName.Value;
            }
        }


        /// <summary>
        /// If true, the cache allows separate keys by culture.
        /// </summary>
        public static bool AllowCacheByCulture
        {
            get
            {
                if (mAllowCacheByCulture == null)
                {
                    mAllowCacheByCulture = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowCacheByCulture"], true);
                }

                return mAllowCacheByCulture.Value;
            }
        }


        /// <summary>
        /// Returns the base cache key (created with all parameters considered to be valid for proper caching).
        /// </summary>
        public static string BaseCacheKey
        {
            get
            {
                return GetBaseCacheKey(true, true);
            }
        }


        /// <summary>
        /// Identifies default cache dependencies macro in string of cache dependencies
        /// </summary>
        private static Regex DefaultDependenciesRegex
        {
            get
            {
                return mDefaultDependenciesRegex ?? (mDefaultDependenciesRegex = RegexHelper.GetRegex(String.Format("([\n\r]*{0}[\n\r]*$)|({0}[\n\r]*)", DEFAULT_CACHE_DEPENDENCIES)));
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Persistent data directory.
        /// </summary>
        protected StringAppSetting PersistentDirectoryInternal
        {
            get
            {
                return mPersistentDirectory;
            }
        }

        #endregion


        #region "Settings retrieval"

        /// <summary>
        /// Gets if files are always cached on client, even outside the live site.
        /// </summary>
        public static bool AlwaysCacheFiles
        {
            get
            {
                if (mAlwaysCacheFiles == null)
                {
                    mAlwaysCacheFiles = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheFiles"], true);
                }

                return mAlwaysCacheFiles.Value;
            }
        }


        /// <summary>
        /// Gets if resources are always cached on client, even outside the live site.
        /// </summary>
        public static bool AlwaysCacheResources
        {
            get
            {
                if (mAlwaysCacheResources == null)
                {
                    mAlwaysCacheResources = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAlwaysCacheResources"], true);
                }

                return mAlwaysCacheResources.Value;
            }
        }


        /// <summary>
        /// Gets the expiration time in minutes that should be set for the physical files in the client cache.
        /// </summary>        
        public static int PhysicalFilesCacheMinutes
        {
            get
            {
                if (mPhysicalFilesCacheMinutes == null)
                {
                    // Default is 1 week
                    mPhysicalFilesCacheMinutes = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSPhysicalFilesCacheMinutes"], 10080);
                }

                return mPhysicalFilesCacheMinutes.Value;
            }
        }


        /// <summary>
        /// Gets if the client specified cache settings in request.
        /// </summary>
        public static bool ClientCacheRequested
        {
            get
            {
                return QueryHelper.GetBoolean("clientcache", true);
            }
        }


        /// <summary>
        /// Cache content minutes.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int CacheMinutes(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSCacheMinutes"].ToInteger(0);
        }


        /// <summary>
        /// Cache image minutes.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int CacheImageMinutes(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSCacheImages"].ToInteger(0);
        }


        /// <summary>
        /// Client cache minutes for the processed content (both files and pages).
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int ClientCacheMinutes(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSClientCacheMinutes"].ToInteger(0);
        }


        /// <summary>
        /// Returns whether the client cache should be revalidated by the call to the server.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool RevalidateClientCache(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSRevalidateClientCache"].ToBoolean(false);
        }


        /// <summary>
        /// Maximum size of the file that is allowed to be cached in kilobytes.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int MaxCacheFileSize(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSMaxCacheFileSize"].ToInteger(0);
        }


        /// <summary>
        /// Returns true if the content caching is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool CacheEnabled(string siteName)
        {
            return (CacheMinutes(siteName) > 0);
        }


        /// <summary>
        /// Returns true if the image caching is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool CacheImageEnabled(string siteName)
        {
            return (CacheImageMinutes(siteName) > 0);
        }


        /// <summary>
        /// Checks if the file attachment is below the maximum file size used for caching.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="filesize">File size in bytes</param>
        public static bool CacheImageAllowed(string siteName, int filesize)
        {
            return (filesize <= MaxCacheFileSize(siteName) * 1024);
        }

        #endregion


        #region "Cache item name methods"

        /// <summary>
        /// Returns the base cache key (created with all parameters considered to be valid for proper caching).
        /// </summary>
        public static string GetBaseCacheKey(bool userName, bool cultureCode)
        {
            return GetCacheItemName(null, (userName ? GetUserCacheKey() : null), (cultureCode ? GetCultureCacheKey() : null));
        }


        /// <summary>
        /// Gets the cache key for the given culture
        /// </summary>
        /// <param name="culture">Culture</param>
        public static string GetCultureCacheKey(string culture)
        {
            return AllowCacheByCulture ? culture : null;
        }


        /// <summary>
        /// Gets the cache key for current culture
        /// </summary>
        public static string GetCultureCacheKey()
        {
            return AllowCacheByCulture ? GetCultureCacheKey(CultureHelper.GetPreferredCulture()) : null;
        }


        /// <summary>
        /// Gets the cache key for current user the given user
        /// </summary>
        public static string GetUserCacheKey(string userName)
        {
            return AllowCacheByUserName ? userName : null;
        }


        /// <summary>
        /// Gets the cache key for current user
        /// </summary>
        public static string GetUserCacheKey()
        {
            return AllowCacheByUserName ? GetUserCacheKey(RequestContext.UserName.ToLowerInvariant()) : null;
        }


        /// <summary>
        /// Gets the cache item string.
        /// </summary>
        /// <param name="customName">Custom cache item name</param>
        /// <param name="parts">Parts of the key if the custom name is empty or null</param>
        public static string GetCacheItemName(string customName, params object[] parts)
        {
            return GetCacheItemName(true, customName, parts);
        }


        /// <summary>
        /// Gets the cache item string.
        /// </summary>
        /// <param name="lowerCase">Make the key lowercase</param>
        /// <param name="customName">Custom cache item name</param>
        /// <param name="parts">Parts of the key if the custom name is empty or null</param>
        public static string GetCacheItemName(bool lowerCase, string customName, params object[] parts)
        {
            if (!String.IsNullOrEmpty(customName))
            {
                return customName;
            }
            else
            {
                return BuildCacheItemName(parts, lowerCase);
            }
        }


        /// <summary>
        /// Builds the cache item name from the given parts
        /// </summary>
        /// <param name="parts">Parts to build</param>
        /// <param name="lowerCase">Lower case the result</param>
        public static string BuildCacheItemName(IEnumerable parts, bool lowerCase = true)
        {
            // Build the cache key
            var sb = new StringBuilder();

            bool first = true;

            foreach (object part in parts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    // Parameter separator
                    sb.Append(SEPARATOR);
                }

                // Append the next parameter if not empty
                if (part != null)
                {
                    sb.Append(part);
                }
            }

            var name = sb.ToString();
            if (lowerCase)
            {
                // Get lowercase key
                return name.ToLowerInvariant();
            }

            // Get normal key
            return name;
        }

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Wraps the operation to a cached section. This method supports progressive caching and is able to distribute unhandled exceptions to other simultaneously running threads.
        /// </summary>
        /// <param name="loadMethod">Method that loads the data</param>
        /// <param name="settings">Cache settings</param>
        public static TData Cache<TData>(Func<CacheSettings, TData> loadMethod, CacheSettings settings)
        {
            TData result = default(TData);

            // Wrap the action to a cached section
            using (var cs = new CachedSection<TData>(ref result, settings))
            {
                cs.LoadDataHandled(s => result = loadMethod(s));
            }

            return result;
        }


        /// <summary>
        /// Wraps the operation to a cached section. This method supports progressive caching and is able to distribute unhandled exceptions to other simultaneously running threads.
        /// </summary>
        /// <param name="loadMethod">Method that loads the data</param>
        /// <param name="settings">Cache settings</param>
        public static TData Cache<TData>(Func<TData> loadMethod, CacheSettings settings)
        {
            return Cache(_ => loadMethod(), settings);
        }


        /// <summary>
        /// Mirror to Cache.Add().
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Cache absolute expiration</param>
        /// <param name="slidingExpiration">Cache sliding expiration</param>
        [HideFromDebugContext]
        public static void Add(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            Add(key, value, dependencies, absoluteExpiration, slidingExpiration, CacheItemPriority);
        }


        /// <summary>
        /// Mirror to Cache.Add().
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Cache absolute expiration</param>
        /// <param name="slidingExpiration">Cache sliding expiration</param>
        /// <param name="priority">Cache priority</param>
        /// <param name="onCacheRemoveCallback">Cache callback on remove</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        [HideFromDebugContext]
        public static void Add(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onCacheRemoveCallback = null, bool caseSensitive = false)
        {
            HelperObject.AddInternal(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onCacheRemoveCallback, caseSensitive, true, true);
        }


        /// <summary>
        /// Gets item from cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <returns>object from cache</returns>
        public static object GetItem(string key, bool caseSensitive = false)
        {
            return GetItem(key, caseSensitive, true, true);
        }


        /// <summary>
        /// Gets item from cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="logOperation">Log the cache operation</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        private static object GetItem(string key, bool caseSensitive, bool logOperation, bool useFullKey)
        {
            // Manage case sensitivity
            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            // Get the value from cache
            object value = HelperObject.GetInternal(key, useFullKey);
            if (value != null)
            {
                // Get the real value
                value = GetInnerValue(value);

                // Log the get operation
                if (logOperation)
                {
                    CacheDebug.LogCacheOperation(CacheOperation.GET, key, value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, useFullKey);
                }

                return value;
            }

            return null;
        }


        /// <summary>
        /// Gets the inner value for the cache item
        /// </summary>
        /// <param name="value">Value to get</param>
        public static object GetInnerValue(object value)
        {
            // Handle the cache item container
            if (value is CacheItemContainer)
            {
                value = ((CacheItemContainer)value).Data;
            }

            // Handle the persistent cache items
            if (value is IPersistentCacheItem)
            {
                value = ((IPersistentCacheItem)value).Value;
            }

            if (value == DBNull.Value)
            {
                value = null;
            }

            return value;
        }


        /// <summary>
        /// Returns true if the cache contains the item with specified key.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        private static bool PrivateContains(string key, bool caseSensitive, bool useFullKey)
        {
            // Manage case sensitivity
            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            return (HelperObject.GetInternal(key, useFullKey) != null);
        }


        /// <summary>
        /// Returns true if the cache contains the item with specified key.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Returning the value of the object</param>
        [HideFromDebugContext]
        public static bool TryGetItem<OutputType>(string key, out OutputType value)
        {
            return TryGetItem(key, false, out value);
        }


        /// <summary>
        /// Returns true if the cache contains the item with specified key.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Returning the value of the object</param>
        [HideFromDebugContext]
        public static bool TryGetItem(string key, out object value)
        {
            return TryGetItem(key, false, out value, true);
        }


        /// <summary>
        /// Returns true if the cache contains the item with specified key.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="output">Returning the value of the object</param>
        /// <param name="logOperation">Log the cache operation</param>
        [HideFromDebugContext]
        public static bool TryGetItem<OutputType>(string key, bool caseSensitive, out OutputType output, bool logOperation = true)
        {
            // Manage case sensitivity
            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            // Get the value
            object value = HelperObject.GetInternal(key, true);
            bool found = (value != null);

            // Get the inner value
            value = GetInnerValue(value);

            // Log the get operation
            if (found && logOperation)
            {
                CacheDebug.LogCacheOperation(CacheOperation.GET, key, value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, true);
            }

            // Assign the output value
            if (value != null)
            {
                output = (OutputType)value;
            }
            else
            {
                output = default(OutputType);
            }

            return found;
        }


        /// <summary>
        /// Returns true if the cache contains the item with specified key.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="value">Returning the value of the object</param>
        [HideFromDebugContext]
        public static bool TryGetItem(string key, bool caseSensitive, out object value)
        {
            return TryGetItem(key, caseSensitive, out value, true);
        }


        /// <summary>
        /// Removes object from cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="logOperation">If true, the operation is logged</param>
        /// <param name="logTask">Log web farm task</param>
        [HideFromDebugContext]
        public static void Remove(string key, bool caseSensitive = false, bool logOperation = true, bool logTask = true)
        {
            string originalKey = key;

            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            if (HelperObject.GetInternal(key, true) != null)
            {
                HelperObject.RemoveInternal(key, true);
            }

            // Log the operation
            if (logOperation)
            {
                CacheDebug.LogCacheOperation(CacheOperation.REMOVE, key, null, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, true);
            }

            // Create web farm task if the cache is to be synchronized
            if (logTask)
            {
                CacheSynchronization.LogRemoveTask(originalKey, caseSensitive);
            }
        }


        /// <summary>
        /// Gets the full cache key with the prefix
        /// </summary>
        /// <param name="key">Cache key</param>
        internal static string GetFullKey(string key)
        {
            return HelperObject.GetFullKeyInternal(key);
        }


        /// <summary>
        /// Clears the entire system cache.
        /// </summary>
        public static void ClearCache()
        {
            ClearCache(null, true);

            // Collect the memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        /// <summary>
        /// Clears the cache content starting with given string. Clears also full page cache (including persistent) when called on website.
        /// </summary>
        /// <param name="startsWith">If null, removes all cache items, if set, remove only items starting with given string</param>
        /// <param name="caseSensitive">Case sensitivity indicator.</param>
        /// <param name="logTask">Log web farm task</param>
        public static void ClearCache(string startsWith, bool caseSensitive = false, bool logTask = true)
        {
            // Create web farm task if the cache is to be synchronized
            if (logTask)
            {
                CacheSynchronization.LogClearCacheTask(startsWith, caseSensitive);
            }

            if (!caseSensitive && (startsWith != null))
            {
                startsWith = startsWith.ToLowerInvariant();
            }

            var keyList = new List<string>();
            var cacheEnumerator = HelperObject.GetEnumeratorInternal();

            // Build the items list
            while (cacheEnumerator.MoveNext())
            {
                string key = cacheEnumerator.Key.ToString();
                if ((startsWith == null) || key.StartsWith(startsWith, StringComparison.InvariantCulture))
                {
                    keyList.Add(cacheEnumerator.Key.ToString());
                }
            }

            // Remove the items
            foreach (string key in keyList)
            {
                Remove(key, caseSensitive, false, false);
            }

            // Clear output cache when called from website
            // Output cache should not be cleared when calling from non-website application (eg. Smart search worker role)
            if ((startsWith == null) && SystemContext.IsWebSite)
            {
                ClearFullPageCache(false);
            }
        }


        /// <summary>
        /// Clear the CSS cache
        /// </summary>
        public static void ClearCSSCache()
        {
            TouchKey(CSS_KEY);
        }


        /// <summary>
        /// Clear the full page cache (output cache) of the pages.
        /// </summary>
        /// <param name="logTask">If true, web farm task is logged for this operation</param>
        public static void ClearFullPageCache(bool logTask = true)
        {
            HelperObject.ClearFullPageCacheInternal(logTask);
        }


        /// <summary>
        /// Clear the cache for the file document nodes.
        /// </summary>
        public static void ClearFileNodeCache(string siteName)
        {
            if (!String.IsNullOrEmpty(siteName))
            {
                TouchKey(FILENODE_KEY + "|" + siteName.ToLowerInvariant());
            }
            else
            {
                TouchKey(FILENODE_KEY);
            }
        }


        /// <summary>
        /// Clears the output cache for specific page.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="culture">Culture</param>
        /// <param name="allCultures">Clear all cultures cache</param>
        /// <param name="childNodes">If true, the output cache of the child nodes is cleared as well</param>
        public static void ClearOutputCache(int siteId, string aliasPath, string culture, bool allCultures, bool childNodes)
        {
            // Current document
            string key = allCultures ? GetCacheItemName(null, FULLPAGE_KEY, siteId, aliasPath) : GetCacheItemName(null, FULLPAGE_KEY, siteId, aliasPath, culture);

            TouchKey(key);

            // Child documents
            if (childNodes)
            {
                key = GetCacheItemName(null, FULLPAGE_KEY, siteId, aliasPath, "childnodes");
                TouchKey(key);
            }
        }


        /// <summary>
        /// Clear the partial cache (output cache) of the controls.
        /// </summary>
        public static void ClearPartialCache()
        {
            TouchKey(PARTIAL_KEY);
        }


        /// <summary>
        /// Registers a cache callback for given list of cache dependencies
        /// </summary>
        /// <param name="key">Cache key to use</param>
        public static void RemoveDependencyCallback(string key)
        {
            Remove(key);
        }


        /// <summary>
        /// Registers a cache callback for given list of cache dependencies
        /// </summary>
        /// <param name="key">Cache key to use</param>
        /// <param name="cd">Cache dependency to which bind the handler</param>
        /// <param name="target">Target object for the callback</param>
        /// <param name="handler">Callback handler</param>
        /// <param name="parameter">Callback parameter</param>
        /// <param name="useManagedThread">If true, managed thread (CMSThread) is used for the callback</param>
        public static void RegisterDependencyCallback<TTarget>(string key, CMSCacheDependency cd, TTarget target, Action<TTarget, object> handler, object parameter, bool useManagedThread = true)
            where TTarget : class
        {
            if (key == null)
            {
                key = "cachecallback|" + Guid.NewGuid();
            }

            // Wrap the callback into a CMSThread - callback creates anonymous thread
            if (useManagedThread)
            {
                handler = CMSThread.Wrap(handler);
            }

            var h = new CacheDependencyCallback<TTarget>(key, target, handler, parameter);

            // Add the new key representing the callback
            Add(key, h, cd, DateTime.Now.AddYears(1), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable);
        }


        /// <summary>
        /// Registers the automatic callback that is executed at the specific time
        /// </summary>
        /// <param name="key">Cache key to leverage</param>
        /// <param name="when">Time of execution</param>
        /// <param name="callback">Callback method</param>
        /// <param name="useManagedThread">If true, managed thread (CMSThread) is used for the callback</param>
        public static void RegisterAutomaticCallback(string key, DateTime when, Action callback, bool useManagedThread = true)
        {
            // Wrap the callback to CMSThread - Creates anonymous thread
            if (useManagedThread)
            {
                callback = CMSThread.Wrap(callback);
            }

            Add(key, null, null, when, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, (s, value, reason) => callback());
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="key">Key to touch</param>
        [HideFromDebugContext]
        public static void TouchKey(string key)
        {
            TouchKey(key, true, false, DateTime.Now.AddYears(1), true);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="key">Key to touch</param>
        /// <param name="logTask">Log web farm task</param>
        /// <param name="ensureKey">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        [HideFromDebugContext]
        public static void TouchKey(string key, bool logTask, bool ensureKey)
        {
            TouchKey(key, logTask, ensureKey, DateTime.Now.AddYears(1), true);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="key">Key to touch</param>
        /// <param name="logTask">Log web farm task</param>
        /// <param name="ensureKey">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        /// <param name="expiration">Expiration time</param>
        [HideFromDebugContext]
        public static void TouchKey(string key, bool logTask, bool ensureKey, DateTime expiration)
        {
            TouchKey(key, logTask, ensureKey, expiration, true);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="key">Key to touch</param>
        /// <param name="logTask">Log web farm task</param>
        /// <param name="ensureKey">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="logOperation">Log the operation</param>
        [HideFromDebugContext]
        public static void TouchKey(string key, bool logTask, bool ensureKey, DateTime expiration, bool logOperation)
        {
            HelperObject.TouchKeyInternal(key, logTask, ensureKey, expiration, logOperation);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="keys">Key array to touch</param>
        [HideFromDebugContext]
        public static void TouchKeys(IEnumerable<string> keys)
        {
            TouchKeys(keys, true, false);
        }


        /// <summary>
        /// Touches the cache keys separated by new line to drop the dependencies.
        /// </summary>
        /// <param name="allKeys">Key list to touch</param>
        /// <param name="logTasks">Log web farm tasks</param>
        /// <param name="ensureKeys">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        [HideFromDebugContext]
        public static void TouchKeys(string allKeys, bool logTasks, bool ensureKeys)
        {
            string[] keys = allKeys.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            TouchKeys(keys, logTasks, ensureKeys);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="keys">Key array to touch</param>
        /// <param name="logTasks">Log web farm tasks</param>
        /// <param name="ensureKeys">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        [HideFromDebugContext]
        public static void TouchKeys(IEnumerable<string> keys, bool logTasks, bool ensureKeys)
        {
            HelperObject.TouchKeysInternal(keys, logTasks, ensureKeys);
        }


        /// <summary>
        /// Processes the dependencies string. Returns default dependencies if the source is null or empty. 
        /// If source is NO_CACHE_DEPENDENCY constant, then method also returns this constant. 
        /// When source is specified and contains DEFAULT_CACHE_DEPENDENCIES constant, then this constant
        /// indicates that default dependencies should be appended to the source. The DEFAULT_CACHE_DEPENDENCIES also remains
        /// in the string so other processes can insert their default dependencies.
        /// </summary>
        /// <param name="dependencies">Source dependencies</param>
        /// <param name="defaultDependencies">Default dependencies</param>
        public static string GetCacheDependencies(object dependencies, string defaultDependencies)
        {
            string dep = ValidationHelper.GetString(dependencies, String.Empty);

            // Do nothing
            if (dep.Contains(NO_CACHE_DEPENDENCIES))
            {
                return NO_CACHE_DEPENDENCIES;
            }

            // Remove default dependencies macros from default dependencies
            if (!String.IsNullOrEmpty(defaultDependencies))
            {
                defaultDependencies = DefaultDependenciesRegex.Replace(defaultDependencies, "");
            }

            // Return only default dependencies
            if (String.IsNullOrEmpty(dep))
            {
                if (!String.IsNullOrEmpty(defaultDependencies))
                {
                    dep = defaultDependencies + '\n';
                }

                return (dep + DEFAULT_CACHE_DEPENDENCIES);
            }

            // Add default cache dependencies, keep default in dependencies so other control can append their defaults
            if (!String.IsNullOrEmpty(defaultDependencies) && dep.Contains(DEFAULT_CACHE_DEPENDENCIES))
            {
                // Remove all default dependencies macros
                dep = DefaultDependenciesRegex.Replace(dep, "");

                // Insert default dependencies
                // Keep default dependencies macro in dependencies so other controls can append their defaults
                dep += String.Format("\n{0}\n{1}", defaultDependencies, DEFAULT_CACHE_DEPENDENCIES);
            }

            return dep;
        }


        /// <summary>
        /// Parses the string and returns the array of the cache dependency keys.
        /// </summary>
        /// <param name="keys">Cache keys as a single string separated by new lines</param>
        public static string[] GetDependencyCacheKeys(string keys)
        {
            if (keys == null)
            {
                return null;
            }

            // Split into the lines
            string[] result = keys.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return result;
        }


        /// <summary>
        /// Creates the cache dependency from the given file path.
        /// </summary>
        /// <param name="path">File path</param>
        public static CMSCacheDependency GetFileCacheDependency(string path)
        {
            return GetCacheDependency(new[] { path }, null);
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="keys">Cache keys</param>
        public static CMSCacheDependency GetCacheDependency(string keys)
        {
            return GetCacheDependency(GetDependencyCacheKeys(keys));
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="keys">Cache keys</param>
        public static CMSCacheDependency GetCacheDependency(string[] keys)
        {
            return GetCacheDependency(null, keys);
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="keys">Cache keys</param>
        public static CMSCacheDependency GetCacheDependency(ICollection<string> keys)
        {
            return GetCacheDependency(null, keys);
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="files">Files for the cache dependency</param>
        /// <param name="keys">Cache keys</param>
        public static CMSCacheDependency GetCacheDependency(List<string> files, ICollection<string> keys)
        {
            // Prepare file list
            string[] fileList = null;
            if ((files != null) && (files.Count > 0))
            {
                fileList = files.ToArray();
            }

            // Prepare key list
            string[] keyList = null;
            if ((keys != null) && (keys.Count > 0))
            {
                keyList = keys.ToArray();
            }

            return GetCacheDependency(fileList, keyList);
        }


        /// <summary>
        /// Creates the cache dependency from the given keys.
        /// </summary>
        /// <param name="files">Files for the cache dependency</param>
        /// <param name="keys">Cache keys</param>
        public static CMSCacheDependency GetCacheDependency(string[] files, string[] keys)
        {
            if ((keys == null) && (files == null))
            {
                return null;
            }

            if (keys != null)
            {
                // Check if the request dependencies should be tracked automatically
                CacheDependencyList currentList = null;
                if (TrackCacheDependencies)
                {
                    currentList = CurrentRequestDependencyList;
                }

                // Prepare the records
                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];
                    switch (key)
                    {
                        case null:
                        case NO_CACHE_DEPENDENCIES:
                        case DEFAULT_CACHE_DEPENDENCIES:
                            // Do not process dependency constants or empty ones
                            keys[i] = "";
                            key = "";
                            break;

                        default:
                            // Convert key name to lower case
                            key = key.ToLowerInvariant();

                            // Track current request dependencies
                            if (currentList != null)
                            {
                                currentList.Add(key);
                            }
                            break;
                    }

                    // Ensure the key
                    EnsureDummyKey(key);
                }
            }

            var cd = HelperObject.CreateCacheDependencyInternal(files, keys);

            return cd;
        }


        /// <summary>
        /// Ensures the dummy key with the given name
        /// </summary>
        /// <param name="key">Key to ensure</param>
        public static void EnsureDummyKey(string key)
        {
            // Ensure the key
            EnsureKey(key, DateTime.Now.AddYears(1));
        }


        /// <summary>
        /// Ensures the specified cache key.
        /// </summary>
        /// <param name="key">Key to ensure</param>
        /// <param name="expiration">Expiration time</param>
        public static void EnsureKey(string key, DateTime expiration)
        {
            if (!PrivateContains(key, true, false))
            {
                TouchKey(key, false, true, expiration, false);
            }
        }


        /// <summary>
        /// Callback to report removed items from the cache.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="removedReason">Reason for removal</param>
        private static void ReportRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
            switch (removedReason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                case CacheItemRemovedReason.Removed:
                    {
                        // Handle the cache item container
                        var container = value as CacheItemContainer;
                        if (container != null)
                        {
                            value = container.Data;
                        }

                        // Handle the removal of persistent items
                        var persistentItem = value as IPersistentCacheItem;
                        if (persistentItem != null)
                        {
                            if (!SystemContext.ApplicationTerminating)
                            {
                                // Allow empty context for this operation - Cache callback isn't initiated from request
                                CMSThread.AllowEmptyContext();

                                DeletePersistent(persistentItem.CacheKey, persistentItem.SiteName);
                            }
                        }
                        else
                        {
                            // Handle the registered callback
                            var callback = value as ICacheDependencyCallback;
                            if (callback != null)
                            {
                                // Allow empty context for this operation - Cache callback isn't initiated from request
                                CMSThread.AllowEmptyContext();

                                callback.PerformCallback();
                            }
                        }

                        if (removedReason == CacheItemRemovedReason.DependencyChanged)
                        {
                            DependencyChanged.Increment(null);
                        }
                        else
                        {
                            Removed.Increment(null);
                        }
                    }
                    break;

                case CacheItemRemovedReason.Expired:
                    Expired.Increment(null);
                    break;

                case CacheItemRemovedReason.Underused:
                    Underused.Increment(null);
                    break;
            }
        }


        /// <summary>
        /// Adds the given dependency cache keys to the Response cache.
        /// </summary>
        /// <param name="keys">Keys to add as the dependencies</param>
        public static void AddResponseCacheDependencies(string[] keys)
        {
            // Add the cache dependency
            CMSCacheDependency cd = GetCacheDependency(keys);
            if (cd != null)
            {
                HttpContext.Current.Response.AddCacheDependency(cd.CacheDependency);
            }
        }


        /// <summary>
        /// Adds the given keys as a dependency for current page output.
        /// </summary>
        public static CacheDependencyList AddOutputCacheDependencies(params string[] keys)
        {
            // Ensure the list
            CacheDependencyList list = CurrentRequestDependencyList;
            if (list == null)
            {
                list = new CacheDependencyList();

                CurrentRequestDependencyList = list;
            }

            // Add the given dependencies to the list
            list.Add(keys);

            return list;
        }


        /// <summary>
        /// Ensures the dependency list for the given key, so the dependencies for current page output can be tracked. The dependencies are tracked automatically only when TrackCacheDependencies property is enabled.
        /// </summary>
        /// <param name="listKey">List key</param>
        public static CacheDependencyList EnsureOutputCacheDependencies(string listKey)
        {
            // Check current list
            CacheDependencyList list = CurrentRequestDependencyList;
            if (list == null)
            {
                // Ensure new list for the dependencies
                list = CacheDependencyList.EnsureList(listKey);

                CurrentRequestDependencyList = list;
            }

            return list;
        }


        /// <summary>
        /// Gets collection of cache items defined in settings
        /// </summary>
        /// <param name="values">Items stored in system settings</param>
        private static Dictionary<string, bool> GetSettingsCacheItems(String values)
        {
            // Create output dictionary object
            Dictionary<string, bool> itemsDict = null;

            // Generate dictionary from string (format: name,value;name,value;....)
            if (!String.IsNullOrEmpty(values))
            {
                itemsDict = values.Split(';')
                    .Select(x => x.Split(','))
                    .ToDictionary(x => x[0], x => ValidationHelper.GetBoolean(x[1], false));
            }
            return itemsDict;
        }


        /// <summary>
        /// Gets combined cache items for setting and default items
        /// </summary>
        /// <param name="values">Items stored in system settings</param>
        /// <param name="items">Basic item collection defined in the code</param>
        public static Dictionary<string, bool> GetCombinedCacheItems(String values, Dictionary<string, bool> items)
        {
            // Try get setting values
            Dictionary<string, bool> itemsDict = GetSettingsCacheItems(values);

            // Result values
            Dictionary<string, bool> resultDict = new Dictionary<string, bool>();

            // Loop thru available items
            foreach (KeyValuePair<string, bool> obj in items)
            {
                // If exist in settings add setting value
                if ((itemsDict != null) && itemsDict.ContainsKey(obj.Key))
                {
                    resultDict.Add(obj.Key, itemsDict[obj.Key]);
                }
                // Otherwise use default value
                else
                {
                    resultDict.Add(obj.Key, obj.Value);
                }
            }

            return resultDict;
        }


        /// <summary>
        /// Gets the cache items string (item names separated by semicolon)
        /// </summary>
        /// <param name="items">Basic item collection defined in the code</param>
        public static string GetCacheItemsString(Dictionary<string, bool> items)
        {
            // Result string
            StringBuilder sbResult = new StringBuilder();

            // Loop thru items and generate output string
            foreach (var item in items)
            {
                if (item.Value)
                {
                    sbResult.Append(item.Key, ';');
                }
            }

            // Return result and remove trailing semicolon
            return sbResult.ToString().TrimEnd(';');
        }

        #endregion


        #region "Persistent cache methods"

        /// <summary>
        /// Saves the persistent item to the file
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="data">Data to cache</param>
        /// <param name="cacheMinutes">Cache minutes for the standard cache item</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="fileExpires">Expiration time for the physical file</param>
        /// <param name="cd">Cache dependency</param>
        /// <param name="siteName">Site name</param>
        public static PersistentCacheItem<DataType> AddPersistent<DataType>(string cacheKey, DataType data, CMSCacheDependency cd, int cacheMinutes, DateTime expires, DateTime fileExpires, string siteName)
        {
            // Save to the file system
            PersistentCacheItem<DataType> item = new PersistentCacheItem<DataType>(cacheKey, data, cd, cacheMinutes, expires, fileExpires, siteName);
            HelperObject.SaveToPersistentFileInternal(item, true);

            return item;
        }


        /// <summary>
        /// Restores the persistent item from the cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        /// <param name="data">Returning the cached data if found</param>
        public static bool TryRestorePersistent<DataType>(string cacheKey, string siteName, out DataType data)
        {
            data = default(DataType);

            // Get locking object
            var lockObj = LockHelper.GetLockObject("restorepersistent|" + cacheKey);
            bool lockAcquired = false;

            try
            {
                // Check if the thread can read and lock the context
                if (lockObj.EnterRead(ref data))
                {
                    lockAcquired = true;

                    // Get from the file system
                    PersistentCacheItem<DataType> item = HelperObject.GetFromPersistentFileInternal<DataType>(null, cacheKey, siteName, true);

                    if ((item != null) && (item.FileExpires > DateTime.Now))
                    {
                        // Prepare the expiration time
                        DateTime expires = item.Expires;
                        if (expires == System.Web.Caching.Cache.NoAbsoluteExpiration)
                        {
                            // Prolong by cache minutes
                            expires = DateTime.Now.AddMinutes(item.CacheMinutes);
                        }

                        // Check if the expiration is in future
                        if (expires > DateTime.Now)
                        {
                            data = item.Data;

                            // Ensure the dummy keys for dependencies
                            if (item.CacheDependencies != null)
                            {
                                item.CacheDependencies = GetCacheDependency(item.CacheDependencies.FileNames, item.CacheDependencies.CacheKeys);
                            }

                            // Add to the regular cache as persistent item
                            Add(cacheKey, item, item.CacheDependencies, expires, System.Web.Caching.Cache.NoSlidingExpiration);

                            return true;
                        }
                    }
                }
            }
            finally
            {
                // Finish the reading and save data for other threads
                if (lockAcquired)
                {
                    lockObj.FinishRead(data);
                }
            }

            return false;
        }


        /// <summary>
        /// Restores the persistent item from the cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        public static void DeletePersistent(string cacheKey, string siteName)
        {
            HelperObject.DeletePersistentInternal(null, cacheKey, siteName, true);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Creates the cache dependency with specified parameters
        /// </summary>
        /// <param name="files">Dependency files</param>
        /// <param name="keys">Dependency cache keys</param>
        protected virtual CMSCacheDependency CreateCacheDependencyInternal(string[] files, string[] keys)
        {
            return new CMSCacheDependency(files, keys, DateTime.Now);
        }


        /// <summary>
        /// Gets the full cache key with the prefix
        /// </summary>
        /// <param name="key">Cache key</param>
        protected virtual string GetFullKeyInternal(string key)
        {
            string prefix = CurrentCachePrefix;
            if (String.IsNullOrEmpty(prefix))
            {
                return key;
            }

            return String.Format("{0}-{1}", prefix, key);
        }


        /// <summary>
        /// Inserts the item into the cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Absolute expiration time</param>
        /// <param name="slidingExpiration">Sliding expiration interval</param>
        /// <param name="priority">Cache priority</param>
        /// <param name="onRemoveCallback">Callback called on the removal of the item</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual void InsertInternal(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                key = GetFullKeyInternal(key);
            }

            CacheDependency dep = null;
            if (dependencies != null)
            {
                dep = dependencies.CacheDependency;
            }

            // Insert the item to the cache
            HttpRuntime.Cache.Insert(key, value, dep, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }


        /// <summary>
        /// Mirror to Cache.Add().
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Cache absolute expiration</param>
        /// <param name="slidingExpiration">Cache sliding expiration</param>
        /// <param name="priority">Cache priority</param>
        /// <param name="onCacheRemoveCallback">Cache callback on remove</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        /// <param name="logOperation">Log the cache operation</param>
        /// <param name="useFullKey">If true, the add operation uses the full cache key which includes the context prefix</param>
        protected virtual void AddInternal(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onCacheRemoveCallback, bool caseSensitive, bool logOperation, bool useFullKey)
        {
            // Use DBNull to allow caching of null values
            if (value == null)
            {
                value = DBNull.Value;
            }
            // Manage case sensitivity
            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            if (onCacheRemoveCallback == null)
            {
                onCacheRemoveCallback = ReportRemovedCallback;
            }

            // Logs cache to the file
            if (logOperation)
            {
                CacheDebug.LogCacheOperation(CacheOperation.ADD, key, value, dependencies, absoluteExpiration, slidingExpiration, priority, useFullKey);
            }

            // Encapsulate the item into the cache container
            value = new CacheItemContainer(value, dependencies, absoluteExpiration, slidingExpiration, priority);

            // Insert the item to the cache
            InsertInternal(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onCacheRemoveCallback, useFullKey);
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="key">Key to touch</param>
        /// <param name="logTask">Log web farm task</param>
        /// <param name="ensureKey">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="logOperation">Log the operation</param>
        protected virtual void TouchKeyInternal(string key, bool logTask, bool ensureKey, DateTime expiration, bool logOperation)
        {
            // Add the new key
            if (ensureKey || PrivateContains(key, false, false))
            {
                AddInternal(key, DUMMY_KEY, null, expiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null, false, false, false);
            }

            // Create web farm task if the cache is to be synchronized
            if (logTask)
            {
                CacheSynchronization.LogTouchKeysTask(key);
            }

            // Log the operation
            if (logOperation)
            {
                CacheDebug.LogCacheOperation(CacheOperation.TOUCH, key, null, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, false);
            }
        }


        /// <summary>
        /// Touches the cache key to drop the dependencies.
        /// </summary>
        /// <param name="keys">Key array to touch</param>
        /// <param name="logTasks">Log web farm tasks</param>
        /// <param name="ensureKeys">If true, the keys must be present in the cache, if false, the non-existing key is not touched</param>
        protected virtual void TouchKeysInternal(IEnumerable<string> keys, bool logTasks, bool ensureKeys)
        {
            if (keys == null)
            {
                return;
            }

            // Touch all keys
            foreach (string key in keys)
            {
                if (!String.IsNullOrEmpty(key))
                {
                    TouchKey(key, false, ensureKeys, DateTime.Now.AddYears(1), false);
                }
            }

            // Synchronize web farm
            if (logTasks)
            {
                CacheSynchronization.LogTouchKeysTask(keys);
            }

            // Log the operation
            if (CacheDebug.DebugCurrentRequest)
            {
                string allKeys = keys.Join("\n");
                CacheDebug.LogCacheOperation(CacheOperation.TOUCH, allKeys, null, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, false);
            }
        }


        /// <summary>
        /// Gets the item from the cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual object GetInternal(string key, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                key = GetFullKeyInternal(key);
            }

            return HttpRuntime.Cache[key];
        }


        /// <summary>
        /// Removes the item from the cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual object RemoveInternal(string key, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                key = GetFullKeyInternal(key);
            }

            return HttpRuntime.Cache.Remove(key);
        }


        /// <summary>
        /// Gets the enumerator for the cache items.
        /// </summary>
        protected virtual IDictionaryEnumerator GetEnumeratorInternal()
        {
            return HttpRuntime.Cache.GetEnumerator();
        }


        /// <summary>
        /// Saves the persistent item to the file
        /// </summary>
        /// <param name="item">Item to save</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual void SaveToPersistentFileInternal<DataType>(PersistentCacheItem<DataType> item, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                item.CacheKey = GetFullKeyInternal(item.CacheKey);
            }

            string fileName = GetPersistentFilePathInternal(item.CacheKey, item.SiteName);

            lock (fileLock)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                    CacheDebug.LogCacheOperation(CacheOperation.ADD_PERSISTENT, item.CacheKey, item.Data, item.CacheDependencies, item.Expires, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority, true);

                    using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(PersistentCacheItem<DataType>));
                        xs.Serialize(fs, item);
                    }
                }
                catch (IOExceptions.IOException)
                {
                    // Suppress IO exceptions. There is a potential issue with shared storage.
                }
                catch (Exception ex)
                {
                    CoreServices.EventLog.LogException("FileSystemCache", "SAVETOFILE", ex);
                }
            }
        }


        /// <summary>
        /// Gets the persistent item from file
        /// </summary>
        /// <param name="fileName">File name for the storage, prioritized</param>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual PersistentCacheItem<DataType> GetFromPersistentFileInternal<DataType>(string fileName, string cacheKey, string siteName, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                cacheKey = GetFullKeyInternal(cacheKey);
            }

            // Prepare the file name
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = GetPersistentFilePathInternal(cacheKey, siteName);
            }

            if (File.Exists(fileName))
            {
                lock (fileLock)
                {
                    try
                    {
                        PersistentCacheItem<DataType> item = null;

                        // Read the object from file
                        using (var fs = File.OpenRead(fileName))
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(PersistentCacheItem<DataType>));
                            item = (PersistentCacheItem<DataType>)xs.Deserialize(fs);
                        }

                        // Check validity
                        if (item != null)
                        {
                            if (item.CacheKey != cacheKey)
                            {
                                // Cache key doesn't match (different data storage), do not perform any action
                                return null;
                            }

                            if (item.FileExpires > DateTime.Now)
                            {
                                CacheDebug.LogCacheOperation(CacheOperation.GET_PERSISTENT, item.CacheKey, item.Data, item.CacheDependencies, item.Expires, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority, true);

                                // Not expired, get the item
                                return item;
                            }
                            else
                            {
                                CacheDebug.LogCacheOperation(CacheOperation.REMOVE_PERSISTENT, item.CacheKey, null, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority, true);

                                try
                                {
                                    // Expired, delete the file
                                    File.Delete(fileName);
                                }
                                catch
                                {
                                    // Suppress error due to potential issue on shared storage
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Suppress error due to potential issue on shared storage
                        try
                        {
                            // May be a problem with file structure, try to delete the file to recover
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                        }
                        catch
                        {
                            // Suppress error due to potential issue on shared storage
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Restores the persistent item from the cache
        /// </summary>
        /// <param name="fileName">File name, if set, takes priority over the cache key</param>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        /// <param name="useFullKey">If true, the full cache key is used</param>
        protected virtual void DeletePersistentInternal(string fileName, string cacheKey, string siteName, bool useFullKey)
        {
            // Get the full cache key
            if (useFullKey)
            {
                cacheKey = GetFullKeyInternal(cacheKey);
            }

            // Prepare the file name
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = GetPersistentFilePathInternal(cacheKey, siteName);
            }

            lock (fileLock)
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        CacheDebug.LogCacheOperation(CacheOperation.REMOVE_PERSISTENT, cacheKey, null, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority, true);

                        File.Delete(fileName);
                    }
                    catch
                    {
                        // Suppress error
                    }
                }
            }
        }


        /// <summary>
        /// Gets the file name for the given cache key
        /// </summary>
        /// <param name="cacheKey">Full cache key</param>
        /// <param name="siteName">Site name</param>
        protected virtual string GetPersistentFilePathInternal(string cacheKey, string siteName)
        {
            string fileName;

            // Parse out the folder
            int separatorIndex = cacheKey.IndexOf(SEPARATOR);
            if (separatorIndex > 0)
            {
                fileName = ValidationHelper.GetSafeFileName(cacheKey.Substring(0, separatorIndex));
            }
            else
            {
                fileName = "_general";
            }

            // Convert the rest of the key to file name
            cacheKey = GetPersistentFileName(cacheKey);

            if (String.IsNullOrEmpty(siteName))
            {
                siteName = "_Global";
            }

            return String.Concat(PersistentDirectory, "\\", siteName, "\\", fileName, "\\", cacheKey, ".cache");
        }


        /// <summary>
        /// Gets the persistent file name for the given cache key
        /// </summary>
        /// <param name="cacheKey">Full cache key</param>
        protected virtual string GetPersistentFileName(string cacheKey)
        {
            return String.Format("{1:X4}\\{0:X12}", cacheKey.GetHashCode().ToString().Replace('-', 'm'), cacheKey.Length);
        }


        /// <summary>
        /// Clear the full page cache (output cache) of the pages.
        /// </summary>
        /// <param name="logTask">If true, web farm task is logged for this operation</param>
        protected virtual void ClearFullPageCacheInternal(bool logTask)
        {
            // Clear file system cache
            ClearFullPageCacheFilesInternal();

            // Clear in memory .Net Cache
            TouchKey(FULLPAGE_KEY, false, false);

            CacheDependencyList.ClearLists();

            if (logTask)
            {
                CacheSynchronization.LogClearFullPageCacheTask();
            }
        }


        /// <summary>
        /// Deletes all files in persistent file system cache (extension of output cache)
        /// </summary>
        protected virtual void ClearFullPageCacheFilesInternal()
        {
            try
            {
                if (Directory.Exists(PersistentDirectoryInternal))
                {
                    var dir = DirectoryInfo.New(PersistentDirectoryInternal);
                    dir.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(file =>
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (IOExceptions.IOException)
                        {
                            // Skip files that cannot be deleted.
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("FileSystemCache", "CLEARCACHE", ex);
            }
        }

        #endregion
    }
}
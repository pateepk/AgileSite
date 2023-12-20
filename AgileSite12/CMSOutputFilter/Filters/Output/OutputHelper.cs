using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;

using CMS.AspNet.Platform.Cache.Extension;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.IO;

using HttpCacheability = System.Web.HttpCacheability;
using HttpCacheRevalidation = System.Web.HttpCacheRevalidation;
using IOExceptions = System.IO;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Output helper.
    /// </summary>
    public class OutputHelper : AbstractHelper<OutputHelper>
    {
        #region "Variables"

        // Use output filter caching?
        private static bool? mUseOutputFilterCache;

        // Available cache items
        private static Dictionary<string, bool> mAvailableCacheItemNames;

        // List of the cache key items.
        internal static string mCacheItems;

        // If true, output cache is cleared on postback.
        private static bool? mClearOutputCacheOnPostback;

        // File access lock object
        private static readonly object fileLock = new object();

        ///<summary>
        /// Cache item priority.
        /// </summary>
        public static CMSCacheItemPriority CacheItemPriority = CMSCacheItemPriority.High;

        #endregion


        #region "Public properties"

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
        /// Persistent data directory.
        /// </summary>
        protected StringAppSetting PersistentDirectoryInternal
        {
            get;
        } = new StringAppSetting("CMSPersistentCacheDirectory", Path.Combine(PersistentStorageHelper.PersistentDirectory, "Cache"));


        /// <summary>
        /// Returns true if full client cache is enabled (no revalidation requests).
        /// </summary>
        public static bool UseOutputFilterCache
        {
            get
            {
                if (mUseOutputFilterCache == null)
                {
                    mUseOutputFilterCache = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSOutputFilterCache"], true);
                }

                return mUseOutputFilterCache.Value;
            }
            set
            {
                mUseOutputFilterCache = value;
            }
        }


        /// <summary>
        /// Gets list of cache items supported by system
        /// </summary>
        public static Dictionary<string, bool> AvailableCacheItemNames
        {
            get
            {
                if (mAvailableCacheItemNames == null)
                {
                    Dictionary<string, bool> items = new Dictionary<string, bool>();

                    // Items enabled by default
                    items.Add("username", true);
                    items.Add("sitename", true);
                    items.Add("lang", true);
                    items.Add("browser", true);
                    items.Add("cookielevel", true);
                    items.Add("deviceprofile", true);

                    // Items disabled by default
                    items.Add("domain", false);
                    items.Add("viewmode", false);

                    mAvailableCacheItemNames = items;
                }

                return mAvailableCacheItemNames;
            }
        }


        /// <summary>
        /// List of the cache key items separated by semicolon. Defaults to "username;sitename;lang;browser;cookielevel;deviceprofile". Other available values are: domain, viewmode
        /// </summary>
        public static string CacheItems
        {
            get
            {
                if (mCacheItems == null)
                {
                    // Prioritize keys defined in web.config section
                    string appItems = SettingsHelper.AppSettings["CMSOutputCacheItems"];
                    if (!String.IsNullOrEmpty(appItems))
                    {
                        mCacheItems = appItems;
                    }
                    // Get keys from settings (combine with new keys defined in system)
                    else
                    {
                        Dictionary<string, bool> combined = CacheHelper.GetCombinedCacheItems(SettingsKeyInfoProvider.GetValue("CMSOutputCacheItems"), AvailableCacheItemNames);
                        mCacheItems = CacheHelper.GetCacheItemsString(combined);
                    }
                }

                return mCacheItems;
            }
            set
            {
                mCacheItems = value;
            }
        }


        /// <summary>
        /// If true, output cache is cleared on postback.
        /// </summary>
        public static bool ClearOutputCacheOnPostback
        {
            get
            {
                if (mClearOutputCacheOnPostback == null)
                {
                    mClearOutputCacheOnPostback = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSClearOutputCacheOnPostback"], true);
                }

                return mClearOutputCacheOnPostback.Value;
            }
            set
            {
                mClearOutputCacheOnPostback = value;
            }
        }

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Returns true if the output caching is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool EnableOutputCache(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableOutputCache");
        }


        /// <summary>
        /// Returns the number of minutes for which the output cache should be stored in the file system
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int FileSystemOutputCacheMinutes(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSFileSystemOutputCacheMinutes");
        }


        /// <summary>
        /// Gets the output cache key for the request.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        public static string GetOutputCacheKey(ViewModeOnDemand viewMode, SiteNameOnDemand siteName)
        {
            return HelperObject.GetOutputCacheKeyInternal(viewMode, siteName);
        }


        /// <summary>
        /// Saves the output data to the cache.
        /// </summary>
        /// <param name="outputData">Output to save</param>
        public static void SaveOutputToCache(OutputData outputData)
        {
            HelperObject.SaveOutputToCacheInternal(outputData);
        }


        /// <summary>
        /// Sends the page output from the output cache.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        /// <param name="output">Return cached output</param>
        public static bool SendOutputFromCache(ViewModeOnDemand viewMode, SiteNameOnDemand siteName, out CachedOutput output)
        {
            return HelperObject.SendOutputFromCacheInternal(viewMode, siteName, out output);
        }


        /// <summary>
        /// Sets the page caching.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="currentPage">Current page info</param>
        /// <param name="response">Response object</param>
        public static bool SetCaching(string siteName, PageInfo currentPage, HttpResponse response)
        {
            bool result = false;

            if (currentPage != null)
            {
                // Cache control
                int cacheMinutes = currentPage.NodeCacheMinutes;
                if (cacheMinutes < 0)
                {
                    cacheMinutes = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSCacheMinutes");
                }

                ViewModeOnDemand viewMode = new ViewModeOnDemand();
                if ((cacheMinutes > 0) && (viewMode.IsLiveSite()))
                {
                    if (!RequestHelper.IsPostBack())
                    {
                        if (UseOutputFilterCache)
                        {
                            // Let output filter handle the cache
                            OutputFilterContext.CurrentOutputCacheMinutes = cacheMinutes;

                            // Track the cache dependencies for the page
                            SiteNameOnDemand site = new SiteNameOnDemand();
                            site.Value = siteName;

                            string key = GetOutputCacheKey(viewMode, site);

                            // Track the dependencies
                            if (CacheHelper.TrackCacheDependencies)
                            {
                                CacheDependencyList list = CacheHelper.EnsureOutputCacheDependencies(key);

                                // Add default dependencies
                                list.Add(currentPage.GetResponseCacheDependencies());
                            }
                        }
                        else
                        {
                            // Set the cacheMinutes for the URL rewriter
                            OutputFilterContext.CurrentNETOutputCacheMinutes = cacheMinutes;

                            // Set the expiration
                            DateTime expiration = DateTime.Now.AddMinutes(cacheMinutes);
                            if (currentPage.DocumentPublishTo < expiration)
                            {
                                expiration = currentPage.DocumentPublishTo;
                            }

                            // Set the caching
                            var cache = response.Cache;
                            cache.SetCacheability(HttpCacheability.Server);
                            cache.SetValidUntilExpires(true);
                            cache.SetExpires(expiration);
                            cache.VaryByParams["*"] = true;
                            cache.SetVaryByCustom(CacheItems); //domain

                            // Add output cache dependencies
                            var cacheDependency = CacheHelper.GetCacheDependency(currentPage.GetResponseCacheDependencies());
                            if (cacheDependency != null)
                            {
                                CMSHttpContext.Current.Response.AddCacheDependency(cacheDependency.CreateCacheDependency());
                            }
                        }
                    }
                    else if (ClearOutputCacheOnPostback)
                    {
                        // Clear page output cache on postback
                        currentPage.ClearOutputCache(true);
                    }

                    result = true;
                }
                else
                {
                    response.Cache.SetNoStore();
                }
            }
            else
            {
                response.Cache.SetNoStore();
            }

            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            return result;
        }


        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="data">Cached data</param>
        /// <param name="cd">Cache dependency</param>
        public static void AddToCache(CachedOutput data, CMSCacheDependency cd)
        {
            HelperObject.AddToCacheInternal(data, cd);
        }


        /// <summary>
        /// Custom cache parameters processing for the full page cache and web part partial cache.
        /// </summary>
        public static string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (context == null)
            {
                return "";
            }

            HttpResponse response = context.Response;
            bool isPartialCacheControl = custom.StartsWith("control;", StringComparison.Ordinal);
            bool preserveCachedOutputOnPostback = custom.EndsWith(";preserveonpostback", StringComparison.OrdinalIgnoreCase);

            // Full page cache: Do not cache on postback
            // Partial cache: Do not cache on postback unless explicitly specified
            //     - generating new GUID will cause that the partial cache will use a fresh cache item which won't be cached later.
            if (RequestHelper.IsPostBack() && !preserveCachedOutputOnPostback)
            {
                response.Cache.SetNoStore();
                return Guid.NewGuid().ToString();
            }

            PageInfo currentPage = DocumentContext.CurrentPageInfo;
            string result;

            // Full page cache
            if ((currentPage != null) && !isPartialCacheControl)
            {
                // Check page caching minutes
                int cacheMinutes = currentPage.NodeCacheMinutes;
                if (cacheMinutes <= 0)
                {
                    // Do not cache
                    response.Cache.SetNoStore();
                    return Guid.NewGuid().ToString();
                }
            }

            SiteNameOnDemand siteName = new SiteNameOnDemand();
            ViewModeOnDemand viewMode = new ViewModeOnDemand();

            // Parse the custom parameters
            string contextString = GetContextCacheString(custom, viewMode, siteName);
            if (contextString == null)
            {
                // Do not cache
                response.Cache.SetNoStore();
                return Guid.NewGuid().ToString();
            }
            else
            {
                result = "cached" + contextString;
            }

            return result.ToLowerInvariant();
        }


        /// <summary>
        /// Gets the context cache string for the given set of items.
        /// </summary>
        /// <param name="items">Items to use in the cache string</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        public static string GetContextCacheString(string items, ViewModeOnDemand viewMode, SiteNameOnDemand siteName)
        {
            StringBuilder sb = new StringBuilder();

            items = items.ToLowerInvariant();

            // Parse the custom parameters
            string[] customs = items.Split(';');
            foreach (string cust in customs)
            {
                switch (cust)
                {
                    // View mode
                    case "viewmode":
                        if (viewMode.Value != ViewModeEnum.LiveSite)
                        {
                            // Return null in meaning not cache
                            return null;
                        }
                        break;

                    // User name
                    case "username":
                        if (CacheHelper.AllowCacheByUserName)
                        {
                            sb.Append("|username=");
                            sb.Append(RequestContext.UserName);
                        }
                        break;

                    // Site name
                    case "sitename":
                        {
                            sb.Append("|sitename=");
                            sb.Append(siteName.Value);
                        }
                        break;

                    // Language
                    case "lang":
                    case "culture":
                        if (CacheHelper.AllowCacheByCulture)
                        {
                            sb.Append("|lang=");
                            sb.Append(CultureHelper.GetPreferredCulture());
                        }
                        break;

                    // Browser
                    case "browser":
                        {
                            sb.Append("|browser=");
                            sb.Append(BrowserHelper.GetBrowserClass());
                        }
                        break;

                    // Domain name (including port)
                    case "domain":
                        {
                            sb.Append("|domain=");
                            sb.Append(RequestContext.FullDomain);
                        }
                        break;

                    // GZip
                    case "gzip":
                        {
                            bool gzip = (RequestHelper.AllowGZip && RequestHelper.IsGZipSupported());
                            sb.Append("|gzip=");
                            sb.Append(gzip);
                        }
                        break;

                    // Cookie level
                    case "cookielevel":
                        {
                            var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();

                            sb.Append("|cookielevel=");
                            sb.Append(cookieLevelProvider.GetCurrentCookieLevel());
                        }
                        break;

                    // Device profile
                    case "deviceprofile":
                        {
                            sb.Append("|deviceprofile=");
                            sb.Append(DeviceContext.CurrentDeviceProfileName);
                        }
                        break;

                    // Cached control
                    case "control":
                        sb.Append("control");
                        break;

                    // This custom value serves only as an indicator for reseting the VaryByCustom string
                    case "preserveonpostback":
                        break;

                    // Other items - just add to the cache item string (supports macros)
                    default:
                        if (!String.IsNullOrEmpty(cust))
                        {
                            var result = MacroProcessor.ContainsMacro(cust) ? MacroResolver.Resolve(cust) : cust;
                            sb.Append(result);
                        }
                        break;
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Attempts to get the cached output from the cache. Returns true if output was found in cache. Otherwise false.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        /// <param name="output">Returning output</param>
        public static bool TryGetFromCache(string cacheKey, string siteName, out CachedOutput output)
        {
            return HelperObject.TryGetFromCacheInternal(cacheKey, siteName, out output);
        }


        /// <summary>
        /// Clears <see cref="CacheItems"/> and optionally logs a web farm task to propagate the change.
        /// </summary>
        /// <param name="logTask">A value indicating whether to log a web farm task.</param>
        internal static void ClearCacheItems(bool logTask = true)
        {
            CacheItems = null;

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearCacheItemsWebFarmTask());
            }
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
                        if (expires == Cache.NoAbsoluteExpiration)
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
                                item.CacheDependencies = CacheHelper.GetCacheDependency(item.CacheDependencies.FileNames, item.CacheDependencies.CacheKeys);
                            }

                            // Add to the regular cache as persistent item
                            CacheHelper.Add(cacheKey, item, item.CacheDependencies, expires, Cache.NoSlidingExpiration);

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


        /// <summary>
        /// Deletes all files in persistent file system cache (extension of output cache)
        /// </summary>
        internal static void ClearFullPageCacheFiles()
        {
            HelperObject.ClearFullPageCacheFilesInternal();
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
                item.CacheKey = CacheHelper.GetFullKey(item.CacheKey);
            }

            string fileName = GetPersistentFilePathInternal(item.CacheKey, item.SiteName);

            lock (fileLock)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                    CacheDebug.LogCacheOperation(CacheOperation.ADD_PERSISTENT, item.CacheKey, item.Data, item.CacheDependencies, item.Expires, Cache.NoSlidingExpiration, CacheItemPriority, true);

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
                cacheKey = CacheHelper.GetFullKey(cacheKey);
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
                                CacheDebug.LogCacheOperation(CacheOperation.GET_PERSISTENT, item.CacheKey, item.Data, item.CacheDependencies, item.Expires, Cache.NoSlidingExpiration, CacheItemPriority, true);

                                // Not expired, get the item
                                return item;
                            }
                            else
                            {
                                CacheDebug.LogCacheOperation(CacheOperation.REMOVE_PERSISTENT, item.CacheKey, null, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority, true);

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
                cacheKey = CacheHelper.GetFullKey(cacheKey);
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
                        CacheDebug.LogCacheOperation(CacheOperation.REMOVE_PERSISTENT, cacheKey, null, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority, true);

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
            int separatorIndex = cacheKey.IndexOf(CacheHelper.SEPARATOR);
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

        #region "Internal methods"

        /// <summary>
        /// Gets the output cache key for the request.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        protected virtual string GetOutputCacheKeyInternal(ViewModeOnDemand viewMode, SiteNameOnDemand siteName)
        {
            string contextString = GetContextCacheString(CacheItems, viewMode, siteName);
            if (contextString == null)
            {
                // Do not cache
                return null;
            }
            else
            {
                // Valid cache string
                return CacheHelper.GetCacheItemName(null, "outputdata", RequestContext.CurrentScheme, CMSHttpContext.Current.Request.RawUrl, contextString);
            }
        }


        /// <summary>
        /// Saves the output data to the cache.
        /// </summary>
        /// <param name="outputData">Output to save</param>
        protected virtual void SaveOutputToCacheInternal(OutputData outputData)
        {
            // Check if the output should be cached
            int cacheMinutes = OutputFilterContext.CurrentOutputCacheMinutes;
            if (cacheMinutes <= 0)
            {
                return;
            }

            // Check if output cache is enabled
            SiteNameOnDemand siteName = new SiteNameOnDemand();
            if (!EnableOutputCache(siteName.Value))
            {
                return;
            }

            ViewModeOnDemand viewMode = new ViewModeOnDemand();

            // Add to the cache
            string key = GetOutputCacheKey(viewMode, siteName);
            if (key == null)
            {
                return;
            }

            // Prepare the cache item
            CachedOutput output = new CachedOutput();
            output.OutputData = outputData;
            output.AliasPath = DocumentContext.CurrentAliasPath;
            output.OriginalAliasPath = DocumentContext.OriginalAliasPath;
            output.Status = RequestContext.CurrentStatus;
            output.HttpStatusCode = CMSHttpContext.Current.Response.StatusCode;

            // Set current URL with query parameters
            output.URL = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);

            PageInfo currentPage = DocumentContext.CurrentPageInfo;

            // Set the expiration
            DateTime expiration = currentPage.DocumentPublishTo;

            output.CachePageInfo = currentPage;

            // Set conversions and campaigns
            output.DocumentConversionValue = currentPage.DocumentConversionValue;
            output.DocumentTrackConversionName = currentPage.DocumentTrackConversionName;

            // Get cache dependency list
            CacheDependencyList list = CacheHelper.CurrentRequestDependencyList ?? CacheHelper.AddOutputCacheDependencies(currentPage.GetResponseCacheDependencies());

            // Add dependencies specific for Content personalization
            if ((list != null)
                && PortalContext.ContentPersonalizationEnabled
                && PortalContext.ContentPersonalizationVariantsEnabled
                && (currentPage.UsedPageTemplateInfo != null))
            {
                list.Add("om.personalizationvariant|bytemplateid|" + currentPage.UsedPageTemplateInfo.PageTemplateId);
            }

            bool allowFs = currentPage.NodeAllowCacheInFileSystem;

            output.SiteName = siteName.Value;
            output.CacheMinutes = cacheMinutes;
            output.CacheKey = key;
            output.Expiration = expiration;
            output.CacheInFileSystem = allowFs;
            output.CacheDependencies = list;

            // Add the output to cache
            using (var h = OutputFilterEvents.SaveOutputToCache.StartEvent(output))
            {
                if (h.CanContinue())
                {
                    var cd = output.CacheDependencies != null ? output.CacheDependencies.GetCacheDependency() : CacheHelper.GetCacheDependency(currentPage.GetResponseCacheDependencies());

                    // Save to the cache
                    AddToCacheInternal(output, cd);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="data">Cached data</param>
        /// <param name="cd">Cache dependency</param>
        protected virtual void AddToCacheInternal(CachedOutput data, CMSCacheDependency cd)
        {
            object value = data;

            if (data.CacheInFileSystem)
            {
                int fileCacheMinutes = FileSystemOutputCacheMinutes(data.SiteName);
                if (fileCacheMinutes > 0)
                {
                    // Add to the persistent cache
                    value = AddPersistent(data.CacheKey, data, cd, data.CacheMinutes, data.Expiration, DateTime.Now.AddMinutes(fileCacheMinutes), data.SiteName);
                }
            }

            // Set the expiration for the standard cache item
            SetCacheExpiration(data, DateTime.Now);

            // Save to the cache
            CacheHelper.Add(data.CacheKey, value, cd, data.Expiration, Cache.NoSlidingExpiration, CacheItemPriority, null, true);
        }


        /// <summary>
        /// Sets the expiration time either to the interval specified in page settings or to the value set in <see cref="CachedOutput.Expiration"/> depending on which is closer to the current time.
        /// </summary>
        /// <param name="data">Data to set expiration for.</param>
        /// <param name="now">Current date time.</param>
        internal static void SetCacheExpiration(CachedOutput data, DateTime now)
        {
            var unpublishInterval = data.Expiration - now;
            var settingsInterval = TimeSpan.FromMinutes(data.CacheMinutes);
            if (settingsInterval < unpublishInterval)
            {
                data.Expiration = now.AddMinutes(data.CacheMinutes);
            }
        }


        /// <summary>
        /// Attempts to get the cached output from the cache. Returns true if output was found in cache. Otherwise false.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="siteName">Site name</param>
        /// <param name="output">Returning output</param>
        protected virtual bool TryGetFromCacheInternal(string cacheKey, string siteName, out CachedOutput output)
        {
            // Try to get from normal cache
            if (!CacheHelper.TryGetItem(cacheKey, true, out output))
            {
                int cacheMinutes = FileSystemOutputCacheMinutes(siteName);
                if (cacheMinutes > 0)
                {
                    // Try to restore persistent cache
                    return TryRestorePersistent(cacheKey, siteName, out output);
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Sends the page output from the output cache.
        /// </summary>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        /// <param name="output">Return cached output</param>
        protected virtual bool SendOutputFromCacheInternal(ViewModeOnDemand viewMode, SiteNameOnDemand siteName, out CachedOutput output)
        {
            output = null;

            // Do not use cached output on postback
            if (RequestHelper.IsPostBack())
            {
                return false;
            }

            // Get the output from the cache
            string key = GetOutputCacheKey(viewMode, siteName);
            if (key != null)
            {
                // Get the output
                if (TryGetFromCacheInternal(key, siteName.Value, out output))
                {
                    if (output != null)
                    {
                        using (var h = OutputFilterEvents.SendCacheOutput.StartEvent(output, viewMode))
                        {
                            // If method should not continue, the subscriber of the event had already taken care of the sending output to the response
                            if (h.CanContinue())
                            {
                                // If event subscriber indicated that page should be loaded regularly (without cache), return false
                                if (h.EventArguments.FallbackToRegularLoad)
                                {
                                    return false;
                                }

                                // Get output from event arguments
                                output = h.EventArguments.Output;

                                var response = CMSHttpContext.Current.Response;

                                // Clear response.
                                response.Clear();

                                // Initialize content type property due to .NET 4 optimization (content type wasn't set without this initialization)
                                response.ContentType = response.ContentType;

                                // Set the cache headers
                                response.Cache.SetCacheability(HttpCacheability.Server);
                                response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

                                response.Charset = output.OutputData.Encoding.WebName;
                                response.StatusCode = output.HttpStatusCode;
                                if (response.StatusCode == 404)
                                {
                                    response.TrySkipIisCustomErrors = true;
                                }

                                OutputFilterContext.SentFromCache = true;

                                // Handle click-jacking in the output headers
                                SecurityHelper.HandleClickjacking();

                                DocumentContext.CurrentPageInfo = output.CachePageInfo;

                                // Write the output
                                long size = output.OutputData.WriteOutputToStream(response.OutputStream, true, true);

                                // Log request operation
                                RequestDebug.LogRequestOperation("SendOutputFromCache", DataHelper.GetSizeString(size), 1);

                                // Counter content page
                                RequestHelper.TotalPageRequests.Increment(siteName.Value);
                            }

                            h.FinishEvent();

                            if (OutputFilterContext.EndRequest)
                            {
                                EndRequest();
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Ends the current request
        /// </summary>
        public static void EndRequest()
        {
            if (ResponseOutputFilter.OutputFilterEndRequest)
            {
                RequestHelper.CompleteRequest();
            }
            else
            {
                RequestHelper.EndResponse();
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Caching;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.SiteProvider;

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
        private static string mCacheItems;

        // If true, output cache is cleared on postback.
        private static bool? mClearOutputCacheOnPostback;

        ///<summary>
        /// Cache item priority.
        /// </summary>
        public static CacheItemPriority CacheItemPriority = CacheItemPriority.High;

        #endregion


        #region "Public properties"

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
                        if (ResponseOutputFilter.UseOutputFilterCache)
                        {
                            // Let output filter handle the cache
                            OutputFilterContext.CurrentOutputCacheMinutes = cacheMinutes;

                            // Track the cache dependencies for the page
                            SiteNameOnDemand site = new SiteNameOnDemand();
                            site.Value = siteName;

                            string key = ResponseOutputFilter.GetOutputCacheKey(viewMode, site);

                            // Track the dependencies
                            if (ResponseOutputFilter.TrackCacheDependencies)
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
                            HttpCachePolicy cache = response.Cache;
                            cache.SetCacheability(HttpCacheability.Server);
                            cache.SetValidUntilExpires(true);
                            cache.SetExpires(expiration);
                            cache.VaryByParams["*"] = true;
                            cache.SetVaryByCustom(CacheItems); //domain

                            // Add output cache dependencies
                            currentPage.AddResponseCacheDependencies();
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
                return CacheHelper.GetCacheItemName(null, "outputdata", RequestContext.CurrentScheme, HttpContext.Current.Request.RawUrl, contextString);
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
            output.HttpStatusCode = HttpContext.Current.Response.StatusCode;

            // Set current URL with query parameters
            output.URL = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);

            // Set the expiration
            DateTime expiration = Cache.NoAbsoluteExpiration;
            PageInfo currentPage = DocumentContext.CurrentPageInfo;

            output.CachePageInfo = currentPage;

            // Set conversions and campaigns
            output.DocumentConversionValue = currentPage.DocumentConversionValue;
            output.DocumentTrackConversionName = currentPage.DocumentTrackConversionName;

            if (expiration > currentPage.DocumentPublishTo)
            {
                expiration = currentPage.DocumentPublishTo;
            }

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
                    value = CacheHelper.AddPersistent(data.CacheKey, data, cd, data.CacheMinutes, data.Expiration, DateTime.Now.AddMinutes(fileCacheMinutes), data.SiteName);
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
                    return CacheHelper.TryRestorePersistent(cacheKey, siteName, out output);
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

                                HttpResponse response = HttpContext.Current.Response;

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
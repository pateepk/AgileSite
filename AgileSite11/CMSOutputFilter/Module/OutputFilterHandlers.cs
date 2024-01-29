using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Event handlers for output filter module
    /// </summary>
    internal class OutputFilterHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += HandleOutputCache;

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.GetVaryByCustomString.Execute += GetVaryByCustomString;

                RequestEvents.PostAcquireRequestState.Execute += EndRequestByOutputFilter;
                RequestEvents.End.Execute += RestoreResponseCookies;
            }

            OutputFilterEvents.SendCacheOutput.Before += BeforeSendCacheOutput;
        }


        /// <summary>
        /// Checks whether current URL is in correct case form. If not then redirects to correct one.
        /// </summary>
        private static void BeforeSendCacheOutput(object sender, OutputCacheEventArgs outputCacheArgs)
        {
            SiteNameOnDemand siteName = new SiteNameOnDemand();
            if (!IsCaseRedirectRequired(siteName.Value))
            {
                return;
            }

            string currentUrl = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);
            var outputURL = outputCacheArgs.Output.URL;

            // If current URL and cached URL are different, use cached URL because it's in correct form - before caching
            // URL address is converted to correct form (are applied rewrite rules)
            if (CMSString.Compare(currentUrl, outputURL, StringComparison.InvariantCulture) != 0)
            {
                URLHelper.RedirectPermanent(outputURL, siteName.Value);
            }
        }

        /// <summary>
        /// Restores response cookies for current request if the output was sent from cache
        /// </summary>
        private static void RestoreResponseCookies(object sender, EventArgs e)
        {
            // Restore the response cookies if fullpage caching is set
            if ((RequestContext.CurrentStatus == RequestStatusEnum.SentFromCache) && (OutputFilterContext.CurrentNETOutputCacheMinutes > 0))
            {
                CookieHelper.RestoreResponseCookies();
            }

            // Set cookies as read-only for further usage in the request
            CookieHelper.ReadOnly = true;
        }


        /// <summary>
        /// Checks whether request should be ended for full page cached page, and ends the request if so
        /// </summary>
        private static void EndRequestByOutputFilter(object sender, EventArgs e)
        {
            if (OutputFilterContext.OutputFilterEndRequestRequired)
            {
                OutputHelper.EndRequest();
            }
        }


        /// <summary>
        /// Gets the variable part of the output cache for the given custom string
        /// </summary>
        private static void GetVaryByCustomString(object sender, GetVaryByCustomStringEventArgs e)
        {
            e.Result = OutputHelper.GetVaryByCustomString(e.Context, e.Custom);
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        private static void HandleOutputCache(object sender, SettingsKeyChangedEventArgs e)
        {
            var keyName = e.KeyName;

            // Ensure the cache items reload after change of setting key
            ClearOutputCacheItems(keyName);
            
            // Ensure the output cache is cleared after change of related settings keys
            ClearOutputCache(keyName);
        }


        /// <summary>
        /// Clears output cache if related settings key changed
        /// </summary>
        /// <param name="keyName">Setting key name</param>
        private static void ClearOutputCache(string keyName)
        {
            switch (keyName.ToLowerInvariant())
            {
                case "cmscontrolelement":
                case "cmspagedescriptionprefix":
                case "cmspagekeywordsprefix":
                case "cmspagetitleformat":
                case "cmspagetitleprefix":
                case "cmsenableoutputcache":
                case "cmsfilesystemoutputcacheminutes":
                case "cmsoutputcacheitems":
                case "cmsallowurlswithoutlanguageprefixes":
                case "cmsdefaulpage":
                case "cmsprocessdomainprefix":
                case "cmsredirectaliasestomainurl":
                case "cmsredirectinvalidcasepages":
                case "cmsredirecttomainextension":
                case "cmsusedomainforculture":
                case "cmsuselangprefixforurls":
                case "cmsuseurlswithtrailingslash":
                    CacheHelper.ClearFullPageCache();
                    break;
            }
        }


        /// <summary>
        /// Clears output cache items when appropriate settings key changed
        /// </summary>
        /// <param name="keyName"></param>
        private static void ClearOutputCacheItems(string keyName)
        {
            if (keyName.Equals("CMSOutputCacheItems", StringComparison.OrdinalIgnoreCase))
            {
                OutputHelper.CacheItems = null;
            }
        }


        /// <summary>
        /// Returns true if case redirect is required - if redirect case rules are defined in settings
        /// </summary>
        private static bool IsCaseRedirectRequired(string siteName)
        {
            return URLHelper.GetCaseRedirectEnum(siteName) != CaseRedirectEnum.None;
        }
    }
}

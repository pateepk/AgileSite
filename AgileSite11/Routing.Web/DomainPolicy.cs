using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Search;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Ensures domain policies like redirection to site alias, using of domain prefix etc.
    /// </summary>
    public class DomainPolicy
    {
        private static bool? mUseDomainRedirectForFiles;


        /// <summary>
        /// Do not process domain prefix
        /// </summary>
        public const string DOMAIN_PREFIX_NONE = "NONE";


        /// <summary>
        /// Always use www prefix for domains
        /// </summary>
        public const string DOMAIN_PREFIX_WWW = "WWW";


        /// <summary>
        /// Never use www prefix for domains
        /// </summary>
        public const string DOMAIN_PREFIX_WITHOUTWWW = "WITHOUTWWW";


        /// <summary>
        /// Provider object.
        /// </summary>
        protected static DomainPolicy ProviderObject
        {
            get
            {
                return ObjectFactory<DomainPolicy>.StaticSingleton();
            }
        }


        /// <summary>
        /// Indicates whether domain redirect should be used for file request (getattachment, getmediafile, getmetafile, etc)
        /// </summary>
        public static bool UseDomainRedirectForFiles
        {
            get
            {
                if (mUseDomainRedirectForFiles == null)
                {
                    mUseDomainRedirectForFiles = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseDomaiRedirectForFiles"], true);
                }
                return mUseDomainRedirectForFiles.Value;
            }
            set
            {
                mUseDomainRedirectForFiles = value;
            }
        }


        /// <summary>
        /// Applies domain policy regarding given site.
        /// </summary>
        /// <param name="site">Site info for which the policy is applied.</param>
        public static void Apply(SiteInfo site)
        {
            if (site == null)
            {
                return;
            }

            EnsureDomainAliasRedirection(site);
            EnsureDomainPrefix(site);
        }


        /// <summary>
        /// Applies domain policy for files regarding given site.
        /// </summary>
        /// <param name="site">Site info for which the policy is applied.</param>
        public static void ApplyForFiles(SiteInfo site)
        {
            if (site == null || !UseDomainRedirectForFiles)
            {
                return;
            }

            // Ensures site alias redirect
            if (Service.Resolve<ISiteService>().IsLiveSite && !SearchCrawler.IsCrawlerRequest())
            {
                if (site.SiteIsOffline)
                {
                    RedirectToSiteOffline(site);
                }

                Apply(site);
            }
        }


        /// <summary>
        /// Ensures redirection when domain alias redirect URL is set.
        /// </summary>
        /// <param name="site">Site info for which the domain alias redirection is applied.</param>
        internal static void EnsureDomainAliasRedirection(SiteInfo site)
        {
            // Get current domain
            string currentFullDomain = RequestContext.FullDomain;
            string currentDomain = URLHelper.RemoveWWW(currentFullDomain);

            if (site.DomainName.Equals(currentFullDomain, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var domainAliases = site.SiteDomainAliases;

            // Get domain alias
            var alias = (SiteDomainAliasInfo)domainAliases[currentFullDomain] ?? (SiteDomainAliasInfo)domainAliases[currentDomain];

            // There is URL to redirect to
            string url = GetDomainAliasRedirectionUrl(alias, currentFullDomain);

            if (!String.IsNullOrEmpty(url))
            {
                RedirectPermanent(url, site.SiteName);
            }
        }


        /// <summary>
        /// Returns domain alias redirection URL.
        /// </summary>
        /// <param name="alias">Domain alias which matches domain of the current request.</param>
        /// <param name="currentDomain">The current domain, including "www." prefix if present</param>
        /// <returns>
        /// Redirection URL when specified and redirect is necessary, <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Only absolute URL is supported for the redirection URL of a site domain alias.</exception>
        internal static string GetDomainAliasRedirectionUrl(SiteDomainAliasInfo alias, string currentDomain)
        {
            if (String.IsNullOrEmpty(alias?.SiteDomainRedirectUrl))
            {
                return null;
            }

            // Replace relative path macro
            MacroResolver resolver = MacroContext.CurrentResolver;
            string aliasRedirectionUrl = resolver.ResolveMacros(alias.SiteDomainRedirectUrl).TrimEnd('/');

            // Ensure protocol if not present 
            if (!aliasRedirectionUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                if (RequestContext.IsSSL)
                {
                    aliasRedirectionUrl = "https://" + aliasRedirectionUrl;
                }
                else
                {
                    aliasRedirectionUrl = "http://" + aliasRedirectionUrl;
                }
            }

            if (!IsAbsoluteUrl(aliasRedirectionUrl))
            {
                throw new NotSupportedException("Only absolute URL is supported for the redirection URL of a site domain alias.");
            }

            // Get domain from alias redirection URL
            string aliasRedirectionUrlDomain = URLHelper.GetDomain(aliasRedirectionUrl);

            // Prepare alias redirection URL path
            string aliasUrlPath = new Uri(aliasRedirectionUrl).AbsolutePath;

            // Prepare current URL path
            string currentUrlPath = RequestContext.URL.AbsolutePath;
            if (!currentUrlPath.Equals("/", StringComparison.Ordinal))
            {
                currentUrlPath = currentUrlPath.TrimEnd('/');
            }

            bool isRedirectionUrlDifferentFromCurrentUrl = !currentUrlPath.Equals(aliasUrlPath, StringComparison.InvariantCultureIgnoreCase) || !aliasRedirectionUrlDomain.Equals(currentDomain, StringComparison.InvariantCultureIgnoreCase);
            if (isRedirectionUrlDifferentFromCurrentUrl && !URLHelper.IsExcludedSystem(RequestContext.CurrentRelativePath))
            {
                return aliasRedirectionUrl;
            }

            return null;
        }


        private static bool IsAbsoluteUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult);
        }


        private static void EnsureDomainPrefix(SiteInfo site)
        {
            var siteName = site.SiteName;

            // Get setting for domain prefix
            string processDomainprefix = SettingsKeyInfoProvider.GetValue(siteName + ".CMSProcessDomainPrefix");

            // Switch by domain prefix preferences 
            switch (processDomainprefix)
            {
                // Always use WWW prefix
                case DOMAIN_PREFIX_WWW:
                    string domain = RequestContext.CurrentDomain;
                    if (!domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase) && !URLHelper.IsTldDomain(domain))
                    {
                        string currentUrl = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);
                        currentUrl = URLHelper.GetAbsoluteUrl(currentUrl, "www." + domain);
                        RedirectPermanent(currentUrl, siteName);
                    }
                    break;

                // Never use WWW prefix
                case DOMAIN_PREFIX_WITHOUTWWW:
                    domain = RequestContext.CurrentDomain;
                    if (domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                    {
                        string currentUrl = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);
                        currentUrl = URLHelper.GetAbsoluteUrl(currentUrl, URLHelper.RemoveWWW(domain));
                        RedirectPermanent(currentUrl, siteName);
                    }
                    break;

            }

        }


        private static void RedirectToSiteOffline(SiteInfo site)
        {
            if (String.IsNullOrEmpty(site.SiteOfflineRedirectURL))
            {
                // Display site offline message
                CMSHttpContext.Current.RewritePath("~/CMSMessages/SiteOffline.aspx", CMSHttpContext.Current.Request.PathInfo, String.Empty);
                RequestContext.ClearCachedUrlValues();

                return;
            }

            // Redirect to specific URL
            Redirect(site.SiteOfflineRedirectURL);
        }


        private static void RedirectPermanent(string url, string siteName)
        {
            ProviderObject.RedirectPermanentInternal(url, siteName);
        }


        private static void Redirect(string url)
        {
            ProviderObject.RedirectInternal(url);
        }


        internal virtual void RedirectPermanentInternal(string url, string siteName)
        {
            URLHelper.RedirectPermanent(url, siteName);
        }


        internal virtual void RedirectInternal(string url)
        {
            URLHelper.Redirect(url);
        }
    }
}

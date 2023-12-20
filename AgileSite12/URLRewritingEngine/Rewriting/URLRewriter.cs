using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.Protection;
using CMS.Protection.Web.UI;
using CMS.Routing.Web;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.WebServices;

using RequestContext = CMS.Helpers.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// URL rewriter class.
    /// </summary>
    public class URLRewriter
    {
        #region "Properties"

        /// <summary>
        /// Provider object.
        /// </summary>
        protected static URLRewriter ProviderObject
        {
            get
            {
                return ObjectFactory<URLRewriter>.StaticSingleton();
            }
        }

        #endregion


        #region "Variables"

        /// <summary>
        /// GetAttachment prefix.
        /// </summary>
        public const string PREFIX_GETATTACHMENT = "/getattachment/";


        /// <summary>
        /// GetAttachment prefix for CMS pages (pages which require authentication).
        /// </summary>
        public const string PREFIX_CMS_GETATTACHMENT = "/cms/getattachment/";


        /// <summary>
        /// GetFile prefix.
        /// </summary>
        public const string PREFIX_GETFILE = "/getfile/";


        /// <summary>
        /// GetFile prefix for CMS pages (pages which require authentication).
        /// </summary>
        public const string PREFIX_CMS_GETFILE = "/cms/getfile/";


        /// <summary>
        /// GetDoc prefix for CMS pages (pages which require authentication).
        /// </summary>
        public const string PREFIX_CMS_GETDOC = PageInfoProvider.PREFIX_CMS_GETDOC;


        private const string GETFILE_PAGE = "~/CMSPages/getfile.aspx";


        /// <summary>
        /// Prefixes for the available permanent attachment URLs.
        /// </summary>
        private static string[] mGetAttachmentPrefixes;


        /// <summary>
        /// Indicates whether node alias path should be encoded for RewritePath method.
        /// </summary>
        private static bool? mEncodeNodeAliasPath;

        /// <summary>
        /// Indicates whether culture parameter should be redirected to the culture prefix if
        /// culture prefix is enabled for current site
        /// </summary>
        private static bool? mRedirectLangParameterToPrefix;

        /// <summary>
        /// Indicates whether URL without path prefix should be redirected to the correspondent URL
        /// with path prefix
        /// </summary>
        private static bool? mRedirectURLToPathPrefix;

        /// <summary>
        /// Indicates whether default.aspx page should be redirected to the main url if it is required.
        /// </summary>
        private static bool? mUseRedirectForDefaultPage;

        /// <summary>
        /// Wildcard query mapping setting. Possible values: join replaceexisting keepexisting.
        /// </summary>
        private static string mWildcardQueryMapping;

        /// <summary>
        /// If true, the URL rewriting process attempts to fix the redirect that some module did with wrong URL by another redirect with the original URL. Set true when using cookieless mode. Default is false.
        /// </summary>
        private static bool? mFixRewriteRedirect;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the URL rewriting process attempts to fix the redirect that some module did with wrong URL by another redirect with the original URL. Set true when using cookieless mode. Default is false.
        /// </summary>
        public static bool FixRewriteRedirect
        {
            get
            {
                if (mFixRewriteRedirect == null)
                {
                    mFixRewriteRedirect = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSFixRewriteRedirect"], false);
                }

                return mFixRewriteRedirect.Value;
            }
            set
            {
                mFixRewriteRedirect = value;
            }
        }


        /// <summary>
        /// Gets the wildcard query mapping setting. Possible values: join replaceexisting keepexisting.
        /// </summary>
        private static string WildcardQueryMapping
        {
            get
            {
                return mWildcardQueryMapping ?? (mWildcardQueryMapping = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSWildcardQueryMapping"], "join").ToLowerInvariant());
            }
        }


        /// <summary>
        /// Gets the value that indicates whether default page should be redirected to the main URL.
        /// </summary>
        private static bool UseRedirectForDefaultPage
        {
            get
            {
                if (mUseRedirectForDefaultPage == null)
                {
                    mUseRedirectForDefaultPage = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseRedirectForDefaultPage"], false);
                }

                return mUseRedirectForDefaultPage.Value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether URL with lang prefix should be redirected to the
        /// culture prefix if culture prefix is enabled for current site
        /// </summary>
        public static bool RedirectLangParameterToPrefix
        {
            get
            {
                if (mRedirectLangParameterToPrefix == null)
                {
                    mRedirectLangParameterToPrefix = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRedirectLangToPrefix"], true);
                }
                return mRedirectLangParameterToPrefix.Value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether node alias path should be encoded for RewritePath method.
        /// </summary>
        private static bool EncodeNodeAliasPath
        {
            get
            {
                if (mEncodeNodeAliasPath == null)
                {
                    mEncodeNodeAliasPath = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEncodeNodeAliasPath"], true);
                }

                return mEncodeNodeAliasPath.Value;
            }
        }


        /// <summary>
        /// Prefixes for the available permanent attachment URLs.
        /// </summary>
        private static string[] GetAttachmentPrefixes
        {
            get
            {
                return mGetAttachmentPrefixes ?? (mGetAttachmentPrefixes = new[] { PREFIX_GETATTACHMENT, PREFIX_CMS_GETATTACHMENT });
            }
        }

        #endregion


        #region "URL Rewriting"

        /// <summary>
        /// Returns sitemap URL for specific site
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetGoogleSiteMapURL(string siteName)
        {
            return "/" + SettingsKeyInfoProvider.GetValue(siteName + ".CMSGoogleSitemapURL").TrimStart('/').ToLowerCSafe();
        }


        /// <summary>
        /// Returns sitemap node alias path for specific site
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetGoogleSiteMapPath(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSSitemapPath");
        }


        /// <summary>
        /// Gets the value that indicates whether domain should be used for culture
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseDomainForCulture(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseDomainForCulture");
        }


        /// <summary>
        /// Gets the default page behavior for given site
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string DefaultPageBehavior(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaulPage");
        }


        /// <summary>
        /// If true, aliases of the document (URLPath, DocumentAlias) are redirected to the main document URL (only if not wildcard).
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static bool RedirectAliasesToMainURL(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRedirectAliasesToMainURL");
        }


        /// <summary>
        /// If true, URLs without language prefix will be redirected to the correspondent page with lang prefix.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static bool RedirectURLToLangPrefix(string siteName)
        {
            return URLHelper.UseLangPrefixForUrls(siteName) && !SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowUrlsWithoutLanguagePrefixes");
        }


        /// <summary>
        /// If true, URLs without path prefix will be redirected to the correspondent page with path prefix.
        /// </summary>
        private static bool RedirectURLToPathPrefix()
        {
            if (mRedirectURLToPathPrefix == null)
            {
                mRedirectURLToPathPrefix = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRedirectUrlToPathPrefix"], true);
            }
            return mRedirectURLToPathPrefix.Value;
        }


        /// <summary>
        /// Performs the redirect that was previously planned by URL rewriter
        /// </summary>
        public static void PerformPlannedRedirect()
        {
            string redirectUrl = URLRewritingContext.PlannedRedirectUrl;
            if (!String.IsNullOrEmpty(redirectUrl))
            {
                Redirect(redirectUrl);
            }
        }


        /// <summary>
        /// Handles the rewrite redirect of the request
        /// </summary>
        public static bool HandleRewriteRedirect()
        {
            var context = CMSHttpContext.Current;

            // Handle the raw URL from query string (if previous rewrite action was improperly redirected)
            string queryRawUrl = context.Request.QueryString["rawUrl"];
            if (String.IsNullOrEmpty(queryRawUrl))
            {
                return false;
            }

            // Set redirect URL
            URLRewritingContext.PlannedRedirectUrl = queryRawUrl;

            return true;
        }


        /// <summary>
        /// Rewrites the path by using the given file path and query string.
        /// </summary>
        /// <param name="filePath">The internal rewrite path</param>
        /// <param name="queryString">The request query string</param>
        /// <param name="useCurrentPathInfo">If true, the current request path info is used</param>
        public static void RewritePath(string filePath, string queryString, bool useCurrentPathInfo = true)
        {
            RewritePathWithoutClearCachedUrlValues(filePath, useCurrentPathInfo ? CMSHttpContext.Current.Request.PathInfo : null, queryString);

            RequestContext.ClearCachedUrlValues();
        }


        /// <summary>
        /// Rewrites the path based on the given action and clears the cached values related to URL of the request
        /// </summary>
        /// <param name="action">URL rewriting action</param>
        private static void RewritePath(UrlRewritingAction action)
        {
            RewritePath(action.RewritePath, action.RewriteQuery);
        }


        /// <summary>
        /// Rewrites the URL by using the given path, path information and query string information,
        /// without clearing the cached values related to URL of the request
        /// </summary>
        /// <param name="filePath">The internal rewrite path.</param>
        /// <param name="pathInfo">Additional path information for a resource.</param>
        /// <param name="queryString">The request query string.</param>
        private static void RewritePathWithoutClearCachedUrlValues(string filePath, string pathInfo, string queryString)
        {
            ProviderObject.RewritePathWithoutClearCachedUrlValuesInternal(filePath, pathInfo, queryString);
        }


        /// <summary>
        /// Rewrites the URL by using the given path
        /// </summary>
        /// <param name="path">Path to rewrite to</param>
        private static void RewritePath(string path)
        {
            ProviderObject.RewritePathInternal(path);
        }


        /// <summary>
        /// Rewrites the URL and performs all operations required after URL rewriting.
        /// </summary>
        /// <param name="status">Current rewriting status</param>
        /// <param name="relativePath">Relative path</param>
        /// <param name="excludedEnum">Excluded page status</param>
        public static void RewriteUrl(RequestStatusEnum status, string relativePath, ExcludedSystemEnum excludedEnum)
        {
            ViewModeOnDemand viewMode = new ViewModeOnDemand();
            SiteNameOnDemand siteName = new SiteNameOnDemand();

            // Do the rewriting if status not yet determined
            if (status == RequestStatusEnum.Unknown)
            {
                // Rewrite URL
                status = RewriteUrl(relativePath, excludedEnum, siteName, viewMode);
            }

            // Process actions after rewriting
            ProcessRewritingResult(status, excludedEnum, siteName, viewMode, relativePath);
        }


        /// <summary>
        /// Applies rewriting to actual URL.
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="excludedEnum">Excluded page status</param>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <returns>Returns how current URL was processed</returns>
        protected static RequestStatusEnum RewriteUrl(string relativePath, ExcludedSystemEnum excludedEnum, SiteNameOnDemand siteName, ViewModeOnDemand viewMode)
        {
            RequestDebug.LogRequestOperation("RewriteURL", relativePath, 0);

            // Check context
            var context = CMSHttpContext.Current;
            if (context == null)
            {
                RequestContext.CurrentStatus = RequestStatusEnum.NotPage;
                return RequestStatusEnum.NotPage;
            }

            // Ensure the relative path
            if (relativePath == null)
            {
                relativePath = RequestContext.CurrentRelativePath;
            }

            // Store original relative path
            string originalPath = relativePath;

            // Remove trailing slash if required
            relativePath = RemoveTrailingSlashFromPath(relativePath);

            // Increment Robot.txt counter
            if (relativePath.EqualsCSafe("/robots.txt", true))
            {
                RequestHelper.TotalPageRobotsTxtRequests.Increment(siteName);
            }

            // Ensure the exclusion status
            if (excludedEnum == ExcludedSystemEnum.Unknown)
            {
                excludedEnum = URLHelper.IsExcludedSystemEnum(relativePath);
            }

            // Check if excluded
            bool aspxPage = relativePath.EndsWithCSafe(".aspx", true);
            bool excludedSystem = (excludedEnum != ExcludedSystemEnum.NotExcluded);
            var request = context.Request;
            var action = new UrlRewritingAction();

            // Log the output for ASPX pages
            if (aspxPage && OutputDebug.Settings.LogToFile)
            {
                OutputFilterContext.LogCurrentOutputToFile = true;
            }


            #region "Try finish processing with excluded system page (not 404 handler nor ASPX page)"

            if (excludedSystem)
            {
                // Identify requests for non-aspx pages (i.e.: .html, .axd, etc.)
                // Omit handler routes - these request can have various extensions
                if (!aspxPage && (excludedEnum != ExcludedSystemEnum.GetFileHandler))
                {
                    // Status 'RequestStatusEnum.NotPage' disables the debug feature
                    RequestContext.CurrentStatus = RequestStatusEnum.NotPage;
                    return RequestContext.CurrentStatus;
                }

                var status = RequestStatusEnum.SystemPage;

                // CMS Dialog requiring authentication
                if (excludedEnum == ExcludedSystemEnum.CMSDialog)
                {
                    relativePath = relativePath.Substring(12);
                    excludedEnum = URLHelper.IsExcludedSystemEnum(relativePath);

                    RewritePath("~" + relativePath, RequestContext.CurrentQueryString.TrimStart('?'));
                }

                // Perform additional actions with excluded pages and analyze the status
                switch (excludedEnum)
                {
                    case ExcludedSystemEnum.GetFileHandler:
                        // GetFile page
                        status = RequestStatusEnum.GetFileHandler;
                        break;

                    case ExcludedSystemEnum.PortalTemplate:
                        // PortalTemplate page - Act as properly rewritten
                        {
                            status = RequestStatusEnum.PathRewritten;

                            // Handle the rewrite redirect
                            if (FixRewriteRedirect)
                            {
                                HandleRewriteRedirect();
                            }
                        }
                        break;

                    case ExcludedSystemEnum.LogonPage:
                        // Redirect to https version if needed
                        if (SettingsKeyInfoProvider.GetBoolValue("CMSUseSSLForAdministrationInterface"))
                        {
                            if (!RequestContext.IsSSL)
                            {
                                Redirect("https" + RequestContext.URL.AbsoluteUri.Substring(RequestContext.CurrentScheme.Length));
                            }
                        }
                        break;
                }

                // Get language from parameters(on postback) or query, if present, change the user culture.Value to the given culture.Value
                string lang = request.Form["lng"];

                // Set language
                if (!String.IsNullOrEmpty(lang))
                {
                    SetLanguage(siteName, viewMode, lang);
                }

                RequestContext.CurrentStatus = status;

                // Redirect administrators to DB separation page
                if (IsDBSeparationInProgress())
                {
                    DBSeparationRedirect(action);
                }

                return status;
            }

            #endregion


            #region "Try finish processing with REST handler"

            // REST Service
            if (relativePath.StartsWithCSafe("/rest/", true) || relativePath.EqualsCSafe("/rest", true))
            {
                BannedIPInfoProvider.CheckBannedIP();

                RewriteToREST(relativePath, RequestContext.FullDomain.ToLowerCSafe());

                RequestContext.CurrentStatus = RequestStatusEnum.RESTService;
                return RequestStatusEnum.RESTService;
            }

            #endregion

            // Live site root of a content only site
            if (URLHelper.IsRootDefaultDocumentRelativePath(relativePath) && (SiteContext.CurrentSite?.SiteIsContentOnly ?? false))
            {
                Redirect(AdministrationUrlHelper.GetAdministrationUrl());
            }

            // Robots.txt
            bool continueProcessing = !RewriteToRobotsPage(relativePath, siteName.Value, action);

            // Get friendly extension
            string friendlyExtString = null;

            RequestContext.RawURL = request.RawUrl;
            string query = RequestContext.CurrentQueryString;
            string actualExt = Path.GetExtension(relativePath).ToLowerCSafe();
            string originalQuery = query;

            if (continueProcessing)
            {
                // Check if site is running
                if ((actualExt == ".aspx") && (siteName.Value == ""))
                {
                    // Rewrite to invalid web site
                    action.RewritePath = "~/cmsmessages/invalidwebsite.aspx";
                    action.Status = RequestStatusEnum.PathRewrittenDisableOutputFilter;

                    RewritePath(action);
                }
                else
                {
                    // Check if path is excluded (excluded paths) - not a content page
                    string customExcludedPaths = SettingsKeyInfoProvider.GetValue(siteName.Value + ".CMSExcludedURLs");
                    if (URLHelper.IsExcludedCustom(relativePath, customExcludedPaths))
                    {
                        action.Status = RequestStatusEnum.SystemPage;
                        continueProcessing = false;
                    }

                    if (continueProcessing)
                    {
                        string url = RequestContext.URL.GetLeftPart(UriPartial.Path);

                        // Decode URL (support for special characters)
                        url = HttpUtility.UrlDecode(url);

                        // Redirect to ensure trailing slash if necessary
                        RedirectToTrailingSlash(siteName, url, query);

                        #region "Handle language"

                        // Get language from parameters(on postback) or query, if present, change the user culture.Value to the given culture.Value
                        string lang = request.Form["lng"];

                        PreferredCultureOnDemand culture = new PreferredCultureOnDemand();
                        CultureInfo langCultureInfo = null;
                        bool? culturePrefixPresent = null;
                        bool pathPrefixPresent = false;
                        bool checkCulture = viewMode.IsLiveSite();

                        // Check culture prefix
                        if (String.IsNullOrEmpty(lang))
                        {
                            // Remove path prefix if present
                            pathPrefixPresent = PageInfoProvider.RemovePathPrefix(siteName.Value, ref relativePath);

                            // Check whether culture prefix is enabled
                            if (URLHelper.UseLangPrefixForUrls(siteName.Value) && checkCulture)
                            {
                                culturePrefixPresent = false;

                                // Try get language prefix from URL
                                string langCulture = PathPrefixRemoval.RemoveCulturePrefix(siteName.Value, ref relativePath, ref langCultureInfo);
                                if ((langCultureInfo != null) && !RequestHelper.IsPostBack())
                                {
                                    // Get culture code
                                    lang = langCultureInfo.CultureCode;
                                    SetUrlCulturePrefix(lang);
                                    // Set flag that culture prefix exists
                                    culturePrefixPresent = true;

                                    // Check if lang parameter is present
                                    if (RedirectLangParameterToPrefix)
                                    {
                                        // Language parameter is present
                                        string queryLang = request.QueryString[URLHelper.LanguageParameterName];
                                        if (!String.IsNullOrEmpty(queryLang))
                                        {
                                            // Get culture to use for redirect
                                            CultureInfo langInfo = CultureInfoProvider.GetCultureInfoForCulture(queryLang);
                                            if (langInfo != null)
                                            {
                                                if (CultureSiteInfoProvider.IsCultureAllowed(langInfo.CultureCode, siteName.Value))
                                                {
                                                    string culturePrefix = langInfo.CultureCode;
                                                    if (!String.IsNullOrEmpty(langInfo.CultureAlias))
                                                    {
                                                        culturePrefix = langInfo.CultureAlias;
                                                    }

                                                    RedirectPermanent(UrlResolver.ResolveUrl("~/" + culturePrefix + relativePath + URLHelper.RemoveParameterFromUrl(query, URLHelper.LanguageParameterName)), siteName.Value);
                                                }
                                            }
                                        }
                                    }

                                    // If culture alias is defined and current culture prefix is not alias, redirect to the URL using the alias for prefix
                                    if (!String.IsNullOrEmpty(langCultureInfo.CultureAlias) && (CMSString.Compare(langCulture, langCultureInfo.CultureAlias, true) != 0))
                                    {
                                        RedirectPermanent(UrlResolver.ResolveUrl("~/" + langCultureInfo.CultureAlias + relativePath + URLHelper.RemoveParameterFromUrl(query, URLHelper.LanguageParameterName)), siteName.Value);
                                    }
                                }
                            }

                            // Check language in querystring
                            if (String.IsNullOrEmpty(lang))
                            {
                                langCultureInfo = CultureInfoProvider.GetCultureInfoForCulture(request.QueryString[URLHelper.LanguageParameterName]);
                                if (langCultureInfo != null)
                                {
                                    lang = langCultureInfo.CultureCode;
                                }
                            }
                        }

                        // Set language with dependence on domain culture if required
                        if (UseDomainForCulture(siteName.Value) && checkCulture)
                        {
                            // Get current domain
                            string fullDomain = RequestContext.FullDomain;
                            string currentDomain = URLHelper.RemoveWWW(fullDomain);
                            string domainCulture = String.Empty;

                            // Check domain culture
                            SiteInfo csi = SiteContext.CurrentSite;
                            if (currentDomain.CompareToCSafe(csi.DomainName, true) == 0)
                            {
                                domainCulture = csi.DefaultVisitorCulture;
                            }
                            // Check domain alias culture
                            else
                            {
                                SiteDomainAliasInfo sdai = csi.SiteDomainAliases[currentDomain] as SiteDomainAliasInfo;
                                if (sdai != null)
                                {
                                    domainCulture = sdai.SiteDefaultVisitorCulture;
                                }
                            }

                            string queryLang = request.QueryString[URLHelper.LanguageParameterName];
                            if (!String.IsNullOrEmpty(queryLang))
                            {
                                CultureInfo langInfo = CultureInfoProvider.GetCultureInfoForCulture(queryLang);
                                if ((langInfo != null) && (domainCulture.CompareToCSafe(queryLang, true) != 0))
                                {
                                    RedirectToDomainCulture(siteName.Value, queryLang, currentDomain);
                                }
                            }

                            // Set lang for current domain
                            if (!String.IsNullOrEmpty(domainCulture))
                            {
                                lang = domainCulture;
                            }
                        }

                        // Set language
                        if (!String.IsNullOrEmpty(lang))
                        {
                            culture.Value = lang;
                            SetLanguage(siteName, viewMode, culture.Value);
                        }

                        #endregion


                        if (!RewriteToGoogleSitemap(relativePath, siteName.Value)
                            && !HandledByGetAttachmentHandler(relativePath, query, action))
                        {
                            // Rewrite path for URLs handled by special pages
                            if (RewriteGetProduct(relativePath, query, action) ||
                                RewriteGetProductFile(relativePath, query, action))
                            {
                                FileDomainRedirect(siteName, viewMode, action);
                            }
                            else
                            {
                                // Get current domain
                                string fullDomain = RequestContext.FullDomain;
                                string currentDomain = URLHelper.RemoveWWW(fullDomain);

                                // Check extensions from settings
                                bool friendlyExtension = false;
                                if (friendlyExtString == null)
                                {
                                    friendlyExtString = URLHelper.GetFriendlyExtensions(siteName.Value);
                                }
                                friendlyExtString = ";" + friendlyExtString + ";";
                                if (friendlyExtString.IndexOf(";" + actualExt + ";", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    friendlyExtension = true;
                                }

                                // If friendly extension or .aspx, process as a document page
                                bool isDocumentPage = (friendlyExtension || (actualExt == ".aspx") || (relativePath.ToLowerCSafe() == "/default.aspx"));

                                // Ensure portal view mode for document pages
                                viewMode.Value = PortalContext.ViewMode;
                                if (isDocumentPage)
                                {
                                    PortalContext.EnsurePortalMode(viewMode.Value);
                                }

                                // Allow other than preview mode only for /cms/getdoc/ path
                                if (!IsGetDocPrefix(relativePath) && (viewMode.Value != ViewModeEnum.LiveSite) && (viewMode.Value != ViewModeEnum.EditLive))
                                {
                                    PortalContext.SetRequestViewMode(ViewModeEnum.Preview);
                                    viewMode.Value = ViewModeEnum.Preview;
                                }

                                // Ensures site alias redirect
                                if ((viewMode.IsLiveSite()) && (!SearchCrawler.IsCrawlerRequest()))
                                {
                                    continueProcessing = RedirectToDomain(siteName, action);
                                }

                                if (continueProcessing)
                                {
                                    // Check license for the site (only for the content documents)
                                    var licenseStatus = ValidateLicense(currentDomain, url);
                                    if (licenseStatus != LicenseValidationEnum.Valid)
                                    {
                                        RewriteToInvalidLicensePage(url, licenseStatus, action);
                                    }
                                    else
                                    {
                                        // Document - Validate the culture
                                        ValidateCulture(siteName, viewMode, culture, action);

                                        // Remove query part from URL
                                        url = URLHelper.RemoveQuery(url);

                                        // Remove path info if present. Path info is used for web method cannot be used as part of the URL
                                        url = RemovePathInfo(url, request.PathInfo);

                                        // Remove trailing slash
                                        url = RemoveTrailingSlashFromUrl(url);

                                        try
                                        {
                                            // Check default path prefix and add to query parameters if exists
                                            RequestContext.CurrentURLPathPrefix = SettingsKeyInfoProvider.GetValue(siteName.Value + ".CMSDefaultUrlPathPrefix");


                                            #region "Get PageInfo"

                                            PageInfo pageInfo;
                                            switch (viewMode.Value)
                                            {
                                                case ViewModeEnum.Design:
                                                case ViewModeEnum.Edit:

                                                    // If edit / design mode, override the default alias path
                                                    pageInfo = PageInfoProvider.GetPageInfoForUrl(url, culture.Value, "/", true, isDocumentPage, SiteContext.CurrentSiteName);

                                                    break;

                                                // In live edit mode do not combine with default culture
                                                case ViewModeEnum.EditLive:
                                                    // If edit / design mode, override the default alias path
                                                    pageInfo = PageInfoProvider.GetPageInfoForUrl(url, culture.Value, "/", false, isDocumentPage, SiteContext.CurrentSiteName);
                                                    break;

                                                default:

                                                    // If view mode, leave default alias path
                                                    pageInfo = PageInfoProvider.GetPageInfoForUrl(url, culture.Value, null, true, isDocumentPage, SiteContext.CurrentSiteName);

                                                    break;
                                            }

                                            #endregion

                                            if (pageInfo == null)
                                            {
                                                action.Status = RequestStatusEnum.PageNotFound;
                                            }
                                            else
                                            {
                                                var pageInfoSource = pageInfo.PageResult.PageSource;
                                                var wildcardQueryString = pageInfo.PageResult.WildcardQueryString;

                                                if (viewMode.Value == ViewModeEnum.LiveSite)
                                                {
                                                    // Set current page info source
                                                    URLRewritingContext.CurrentPageInfoSource = pageInfoSource;
                                                }

                                                // Check published state of the document
                                                CheckPublishedState(ref pageInfo, siteName, viewMode);

                                                // Ensure the default page settings
                                                RedirectToDefaultPage(siteName, viewMode, pageInfo);

                                                ProcessWildcardParameters(action, wildcardQueryString, ref query, pageInfo);


                                                #region "Validate PageInfo"

                                                switch (pageInfoSource)
                                                {
                                                    case PageInfoSource.UrlPath:
                                                    case PageInfoSource.DocumentAlias:
                                                        // If language switched manually (collision of URLpath and language), redirect to proper language version, only when not postback
                                                        if ((pageInfo != null) && (langCultureInfo != null) && !CMSString.Equals(langCultureInfo.CultureCode, pageInfo.DocumentCulture, true) && !CMSString.Equals(pageInfo.DocumentUrlPath, pageInfo.NodeAliasPath, true) && (request.HttpMethod.ToLowerCSafe() != "post"))
                                                        {
                                                            // Do not process for general document aliases
                                                            if ((pageInfoSource != PageInfoSource.DocumentAlias) || !String.IsNullOrEmpty(pageInfo.PageResult.DocumentAliasCulture))
                                                            {
                                                                // Set current culture prefix if culture prefixes are enabled
                                                                if (URLHelper.UseLangPrefixForUrls(siteName.Value))
                                                                {
                                                                    SetUrlCulturePrefix(pageInfo.DocumentCulture);
                                                                }

                                                                // Redirection to different page
                                                                action.RedirectURL = DocumentURLProvider.GetUrl(pageInfo.NodeAliasPath, null, siteName.Value);
                                                                action.Status = RequestStatusEnum.PathRedirected;

                                                                RequestContext.UseNoCacheForRedirect = true;
                                                                RedirectPermanent(action.RedirectURL, siteName.Value);
                                                                continueProcessing = false;
                                                            }
                                                        }
                                                        break;

                                                    case PageInfoSource.GetDocCulture:
                                                        // Nothing, keeps the document (switches the culture)
                                                        break;

                                                    default:
                                                        // If not combined
                                                        if ((pageInfo != null) && ((pageInfo.DocumentID <= 0) || (culture.Value.ToLowerCSafe() != pageInfo.DocumentCulture.ToLowerCSafe())))
                                                        {
                                                            if (pageInfo.ClassName.EqualsCSafe(SystemDocumentTypes.File, true))
                                                            {
                                                                if (!SiteInfoProvider.CombineFilesWithDefaultCulture(siteName.Value))
                                                                {
                                                                    pageInfo = null;
                                                                }
                                                            }
                                                            // If not combined, not found
                                                            else if (!SiteInfoProvider.CombineWithDefaultCulture(siteName.Value))
                                                            {
                                                                pageInfo = null;
                                                            }
                                                            // If combined and blank found, default culture does not exist
                                                            else if (pageInfo.DocumentID <= 0)
                                                            {
                                                                pageInfo = null;
                                                            }
                                                        }
                                                        break;
                                                }

                                                #endregion


                                                if (continueProcessing)
                                                {
                                                    // If page info not found, page does not exist
                                                    if (pageInfo == null)
                                                    {
                                                        // Page not found
                                                        action.Status = RequestStatusEnum.PageNotFound;
                                                    }
                                                    else
                                                    {
                                                        var documentAliasUrlPath = pageInfo.PageResult.DocumentAliasUrlPath;

                                                        // If document is file type use GetFile function
                                                        if (pageInfo.ClassName.EqualsCSafe(SystemDocumentTypes.File, true))
                                                        {
                                                            #region "File document type"

                                                            // Requesting a CMS.File type document should not change preferred culture cookie value
                                                            // therefore we need to clear it because there can be possible redirect that would change the cookie value.
                                                            CookieHelper.RemoveResponseCookie(CookieName.PreferredCulture);

                                                            DocumentContext.CurrentPageInfo = pageInfo;

                                                            // Add node guid to query parameters
                                                            query = URLHelper.AddUrlParameter(query, "nodeguid", pageInfo.NodeGUID.ToString());

                                                            // Rewrite path and disable output filter (getfile returns binary content)
                                                            SetRewritingAction(GETFILE_PAGE, query, action, RequestStatusEnum.GetFileHandler);

                                                            // Change culture in Live site mode if culture is not current
                                                            if (pageInfo.DocumentCulture.ToLowerCSafe() != culture.Value.ToLowerCSafe())
                                                            {
                                                                switch (pageInfoSource)
                                                                {
                                                                    // Set language parameter only for URL path and domain aliases
                                                                    case PageInfoSource.DocumentAlias:
                                                                    case PageInfoSource.UrlPath:

                                                                        // Ensures proper request language for images
                                                                        ContextHelper.Add(CookieName.PreferredCulture, pageInfo.DocumentCulture, true, false, false, DateTime.Now.AddYears(1));
                                                                        action.RewriteQuery = URLHelper.UpdateParameterInUrl("?" + action.RewriteQuery, URLHelper.LanguageParameterName, pageInfo.DocumentCulture).TrimStart('?');
                                                                        break;
                                                                }
                                                            }

                                                            RewritePathWithoutClearCachedUrlValues(action.RewritePath, CMSHttpContext.Current.Request.PathInfo, action.RewriteQuery);

                                                            if ((viewMode.IsLiveSite()) && !RequestHelper.IsPostBack())
                                                            {
                                                                // Redirect to main document URL
                                                                RedirectToMainUrl(siteName, pageInfo, pageInfoSource, wildcardQueryString, originalQuery, action, culture);

                                                                // Redirect to URL with lang prefix
                                                                if (URLHelper.UseLangPrefixForUrls(siteName.Value))
                                                                {
                                                                    RedirectToLangPrefix(siteName, pageInfo, originalQuery, action, culturePrefixPresent, culture);
                                                                }

                                                                // Redirect to URL with path prefix
                                                                RedirectToPathPrefix(pageInfo, originalQuery, action, pathPrefixPresent, culture);

                                                                // Check path case
                                                                RedirectToCorrectCase(siteName, pageInfo, pageInfoSource, originalPath, originalQuery, action, wildcardQueryString, documentAliasUrlPath);

                                                                // Redirect to main extension
                                                                RedirectToMainExtension(siteName, pageInfo, pageInfoSource, originalQuery, action);
                                                            }

                                                            // Send the file using the GetAttachmentHandler
                                                            ProcessByGetAttachmentHandler();

                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            // Change culture in Live site mode if culture is not current
                                                            if (pageInfo.DocumentCulture.ToLowerCSafe() != culture.Value.ToLowerCSafe())
                                                            {
                                                                switch (pageInfoSource)
                                                                {
                                                                    // Document through URL path and getdoc is always accessible only through live site mode
                                                                    case PageInfoSource.UrlPath:
                                                                    case PageInfoSource.GetDocCulture:
                                                                    case PageInfoSource.DocumentAlias:
                                                                        {
                                                                            // Do not process for general document aliases
                                                                            if ((pageInfoSource != PageInfoSource.DocumentAlias) || !String.IsNullOrEmpty(pageInfo.PageResult.DocumentAliasCulture))
                                                                            {
                                                                                string docCulture = pageInfo.DocumentCulture;

                                                                                // Try redirect document to the correct culture domain
                                                                                RedirectToDomainCulture(siteName.Value, docCulture, currentDomain);

                                                                                // Fully specified document
                                                                                culture.Value = docCulture;
                                                                                SetLanguage(siteName, viewMode, culture.Value);
                                                                            }
                                                                        }
                                                                        break;

                                                                    default:
                                                                        // If the combination with default culture is not used, change the culture to the page found (only in live site mode)
                                                                        if (viewMode.IsLiveSite() && !SiteInfoProvider.CombineWithDefaultCulture(siteName.Value))
                                                                        {
                                                                            SetLanguage(siteName, viewMode, culture.Value);
                                                                        }
                                                                        break;
                                                                }
                                                            }

                                                            // Redirect to DocumentRedirectUrl if is defined
                                                            bool isCrawler = SearchCrawler.IsCrawlerRequest();
                                                            if (isCrawler || !RedirectToDocumentRedirect(pageInfo, originalQuery, action, viewMode.Value, siteName.Value))
                                                            {
                                                                // Set page info to current context
                                                                DocumentContext.CurrentPageInfo = pageInfo;

                                                                RequestStatusEnum status = RequestStatusEnum.PathRewritten;

                                                                if (viewMode.IsLiveSite() && !isCrawler && !RequestHelper.IsPostBack())
                                                                {
                                                                    // Redirect to main document URL
                                                                    RedirectToMainUrl(siteName, pageInfo, pageInfoSource, wildcardQueryString, originalQuery, action, culture);

                                                                    // Redirect to URL with lang prefix
                                                                    if (URLHelper.UseLangPrefixForUrls(siteName.Value))
                                                                    {
                                                                        RedirectToLangPrefix(siteName, pageInfo, originalQuery, action, culturePrefixPresent, culture);
                                                                    }

                                                                    // Redirect to URL with path prefix
                                                                    RedirectToPathPrefix(pageInfo, originalQuery, action, pathPrefixPresent, culture);

                                                                    // Check path case
                                                                    RedirectToCorrectCase(siteName, pageInfo, pageInfoSource, originalPath, originalQuery, action, wildcardQueryString, documentAliasUrlPath);

                                                                    // Redirect to main extension
                                                                    RedirectToMainExtension(siteName, pageInfo, pageInfoSource, originalQuery, action);
                                                                }

                                                                // Process AB Testing pages
                                                                pageInfo = ProcessABTest(siteName.Value, viewMode.Value, pageInfo);

                                                                // Process the page template and get the file path to rewrite
                                                                string filename = GetTemplateFileName(pageInfo);


                                                                #region "URL rewriting"

                                                                action.Status = status;

                                                                // Rewrite the path to target location
                                                                if (filename != null)
                                                                {
                                                                    // Get node alias path
                                                                    string nodeAliasQueryString = pageInfo.NodeAliasPath;

                                                                    // Encode node alias path if it is enabled
                                                                    if (EncodeNodeAliasPath)
                                                                    {
                                                                        nodeAliasQueryString = HttpUtility.UrlEncode(nodeAliasQueryString);
                                                                    }

                                                                    // Add alias path to the query
                                                                    string newQueryString = URLHelper.AddUrlParameter(query, URLHelper.AliasPathParameterName, nodeAliasQueryString);

                                                                    // Add raw URL to the processing page
                                                                    if (FixRewriteRedirect)
                                                                    {
                                                                        newQueryString = URLHelper.AddUrlParameter(newQueryString, "rawUrl", HttpUtility.UrlEncode(RequestContext.RawURL));
                                                                    }

                                                                    // Rewrite the path to the processing page
                                                                    action.RewritePath = filename;
                                                                    action.RewriteQuery = newQueryString.TrimStart('?');

                                                                    RewritePath(action);
                                                                }

                                                                #endregion
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (DomainNotFoundException)
                                        {
                                            // Domain not found - Invalid web site
                                            action.RewritePath = "~/cmsmessages/invalidwebsite.aspx";
                                            action.Status = RequestStatusEnum.PathRewrittenDisableOutputFilter;

                                            RewritePath(action);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

            // Log the rewriting result
            RequestDebug.LogRequestOperation(action.Status.ToString(), action.RewritePath, 0);

            // Return the action
            RequestContext.CurrentStatus = action.Status;

            return action.Status;
        }


        /// <summary>
        /// Removes the path info from URL.
        /// </summary>
        /// <param name="url">URL to process</param>
        /// <param name="pathInfo">Path info to remove</param>
        private static string RemovePathInfo(string url, string pathInfo)
        {
            // No URL, return null
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // No path info, return original URL
            if (string.IsNullOrEmpty(pathInfo))
            {
                return url;
            }

            // Remove the path info
            if (url.EndsWithCSafe(pathInfo, true))
            {
                return url.Substring(0, url.Length - pathInfo.Length);
            }

            return url;
        }


        /// <summary>
        /// Redirects files to proper domain if enabled
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="action">Rewriting action</param>
        public static void FileDomainRedirect(SiteNameOnDemand siteName = null, ViewModeOnDemand viewMode = null, UrlRewritingAction action = null)
        {
            if (!DomainPolicy.UseDomainRedirectForFiles)
            {
                return;
            }

            viewMode = viewMode ?? new ViewModeOnDemand();

            // Ensures site alias redirect
            if (viewMode.IsLiveSite() && !SearchCrawler.IsCrawlerRequest())
            {
                siteName = siteName ?? new SiteNameOnDemand();

                RedirectToDomain(siteName, action ?? new UrlRewritingAction());
            }
        }


        /// <summary>
        /// Ensures the default page settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="pageInfo">Current page info</param>
        private static void RedirectToDefaultPage(SiteNameOnDemand siteName, ViewModeOnDemand viewMode, PageInfo pageInfo)
        {
            // Page not available
            if (pageInfo == null)
            {
                return;
            }

            // Do not ensure default page if current request is postback
            if (RequestHelper.IsPostBack())
            {
                return;
            }

            // Only for live site
            if (!viewMode.IsLiveSite())
            {
                return;
            }

            // Get default page behavior
            var siteNameValue = siteName;
            string defaultBehavior = DefaultPageBehavior(siteNameValue);

            // Check if default page handling is required
            if (defaultBehavior.EqualsCSafe(DocumentURLProvider.DEFAULT_PAGE_NONE))
            {
                return;
            }

            // Get current default alias path
            string defaultAliasPath = PageInfoProvider.GetDefaultAliasPath(RequestContext.CurrentDomain, siteNameValue);

            // Check if current page is default page
            if (!pageInfo.NodeAliasPath.EqualsCSafe(defaultAliasPath, true))
            {
                return;
            }

            switch (defaultBehavior)
            {
                // Use domain
                case DocumentURLProvider.DEFAULT_PAGE_DOMAIN:
                    {
                        string path = RequestContext.CurrentRelativePath;

                        PageInfoProvider.RemovePathPrefix(siteNameValue, ref path);
                        PathPrefixRemoval.RemoveCulturePrefix(siteNameValue, ref path);

                        if (!String.IsNullOrEmpty(path) && (path != "/"))
                        {
                            // Redirect to the domain
                            var resolveUrl = URLHelper.AppendQuery("~/", RequestContext.CurrentQueryString);
                            RedirectPermanent(UrlResolver.ResolveUrl(resolveUrl), siteNameValue);
                        }
                    }
                    break;

                // Use default alias path
                case DocumentURLProvider.DEFAULT_PAGE_PAGE:
                    var pageSource = pageInfo.PageResult.PageSource;
                    if (pageSource == PageInfoSource.DefaultAliasPath)
                    {
                        // Redirect
                        string defaultAliasUrl = DocumentURLProvider.GetUrl(pageInfo.NodeAliasPath, null, siteNameValue);
                        defaultAliasUrl = URLHelper.AppendQuery(defaultAliasUrl, RequestContext.CurrentQueryString);
                        RedirectPermanent(UrlResolver.ResolveUrl(defaultAliasUrl), siteNameValue);
                    }
                    break;

                // Use default page URL
                case DocumentURLProvider.DEFAULT_PAGE_DEFAULT:
                    {
                        string path = RequestContext.CurrentRelativePath;

                        // Handle trailing slash
                        path = RemoveTrailingSlashFromPath(path);

                        PageInfoProvider.RemovePathPrefix(siteNameValue, ref path);
                        PathPrefixRemoval.RemoveCulturePrefix(siteNameValue, ref path);

                        var defaultPageUrl = DocumentURLProvider.DefaultPageURL;
                        if ((path != null) && !path.EqualsCSafe(defaultPageUrl.TrimStart('~'), true))
                        {
                            // Redirect to the default page (default.aspx)
                            defaultPageUrl = URLHelper.AppendQuery(defaultPageUrl, RequestContext.CurrentQueryString);
                            RedirectPermanent(UrlResolver.ResolveUrl(defaultPageUrl), siteNameValue);
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Checks the published state of the document. If the document is not published and combine with default culture is enabled and default culture is published, default culture is returned. If no published version is found, the page info is set to null.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        public static void CheckPublishedState(ref PageInfo pageInfo, SiteNameOnDemand siteName, ViewModeOnDemand viewMode)
        {
            // Only for live site
            if (!viewMode.IsLiveSite())
            {
                return;
            }

            // Check if page inf should be validated
            if ((pageInfo == null) || pageInfo.IsPublished || !PageNotFoundForNonPublished(siteName))
            {
                return;
            }

            // Documents should be combined with default culture
            var siteNameValue = siteName;
            if (SiteInfoProvider.CombineWithDefaultCulture(siteNameValue))
            {
                // Try to find published document in default culture
                string defaultCulture = CultureHelper.GetDefaultCultureCode(siteNameValue);
                PageInfo pi = PageInfoProvider.GetPageInfo(siteNameValue, pageInfo.NodeAliasPath, defaultCulture, null, false);
                if ((pi != null) && pi.IsPublished)
                {
                    pageInfo = pi;
                }
                else
                {
                    pageInfo = null;
                }
            }
            else
            {
                pageInfo = null;
            }
        }


        /// <summary>
        /// Processes the page template and returns file name to rewrite to, if returning file name is null, no rewriting should occur and was handled internally
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        private static string GetTemplateFileName(PageInfo pageInfo)
        {
            // Check the template
            var template = pageInfo.UsedPageTemplateInfo;
            if (template == null)
            {
                throw new Exception("[URLRewriter.ProcessPageTemplate]: Missing page template info for page '" + pageInfo.NodeAliasPath + "'.");
            }

            string filename;

            if (!template.IsAspx)
            {
                // Use portal template page
                filename = URLHelper.PortalTemplatePage;
            }
            else
            {
                // Use configured file for ASPX page
                filename = template.FileName;
                if (String.IsNullOrEmpty(filename))
                {
                    throw new Exception("[URLRewriter.ProcessPageTemplate]: Missing template file name for ASPX template '" + template.DisplayName + "'.");
                }
            }

            return filename;
        }


        /// <summary>
        /// Indicates if database separation is in progress.
        /// </summary>
        private static bool IsDBSeparationInProgress()
        {
            string currentURL = RequestContext.CurrentURL.ToLowerInvariant();
            return DatabaseSeparationHelper.SeparationInProgress && (!currentURL.Contains("separatedb.aspx") && !currentURL.Contains("joindb.aspx") && !currentURL.Contains("webfarmupdater.aspx"));
        }


        /// <summary>
        /// Redirects administrators to separation page.
        /// </summary>
        private static void DBSeparationRedirect(UrlRewritingAction action)
        {
            // Handle DB separation
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                string returnUrl = HttpUtility.UrlEncode(RequestContext.CurrentURL);
                string finalUrl;
                // Display separation progress for admin
                if ((SettingsHelper.ConnectionStrings["CMSOMConnectionString"] != null) && !String.IsNullOrEmpty(SettingsHelper.ConnectionStrings["CMSOMConnectionString"].ConnectionString))
                {
                    finalUrl = UrlResolver.ResolveUrl("~/CMSInstall/JoinDB.aspx");
                }
                else
                {
                    finalUrl = UrlResolver.ResolveUrl("~/CMSInstall/SeparateDB.aspx");
                }
                finalUrl = URLHelper.AddParameterToUrl(finalUrl, "returnurl", returnUrl);
                Redirect(finalUrl);
            }
            else
            {
                // Display Offline mode message
                var currentSite = SiteContext.CurrentSite;
                if (currentSite != null)
                {
                    RedirectToSiteOffline(action, currentSite);
                }
            }
        }


        /// <summary>
        /// Redirects current request to domain which is set for required culture. Domain redirection must be enabled in settings.
        /// </summary>
        /// <param name="siteName">Current site name</param>
        /// <param name="docCulture">Required culture</param>
        /// <param name="currentDomain">Current domain</param>
        private static void RedirectToDomainCulture(string siteName, string docCulture, string currentDomain)
        {
            // Check settings
            if (!UseDomainForCulture(siteName))
            {
                return;
            }

            // Get the domain
            string domain = SiteInfoProvider.GetDomainForCulture(siteName, docCulture);

            // Domain not found or is different from the current
            if (String.IsNullOrEmpty(domain) || (URLHelper.DomainMatch(domain, currentDomain, true)))
            {
                return;
            }

            // Redirect to correct domain
            string url = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.LanguageParameterName);
            url = URLHelper.GetAbsoluteUrl(url, domain);
            RedirectPermanent(url, siteName);
        }


        /// <summary>
        /// Returns current page info with dependence on A/B test settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="pageInfo">Original page info</param>
        public static PageInfo ProcessABTest(string siteName, ViewModeEnum viewMode, PageInfo pageInfo)
        {
            // Apply page variants only when AB Testing, Web analytics, Track conversions are enabled && current view mode is live site
            var args = new ProcessABTestEventArgs
            {
                PageInfo = pageInfo,
                SiteName = siteName,
                ViewMode = viewMode
            };

            // Try get page info for some variant
            URLRewritingEvents.ProcessABTest.StartEvent(args);

            var variantPageInfo = args.ReturnedPageInfo;

            // Variant doesn't exist
            if (variantPageInfo == null)
            {
                return pageInfo;
            }

            // Keep the original alias path and original page info for title purposes
            DocumentContext.OriginalAliasPath = pageInfo.NodeAliasPath;
            DocumentContext.OriginalPageInfo = pageInfo;

            // Replace original page info object
            pageInfo = variantPageInfo;

            // Set page info to current context
            DocumentContext.CurrentPageInfo = pageInfo;

            return pageInfo;
        }


        /// <summary>
        /// Processes the wildcard parameters
        /// </summary>
        /// <param name="action">Current rewriting action</param>
        /// <param name="wildcardQueryString">Wildcard querystring</param>
        /// <param name="query">Current querystring</param>
        /// <param name="pageInfo">Page info</param>
        private static void ProcessWildcardParameters(UrlRewritingAction action, string wildcardQueryString, ref string query, PageInfo pageInfo)
        {
            // There is no wilcard to process
            if ((pageInfo == null) || String.IsNullOrEmpty(wildcardQueryString))
            {
                return;
            }

            // Check whether query is available
            if (!String.IsNullOrEmpty(query))
            {
                // Switch by
                switch (WildcardQueryMapping)
                {
                    // Keep existing query string values  if same values are defined in wildcard querystring
                    case "keepexisting":
                        {
                            // Split originl query by ampersand
                            string[] splittedQuery = query.Trim('?').Split('&');

                            // Set wildcard query as main query
                            query = "?" + wildcardQueryString.TrimStart('&');

                            // Loop thru all original query items
                            foreach (string queryItem in splittedQuery)
                            {
                                // Split query item to name and value
                                string[] nameValue = queryItem.Split('=');

                                // Set key name
                                string key = nameValue[0];

                                // Check whether key is defined
                                if (!String.IsNullOrEmpty(key))
                                {
                                    // Set default value
                                    string value = String.Empty;

                                    // Set value for current name of os defined
                                    if (nameValue.Length == 2)
                                    {
                                        value = nameValue[1];
                                    }

                                    // Update parameter in URL
                                    query = URLHelper.UpdateParameterInUrl(query, key, value);
                                }
                            }
                        }
                        break;

                    // Replace exisiting querystring values if same values are defined in wildcard querystring
                    case "replaceexisting":
                        {
                            // Split originl query by ampersand
                            string[] splittedQuery = wildcardQueryString.Trim('&').Split('&');

                            // Set wildcard query as main query
                            query = "?" + wildcardQueryString.TrimStart('&');

                            // Loop thru all original query items
                            foreach (string queryItem in splittedQuery)
                            {
                                // Split query item to name and value
                                string[] nameValue = queryItem.Split('=');

                                // Set key name
                                string key = nameValue[0];

                                // Check whether key is defined
                                if (!String.IsNullOrEmpty(key))
                                {
                                    // Set default value
                                    string value = String.Empty;

                                    // Set value for current name of os defined
                                    if (nameValue.Length == 2)
                                    {
                                        value = nameValue[1];
                                    }

                                    // Update parameter in URL
                                    query = URLHelper.UpdateParameterInUrl(query, key, value);
                                }
                            }
                        }
                        break;

                    // Join querystrings
                    default:
                        query += wildcardQueryString;
                        break;
                }
            }
            else
            {
                query = "?" + wildcardQueryString.TrimStart('&');
            }

            action.RewriteQuery = query;
        }


        /// <summary>
        /// Processes the result of URL rewriting and handlers appropriate actions
        /// </summary>
        /// <param name="status">Resulting status from the URL rewriter</param>
        /// <param name="excludedEnum">Excluded page status</param>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="relativePath">Relative path of the request</param>
        public static void ProcessRewritingResult(RequestStatusEnum status, ExcludedSystemEnum excludedEnum, SiteNameOnDemand siteName, ViewModeOnDemand viewMode, string relativePath)
        {
            ProcessRewritingResult(new URLRewritingParams
            {
                Status = status,
                ExcludedEnum = excludedEnum,
                SiteName = siteName,
                ViewMode = viewMode,
                RelativePath = relativePath
            });
        }


        /// <summary>
        /// Processes the result of URL rewriting and handlers appropriate actions
        /// </summary>
        /// <param name="p">URL rewriting parameters</param>
        public static void ProcessRewritingResult(URLRewritingParams p)
        {
            // Fire the process rewriting result event handler
            using (var h = URLRewritingEvents.ProcessRewritingResult.StartEvent(p))
            {
                if (h.CanContinue())
                {
                    switch (p.Status)
                    {
                        // Document page
                        case RequestStatusEnum.PathRewritten:

                            // Apply output filter
                            OutputFilterContext.ApplyOutputFilter = true;

                            // Enable GZip if supported
                            if (RequestHelper.AllowGZip)
                            {
                                RequestContext.UseGZip = RequestHelper.IsGZipSupported();
                            }

                            // Counter page request
                            RequestHelper.TotalPageRequests.Increment(p.SiteName);
                            break;

                        // Get file script
                        case RequestStatusEnum.GetFileHandler:
                            // Set the debugging
                            DebugHelper.SetContentPageDebug();

                            // Counter GetFile request
                            RequestHelper.TotalGetFileRequests.Increment(p.SiteName);
                            break;

                        // Page not found - Log hit and redirect
                        case RequestStatusEnum.PageNotFound:
                            PageNotFound(p.SiteName, p.ViewMode);
                            break;

                        // Not a page
                        case RequestStatusEnum.NotPage:
                            bool disableDebug = ShouldDisableDebug(p);

                            // Handle not page request
                            NotPage(disableDebug);

                            break;

                        // System page
                        case RequestStatusEnum.SystemPage:
                            // Enable GZip if supported
                            if (RequestHelper.AllowGZip)
                            {
                                RequestContext.UseGZip = RequestHelper.IsGZipSupported();
                            }

                            // Set debugging for request
                            DebugHelper.SetSystemPageDebug();

                            // Counter the system page
                            RequestHelper.TotalSystemPageRequests.Increment(null);
                            break;

                        default:
                            // Counter non page
                            RequestHelper.TotalNonPageRequests.Increment(null);
                            break;
                    }
                }

                h.FinishEvent();
            }
        }


        private static bool ShouldDisableDebug(URLRewritingParams parameters)
        {
            // Debugging should not be disabled for WebApi calls
            if (parameters.RelativePath.StartsWith("/cmsapi/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return (parameters.ExcludedEnum != ExcludedSystemEnum.WebDAV) &&
                    !parameters.RelativePath.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, ".axd", ".asmx", ".ashx", ".svc");
        }


        /// <summary>
        /// Handles the page not found response in case it wasn't yet handled
        /// </summary>
        internal static void HandlePageNotFound()
        {
            // Already handled
            if (URLRewritingContext.PageNotFoundHandled || OutputFilterContext.SentFromCache)
            {
                return;
            }

            // Use special filtering for output filter. EndRequest is not able run standard response filter
            OutputFilterContext.FilterResponseOnRender = true;
            PageNotFound();
        }


        /// <summary>
        /// Returns the page not found error or redirects to the page not found page
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        private static void PageNotFound(SiteNameOnDemand siteName = null, ViewModeOnDemand viewMode = null)
        {
            if (siteName == null)
            {
                siteName = new SiteNameOnDemand();
            }

            if (viewMode == null)
            {
                viewMode = new ViewModeOnDemand();
            }

            // If exist redirect this page to not found URL
            RedirectToPageNotFound(siteName, viewMode);
        }


        /// <summary>
        /// Sets up the request as not page request.
        /// </summary>
        /// <param name="disableDebug">If true, the debug is disabled for this request</param>
        private static void NotPage(bool disableDebug)
        {
            // Do not debug non page requests
            if (disableDebug)
            {
                DebugHelper.DisableDebug();
            }
            else
            {
                // Always disable output debug for non-pages
                DebugContext.CurrentRequestSettings[OutputDebug.Settings] = false;
            }

            // Counter not page
            RequestHelper.TotalNonPageRequests.Increment(null);
        }


        /// <summary>
        /// Redirects the user to page not found URL.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        private static void RedirectToPageNotFound(SiteNameOnDemand siteName, ViewModeOnDemand viewMode)
        {
            string relativePath = RequestContext.CurrentRelativePath;
            bool pageNotFoundHandled = URLRewritingContext.PageNotFoundHandled;

            var h = URLRewritingEvents.PageNotFound;
            if (h.IsBound)
            {
                var p = new URLRewriterPageNotFoundEventArgs
                {
                    SiteName = siteName,
                    ViewMode = viewMode,
                    RelativePath = relativePath,
                    PageNotFoundHandled = pageNotFoundHandled
                };

                // Fire the process page not found event handler
                h.StartEvent(p);

                pageNotFoundHandled = p.PageNotFoundHandled;
            }

            // Check whether page not found was already handled by event handler
            if (pageNotFoundHandled)
            {
                URLRewritingContext.PageNotFoundHandled = true;
                return;
            }

            // Check whether context is defined
            var context = CMSHttpContext.Current;
            if (context == null)
            {
                return;
            }

            // Do not use page not found for existing physical files
            var extension = Path.GetExtension(relativePath);

            if (!String.IsNullOrEmpty(extension))
            {
                string physicalPath = SystemContext.WebApplicationPhysicalPath + Path.EnsureBackslashes(relativePath);
                if (File.Exists(physicalPath))
                {
                    return;
                }
            }

            // Do not use page not found for URL handled by route
            if ((context.Handler == null) && (RouteTable.Routes.Count > 0))
            {
                var data = RouteTable.Routes.GetRouteData(context);
                if (data != null)
                {
                    return;
                }
            }

            // Log page not found
            AnalyticsMethods.LogPageNotFound(siteName);

            // Counter page not found
            RequestHelper.TotalPageNotFoundRequests.Increment(siteName.Value);

            if (IsLogPageNotFoundExceptionEnabled() && !IsCrawlerOrWebDAVRequest())
            {
                EventLogProvider.LogEvent(EventType.WARNING, "Application_Error", "PAGENOTFOUND", String.Empty, siteId: SiteContext.CurrentSiteID);
            }

            // Keep information that page not found was handled
            URLRewritingContext.PageNotFoundHandled = true;

            // Indicates whether status code already set
            bool statusCodeValid = (context.Response.StatusCode == 404);

            try
            {
                // Set page not found response by default
                context.Response.StatusCode = 404;
            }
            catch (HttpException)
            {
                // HTTP headers have already been sent and cannot be modified - return the response without additional handling
                return;
            }

            // Add special handling for edit live mode
            if (PortalContext.ViewMode.IsEditLive())
            {
                context.Response.StatusCode = 200;
                var systemUrl = UrlResolver.ResolveUrl("~/CMSModules/PortalEngine/CMSPages/OnSiteEdit/PageNotFound.aspx");
                TransferPageNotFound(systemUrl, false, true);
                return;
            }

            // Try get custom page not found page
            string url = ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName.Value + ".CMSPageNotFoundUrl"), "");

            if (String.IsNullOrEmpty(url) || IsPostBackWithoutCsrfToken(context.Request))
            {
                CompleteRequest();
                return;
            }

            // Resolve URL if starts with "~/"
            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                url = UrlResolver.ResolveUrl(url);
            }

            // Redirect to URL if does NOT start with "/"
            if (!url.StartsWith("/", StringComparison.Ordinal))
            {
                Redirect(url);
                return;
            }

            // Check if path is excluded (excluded paths)
            string customExcludedPaths = "";
            if (siteName.Value != "")
            {
                customExcludedPaths = SettingsKeyInfoProvider.GetValue(siteName.Value + ".CMSExcludedURLs");
            }

            // Final transfer path
            string path = String.Empty;

            // For excluded path use original path
            if (URLHelper.IsExcluded(URLHelper.RemoveApplicationPath(url), customExcludedPaths))
            {
                path = url;
            }

            // If file exists in file system use the path
            if (String.IsNullOrEmpty(path))
            {
                // Remove trailing slash
                var fileUrl = url.TrimEnd('/');
                string physicalPath = SystemContext.WebApplicationPhysicalPath + Path.EnsureBackslashes(URLHelper.RemoveApplicationPath(fileUrl));

                // Check if physical file exists
                if (File.Exists(physicalPath))
                {
                    path = url;
                }
            }

            // Try get path form content
            if (String.IsNullOrEmpty(path))
            {
                // Remove trailing slash
                var pageUrl = URLHelper.GetAbsoluteUrl(url.TrimEnd('/'));

                // Try get page info
                PageInfo pi = PageInfoProvider.GetPageInfoForUrl(pageUrl, CultureHelper.GetPreferredCulture(), null, true, true, SiteContext.CurrentSiteName);

                // if exists get page path
                if (pi != null)
                {
                    // Use document culture for page not found request
                    CultureHelper.SetPreferredCulture(pi.DocumentCulture, false, (viewMode.Value != ViewModeEnum.LiveSite));

                    var template = pi.UsedPageTemplateInfo;
                    if ((template != null) && template.IsAspx)
                    {
                        path = template.FileName;
                    }
                    else
                    {
                        path = URLHelper.PortalTemplatePage;
                    }
                    DocumentContext.CurrentAliasPath = pi.NodeAliasPath;

                    // Apply output filter
                    ResponseOutputFilter filter = ResponseOutputFilter.EnsureOutputFilter();
                    OutputFilterContext.ApplyOutputFilter = true;

                    // Enable GZip if supported
                    if (RequestHelper.AllowGZip)
                    {
                        RequestContext.UseGZip = RequestHelper.IsGZipSupported();
                    }

                    // Set the filter for the response
                    HttpResponseBase response = CMSHttpContext.Current.Response;
                    if (response.Filter != filter)
                    {
                        response.Filter = filter;
                    }
                }
            }

            // Transfer to the final path if is defined
            if (!String.IsNullOrEmpty(path))
            {
                TransferPageNotFound(path, true, statusCodeValid);
            }
            else
            {
                // Complete request
                CompleteRequest();
            }
        }


        private static bool IsPostBackWithoutCsrfToken(HttpRequestBase request)
        {
            return RequestHelper.IsPostBack() && CsrfProtection.IsProtectionEnabled() && request.Form[CsrfProtection.HIDDEN_FIELD_NAME] == null;
        }


        private static bool IsLogPageNotFoundExceptionEnabled()
        {
            return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSLogPageNotFoundException");
        }


        private static bool IsCrawlerOrWebDAVRequest()
        {
            return SearchCrawler.IsCrawlerRequest() || RequestContext.CurrentExcludedStatus == ExcludedSystemEnum.WebDAV;
        }


        /// <summary>
        /// Completes request
        /// </summary>
        private static void CompleteRequest()
        {
            ProviderObject.CompleteRequestInternal();
        }


        /// <summary>
        /// Transfer page not found request
        /// </summary>
        /// <param name="path">Page not found page path</param>
        /// <param name="useRewrite">Indicates whether path should be rewritten for standard requests</param>
        /// <param name="statusCodeValid">Indicates whether status code is valid for request transfer (false use classic handler mapping)</param>
        private static void TransferPageNotFound(string path, bool useRewrite, bool statusCodeValid)
        {
            ProviderObject.TransferPageNotFoundInternal(path, useRewrite, statusCodeValid);
        }


        /// <summary>
        /// Handles the rewriting for REST service.
        /// </summary>
        /// <param name="relativePath">Relative URL</param>
        /// <param name="domain">Domain of the request</param>
        private static void RewriteToREST(string relativePath, string domain)
        {
            var context = CMSHttpContext.Current;
            string[] urlParts = BaseRESTService.RewriteRESTUrl(relativePath, context.Request.QueryString.ToString(), domain, context.Request.HttpMethod);
            if (urlParts.Length != 2)
            {
                return;
            }

            RewritePath(urlParts[0], urlParts[1]);
        }


        /// <summary>
        /// Ensures redirect domain if is required and the offline mode handling.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="action">Rewriting action</param>
        private static bool RedirectToDomain(SiteNameOnDemand siteName, UrlRewritingAction action)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                return true;
            }

            // Handle the Offline mode
            if (site.SiteIsOffline)
            {
                if (!RedirectToSiteOffline(action, site))
                {
                    return false;
                }
            }

            DomainPolicy.Apply(site);

            return true;
        }


        /// <summary>
        /// Redirects to site offline message.
        /// </summary>
        /// <param name="action">Rewriting action</param>
        /// <param name="currentSite">Current site</param>
        /// <returns>Returns true if rewriting path occurs.</returns>
        private static bool RedirectToSiteOffline(UrlRewritingAction action, SiteInfo currentSite)
        {
            if (String.IsNullOrEmpty(currentSite.SiteOfflineRedirectURL))
            {
                // Display site offline message
                RewriteStopPath(action, "~/CMSMessages/SiteOffline.aspx");
                return false;
            }

            // Redirect to specific URL
            Redirect(currentSite.SiteOfflineRedirectURL);
            return true;
        }


        /// <summary>
        /// Rewrites path with disabled output filter.
        /// </summary>
        /// <param name="action">Rewriting action</param>
        /// <param name="path">Target path</param>
        private static void RewriteStopPath(UrlRewritingAction action, string path)
        {
            action.RewritePath = path;
            action.Status = RequestStatusEnum.PathRewrittenDisableOutputFilter;

            RewritePath(action);
        }


        /// <summary>
        /// Validates the given <paramref name="culture"/> and modifies it with a valid culture when the given culture was not valid.
        /// If not valid, it then chooses the site's default culture and sets it as a preferred culture.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="culture">Culture to validate</param>
        /// <param name="action">Rewriting action</param>
        public static void ValidateCulture(SiteNameOnDemand siteName, ViewModeOnDemand viewMode, PreferredCultureOnDemand culture, UrlRewritingAction action)
        {
            bool originalPreferredCultureIsEmpty = String.IsNullOrEmpty(culture.Value);

            // Validate the culture
            if (originalPreferredCultureIsEmpty || !CultureSiteInfoProvider.IsCultureAllowed(culture.Value, siteName.Value))
            {
                // Automatically identify preferred culture
                culture.Value = new PreferredCultureEvaluator(viewMode).Evaluate();
                if (String.IsNullOrEmpty(culture.Value))
                {
                    culture.Value = LocalizationContext.PreferredCultureCode;
                    if (URLHelper.UseLangPrefixForUrls(siteName.Value))
                    {
                        SetUrlCulturePrefix(culture.Value);
                    }
                }
                else
                {
                    // Set culture.Value to automatically identified
                    SetLanguage(siteName, viewMode, culture.Value, !originalPreferredCultureIsEmpty);
                }
            }
            else
            {
                // Set current culture prefix if culture prefixes are enabled and culture prefix wasn't defined yet
                if (URLHelper.UseLangPrefixForUrls(siteName.Value) && String.IsNullOrEmpty(RequestContext.CurrentURLLangPrefix))
                {
                    SetUrlCulturePrefix(culture.Value);
                }
            }
        }


        /// <summary>
        /// Sets the culture code or alias to the culture prefix.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        private static void SetUrlCulturePrefix(string cultureCode)
        {
            CultureInfo ci = CultureInfoProvider.GetCultureInfoForCulture(cultureCode);
            if (ci != null)
            {
                RequestContext.CurrentURLLangPrefix = !String.IsNullOrEmpty(ci.CultureAlias) ? ci.CultureAlias : ci.CultureCode;
            }
            else
            {
                RequestContext.CurrentURLLangPrefix = String.Empty;
            }
        }


        /// <summary>
        /// Returns relative path from url.
        /// Example: http://www.domain.com/mypage.aspx => /mypage.aspx
        /// </summary>
        /// <param name="url">URL</param>
        protected static string GetRelativePathFromUrl(string url)
        {
            // Remove the protocol
            int protocolIndex = url.IndexOfCSafe("://");
            if (protocolIndex >= 0)
            {
                url = url.Substring(protocolIndex + 3);
            }

            // Remove domain and application path
            int applicationIndex = url.IndexOfCSafe(SystemContext.ApplicationPath, true);
            if (applicationIndex >= 0)
            {
                url = url.Substring(applicationIndex + SystemContext.ApplicationPath.Length);
            }

            // Add first slash
            if (!url.StartsWithCSafe("/"))
            {
                url = "/" + url;
            }

            return url;
        }


        /// <summary>
        /// Gets the live page URL.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <remarks>The URL is always formed of NodeAliasPath and URL path is ignored.</remarks>
        public static string GetLiveUrl(TreeNode node)
        {
            return DocumentURLProvider.GetUrl(node.NodeAliasPath);
        }


        /// <summary>
        /// Gets the preview page URL
        /// </summary>
        /// <param name="node">Document node</param>
        public static string GetEditingUrl(TreeNode node)
        {
            // Use '/getdoc' url format
            return UrlResolver.ResolveUrl(DocumentURLProvider.GetPermanentDocUrl(node.NodeGUID, node.NodeAlias, SiteContext.CurrentSiteName, PageInfoProvider.PREFIX_CMS_GETDOC, ".aspx"));
        }


        /// <summary>
        /// Checks whether a license for the given URL and domain exists and is valid.
        /// </summary>
        /// <param name="domain">Current domain without www. Taken from url when empty.</param>
        /// <param name="url">URL to check</param>
        internal static LicenseValidationEnum ValidateLicense(string domain, string url = null)
        {
            return ProviderObject.ValidateLicenseInternal(domain, url);
        }


        /// <summary>
        /// Sets language.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="culture">Culture code</param>
        /// <param name="setCookie">Set the culture to a cookie as well</param>
        private static void SetLanguage(SiteNameOnDemand siteName, ViewModeOnDemand viewMode, string culture, bool setCookie = true)
        {
            // Do not set if empty
            if (String.IsNullOrEmpty(culture))
            {
                return;
            }

            // Check if already set (do not send the cookie again)
            string currentCulture = CultureHelper.GetPreferredCulture();
            if (currentCulture == culture)
            {
                return;
            }

            // Check the culture
            culture = CultureSiteInfoProvider.CheckCultureCode(culture, siteName.Value);
            if (!String.IsNullOrEmpty(culture))
            {
                CultureHelper.SetPreferredCulture(culture, setCookie, (viewMode.Value != ViewModeEnum.LiveSite));
            }

            if (URLHelper.UseLangPrefixForUrls(siteName.Value))
            {
                SetUrlCulturePrefix(culture);
            }
        }


        /// <summary>
        /// Redirects the page to correct URL based on case settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="pageInfo">Page info</param>
        /// <param name="pageInfoSource">Page info source</param>
        /// <param name="originalPath">Original path</param>
        /// <param name="originalQuery">Original query string</param>
        /// <param name="action">Rewriting action</param>
        /// <param name="wildcardQuery">Wildcard query string</param>
        /// <param name="documentAliasUrlPath">Document alias url path</param>
        private static void RedirectToCorrectCase(SiteNameOnDemand siteName, PageInfo pageInfo, PageInfoSource pageInfoSource, string originalPath, string originalQuery, UrlRewritingAction action, string wildcardQuery, string documentAliasUrlPath)
        {
            // Check settings
            var redirect = URLHelper.GetCaseRedirectEnum(siteName.Value);
            if ((pageInfo == null) || (redirect == CaseRedirectEnum.None))
            {
                return;
            }

            string url = String.Empty;

            switch (redirect)
            {
                // Lower/Upper case URL
                case CaseRedirectEnum.LowerCase:
                case CaseRedirectEnum.UpperCase:
                    var toUpper = redirect == CaseRedirectEnum.UpperCase;
                    var currentUrl = URLHelper.RemoveQuery(URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath));
                    if (!string.IsNullOrEmpty(currentUrl))
                    {
                        var targetUrl = toUpper ? currentUrl.ToUpperInvariant() : currentUrl.ToLowerInvariant();
                        if (!String.Equals(currentUrl, targetUrl, StringComparison.InvariantCulture))
                        {
                            url = targetUrl + originalQuery;
                        }
                    }

                    break;

                // Exact URL
                case CaseRedirectEnum.Exact:

                    string expectedPath = null;

                    // Keep current state to propagate it
                    bool containsTrailingSlash = originalPath.EndsWith("/", StringComparison.Ordinal);

                    // Get path without extension
                    originalPath = URLHelper.RemoveExtension(originalPath.TrimEnd('/'));

                    switch (pageInfoSource)
                    {
                        case PageInfoSource.AliasPath:
                            expectedPath = DocumentURLProvider.GetUrl(pageInfo.NodeAliasPath, null, siteName.Value);
                            break;

                        case PageInfoSource.UrlPath:

                            if (String.IsNullOrEmpty(wildcardQuery))
                            {
                                expectedPath = DocumentURLProvider.GetUrl(pageInfo.NodeAliasPath, pageInfo.DocumentUrlPath, siteName.Value);
                            }
                            else
                            {
                                // Exact is not supported for URLs with wildcards => use original path
                                expectedPath = "~/" + originalPath.TrimStart('~').TrimStart('/') + TreePathUtils.GetUrlExtension();
                            }

                            break;

                        case PageInfoSource.DocumentAlias:

                            if (String.IsNullOrEmpty(wildcardQuery))
                            {
                                expectedPath = "~/" + documentAliasUrlPath.TrimStart('~').TrimStart('/') + TreePathUtils.GetUrlExtension();
                            }
                            else
                            {
                                // Exact is not supported for URLs with wildcards => use original path
                                expectedPath = "~/" + originalPath.TrimStart('~').TrimStart('/') + TreePathUtils.GetUrlExtension();
                            }

                            break;
                    }

                    // Check if the path is expected
                    if (expectedPath != null)
                    {
                        string expectedUrl = expectedPath;

                        // Ensure previous state of trailing slash
                        if (containsTrailingSlash)
                        {
                            expectedUrl = expectedUrl.TrimEnd('/') + "/";
                        }

                        // Remove trailing slash for further processing
                        expectedPath = expectedPath.TrimEnd('/');
                        originalPath = originalPath.TrimEnd('/');

                        // Create expected path out of the path
                        expectedPath = URLHelper.RemoveExtension(expectedPath);
                        if (expectedPath.StartsWith("~/", StringComparison.Ordinal))
                        {
                            expectedPath = expectedPath.Substring(1);
                        }

                        // If the lowercase doesn't match (alias is used), use the original path as base for expected
                        if (!expectedPath.Equals(originalPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            expectedPath = originalPath;
                        }

                        if (expectedPath != originalPath)
                        {
                            url = expectedUrl + originalQuery;
                        }
                    }
                    break;
            }

            // No URL to redirect to
            if (String.IsNullOrEmpty(url))
            {
                return;
            }

            // Set the action
            action.RedirectURL = url;
            action.Status = RequestStatusEnum.PathRedirected;

            // Redirect
            RedirectPermanent(url, siteName.Value);
        }


        /// <summary>
        /// If necessary, redirects to correct URL based on trailing slash settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="url">URL</param>
        /// <param name="query">Query string</param>
        private static void RedirectToTrailingSlash(SiteNameOnDemand siteName, string url, string query)
        {
            // Do not process trailing slash if current request is postback
            if (RequestHelper.IsPostBack())
            {
                return;
            }

            // Check whether URL is defined
            if (String.IsNullOrEmpty(url))
            {
                return;
            }

            // Get trailing slash settings
            var settings = URLHelper.UseURLsWithTrailingSlash(siteName.Value);

            // Do not process trailing slash for getdoc pages
            var relativePath = GetRelativePathFromUrl(url);
            if (IsGetDocPrefix(relativePath))
            {
                return;
            }

            // Do not ensure or keep trailing slash for URL with extension, trailing slashes are used only for extensionless URLs
            var extension = Path.GetExtension(relativePath);
            if ((settings != TrailingSlashEnum.Never) && URLHelper.UseTrailingSlashOnlyForExtensionLess && !String.IsNullOrEmpty(extension))
            {
                return;
            }

            // Process URL if not we site root or there is a virtual directory used
            bool process = relativePath != "/";
            if (!process)
            {
                // Process URLs for virtual directories
                if (SystemContext.ApplicationPath != "/")
                {
                    process = true;
                    relativePath = URLHelper.RemoveQuery(RequestContext.CurrentURL);
                }
            }

            // For web site root without virtual directory, no action is required
            if (!process)
            {
                return;
            }

            if (relativePath.EndsWithCSafe("/"))
            {
                // Remove trailing slash from path
                if (settings == TrailingSlashEnum.Never)
                {
                    RedirectPermanent(url.TrimEnd('/') + query, siteName.Value);
                }
            }
            else
            {
                // Ensure trailing slash in path
                if (settings == TrailingSlashEnum.Always)
                {
                    RedirectPermanent(url + "/" + query, siteName.Value);
                }
            }
        }


        /// <summary>
        /// Redirects document to its URL with main extension if current extension isn't main.
        /// </summary>
        /// <param name="siteName">Current site name</param>
        /// <param name="pageInfo">Page info</param>
        /// <param name="pageInfoSource">Page info source</param>
        /// <param name="originalQuery">Original querystring</param>
        /// <param name="action">UrlRewriting action</param>
        /// <returns>Returns true if document was redirected</returns>
        private static void RedirectToMainExtension(SiteNameOnDemand siteName, PageInfo pageInfo, PageInfoSource pageInfoSource, string originalQuery, UrlRewritingAction action)
        {
            // Do not redirect to main extension if current request is postback
            if (RequestHelper.IsPostBack())
            {
                return;
            }

            // Check settings
            if (!SettingsKeyInfoProvider.GetBoolValue(siteName.Value + ".CMSRedirectToMainExtension") || (pageInfoSource == PageInfoSource.DefaultAliasPath))
            {
                return;
            }

            // Get main extension
            string mainExtension = DocumentURLProvider.GetExtension(pageInfo);

            // Get current extension
            string currentExtension = RequestContext.CurrentUrlExtension;

            // Do not process for same extensions
            if (mainExtension.EqualsCSafe(currentExtension, true))
            {
                return;
            }

            // Get current URL
            string currentUrl = URLHelper.RemoveQuery(URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath));

            // Do not redirect if current URL is root
            if (currentUrl == "~/")
            {
                return;
            }

            // Keep info about trailing slash
            bool containsTrailingSlash = currentUrl.EndsWithCSafe("/");

            // Remove extension
            currentUrl = URLHelper.RemoveExtension(currentUrl.TrimEnd('/'));

            // Add main extension
            currentUrl += mainExtension;

            // Add trailing slash if original URL contained it
            if (containsTrailingSlash)
            {
                currentUrl += "/";
            }

            // Add original querystring
            currentUrl = currentUrl + originalQuery;

            // Set action status
            action.RedirectURL = currentUrl;
            action.Status = RequestStatusEnum.PathRedirected;

            // Use permanent redirect
            RedirectPermanent(currentUrl, siteName.Value);
        }


        /// <summary>
        /// Redirects the page to its main URL.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="pageInfo">Page info</param>
        /// <param name="pageInfoSource">Page info source</param>
        /// <param name="wildCardQueryString">Wildcard query string</param>
        /// <param name="originalQuery">Original query string</param>
        /// <param name="action">Rewriting action</param>
        /// <param name="culture">Preferred culture</param>
        internal static bool RedirectToMainUrl(SiteNameOnDemand siteName, PageInfo pageInfo, PageInfoSource pageInfoSource, string wildCardQueryString, string originalQuery, UrlRewritingAction action, PreferredCultureOnDemand culture)
        {
            // Check page
            if (pageInfo == null)
            {
                return false;
            }

            // There is already redirection requested
            if (!String.IsNullOrEmpty(action?.RedirectURL))
            {
                return false;
            }

            // If document alias redirection is not specified check site settings
            if ((pageInfo.PageResult.DocumentAliasActionMode == AliasActionModeEnum.UseSiteSettings) && String.IsNullOrEmpty(wildCardQueryString) && RedirectAliasesToMainURL(siteName) ||
                // Redirect document alias if it is required
                (pageInfo.PageResult.DocumentAliasActionMode == AliasActionModeEnum.RedirectToMainURL))
            {
                bool redirect = false;

                switch (pageInfoSource)
                {
                    case PageInfoSource.UrlPath:
                        // URL path is the main path - keep
                        break;

                    case PageInfoSource.DocumentAlias:
                        {
                            redirect = true;
                            break;
                        }

                    case PageInfoSource.GetDoc:
                    case PageInfoSource.GetDocCulture:
                        // Aliases and permanent URLs are alternatives for the document URL - redirect
                        redirect = true;
                        break;

                    case PageInfoSource.AliasPath:
                        {
                            // Redirect alias path only in case URL path is available and cultures match
                            if (culture.Value.EqualsCSafe(pageInfo.DocumentCulture, true))
                            {
                                string urlPath = pageInfo.DocumentUrlPath;
                                if (!String.IsNullOrEmpty(urlPath))
                                {
                                    // Try resolve wildcard
                                    if (urlPath.Contains("{"))
                                    {
                                        urlPath = DocumentURLProvider.EnsureWildcardPath(urlPath);
                                    }

                                    if (!urlPath.Contains("{"))
                                    {
                                        string aliasPath = pageInfo.NodeAliasPath;
                                        if (!urlPath.EqualsCSafe(aliasPath, true))
                                        {
                                            redirect = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case PageInfoSource.DefaultAliasPath:
                        {
                            // Get default page behavior
                            string defaultPageBehavior = DefaultPageBehavior(siteName);

                            // Check if default page handling is required
                            if (defaultPageBehavior.EqualsCSafe(DocumentURLProvider.DEFAULT_PAGE_NONE))
                            {
                                // Redirect only if not redirection to default
                                string aliasPath = pageInfo.NodeAliasPath;
                                string documentUrlPath = pageInfo.DocumentUrlPath;
                                if (!documentUrlPath.EqualsCSafe("/", true) && !aliasPath.EqualsCSafe("/", true) &&
                                    !documentUrlPath.EqualsCSafe("/default", true) && !aliasPath.EqualsCSafe("/default", true))
                                {
                                    redirect = true;
                                }
                                // Check whether /default.aspx page should be redirected to the main root version. This option could cause an infinite redirection for IIS < 7
                                else if (UseRedirectForDefaultPage && (CMSString.Compare(RequestContext.CurrentRelativePath, "/default.aspx", true) == 0))
                                {
                                    redirect = true;
                                }
                            }
                        }
                        break;
                }

                // Redirect to the page main URL
                if (redirect)
                {
                    RedirectToMainUrl(pageInfo, originalQuery, action, culture);
                }
            }

            return false;
        }


        /// <summary>
        /// Redirects the page to the URL with language prefix.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="pageInfo">Page info</param>
        /// <param name="originalQuery">Original query</param>
        /// <param name="action">Action</param>
        /// <param name="culturePrefixPresent">Indicates whether culture prefix is defined in the current URL</param>
        /// <param name="culture">Preferred culture</param>
        private static void RedirectToLangPrefix(SiteNameOnDemand siteName, PageInfo pageInfo, string originalQuery, UrlRewritingAction action, bool? culturePrefixPresent, PreferredCultureOnDemand culture)
        {
            if ((culturePrefixPresent == null) || culturePrefixPresent.Value || (pageInfo == null) || !String.IsNullOrEmpty(action.RedirectURL) || !RedirectURLToLangPrefix(siteName.Value))
            {
                return;
            }

            RedirectToMainUrl(pageInfo, originalQuery, action, culture);
        }


        /// <summary>
        /// Redirects the page to the URL with path prefix.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="originalQuery">Original query</param>
        /// <param name="action">Action</param>
        /// <param name="pathPrefixPresent">Indicates whether path prefix is present in current URL</param>
        /// <param name="culture">Preferred culture</param>
        private static void RedirectToPathPrefix(PageInfo pageInfo, string originalQuery, UrlRewritingAction action, bool pathPrefixPresent, PreferredCultureOnDemand culture)
        {
            if (pathPrefixPresent || String.IsNullOrEmpty(RequestContext.CurrentURLPathPrefix) || (pageInfo == null) || !String.IsNullOrEmpty(action.RedirectURL) || !RedirectURLToPathPrefix())
            {
                return;
            }

            RedirectToMainUrl(pageInfo, originalQuery, action, culture);
        }


        /// <summary>
        /// Redirects the page to its main URL.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="originalQuery">Original query string</param>
        /// <param name="action">Rewriting action</param>
        /// <param name="culture">Preferred culture</param>
        public static void RedirectToMainUrl(PageInfo pageInfo, string originalQuery, UrlRewritingAction action, PreferredCultureOnDemand culture)
        {
            // Do not redirect to main URL if current request is postback
            if (RequestHelper.IsPostBack())
            {
                return;
            }

            string documentUrlPath = null;

            // Do not change document culture, use only node alias path for URL
            if (culture.Value.EqualsCSafe(pageInfo.DocumentCulture, true))
            {
                documentUrlPath = pageInfo.DocumentUrlPath;
            }

            string url = DocumentURLProvider.GetUrl(pageInfo.NodeAliasPath, documentUrlPath) + originalQuery;

            if (action != null)
            {
                action.RedirectURL = url;
                action.Status = RequestStatusEnum.PathRedirected;
            }

            RequestContext.UseNoCacheForRedirect = true;

            // Redirect
            RedirectPermanent(url, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Redirect to URL if is defined DocumentMenuRedirectUrl.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="query">Original request query</param>
        /// <param name="action">Rewriting action</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns true if the page was redirected</returns>
        private static bool RedirectToDocumentRedirect(PageInfo pageInfo, string query, UrlRewritingAction action, ViewModeEnum viewMode, string siteName)
        {
            // Check page
            if (pageInfo == null)
            {
                return false;
            }

            // Get view mode
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            // Only for live site
            if (!viewMode.IsLiveSite())
            {
                return false;
            }

            // Get redirection URL
            string url = GetRedirectionUrl(pageInfo, query, siteName);
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            // Set action
            action.RedirectURL = url;
            action.Status = RequestStatusEnum.PathRedirected;

            // Make sure redirect is not cached if URL is based on macro expression or redirect to first child should be performed
            if (MacroProcessor.ContainsMacro(pageInfo.DocumentMenuRedirectUrl) || pageInfo.DocumentMenuRedirectToFirstChild)
            {
                RequestContext.UseNoCacheForRedirect = true;
            }

            // Redirect
            RedirectPermanent(url, siteName);

            return true;
        }


        /// <summary>
        /// Redirects the response using specified URL using permanent redirection using 301.
        /// </summary>
        /// <param name="url">URL for redirection</param>
        /// <param name="siteName">Site name</param>
        private static void RedirectPermanent(string url, string siteName)
        {
            ProviderObject.RedirectPermanentInternal(url, siteName);
        }


        /// <summary>
        /// Redirects the response using specified URL (302 HTTP status code).
        /// </summary>
        /// <param name="url">URL for redirection</param>
        private static void Redirect(string url)
        {
            ProviderObject.RedirectInternal(url);
        }


        /// <summary>
        /// Get redirection URL
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="query">Query string</param>
        /// <param name="siteName">Site name</param>
        private static string GetRedirectionUrl(PageInfo pageInfo, string query, string siteName)
        {
            // Check page
            if (pageInfo == null)
            {
                return String.Empty;
            }

            // Init resolver
            MacroResolver resolver = MacroContext.CurrentResolver;
            if (!String.IsNullOrEmpty(pageInfo.DocumentMenuRedirectUrl))
            {
                // Get document
                var tree = new TreeProvider();
                var node = tree.SelectSingleNode(siteName, pageInfo.NodeAliasPath, pageInfo.DocumentCulture, false, pageInfo.ClassName, false);
                if (node != null)
                {
                    // Set up source data for macro resolver
                    resolver = resolver.CreateChild();
                    resolver.Culture = node.DocumentCulture;
                    resolver.SetAnonymousSourceData(node);
                }
            }

            // Get redirection URL
            string url = TreePathUtils.GetRedirectionUrl(pageInfo, resolver);

            // There is no URL
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            // Add original query if no query present in redirect URL
            string lowerQuery = query.ToLowerCSafe();
            if (!url.Contains("?") && !(lowerQuery.Contains("404;http") || lowerQuery.Contains("404;http")))
            {
                url += query;
            }

            return url;
        }


        /// <summary>
        /// Process robots.txt rewriting
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="action">Rewriting action</param>
        private static bool RewriteToRobotsPage(string relativePath, string siteName, UrlRewritingAction action)
        {
            // No robots page
            if (!relativePath.EqualsCSafe("/robots.txt", true))
            {
                return false;
            }

            // Get custom robots path
            string robotsPath = SettingsKeyInfoProvider.GetValue(siteName + ".CMSRobotsPath");
            if (String.IsNullOrEmpty(robotsPath))
            {
                return false;
            }

            var pageInfo = PageInfoProvider.GetPageInfo(siteName, robotsPath, LocalizationContext.PreferredCultureCode, null, SiteInfoProvider.CombineWithDefaultCulture(siteName));

            if (pageInfo == null)
            {
                return false;
            }

            // Set context
            DocumentContext.CurrentAliasPath = robotsPath;
            var query = UpdatePreviewHashForVirtualContext(null, relativePath);
            SetRewritingAction(relativePath, query, action, RequestStatusEnum.PathRewritten);

            // Check secured areas
            PageSecurityHelper.CheckSecuredAreas(siteName, pageInfo, true, ViewModeEnum.LiveSite);

            // Check permissions
            PageSecurityHelper.CheckPermissions(siteName, pageInfo, true, null);

            if (EncodeNodeAliasPath)
            {
                robotsPath = HttpUtility.UrlEncode(robotsPath);
            }

            // Rewrite path
            RewritePath(URLHelper.PortalTemplatePage, URLHelper.AliasPathParameterName + "=" + robotsPath);
            return true;
        }


        /// <summary>
        /// Processes the Google site map rewriting.
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="siteName">Site name</param>
        private static bool RewriteToGoogleSitemap(string relativePath, string siteName)
        {
            string url = GetGoogleSiteMapURL(siteName);

            // Not Google sitemap page
            if ((url == "/") || !url.EqualsCSafe(relativePath, true))
            {
                return false;
            }

            // Get node alias path representing document with Google site map
            string nodeAliasPath = GetGoogleSiteMapPath(siteName);

            // Use system page
            if (String.IsNullOrEmpty(nodeAliasPath))
            {
                RewritePathWithoutClearCachedUrlValues("~/CMSPages/GoogleSiteMap.aspx", null, null);
                return true;
            }

            // Get page
            var pageInfo = PageInfoProvider.GetPageInfo(siteName, nodeAliasPath, LocalizationContext.PreferredCultureCode, null, SiteInfoProvider.CombineWithDefaultCulture(siteName));

            // Set current page info to populate DocumentContext
            DocumentContext.CurrentPageInfo = pageInfo;

            // Check secured areas
            PageSecurityHelper.CheckSecuredAreas(siteName, pageInfo, true, ViewModeEnum.LiveSite);

            // Check permissions
            PageSecurityHelper.CheckPermissions(siteName, pageInfo, true, null);

            // Encode alias path
            if (EncodeNodeAliasPath)
            {
                nodeAliasPath = HttpUtility.UrlEncode(nodeAliasPath);
            }

            RewritePath(URLHelper.PortalTemplatePage, URLHelper.AliasPathParameterName + "=" + nodeAliasPath);
            return true;
        }


        /// <summary>
        /// Sets rewriting action.
        /// </summary>
        /// <param name="path">New path</param>
        /// <param name="query">Query string</param>
        /// <param name="action">Rewriting action object</param>
        /// <param name="status">Request status enumeration</param>
        private static void SetRewritingAction(string path, string query, UrlRewritingAction action, RequestStatusEnum status)
        {
            action.RewritePath = path;
            action.RewriteQuery = (query != null) ? query.TrimStart('?') : null;
            action.Status = status;
        }


        private static string UpdatePreviewHashForVirtualContext(string query, string path)
        {
            if (!VirtualContext.IsInitialized)
            {
                return query;
            }

            var url = VirtualContext.AddPathHash(path + query);
            return url.Substring(url.IndexOf('?'));
        }


        /// <summary>
        /// Checks if the link if GetProduct path.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="removePrefix">Remove the prefix if match</param>
        private static bool CheckGetProduct(ref string path, bool removePrefix)
        {
            return URLHelper.CheckPrefix(ref path, "/getproduct/", removePrefix);
        }


        /// <summary>
        /// Rewrite path when path started with /getproduct/GUID/ to  ..../getproduct.aspx?guid=GUID....
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="query">Query</param>
        /// <param name="action">Rewriting action</param>
        /// <returns>Return true if path is rewritten</returns>
        private static bool RewriteGetProduct(string relativePath, string query, UrlRewritingAction action)
        {
            if (CMSHttpContext.Current == null)
            {
                return false;
            }

            if (!CheckGetProduct(ref relativePath, true))
            {
                return false;
            }

            // Remove all behind the GUID
            int slashIndex = relativePath.IndexOfCSafe('/');
            string guid = null;
            if (slashIndex >= 0)
            {
                guid = relativePath.Substring(0, slashIndex);
            }

            // Add guid to query parameters
            query = URLHelper.AddUrlParameter(query, "skuguid", guid);

            var path = "~/CMSPages/Ecommerce/GetProduct.aspx";
            query = UpdatePreviewHashForVirtualContext(query, path);
            SetRewritingAction(path, query, action, RequestStatusEnum.GetProduct);
            RewritePath(action);

            return true;
        }


        /// <summary>
        /// Checks if the link is GetProductFile path.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="removePrefix">Remove the prefix if match</param>
        private static bool CheckGetProductFile(ref string path, bool removePrefix)
        {
            return URLHelper.CheckPrefix(ref path, "/GetProductFile/", removePrefix);
        }


        /// <summary>
        /// Rewrite path when path is starting with /getproductfile/GUID/ to  .../GetProductFile.aspx?token=GUID...
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="query">Query</param>
        /// <param name="action">Rewriting action</param>
        /// <returns>Returns true if path is rewritten</returns>
        private static bool RewriteGetProductFile(string relativePath, string query, UrlRewritingAction action)
        {
            if (CMSHttpContext.Current == null)
            {
                return false;
            }

            if (!CheckGetProductFile(ref relativePath, true))
            {
                return false;
            }

            // Remove all behind the GUID
            int slashIndex = relativePath.IndexOfCSafe('/');
            string guid = null;
            if (slashIndex >= 0)
            {
                guid = relativePath.Substring(0, slashIndex);
            }

            // Add token GUID to query parameters
            query = URLHelper.AddUrlParameter(query, "token", guid);

            var path = "~/CMSPages/Ecommerce/GetProductFile.aspx";
            query = UpdatePreviewHashForVirtualContext(query, path);
            SetRewritingAction(path, query, action, RequestStatusEnum.GetProductFile);
            RewritePath(action);

            return true;
        }


        /// <summary>
        /// Checks whether the given path is GetAttachment path: /getattachment/ and removes this prefix from the path.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>Returns true if prefix was found and removed.</returns>
        private static bool RemoveGetAttachmentPrefix(ref string path)
        {
            return URLHelper.CheckPrefixes(ref path, GetAttachmentPrefixes, true);
        }


        /// <summary>
        /// Rewrites path for attachments identified by node alias path.
        /// URL format: /getattachment/node/alias/path/filename{*.extension} rewrites to  CMSPages/getfile.aspx?filename=...aliaspath=...
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="query">Query</param>
        /// <param name="action">Rewriting action</param>
        /// <returns>Return true if path was rewritten.</returns>
        private static bool RewriteGetAttachment(string relativePath, string query, UrlRewritingAction action)
        {
            if (CMSHttpContext.Current == null)
            {
                return false;
            }

            // Check URL for /getattachment/ pattern
            if (RemoveGetAttachmentPrefix(ref relativePath))
            {
                // Extend the original query string with parameters for GetFile handler
                query = URLHelper.AppendQuery(query, GenerateGetFileParameters(relativePath));

                query = UpdatePreviewHashForVirtualContext(query, GETFILE_PAGE);
                SetRewritingAction(GETFILE_PAGE, query, action, RequestStatusEnum.GetFileHandler);
                RewritePath(action);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Generates query string parameters for GetFile handler
        /// </summary>
        /// <param name="relativePath">Document relative path. Format: "document/relative/path/filename{*.extension}"</param>
        private static string GenerateGetFileParameters(string relativePath)
        {
            string query = string.Empty;

            // Get the file name (last part of the path)
            int fileNameIndex = relativePath.LastIndexOfCSafe('/') + 1;

            string filename = relativePath.Substring(fileNameIndex);
            if (!String.IsNullOrEmpty(filename))
            {
                // Get the friendly extension
                string friendlyUrlExtension = TreePathUtils.GetFilesUrlExtension().ToLowerCSafe();

                // Remove the ASPX extension if not set as a custom
                if ((friendlyUrlExtension != ".aspx") && filename.ToLowerCSafe().EndsWithCSafe(".aspx"))
                {
                    filename = filename.Substring(0, filename.Length - 5);
                }

                // Remove friendly extension from the end
                if ((friendlyUrlExtension != String.Empty) && filename.ToLowerCSafe().EndsWithCSafe(friendlyUrlExtension))
                {
                    filename = filename.Substring(0, filename.Length - friendlyUrlExtension.Length);
                }

                filename = HttpUtility.UrlEncode(filename);
                query = URLHelper.AddUrlParameter(query, "filename", filename);
            }

            string aliasPath = "/" + relativePath.Substring(0, fileNameIndex).TrimEnd('/');

            // Encode node alias path if it is enabled
            if (EncodeNodeAliasPath)
            {
                aliasPath = HttpUtility.UrlEncode(aliasPath);
            }

            // Add alias path to query parameters
            query = URLHelper.AddUrlParameter(query, "aliaspath", aliasPath);

            return query;
        }


        /// <summary>
        /// Determines whether the url is a document url starting with "/cms/getdoc/" prefix.
        /// </summary>
        /// <param name="relativePath">The relative path</param>
        private static bool IsGetDocPrefix(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            return relativePath.StartsWithCSafe(PREFIX_CMS_GETDOC, true);
        }


        /// <summary>
        /// Removes trailing slash from given relative path
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        private static string RemoveTrailingSlashFromPath(string relativePath)
        {
            // Check relative path
            if (string.IsNullOrEmpty(relativePath))
            {
                return relativePath;
            }

            // Do not handle slash for getdoc path
            if (IsGetDocPrefix(relativePath))
            {
                return relativePath;
            }

            // Path is correct
            if (!relativePath.EndsWithCSafe("/"))
            {
                return relativePath;
            }

            // Remove slash
            return relativePath.TrimEnd('/');
        }


        /// <summary>
        /// Removes trailing slash from given URL
        /// </summary>
        /// <param name="url">URL</param>
        private static string RemoveTrailingSlashFromUrl(string url)
        {
            // Check URL
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // Do not process trailing slash for getdoc pages
            var relativePath = GetRelativePathFromUrl(url);
            if (IsGetDocPrefix(relativePath))
            {
                return url;
            }

            // URL is correct
            if (!url.EndsWithCSafe("/"))
            {
                return url;
            }

            // Remove slash
            return url.TrimEnd('/');
        }


        /// <summary>
        /// Returns true if the non published documents should return page not found result.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool PageNotFoundForNonPublished(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPageNotFoundForNonPublished");
        }

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Attempts to send the output of the page from the cache.
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="excludedEnum">Excluded page status</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        public static bool SendOutputFromCache(string relativePath, ExcludedSystemEnum excludedEnum, ViewModeOnDemand viewMode, SiteNameOnDemand siteName)
        {
            if (!AllowOutputCache(relativePath, excludedEnum))
            {
                return false;
            }

            var preferredCulture = new PreferredCultureOnDemand();
            if (String.IsNullOrEmpty(preferredCulture.Value))
            {
                // First request with no culture cookie set needs to try to get the culture from the URL culture prefix
                // to ensure that the correct culture is used in the output cache key
                SetPreferredCultureFromCulturePrefix(relativePath, siteName, viewMode);
            }

            ValidateCulture(siteName, viewMode, preferredCulture, null);

            // Try to get result from the cache
            CachedOutput output;

            if (!OutputHelper.SendOutputFromCache(viewMode, siteName, out output))
            {
                return false;
            }

            RequestContext.CurrentStatus = RequestStatusEnum.SentFromCache;

            return true;
        }


        /// <summary>
        /// Sets the preferred culture from URL culture prefix.
        /// </summary>
        private static void SetPreferredCultureFromCulturePrefix(string relativePath, SiteNameOnDemand siteName, ViewModeOnDemand viewMode)
        {
            // Remove path prefix if present
            PageInfoProvider.RemovePathPrefix(siteName.Value, ref relativePath);

            // Remove language prefix if present and enabled for the given site
            CultureInfo langCultureInfo = null;
            PathPrefixRemoval.RemoveCulturePrefix(siteName.Value, ref relativePath, ref langCultureInfo);

            if (langCultureInfo != null)
            {
                SetLanguage(siteName, viewMode, langCultureInfo.CultureCode);
            }
        }


        /// <summary>
        /// Returns true if the given path excludes output cache
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="excludedEnum">Excluded page status</param>
        private static bool AllowOutputCache(string relativePath, ExcludedSystemEnum excludedEnum)
        {
            if (!OutputHelper.UseOutputFilterCache || excludedEnum != ExcludedSystemEnum.NotExcluded)
            {
                return false;
            }

            var specialPath =
                relativePath.StartsWith(PREFIX_GETFILE, StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith(PREFIX_CMS_GETFILE, StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith("/getmetafile/", StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith("/getmediafile/", StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith("/getimage/", StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith("/getmedia/", StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith(PREFIX_GETATTACHMENT, StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith(PREFIX_CMS_GETATTACHMENT, StringComparison.InvariantCultureIgnoreCase) ||
                relativePath.StartsWith(VirtualContext.VirtualContextPrefix, StringComparison.InvariantCultureIgnoreCase);

            return !specialPath;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Transfer page not found request
        /// </summary>
        /// <param name="path">Page not found page path</param>
        /// <param name="useRewrite">Indicates whether path should be rewritten for standard requests</param>
        /// <param name="statusCodeValid">Indicates whether status code is valid for request transfer (false use classic handler mapping)</param>
        protected virtual void TransferPageNotFoundInternal(string path, bool useRewrite, bool statusCodeValid)
        {
            var context = CMSHttpContext.Current;

            // Clear generated content
            if (statusCodeValid)
            {
                context.Response.ClearContent();
            }

            if (useRewrite)
            {
                // Rewrite path for standard requests
                RewritePath(path);
            }

            // Try skip IIS http errors
            context.Response.TrySkipIisCustomErrors = true;

            // Set handler to the PortalTemplate page
            var handler = BuildManager.CreateInstanceFromVirtualPath(path, typeof(Page)) as IHttpHandler;
            context.Handler = handler;

            // Transfer & complete request after external 404 handling
            if (statusCodeValid)
            {
                // Reset the page context to allow another execution of page life-cycle within the same request
                // Necessary when the life-cycle has already been (partially) executed within the request before deciding to send 404
                PageContext.Reset();

                context.Server.Transfer(handler, true);
                CompleteRequest();
            }
        }


        /// <summary>
        /// Completes request
        /// </summary>
        protected virtual void CompleteRequestInternal()
        {
            RequestHelper.CompleteRequest();
        }


        /// <summary>
        /// Checks whether a license for the given URL and domain exists and is valid.
        /// </summary>
        /// <param name="domain">Current domain without www. Taken from url when empty.</param>
        /// <param name="url">URL to check</param>
        protected internal virtual LicenseValidationEnum ValidateLicenseInternal(string domain, string url = "")
        {
            // Get domain from URL
            if (String.IsNullOrEmpty(domain))
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(url);
            }

            // Get validation result
            return LicenseHelper.ValidateLicenseForDomain(domain);
        }


        /// <summary>
        /// Rewrites the URL to the invalid license page.
        /// </summary>
        /// <param name="url">The URL without valid license</param>
        /// <param name="licenseStatus">License status</param>
        /// <param name="action">The action which </param>
        private static void RewriteToInvalidLicensePage(string url, LicenseValidationEnum licenseStatus, UrlRewritingAction action)
        {
            string query = "rawurl=" + url + "&result=" + Convert.ToInt32(licenseStatus);

            // Rewrite to invalid license key
            const string rewritePath = "~/cmsmessages/invalidlicensekey.aspx";

            if (action != null)
            {
                action.RewritePath = rewritePath;
                action.RewriteQuery = query;
                action.Status = RequestStatusEnum.PathRewrittenDisableOutputFilter;
            }

            RewritePath(rewritePath, query);
        }


        /// <summary>
        /// Redirects the response using specified URL using permanent redirection using 301.
        /// </summary>
        /// <param name="url">URL for redirection</param>
        /// <param name="siteName">Site name</param>
        protected virtual void RedirectPermanentInternal(string url, string siteName)
        {
            URLHelper.RedirectPermanent(url, siteName);
        }


        /// <summary>
        /// Redirects the response using specified URL (302 HTTP status code).
        /// </summary>
        /// <param name="url">URL for redirection</param>
        protected virtual void RedirectInternal(string url)
        {
            URLHelper.Redirect(UrlResolver.ResolveUrl(url));
        }


        /// <summary>
        /// Rewrites the URL by using the given path, path information and query string information,
        /// without clearing the cached values related to URL of the request
        /// </summary>
        /// <param name="filePath">The internal rewrite path.</param>
        /// <param name="pathInfo">Additional path information for a resource.</param>
        /// <param name="queryString">The request query string.</param>
        protected virtual void RewritePathWithoutClearCachedUrlValuesInternal(string filePath, string pathInfo, string queryString)
        {
            CMSHttpContext.Current.RewritePath(filePath, pathInfo, queryString);
        }


        /// <summary>
        /// Rewrites the URL by using the given path
        /// </summary>
        /// <param name="path">Path to rewrite to</param>
        protected virtual void RewritePathInternal(string path)
        {
            CMSHttpContext.Current.RewritePath(path);
        }


        /// <summary>
        /// Rewrites the URL by using the given path
        /// </summary>
        /// <param name="path">Path to rewrite to</param>
        /// <param name="rebaseClientPath">True to reset the virtual path; false to keep the virtual path unchanged</param>
        protected virtual void RewritePathInternal(string path, bool rebaseClientPath)
        {
            CMSHttpContext.Current.RewritePath(path, rebaseClientPath);
        }


        /// <summary>
        /// Process the request by GetAttachment handler for attachments identified by node alias path.
        /// URL format: /getattachment/node/alias/path/filename{*.extension} rewrites to  CMSPages/getfile.aspx?filename=...aliaspath=...
        /// </summary>
        /// <returns>Return true if the request was processed by GetAttachment handler.</returns>
        private static bool HandledByGetAttachmentHandler(string relativePath, string query, UrlRewritingAction action)
        {
            if (RewriteGetAttachment(relativePath, query, action))
            {
                FileDomainRedirect();

                // Send the file by the GetAttachment handler
                ProcessByGetAttachmentHandler();

                return true;
            }

            return false;
        }


        private static void ProcessByGetAttachmentHandler()
        {
            // We need to remap handler to initialize session (see CM-4373 for details)
            var handler = Service.Resolve<IGetAttachmentHandler>();
            CMSHttpContext.Current.RemapHandler(handler);
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.DataEngine;
using CMS.Localization;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.EventLog;
using CMS.Routing.Web;
using CMS.SiteProvider;
using CMS.MacroEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// URL provider for the documents.
    /// </summary>
    public class DocumentURLProvider : AbstractBaseProvider<DocumentURLProvider>
    {
        #region "Constants"

        /// <summary>
        /// Do not process default page
        /// </summary>
        public const string DEFAULT_PAGE_NONE = "NONE";


        /// <summary>
        /// Use domain for default page
        /// </summary>
        public const string DEFAULT_PAGE_DOMAIN = "DOMAIN";


        /// <summary>
        /// Use default alias path page
        /// </summary>
        public const string DEFAULT_PAGE_PAGE = "PAGE";


        /// <summary>
        /// Use default page URL
        /// </summary>
        public const string DEFAULT_PAGE_DEFAULT = "DEFAULT";


        /// <summary>
        /// No extension constant
        /// </summary>
        public const string NO_EXTENSION = "##NONE##";


        /// <summary>
        /// Path to PresentationUrlRedirect handler.
        /// </summary>
        internal const string PRESENTATION_URL_HANDLER_PATH = "~/CMSModules/Content/CMSPages/PresentationUrlRedirect.ashx";

        #endregion


        #region Enumerations

        /// <summary>
        /// Constants for SSL options.
        /// </summary>
        private enum SSLOptions
        {
            /// <summary>
            /// Node inherits SSL setting from parent.
            /// </summary>
            INHERITS_SSL = -1,


            /// <summary>
            /// Node´s SSL setting is set to NO.
            /// </summary>
            DO_NOT_REQUIRE_SSL,


            /// <summary>
            /// Node´s SSL setting is set to YES.
            /// </summary>
            REQUIRE_SSL,


            /// <summary>
            /// Node´s SSL setting is set to NEVER.
            /// </summary>
            NEVER_REQUIRE_SSL
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Default (root) page URL.
        /// </summary>
        private static string mDefaultPageURL;

        /// <summary>
        /// Default wildcard value delimiter.
        /// </summary>
        private static char? mWildcardDefaultValueDelimiter;


        /// <summary>
        /// Indicates whether current wildcard query string value should be used as default value.
        /// </summary>
        private static bool? mUseCurrentWildcardValueAsDefaultValue;


        /// <summary>
        /// Regular expression for wildcard.
        /// </summary>
        private static Regex mWildcardRegex;

        /// <summary>
        /// Regular expression for escaping special regular chars if they are present in URL.
        /// </summary>
        private static Regex mEscapeRegex;

        /// <summary>
        /// Document URL provider dependencies.
        /// </summary>
        private DocumentURLProviderDependencies mDocumentUrlProviderDependencies;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for escaping special regular chars if they are present in URL.
        /// </summary>
        private static Regex EscapeRegex
        {
            get
            {
                return mEscapeRegex ?? (mEscapeRegex = RegexHelper.GetRegex("([\\$|\\(|\\)|\\^])"));
            }
        }


        /// <summary>
        /// Gets the regular expression for wildcard rule.
        /// </summary>
        public static Regex WildcardRegex
        {
            get
            {
                return mWildcardRegex ?? (mWildcardRegex = RegexHelper.GetRegex("{(.+?)}", RegexOptions.None));
            }
        }


        /// <summary>
        /// Default (root) page URL.
        /// </summary>
        public static string DefaultPageURL
        {
            get
            {
                return mDefaultPageURL ?? (mDefaultPageURL = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSDefaultPageURL"], "~/default.aspx"));
            }
            set
            {
                mDefaultPageURL = value;
            }
        }


        /// <summary>
        /// Document URL external dependencies
        /// </summary>
        internal DocumentURLProviderDependencies URLDependencies
        {
            get
            {
                return mDocumentUrlProviderDependencies;
            }
            set
            {
                mDocumentUrlProviderDependencies = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures domain prefix with dependence on specified site setting
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="siteName">Site name</param>
        public static string EnsureDomainPrefix(string domain, string siteName)
        {
            return ProviderObject.EnsureDomainPrefixInternal(domain, siteName);
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable if urlpath is not wildcard URL) and a specified site name.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URLU Path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="langPrefix">Lang prefix. If is empty current lang prefix is used</param>
        /// <param name="extension">URL extension</param>
        public static string GetUrl(string aliasPath, string urlPath = null, string siteName = null, string langPrefix = null, string extension = null)
        {
            return ProviderObject.GetUrlInternal(aliasPath, urlPath, siteName, langPrefix, extension);
        }


        /// <summary>
        /// Returns relative URL for the specified tree node.
        /// </summary>
        /// <param name="node">Tree node</param>
        public static string GetUrl(TreeNode node)
        {
            return ProviderObject.GetUrlInternal(node);
        }


        /// <summary>
        /// Returns presentation URL for the specified node. This is the absolute URL where live presentation of given node can be found.
        /// </summary>
        /// <param name="node">Tree node to return presentation URL for.</param>
        /// <param name="preferredDomainName">A preferred domain name that should be used as the host part of the URL.</param>
        public static string GetPresentationUrl(TreeNode node, string preferredDomainName = null)
        {
            return ProviderObject.GetPresentationUrlInternal(node, preferredDomainName);
        }


        /// <summary>
        /// Creates culture version of URLs for given node based on provided parameters.
        /// </summary>
        /// <param name="node">Node to generate culture specific URLs for.</param>
        /// <param name="originalUrl">Original URL which will be changed to culture specific.</param>
        /// <param name="excludedCultureCode">Culture code (for example "en-US"), for which culture specific URL will not be generated</param>
        /// <param name="options">Flag enum which influences final culture specific URL. By default only URL for translated pages is generated and URL is culture specific</param>
        public static List<DocumentCultureUrl> GetDocumentCultureUrls(TreeNode node, string originalUrl, string excludedCultureCode = "", UrlOptionsEnum options = UrlOptionsEnum.ExcludeUntranslatedDocuments | UrlOptionsEnum.UseCultureSpecificURLs)
        {
            var documentCultureURLs = new List<DocumentCultureUrl>();

            if (node == null)
            {
                return documentCultureURLs;
            }

            var siteName = node.NodeSiteName;
            var originalAliasPath = node.NodeAliasPath;
            excludedCultureCode = excludedCultureCode.ToLowerCSafe();

            var siteCultures = CultureSiteInfoProvider.GetSiteCultures(siteName);

            // Returns empty list if site is not multilingual
            if (DataHelper.DataSourceIsEmpty(siteCultures) || (siteCultures.Tables[0].Rows.Count <= 1))
            {
                return documentCultureURLs;
            }

            // Find culture versions of page
            TreeNodeCollection translatedDocuments = node.CultureVersions;

            // Loop thru all site cultures
            foreach (DataRow dr in siteCultures.Tables[0].Rows)
            {
                // Get culture data
                string cultureCode = dr["CultureCode"].ToString().ToLowerCSafe();
                string cultureShortName = dr["CultureShortName"].ToString();
                string cultureAlias = Convert.ToString(dr["CultureAlias"]);
                TreeNode translatedDocument = null;

                if (translatedDocuments != null)
                {
                    translatedDocument = translatedDocuments.FirstOrDefault(item => item.DocumentCulture.ToLowerCSafe().EqualsCSafe(cultureCode));
                }

                // Document is not translated to language but translation is required or culture is excluded and URL for culture should not be created
                if ((options.HasFlag(UrlOptionsEnum.ExcludeUntranslatedDocuments) && (translatedDocument == null)) || excludedCultureCode.EqualsCSafe(cultureCode))
                {
                    continue;
                }

                // Culture alias has higher priority than culture code
                string lang = String.IsNullOrEmpty(cultureAlias) ? cultureCode : cultureAlias;
                string documentUrlPath = (translatedDocument == null) ? "" : translatedDocument.DocumentUrlPath;
                string redirectDomain = GetRedirectDomain(siteName, cultureCode);

                string url;
                if (options.HasFlag(UrlOptionsEnum.UseCultureSpecificURLs))
                {
                    url = GetCultureSpecificUrl(redirectDomain, siteName, originalAliasPath, originalUrl, documentUrlPath, lang);
                }
                else
                {
                    url = GetLanguageParameterBasedUrl(redirectDomain, originalUrl, lang);
                }

                // Add new DocumentCultureURL to the collection
                var documentCultureURL = new DocumentCultureUrl
                {
                    CultureCode = cultureCode,
                    CultureName = cultureShortName,
                    Url = url
                };

                documentCultureURLs.Add(documentCultureURL);
            }

            return documentCultureURLs;
        }


        /// <summary>
        /// Returns permanent document URL.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="nodeAlias">Node alias</param>
        /// <param name="siteName">Site name</param>
        /// <param name="prefix">URL prefix, if null, "getdoc" is used, the prefix must be supported by the URL rewriting engine</param>
        /// <param name="extension">Extension for the URL, if null, it is taken from the Friendly URL settings</param>
        public static string GetPermanentDocUrl(Guid nodeGuid, string nodeAlias, string siteName, string prefix = null, string extension = null)
        {
            return ProviderObject.GetPermanentDocUrlInternal(nodeGuid, nodeAlias, siteName, prefix, extension);
        }


        /// <summary>
        /// Indicates if permanent URLs should be generated for documents and attachments
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UsePermanentUrls(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUsePermanentURLs");
        }


        /// <summary>
        /// Returns resolved (absolute) URL of a page represented by data container. Method reflects page navigation settings.
        /// </summary>
        /// <param name="container">Data container that represents a node (page)</param>
        /// <param name="resolver">Macro resolver to resolve macros in document properties</param>
        public static string GetNavigationUrl(ISimpleDataContainer container, MacroResolver resolver)
        {
            if (container == null)
            {
                return String.Empty;
            }

            var siteId = container["NodeSiteID"].ToInteger(0);
            var alias = container["NodealiasPath"].ToString(String.Empty);
            var culture = container["DocumentCulture"].ToString(String.Empty);
            var redirectionUrl = container["DocumentMenuRedirectUrl"].ToString(String.Empty);
            var redirectToFirstChild = container["DocumentMenuRedirectToFirstChild"].ToBoolean(false);

            var siteName = SiteInfoProvider.GetSiteName(siteId);

            var url = TreePathUtils.GetRedirectionUrl(siteName, alias, culture, redirectionUrl, redirectToFirstChild, resolver);

            // Further processing if redirection URL is empty
            if (String.IsNullOrEmpty(url))
            {
                var urlPath = container["DocumentUrlPath"].ToString(String.Empty);
                var isLink = container["NodeLinkedNodeID"].ToInteger(0) > 0;

                if (isLink)
                {
                    urlPath = null;
                }

                // Get URL with dependence on site name
                url = GetUrl(alias, urlPath, siteName);
            }

            return URLHelper.ResolveUrl(url);
        }


        /// <summary>
        /// Returns a relative URL path to PresentationUrlRedirect handler with parameters based on which the live page URL can be generated when needed.
        /// </summary>
        /// <param name="cultureName">Document culture.</param>
        /// <param name="nodeID">Node ID as an unique identifier.</param>
        /// <param name="query">Additional query parameters (optional).</param>
        public static string GetPresentationUrlHandlerPath(string cultureName, int nodeID, string query = "")
        {
            var url = URLHelper.AddParameterToUrl(PRESENTATION_URL_HANDLER_PATH, "cultureName", cultureName);
            url = URLHelper.AddParameterToUrl(url, "nodeid", ValidationHelper.GetString(nodeID, ""));
            url = URLHelper.AppendQuery(url, query);
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url, false));

            return url;
        }


        /// <summary>
        /// Returns extension for given document. Takes document custom extensions and also 'Files friendly URL extension' and 'Friendly URL extensions' settings into account.
        /// </summary>
        /// <param name="node">Document to determine extension for.</param>
        public static string GetExtension(ITreeNode node)
        {
            var siteName = node.NodeSiteName;

            // Get extension from settings
            var extension = node.IsFile() ? TreePathUtils.GetFilesUrlExtension(siteName) : TreePathUtils.GetUrlExtension(siteName);
            if (string.IsNullOrEmpty(extension))
            {
                // Use first custom extension when no extension found in the settings
                extension = node.DocumentExtensions;
                var separatorPosition = extension.IndexOfCSafe(';');
                if (separatorPosition >= 0)
                {
                    extension = extension.Substring(0, separatorPosition);
                }
            }

            return extension;
        }


        /// <summary>
        /// Gets absolute live site URL according to SSL option of node and ensure proper port if custom port for Http/Https is defined.
        /// </summary>
        /// <remarks>If site is content only, SSL options and custom SSL port policies are not enforced.</remarks>
        /// <param name="node">Tree node to return absolute live site URL for.</param>
        public static string GetAbsoluteLiveSiteURL(TreeNode node)
        {
            UriBuilder presentationUri;
            try
            {
                presentationUri = new UriBuilder(GetPresentationUrl(node, RequestContext.URL.Host));
            }
            catch (UriFormatException ex)
            {
                EventLogProvider.LogException(EventType.ERROR, "Malformed presentation URL", ex);
                return null;
            }

            // Do not enforce RequireSSL policy if site is content only
            var site = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (site.SiteIsContentOnly)
            {
                return presentationUri.Uri.AbsoluteUri;
            }

            var nodeSSL = (SSLOptions)ValidationHelper.GetInteger(node.RequiresSSL, (int)SSLOptions.INHERITS_SSL);
            if (nodeSSL == SSLOptions.INHERITS_SSL)
            {
                nodeSSL = (SSLOptions)ValidationHelper.GetInteger(node.GetInheritedValue("RequiresSSL"), (int)SSLOptions.DO_NOT_REQUIRE_SSL);
            }

            int port;
            string uriScheme;
            bool requireSSL = nodeSSL == SSLOptions.REQUIRE_SSL;

            if (RequestContext.IsSSL == requireSSL)
            {
                port = RequestContext.URL.Port;
                uriScheme = RequestContext.URL.Scheme;
            }
            else
            {
                port = URLHelper.DEFAULT_HTTP_PORT;
                uriScheme = Uri.UriSchemeHttp;
                if (requireSSL)
                {
                    port = URLHelper.DEFAULT_HTTPS_PORT;
                    uriScheme = Uri.UriSchemeHttps;
                }
            }

            presentationUri.Port = port;
            presentationUri.Scheme = uriScheme;

            return URLHelper.EnsureProperPort(requireSSL, presentationUri.Uri).AbsoluteUri;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns URL for the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="langPrefix">Language prefix. If is empty, current language prefix is used</param>
        /// <param name="extension">Extension</param>
        internal virtual string GetUrlForPathInternal(string path, string siteName, string langPrefix, string extension)
        {
            // Process the extension
            if (String.IsNullOrEmpty(extension))
            {
                extension = TreePathUtils.GetUrlExtension(siteName);
            }
            if (extension == NO_EXTENSION)
            {
                extension = null;
            }

            var urlDependencies = GetURLDependencies();

            // Do not use any URL option for on-site edit mode
            if (urlDependencies.ViewMode.IsEditLive())
            {
                return "~" + path + extension;
            }

            string currentSiteName = urlDependencies.CurrentSiteName;
            var useContextValues = (String.IsNullOrEmpty(siteName) || currentSiteName.EqualsCSafe(siteName, true));

            // Get prefix from the context
            string prefix = useContextValues ? urlDependencies.CurrentURLPathPrefix.Trim('/') : String.Empty;

            // Try get site specific prefix
            if (String.IsNullOrEmpty(prefix))
            {
                prefix = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaultUrlPathPrefix").Trim('/');
            }

            if (!String.IsNullOrEmpty(prefix))
            {
                prefix = "/" + prefix;
            }

            string cultureprefix = langPrefix;

            // Try get culture prefix directly if request is for document on current site or site is not specified
            if (String.IsNullOrEmpty(cultureprefix) && useContextValues)
            {
                // Try get culture prefix from the context
                cultureprefix = urlDependencies.CurrentURLLangPrefix;
            }

            if (!String.IsNullOrEmpty(cultureprefix))
            {
                cultureprefix = "/" + cultureprefix;
            }

            string defaultPage = String.Empty;

            // Get default page setting option
            string defaultPageSetting = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaulPage");

            // Check whether default page handling is required
            if (!defaultPageSetting.EqualsCSafe(DEFAULT_PAGE_NONE))
            {
                // Get default alias path
                string defaultAliasPath = PageInfoProvider.GetDefaultAliasPath(urlDependencies.CurrentDomain, String.IsNullOrEmpty(siteName) ? currentSiteName : siteName);
                // Check whether path is equal to default alias path
                if ((path == "/") || (path.EqualsCSafe(defaultAliasPath, true)))
                {
                    path = String.Empty;

                    // Switch by page settings
                    switch (defaultPageSetting)
                    {
                        // Use domain
                        case DEFAULT_PAGE_DOMAIN:
                            defaultPage = "~/";
                            break;

                        // Use default alias path page
                        case DEFAULT_PAGE_PAGE:
                            path = defaultAliasPath;
                            break;

                        // Use default page URL
                        case DEFAULT_PAGE_DEFAULT:
                            defaultPage = DefaultPageURL;
                            break;
                    }
                }
            }

            // Site root
            if (String.IsNullOrEmpty(path) || (path == "/"))
            {
                if (String.IsNullOrEmpty(defaultPage))
                {
                    defaultPage = DefaultPageURL;
                }

                // Process path prefix and lang prefix only for relative paths
                if (defaultPage.StartsWithCSafe("~/"))
                {
                    defaultPage = "~" + prefix + cultureprefix + defaultPage.TrimStart('~');
                }

                // Default path is composed from either path prefix or culture prefix or both so there's need to apply SEO settings.
                if (!defaultPage.Equals("~/", StringComparison.Ordinal))
                {
                    // Correct trailing slash
                    switch (URLHelper.UseURLsWithTrailingSlash(siteName))
                    {
                        case TrailingSlashEnum.Always:
                            defaultPage = defaultPage.TrimEnd('/') + "/";
                            break;
                        case TrailingSlashEnum.Never:
                            defaultPage = defaultPage.TrimEnd('/');
                            break;
                    }

                    // Correct URL case
                    defaultPage = EnsureCorrectCase(defaultPage, siteName);
                }

                // Return URL for default page
                return defaultPage;
            }
            // Regular page
            else
            {
                // Make the path
                string url = "~" + prefix + cultureprefix + path + extension;

                // Add trailing slash if it is required
                TrailingSlashEnum slash = URLHelper.UseURLsWithTrailingSlash(siteName);
                if ((slash == TrailingSlashEnum.Always) && !url.EndsWithCSafe("/") && (!URLHelper.UseTrailingSlashOnlyForExtensionLess || String.IsNullOrEmpty(extension)))
                {
                    url += "/";
                }

                // Correct URL case according to settings
                url = EnsureCorrectCase(url, siteName);

                return url;
            }
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable if urlpath is not wildcard URL) and a specified site name.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL Path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="langPrefix">Lang prefix. If is empty, current lang prefix is used</param>
        /// <param name="extension">URL extension</param>
        protected virtual string GetUrlInternal(string aliasPath, string urlPath, string siteName, string langPrefix, string extension)
        {
            // Get path to use
            var path = GetPreferredPathForUrl(aliasPath, urlPath, ref extension);

            // Get relative link for current site
            string currentSiteName = SiteContext.CurrentSiteName;
            if (String.IsNullOrEmpty(siteName) || currentSiteName.EqualsCSafe(siteName, true))
            {
                siteName = currentSiteName;
                return GetUrlForPathInternal(path, siteName, langPrefix, extension);
            }

            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if ((si == null) || (HttpContext.Current == null))
            {
                // Get with global site settings if site not found
                return GetUrlForPathInternal(path, null, langPrefix, extension);
            }

            // Prepare the path parts
            string appPath = SystemContext.ApplicationPath.TrimEnd('/');
            string domain = EnsureDomainPrefix(si.DomainName, siteName);

            if (domain.Contains("/"))
            {
                // If domain contains the application path, do not add it
                appPath = null;
            }

            // Get the relative URL
            string url = GetUrlForPathInternal(path, siteName, langPrefix, extension);

            // Get the absolute link
            return RequestContext.CurrentScheme + "://" + si.DomainName + appPath + "/" + url.TrimStart(new[] { '~', '/' });
        }


        /// <summary>
        /// Returns relative URL for the specified tree node.
        /// </summary>
        /// <param name="node">Tree node</param>
        protected virtual string GetUrlInternal(TreeNode node)
        {
            if (node.NodeIsContentOnly)
            {
                // Resolve macros in URL pattern and return relative URL
                var resolver = MacroResolver.GetInstance(false);
                resolver.SetAnonymousSourceData(node);
                var url = resolver.ResolveMacros(node.DataClassInfo.ClassURLPattern);

                return string.IsNullOrEmpty(url) ? "~/" : "~" + url;
            }

            // Handle root node of content only site
            if (node.IsRoot() && (node.Site != null) && node.Site.SiteIsContentOnly)
            {
                return "~/";
            }

            // Get extension for node
            var extension = GetExtension(node);

            // Prepare language prefix if enabled
            var languagePrefix = GetLanguagePrefix(node);

            // Don't use document URL path for linked document
            var documentUrlPath = node.IsLink ? null : node.DocumentUrlPath;

            // Select usable path
            var path = GetPreferredPathForUrl(node.NodeAliasPath, documentUrlPath, ref extension);

            // Get relative link
            return GetUrlForPathInternal(path, node.NodeSiteName, languagePrefix, extension);
        }


        /// <summary>
        /// Returns presentation URL for the specified node. This is the absolute URL where live presentation of given node can be found.
        /// </summary>
        /// <param name="node">Tree node to return presentation url for.</param>
        /// <param name="preferredDomainName">A preferred domain name that should be used as the host part of the URL.</param>
        protected virtual string GetPresentationUrlInternal(TreeNode node, string preferredDomainName = null)
        {
            if (node == null)
            {
                return null;
            }

            SiteInfo site = node.Site;
            string url = GetUrl(node);

            if (!String.IsNullOrEmpty(site.SitePresentationURL))
            {
                return URLHelper.CombinePath(url, '/', site.SitePresentationURL, null);
            }

            if (!String.IsNullOrEmpty(preferredDomainName))
            {
                return URLHelper.GetAbsoluteUrl(url, preferredDomainName);
            }

            return URLHelper.GetAbsoluteUrl(url, site.DomainName);
        }


        /// <summary>
        /// Returns permanent document URL.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="nodeAlias">Node alias</param>
        /// <param name="siteName">Site name</param>
        /// <param name="prefix">URL prefix, if null, "getdoc" is used, the prefix must be supported by the URL rewriting engine</param>
        /// <param name="extension">Extension for the URL, if null, it is taken from the Friendly URL settings</param>
        protected virtual string GetPermanentDocUrlInternal(Guid nodeGuid, string nodeAlias, string siteName, string prefix, string extension)
        {
            // Get the extension if not specified
            if (extension == null)
            {
                extension = URLHelper.GetFriendlyExtension(siteName);

                // Ensure extension format
                if (extension != "")
                {
                    extension = "." + extension.TrimStart('.');
                }
            }

            // Ensure non-empty node alias
            if (String.IsNullOrEmpty(nodeAlias))
            {
                nodeAlias = "default";
            }

            // Ensure the default prefix
            prefix = prefix == null ? "getdoc" : prefix.Trim('/');

            return "~/" + prefix + "/" + Convert.ToString(nodeGuid) + "/" + nodeAlias + extension;
        }

        #endregion


        #region "Wildcard methods & properties"

        /// <summary>
        /// Gets the delimiter for default wildcard value.
        /// </summary>
        public static char WildcardDefaultValueDelimiter
        {
            get
            {
                if (mWildcardDefaultValueDelimiter == null)
                {
                    mWildcardDefaultValueDelimiter = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSWildcardDefaultValueDelimiter"], ";")[0];
                }

                return mWildcardDefaultValueDelimiter.Value;
            }
            set
            {
                mWildcardDefaultValueDelimiter = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current wildcard query string value should be used as default value.
        /// </summary>
        public static bool UseCurrentWildcardValueAsDefaultValue
        {
            get
            {
                if (mUseCurrentWildcardValueAsDefaultValue == null)
                {
                    mUseCurrentWildcardValueAsDefaultValue = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseCurrentWildcardValueAsDefaultValue"], true);
                }
                return mUseCurrentWildcardValueAsDefaultValue.Value;
            }
        }


        /// <summary>
        /// Replace wildcard match if contain default value definition.
        /// </summary>
        /// <param name="m">Regex match</param>
        private static string WildcardMatch(Match m)
        {
            string wildCard = m.Value.Trim('{', '}');

            // Return empty string for wildcard with hidden default value
            bool hidden = wildCard.StartsWithCSafe("*") && wildCard.EndsWithCSafe("*");
            if (hidden)
            {
                return string.Empty;
            }

            // Split by default value delimiter
            string[] values = m.Value.Split(WildcardDefaultValueDelimiter);

            // If default value is not defined try get value from query string
            if (UseCurrentWildcardValueAsDefaultValue)
            {
                string queryValue = QueryHelper.GetString(values[0].Trim('{', '}'), String.Empty);
                if (!String.IsNullOrEmpty(queryValue))
                {
                    return URLHelper.GetSafeUrlPart(queryValue, SiteContext.CurrentSiteName);
                }
            }

            // Check whether wildcard contains default value
            if (values.Length == 2)
            {
                // Get default value and trim wildcard closing bracket
                string value = values[1].TrimEnd('}');

                // Return default value if is not empty
                if (!String.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            // By default return full wildcard
            return m.Value;
        }


        /// <summary>
        /// Ensures URL path with wildcards. The wildcard with default value are replace to the default value.
        /// </summary>
        /// <param name="path">URL path</param>
        public static string EnsureWildcardPath(string path)
        {
            // Check whether path is defined and contains wildcard character
            if (ContainsWildcard(path))
            {
                path = WildcardRegex.Replace(path, WildcardMatch);
            }

            return path;
        }


        /// <summary>
        /// Checks if path contains a wildcard.
        /// </summary>
        /// <param name="path">URL path</param>
        internal static bool ContainsWildcard(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.Contains("{");
        }


        /// <summary>
        /// Returns query string for wildcard url. Wildcard is in format /{name
        /// </summary>
        /// <param name="urlPath">Url path from database</param>
        /// <param name="currentPath">Current requested url path</param>
        public static string BuildWildcardQueryString(string urlPath, string currentPath)
        {
            string wildcardQueryString = String.Empty;

            ArrayList queryKey = new ArrayList();

            // Get the matched wildcards
            MatchCollection matches = WildcardRegex.Matches(urlPath);
            for (int i = 0; i < matches.Count; i++)
            {
                queryKey.Add(matches[i].Result("$1"));
            }

            // Escape regular expression characters
            urlPath = EscapeRegex.Replace(urlPath, "\\$1");

            // Prepare pattern
            string pattern = WildcardRegex.Replace(urlPath, "(.*)");
            Regex re = RegexHelper.GetRegex(pattern, true);

            GroupCollection groups = re.Match(currentPath).Groups;
            for (int i = 0; i < groups.Count; i++)
            {
                // Groups[0] is whole matched string 
                if (i != 0)
                {
                    // Test if matched value contains slash
                    string value = groups[i].Value;
                    if (value.Contains("/"))
                    {
                        return null;
                    }

                    string currentQueryKey = ValidationHelper.GetString(queryKey[i - 1], String.Empty);

                    // Check whether default wildcard value is defined and if so, remove it
                    int delimiterIndex = currentQueryKey.IndexOfCSafe(WildcardDefaultValueDelimiter);
                    if (delimiterIndex > 0)
                    {
                        currentQueryKey = currentQueryKey.Substring(0, delimiterIndex);
                    }

                    // Build wildcard query string
                    wildcardQueryString += "&" + currentQueryKey + "=" + HttpUtility.UrlEncode(value);
                }
            }

            return wildcardQueryString;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns URL dependencies. If not specified by the URLDependencies property then creates new instance with values from the context.
        /// </summary>
        private DocumentURLProviderDependencies GetURLDependencies()
        {
            return mDocumentUrlProviderDependencies ?? new DocumentURLProviderDependencies(RequestContext.CurrentURLLangPrefix, RequestContext.CurrentURLPathPrefix, RequestContext.CurrentDomain, PortalContext.ViewMode, SiteContext.CurrentSiteName);
        }


        private static string GetRedirectDomain(string siteName, string cultureCode)
        {
            string redirectDomain = "";
            if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseDomainForCulture") && (PortalContext.ViewMode != ViewModeEnum.EditLive))
            {
                redirectDomain = SiteInfoProvider.GetDomainForCulture(siteName, cultureCode);
                if (!String.IsNullOrEmpty(redirectDomain))
                {
                    redirectDomain = EnsureDomainPrefix(redirectDomain, siteName);
                }
            }

            return redirectDomain;
        }


        /// <summary>
        /// Adds language parameter to the URL. URL is resolved if domain name is defined.
        /// </summary>
        /// <param name="domainName">Domain name in the URL. If empty, relative URL is returned.</param>
        /// <param name="originalUrl">URL where language parameter will be added.</param>
        /// <param name="lang">Language parameter.</param>
        private static string GetLanguageParameterBasedUrl(string domainName, string originalUrl, string lang)
        {
            string url = originalUrl;
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.LanguageParameterName);
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.AliasPathParameterName);

            if (String.IsNullOrEmpty(domainName))
            {
                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, lang);
            }

            return GetUrlWithDomain(url, domainName);
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable if urlPath is not wildcard URL) and a specified site name.
        /// Language parameter is added to the URL if urlPath or langPrefix is not defined. 
        /// URL is resolved if domainName is defined.
        /// </summary>
        /// <param name="domainName">Domain name in the URL. If empty, relative URL is returned.</param>
        /// <param name="siteName">Site to find language prefix for.</param>
        /// <param name="originalAliasPath">Alias path to create URL.</param>
        /// <param name="originalUrl">URL where language parameter will be added.</param>
        /// <param name="documentUrlPath">Document URL path. If defined it has higher priority than alias path </param>
        /// <param name="lang">Language parameter.</param>
        private static string GetCultureSpecificUrl(string domainName, string siteName, string originalAliasPath, string originalUrl, string documentUrlPath, string lang)
        {
            string langPrefix = URLHelper.UseLangPrefixForUrls(siteName) ? lang : String.Empty;
            string url = GetUrl(originalAliasPath, documentUrlPath, siteName, langPrefix);
            url += URLHelper.GetQuery(originalUrl);
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.LanguageParameterName);
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.AliasPathParameterName);

            // if domain is not specified use lang parameter
            if (String.IsNullOrEmpty(domainName) && String.IsNullOrEmpty(documentUrlPath) && String.IsNullOrEmpty(langPrefix))
            {
                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, lang);
            }

            return GetUrlWithDomain(url, domainName);
        }


        /// <summary>
        /// Replaces original domain name in URL with a new one.
        /// </summary>
        /// <param name="url">URL to be resolved with new domain name.</param>
        /// <param name="domainName">Domain name to resolve URL with.</param>
        private static string GetUrlWithDomain(string url, string domainName)
        {
            if (!string.IsNullOrEmpty(domainName))
            {
                url = URLHelper.UnResolveUrl(url, SystemContext.ApplicationPath);
                url = URLHelper.GetAbsoluteUrl(url, domainName);
            }

            return url;
        }


        /// <summary>
        /// Ensures domain prefix with dependence on specified site setting
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="siteName">Site name</param>
        protected virtual string EnsureDomainPrefixInternal(string domain, string siteName)
        {
            if (!URLHelper.ContainsProtocol(domain))
            {
                // Get setting for domain prefix
                string processDomainprefix = SettingsKeyInfoProvider.GetValue(siteName + ".CMSProcessDomainPrefix");

                // Switch by domain prefix preferences 
                switch (processDomainprefix)
                {
                    // Always use WWW prefix
                    case DomainPolicy.DOMAIN_PREFIX_WWW:
                        if (!domain.StartsWithCSafe("www.") && !URLHelper.IsTldDomain(domain))
                        {
                            domain = "www." + domain;
                        }
                        break;

                    // Never use WWW prefix
                    case DomainPolicy.DOMAIN_PREFIX_WITHOUTWWW:
                        if (domain.StartsWithCSafe("www.", true))
                        {
                            domain = domain.Substring(4);
                        }
                        break;
                }
            }
            return domain;
        }


        /// <summary>
        /// Selects preferred path (alias or URL) for use in URLs.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="urlPath">Document Url path</param>
        /// <param name="extension">Url extension</param>
        private string GetPreferredPathForUrl(string aliasPath, string urlPath, ref string extension)
        {
            if (String.IsNullOrEmpty(urlPath))
            {
                return aliasPath;
            }

            // Remove the prefix from URL path
            string prefix;
            TreePathUtils.ParseUrlPath(ref urlPath, out prefix, null);

            if (!String.IsNullOrEmpty(prefix))
            {
                extension = NO_EXTENSION;
            }

            // Ensure wildcard path
            urlPath = EnsureWildcardPath(urlPath);

            // If there is still some wildcard, do not use URL path as the main URL
            if (urlPath.Contains("{"))
            {
                urlPath = "";
            }

            // Get path to use
            return String.IsNullOrEmpty(urlPath) ? aliasPath : urlPath;
        }


        /// <summary>
        /// Returns language prefix for given document if language prefixes are enabled. Returns null otherwise.
        /// </summary>
        /// <param name="node">Document to get language prefix for.</param>
        private string GetLanguagePrefix(TreeNode node)
        {
            // Check if language prefixes are enabled
            if (URLHelper.UseLangPrefixForUrls(node.NodeSiteName))
            {
                var cultureInfo = CultureInfoProvider.GetCultureInfo(node.DocumentCulture);
                if (cultureInfo != null)
                {
                    return String.IsNullOrEmpty(cultureInfo.CultureAlias) ? cultureInfo.CultureCode : cultureInfo.CultureAlias;
                }
            }

            return null;
        }


        /// <summary>
        /// Changes the case of given <paramref name="url"/> according to site's SEO settings.
        /// </summary>
        /// <param name="url">URL to be corrected</param>
        /// <param name="siteName">Site name</param>
        private string EnsureCorrectCase(string url, string siteName)
        {
            CaseRedirectEnum redirect = URLHelper.GetCaseRedirectEnum(siteName);

            // Switch by case settings
            switch (redirect)
            {
                // Lower case URL
                case CaseRedirectEnum.LowerCase:
                    url = url.ToLowerCSafe();
                    break;

                // Upper case URL
                case CaseRedirectEnum.UpperCase:
                    url = url.ToUpperCSafe();
                    break;
            }

            return url;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Localization;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides information on the page that are necessary for its processing.
    /// </summary>
    /// <remarks>The class uses cache (if it's available) to store information about pages.</remarks>
    public class PageInfoProvider
    {
        #region "Constants"

        /// <summary>
        /// GetDoc prefix for CMS pages (pages which require authentication).
        /// </summary>
        public const string PREFIX_CMS_GETDOC = "/cms/getdoc/";


        // Columns required for the page info
        private const string SELECT_COLUMNS = @"
ClassName,

NodeID, NodeAliasPath, NodeName, NodeAlias, NodeClassID, NodeParentID, NodeLevel, NodeACLID, NodeSiteID, NodeGUID, NodeOrder, NodeLinkedNodeSiteID, 
IsSecuredNode, NodeCacheMinutes, NodeSKUID, NodeDocType, NodeHeadTags, NodeInheritPageLevels, NodeHasChildren, NodeBodyElementAttributes, RequiresSSL, 
NodeLinkedNodeID, NodeOwner, NodeTemplateForAllCultures, NodeInheritPageTemplate, NodeAllowCacheInFileSystem, NodeTemplateID, NodeGroupID, NodeBodyScripts,

DocumentMenuClass, DocumentMenuItemInactive, DocumentMenuStyle, DocumentMenuItemHideInNavigation, DocumentContent, DocumentStylesheetID, DocumentInheritsStylesheet, DocumentID, 
DocumentName, DocumentNamePath, DocumentPublishFrom, DocumentPublishTo, DocumentUrlPath, DocumentCulture, DocumentPageTitle, DocumentPageKeyWords, DocumentPageDescription, 
DocumentMenuCaption, DocumentPageTemplateID, DocumentMenuRedirectUrl, DocumentMenuJavascript, DocumentCheckedOutVersionHistoryID, DocumentPublishedVersionHistoryID, 
DocumentWorkflowStepID, DocumentExtensions, DocumentWebParts, DocumentGroupWebParts, DocumentTrackConversionName, DocumentConversionValue, 
DocumentWorkflowCycleGUID, DocumentGUID, DocumentSearchExcluded, DocumentLogVisitActivity, DocumentMenuRedirectToFirstChild";

        #endregion


        #region "Variables"

        private static string[] mGetDocPrefixes;
        private static bool? mUseCultureForBestPageInfoResult;
        private static IQueryColumn[] mSelectColumnList;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether culture should be used for
        /// selecting best result instead of URL path.
        /// </summary>
        public static bool UseCultureForBestPageInfoResult
        {
            get
            {
                if (mUseCultureForBestPageInfoResult == null)
                {
                    mUseCultureForBestPageInfoResult = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseCultureForBestPageInfoResult"], false);
                }

                return mUseCultureForBestPageInfoResult.Value;
            }
            set
            {
                mUseCultureForBestPageInfoResult = value;
            }
        }


        /// <summary>
        /// Prefixes for the available permanent document URLs.
        /// </summary>
        public static string[] GetDocPrefixes
        {
            get
            {
                return mGetDocPrefixes ?? (mGetDocPrefixes = new[] { "/getdoc/", PREFIX_CMS_GETDOC });
            }
            set
            {
                mGetDocPrefixes = value;
            }
        }


        /// <summary>
        /// List of columns required for the page info
        /// </summary>
        private static IQueryColumn[] SelectColumnList
        {
            get
            {
                if (mSelectColumnList != null)
                {
                    return mSelectColumnList;
                }

                mSelectColumnList = new QueryColumnList(SELECT_COLUMNS).ToArray();
                return mSelectColumnList;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the page info for the given Alias path or URL path. Use the overload to provide node ID for better performance if possible.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="cultureCode">Preferred culture code to be used to get the best matching candidate. 
        /// When the URL path is provided and there is a match by the URL path, the culture code of the matched candidate has higher priority than the given culture code and is used for the result.</param>
        /// <param name="urlPath">URL path</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture?</param>
        public static PageInfo GetPageInfo(string siteName, string aliasPath, string cultureCode, string urlPath, bool combineWithDefaultCulture)
        {
            return GetPageInfo(siteName, aliasPath, cultureCode, urlPath, 0, combineWithDefaultCulture);
        }


        /// <summary>
        /// Returns the page info for the given Alias path or URL path. If NodeID is provided, pageinfo retrieval is faster.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="cultureCode">Preferred culture code to be used to get the best matching candidate. 
        /// When the URL path is provided and there is a match by the URL path, the culture code of the matched candidate has higher priority than the given culture code and is used for the result.</param>
        /// <param name="urlPath">URL path</param>
        /// <param name="nodeId">Node ID of the page (when provided, retrieval from the database is faster, use to get parent document)</param>
        /// <param name="combineWithDefaultCulture">Indicates if the result should be combined with default site culture</param>
        public static PageInfo GetPageInfo(string siteName, string aliasPath, string cultureCode, string urlPath, int nodeId, bool combineWithDefaultCulture)
        {
            PageInfo result = null;

            string requestStockKey = CacheHelper.GetCacheItemName(null, siteName, aliasPath, CacheHelper.GetCultureCacheKey(cultureCode), urlPath, combineWithDefaultCulture);

            // Try to get from request cache
            if (RequestStockHelper.Contains(PageInfoCacheHelper.CACHE_KEY, requestStockKey, false))
            {
                result = (PageInfo)RequestStockHelper.GetItem(PageInfoCacheHelper.CACHE_KEY, requestStockKey, false);
            }
            else
            {
                // Try to get from cache
                int cacheMinutes = PageInfoCacheHelper.CacheMinutes(siteName);
                if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                {
                    cacheMinutes = 0;
                }

                string key = CacheHelper.GetCacheItemName(null, PageInfoCacheHelper.CACHE_KEY, requestStockKey).ToLowerCSafe();

                // Try to get data from cache
                using (var cs = new CachedSection<PageInfo>(ref result, cacheMinutes, true, key))
                {
                    if (cs.LoadData)
                    {
                        // Gets data from database
                        result = PageInfoDataProvider.GetDataForPageInfo(siteName, aliasPath, urlPath, nodeId, cultureCode, combineWithDefaultCulture);
                        if (result != null)
                        {
                            // Save to the cache
                            if (cs.Cached)
                            {
                                // Cache item depends on changing the document and template
                                string[] dependencies = PageInfoCacheHelper.GetDependencyCacheKeys(result);

                                cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                            }
                        }
                        else
                        {
                            // Do not cache if not found
                            cs.CacheMinutes = 0;
                        }

                        cs.Data = result;
                    }
                }

                // Add to current request
                RequestStockHelper.AddToStorage(PageInfoCacheHelper.CACHE_KEY, requestStockKey, result, false);
            }

            return result;
        }


        /// <summary>
        /// Gets a virtual page info instance based on the given page template info
        /// </summary>
        /// <param name="pti">Page template info</param>
        public static PageInfo GetVirtualPageInfo(PageTemplateInfo pti)
        {
            // Prepare virtual page info
            var pi = GetVirtualPageInfo(pti.PageTemplateId);
            pi.LoadPageTemplateInfo(pti);

            return pi;
        }


        /// <summary>
        /// Gets a virtual page info instance based on the given page template
        /// </summary>
        /// <param name="pageTemplateId">Page template ID</param>
        public static PageInfo GetVirtualPageInfo(int pageTemplateId)
        {
            return GetVirtualPageInfoInternal(pageTemplateId, SiteContext.CurrentSiteID);
        }


        /// <summary>
        /// Gets a virtual page info instance based on the given page template
        /// </summary>
        /// <param name="pageTemplateId">Page template ID</param>
        /// <param name="siteId">Current site ID</param>
        internal static PageInfo GetVirtualPageInfoInternal(int pageTemplateId, int siteId)
        {
            // Prepare virtual page info
            var pi = new PageInfo
            {
                DocumentCulture = CultureHelper.GetPreferredCulture(),
                ClassName = SystemDocumentTypes.Root,
                NodeAliasPath = String.Empty,
                NodeSiteID = siteId
            };

            pi.SetPageTemplateId(pageTemplateId);

            return pi;
        }


        /// <summary>
        /// Parses the provided URL and returns a PageInfo object describing the required page in best matching culture.
        /// </summary>
        /// <param name="url">Full URL of the request, including domain name, application, path and extension</param>
        /// <param name="cultureCode">Current culture code</param>
        /// <param name="defaultAliasPath">Default alias path to use in case of default page, if null the path is taken from the system configuration</param>
        /// <param name="combineWithDefaultCulture">Indicates if the culture version should be combined with default culture</param>
        /// <param name="isDocumentPage">Indicates if the URL stands for a document page</param>
        /// <param name="siteName">Site name</param>
        /// <returns>
        /// Returns null if no page exists in any culture version.
        /// </returns>
        public static PageInfo GetPageInfoForUrl(string url, string cultureCode, string defaultAliasPath, bool combineWithDefaultCulture, bool isDocumentPage, string siteName)
        {
            // Default source
            if (CMSHttpContext.Current == null)
            {
                return null;
            }

            // Try to get page info from cache
            int cacheMinutes = PageInfoCacheHelper.CacheMinutes(siteName);
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                cacheMinutes = 0;
            }

            var settings = new CacheSettings(cacheMinutes, PageInfoCacheHelper.CACHE_BYURL_KEY, siteName, url, CacheHelper.GetCultureCacheKey(cultureCode), defaultAliasPath, combineWithDefaultCulture, isDocumentPage);
            var result = CacheHelper.Cache(cs =>
                {
                    // keep original cache minutes & set no cache
                    double originalCacheMinutes = cs.CacheMinutes;
                    cs.CacheMinutes = 0;

                    // Get the data
                    PageInfoResult pageResult;

                    var pi = PageInfoDataProvider.GetDataForPageInfoForUrl(siteName, url, cultureCode, defaultAliasPath, combineWithDefaultCulture, isDocumentPage, out pageResult);

                    if ((pi != null) && (pageResult != null))
                    {
                        // Restore original cache minutes
                        cs.CacheMinutes = originalCacheMinutes;

                        // Store the page result data in the current page info object
                        pi.SetPageResult(url, pageResult);

                        // Save to the cache
                        if (cs.Cached)
                        {
                            // Cache item depends on changing the document and template
                            string[] dependencies = PageInfoCacheHelper.GetDependencyCacheKeys(pi);

                            cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                        }
                    }
                    else
                    {
                        // Do not cache
                        cs.CacheMinutes = 0;
                    }

                    return pi;
                }, settings);

            // Set the current URL to ensure that the PageInfo.PageResult will work with the correct PageInfoResult object (ensures correct document alias data)
            DocumentContext.PageResultUrlPath = url;

            return result;
        }


        /// <summary>
        /// Removes prefix from path if is defined.
        /// </summary>
        /// <param name="siteName">Sitename</param>
        /// <param name="path">Path</param>
        /// <returns>Returnes TRUE if the prefix was removed.</returns>
        public static bool RemovePathPrefix(string siteName, ref string path)
        {
            return PathPrefixRemoval.RemovePathPrefix(siteName, ref path);
        }


        /// <summary>
        /// Returns culture code or culture alias defined in URL and removes it from input path.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="siteName">Site name</param>
        public static string RemoveCulturePrefix(string siteName, ref string path)
        {
            return PathPrefixRemoval.RemoveCulturePrefix(siteName, ref path);
        }


        /// <summary>
        /// Returns culture code or culture alias defined in URL and removes it from input path.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="ci">Culture info for current culture prefix in URL</param>
        public static string RemoveCulturePrefix(string siteName, ref string path, ref CultureInfo ci)
        {
            return PathPrefixRemoval.RemoveCulturePrefix(siteName, ref path, ref ci);
        }


        /// <summary>
        /// Returns cache info of the parent page.
        /// </summary>
        /// <param name="currentPage">Current page info</param>
        /// <returns>Returns false if no parent object exists with not null cache info.</returns>
        public static int GetParentCachePageInfo(PageInfo currentPage)
        {
            if (currentPage == null)
            {
                return -1;
            }

            return GetParentProperty<int>(currentPage.NodeSiteID, currentPage.NodeAliasPath, "NodeCacheMinutes");
        }


        /// <summary>
        /// Returns SSL info of the parent page.
        /// </summary>
        /// <param name="currentPage">Current page info</param>
        /// <returns>Returns false if no parent object exists with not null secured info.</returns>
        public static bool GetParentSSLPageInfo(PageInfo currentPage)
        {
            if (currentPage == null)
            {
                return false;
            }

            return GetParentProperty<bool>(currentPage.NodeSiteID, currentPage.NodeAliasPath, "RequiresSSL");
        }


        /// <summary>
        /// Returns value inherited from parent in given culture for given column name.
        /// </summary>
        /// <param name="siteId">SiteId</param>
        /// <param name="nodeAlias">Alias path</param>
        /// <param name="column">Column name</param>
        /// <param name="culture">Document culture</param>
        /// <param name="notEmptyCondition">Where condition indicating if inherited value is present</param>
        public static ReturnType GetParentProperty<ReturnType>(int siteId, string nodeAlias, string column, string culture = null, WhereCondition notEmptyCondition = null)
        {
            var value = TreePathUtils.GetNodeInheritedValueInternal(siteId, nodeAlias, column, culture, notEmptyCondition);
            return ValidationHelper.GetValue<ReturnType>(value);
        }


        /// <summary>
        /// Returns secured info of the parent page.
        /// </summary>
        /// <param name="currentPage">Current page info</param>
        /// <returns>Returns false if no parent object exists with not null secured info.</returns>
        public static bool GetParentSecurePageInfo(PageInfo currentPage)
        {
            if (currentPage == null)
            {
                return false;
            }

            return GetParentProperty<bool>(currentPage.NodeSiteID, currentPage.NodeAliasPath, "IsSecuredNode");
        }


        /// <summary>
        /// Gets the Page info object that defines the page template used by the given page info object.
        /// </summary>
        /// <param name="pi">Page info for which the template source should be evaluated</param>
        /// <param name="culture">Culture to use for retrieving the information</param>
        /// <param name="combineWithDefaultCulture">Combine the results with default culture?</param>
        public static PageInfo GetTemplateSourcePageInfo(PageInfo pi, string culture, bool combineWithDefaultCulture)
        {
            if (pi == null)
            {
                return null;
            }

            // Get the site
            var site = SiteInfoProvider.GetSiteInfo(pi.NodeSiteID);
            if (site == null)
            {
                throw new Exception("[PageInfoProvider.GetTemplateSourcePageInfo]: Node site not found.");
            }

            // Get the inherited template
            string aliasPath = pi.NodeAliasPath;

            while (pi.GetUsedPageTemplateId() <= 0)
            {
                aliasPath = TreePathUtils.GetParentPath(aliasPath);

                pi = GetPageInfo(site.SiteName, aliasPath, culture, null, combineWithDefaultCulture);
                if ((aliasPath == "/") || (pi == null))
                {
                    break;
                }
            }

            return pi;
        }


        #endregion


        #region "Internal methods for data retrieval"


        /// <summary>
        /// Returns default alias name for domain or site if is defined
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="siteName">Site name</param>
        public static string GetDefaultAliasPath(string domain, string siteName)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                return "/";
            }

            if (string.IsNullOrEmpty(domain))
            {
                // Get site settings
                return GetValidDefaultPath(null, siteName);
            }

            // Prepare candidates for domain check
            var plainDomain = URLHelper.RemoveWWW(domain);
            var probes = new List<Func<string>>
            {
                // Try to find default alias path in current domain alias
                () => domain,
                // Try to find default alias path in current domain alias with removed www
                () => plainDomain,
                // Try to find default alias path in current domain alias with removed port
                () => URLHelper.RemovePort(domain),
                // Try to find default alias path in current domain alias with removed www and port
                () => URLHelper.RemovePort(plainDomain)
            };

            // Get default path of the domain alias
            foreach (var probe in probes)
            {
                var path = GetSiteDomainAliasDefaultPath(site, probe());
                if (path != null)
                {
                    return GetValidDefaultPath(path, siteName);
                }
            }

            // Get default path from site settings
            return GetValidDefaultPath(null, siteName);
        }


        /// <summary>
        /// Gets validated default path
        /// </summary>
        /// <param name="path">Current path to validate</param>
        /// <param name="siteName">Site name</param>
        private static string GetValidDefaultPath(string path, string siteName)
        {
            // If path not set, get from settings
            if (String.IsNullOrEmpty(path))
            {
                path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaultAliasPath");
            }

            // Ensure default value
            if (String.IsNullOrEmpty(path))
            {
                path = "/";
            }

            return path;
        }


        /// <summary>
        /// Gets site domain alias default alias path for given site and domain
        /// </summary>
        /// <param name="site">Site</param>
        /// <param name="domain">Domain</param>
        private static string GetSiteDomainAliasDefaultPath(SiteInfo site, string domain)
        {
            var alias = (SiteDomainAliasInfo)site.SiteDomainAliases[domain];
            return alias != null ? alias.SiteDomainDefaultAliasPath : null;
        }


        /// <summary>
        /// Gets the page info query.
        /// </summary>
        internal static MultiDocumentQuery GetPageInfos()
        {
            var tree = new TreeProvider();
            return tree.SelectNodes()
                            .All()
                            .Columns(SelectColumnList);
        }
        #endregion
    }
}
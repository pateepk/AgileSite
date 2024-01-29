using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    internal static class PageInfoDataProvider
    {
        // Default path.
        private const string DEFAULT_PATH = "/default.aspx";


        /// <summary>
        /// Returns the data for the specified page info.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Document alias path</param>
        /// <param name="urlPath">Document URL path</param>
        /// <param name="nodeId">Node ID of the page (when provided, retrieval from the database is faster, use to get parent document)</param>
        /// <param name="cultureCode">Preferred culture code to be used to get the best matching candidate. 
        /// When the URL path is provided and there is a match by the URL path, the culture code of the matched candidate has higher priority than the given culture code and is used for the result.</param>
        /// <param name="combineWithDefaultCulture">Indicates if results should be combined with default culture</param>
        public static PageInfo GetDataForPageInfo(string siteName, string aliasPath, string urlPath, int nodeId, string cultureCode, bool combineWithDefaultCulture)
        {
            // Get the site
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                return null;
            }

            var siteId = site.SiteID;
            var defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);
            var priorityColumn = GetPriorityColumn(urlPath, nodeId, cultureCode, combineWithDefaultCulture, defaultCulture);
            var where = GetDataWhereCondition(siteId, aliasPath, urlPath, nodeId, cultureCode, combineWithDefaultCulture, defaultCulture);

            // Get the best candidate
            var query = PageInfoProvider.GetPageInfos()
                .Where(where)
                .AsNested<DataQuery>()
                .TopN(1);

            // Sort the candidates based on priority
            if (priorityColumn != null)
            {
                query.AddColumn(priorityColumn).OrderByDescending(priorityColumn.Name);
            }

            // Get the candidate
            var data = query.Result;

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                var pageInfo = new PageInfo(data.Tables[0].Rows[0]);

                // Return null if Page is ContentOnly
                return pageInfo.NodeIsContentOnly ? null : pageInfo;
            }

            // There is not best candidate, use blank page info as a result
            return GetBlankPageInfo(siteId, aliasPath, cultureCode, nodeId);
        }


        public static PageInfo GetDataForPageInfoForUrl(string siteName, string url, string cultureCode, string defaultAliasPath, bool combineWithDefaultCulture, bool isDocumentPage, out PageInfoResult pageResult)
        {
            pageResult = new PageInfoResult();

            if (String.IsNullOrEmpty(url))
            {
                return null;
            }

            // Parse the URL to domain and path
            string path;
            string domain;

            ParseUrl(url, out path, out domain);

            if (!String.IsNullOrEmpty(siteName))
            {
                ValidateSiteName(siteName);
            }
            else
            {
                siteName = GetSiteNameByDomain(domain);
            }

            // Get page info for URL using the getdoc prefix
            if (PathIsPermanent(ref path))
            {
                return PageInfoByPermanentPathSearcher.Search(siteName, path, cultureCode, combineWithDefaultCulture);
            }

            PathPrefixRemoval.RemovePathPrefix(siteName, ref path);

            PathPrefixRemoval.RemoveCulturePrefix(siteName, ref path);

            if (IsDefaultPath(path))
            {
                path = defaultAliasPath ?? PageInfoProvider.GetDefaultAliasPath(domain, siteName);
                return GetPageInfoForDefaultPath(siteName, path, cultureCode, combineWithDefaultCulture, out pageResult);
            }

            // Remove file extension from the path if exists
            var currentExtension = ExtractExtension(ref path);

            var parameters = new PageInfoSearchParameters(siteName, path, currentExtension, cultureCode, combineWithDefaultCulture, isDocumentPage);
            // Get page info by path
            var resultCandidate = PageInfoByPathSearcher.Search(parameters, out pageResult);
            if (resultCandidate != null)
            {
                return resultCandidate;
            }

            // Get page info based on wildcard rule
            resultCandidate = PageInfoByWildcardSearcher.Search(parameters, out pageResult);
            if (resultCandidate != null)
            {
                return resultCandidate;
            }

            // Get page info based on alias
            resultCandidate = PageInfoByDocumentAliasesSearcher.Search(parameters, out pageResult);

            return resultCandidate;
        }


        /// <summary>
        /// Gets page info for default path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="defaultPath">Default path</param>
        /// <param name="cultureCode">Current culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates if the result should be combined with default site culture</param>
        /// <param name="result">Page result to be modified</param>
        internal static PageInfo GetPageInfoForDefaultPath(string siteName, string defaultPath, string cultureCode, bool combineWithDefaultCulture, out PageInfoResult result)
        {
            result = new PageInfoResult();
            var pageInfo = PageInfoProvider.GetPageInfo(siteName, defaultPath, cultureCode, null, combineWithDefaultCulture);
            if (pageInfo == null)
            {
                return null;
            }

            result.PageSource = PageInfoSource.DefaultAliasPath;

            return pageInfo;
        }


        private static string GetSiteNameByDomain(string domain)
        {
            var siteInfo = SiteInfoProvider.GetRunningSiteInfo(domain, SystemContext.ApplicationPath);
            if (siteInfo == null)
            {
                throw new DomainNotFoundException("Domain '" + domain + "' not found.'");
            }

            return siteInfo.SiteName;
        }


        private static void ValidateSiteName(string siteName)
        {
            var siteInfo = SiteInfoProvider.GetSiteInfo(siteName);
            if (siteInfo == null)
            {
                throw new DomainNotFoundException("Site '" + siteName + "' not found.'");
            }
        }


        /// <summary>
        /// Parses the given URL to the path and domain part
        /// </summary>
        /// <param name="url">URL to parse</param>
        /// <param name="path">Outputs relative path</param>
        /// <param name="domain">Outputs domain name</param>
        private static void ParseUrl(string url, out string path, out string domain)
        {
            // Nothing to parse
            if (url == null)
            {
                path = String.Empty;
                domain = String.Empty;
                return;
            }

            // Decode URL
            var decodedUrl = HttpUtility.UrlDecode(url);
            if (decodedUrl == null)
            {
                path = String.Empty;
                domain = String.Empty;
                return;
            }

            var applicationPath = SystemContext.ApplicationPath;

            // Remove application path
            int index = decodedUrl.IndexOf(applicationPath, StringComparison.InvariantCultureIgnoreCase);

            if (decodedUrl.Contains("://"))
            {
                index = decodedUrl.Replace("://", "").IndexOf(applicationPath, StringComparison.InvariantCultureIgnoreCase);
                if (index > -1)
                {
                    index = index + 3;
                }
            }

            path = DEFAULT_PATH;
            domain = decodedUrl;

            if (index > -1)
            {
                path = url.Substring(index + applicationPath.Length);
                domain = url.Substring(0, index);
            }

            if (applicationPath.Length == 1)
            {
                string mDomain = "";
                int mLen = 7;

                // Remove http:// protocol
                if (decodedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    mDomain = decodedUrl.Remove(0, mLen);
                }

                // Remove https:// protocol
                if (decodedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    mLen = 8;
                    mDomain = decodedUrl.Remove(0, mLen);
                }

                if (index > -1)
                {
                    // Extract domain
                    path = url.Substring(mDomain.IndexOf("/", StringComparison.Ordinal) + mLen + applicationPath.Length - 1);
                    mDomain = mDomain.Substring(0, mDomain.IndexOf("/", StringComparison.Ordinal));
                }

                domain = mDomain;
            }

            // Set default path if current path is empty. e.g. http://domain
            if (String.IsNullOrEmpty(path))
            {
                path = DEFAULT_PATH;
            }

            path = URLHelper.RemoveQuery(path);

            // Remove the protocol from the domain
            domain = URLHelper.RemoveProtocol(domain);
        }


        /// <summary>
        /// Checks if the path starts with permanent document URL (/getdoc) prefix.
        /// </summary>
        /// <param name="path">Path to check.</param>
        private static bool PathIsPermanent(ref string path)
        {
            return URLHelper.CheckPrefixes(ref path, PageInfoProvider.GetDocPrefixes, true);
        }


        private static string ExtractExtension(ref string path)
        {
            var trimmedPath = path.TrimEnd('/');
            var currentExtension = Path.GetExtension(trimmedPath);
            if (!String.IsNullOrEmpty(currentExtension))
            {
                path = URLHelper.RemoveExtension(trimmedPath);
            }
            return currentExtension;
        }


        private static bool IsDefaultPath(string path)
        {
            return String.IsNullOrEmpty(path) || path.Equals(DEFAULT_PATH, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets column to prioritize the candidates
        /// </summary>
        /// <param name="urlPath">Document URL path</param>
        /// <param name="nodeId">Node ID</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture?</param>
        /// <param name="defaultCulture">Default site culture code</param>
        internal static QueryColumn GetPriorityColumn(string urlPath, int nodeId, string cultureCode, bool combineWithDefaultCulture, string defaultCulture)
        {
            var parts = new List<string>();

            // Include the priority based on culture match if culture provided
            if (!String.IsNullOrEmpty(cultureCode))
            {
                parts.Add(SqlHelper.GetCase(new[] { new KeyValuePair<string, string>(new WhereCondition("DocumentCulture", QueryOperator.Equals, cultureCode).ToString(true), "2") }, "0"));
            }

            // Priority based on default culture
            if (combineWithDefaultCulture && !String.IsNullOrEmpty(defaultCulture) && !String.Equals(cultureCode, defaultCulture, StringComparison.InvariantCultureIgnoreCase))
            {
                parts.Add(SqlHelper.GetCase(new[] { new KeyValuePair<string, string>(new WhereCondition("DocumentCulture", QueryOperator.Equals, defaultCulture).ToString(true), "1") }, "0"));
            }

            // When node ID is specified, the paths are not taken into account, node ID is an identifier for single node so links are not part of the candidates set
            if (nodeId == 0)
            {
                // When URL path defined, include the priority based on path match - this is a best fit by default.
                // There is a web.config key to ensure, that the URL path match is not the best fit and the best fit is driven by culture match
                if (!string.IsNullOrEmpty(urlPath) && !PageInfoProvider.UseCultureForBestPageInfoResult)
                {
                    parts.Add(SqlHelper.GetCase(new[] { new KeyValuePair<string, string>(new WhereCondition().WhereEquals("DocumentUrlPath", urlPath).ToString(true), "4") }, "0"));
                }

                // Include priority for original documents before links
                parts.Add(SqlHelper.GetCase(new[] { new KeyValuePair<string, string>("[NodeLinkedNodeID] IS NULL", "8") }, "0"));
            }

            // There is no data to build the priority
            if (parts.Count == 0)
            {
                return null;
            }

            // The row number is based on the sum of all partial priorities
            var priority = parts.Join(" + ");
            var priorityRowNumber = new QueryColumn(priority).As("CMS_P");

            return priorityRowNumber;
        }


        /// <summary>
        /// Gets blank page info
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="nodeId">Node ID</param>
        private static PageInfo GetBlankPageInfo(int siteId, string aliasPath, string cultureCode, int nodeId)
        {
            DocumentNodeDataInfo nodeData;

            if (nodeId > 0)
            {
                nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            }
            else
            {
                nodeData = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                       .Where(GetSiteWhereCondition(siteId))
                                                       .Where(GetNodeAliasPathWhereCondition(aliasPath))
                                                       .TopN(1)
                                                       .FirstOrDefault();
            }

            // Check the data
            if (nodeData == null)
            {
                return null;
            }

            // Blank page info in requested culture
            var blank = new PageInfo(nodeData)
            {
                DocumentCulture = cultureCode
            };

            return blank;
        }


        /// <summary>
        /// Returns the data for the specified page info.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="aliasPath">Document alias path</param>
        /// <param name="urlPath">Document URL path</param>
        /// <param name="nodeId">Node ID of the page (when provided, retrieval from the database is faster, use to get parent document)</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture?</param>
        /// <param name="defaultCulture">Default site culture code</param>
        internal static WhereCondition GetDataWhereCondition(int siteId, string aliasPath, string urlPath, int nodeId, string cultureCode, bool combineWithDefaultCulture, string defaultCulture)
        {
            if (string.IsNullOrEmpty(aliasPath))
            {
                throw new ArgumentException("[PageInfoProvider.GetDataWhereCondition]: The node alias path needs to be provided.");
            }

            var where = new WhereCondition();

            // Prepare site condition
            var siteWhere = GetSiteWhereCondition(siteId);

            // Prepare culture condition
            var cultureWhere = GetCultureWhereCondition(cultureCode, combineWithDefaultCulture, defaultCulture);

            if (nodeId > 0)
            {
                where.Where(cultureWhere).WhereEquals("NodeID", nodeId);
            }
            else
            {
                var pathWhere = new WhereCondition();

                // Based on document URL path
                if (!string.IsNullOrEmpty(urlPath))
                {
                    pathWhere.Where(GetUrlPathWhereCondition(urlPath));
                }

                // Based on node alias path
                pathWhere.Or().Where(GetNodeAliasPathWhereCondition(aliasPath).Where(cultureWhere));

                where.Where(siteWhere).Where(pathWhere);
            }

            return where;
        }


        /// <summary>
        /// Creates culture where condition if culture code is specified.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture flag</param>
        /// <param name="defaultCultureCode">Default culture code</param>
        private static WhereCondition GetCultureWhereCondition(string cultureCode, bool combineWithDefaultCulture, string defaultCultureCode)
        {
            var where = new WhereCondition();

            // No special condition
            if (cultureCode == TreeProvider.ALL_CULTURES)
            {
                return where;
            }

            if (!String.IsNullOrEmpty(cultureCode))
            {
                where.WhereEquals("DocumentCulture", cultureCode);
            }

            if (combineWithDefaultCulture && !String.IsNullOrEmpty(defaultCultureCode) && !String.Equals(cultureCode, defaultCultureCode, StringComparison.InvariantCultureIgnoreCase))
            {
                where.Or().WhereEquals("DocumentCulture", defaultCultureCode);
            }

            return where;
        }


        /// <summary>
        /// Creates where condition for the node alias path
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        private static WhereCondition GetNodeAliasPathWhereCondition(string aliasPath)
        {
            return new WhereCondition().WhereEquals("NodeAliasPath", aliasPath);
        }


        /// <summary>
        /// Creates where condition for the document URL path
        /// </summary>
        /// <param name="urlPath">Document URL path</param>
        private static WhereCondition GetUrlPathWhereCondition(string urlPath)
        {
            // Do not include pages with same node alias and document URL path. If there was a culture version with document URL path same as the node alias path,
            // this culture version would always win as a best match and the culture couldn't be switched for such page.
            return new WhereCondition().WhereEquals("DocumentUrlPath", urlPath).Where("[DocumentUrlPath] <> [NodeAliasPath]");
        }


        /// <summary>
        /// Creates site where condition
        /// </summary>
        /// <param name="siteId">Site ID</param>
        private static WhereCondition GetSiteWhereCondition(int siteId)
        {
            return new WhereCondition().WhereEquals("NodeSiteID", siteId);
        }
    }
}
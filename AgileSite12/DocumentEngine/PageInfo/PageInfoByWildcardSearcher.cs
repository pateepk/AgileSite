using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Searches <see cref="PageInfo"/> based on wildcard rule in DocumentUrlPath.
    /// </summary>
    internal static class PageInfoByWildcardSearcher
    {
        /// <summary>
        /// Gets <see cref="PageInfo"/> based on document URL path containing wildcard.
        /// </summary>
        public static PageInfo Search(PageInfoSearchParameters parameters, out PageInfoResult result)
        {
            result = new PageInfoResult();
            var path = parameters.RelativeUrlPath;

            // Try to find document by wildcard URL
            var documentSimpleData = FindCandidateByWildcard(path, parameters.SiteName, parameters.CultureCode, parameters.CombineWithDefaultCulture);

            // Try find document by original path with culture prefix
            if (documentSimpleData == null && parameters.CheckUrlPathWithLanguagePrefix)
            {
                documentSimpleData = FindCandidateByWildcard(parameters.RelativeUrlPathWithLanguagePrefix, parameters.SiteName, parameters.CultureCode, parameters.CombineWithDefaultCulture);
                path = parameters.RelativeUrlPathWithLanguagePrefix;
            }

            // No candidates found
            if (documentSimpleData == null)
            {
                return null;
            }

            var aliasPath = ValidationHelper.GetString(documentSimpleData.GetValue("NodeAliasPath"), String.Empty);
            var culture = ValidationHelper.GetString(documentSimpleData.GetValue("DocumentCulture"), String.Empty);
            var urlPath = ValidationHelper.GetString(documentSimpleData.GetValue("DocumentUrlPath"), String.Empty);
            var extensions = ValidationHelper.GetString(documentSimpleData.GetValue("DocumentExtensions"), String.Empty);

            // Validate extension
            if (!parameters.UrlIsConfirmedAsPage && !ExtensionMatcher.IsExtensionInList(extensions, parameters.UrlExtension))
            {
                return null;
            }

            // Try to get page info based on wildcard
            var pageInfo = PageInfoProvider.GetPageInfo(parameters.SiteName, aliasPath, culture, urlPath, parameters.CombineWithDefaultCulture);
            if (pageInfo == null)
            {
                return null;
            }

            // Validate wildcard rule
            var currentWildcardQuery = DocumentURLProvider.BuildWildcardQueryString(urlPath, path);
            if (currentWildcardQuery == null)
            {
                return null;
            }

            result = PreparePageResult(currentWildcardQuery);

            return pageInfo;
        }


        private static PageInfoResult PreparePageResult(string currentWildcardQuery)
        {
            return new PageInfoResult
            {
                WildcardQueryString = currentWildcardQuery,
                PageSource = PageInfoSource.UrlPath
            };
        }


        private static ISimpleDataContainer FindCandidateByWildcard(string path, string siteName, string currentCultureCode, bool combineWithDefaultCulture)
        {
            var culturePriority = GetCulturePriority(siteName, currentCultureCode, combineWithDefaultCulture);
            var wildcardWhereCondition = GetNodeWildcardWhereCondition(path, siteName);

            // All candidates that satisfy the where condition are ordered as follows:
            //  - By culture priority column
            //      - If parameter  combineWithDefaultCulture is true priorities are: current culture, default culture, others
            //      - Otherwise default culture is not prioritized, thus priorities are: current culture, others
            //  - By DocumentPriority column (documents with the most specific DocumentUrlPath first)
            //  - By count of sections in URL path (documents with more sections are more specific)
            //  - Alphabetically by culture code (it ensures that order does not depend on insertion order into DB)
            // The first row is the best candidate
            var provider = new TreeProvider();
            var query = provider.SelectNodes()
                                .All()
                                .Columns("NodeAliasPath", "DocumentCulture", "DocumentURLPath", "DocumentExtensions")
                                .Where(wildcardWhereCondition)
                                .OrderByDescending(culturePriority)
                                .OrderByDescending("DocumentPriority")
                                .OrderByDescending(DocumentQueryColumnBuilder.DocumentURLSectionsCountPriority)
                                .OrderByAscending("DocumentCulture")
                                .TopN(1);

            var wildcard = query.Result;
            return DataHelper.DataSourceIsEmpty(wildcard) ? null : new DataRowContainer(wildcard.Tables[0].Rows[0]);
        }


        private static string GetCulturePriority(string siteName, string currentCultureCode, bool combineWithDefaultCulture)
        {
            var defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

            return DocumentQueryColumnBuilder.GetCulturePriorityColumn("DocumentCulture", currentCultureCode, defaultCulture, combineWithDefaultCulture).GetExpression();
        }


        private static WhereCondition GetNodeWildcardWhereCondition(string path, string siteName)
        {
            return new WhereCondition()
                .Where(path.AsValue(), QueryOperator.Like, "DocumentWildcardRule".AsColumn())
                .WhereEquals("NodeSiteID", SiteInfoProvider.GetSiteID(siteName));
        }
    }
}

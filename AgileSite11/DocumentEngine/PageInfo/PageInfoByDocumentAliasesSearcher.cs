using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Searches <see cref="PageInfo"/> based on document aliases.
    /// </summary>
    internal static class PageInfoByDocumentAliasesSearcher
    {
        /// <summary>
        /// Gets <see cref="PageInfo"/> based on document aliases.
        /// </summary>
        public static PageInfo Search(PageInfoSearchParameters parameters, out PageInfoResult result)
        {
            result = new PageInfoResult();
            var path = parameters.RelativeUrlPath;

            var alias = DocumentAliasInfoProvider.GetDocumentAliasInternal(path, parameters.SiteName, parameters.CultureCode, parameters.CombineWithDefaultCulture);

            // Try find document by original path with culture prefix
            if (alias == null && parameters.CheckUrlPathWithLanguagePrefix)
            {
                alias = DocumentAliasInfoProvider.GetDocumentAliasInternal(parameters.RelativeUrlPathWithLanguagePrefix, parameters.SiteName, parameters.CultureCode, parameters.CombineWithDefaultCulture);
                path = parameters.RelativeUrlPathWithLanguagePrefix;
            }

            if (alias == null)
            {
                return null;
            }

            var aliasPath = TreePathUtils.GetAliasPathByNodeId(alias.AliasNodeID);

            // Try to get page info for alias
            var pageInfo = GetPageInfoByDocumentAliasInfo(parameters.SiteName, aliasPath, alias.AliasCulture, parameters.CultureCode, parameters.CombineWithDefaultCulture);
            if (pageInfo == null)
            {
                return null;
            }

            // Validate alias extensions
            if (!parameters.UrlIsConfirmedAsPage && !ExtensionMatcher.IsExtensionInList(alias.AliasExtensions + ";" + pageInfo.DocumentExtensions, parameters.UrlExtension))
            {
                return null;
            }

            // Validate wildcard rule - for alias with wildcard must create wildcard query string
            var wildcardQueryString = DocumentURLProvider.BuildWildcardQueryString(alias.AliasURLPath, path);
            if (DocumentURLProvider.ContainsWildcard(alias.AliasURLPath) && string.IsNullOrEmpty(wildcardQueryString))
            {
                return null;
            }

            result = PreparePageResult(wildcardQueryString, alias);

            return pageInfo;
        }


        private static PageInfoResult PreparePageResult(string wildcardQueryString, DocumentAliasInfo alias)
        {
            return new PageInfoResult
            {
                WildcardQueryString = wildcardQueryString,
                DocumentAliasCulture = alias.AliasCulture,
                DocumentAliasUrlPath = alias.AliasURLPath,
                DocumentAliasActionMode = GetDocumentAliasActionMode(alias),
                PageSource = PageInfoSource.DocumentAlias
            };
        }


        private static PageInfo GetPageInfoByDocumentAliasInfo(string siteName, string aliasPath, string aliasCulture, string cultureCode, bool combineWithDefaultCulture)
        {
            aliasCulture = string.IsNullOrEmpty(aliasCulture) ? TreeProvider.ALL_CULTURES : aliasCulture;

            PageInfo pi = null;
            if (aliasCulture == TreeProvider.ALL_CULTURES)
            {
                // Try to get info for requested culture
                pi = PageInfoProvider.GetPageInfo(siteName, aliasPath, cultureCode, null, combineWithDefaultCulture);
            }

            if (pi == null)
            {
                pi = PageInfoProvider.GetPageInfo(siteName, aliasPath, aliasCulture, null, combineWithDefaultCulture);
            }

            if (pi == null)
            {
                return null;
            }

            // Empty page info is not a valid result
            if (pi.DocumentID <= 0)
            {
                return null;
            }

            // Change original page source which is URL path for the aliases
            pi.PageResult.PageSource = PageInfoSource.UrlPath;

            return pi;
        }


        private static AliasActionModeEnum GetDocumentAliasActionMode(ISimpleDataContainer alias)
        {
            var aliasAction = ValidationHelper.GetString(alias.GetValue("AliasActionMode"), String.Empty);
            var action = aliasAction.ToEnum<AliasActionModeEnum>();
            return action;
        }
    }
}

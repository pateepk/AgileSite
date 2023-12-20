using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Searches <see cref="PageInfo"/> based on DocumentURLPath or NodeAliasPath document properties.
    /// </summary>
    internal static class PageInfoByPathSearcher
    {
        /// <summary>
        /// Gets <see cref="PageInfo"/> based on DocumentURLPath or NodeAliasPath properties.
        /// </summary>
        public static PageInfo Search(PageInfoSearchParameters parameters, out PageInfoResult result)
        {
            result = new PageInfoResult();

            // Get the page info by the given path
            var pageInfo = PageInfoProvider.GetPageInfo(parameters.SiteName, parameters.RelativeUrlPath, parameters.CultureCode, parameters.RelativeUrlPath, parameters.CombineWithDefaultCulture);
            if (pageInfo != null)
            {
                // Validate extension
                if (!ValidateExtension(parameters.UrlIsConfirmedAsPage, pageInfo, parameters.UrlExtension, parameters.SiteName))
                {
                    return null;
                }
            }
            // Try get page info for culture prefix if not found by the original path
            else if (parameters.CheckUrlPathWithLanguagePrefix)
            {
                pageInfo = PageInfoProvider.GetPageInfo(parameters.SiteName, parameters.RelativeUrlPathWithLanguagePrefix, parameters.CultureCode, parameters.RelativeUrlPathWithLanguagePrefix, parameters.CombineWithDefaultCulture);
                if (pageInfo != null)
                {
                    // Validate extension
                    if (!ValidateExtension(parameters.UrlIsConfirmedAsPage, pageInfo, parameters.UrlExtension, parameters.SiteName))
                    {
                        return null;
                    }
                }
            }

            if (pageInfo != null)
            {
                result.PageSource = GetPageSource(parameters.RelativeUrlPath, pageInfo);
            }

            return pageInfo;
        }


        private static bool ValidateExtension(bool urlIsConfirmedAsPage, PageInfo result, string extension, string siteName)
        {
            // Get files extension from site settings
            var filesExtension = TreePathUtils.GetFilesUrlExtension(siteName);

            return urlIsConfirmedAsPage || ExtensionMatcher.IsExtensionInList(result.DocumentExtensions, extension) ||
                   (string.Equals(result.ClassName, SystemDocumentTypes.File, StringComparison.InvariantCultureIgnoreCase) && string.Equals(extension, filesExtension, StringComparison.InvariantCultureIgnoreCase));
        }


        private static PageInfoSource GetPageSource(string path, PageInfo result)
        {
            var ulrPathMatch = string.Equals(path, result.DocumentUrlPath, StringComparison.InvariantCultureIgnoreCase);
            var differentPaths = !string.Equals(result.NodeAliasPath, result.DocumentUrlPath, StringComparison.InvariantCultureIgnoreCase);

            if (ulrPathMatch && (differentPaths || !PageInfoProvider.UseCultureForBestPageInfoResult))
            {
                return PageInfoSource.UrlPath;
            }

            return PageInfoSource.AliasPath;
        }
    }
}

using System;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.PortalEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Searches <see cref="PageInfo"/> based on permanent document path part (<see cref="PageInfoProvider.GetDocPrefixes"/>).
    /// </summary>
    internal static class PageInfoByPermanentPathSearcher
    {
        /// <summary>
        /// Gets the <see cref="PageInfo"/> based on permanent document path part.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="permanentPathPart">Part of the permanent docuemnt path in format <![CDATA[<node GUID>/<documentname>/<culture code><extension>]]>.</param>
        /// <param name="cultureCode">Culture code.</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture.</param>
        public static PageInfo Search(string siteName, string permanentPathPart, string cultureCode, bool combineWithDefaultCulture)
        {
            var urlParts = permanentPathPart.Split('/');

            // Get GUID from URL
            var nodeGuid = ValidationHelper.GetGuid(urlParts[0], Guid.Empty);

            // Load culture if culture part present
            PageInfoSource pageSource;

            if (urlParts.Length >= 3)
            {
                cultureCode = Path.GetFileNameWithoutExtension(urlParts[2]);
                
                pageSource = PageInfoSource.GetDocCulture;
            }
            else
            {
                pageSource = PageInfoSource.GetDoc;
            }

            // Get cached data required to get the page info
            var cacheMinutes = (PortalContext.ViewMode != ViewModeEnum.LiveSite) ? 0 : PageInfoCacheHelper.CacheMinutes(siteName);
            var culture = cultureCode;
            var data = CacheHelper.Cache(cs => GetDocumentData(siteName, culture, combineWithDefaultCulture, nodeGuid, cs), new CacheSettings(cacheMinutes, "getdoc", siteName, nodeGuid, CacheHelper.GetCultureCacheKey(cultureCode)));

            // Document not found
            if (data == null)
            {
                return null;
            }

            // Get the fields from the data
            var urlPath = data[0];
            var aliasPath = data[1];
            cultureCode = data[2];

            var result = PageInfoProvider.GetPageInfo(siteName, aliasPath, cultureCode, urlPath, combineWithDefaultCulture);
            if (result != null)
            {
                // Store the page source
                result.PageResult.PageSource = pageSource;
            }

            return result;
        }


        /// <summary>
        /// Gets limited set of columns for document data caching based on parameters.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="cultureCode">Document culture.</param>
        /// <param name="combineWithDefaultCulture">Indicates if result should be combined with site default culture.</param>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="cs">Cache settings.</param>
        private static string[] GetDocumentData(string siteName, string cultureCode, bool combineWithDefaultCulture, Guid nodeGuid, CacheSettings cs)
        {
            string[] parts = null;

            // Get the document
            var tree = new TreeProvider();
            var data = tree.SelectNodes()
                           .OnSite(siteName)
                           .Culture(cultureCode)
                           .CombineWithDefaultCulture(combineWithDefaultCulture)
                           .WhereEquals("NodeGUID", nodeGuid)
                           .Published(false)
                           .Columns("NodeID", "NodeAliasPath", "DocumentUrlPath", "DocumentCulture")
                           .Result;

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                var row = data.Tables[0].Rows[0];
                var documentUrlPath = row["DocumentUrlPath"].ToString(String.Empty);
                var nodeAliasPath = row["NodeAliasPath"].ToString(String.Empty);
                var documentCulture = row["DocumentCulture"].ToString(String.Empty);

                // Load the data
                parts = new string[3];
                parts[0] = documentUrlPath;
                parts[1] = nodeAliasPath;
                parts[2] = documentCulture;

                // Save to the cache
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { "nodeid|" + row["NodeID"] });
                }
            }
            else
            {
                // Do not cache
                cs.CacheMinutes = 0;
            }

            return parts;
        }
    }
}

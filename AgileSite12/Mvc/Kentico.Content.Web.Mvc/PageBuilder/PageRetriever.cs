using System.Linq;

using CMS.DocumentEngine;
using CMS.Helpers;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Encapsulates retrieving a page.
    /// </summary>
    internal class PageRetriever : IPageRetriever
    {
        internal int CacheMinutes { get; set; } = 10;


        /// <summary>
        /// Retrieves page based on given parameters.
        /// </summary>
        /// <param name="pageIdentifier">Identifier of the page.</param>
        /// <param name="latest">Indicates if configuration should be loaded from latest version of a page if workflow is applied.</param>
        /// <remarks>The page result is cached if latest version not retrieved.</remarks>
        public TreeNode Retrieve(int pageIdentifier, bool latest)
        {
            if (pageIdentifier <= 0)
            {
                return null;
            }

            if (latest)
            {
                // Do not cache for latest version used for administration
                return GetPage(pageIdentifier, latest);
            }

            return GetCachedPage(pageIdentifier, latest);
        }


        private TreeNode GetCachedPage(int pageIdentifier, bool latest)
        {
            return CacheHelper.Cache(() => GetPage(pageIdentifier, latest), GetCacheSettings(pageIdentifier, latest));
        }


        private CacheSettings GetCacheSettings(int pageIdentifier, bool latest)
        {
            return new CacheSettings(CacheMinutes, "PageBuilder", "Page", pageIdentifier, latest)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { GetCacheItemName(pageIdentifier) })
            };
        }


        /// <summary>
        /// Get cache item name for dependency.
        /// </summary>
        /// <param name="pageIdentifier">Page identifier.</param>
        internal string GetCacheItemName(int pageIdentifier)
        {
            return CacheHelper.GetCacheItemName(null, "documentid", pageIdentifier);
        }


        /// <summary>
        /// Gets page based on parameters.
        /// </summary>
        /// <param name="pageIdentifier">Identifier of the page.</param>
        /// <param name="latest">Indicates if configuration should be loaded from latest version of a page if workflow is applied.</param>
        internal virtual TreeNode GetPage(int pageIdentifier, bool latest)
        {
            var className = TreePathUtils.GetClassNameByDocumentID(pageIdentifier);

            return new DocumentQuery(className)
                .WithID(pageIdentifier)
                .LatestVersion(latest)
                .Published(!latest)
                .FirstOrDefault();
        }
    }
}
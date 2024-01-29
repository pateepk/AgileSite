using System;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides helper methods for caching of PageInfo.
    /// </summary>
    public static class PageInfoCacheHelper
    {
        /// <summary>
        /// Page info caching key.
        /// </summary>
        internal const string CACHE_KEY = "pageinfo";


        /// <summary>
        /// Cache key prefix of PageInfo obtained by URL. 
        /// </summary>
        internal const string CACHE_BYURL_KEY = "pageinfobyurl";


        /// <summary>
        /// Removes all page info records from the cache.
        /// </summary>
        /// <remarks>
        /// If <paramref name="siteName"/> is provided, system removes from the cache only instances of <see cref="PageInfo"/> which are related to this site.
        /// If <paramref name="siteName"/> is not provided, all cached instances of <see cref="PageInfo"/> are removed regardless of site. 
        /// </remarks>
        /// <param name="siteName">Site name</param>
        public static void ClearCache(string siteName = null)
        {
            CacheHelper.ClearCache(GetCacheKey(CACHE_KEY, siteName));
            CacheHelper.ClearCache(GetCacheKey(CACHE_BYURL_KEY, siteName));
        }
        

        private static string GetCacheKey(string baseKey, string siteName = null)
        {
            var key = baseKey + "|";
            if (!string.IsNullOrEmpty(siteName))
            {
                key += siteName + "|";
            }

            return  key;
        }


        /// <summary>
        /// Cache page info minutes.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int CacheMinutes(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSCachePageInfo"].ToInteger(0);
        }


        /// <summary>
        /// Returns true if the page info caching is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool CacheEnabled(string siteName)
        {
            return CacheMinutes(siteName) > 0;
        }


        /// <summary>
        /// Gets the dependency cache keys for the given page info.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        public static string[] GetDependencyCacheKeys(PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                return null;
            }

            // Cache item depends on changing the document and template
            string[] dependencies = new string[4];

            // Page template ID
            int pageTemplateId = pageInfo.GetUsedPageTemplateId();
            if (pageTemplateId > 0)
            {
                dependencies[0] = "template|" + pageTemplateId;
            }
            else
            {
                dependencies[0] = "template|" + pageInfo.UsedPageTemplateInfo.PageTemplateId;
            }

            // Node ID
            dependencies[1] = "nodeid|" + pageInfo.NodeID;

            if (pageInfo.NodeLinkedNodeID > 0)
            {
                dependencies[2] = "nodeid|" + pageInfo.NodeLinkedNodeID;
            }

            // Group ID
            int groupId = pageInfo.NodeGroupID;
            if (groupId > 0)
            {
                dependencies[3] = "community.group|byid|" + groupId;
            }

            return dependencies;
        }
    }
}

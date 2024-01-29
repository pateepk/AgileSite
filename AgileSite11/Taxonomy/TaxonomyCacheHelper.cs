using CMS.Helpers;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Provides helper methods for caching of tags.
    /// </summary>
    public static class TaxonomyCacheHelper
    {
        /// <summary>
        /// Gets the dependency cache keys for the given tag group.
        /// </summary>
        /// <param name="tagGroupId">ID of the tag group.</param>
        public static string GetTagGroupCacheDependencyKey(int tagGroupId)
        {
            return CacheHelper.GetCacheItemName(null, TagGroupInfo.OBJECT_TYPE, "byid", tagGroupId);
        }
    }
}

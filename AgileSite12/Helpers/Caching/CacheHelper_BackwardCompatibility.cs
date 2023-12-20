#if NETFULLFRAMEWORK

using System;
using System.Web.Caching;

using CMS.Base;

namespace CMS.Helpers
{
    public partial class CacheHelper
    {
        /// <summary>
        /// Cache item priority.
        /// </summary>
        [Obsolete("Use DefaultCacheItemPriority instead.")]
        public static CacheItemPriority CacheItemPriority
        {
            get
            {
                return ChangeCMSCacheItemPriorityType(DefaultCacheItemPriority);
            }
            set
            {
                DefaultCacheItemPriority = ChangeCacheItemPriorityType(value);
            }
        }


        /// <summary>
        /// Mirror to Cache.Add().
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Cache absolute expiration</param>
        /// <param name="slidingExpiration">Cache sliding expiration</param>
        /// <param name="priority">Cache priority</param>
        /// <param name="onCacheRemoveCallback">Cache callback on remove</param>
        /// <param name="caseSensitive">Cache key is case sensitive</param>
        [HideFromDebugContext]
        [Obsolete("Use Add with CMSCacheItemRemovedCallback and CMSCacheItemPriority parameters instead.")]
        public static void Add(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onCacheRemoveCallback = null, bool caseSensitive = false)
        {
            Add(key, value, dependencies, absoluteExpiration, slidingExpiration, ChangeCacheItemPriorityType(priority), ChangeItemRemovedCallBackType(onCacheRemoveCallback), caseSensitive);
        }


        private static CMSCacheItemRemovedCallback ChangeItemRemovedCallBackType(CacheItemRemovedCallback onRemoveCallback)
        {
            if (onRemoveCallback == null)
            {
                return null;
            }

            return (key, value, reason) =>
            {
                onRemoveCallback(key, value, ChangeReasonType(reason));
            };
        }


        private static  CacheItemPriority ChangeCMSCacheItemPriorityType(CMSCacheItemPriority priority)
        {
            switch (priority)
            {
                case CMSCacheItemPriority.High:
                    return CacheItemPriority.High;

                case CMSCacheItemPriority.Normal:
                    return CacheItemPriority.Normal;

                case CMSCacheItemPriority.NotRemovable:
                    return CacheItemPriority.NotRemovable;

                case CMSCacheItemPriority.Low:
                default:
                    return CacheItemPriority.Low;
            }
        }
      

        private static CMSCacheItemPriority ChangeCacheItemPriorityType(CacheItemPriority priority)
        {
            switch (priority)
            {
                case CacheItemPriority.High:
                case CacheItemPriority.AboveNormal:
                    return CMSCacheItemPriority.High;

                case CacheItemPriority.Normal:
                    return CMSCacheItemPriority.Normal;

                case CacheItemPriority.NotRemovable:
                    return CMSCacheItemPriority.NotRemovable;

                case CacheItemPriority.BelowNormal:
                case CacheItemPriority.Low:
                default:
                    return CMSCacheItemPriority.Low;
            }
        }


        private static CacheItemRemovedReason ChangeReasonType(CMSCacheItemRemovedReason reason)
        {
            switch (reason)
            {
                case CMSCacheItemRemovedReason.DependencyChanged:
                    return CacheItemRemovedReason.DependencyChanged;

                case CMSCacheItemRemovedReason.Expired:
                    return CacheItemRemovedReason.Expired;

                case CMSCacheItemRemovedReason.Underused:
                    return CacheItemRemovedReason.Underused;

                case CMSCacheItemRemovedReason.Removed:
                default:
                    return CacheItemRemovedReason.Removed;
            }
        }
    }
}

#endif

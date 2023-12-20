using System;
using System.Collections;
using System.Web;
using System.Web.Caching;

using CMS;
using CMS.AspNet.Platform;
using CMS.AspNet.Platform.Cache.Extension;
using CMS.Core;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;

[assembly: RegisterImplementation(typeof(ICacheAccessor), typeof(CacheAccessorImpl), Priority = RegistrationPriority.Fallback, Lifestyle = Lifestyle.Singleton)]

namespace CMS.AspNet.Platform
{
    internal sealed class CacheAccessorImpl : ICacheAccessor
    {
        /// <summary>
        /// Inserts an object into the cache.
        /// </summary>
        /// <param name="key">The cache key used to reference the object.</param>
        /// <param name="value">The object to be inserted in the cache.</param>
        /// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache.</param>
        /// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. </param>
        /// <param name="slidingExpiration">The interval between the time the inserted object was last accessed and the time at which that object expires.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache.</param>
        /// <param name="onRemoveCallback">A delegate that, if provided, will be called when an object is removed from the cache.</param>
        public void Insert(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CMSCacheItemPriority priority, CMSCacheItemRemovedCallback onRemoveCallback)
        {
            CacheDependency dep = null;
            if (dependencies != null)
            {
                dep = dependencies.CreateCacheDependency();
            }

            HttpRuntime.Cache.Insert(key, value, dep, absoluteExpiration, slidingExpiration, ChangeItemPriorityType(priority), ChangeItemRemovedCallBackType(onRemoveCallback));
        }


        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <param name="key">The identifier for the cache item to retrieve.</param>
        public object Get(string key)
        {
            return HttpRuntime.Cache[key];
        }


        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key">The identifier for the cache item to remove.</param>
        public object Remove(string key)
        {
            return HttpRuntime.Cache.Remove(key);
        }


        /// <summary>
        /// <summary>Retrieves a dictionary enumerator used to iterate through the key settings and their values contained in the cache.</summary>
        /// </summary>
        public IDictionaryEnumerator GetEnumerator()
        {
            return HttpRuntime.Cache.GetEnumerator();
        }


        private static CacheItemRemovedCallback ChangeItemRemovedCallBackType(CMSCacheItemRemovedCallback onRemoveCallback)
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


        private static CMSCacheItemRemovedReason ChangeReasonType(CacheItemRemovedReason reason)
        {
            switch(reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    return CMSCacheItemRemovedReason.DependencyChanged;

                case CacheItemRemovedReason.Expired:
                    return CMSCacheItemRemovedReason.Expired;

                case CacheItemRemovedReason.Underused:
                    return CMSCacheItemRemovedReason.Underused;

                case CacheItemRemovedReason.Removed:
                default:
                    return CMSCacheItemRemovedReason.Removed;
            }
        }


        private static CacheItemPriority ChangeItemPriorityType(CMSCacheItemPriority priority)
        {
            switch (priority)
            {
                case CMSCacheItemPriority.High:
                    return CacheItemPriority.High;

                case CMSCacheItemPriority.Low:
                    return CacheItemPriority.Low;

                case CMSCacheItemPriority.NotRemovable:
                    return CacheItemPriority.NotRemovable;

                case CMSCacheItemPriority.Normal:
                default:
                    return CacheItemPriority.Normal;
            }
        }
    }
}

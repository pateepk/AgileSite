using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// This class uses cache to control timeouts of other cached items.
    /// 
    /// It does not store any data itself.
    /// 
    /// It allows to invalidate caches across web farms (that's the reason why cache is used here - touching cache is propagated across web farms) or only locally.
    /// </summary>
    public class ChatCacheBeacon
    {
        #region "Private fields"

        private string key;
        // Cache with this key is invalidated. 'Real' cached object is dependent on this dummy cache item, so it will be flushed.
        private string dummyKey;
        private object sync = new object();
        private TimeSpan persistence;
        private bool locallyInvalidated = false;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs cache beacon.
        /// 
        /// First call to IsValid method will return false.
        /// </summary>
        /// <param name="key">Globally unique key</param>
        /// <param name="persistence">Persistence - method IsValid() will return false after this amount of time passes</param>
        public ChatCacheBeacon(string key, TimeSpan persistence)
        {
            this.key = key;
            this.persistence = persistence;

            dummyKey = key + "|(dummy)";
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if beacon is valid. It takes into consideration local and cache invalidation.
        /// 
        /// If beacon is not valid, it calls Reset itself, so next call will return true again.
        /// 
        /// First call to this method will return false.
        /// 
        /// Method is thread safe.
        /// </summary>
        /// <returns>True if timeout has not passed and nobody has invalidated this beacon</returns>
        public bool IsValid()
        {
            object val;

            if (!locallyInvalidated && CacheHelper.TryGetItem(key, out val))
            {
                return true;
            }

            lock (sync)
            {
                // Check one more time inside lock
                if (!locallyInvalidated && CacheHelper.TryGetItem(key, out val))
                {
                    return true;
                }
                Reset();

                return false;
            }
        }


        /// <summary>
        /// Resets timeout, so next time IsValid() will return false again after amount of time specified by persistence argument
        /// </summary>
        public void Reset()
        {
            CacheHelper.Add(key, new object(), CacheHelper.GetCacheDependency(dummyKey), DateTime.Now.Add(persistence), Cache.NoSlidingExpiration);

            locallyInvalidated = false;
        }


        /// <summary>
        /// Invalidates beacon - next call of IsValid will return false.
        /// 
        /// This invalidation works across web farms.
        /// </summary>
        public void Invalidate()
        {
            CacheHelper.TouchKey(dummyKey);
        }


        /// <summary>
        /// Invalidates beacon - next call of IsValid will return false.
        /// 
        /// This invalidation works only locally.
        /// </summary>
        public void InvalidateLocally()
        {
            locallyInvalidated = true;
        }

        #endregion
    }
}

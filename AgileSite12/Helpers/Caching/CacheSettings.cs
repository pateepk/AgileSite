using System;
using System.Collections;

namespace CMS.Helpers
{
    /// <summary>
    /// Cache settings container.
    /// </summary>
    public class CacheSettings
    {
        #region "Variables"
        
        private double mCacheMinutes;
        private bool mBoolCondition = true;
        
        // True if the cache should be used.
        private bool? mCached;

        private string mCacheItemName;
        private CMSCacheDependency mCacheDependency;

        #endregion


        #region "Properties"


        /// <summary>
        /// Cache condition.
        /// </summary>
        public bool BoolCondition
        {
            get
            {
                return mBoolCondition;
            }
            set
            {
                mBoolCondition = value;
                mCached = null;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        /// <remarks>Value is used as absolute or sliding expiration time based on <see cref="UseSlidingExpiration"/> value.</remarks>
        public double CacheMinutes
        {
            get
            {
                return mCacheMinutes;
            }
            set
            {
                mCacheMinutes = value;
                mCached = null;
            }
        }


        /// <summary>
        /// Indicates whether sliding expiration should be used. If enabled, objects are removed from the cache only if they are not accessed for the number of minutes specified by <see cref="CacheMinutes"/>.
        /// </summary>
        public bool UseSlidingExpiration
        {
            get;
        }


        /// <summary>
        /// Cache dependency to use for the cache item. When GetCacheDependency is set and this property is not set explicitly, the value is retrieved by calling the delegate in GetCacheDependency property.
        /// Use setter of this property only in the data loading code of the cached code block to avoid unnecessary initialization of the dependencies.
        /// Use delegate property GetCacheDependency to provide cache dependencies in a more efficient way only when the system really needs them.
        /// </summary>
        public CMSCacheDependency CacheDependency
        {
            get
            {
                return mCacheDependency ?? (mCacheDependency = (GetCacheDependency != null) ? GetCacheDependency() : null);
            }
            set
            {
                mCacheDependency = value;
            }
        }


        /// <summary>
        /// Cache item name used for the caching.
        /// </summary>
        public string CacheItemName
        {
            get
            {
                return mCacheItemName ?? (mCacheItemName = GetCacheItemName());
            }
        }


        /// <summary>
        /// If true, the data is used from the cache if available / cached.
        /// </summary>
        public bool Cached
        {
            get
            {
                if (mCached == null)
                {
                    mCached = (mBoolCondition && (mCacheMinutes > 0));
                }

                return mCached.Value;
            }
            set
            {
                mCached = value;
            }
        }


        /// <summary>
        /// Custom cache item name (if set, used instead of the parts).
        /// </summary>
        public string CustomCacheItemName { get; set; }


        /// <summary>
        /// Cache item name parts (form the cache item name if the custom item name is not available).
        /// </summary>
        public IEnumerable CacheItemNameParts { get; set; }


        /// <summary>
        /// If true, progressive caching is enabled, meaning that two threads accessing the same code share the result of an internal operation.
        /// </summary>
        public bool AllowProgressiveCaching { get; set; } = true;


        /// <summary>
        /// Function to dynamically get the cache dependency.
        /// </summary>
        public Func<CMSCacheDependency> GetCacheDependency { get; set; }


        /// <summary>
        /// Cache priority.
        /// </summary>
        public CMSCacheItemPriority CacheItemPriority { get; set; } = CacheHelper.DefaultCacheItemPriority;

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheSettings"/> with absolute expiration.
        /// </summary>
        /// <param name="cacheMinutes">Cache minutes.</param>
        /// <param name="cacheItemNameParts">Cache item name parts (form the cache item name if the custom item name is not available).</param>
        public CacheSettings(double cacheMinutes, params object[] cacheItemNameParts)
        {
            CacheMinutes = cacheMinutes;
            CacheItemNameParts = cacheItemNameParts;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CacheSettings"/>.
        /// </summary>
        /// <param name="cacheMinutes">Cache minutes.</param>
        /// <param name="useSlidingExpiration">Indicates whether sliding expiration should be used. If enabled, objects are removed from the cache only if they are not accessed for the number of minutes specified by <paramref name="cacheMinutes"/>.</param>
        /// <param name="cacheItemNameParts">Cache item name parts (form the cache item name if the custom item name is not available).</param>
        public CacheSettings(double cacheMinutes, bool useSlidingExpiration, params object[] cacheItemNameParts)
        {
            UseSlidingExpiration = useSlidingExpiration;
            CacheMinutes = cacheMinutes;
            CacheItemNameParts = cacheItemNameParts;
        }


        /// <summary>
        /// Gets the cache item name
        /// </summary>
        private string GetCacheItemName()
        {
            if (!String.IsNullOrEmpty(CustomCacheItemName))
            {
                return CustomCacheItemName;
            }

            return CacheHelper.BuildCacheItemName(CacheItemNameParts);
        }
        
        #endregion
    }
}
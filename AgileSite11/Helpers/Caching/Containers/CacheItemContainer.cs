using System;
using System.Web.Caching;

namespace CMS.Helpers
{
    /// <summary>
    /// Container for the cache item.
    /// </summary>
    public class CacheItemContainer
    {
        #region "Properties"

        /// <summary>
        /// Time when the item was created
        /// </summary>
        public DateTime Created
        {
            get;
            private set;
        }


        /// <summary>
        /// Data item.
        /// </summary>
        public object Data
        {
            get;
            private set;
        }


        /// <summary>
        /// Cache dependencies.
        /// </summary>
        public CMSCacheDependency Dependencies
        {
            get;
            private set;
        }


        /// <summary>
        /// Cache absolute expiration.
        /// </summary>
        public DateTime AbsoluteExpiration
        {
            get;
            private set;
        }


        /// <summary>
        /// Cache sliding expiration.
        /// </summary>
        public TimeSpan SlidingExpiration
        {
            get;
            private set;
        }


        /// <summary>
        /// Cache priority.
        /// </summary>
        public CacheItemPriority Priority
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Cache value</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Cache absolute expiration</param>
        /// <param name="slidingExpiration">Cache sliding expiration</param>
        /// <param name="priority">Cache priority</param>
        public CacheItemContainer(object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            Data = value;
            Dependencies = dependencies;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
            Priority = priority;

            Created = DateTime.Now;
        }

        #endregion
    }
}
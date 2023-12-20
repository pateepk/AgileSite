using System;
using System.Xml.Serialization;

using CMS.Helpers;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Class to cover the cache item with all its information
    /// </summary>
    [Serializable]
    public class PersistentCacheItem<DataType> : IPersistentCacheItem
    {
        #region "Properties"

        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Cache key
        /// </summary>
        public string CacheKey
        {
            get;
            set;
        }


        /// <summary>
        /// Cache minutes
        /// </summary>
        public int CacheMinutes
        {
            get;
            set;
        }


        /// <summary>
        /// Time when the cache item expires
        /// </summary>
        public DateTime Expires
        {
            get;
            set;
        }


        /// <summary>
        /// Time when the cache file expires
        /// </summary>
        public DateTime FileExpires
        {
            get;
            set;
        }


        /// <summary>
        /// Item data - Strongly typed
        /// </summary>
        public DataType Data
        {
            get;
            set;
        }


        /// <summary>
        /// Cache dependencies
        /// </summary>
        public CMSCacheDependency CacheDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Item value
        /// </summary>
        [XmlIgnore]
        public object Value
        {
            get
            {
                return Data;
            }
            set
            {
                Data = (DataType)value;
            }
        }


        /// <summary>
        /// Data item.
        /// </summary>
        [XmlIgnore]
        object ICacheItemContainer.Data => Data;

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCacheItem{DataType}"/> class.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use constructor with parameters instead.")]
        public PersistentCacheItem()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCacheItem{DataType}"/> class.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="data">Data to cache</param>
        /// <param name="cacheMinutes">Cache minutes for the standard cache item</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="fileExpires">Expiration time for the file</param>
        /// <param name="cd">Cache dependency</param>
        /// <param name="siteName">Site name</param>
        public PersistentCacheItem(string cacheKey, DataType data, CMSCacheDependency cd, int cacheMinutes, DateTime expires, DateTime fileExpires, string siteName)
        {
            CacheKey = cacheKey;
            Data = data;
            Expires = expires;
            FileExpires = fileExpires;
            CacheDependencies = cd;
            SiteName = siteName;
            CacheMinutes = cacheMinutes;
        }

        #endregion
    }
}
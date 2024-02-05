using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Web.Caching;
using System.Threading;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Function to return data for ChatParametrizedCacheWrapper. It takes IChatCacheableParam as parameter.
    /// </summary>
    /// <typeparam name="TData">Type of return data</typeparam>
    /// <typeparam name="TParam">Type of parameter</typeparam>
    /// <param name="param">Parameter</param>
    public delegate IEnumerable<TData> FetchDataFunc<TData, TParam>(TParam param) where TParam : IChatCacheableParam;


    /// <summary>
    /// This class represents parametrized cache. This is cache which takes params in form of IChatCacheableParam, makes
    /// hash code from this param (CacheKey) and if this key exists in cache, it returns cache. If key does not exists, 
    /// it uses function of type FetchDataFunc to get data and store them in cache.
    /// 
    /// Function FetchDataFunc returns IEnumerable. This means that this cache is useful only for sequences. 
    /// </summary>
    /// <typeparam name="TData">Type of cached data</typeparam>
    /// <typeparam name="TParam">Type of param</typeparam>
    public class ChatParametrizedCacheWrapper<TData, TParam> where TData : IChatIncrementalCacheable where TParam : IChatCacheableParam
    {
        #region "Nested classes"

        /// <summary>
        /// Internal representation of data in ChatParametrizedCacheWrapper.
        /// </summary>
        /// <typeparam name="TDataInner">Payload</typeparam>
        private class ParametrizedCacheWrapperData<TDataInner> : ICacheWrapperResponse<TDataInner>
        {
            /// <summary>
            /// Items.
            /// </summary>
            public ICollection<TDataInner> Items { get; set; }


            /// <summary>
            /// Last change of items.
            /// </summary>
            public DateTime LastChange { get; set; }
        }

        #endregion


        #region "Private fields"

        private string key;
        private FetchDataFunc<TData, TParam> fetchDataFunc;
        private TimeSpan maxDelay;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <param name="fetchDataFunc">Function to retrieve data</param>
        /// <param name="maxDelay">Cache persistence</param>
        public ChatParametrizedCacheWrapper(string key, FetchDataFunc<TData, TParam> fetchDataFunc, TimeSpan maxDelay)
        {
            this.key = key;
            this.fetchDataFunc = fetchDataFunc;
            this.maxDelay = maxDelay;
        }

        #endregion


        #region "Public methods"
        
        /// <summary>
        /// Gets data from cache (if item in cache with key made from <paramref name="param"/> exists) or from function FetchDataFunc.
        /// 
        /// Post cache filter is applied to data after taking it from cache.
        /// 
        /// Null is returned if nothing was found.
        /// </summary>
        /// <param name="param">Parameter - data will be stored to (and retrieved from) cache based on this parameter.</param>
        public ICacheWrapperResponse<TData> GetData(TParam param)
        {
            return GetData(param, null);
        }


        /// <summary>
        /// Gets data from cache (if item in cache with key made from <paramref name="param"/> exists) or from function FetchDataFunc.
        /// 
        /// Post cache filter is applied to data after taking it from cache.
        /// 
        /// Null is returned if nothing was found.
        /// </summary>
        /// <param name="param">Parameter - data will be stored to (and retrieved from) cache based on this parameter.</param>
        /// <param name="postCacheFilter">Post cache filter - filter which will be applied to data after retrieval from cache</param>
        public ICacheWrapperResponse<TData> GetData(TParam param, Func<TData, bool> postCacheFilter)
        {
            // Build key
            string cacheKey = key + "|" + param.CacheKey;

            // Prepare data for returning
            ParametrizedCacheWrapperData<TData> data = null;

            using (var cs = new CachedSection<ParametrizedCacheWrapperData<TData>>(ref data, maxDelay.TotalMinutes, true, cacheKey))
            {
                // Load data if cache is empty
                if (cs.LoadData)
                {
                    // Data were not found - get data from function
                    ICollection<TData> items = fetchDataFunc(param).ToList();

                    // Create IncrementalCacheWrapperResponse to put into cache
                    // LastChange is set to MinValue if nothing was found. Otherwise LastChange is computed.
                    data = new ParametrizedCacheWrapperData<TData>()
                    {
                        Items = items,
                        LastChange = (items.Count == 0) ? DateTime.MinValue : items.Max(d => d.ChangeTime),
                    };
                    cs.Data = data;
                }
            }
            
            // Return null if nothing was found
            if ((data == null) || (data.Items.Count == 0))
            {
                return null;
            }

            // Return unmodified data
            if (postCacheFilter == null)
            {
                return data;
            }

            // Apply post cache filter
            return new ParametrizedCacheWrapperData<TData>()
            {
                Items = data.Items.Where(item => postCacheFilter(item)).ToList(),
                LastChange = data.LastChange,
            };
        }

        #endregion
    }
}

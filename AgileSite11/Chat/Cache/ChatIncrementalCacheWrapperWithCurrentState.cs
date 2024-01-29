using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// This cache wrapper combines CurrentStateWrapper and ParametrizedCacheWrapper. Where parametrized cache wrapper takes
    /// only one argument - DateTime. This argument is automatically wrapped in IChatCacheableParam.
    /// </summary>
    /// <typeparam name="TData">Type of data</typeparam>
    /// <typeparam name="TPrimaryKey">Type of primary key</typeparam>
    public class ChatIncrementalCacheWithCurrentStateWrapper<TData, TPrimaryKey> where TData : IChatCacheableWithCurrentState<TPrimaryKey>
    {
        #region "Nested classes"

        /// <summary>
        /// Wraps one argument (DateTime) in class implementing IChatCacheableParam.
        /// </summary>
        private class SinceWhenCacheParam : IChatCacheableParam
        {
            /// <summary>
            /// Since when data should be loaded.
            /// </summary>
            public DateTime SinceWhen { get; set; }


            /// <summary>
            /// Cache key
            /// </summary>
            public string CacheKey
            {
                get
                {
                    return SinceWhen.Ticks.ToString();
                }
            }
        }

        #endregion


        #region "Private fields"

        private readonly ChatParametrizedCacheWrapper<TData, SinceWhenCacheParam> incrementalCacheWrapper;
        private readonly ChatCurrentStateCacheWrapper<TData, TPrimaryKey> currentStateCacheWrapper;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor. Inits inner ChatParametrizedCacheWrapper and ChatCurrentStateCacheWrapper
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <param name="fetchAllDataFunc">Function to get all data (current state)</param>
        /// <param name="fetchChangedDataFunc">Function to get changed data</param>
        /// <param name="maxDelay">Cache persistence</param>
        public ChatIncrementalCacheWithCurrentStateWrapper(string key, FetchAllDataFunc<TData> fetchAllDataFunc, FetchChangedDataFunc<TData> fetchChangedDataFunc, TimeSpan maxDelay)
        {
            incrementalCacheWrapper = new ChatParametrizedCacheWrapper<TData, SinceWhenCacheParam>(key + "|inc", (dateTimeParam) => fetchChangedDataFunc(dateTimeParam.SinceWhen), maxDelay);
            currentStateCacheWrapper = new ChatCurrentStateCacheWrapper<TData, TPrimaryKey>(key + "|cur", fetchAllDataFunc, fetchChangedDataFunc, maxDelay);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current state of items. It is not older than 'maxDelay'
        /// </summary>
        public Dictionary<TPrimaryKey, TData> CurrentState
        {
            get
            {
                return currentStateCacheWrapper.CurrentState;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets current state together with time of last change.
        /// </summary>
        public ICacheWrapperResponse<TData> GetCurrentStateWithLastChange()
        {
            return currentStateCacheWrapper.GetCurrentStateWithLastChange();
        }


        /// <summary>
        /// Tries to get item from CurrentState. If item wasn't found, current state is reloaded and then it tries again.
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="item">Output data</param>
        /// <returns>True if item was found</returns>
        public bool ForceGetItem(TPrimaryKey key, out TData item)
        {
            return currentStateCacheWrapper.ForceTryGetItem(key, out item);
        }


        /// <summary>
        /// Invalidates cache and then tries to get item.
        /// </summary>
        /// <param name="key">PK</param>
        /// <param name="item">Output data</param>
        /// <returns>True if item was found</returns>
        public bool UpdateAndTryGetItem(TPrimaryKey key, out TData item)
        {
            return currentStateCacheWrapper.UpdateAndTryGetItem(key, out item);
        }


        /// <summary>
        /// Gets data from cache (if item in cache with key made from <paramref name="changedSince"/> exists). Otherwise it gets items from fetchChangedDataFunc.
        /// 
        /// This function returns only data changed after <paramref name="changedSince"/>. 
        /// 
        /// Null is returned if nothing was found.
        /// </summary>
        /// <param name="changedSince">Data changed after this time will be returned.</param>
        public ICacheWrapperResponse<TData> GetLatestData(DateTime changedSince)
        {
            return incrementalCacheWrapper.GetData(new SinceWhenCacheParam() { SinceWhen = changedSince }, null);
        }


        /// <summary>
        /// Invalidates current state. Before next fetch of current state data (property CurrentState, method UpdateAndTryGetItem(), etc.)
        /// data will be reloaded from data source.
        /// </summary>
        public void InvalidateCurrentState()
        {
            currentStateCacheWrapper.Invalidate();
        }

        #endregion
    }
}
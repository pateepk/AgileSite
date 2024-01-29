using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Function which returns all data in CurrentStateCacheWrapper. This function is used to load initial data. Then 
    /// FetchChangedDataFunc is used to update current state.
    /// </summary>
    /// <typeparam name="TData">Type of data</typeparam>
    public delegate IEnumerable<TData> FetchAllDataFunc<TData>();


    /// <summary>
    /// This function returns changed data. 
    /// </summary>
    /// <typeparam name="TData">Type of data</typeparam>
    /// <param name="since">Returns data changed since this time</param>
    public delegate IEnumerable<TData> FetchChangedDataFunc<TData>(DateTime since);


    /// <summary>
    /// This cache wrapper holds Dictionary of items called 'current state', which is bascically an key-value collection of items. 
    /// Items in this collections can be modified by FetchChangedDataFunc.
    /// 
    /// It takes two functions:
    /// FetchAllDataFunc - this function is used to load initial current state
    /// FetchChangedDataFunc - this function is used to load changes to current state in time
    /// </summary>
    /// <typeparam name="TData">Type of cached items</typeparam>
    /// <typeparam name="TKey">Type of primary key of items</typeparam>
    public class ChatCurrentStateCacheWrapper<TData, TKey> where TData : IChatCacheableWithCurrentState<TKey>
    {
        #region "Nested classes"

        /// <summary>
        /// Internal representation of data stored in ChatCurrentStateCacheWrapper. It contains dictionary of TData indexed by TKey.
        /// </summary>
        /// <typeparam name="TKeyInner">Type of key in a dictionary</typeparam>
        /// <typeparam name="TDataInner">Type of item in a dictionary</typeparam>
        protected class CurrentStateCacheWrapperData<TKeyInner, TDataInner> : ICacheWrapperResponse<TDataInner>
        {
            #region "Private fields"

            private readonly Dictionary<TKeyInner, TDataInner> dictionary = new Dictionary<TKeyInner, TDataInner>();
            private DateTime? lastChange;

            #endregion


            #region "Properties"

            /// <summary>
            /// Returns true if current state has been already loaded.
            /// </summary>
            public bool IsLoaded
            {
                get
                {
                    return lastChange.HasValue;
                }
            }


            /// <summary>
            /// Values in this dictionary (implementation of ICacheWrapperResponse's property). User is interested in this value.
            /// </summary>
            public ICollection<TDataInner> Items
            {
                get
                {
                    return dictionary.Values;
                }
            }


            /// <summary>
            /// Dictionary of items.
            /// </summary>
            public Dictionary<TKeyInner, TDataInner> Dictionary
            {
                get
                {
                    return dictionary;
                }
            }


            /// <summary>
            /// Last change of data in this container.
            /// </summary>
            public DateTime LastChange
            {
                get
                {
                    return lastChange.Value;
                }
            }

            #endregion


            #region "Methods"

            /// <summary>
            /// Sets LastChange to the value passed in parameter if this parameter is greater than current LastChange.
            /// </summary>
            /// <param name="newLastChange">New last change</param>
            public void SetLastChangeIfGreater(DateTime newLastChange)
            {
                if (!lastChange.HasValue || (lastChange.Value < newLastChange))
                {
                    lastChange = newLastChange;
                }
            }

            #endregion
        }

        #endregion


        #region "Private fields"

        // Watches time needed to reload cache
        private readonly ChatCacheBeacon cacheBeacon;

        // Object used to synchronize in method loading data from cache
        private readonly object getCurrentStateFromCacheLock = new object();

        // Unique key used for creating cache keys
        private readonly string uniqueKey;

        private readonly FetchAllDataFunc<TData> fetchAllDataFunc;
        private readonly FetchChangedDataFunc<TData> fetchChangedDataFunc;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <param name="fetchAllDataFunc">Function which loads all data</param>
        /// <param name="fetchChangedDataFunc">Function which loads changed data</param>
        /// <param name="maxDelay">Persistence of cache (how often fetchChangedDataFunc will be called)</param>
        public ChatCurrentStateCacheWrapper(string key, FetchAllDataFunc<TData> fetchAllDataFunc, FetchChangedDataFunc<TData> fetchChangedDataFunc, TimeSpan maxDelay)
        {
            uniqueKey = key;

            this.fetchAllDataFunc = fetchAllDataFunc;
            this.fetchChangedDataFunc = fetchChangedDataFunc;

            // This cache will be used to check for invalidation from other web server and for expiring max delay
            cacheBeacon = new ChatCacheBeacon(uniqueKey + "|beacon", maxDelay);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// This method checks cache beacon and if it is needed (timeout or invalidation) it reloads data.
        /// </summary>
        protected virtual CurrentStateCacheWrapperData<TKey, TData> UpdateIfOutdated()
        {
            CurrentStateCacheWrapperData<TKey, TData> currentState = GetDataFromCache();

            // Check if beacon is not valid - this means, that data is too old or was invalidated (or has not been loaded yet)
            if (!cacheBeacon.IsValid())
            {
                // Data wasn't loaded yet (fetchAllDataFunc)
                if (!currentState.IsLoaded)
                {
                    lock (currentState)
                    {
                        // Second check inside lock
                        if (!currentState.IsLoaded)
                        {
                            LoadCurrentState(currentState);
                        }
                    }
                }
                // Updating data (fetchChangedDataFunc)
                else
                {
                    lock (currentState)
                    {
                        UpdateCurrentState(currentState);
                    }

                }
            }

            return currentState;
        }


        /// <summary>
        /// Gets data (CurrentStateCacheWrapperData) from classic CMS Cache. If data are not present, the new empty object is created and immediately inserted into cache.
        /// 
        /// Only this method should be used to get data from Cache, because it is thread safe.
        /// </summary>
        /// <returns>Data from cache</returns>
        protected virtual CurrentStateCacheWrapperData<TKey, TData> GetDataFromCache()
        {
            string cacheKey = uniqueKey + "|cache";

            CurrentStateCacheWrapperData<TKey, TData> outputValue;

            if (!CacheHelper.TryGetItem<CurrentStateCacheWrapperData<TKey, TData>>(cacheKey, out outputValue))
            {
                lock (getCurrentStateFromCacheLock)
                {
                    // Second check inside lock
                    if (!CacheHelper.TryGetItem<CurrentStateCacheWrapperData<TKey, TData>>(cacheKey, out outputValue))
                    {
                        outputValue = new CurrentStateCacheWrapperData<TKey, TData>();

                        // Add new data to cache
                        CacheHelper.Add(cacheKey, outputValue, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

                        // Invalidate beacon, se data will be loaded from DB
                        cacheBeacon.InvalidateLocally();
                    }
                }
            }

            return outputValue;
        }


        /// <summary>
        /// Perform the first load of data and fills passed structure with results. fetchAllDataFunc function is used.
        /// </summary>
        /// <param name="currentState">CurrentState data structure to be filled</param>
        protected virtual void LoadCurrentState(CurrentStateCacheWrapperData<TKey, TData> currentState)
        {
            List<TData> data = fetchAllDataFunc().ToList();

            foreach (TData item in data)
            {
                currentState.Dictionary[item.PK] = item;

                currentState.SetLastChangeIfGreater(item.ChangeTime);
            }
        }


        /// <summary>
        /// Updates data in current state data structure. Update means that only data changed since currentState.LastChange are loaded using fetchChangedDataFunc.
        /// </summary>
        /// <param name="currentState">CurrentState data structure to be updated</param>
        protected virtual void UpdateCurrentState(CurrentStateCacheWrapperData<TKey, TData> currentState)
        {
            IEnumerable<TData> newData = fetchChangedDataFunc(currentState.LastChange);

            foreach (TData item in newData)
            {
                switch (item.ChangeType)
                {
                    case ChangeTypeEnum.Modify:
                        currentState.Dictionary[item.PK] = item;
                        break;
                    case ChangeTypeEnum.Remove:
                        currentState.Dictionary.Remove(item.PK);
                        break;
                }

                currentState.SetLastChangeIfGreater(item.ChangeTime);
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current state of items. It is not older than 'maxDelay'
        /// </summary>
        public Dictionary<TKey, TData> CurrentState
        {
            get
            {
                return UpdateIfOutdated().Dictionary;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets current state together with time of last change.
        /// </summary>
        public ICacheWrapperResponse<TData> GetCurrentStateWithLastChange()
        {
            CurrentStateCacheWrapperData<TKey, TData> currentState = UpdateIfOutdated();

            // Cache has not been loaded yet
            if (!currentState.IsLoaded)
            {
                return null;
            }

            lock (currentState)
            {
                return currentState;
            }
        }


        /// <summary>
        /// Tries to get item from CurrentState. If item wasn't found, current state is reloaded and then it tries again.
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="item">Output data</param>
        /// <returns>True if item was found</returns>
        public bool ForceTryGetItem(TKey key, out TData item)
        {
            if (!CurrentState.TryGetValue(key, out item))
            {
                InvalidateLocally();
            }
            else
            {
                return true;
            }

            return CurrentState.TryGetValue(key, out item);
        }


        /// <summary>
        /// Invalidates cache and then tries to get item.
        /// </summary>
        /// <param name="key">PK</param>
        /// <param name="item">Output data</param>
        /// <returns>True if item was found</returns>
        public bool UpdateAndTryGetItem(TKey key, out TData item)
        {
            InvalidateLocally();

            return CurrentState.TryGetValue(key, out item);
        }


        /// <summary>
        /// Invalidates data - this means, that before next getting of data (get CurrentState) data will be updated.
        /// 
        /// Only on this machine.
        /// </summary>
        public void InvalidateLocally()
        {
            cacheBeacon.InvalidateLocally();
        }


        /// <summary>
        /// Invalidates data - this means, that before next getting of data (get CurrentState) data will be updated.
        /// 
        /// Across web farms.
        /// </summary>
        public void Invalidate()
        {
            cacheBeacon.Invalidate();
        }

        #endregion
    }
}

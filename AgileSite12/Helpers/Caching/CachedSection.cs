using System;
using System.Diagnostics;

using CMS.Core;
using CMS.Core.Internal;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Cached section context handler.
    /// </summary>
    public class CachedSection<TData> : CachedSection
    {
        #region "Variables"

        /// <summary>
        /// Data to be cached / retrieved
        /// </summary>
        protected TData mData;

        private IDateTimeNowService dateTimeNowService;

        #endregion


        #region "Properties"

        /// <summary>
        /// Data to be cached / retrieved
        /// </summary>
        public TData Data
        {
            get
            {
                return mData;
            }
            set
            {
                mData = value;
                mDataWasSet = true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new instance of <see cref="CachedSection"/> with absolute expiration.
        /// </summary>
        /// <param name="result">Returns the result of the cached data if available</param>
        /// <param name="cacheMinutes">Defines how long this item will stay in cache (in minutes)</param>
        /// <param name="condition">Cache condition</param>
        /// <param name="customCacheItemName">Custom cache item name</param>
        /// <param name="cacheItemNameParts">Cache item name parts</param>
        [HideFromDebugContext]
        public CachedSection(ref TData result, double cacheMinutes, bool condition, string customCacheItemName, params object[] cacheItemNameParts)
            : this(ref result, new CacheSettings(cacheMinutes, cacheItemNameParts)
            {
                BoolCondition = condition,
                CustomCacheItemName = customCacheItemName,
                AllowProgressiveCaching = false
            })
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="CachedSection"/>.
        /// </summary>
        /// <param name="result">Returns the result of the cached data if available</param>
        /// <param name="settings">Cache settings</param>
        [HideFromDebugContext]
        public CachedSection(ref TData result, CacheSettings settings)
            : this(ref result, settings, null)
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="CachedSection"/>.
        /// </summary>
        /// <param name="result">Returns the result of the cached data if available</param>
        /// <param name="settings">Cache settings</param>
        /// <param name="dateTimeNowService">DateTime.Now service used for testing purposes.</param>
        internal CachedSection(ref TData result, CacheSettings settings, IDateTimeNowService dateTimeNowService)
            : base(settings)
        {
            this.dateTimeNowService = dateTimeNowService ?? Service.Resolve<IDateTimeNowService>();

            var cached = Cached;

            if (cached || (settings.AllowProgressiveCaching && CacheHelper.ProgressiveCaching))
            {
                // Attempt to get the data
                if (cached)
                {
                    result = GetData();

                    // If not loaded, lock the thread
                    if (LoadData)
                    {
                        // Lock the thread to ensure one load only
                        EnterLock(ref result);

                        if (mLockAcquired)
                        {
                            // If 1st thread cached the result and leaved CS while any other thread was after cache check, but before the CS, then it's necessary to check the cache again
                            result = GetData();
                        }
                    }
                }
                else
                {
                    // Progressive caching, lock the context to avoid multiple loads
                    EnterLock(ref result);
                }
            }
        }


        /// <summary>
        /// Enters the cache lock
        /// </summary>
        /// <param name="result">Returning result of the operation in case other thread loaded that (waits for it in that case)</param>
        private void EnterLock(ref TData result)
        {
            mLock = LockHelper.GetLockObject(CacheItemName);

            // Initiate the read lock to avoid concurrency and contention
            if (mLock.EnterRead(ref result))
            {
                // First thread - Acquire the lock
                mLockAcquired = true;
            }
            else
            {
                // Other threads - Receive the data if read not failed (do not load)
                mLoadData = mLock.ReadFailed;
            }
        }


        /// <summary>
        /// Attempts to get the data from cache.
        /// </summary>
        [HideFromDebugContext]
        public TData GetData()
        {
            mLoadData = !CacheHelper.TryGetItem(CacheItemName, out mData);

            return mData;
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        [HideFromDebugContext]
        public override void Dispose()
        {
            if (!mRemoved && Cached && LoadData)
            {
                // Add data to the cache in case it was loaded (exception hasn't been thrown)
                if (mDataWasSet)
                {
                    var absoluteExpiration = Settings.UseSlidingExpiration ? CacheConstants.NoAbsoluteExpiration : dateTimeNowService.GetDateTimeNow().AddMinutes(CacheMinutes);
                    var slidingExpiration = Settings.UseSlidingExpiration ? TimeSpan.FromMinutes(CacheMinutes) : CacheConstants.NoSlidingExpiration;

                    CacheHelper.Add(CacheItemName, Data, CacheDependency, absoluteExpiration, slidingExpiration, CacheItemPriority);
                }
            }

            // Release the lock
            if (mLockAcquired)
            {
                // Check if the data was set
                if (LoadData && !mDataWasSet)
                {
                    // Reading failed, exit the lock and let other threads do the load on their own
                    mLock.FinishReadFailed();

                    // Log the error
                    LogDataError();
                }
                else
                {
                    // Exit the lock and provide data to other threads
                    mLock.FinishRead(Data);
                }
            }

            base.Dispose();
        }


        /// <summary>
        /// Loads the data with the given method if the section requests to load the data. This method supports progressive caching and is able to distribute unhandled exceptions to other simultaneously running threads.
        /// </summary>
        /// <param name="loadMethod">Method that loads the cached section data</param>
        internal bool LoadDataHandled(Func<CacheSettings, TData> loadMethod)
        {
            if (LoadData)
            {
                try
                {
                    Data = loadMethod(Settings);
                }
                catch (Exception ex)
                {
                    // Distribute the exception to other threads
                    Exception = ex;
                    throw;
                }

                return true;
            }

            return false;
        }

        #endregion
    }


    /// <summary>
    /// Cached section context handler.
    /// </summary>
    public abstract class CachedSection : Trackable<CachedSection>
    {
        #region "Variables"

        /// <summary>
        /// Object for the synchronization.
        /// </summary>
        protected LockObject mLock;

        /// <summary>
        /// True if the data should be loaded by the code.
        /// </summary>
        protected bool mLoadData = true;

        /// <summary>
        /// Flag whether the lock was acquired for this cached section
        /// </summary>
        protected bool mLockAcquired;

        /// <summary>
        /// Flag set when the item is removed from the cache
        /// </summary>
        protected bool mRemoved;

        /// <summary>
        /// Flag indicating whether the data property was set. The data property must be set if progressive caching is enabled
        /// </summary>
        protected bool mDataWasSet;

        /// <summary>
        /// Exception that occurred when loading data
        /// </summary>
        protected Exception mException;

        /// <summary>
        /// Cache settings
        /// </summary>
        protected CacheSettings mSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Cache settings
        /// </summary>
        public CacheSettings Settings
        {
            get
            {
                return mSettings ?? (mSettings = new CacheSettings(0));
            }
            protected set
            {
                mSettings = value;
            }
        }


        /// <summary>
        /// Cache condition.
        /// </summary>
        public bool BoolCondition
        {
            get
            {
                return Settings.BoolCondition;
            }
            set
            {
                Settings.BoolCondition = value;
            }
        }


        /// <summary>
        /// Cache minutes.
        /// </summary>
        public double CacheMinutes
        {
            get
            {
                return Settings.CacheMinutes;
            }
            set
            {
                Settings.CacheMinutes = value;
            }
        }


        /// <summary>
        /// Cache dependency.
        /// </summary>
        public CMSCacheDependency CacheDependency
        {
            get
            {
                return Settings.CacheDependency;
            }
            set
            {
                Settings.CacheDependency = value;
            }
        }


        /// <summary>
        /// Cache item priority.
        /// </summary>
        internal CMSCacheItemPriority CacheItemPriority
        {
            get
            {
                return Settings.CacheItemPriority;
            }
            set
            {
                Settings.CacheItemPriority = value;
            }
        }


        /// <summary>
        /// Cache item name used for the caching.
        /// </summary>
        public string CacheItemName
        {
            get
            {
                return Settings.CacheItemName;
            }
        }


        /// <summary>
        /// If true, the data is used from the cache if available / cached
        /// </summary>
        public bool Cached
        {
            get
            {
                return Settings.Cached;
            }
            set
            {
                Settings.Cached = value;
            }
        }


        /// <summary>
        /// If true, the external code should load the data. Using this property to control does not support distribution of unhandled exceptions in case of progressive caching. 
        /// </summary>
        public bool LoadData
        {
            get
            {
                return mLoadData;
            }
        }


        /// <summary>
        /// Exception that occurred within loading of the data
        /// </summary>
        public Exception Exception
        {
            get
            {
                return mException;
            }
            set
            {
                mException = value;

                // Set the lock object exception
                if (mLock != null)
                {
                    mLock.Exception = value;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Cache settings</param>
        protected CachedSection(CacheSettings settings)
        {
            mSettings = settings;
        }
        

        /// <summary>
        /// Removes the cache item
        /// </summary>
        public void Remove()
        {
            CacheHelper.Remove(CacheItemName);
            mRemoved = true;
        }


        /// <summary>
        /// Logs the error when data was not set
        /// </summary>
        protected void LogDataError()
        {
            if ((Exception == null) && SystemContext.DiagnosticLogging)
            {
                var st = new StackTrace();
                string message = "The Data property must be always set to some value (can be null) when LoadData is true before the CachedSection object is disposed in order to support all scenarios including Progressive caching and exception handling. Alternatively you can catch the exception if fired, and assign it to the CachedSection object to propagate the exception to other threads.\n\n" + st;

                CoreServices.EventLog.LogEvent("E", "CachedSection", "DATANOTSET", message);
            }
        }

        #endregion
    }
}
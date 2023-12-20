using System;
using System.ComponentModel;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control to provide basic data source for user controls.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSBaseDataSource : CMSAbstractBaseFilterControl, IUniPageable
    {
        #region "Variables"

        /// <summary>
        /// Fake dataset number of results.
        /// </summary>
        private int mPagerForceNumberOfResults = -1;

        /// <summary>
        /// Data source.
        /// </summary>
        protected object mDataSource = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get
            {
                return "DataSource_" + ID;
            }
            set
            {
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current data source contains selected item.
        /// </summary>
        public virtual bool IsSelected
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets source with data.
        /// </summary>
        public virtual object DataSource
        {
            get
            {
                if (mDataSource == null)
                {
                    mDataSource = GetDataSource();

                    // Handle the pager
                    if (LoadPagesIndividually)
                    {
                        PagerForceNumberOfResults = TotalRecords;
                    }

                    // Raise the page binding
                    RaiseOnPageBinding();
                }

                return mDataSource;
            }
            set
            {
                mDataSource = value;
            }
        }


        /// <summary>
        /// Related data.
        /// </summary>
        public virtual object RelatedData
        {
            get;
            set;
        }


        /// <summary>
        /// Total number of records.
        /// </summary>
        public int TotalRecords
        {
            get;
            set;
        }


        /// <summary>
        /// If true, each page is loaded individually in case of paging.
        /// </summary>
        public virtual bool LoadPagesIndividually
        {
            get;
            set;
        }


        /// <summary>
        /// Offset where the data retrieved from database should start.
        /// </summary>
        protected virtual int Offset
        {
            get
            {
                if (!LoadPagesIndividually || (UniPagerControl == null) || !UniPagerControl.Enabled)
                {
                    return 0;
                }

                return (UniPagerControl.CurrentPage - 1) * UniPagerControl.PageSize;
            }
        }


        /// <summary>
        /// Maximum number of records to get from the database.
        /// </summary>
        protected virtual int MaxRecords
        {
            get
            {
                if (!LoadPagesIndividually || (UniPagerControl == null) || !UniPagerControl.Enabled)
                {
                    return 0;
                }

                return UniPagerControl.PageSize;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads data from the database according to the current values of properties.
        /// </summary>
        /// <param name="forceReload">If true, the data is loaded even when already present</param>
        public object LoadData(bool forceReload)
        {
            if (forceReload)
            {
                InvalidateLoadedData();
            }

            return DataSource;
        }


        /// <summary>
        /// Gets data source according to caching properties.
        /// </summary>
        /// <returns>Dataset with data source</returns>
        protected object GetDataSource()
        {
            return GetDataSource(Offset, MaxRecords);
        }


        /// <summary>
        /// Gets data source according to caching properties.
        /// </summary>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <returns>Dataset with data source</returns>
        protected virtual object GetDataSource(int offset, int maxRecords)
        {
            if (StopProcessing)
            {
                return null;
            }

            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            object[] result = null;
            string useCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, GetDefaultCacheKey());

            // Try to get data from cache
            using (var cs = new CachedSection<object[]>(ref result, CacheMinutes, IsLiveSite, null, useCacheItemName, (maxRecords > 0 ? "paged|" + offset + "|" + maxRecords : null)))
            {
                if (cs.LoadData)
                {
                    DebugHelper.SetContext(ControlContext);

                    // Get the data
                    result = new object[3];
                    int totalRecords = 0;

                    if (LoadPagesIndividually)
                    {
                        // Get paged data
                        result[0] = GetDataSourceFromDB(offset, maxRecords, ref totalRecords);
                    }
                    else
                    {
                        // Get normal data
                        result[0] = GetDataSourceFromDB();
                    }

                    result[1] = GetRelatedData();
                    result[2] = totalRecords;

                    DebugHelper.ReleaseContext();

                    // Cache the data
                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = GetCacheDependency();
                    }

                    cs.Data = result;
                }
            }

            // Assign the data
            RelatedData = result[1];
            TotalRecords = (int)result[2];

            if (LoadPagesIndividually)
            {
                PagerForceNumberOfResults = TotalRecords;
            }

            return result[0];
        }


        /// <summary>
        /// Gets the default cache key.
        /// </summary>
        /// <returns>Default cache key</returns>
        protected virtual object[] GetDefaultCacheKey()
        {
            return new object[] { "basedatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy };
        }


        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <returns>Datasource from DB</returns>
        protected virtual object GetDataSourceFromDB()
        {
            int totalRecords = 0;
            var result = GetDataSourceFromDB(0, 0, ref totalRecords);

            return result;
        }


        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        /// <returns>Datasource from DB</returns>
        protected virtual object GetDataSourceFromDB(int offset, int maxRecords, ref int totalRecords)
        {
            return null;
        }


        /// <summary>
        /// Gets related data.
        /// </summary>
        /// <returns>Related data</returns>
        protected virtual object GetRelatedData()
        {
            return null;
        }


        /// <summary>
        /// Invalidate loaded data.
        /// </summary>
        public override void InvalidateLoadedData()
        {
            DataSource = null;
            RelatedData = null;
        }


        /// <summary>
        /// Resets pager data if pager is bound
        /// </summary>
        protected override void ResetPager()
        {
            base.ResetPager();

            if (UniPagerControl != null)
            {
                UniPagerControl.CurrentPage = 0;
            }
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public void ClearCache()
        {
            // Prepare the cache item name
            string useCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, GetDefaultCacheKey());

            CacheHelper.ClearCache(useCacheItemName);
        }

        #endregion


        #region "IUniPageable Members"

        /// <summary>
        /// Pager data item object.
        /// </summary>
        public object PagerDataItem
        {
            get
            {
                return mDataSource;
            }
            set
            {
                mDataSource = value;
            }
        }


        /// <summary>
        /// Pager control.
        /// </summary>
        public UniPager UniPagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Occurs when the control bind data.
        /// </summary>
        public event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Raises the page binding event.
        /// </summary>
        protected void RaiseOnPageBinding()
        {
            // Call page binding event
            if (OnPageBinding != null)
            {
                OnPageBinding(this, null);
            }
        }


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Evokes control databind.
        /// </summary>
        public void ReBind()
        {
            if (OnPageChanged != null)
            {
                OnPageChanged(this, null);
            }
        }


        /// <summary>
        /// Gets or sets the number of result. Enables proceed "fake" datasets, where number 
        /// of results in the dataset is not correspondent to the real number of results
        /// This property must be equal -1 if should be disabled
        /// </summary>
        public int PagerForceNumberOfResults
        {
            get
            {
                return mPagerForceNumberOfResults;
            }
            set
            {
                mPagerForceNumberOfResults = value;
            }
        }

        #endregion
    }
}
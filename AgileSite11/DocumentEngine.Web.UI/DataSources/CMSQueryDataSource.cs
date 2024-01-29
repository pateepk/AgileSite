using System;
using System.ComponentModel;
using System.Web.UI;
using System.Data;
using System.Web.UI.Design;

using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Query data source control.
    /// </summary>
    [ToolboxData("<{0}:CMSQueryDataSource runat=server></{0}:CMSQueryDataSource>"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSQueryDataSource : CMSBaseDataSource, ICMSQueryProperties
    {
        #region "Variables"

        private readonly CMSQueryProperties mProperties = new CMSQueryProperties();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether cache minutes must be set manually (Cache minutes value is independent on view mode and cache settings)
        /// </summary>
        public bool ForceCacheMinutes
        {
            get
            {
                return mProperties.ForceCacheMinutes;
            }
            set
            {
                mProperties.ForceCacheMinutes = value;
            }
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        public int SelectTopN
        {
            get
            {
                return mProperties.SelectTopN;
            }
            set
            {
                mProperties.SelectTopN = value;
            }
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        public string QueryName
        {
            get
            {
                return mProperties.QueryName;
            }
            set
            {
                mProperties.QueryName = value;
            }
        }


        /// <summary>
        /// Parameters for the query.
        /// </summary>
        public QueryDataParameters QueryParameters
        {
            get
            {
                return mProperties.QueryParameters;
            }
            set
            {
                mProperties.QueryParameters = value;
            }
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize
        {
            get
            {
                return mProperties.PageSize;
            }
            set
            {
                mProperties.PageSize = value;
            }
        }


        /// <summary>
        /// Parent control.
        /// </summary>
        public Control ParentControl
        {
            get
            {
                return mProperties.ParentControl;
            }
            set
            {
                mProperties.ParentControl = value;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public override string ControlContext
        {
            get
            {
                if (mProperties.ControlContext == null)
                {
                    return base.ControlContext;
                }

                return mProperties.ControlContext;
            }
            set
            {
                mProperties.ControlContext = value;
            }
        }


        /// <summary>
        /// Gets or sets query string key name. Presence of the key in query string indicates, 
        /// that some item should be selected. The item is determined by query string value.        
        /// </summary>
        public string SelectedQueryStringKeyName
        {
            get
            {
                return mProperties.SelectedQueryStringKeyName;
            }
            set
            {
                mProperties.SelectedQueryStringKeyName = value;
            }
        }


        /// <summary>
        /// Gets or sets columns name by which the item is selected.
        /// </summary>
        public string SelectedDatabaseColumnName
        {
            get
            {
                return mProperties.SelectedDatabaseColumnName;
            }
            set
            {
                mProperties.SelectedDatabaseColumnName = value;
            }
        }


        /// <summary>
        /// Gets or sets validation type for query string value which determines selected item. 
        /// Options are int, guid and string.
        /// </summary>
        public string SelectedValidationType
        {
            get
            {
                return mProperties.SelectedValidationType;
            }
            set
            {
                mProperties.SelectedValidationType = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current datasource contains  selected item.
        /// </summary>
        public override bool IsSelected
        {
            get
            {
                return mProperties.IsSelected;
            }
            set
            {
                mProperties.IsSelected = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets data source according to caching properties.
        /// </summary>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <returns>Dataset with data source</returns>
        protected override object GetDataSource(int offset, int maxRecords)
        {
            if (StopProcessing)
            {
                return null;
            }

            // Initialize data and return dataset
            InitDataProperties(mProperties);

            // Check if source filter is set
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(mProperties);
            }

            int totalRecords = 0;

            // Load the data
            DataSet ds = mProperties.LoadData(false, offset, maxRecords, ref totalRecords);
            TotalRecords = totalRecords;

            return ds;
        }


        /// <summary>
        /// Initializes properties.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public virtual void InitDataProperties(ICMSQueryProperties properties)
        {
            base.InitDataProperties(properties);

            // Set cache values
            mProperties.CacheDependencies = CacheDependencies;
            mProperties.CacheItemName = CacheItemName;
            mProperties.ForceCacheMinutes = ForceCacheMinutes;
            mProperties.CacheMinutes = CacheMinutes;
            mProperties.QueryParameters = QueryParameters;

        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            // Add document dependencies
            string newDependencies = QueryInfoProvider.GetQueryCacheDependencies(QueryName);
            if (newDependencies != null)
            {
                if (result != null)
                {
                    result += "\n";
                }

                result += newDependencies;
            }

            return result;
        }


        /// <summary>
        /// Sets the web part context.
        /// </summary>
        public virtual void SetContext()
        {
            mProperties.SetContext();
        }


        /// <summary>
        /// Releases the web part context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            mProperties.ReleaseContext();
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract class for filter controls.
    /// </summary>
    public abstract class CMSAbstractBaseFilterControl : AbstractUserControl, ICMSBaseProperties, IFilterControl
    {
        #region "Variables"

        private ICMSBaseProperties mProperties = new CMSBaseProperties();

        private string mCacheItemName;
        private string mCacheDependencies;
        private int? mCacheMinutes;

        private string mSiteName;
        private int mSiteID;

        private string mWhereCondition;
        private string mOrderBy;
        private int? mTopN;
        private string mSelectedColumns;

        private string mFilterName;
        private string mSourceFilterName;

        private CMSAbstractBaseFilterControl mFilterControl;
        private CMSAbstractBaseFilterControl mSourceFilterControl;

        private bool mChangedHandlerUsed;
        private bool mToggleHandlerUsed;
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Data properties
        /// </summary>
        protected CMSAbstractBaseProperties BaseProperties
        {
            get
            {
                return (CMSAbstractBaseProperties)mProperties;
            }
        }


        /// <summary>
        /// Filter change handler.
        /// </summary>
        public event ActionEventHandler OnFilterChanged;


        /// <summary>
        /// Stop processing.
        /// </summary>
        public override bool StopProcessing
        {
            get
            {
                return BaseProperties.StopProcessing;
            }
            set
            {
                BaseProperties.StopProcessing = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        public virtual string CacheItemName
        {
            get
            {
                return BaseProperties.CacheItemName;
            }
            set
            {
                BaseProperties.CacheItemName = value;
                mCacheItemName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        public virtual string CacheDependencies
        {
            get
            {
                return BaseProperties.CacheDependencies;
            }
            set
            {
                BaseProperties.CacheDependencies = value;
                mCacheDependencies = value;
            }
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        public virtual int CacheMinutes
        {
            get
            {
                return BaseProperties.CacheMinutes;
            }
            set
            {
                BaseProperties.CacheMinutes = value;
                mCacheMinutes = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the SiteName.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return BaseProperties.SiteName;
            }
            set
            {
                BaseProperties.SiteName = value;
                mSiteName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the filter SiteID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                if (mSiteID < 0)
                {
                    mSiteID = SiteContext.CurrentSiteID;
                }
                return mSiteID;
            }
            set
            {
                mSiteID = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the WhereCondition.
        /// </summary>
        public virtual string WhereCondition
        {
            get
            {
                return BaseProperties.WhereCondition;
            }
            set
            {
                BaseProperties.WhereCondition = value;
                mWhereCondition = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the OrderBy.
        /// </summary>
        public virtual string OrderBy
        {
            get
            {
                return BaseProperties.OrderBy;
            }
            set
            {
                BaseProperties.OrderBy = value;
                mOrderBy = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the Top N property.
        /// </summary>
        public virtual int TopN
        {
            get
            {
                return BaseProperties.TopN;
            }
            set
            {
                BaseProperties.TopN = value;
                mTopN = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Columns which should be selected.
        /// </summary>
        public string SelectedColumns
        {
            get
            {
                return BaseProperties.SelectedColumns;
            }
            set
            {
                BaseProperties.SelectedColumns = value;
                mSelectedColumns = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Property to set and get the Filter changed.
        /// </summary>
        public virtual bool FilterChanged
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the Filter name.
        /// </summary>
        public virtual string FilterName
        {
            get
            {
                return mFilterName;
            }
            set
            {
                mFilterName = value;
                CMSControlsHelper.SetFilter(value, this);
            }
        }


        /// <summary>
        /// Property to get if the filter is set.
        /// </summary>
        public virtual bool FilterIsSet
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Gets or sets the path of the filter control.
        /// </summary>
        public virtual string FilterControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Data filter control.
        /// </summary>
        public virtual Control FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (FilterControlPath != null)
                    {
                        try
                        {
                            mFilterControl = (Page.LoadUserControl(FilterControlPath)) as CMSAbstractBaseFilterControl;
                            if (mFilterControl != null)
                            {
                                mFilterControl.ID = "baseFilterControl";
                                Controls.AddAt(0, FilterControl);
                                mFilterControl.FilterName = FilterName;
                                if (Page != null)
                                {
                                    mFilterControl.Page = Page;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Filter control", "LOADFILTER", ex);
                        }
                    }
                }
                return mFilterControl;
            }
            set
            {
                mFilterControl = (CMSAbstractBaseFilterControl)value;
            }
        }


        /// <summary>
        /// Gets or sets the source filter name.
        /// </summary>
        public virtual string SourceFilterName
        {
            get
            {
                return mSourceFilterName;
            }
            set
            {
                mSourceFilterName = value;
                mSourceFilterControl = null;
            }
        }


        /// <summary>
        /// Gets the source filter control.
        /// </summary>
        public virtual CMSAbstractBaseFilterControl SourceFilterControl
        {
            get
            {
                if (mSourceFilterControl != null)
                {
                    return mSourceFilterControl;
                }
                else if (!String.IsNullOrEmpty(SourceFilterName))
                {
                    mSourceFilterControl = CMSControlsHelper.GetFilter(SourceFilterName) as CMSAbstractBaseFilterControl;
                }

                return mSourceFilterControl;
            }
        }


        /// <summary>
        /// Filtered control.
        /// </summary>
        public virtual Control FilteredControl
        {
            get;
            set;
        }


        /// <summary>
        /// Enables or disables filter caching.
        /// </summary>
        public virtual bool DisableFilterCaching
        {
            get;
            set;
        }


        /// <summary>
        /// Gets button used to toggle filter's advanced mode.
        /// </summary>
        public virtual IButtonControl ToggleAdvancedModeButton
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Filter value.
        /// </summary>
        public virtual object Value
        {
            get;
            set;
        }


        /// <summary>
        /// This value is used to initialize filter control according to currently selected value.
        /// </summary>
        public virtual string SelectedValue
        {
            get;
            set;
        }


        /// <summary>
        /// Allow to set filter mode (useful if same filter is used to filter different items)
        /// </summary>
        public virtual string FilterMode
        {
            get;
            set;
        }


        /// <summary>
        /// Hashtable with additional parameters.
        /// </summary>
        public virtual Hashtable Parameters
        {
            get;
            set;
        }

        #endregion


        #region "Page methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RegisterOnFilterChangedHandler();
            base.OnInit(e);
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            RegisterOnFilterChangedHandler();
            base.OnLoad(e);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Propagates given settings
        /// </summary>
        /// <param name="properties">Settings</param>
        protected void PropagateProperties(ICMSBaseProperties properties)
        {
            mProperties = properties;
        }


        /// <summary>
        /// Register OnFilterChanged handler
        /// </summary>
        protected void RegisterOnFilterChangedHandler()
        {
            if (!mChangedHandlerUsed && (SourceFilterControl != null))
            {
                // If the source filter has changed, bubble the event up to the repeater
                SourceFilterControl.OnFilterChanged += SourceFilterControl_OnFilterChanged;
                mChangedHandlerUsed = true;
            }

            if (!mToggleHandlerUsed && (ToggleAdvancedModeButton != null))
            {
                ToggleAdvancedModeButton.Click += ToggleAdvancedModeButton_Click;
                mToggleHandlerUsed = true;
            }
        }


        /// <summary>
        /// Toggle advanced mode button click event
        /// </summary>
        protected virtual void ToggleAdvancedModeButton_Click(object sender, EventArgs e)
        {
            FilterChanged = true;
            RaiseOnFilterChanged();
        }


        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public virtual void InitDataProperties(ICMSBaseProperties properties)
        {
            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            // Set cache values
            if (mCacheDependencies != null)
            {
                properties.CacheDependencies = CacheDependencies;
            }

            if (mCacheItemName != null)
            {
                properties.CacheItemName = CacheItemName;
            }

            if (mCacheMinutes != null)
            {
                properties.CacheMinutes = CacheMinutes;
            }

            if (mWhereCondition != null)
            {
                // Join the where conditions
                if (!CMSString.Equals(properties.WhereCondition, WhereCondition))
                {
                    properties.WhereCondition = SqlHelper.AddWhereCondition(properties.WhereCondition, WhereCondition);
                }
            }

            if (mOrderBy != null)
            {
                properties.OrderBy = mOrderBy;
            }

            if (mSiteName != null)
            {
                properties.SiteName = mSiteName;
            }

            if (DisableFilterCaching)
            {
                properties.CacheMinutes = 0;
            }

            if (mTopN != null)
            {
                properties.TopN = mTopN.Value;
            }

            if (mSelectedColumns != null)
            {
                properties.SelectedColumns = mSelectedColumns;
            }


            // Check whether property control is basic data source if so, invalidate data
            CMSBaseDataSource baseSource = properties as CMSBaseDataSource;
            if (baseSource != null)
            {
                baseSource.InvalidateLoadedData();
            }
        }


        /// <summary>
        /// Handles the OnFilterChanged event of the SourceFilter control.
        /// </summary>
        protected void SourceFilterControl_OnFilterChanged()
        {
            ResetPager();
            InvalidateLoadedData();
            RaiseOnFilterChanged();
        }


        /// <summary>
        /// Resets pager data
        /// </summary>
        protected virtual void ResetPager()
        {
            // Requires implementation in child class
        }


        /// <summary>
        /// Run OnDataFilterChanged handler.
        /// </summary>
        public virtual void RaiseOnFilterChanged()
        {
            if (FilterChanged)
            {
                if (OnFilterChanged != null)
                {
                    OnFilterChanged();
                }
            }
        }


        /// <summary>
        /// Invalidate loaded data.
        /// </summary>
        public virtual void InvalidateLoadedData()
        {
            // Requires implementation in child class
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return null;
        }


        /// <summary>
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            // If default, return default dependencies
            string dep = CacheHelper.GetCacheDependencies(CacheDependencies, GetDefaultCacheDependencies());

            return CacheHelper.GetCacheDependency(dep);
        }


        /// <summary>
        /// Resets filter to default state.
        /// </summary>
        public virtual void ResetFilter()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region "State management"

        /// <summary>
        /// Stores filter state to the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        public virtual void StoreFilterState(FilterState state)
        {
            FilterHelper.StoreCustomFilterState(this, state);
        }


        /// <summary>
        /// Restores filter state from the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        public virtual void RestoreFilterState(FilterState state)
        {
            FilterHelper.RestoreCustomFilterState(this, state);
        }

        #endregion
    }
}
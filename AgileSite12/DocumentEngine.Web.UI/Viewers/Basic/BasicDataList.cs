using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Extended DataList control that automates data binding.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:BasicDataList runat=server></{0}:BasicDataList>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicDataList : UIDataList, IUniPageable, IUniPageableContainer, IRelatedData
    {
        #region "Variables"

        /// <summary>
        /// Data source control
        /// </summary>
        private CMSBaseDataSource mDataSourceControl;

        /// <summary>
        /// Flag saying whether the dynamic controls has been loaded yet.
        /// </summary>
        protected bool mControlsLoaded;

        /// <summary>
        /// Flag saying whether the data has been loaded yet.
        /// </summary>
        protected bool mDataLoaded;

        /// <summary>
        /// Indicates whether dataset is load from view state.
        /// </summary>
        private bool loadedFromViewState;
        
        /// <summary>
        /// Fake dataset number of results.
        /// </summary>
        private int mPagerForceNumberOfResults = -1;

        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData;

        // Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        private bool mDefaultLoadCalled;
        
        /// <summary>
        /// Indicates whether the viewer should obtain data source from the referenced control provided by the value of DataSourceControl property.
        /// </summary>
        protected bool useDataSourceControl = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether dynamic controls should be resolved
        /// </summary>
        public bool ResolveDynamicControls
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ResolveDynamicControls"], true);
            }
            set
            {
                ViewState["ResolveDynamicControls"] = value;
            }
        }


        /// <summary>
        /// Data source name.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DataSourceName"], "");
            }
            set
            {
                ViewState["DataSourceName"] = value;
            }
        }


        /// <summary>
        /// Default data source to use if no external is provided.
        /// </summary>
        protected virtual CMSBaseDataSource DefaultDataSource
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Control with data source.
        /// </summary>
        public virtual CMSBaseDataSource DataSourceControl
        {
            get
            {
                // Check if control is empty and load it with the data
                if (mDataSourceControl == null)
                {
                    if (!String.IsNullOrEmpty(DataSourceName))
                    {
                        mDataSourceControl = CMSControlsHelper.GetFilter(DataSourceName) as CMSBaseDataSource;
                        BoundPagerToDataSource(mDataSourceControl);
                    }
                    else
                    {
                        mDataSourceControl = DefaultDataSource;
                    }
                }

                return mDataSourceControl;
            }
            set
            {
                mDataSourceControl = value;
                BoundPagerToDataSource(mDataSourceControl);
            }
        }


        /// <summary>
        /// Object from which data-bound control retrieves its list of data item.
        /// </summary>
        public override object DataSource
        {
            get
            {

                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
                useDataSourceControl = false;
            }
        }


        /// <summary>
        /// Custom data connected to the object, if not set, returns the Related data of the nearest IDataControl.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                if ((mRelatedData == null) && !mRelatedDataLoaded)
                {
                    // Load the related data to the object
                    mRelatedDataLoaded = true;
                    IRelatedData dataControl = (IRelatedData)ControlsHelper.GetParentControl(this, typeof(IRelatedData));
                    if (dataControl != null)
                    {
                        mRelatedData = dataControl.RelatedData;
                    }
                }

                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
            }
        }


        /// <summary>
        /// Indicates whether data binding should be performed by default.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether data binding should be performed by default.")]
        public virtual bool DataBindByDefault
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DataBindByDefault"], true);
            }
            set
            {
                ViewState["DataBindByDefault"] = value;
            }
        }


        /// <summary>
        /// Hides the control when no data is loaded. The default value is False.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Hides the control when no data loaded. Default value is False.")]
        public virtual bool HideControlForZeroRows
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HideControlForZeroRows"], false);
            }
            set
            {
                ViewState["HideControlForZeroRows"] = value;
            }
        }


        /// <summary>
        /// Text to be shown when the control is hidden by HideControlForZeroRows.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when the control is hidden by HideControlForZeroRows.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return ResHelper.LocalizeString(ValidationHelper.GetString(ViewState["ZeroRowsText"], ""));
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the encapsulated control that implements IUniPageable interface.
        /// </summary>
        public virtual IUniPageable PageableControl
        {
            get
            {
                return DataSourceControl;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// True if the on before init was fired.
        /// </summary>
        protected bool mOnBeforeInitFired;


        /// <summary>
        /// On before init handler.
        /// </summary>
        public event EventHandler OnBeforeInit;


        /// <summary>
        /// Raises the OnBeforeInit event.
        /// </summary>
        protected void RaiseOnBeforeInit()
        {
            if ((OnBeforeInit != null) && !mOnBeforeInitFired)
            {
                mOnBeforeInitFired = true;
                OnBeforeInit(this, null);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicDataList()
        {
            ItemDataBound += BasicDataList_ItemDataBound;
            DataBinding += BasicDataList_DataBinding;
        }


        /// <summary>
        /// Handles DataBinding event of the datalist.
        /// </summary>
        private void BasicDataList_DataBinding(object sender, EventArgs e)
        {
            // Retrieve data source from data source control
            if (useDataSourceControl && (DataSourceControl != null))
            {
                base.DataSource = DataSourceControl.DataSource;
            }

            // Merge the data if more than one table
            if (DataSource is DataSet && string.IsNullOrEmpty(DataMember))
            {
                DataSet ds = (DataSet)DataSource;
                while (ds.Tables.Count > 1)
                {
                    ds.Tables[0].Merge(ds.Tables[1]);
                    ds.Tables.RemoveAt(1);
                }
            }
        }


        private void BasicDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (ResolveDynamicControls)
            {
                ControlsHelper.ResolveDynamicControls(e.Item);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RaiseOnBeforeInit();
            base.OnInit(e);
            Page.InitComplete += Page_InitComplete;
        }


        /// <summary>
        /// Default handling for bind in init complete event
        /// </summary>
        void Page_InitComplete(object sender, EventArgs e)
        {
            InitControl(false);
        }


        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            InitControl(true);
            base.OnLoad(e);
        }


        /// <summary>
        /// Bound pager to the external external DataSource
        /// </summary>
        /// <param name="dataSource">DataSource control</param>
        protected void BoundPagerToDataSource(CMSBaseDataSource dataSource)
        {
            if (dataSource != null)
            {
                if ((UniPagerControl != null) && (dataSource.UniPagerControl == null))
                {
                    UniPagerControl.ReloadData(true);
                }

                // Propagate onPageChanged event from datasource to the main control
                dataSource.OnPageChanged += dataSource_OnPageChanged;
            }
        }


        /// <summary>
        /// Propagates PageChanged event
        /// </summary>
        void dataSource_OnPageChanged(object sender, EventArgs e)
        {
            if (OnPageChanged != null)
            {
                OnPageChanged(this, null);
            }
        }


        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected virtual void InitControl(bool loadPhase)
        {
            if (!mDefaultLoadCalled)
            {
                mDefaultLoadCalled = true;
                if (DataBindByDefault)
                {
                    ReloadData(false);
                }
            }
        }


        /// <summary>
        /// Reloads the control data, skips the loading if the data is already loaded.
        /// </summary>
        /// <param name="forceReload">If true, the data is reloaded even when already loaded</param>
        public virtual void ReloadData(bool forceReload)
        {
            // If already loaded, exit
            if (mDataLoaded && !forceReload)
            {
                return;
            }
            mDataLoaded = true;

            // Reload the data
            if (!DataHelper.IsEmpty(DataSource))
            {
                DataBind();
            }
            //LoadDynamicControls(forceReload);
        }


        /// <summary>
        /// Render event handler, to maintain the proper control rendering.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Is empty, apply the hiding rules
            if (DataHelper.DataSourceIsEmpty(DataSource) && (!loadedFromViewState))
            {
                // Hide the control when HideControlForZeroRows is set
                if (HideControlForZeroRows)
                {
                    return;
                }
                
                if (!string.IsNullOrEmpty(ZeroRowsText))
                {
                    // If the ZeroRowsText is set, display it instead if the control itself
                    writer.Write(ZeroRowsText);
                    return;
                }
            }

            LoadDynamicControls(false);
            base.Render(writer);
        }


        /// <summary>
        /// Handles the Item Create event and creates dynamic control s if present.
        /// </summary>
        protected void LoadDynamicControls(bool forceReload)
        {
            // If already loaded, exit
            if (mControlsLoaded && !forceReload)
            {
                return;
            }
            mControlsLoaded = true;

            // Reload the controls within all the items
            if (ResolveDynamicControls)
            {
                foreach (DataListItem item in Items)
                {
                    ControlsHelper.ResolveDynamicControls(item);
                }
            }
        }


        /// <summary>
        /// Creates a control hierarchy with or without the specified data source.
        /// </summary>
        /// <param name="useDataSource">Indicates whether use data source</param>
        protected override void CreateControlHierarchy(bool useDataSource)
        {
            // Call page binding event
            if (OnPageBinding != null)
            {
                OnPageBinding(this, null);
            }

            base.CreateControlHierarchy(useDataSource);

            // If control doesn't use data source => data source is loaded from view state
            loadedFromViewState = !useDataSource;
        }


        /// <summary>
        /// Indicates if data source contains data.
        /// </summary>
        public virtual bool HasData()
        {
            return (!DataHelper.DataSourceIsEmpty(DataSource));
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
                return DataSource;
            }
            set
            {
                DataSource = value;
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

            DataBind();
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
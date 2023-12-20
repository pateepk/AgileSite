using System;
using System.Collections;
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
    /// Extended Repeater control that automates databinding.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:BasicRepeater runat=server></{0}:BasicRepeater>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicRepeater : UIRepeater, IUniPageable, IUniPageableContainer, IRelatedData
    {
        #region "Variables"

        private CMSBaseDataSource mDataSourceControl;

        /// <summary>
        /// Flag saying whether the dynamic controls has been loaded yet.
        /// </summary>
        protected bool mControlsLoaded = false;

        /// <summary>
        /// Flag saying whether the data has been loaded yet.
        /// </summary>
        protected bool mDataLoaded = false;

        /// <summary>
        /// Indicates whether dataset is load from viewstate.
        /// </summary>
        private bool loadedFromViewState;

        /// <summary>
        /// Fake dataset number of results.
        /// </summary>
        private int mPagerForceNumberOfResults = -1;

        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded = false;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData = null;

        // Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        private bool defaultLoadCalled;

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
        /// Gets or sets name of source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DataSourceName"], string.Empty);
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
        /// Hides the control when no data is loaded. Default value is False.
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


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicRepeater()
        {
            ItemDataBound += BasicRepeater_ItemDataBound;
            DataBinding += BasicRepeater_DataBinding;
        }


        /// <summary>
        /// Handles DataBinding event of the repeater.
        /// </summary>
        private void BasicRepeater_DataBinding(object sender, EventArgs e)
        {
            // Retrieve data source from data source control
            if (useDataSourceControl && (DataSourceControl != null))
            {
                base.DataSource = DataSourceControl.DataSource;
            }
        }


        /// <summary>
        /// Resolve dynamic controls on Item data bound.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">RepeaterItemEventArgs</param>
        private void BasicRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (ResolveDynamicControls)
            {
                ControlsHelper.ResolveDynamicControls(e.Item);
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
        }


        /// <summary>
        /// Indicates if datasource contains data.
        /// </summary>
        public virtual bool HasData()
        {
            return (!DataHelper.DataSourceIsEmpty(DataSource));
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
                dataSource.OnPageChanged +=dataSource_OnPageChanged;
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
            if (!defaultLoadCalled)
            {
                defaultLoadCalled = true;
                if (DataBindByDefault)
                {
                    ReloadData(false);
                }
            }
        }


        /// <summary>
        /// Creates a control hierarchy with or without the specified datasource.
        /// </summary>
        /// <param name="useDataSource">Indicates whether use datasource</param>
        protected override void CreateControlHierarchy(bool useDataSource)
        {
            // Call page binding event
            if (OnPageBinding != null)
            {
                OnPageBinding(this, null);
            }

            base.CreateControlHierarchy(useDataSource);

            loadedFromViewState = !useDataSource;
        }


        /// <summary>
        /// OnPreRender override.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Special handling for pager within abstract transformation
            if ((OnPageBinding != null) && (Parent is CMSAbstractTransformation))
            {
                OnPageBinding(this, null);
            }
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

                // If the ZeroRowsText is set, display it instead if the control itself
                if (!String.IsNullOrEmpty(ZeroRowsText))
                {
                    writer.Write(ZeroRowsText);
                    return;
                }
            }

            base.Render(writer);
        }
        

        /// <summary>
        /// Binds the data.
        /// </summary>
        public override void DataBind()
        {
            // Calling base class DataBind method MUST be performed first
            base.DataBind();

            // Bind additional tables if present
            if (DataSource is DataSet && (string.IsNullOrEmpty(DataMember)))
            {
                DataSet ds = (DataSet)DataSource;
                int tableIndex = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    // Add only 1+ tables
                    if (tableIndex > 0)
                    {
                        int itemIndex = ds.Tables[0].Rows.Count;
                        // Bind all rows
                        IEnumerator viewEnum = dt.DefaultView.GetEnumerator();
                        while (viewEnum.MoveNext())
                        {
                            // Get the row view
                            DataRowView dr = (DataRowView)viewEnum.Current;

                            // Create new item
                            RepeaterItem newItem = CreateItem(itemIndex, ListItemType.Item);
                            InitializeItem(newItem);

                            Controls.Add(newItem);

                            // Bind the item
                            newItem.DataItem = dr;
                            newItem.DataBind();

                            // Raise the item databound event
                            RepeaterItemEventArgs e = new RepeaterItemEventArgs(newItem);
                            OnItemDataBound(e);

                            itemIndex++;
                        }
                    }

                    tableIndex++;
                }
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
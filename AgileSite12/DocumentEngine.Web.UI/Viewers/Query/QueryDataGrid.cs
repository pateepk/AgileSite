using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// QueryDataGrid control.
    /// </summary>
    [ToolboxData("<{0}:QueryDataGrid runat=server></{0}:QueryDataGrid>")]
    public class QueryDataGrid : BasicDataGrid, ICMSQueryProperties
    {
        #region "Variables"

        private readonly CMSQueryProperties mProperties = new CMSQueryProperties();

        private bool mDataLoaded = false;

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;


        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractQueryFilterControl mFilterControl = null;

        // Fake sitename due to interface ICMSBaseProperties
        private string mSiteName = "";

        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool handlersRegistered = false;

        #endregion


        #region "Public properties"

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
        /// Gets or sets the sitename, this property has no effect in this control.
        /// </summary>
        public string SiteName
        {
            get
            {
                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Columns to select.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Columns to select, null or empty select all columns.")]
        public string SelectedColumns
        {
            get
            {
                return mProperties.SelectedColumns;
            }
            set
            {
                mProperties.SelectedColumns = value;
            }
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select top N rows.")]
        public int TopN
        {
            get
            {
                return mProperties.TopN;
            }
            set
            {
                mProperties.TopN = value;
            }
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Number of items per page.")]
        public override int PageSize
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
        /// Stop processing 
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Stop processing.")]
        public virtual bool StopProcessing
        {
            get
            {
                return mProperties.StopProcessing;
            }
            set
            {
                mProperties.StopProcessing = value;
            }
        }


        /// <summary>
        /// Query name in format application.class.query.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Query name in format application.class.query.")]
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
        /// Query parameters.
        /// </summary>
        /// <remarks>Array with Query parameters</remarks>
        [Description("Query parameters"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// Property to set and get the WhereCondition.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Where condition.")]
        public virtual string WhereCondition
        {
            get
            {
                return mProperties.WhereCondition;
            }
            set
            {
                mProperties.WhereCondition = value;
            }
        }


        /// <summary>
        /// Property to set and get the OrderBy.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Order By expression.")]
        public virtual string OrderBy
        {
            get
            {
                return mProperties.OrderBy;
            }
            set
            {
                mProperties.OrderBy = value;
            }
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        [Category("Behavior"), DefaultValue(0), Description("Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.")]
        public virtual int CacheMinutes
        {
            get
            {
                return mProperties.CacheMinutes;
            }
            set
            {
                mProperties.CacheMinutes = value;
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        [Category("Behavior"), DefaultValue(""), Description("Name of the cache item the control will use.")]
        public virtual string CacheItemName
        {
            get
            {
                return mProperties.CacheItemName;
            }
            set
            {
                mProperties.CacheItemName = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Cache dependencies, each cache dependency on a new line.")]
        public virtual string CacheDependencies
        {
            get
            {
                return mProperties.CacheDependencies;
            }
            set
            {
                mProperties.CacheDependencies = value;
            }
        }


        /// <summary>
        /// Current page number.
        /// </summary>
        public int Current
        {
            get
            {
                if (ViewState["Current"] == null)
                {
                    return 1;
                }
                else
                {
                    return Convert.ToInt32(ViewState["Current"]);
                }
            }
            set
            {
                ViewState["Current"] = value;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get
            {
                return mProperties.ControlContext;
            }
            set
            {
                mProperties.ControlContext = value;
            }
        }


        /// <summary>
        /// Filter control.
        /// </summary>
        public CMSAbstractQueryFilterControl FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (FilterName != null)
                    {
                        mFilterControl = CMSControlsHelper.GetFilter(FilterName) as CMSAbstractQueryFilterControl;
                    }
                }
                return mFilterControl;
            }
            set
            {
                mFilterControl = value;
            }
        }


        /// <summary>
        /// Gets or Set filter name.
        /// </summary>
        public string FilterName
        {
            get
            {
                return mFilterName;
            }
            set
            {
                mFilterName = value;
            }
        }


        /// <summary>
        /// Top N rows to select.
        /// </summary>    
        public int SelectTopN
        {
            get
            {
                return TopN;
            }
            set
            {
                TopN = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public QueryDataGrid()
        {
            mProperties.ParentControl = this;

            if (Context != null)
            {
                ProcessSorting = false;
            }
        }


        /// <summary>
        /// OnLoad.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (!StopProcessing)
            {
                ReloadData(false);

                base.OnLoad(e);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RaiseOnBeforeInit();

            if (Context == null)
            {
                return;
            }

            if (!StopProcessing && !handlersRegistered)
            {
                RegisterHandlers();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Fix pager position
            if (AllowPaging && AutoGenerateColumns)
            {
                // Get the table element
                Table tab = (Table)Controls[0];
                // Change the top pager
                if (PagerStyle.Position == PagerPosition.Bottom || PagerStyle.Position == PagerPosition.TopAndBottom)
                {
                    if (tab.Rows.Count > 1)
                    {
                        TableRow tr = tab.Rows[tab.Rows.Count - 1];
                        if (tr.Cells.Count > 0)
                        {
                            tr.Cells[0].Attributes.Add("colspan", tab.Rows[1].Cells.Count.ToString());
                        }
                    }
                }
                // Change the bottom pager
                if (PagerStyle.Position == PagerPosition.Top || PagerStyle.Position == PagerPosition.TopAndBottom)
                {
                    if (tab.Rows.Count > 1)
                    {
                        TableRow tr = tab.Rows[0];
                        if (tr.Cells.Count > 0)
                        {
                            tr.Cells[0].Attributes.Add("colspan", tab.Rows[1].Cells.Count.ToString());
                        }
                    }
                }
            }
        }


        /// <summary>
        /// OnSortCommand event handler. It sorts data by selected column.
        /// </summary>
        protected override void OnSortCommand(DataGridSortCommandEventArgs e)
        {
            base.OnSortCommand(e);
            ReloadData(true);
        }


        /// <summary>
        /// OnPageIndexChanged event handler. It ensures displaying of the given page.
        /// </summary>
        protected override void OnPageIndexChanged(DataGridPageChangedEventArgs e)
        {
            base.OnPageIndexChanged(e);
            ReloadData(true);
        }


        /// <summary>
        /// Reloads the control data.
        /// </summary>
        /// <param name="forceReload">If true, data are forced to be reloaded</param>
        public void ReloadData(bool forceReload)
        {
            ProcessSorting = true;

            if (!StopProcessing || forceReload)
            {
                SetContext();

                RegisterHandlers();

                // If already loaded, exit
                if (mDataLoaded && !forceReload)
                {
                    return;
                }
                mDataLoaded = true;

                if (string.IsNullOrEmpty(QueryName))
                {
                    return;
                }

                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(mProperties);
                }

                // Load the data
                DataSource = mProperties.LoadData(false);

                if (string.IsNullOrEmpty(SortField) && !string.IsNullOrEmpty(OrderBy))
                {
                    //SortAscending = !(SortAscending);
                    SortField = OrderBy;
                }

                // If no data found, create an empty Data source
                if (DataSource == null)
                {
                    DataSource = new DataSet().Tables.Add();
                }

                // Bind the data
                if (DataBindByDefault || forceReload)
                {
                    DataBind();
                }

                ReleaseContext();
            }
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


        /// <summary>
        /// Data filter control handler.
        /// </summary>
        private void FilterControl_OnFilterChanged()
        {
            // Set forcibly parent visibility
            if (Parent != null)
            {
                Parent.Visible = true;
            }

            ReloadData(true);
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public virtual void ClearCache()
        {
            mProperties.ClearCache();
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return mProperties.GetDefaultCacheDependencies();
        }


        /// <summary>
        /// Registers event handlers and js scripts.
        /// </summary>
        private void RegisterHandlers()
        {
            if (!handlersRegistered && (FilterControl != null))
            {
                FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                handlersRegistered = true;
            }
        }

        #endregion
    }
}
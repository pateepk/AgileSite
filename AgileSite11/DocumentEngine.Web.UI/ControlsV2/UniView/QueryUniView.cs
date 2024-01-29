using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// QueryUniView control.
    /// </summary>
    [ToolboxData("<{0}:QueryUniView runat=server></{0}:QueryUniView>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    [ParseChildren(true)]
    [PersistChildren(true)]
    public class QueryUniView : BasicUniView, ICMSQueryProperties
    {
        #region "Variables"

        /// <summary>
        /// Query data source.
        /// </summary>
        protected CMSQueryDataSource mDataSource = new CMSQueryDataSource();


        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName;


        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl;


        /// <summary>
        /// Data pager.
        /// </summary>
        protected DataPager mDataPager = null;


        /// <summary>
        /// External pager control?
        /// </summary>
        protected bool mExternalPager = false;


        /// <summary>
        /// When DataSource is empty NoData  = true.
        /// </summary>
        protected bool NoData;


        /// <summary>
        /// ItemSeparator variable.
        /// </summary>
        protected string mItemSeparator = "";


        /// <summary>
        /// Indicates if page size property is set.
        /// </summary>
        protected bool mPageSizeSet = false;

        // Fake site name due to interface ICMSBaseProperties
        private string mSiteName = "";

        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool mHandlersRegistered;

        /// <summary>
        /// Current unipager.
        /// </summary>
        protected UniPager mUniPager;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether cache minutes must be set manually (Cache minutes value is independent on view mode and cache settings)
        /// </summary>
        public bool ForceCacheMinutes
        {
            get
            {
                return mDataSource.ForceCacheMinutes;
            }
            set
            {
                mDataSource.ForceCacheMinutes = value;
            }
        }


        /// <summary>
        /// Indicates whether the data should be loaded to the load event instead of default init event.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether the data should be loaded to the load event instead of default init event.")]
        public bool DelayedLoading
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DelayedLoading"], false);
            }
            set
            {
                ViewState["DelayedLoading"] = value;
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
                return mDataSource.SelectedColumns;
            }
            set
            {
                mDataSource.SelectedColumns = value;
            }
        }


        /// <summary>
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>    
        public string Columns
        {
            get
            {
                return SelectedColumns;
            }
            set
            {
                SelectedColumns = value;
            }
        }


        /// <summary>
        /// Indicates whether data binding should be performed by default.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether data binding should be performed by default.")]
        public new bool DataBindByDefault
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["CMSDataBindByDefault"], true);
            }
            set
            {
                ViewState["CMSDataBindByDefault"] = value;
            }
        }


        /// <summary>
        /// Show Edit and Delete buttons.
        /// </summary>
        public bool ShowEditDeleteButtons
        {
            get
            {
                return ShowEditButton && ShowDeleteButton;
            }
            set
            {
                ShowEditButton = value;
                ShowDeleteButton = value;
            }
        }


        /// <summary>
        /// Show Delete button.
        /// </summary>
        public bool ShowDeleteButton
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowDeleteButton"], false);
            }
            set
            {
                ViewState["ShowDeleteButton"] = value;
            }
        }


        /// <summary>
        /// Show Edit and Delete buttons.
        /// </summary>
        public bool ShowEditButton
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowEditButton"], false);
            }
            set
            {
                ViewState["ShowEditButton"] = value;
            }
        }


        /// <summary>
        /// Indicates edit buttons mode.
        /// </summary>
        private EditModeButtonEnum EditButtonsMode
        {
            get
            {
                if (ShowEditButton && ShowDeleteButton)
                {
                    return EditModeButtonEnum.Both;
                }
                if (ShowEditButton)
                {
                    return EditModeButtonEnum.Edit;
                }
                if (ShowDeleteButton)
                {
                    return EditModeButtonEnum.Delete;
                }

                return EditModeButtonEnum.None;
            }
        }


        /// <summary>
        /// Parent control.
        /// </summary>
        [Browsable(false)]
        public Control ParentControl
        {
            get
            {
                return mDataSource.ParentControl;
            }
            set
            {
                mDataSource.ParentControl = value;
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
                return mDataSource.TopN;
            }
            set
            {
                mDataSource.TopN = value;
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
                return mDataSource.StopProcessing;
            }
            set
            {
                mDataSource.StopProcessing = value;
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
                return mDataSource.QueryName;
            }
            set
            {
                mDataSource.QueryName = value;
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
                return mDataSource.QueryParameters;
            }
            set
            {
                mDataSource.QueryParameters = value;
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
                return mDataSource.WhereCondition;
            }
            set
            {
                mDataSource.WhereCondition = value;
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
                return mDataSource.OrderBy;
            }
            set
            {
                mDataSource.OrderBy = value;
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
                return mDataSource.CacheMinutes;
            }
            set
            {
                mDataSource.CacheMinutes = value;
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
                return mDataSource.CacheItemName;
            }
            set
            {
                mDataSource.CacheItemName = value;
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
                return mDataSource.CacheDependencies;
            }
            set
            {
                mDataSource.CacheDependencies = value;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get
            {
                return mDataSource.ControlContext;
            }
            set
            {
                mDataSource.ControlContext = value;
            }
        }


        /// <summary>
        /// Filter control.
        /// </summary>
        public CMSAbstractBaseFilterControl FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (!String.IsNullOrEmpty(FilterName))
                    {
                        mFilterControl = CMSControlsHelper.GetFilter(FilterName) as CMSAbstractBaseFilterControl;
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


        /// <summary>
        /// Gets or sets query string key name. Presence of the key in query string indicates, 
        /// that some item should be selected. The item is determined by query string value.        
        /// </summary>
        public string SelectedQueryStringKeyName
        {
            get
            {
                return mDataSource.SelectedQueryStringKeyName;
            }
            set
            {
                mDataSource.SelectedQueryStringKeyName = value;
            }
        }


        /// <summary>
        /// Gets or sets columns name by which the item is selected.
        /// </summary>
        public string SelectedDatabaseColumnName
        {
            get
            {
                return mDataSource.SelectedDatabaseColumnName;
            }
            set
            {
                mDataSource.SelectedDatabaseColumnName = value;
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
                return mDataSource.SelectedValidationType;
            }
            set
            {
                mDataSource.SelectedValidationType = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current datasource contains  selected item.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return mDataSource.IsSelected;
            }
            set
            {
                mDataSource.IsSelected = value;
            }
        }


        /// <summary>
        /// Default data source to use if no external is provided
        /// </summary>
        protected override CMSBaseDataSource DefaultDataSource
        {
            get
            {
                return mDataSource;
            }
        }


        /// <summary>
        /// If true, each page is loaded individually in case of paging.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If true, each page is loaded individually in case of paging")]
        public virtual bool LoadPagesIndividually
        {
            get
            {
                return mDataSource.LoadPagesIndividually;
            }
            set
            {
                mDataSource.LoadPagesIndividually = value;
            }
        }

        #endregion


        #region "Transformation properties"

        /// <summary>
        /// Transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation.")]
        public string TransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TransformationName"], "");
            }
            set
            {
                ViewState["TransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to alternating items.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to alternating items.")]
        public string AlternatingTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["AlternatingTransformationName"], "");
            }
            set
            {
                ViewState["AlternatingTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to header item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to header item.")]
        public string HeaderTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HeaderTransformationName"], String.Empty);
            }
            set
            {
                ViewState["HeaderTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to footer item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to footer item.")]
        public string FooterTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FooterTransformationName"], String.Empty);
            }
            set
            {
                ViewState["FooterTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to footer item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to first item.")]
        public string FirstTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["FirstTransformationName"], String.Empty);
            }
            set
            {
                ViewState["FirstTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to last item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to last item.")]
        public string LastTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["LastTransformationName"], String.Empty);
            }
            set
            {
                ViewState["LastTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to single item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to single item.")]
        public string SingleTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SingleTransformationName"], String.Empty);
            }
            set
            {
                ViewState["SingleTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation applied to footer item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to first item.")]
        public string SeparatorTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SeparatorTransformationName"], String.Empty);
            }
            set
            {
                ViewState["SeparatorTransformationName"] = value;
            }
        }


        /// <summary>
        /// Item separator between displayed records.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Item separator between displayed records.")]
        public string ItemSeparatorValue
        {
            get
            {
                return mItemSeparator;
            }
            set
            {
                mItemSeparator = value;
            }
        }

        #endregion


        #region "Pager templates"

        /// <summary>
        /// Page numbers template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerPageNumbersTemplate
        {
            get
            {
                return PagerControl.PageNumbersTemplate;
            }
            set
            {
                PagerControl.PageNumbersTemplate = value;
            }
        }


        /// <summary>
        /// Current page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerCurrentPageTemplate
        {
            get
            {
                return PagerControl.CurrentPageTemplate;
            }
            set
            {
                PagerControl.CurrentPageTemplate = value;
            }
        }


        /// <summary>
        /// Next group template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerNextGroupTemplate
        {
            get
            {
                return PagerControl.NextGroupTemplate;
            }
            set
            {
                PagerControl.NextGroupTemplate = value;
            }
        }


        /// <summary>
        /// Previous group template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerPreviousGroupTemplate
        {
            get
            {
                return PagerControl.PreviousGroupTemplate;
            }
            set
            {
                PagerControl.PreviousGroupTemplate = value;
            }
        }


        /// <summary>
        /// First page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerFirstPageTemplate
        {
            get
            {
                return PagerControl.FirstPageTemplate;
            }
            set
            {
                PagerControl.FirstPageTemplate = value;
            }
        }


        /// <summary>
        /// Last page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerLastPageTemplate
        {
            get
            {
                return PagerControl.LastPageTemplate;
            }
            set
            {
                PagerControl.LastPageTemplate = value;
            }
        }


        /// <summary>
        /// Next page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerNextPageTemplate
        {
            get
            {
                return PagerControl.NextPageTemplate;
            }
            set
            {
                PagerControl.NextPageTemplate = value;
            }
        }


        /// <summary>
        /// Previous page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerPreviousPageTemplate
        {
            get
            {
                return PagerControl.PreviousPageTemplate;
            }
            set
            {
                PagerControl.PreviousPageTemplate = value;
            }
        }


        /// <summary>
        /// Layout template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerLayoutTemplate
        {
            get
            {
                return PagerControl.LayoutTemplate;
            }
            set
            {
                PagerControl.LayoutTemplate = value;
            }
        }


        /// <summary>
        /// Page numbers separator template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerPageNumbersSeparatorTemplate
        {
            get
            {
                return PagerControl.PageNumbersSeparatorTemplate;
            }
            set
            {
                PagerControl.PageNumbersSeparatorTemplate = value;
            }
        }


        /// <summary>
        /// Direct page control template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PagerDirectPageTemplate
        {
            get
            {
                return PagerControl.DirectPageTemplate;
            }
            set
            {
                PagerControl.DirectPageTemplate = value;
            }
        }

        #endregion


        #region "UniPager Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether scroll position should be cleared after post back paging
        /// </summary>
        public bool ResetScrollPositionOnPostBack
        {
            get
            {
                return PagerControl.ResetScrollPositionOnPostBack;
            }
            set
            {
                PagerControl.ResetScrollPositionOnPostBack = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether paging is enabled.
        /// </summary>
        public bool EnablePaging
        {
            get
            {
                return PagerControl.Enabled;
            }
            set
            {
                PagerControl.Enabled = value;
            }
        }


        /// <summary>
        /// Gets the current unipager instance.
        /// </summary>
        public UniPager PagerControl
        {
            get
            {
                EnsureChildControls();

                return mUniPager;
            }
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Number of items per page.")]
        public int PageSize
        {
            get
            {
                return PagerControl.PageSize;
            }
            set
            {
                PagerControl.PageSize = value;
                mDataSource.PageSize = value;
            }
        }


        /// <summary>
        /// Pager position.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Paging"), DefaultValue(PagingPlaceTypeEnum.Bottom)]
        public PagingPlaceTypeEnum PagerPosition
        {
            get
            {
                if (ViewState["PagerPosition"] == null)
                {
                    return PagingPlaceTypeEnum.Bottom;
                }
                return (PagingPlaceTypeEnum)ViewState["PagerPosition"];
            }
            set
            {
                ViewState["PagerPosition"] = value;
            }
        }

        #endregion


        #region "HierarchicalTransformations"

        /// <summary>
        /// Transformation name for hierarchical transformation collection in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name for hieararchical transformation collection in format application.class.transformation.")]
        public string HierarchicalTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HierarchicalTransformationName"], String.Empty);
            }
            set
            {
                ViewState["HierarchicalTransformationName"] = value;
            }
        }

        #endregion


        #region "Hierarchical properties"

        /// <summary>
        /// Gets or sets the value that indicates whether default hierarchical order value should be used.
        /// The order is used only if LoadHierarchicalData is set to true.
        /// Default order value is "NodeLevel, NodeOrder". Value of OrderBy property is joined at the end of the order by expression
        /// </summary>
        public bool UseHierarchicalOrder
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UseHierarchicalOrdering"], true);
            }
            set
            {
                ViewState["UseHierarchicalOrdering"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indictes whether data should be binded in default format
        /// or changes to hierarchical grouped dataset
        /// </summary>
        public bool LoadHierarchicalData
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["LoadHierarchicalData"], false);
            }
            set
            {
                ViewState["LoadHierarchicalData"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the ID column.
        /// </summary>
        public string IDColumnName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["IDColumnName"], String.Empty);
            }
            set
            {
                ViewState["IDColumnName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the parent ID column.
        /// </summary>
        public string ParentIDColumnName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ParentIDColumnName"], String.Empty);
            }
            set
            {
                ViewState["ParentIDColumnName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the level column.
        /// </summary>
        public string LevelColumnName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["LevelColumnName"], String.Empty);
            }
            set
            {
                ViewState["LevelColumnName"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public QueryUniView()
        {
            if (Context == null)
            {
                return;
            }

            ParentControl = this;
        }


        /// <summary>
        /// Creates the control hierarchy that is used to render a composite data-bound control based on the values that are stored in view state.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Create the UniPager control but do not insert it into the controls collection yet. Pager will be inserted into the controls collection right after data binding.
            // This will ensure that the pager object won't have its ClientID overwritten.
            var pager = new UniPager();
            pager.ID = "uniViewPager";
            pager.PagedControl = this;
            pager.Page = Page;

            mUniPager = pager;
        }


        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected override void InitControl(bool loadPhase)
        {
            // Check for context (due to VS design mode)
            if (Context == null)
            {
                return;
            }

            // if control is in stop processing mode, disable default binding
            if (!DelayedLoading || loadPhase)
            {
                base.InitControl(loadPhase);
            }

            // Do not call base method for delayed loading
            // base.InitControl();
        }


        /// <summary>
        /// Loads data from the  according to the current values of properties.
        /// </summary>
        public override void ReloadData(bool forceReload)
        {
            if (StopProcessing)
            {
                return;
            }

            SetContext();

            RegisterHandlers();

            // If already loaded, exit
            if (mDataLoaded && !forceReload)
            {
                return;
            }

            mDataLoaded = true;

            NoData = false;

            // Load the data
            if ((DataSource == null) || forceReload)
            {
                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(mDataSource);
                }

                // Check whether 
                if (LoadHierarchicalData && (String.IsNullOrEmpty(ParentIDColumnName) || String.IsNullOrEmpty(LevelColumnName) || String.IsNullOrEmpty(IDColumnName)))
                {
                    throw new Exception("[QueryUniView.ReloadData] Hierarchical data cannot be loaded due to missing mandatory column names (ParentIDColumnName, LevelColumnName, IDColumnName).");
                }

                // Keep original order by value
                string originalOrderBy = OrderBy;

                // Check whether hierarchical ordering is required
                if (LoadHierarchicalData && UseHierarchicalOrder)
                {
                    OrderBy = LevelColumnName + "," + originalOrderBy;
                    OrderBy = OrderBy.Trim().TrimEnd(',');
                }

                // Load the data from DataSource control if set
                if ((forceReload || useDataSourceControl) && (DataSourceControl != null))
                {
                    DataSource = DataSourceControl.LoadData(forceReload);
                }

                // Check whether hierarchical data are required
                // and if so, create grouped data source and setup relation column
                if (LoadHierarchicalData)
                {
                    // Set original order by value
                    if (UseHierarchicalOrder)
                    {
                        OrderBy = originalOrderBy;
                    }

                    DataSource = new GroupedDataSource(DataSource, ParentIDColumnName, LevelColumnName);
                    RelationColumnID = IDColumnName;
                }
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                DataSource = new DataSet().Tables.Add();
                NoData = true;
            }

            if (DataBindByDefault || forceReload)
            {
                DataBind();
            }

            ReleaseContext();
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
            mDataSource.ClearCache();
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return mDataSource.GetDefaultCacheDependencies();
        }


        /// <summary>
        /// Registers event handlers and js scripts.
        /// </summary>
        private void RegisterHandlers()
        {
            if (!mHandlersRegistered)
            {
                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                mDataSource.OnPageChanged += OnPageChange;

              
                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }

                mHandlersRegistered = true;
            }
        }



        private void OnPageChange(object sender, EventArgs e)
        {
            // Clear already loaded data source
            if (DataSourceControl != null)
            {
                DataSourceControl.DataSource = null;
            }
            ReloadData(true);
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[QueryUniView: " + ID + "]");
                return;
            }

            bool isVisible = true;

            // Top pager 
            if (EnablePaging)
            {
                if (PagerPosition == PagingPlaceTypeEnum.Top || PagerPosition == PagingPlaceTypeEnum.TopAndBottom)
                {
                    PagerControl.RenderControl(writer);
                }

                isVisible = PagerControl.Visible;
                PagerControl.Visible = false;
            }

            base.Render(writer);

            // Bottom pager
            if (EnablePaging)
            {
                if (PagerPosition == PagingPlaceTypeEnum.Bottom || PagerPosition == PagingPlaceTypeEnum.TopAndBottom)
                {
                    if ((PagerDirectPageTemplate != null) && (PagerPosition == PagingPlaceTypeEnum.TopAndBottom) && (PagerControl.PagerMode == UniPagerMode.PostBack))
                    {
                        // Update the ID of the secondary pager control to ensure that the second pager does not render duplicated client ids.
                        Control directPageControl = ControlsHelper.GetChildControl(this, typeof(System.Web.UI.WebControls.TextBox), PagerControl.DirectPageControlID);
                        directPageControl.ID += "secondary";

                        // Make sure that all the pager scripts are updated with the new id.
                        PagerControl.ReloadData(true);
                    }

                    PagerControl.Visible = isVisible;
                    PagerControl.RenderControl(writer);
                }
            }
        }


        /// <summary>
        /// Loads ItemTemplates according to the current values of properties.
        /// </summary>
        private void LoadTemplates()
        {
            if (!String.IsNullOrEmpty(HierarchicalTransformationName))
            {
                TransformationInfo ti = TransformationInfoProvider.GetTransformation(HierarchicalTransformationName);
                if ((ti != null) && (ti.TransformationIsHierarchical))
                {
                    HierarchicalTransformations ht = new HierarchicalTransformations("ClassName");
                    ht.LoadFromXML(ti.TransformationHierarchicalXMLDocument);
                    ht.EditButtonsMode = EditButtonsMode;

                    Transformations = ht;
                    UseNearestItemForHeaderAndFooter = true;
                }
                else
                {
                    ItemTemplate = TransformationHelper.LoadTransformation(this, HierarchicalTransformationName, EditButtonsMode);
                }
            }
            else
            {
                ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName, EditButtonsMode);
            }

            if (!String.IsNullOrEmpty(AlternatingTransformationName))
            {
                AlternatingItemTemplate = TransformationHelper.LoadTransformation(this, AlternatingTransformationName, EditButtonsMode);
            }

            if (!String.IsNullOrEmpty(FirstTransformationName))
            {
                FirstItemTemplate = TransformationHelper.LoadTransformation(this, FirstTransformationName, EditButtonsMode);
            }

            if (!String.IsNullOrEmpty(LastTransformationName))
            {
                LastItemTemplate = TransformationHelper.LoadTransformation(this, LastTransformationName, EditButtonsMode);
            }

            if (!String.IsNullOrEmpty(SingleTransformationName))
            {
                SingleItemTemplate = TransformationHelper.LoadTransformation(this, SingleTransformationName, EditButtonsMode);
            }

            // Header
            if (!String.IsNullOrEmpty(HeaderTransformationName))
            {
                HeaderTemplate = TransformationHelper.LoadTransformation(this, HeaderTransformationName);
            }

            //Footer
            if (!String.IsNullOrEmpty(FooterTransformationName))
            {
                FooterTemplate = TransformationHelper.LoadTransformation(this, FooterTransformationName);
            }

            // Separator
            if (!String.IsNullOrEmpty(ItemSeparatorValue))
            {
                SeparatorTemplate = new GeneralTemplateClass(ItemSeparatorValue);
            }
            else if (!String.IsNullOrEmpty(SeparatorTransformationName))
            {
                SeparatorTemplate = TransformationHelper.LoadTransformation(this, SeparatorTransformationName);
            }
        }


        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            // Load item templates
            LoadTemplates();

            base.DataBind();

            // Pager control has not been inserted into the controls collection yet, because DataBind would increase numbering of its ClientID (ct100 -> ct102 etc...).
            // Insert the pager at the first position for better performance when searching for the pager control.
            Controls.AddAt(0, PagerControl);

            if (PagerControl != null)
            {
                // Make sure that all the pager scripts are generated with the correct client id.
                PagerControl.RebindPager();
            }
        }

        #endregion


        #region "Context methods"

        /// <summary>
        /// Sets the web part context.
        /// </summary>
        public virtual void SetContext()
        {
            mDataSource.SetContext();
        }


        /// <summary>
        /// Releases the web part context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            mDataSource.ReleaseContext();
        }

        #endregion
    }
}

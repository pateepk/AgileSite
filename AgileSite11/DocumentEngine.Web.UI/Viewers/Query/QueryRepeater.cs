using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// QueryRepeater control.
    /// </summary>
    [ToolboxData("<{0}:QueryRepeater runat=server></{0}:QueryRepeater>")]
    [ParseChildren(true)]
    [PersistChildren(true)]
    public class QueryRepeater : BasicRepeater, ICMSQueryProperties
    {
        #region "Variables"

        /// <summary>
        /// Query data source.
        /// </summary>
        protected CMSQueryDataSource mDataSource = new CMSQueryDataSource();


        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;


        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;


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
        protected bool NoData = false;


        /// <summary>
        /// ItemSeparator variable.
        /// </summary>
        protected string mItemSeparator = "";


        /// <summary>
        /// Indicates if pagesize property is set.
        /// </summary>
        protected bool mPageSizeSet = false;


        /// <summary>
        /// Fake sitename due to interface ICMSBaseProperties.
        /// </summary>
        private string mSiteName = "";


        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool handlersRegistered;

        /// <summary>
        /// Stores the index (position) of a pager in the controls collection.
        /// </summary>
        private int pagerPosition = -1;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Control with data source.
        /// </summary>
        [Browsable(false)]
        public override CMSBaseDataSource DataSourceControl
        {
            get
            {
                return mDataSource;
            }
        }


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
                else if (ShowEditButton)
                {
                    return EditModeButtonEnum.Edit;
                }
                else if (ShowDeleteButton)
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
        /// Item separator between displayed records.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Item separator between displayed records.")]
        public string ItemSeparator
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
        /// Number of items per page.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Number of items per page.")]
        public int PageSize
        {
            get
            {
                return mDataSource.PageSize;
            }
            set
            {
                mDataSource.PageSize = value;
                mPageSizeSet = true;
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
        /// If true, each page is loaded individually in case of paging.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("If true, each page is loaded individually in case of paging")]
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
        /// Gets or sets ItemTemplate for selected item.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation applied to selected item.")]
        public string SelectedItemTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SelectedItemTransformationName"], "");
            }
            set
            {
                ViewState["SelectedItemTransformationName"] = value;
            }
        }

        #endregion


        #region "Paging properties"

        /// <summary>
        /// Enables paging a shows the DataPager control.
        /// </summary>
        [Category("Data Pager"), DefaultValue(false)]
        public bool EnablePaging
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EnablePaging"], false);
            }
            set
            {
                ViewState["EnablePaging"] = value;
                if (value)
                {
                    AddPager();
                }
            }
        }


        /// <summary>
        /// DataPager control that ensured paging.
        /// </summary>
        [Category("Data Pager"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.Attribute), NotifyParentProperty(true), RefreshProperties(RefreshProperties.Repaint)]
        public DataPager PagerControl
        {
            get
            {
                if (mDataPager == null)
                {
                    mDataPager = new DataPager();
                    mDataPager.ID = "pager";
                    mDataPager.EnableViewState = false;

                    if ((Context != null) && EnablePaging)
                    {
                        AddPager();
                    }
                }
                if (Page != null)
                {
                    mDataPager.Page = Page;
                }

                return mDataPager;
            }
            set
            {
                mDataPager = value;

                // Handle external page change event
                if (mDataPager != null)
                {
                    mDataPager.OnPageChange += DataPager_OnPageChange;
                }
                mExternalPager = true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the pager to the control collection.
        /// </summary>
        private void AddPager()
        {
            if (mDataPager == null)
            {
                mDataPager = PagerControl;
            }

            if (pagerPosition < 0)
            {
                Controls.AddAt(0, mDataPager);
                pagerPosition = 0;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public QueryRepeater()
        {
            if (Context == null)
            {
                return;
            }
            ParentControl = this;
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
            base.InitControl(loadPhase);
        }


        private void DataPager_OnPageChange(object sender, EventArgs e)
        {
            ReloadData(true);
        }


        /// <summary>
        /// Loads data according to the current values of properties.
        /// </summary>
        /// <param name="forceReload">Force the reload</param>
        public override void ReloadData(bool forceReload)
        {
            if (!StopProcessing)
            {
                SetContext();

                // if force reload is required, register handlers due to partial caching
                if (forceReload)
                {
                    handlersRegistered = false;
                }

                RegisterHandlers();

                // If already loaded, exit
                if (mDataLoaded && !forceReload)
                {
                    return;
                }

                mDataLoaded = true;
                NoData = false;

                // Load Separator text
                if (!String.IsNullOrEmpty(ItemSeparator))
                {
                    base.SeparatorTemplate = new GeneralTemplateClass(ItemSeparator);
                }


                // Load the data
                if ((DataSource == null) || forceReload)
                {
                    if (FilterControl != null)
                    {
                        FilterControl.InitDataProperties(mDataSource);
                    }

                    DataSource = mDataSource.LoadData(forceReload);
                }

                if (DataHelper.DataSourceIsEmpty(DataSource))
                {
                    DataSource = new DataSet().Tables.Add();
                    NoData = true;
                }

                if (DataBindByDefault || forceReload)
                {
                    // If paging enabled and there is not already processed data source by external pager, process the page
                    if (EnablePaging && !DataHelper.DataSourceIsEmpty(DataSource) && !(DataSource is PagedDataSource))
                    {
                        if (mPageSizeSet)
                        {
                            PagerControl.PageSize = mDataSource.PageSize;
                        }
                        PagerControl.DataSource = DataSource;
                        DataSource = PagerControl.PagedData;
                    }

                    DataBind();

                    base.ReloadData(forceReload);
                }

                ReleaseContext();
            }
        }


        /// <summary>
        /// Loads ItemTemplate according to the current values of properties.
        /// </summary>
        private void LoadItemTemplate()
        {
            if (IsSelected && !String.IsNullOrEmpty(SelectedItemTransformationName))
            {
                ItemTemplate = TransformationHelper.LoadTransformation(this, SelectedItemTransformationName);
            }
            else
            {
                if (!String.IsNullOrEmpty(TransformationName))
                {
                    ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName, EditButtonsMode);
                }

                if (!String.IsNullOrEmpty(AlternatingTransformationName))
                {
                    AlternatingItemTemplate = TransformationHelper.LoadTransformation(this, AlternatingTransformationName, EditButtonsMode);
                }
            }
        }


        /// <summary>
        /// Databind event.
        /// </summary>
        public override void DataBind()
        {
            // Load the template
            LoadItemTemplate();

            base.DataBind();

            // Add pager control
            if (!mExternalPager && EnablePaging)
            {
                if (Context != null)
                {
                    pagerPosition = -1;
                    AddPager();
                }
            }
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[ QueryRepeater : " + ClientID + " ]");
                return;
            }

            if (!StopProcessing)
            {
                if (!NoData)
                {
                    // Top DataPager 
                    if (EnablePaging && (PagerControl.PagerPosition == PagingPlaceTypeEnum.Top || PagerControl.PagerPosition == PagingPlaceTypeEnum.TopAndBottom) && !mExternalPager)
                    {
                        PagerControl.RenderControl(output);
                    }

                    bool isVisible = false;
                    if (EnablePaging)
                    {
                        isVisible = PagerControl.Visible;
                        PagerControl.Visible = false;
                    }

                    // Remove the pager from the controls collection because it is rendered manually.
                    if (pagerPosition >= 0)
                    {
                        Controls.RemoveAt(pagerPosition);
                    }

                    base.Render(output);

                    if (EnablePaging)
                    {
                        PagerControl.Visible = isVisible;
                    }

                    // Down DataPager
                    if (EnablePaging && (PagerControl.PagerPosition == PagingPlaceTypeEnum.Bottom || PagerControl.PagerPosition == PagingPlaceTypeEnum.TopAndBottom) && !mExternalPager)
                    {
                        PagerControl.RenderControl(output);
                    }
                }
                else
                {
                    if (EnablePaging)
                    {
                        PagerControl.Visible = false;
                    }
                    base.Render(output);
                }
            }
        }


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
            if (!handlersRegistered)
            {
                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                mDataSource.OnPageChanged += DataPager_OnPageChange;

                if (EnablePaging)
                {
                    PagerControl.OnPageChange += DataPager_OnPageChange;
                }

                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }

                handlersRegistered = true;
            }
        }

        #endregion
    }
}

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;
using System.Data;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSDataGrid control.
    /// </summary>
    [ToolboxData("<{0}:CMSDataGrid runat=server></{0}:CMSDataGrid>")]
    public class CMSDataGrid : BasicDataGrid, ICMSDataProperties
    {
        #region "Variables"

        private readonly CMSRepeater mRepeater = new CMSRepeater();

        /// <summary>
        /// Properties object to set DataProperties.
        /// </summary>
        public CMSDataProperties mProperties = new CMSDataProperties();

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool mAreEventHandlersRegistered;

        /// <summary>
        /// Gets or sets a value that indicates whether <see cref="T:System.Web.UI.WebControls.BoundColumn"/> objects are automatically created and displayed in the <see cref="T:System.Web.UI.WebControls.DataGrid"/> control for each field in the data source.
        /// </summary>
        private bool? mAutoGenerateColumns;

        #endregion


        #region "Public Properties"

        /// <summary>
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>  
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the columns to be retrieved from database.")]
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
        ///</summary>
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
        /// Property to set and get the SiteName.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the SiteName.")]
        public virtual string SiteName
        {
            get
            {
                return mProperties.SiteName;
            }
            set
            {
                mProperties.SiteName = value;
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
        /// Property to set and get the SelectOnlyPublished flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if only published documents should be displayed.")]
        public virtual bool SelectOnlyPublished
        {
            get
            {
                return mProperties.SelectOnlyPublished;
            }
            set
            {
                mProperties.SelectOnlyPublished = value;
            }
        }


        /// <summary>
        /// If true, the returned nodes are on the right side of the relationship.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If true, the returned nodes are on the right side of the relationship.")]
        public bool RelatedNodeIsOnTheLeftSide
        {
            get
            {
                return mProperties.RelatedNodeIsOnTheLeftSide;
            }
            set
            {
                mProperties.RelatedNodeIsOnTheLeftSide = value;
            }
        }


        /// <summary>
        /// Name of the relationship.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Name of the relationship.")]
        public string RelationshipName
        {
            get
            {
                return mProperties.RelationshipName;
            }
            set
            {
                mProperties.RelationshipName = value;
            }
        }


        /// <summary>
        /// Select nodes with given relationship with given node.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select nodes with given relationship with given node.")]
        public Guid RelationshipWithNodeGuid
        {
            get
            {
                return mProperties.RelationshipWithNodeGuid;
            }
            set
            {
                mProperties.RelationshipWithNodeGuid = value;
            }
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.")]
        public virtual bool CheckPermissions
        {
            get
            {
                return mProperties.CheckPermissions;
            }
            set
            {
                mProperties.CheckPermissions = value;
            }
        }


        /// <summary>
        /// Property to set and get the CultureCode.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Code of the preferred culture (en-us, fr-fr, etc.). If it's not specified, it is read from the CMSPreferredCulture session variable and then from the CMSDefaultCultureCode configuration key.")]
        public virtual string CultureCode
        {
            get
            {
                return mProperties.CultureCode;
            }
            set
            {
                mProperties.CultureCode = value;
            }
        }


        /// <summary>
        /// Property to set and get the CombineWithDefaultCulture flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the results should be combined with default language versions in case the translated version is not available.")]
        public virtual bool CombineWithDefaultCulture
        {
            get
            {
                return mProperties.CombineWithDefaultCulture;
            }
            set
            {
                mProperties.CombineWithDefaultCulture = value;
            }
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the duplicated (linked) items should be filtered out from the data.")]
        public virtual bool FilterOutDuplicates
        {
            get
            {
                return mProperties.FilterOutDuplicates;
            }
            set
            {
                mProperties.FilterOutDuplicates = value;
            }
        }


        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Path to the menu items that should be displayed in the site map.")]
        public virtual string Path
        {
            get
            {
                return mProperties.Path;
            }
            set
            {
                mProperties.Path = value;
            }
        }


        /// <summary>
        /// Property to set and get the classnames list (separated by the semicolon).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the classnames list (separated by the semicolon).")]
        public virtual string ClassNames
        {
            get
            {
                return mProperties.ClassNames;
            }
            set
            {
                mProperties.ClassNames = value;
            }
        }


        /// <summary>
        /// Property to set and get the category name for filtering documents.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the category name for filtering documents.")]
        public virtual string CategoryName
        {
            get
            {
                return mProperties.CategoryName;
            }
            set
            {
                mProperties.CategoryName = value;
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
        /// Property to set and get the MaxRelativeLevel.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1), Description("Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence.")]
        public virtual int MaxRelativeLevel
        {
            get
            {
                return mProperties.MaxRelativeLevel;
            }
            set
            {
                mProperties.MaxRelativeLevel = value;
            }
        }


        /// <summary>
        /// Transformation name for selected item in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name for selected item in format application.class.transformation.")]
        public string SelectedItemTransformationName
        {
            get
            {
                return mProperties.SelectedItemTransformationName;
            }
            set
            {
                mProperties.SelectedItemTransformationName = value;
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
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return mProperties.TreeProvider;
            }
            set
            {
                mProperties.TreeProvider = value;
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
                mRepeater.ControlContext = value;
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
        /// Gets or sets a value that indicates whether <see cref="T:System.Web.UI.WebControls.BoundColumn"/> objects are automatically created and displayed in the <see cref="T:System.Web.UI.WebControls.DataGrid"/> control for each field in the data source.
        /// </summary>
        public override bool AutoGenerateColumns
        {
            get
            {
                return base.AutoGenerateColumns;
            }
            set
            {
                mAutoGenerateColumns = value;
                base.AutoGenerateColumns = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSDataGrid()
        {
            mProperties.ParentControl = this;

            if (Context != null)
            {
                ProcessSorting = false;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="treeProvider">Tree provider instance to be used for retrieving data</param>
        public CMSDataGrid(TreeProvider treeProvider)
        {
            mProperties.ParentControl = this;

            if (Context != null)
            {
                mProperties.TreeProvider = treeProvider;
                ProcessSorting = false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnPreRender event handler. It automatically selects and binds data unless DataBindByDefault is false.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (!StopProcessing)
            {
                if (Context != null)
                {
                    BuildFormat();
                }
                base.OnPreRender(e);
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

            if (!StopProcessing)
            {
                RegisterHandlers();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// OnPreRender event handler. It automatically selects and binds data unless DataBindByDefault is false.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                RegisterHandlers();
                base.OnLoad(e);
            }
        }


        /// <summary>
        /// OnSortCommand event handler. It sorts data by selected column.
        /// </summary>
        protected override void OnSortCommand(DataGridSortCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                DataSource = null;
                BuildFormat();
                base.OnSortCommand(e);
            }
        }


        /// <summary>
        /// OnPageIndexChanged event handler. It ensures displaying of the given page.
        /// </summary>
        protected override void OnPageIndexChanged(DataGridPageChangedEventArgs e)
        {
            if (!StopProcessing)
            {
                BuildFormat();
                base.OnPageIndexChanged(e);
            }
        }


        /// <summary>
        /// Render CMSDataGrid.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(mRepeater.TransformationName))
            {
                mRepeater.RenderControl(writer);
                return;
            }
            base.Render(writer);
        }


        /// <summary>
        /// Builds the control format.
        /// </summary>
        private void BuildFormat()
        {
            SetContext();

            bool isSelected = false;
            if (!String.IsNullOrEmpty(SelectedItemTransformationName))
            {
                isSelected = mProperties.IsSelected;
            }

            if (isSelected)
            {
                mRepeater.TransformationName = SelectedItemTransformationName;
                mRepeater.Path = mProperties.SelectedItemPath;
                mRepeater.EnablePaging = false;
                mRepeater.CacheItemName = CacheItemName;
                mRepeater.CacheMinutes = CacheMinutes;
                mRepeater.CheckPermissions = CheckPermissions;
                mRepeater.ClassNames = mProperties.SelectedItemClass;
                mRepeater.CategoryName = CategoryName;
                mRepeater.WhereCondition = WhereCondition;
                mRepeater.MaxRelativeLevel = MaxRelativeLevel;
                mRepeater.CultureCode = CultureCode;
                mRepeater.RelationshipWithNodeGuid = RelationshipWithNodeGuid;
                mRepeater.RelationshipName = RelationshipName;
                mRepeater.RelatedNodeIsOnTheLeftSide = RelatedNodeIsOnTheLeftSide;

                Controls.Add(mRepeater);
            }
            // Standard load process
            else
            {
                LoadData(false);
                base.PrepareControlHierarchy();
            }

            ReleaseContext();
        }


        /// <summary>
        /// Prepare control hierarchy.
        /// </summary>
        protected override void PrepareControlHierarchy()
        {
            if (Context == null)
            {
                base.PrepareControlHierarchy();
            }
        }


        /// <summary>
        /// Reloads the control data.
        /// </summary>
        public void ReloadData(bool forceReload)
        {
            if (!StopProcessing)
            {
                RegisterHandlers();
                LoadData(forceReload);
            }
        }


        /// <summary>
        /// Loads data from the database according to the current values of properties.
        /// </summary>
        private void LoadData(bool forceReload)
        {
            SetContext();

            ProcessSorting = true;

            if ((DataSource == null) || forceReload)
            {
                if (String.IsNullOrEmpty(Path))
                {
                    return;
                }

                // Set default sort field to Order by if not set
                if (String.IsNullOrEmpty(SortField) && !String.IsNullOrEmpty(OrderBy))
                {
                    //SortAscending = !SortAscending;
                    SortField = OrderBy;
                }

                // Get the data
                object ds = DataSource;
                mProperties.LoadData(ref ds, forceReload);
                DataSource = ds;

                // Bind the data
                if (DataBindByDefault || forceReload)
                {
                    base.DataBind();
                }
            }

            ReleaseContext();
        }


        /// <summary>
        /// Retrieves DataSet.
        /// </summary>
        protected virtual DataSet GetDataSet()
        {
            // Get the data
            return mProperties.GetDataSet(Path);
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
            if (mAutoGenerateColumns == null)
            {
                // Enable AutoGenerateColumns only when not set explicitly 
                base.AutoGenerateColumns = true;
            }

            // Set forcibly parent visibility
            if (Parent != null)
            {
                Parent.Visible = true;
            }

            FilterControl.InitDataProperties(mProperties);
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
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            return mProperties.GetCacheDependency();
        }


        /// <summary>
        /// Registers event handlers and javascripts.
        /// </summary>
        private void RegisterHandlers()
        {
            if (!mAreEventHandlersRegistered)
            {
                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }

                mAreEventHandlersRegistered = true;
            }
        }

        #endregion
    }
}

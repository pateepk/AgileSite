using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Generic;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Repeater control that can be bounded to the CMS content.
    /// </summary>
    [ToolboxData("<{0}:CMSRepeater runat=server></{0}:CMSRepeater>")]
    [ParseChildren(true)]
    [PersistChildren(true)]
    public class CMSRepeater : BasicRepeater, ICMSDataProperties, IStopProcessing
    {
        #region "Variables"

        /// <summary>
        /// DataPager variable.
        /// </summary>
        protected DataPager mDataPager = null;

        /// <summary>
        /// External pager control?
        /// </summary>
        protected bool mExternalPager = false;

        /// <summary> 
        /// When DataSource is empty NoData  = true
        /// </summary>
        protected bool NoData = false;

        /// <summary>
        /// Item separator code.
        /// </summary>
        protected string mItemSeparator = "";

        /// <summary>
        /// Document data source.
        /// </summary>
        protected CMSDocumentsDataSource mDataSource = new CMSDocumentsDataSource { LoadCurrentPageOnly = false };


        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Indicates if page size property is set.
        /// </summary>
        protected bool mPageSizeSet = false;


        /// <summary>
        /// If true OnPageChange event handler set.
        /// </summary>
        private bool pageEventSet;


        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool handlersRegistered;


        /// <summary>
        /// Indicates whether pager was added to the controls collection.
        /// </summary>
        private bool pagerAdded;

        #endregion


        #region "Public properties"

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
        /// Nested controls ID, use ';' like separator.
        /// </summary>
        public string NestedControlsID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["NestedControlsID"], "");
            }
            set
            {
                ViewState["NestedControlsID"] = value;
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
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>  
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the columns to be retrieved from database.")]
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
        /// Property to set and get the SelectOnlyPublished flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if only published documents should be displayed.")]
        public virtual bool SelectOnlyPublished
        {
            get
            {
                return mDataSource.SelectOnlyPublished;
            }
            set
            {
                mDataSource.SelectOnlyPublished = value;
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
                return mDataSource.RelatedNodeIsOnTheLeftSide;
            }
            set
            {
                mDataSource.RelatedNodeIsOnTheLeftSide = value;
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
                return mDataSource.RelationshipName;
            }
            set
            {
                mDataSource.RelationshipName = value;
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
                return mDataSource.RelationshipWithNodeGuid;
            }
            set
            {
                mDataSource.RelationshipWithNodeGuid = value;
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
                return mDataSource.CheckPermissions;
            }
            set
            {
                mDataSource.CheckPermissions = value;
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
                return mDataSource.CultureCode;
            }
            set
            {
                mDataSource.CultureCode = value;
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
                return mDataSource.CombineWithDefaultCulture;
            }
            set
            {
                mDataSource.CombineWithDefaultCulture = value;
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
                return mDataSource.FilterOutDuplicates;
            }
            set
            {
                mDataSource.FilterOutDuplicates = value;
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
                return mDataSource.SiteName;
            }
            set
            {
                mDataSource.SiteName = value;
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
                return mDataSource.Path;
            }

            set
            {
                mDataSource.Path = value;
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
                return mDataSource.ClassNames;
            }
            set
            {
                mDataSource.ClassNames = value;
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
                return mDataSource.CategoryName;
            }
            set
            {
                mDataSource.CategoryName = value;
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
        /// Property to set and get the MaxRelativeLevel.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1), Description("Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence.")]
        public virtual int MaxRelativeLevel
        {
            get
            {
                return mDataSource.MaxRelativeLevel;
            }
            set
            {
                mDataSource.MaxRelativeLevel = value;
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
                return mDataSource.SelectedItemTransformationName;
            }
            set
            {
                mDataSource.SelectedItemTransformationName = value;
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
                return mDataSource.TreeProvider;
            }
            set
            {
                mDataSource.TreeProvider = value;
            }
        }


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
        /// Gets a value indicating whether the current item is a selected item.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                // Use data source property to reflect current settings
                return DataSourceControl.IsSelected;
            }
            private set
            {
                DataSourceControl.IsSelected = value;
            }
        }

        #endregion


        #region "Paging properties"

        ///<summary>Enables paging on Data control</summary>
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
                    if (!pageEventSet)
                    {
                        pageEventSet = true;
                        PagerControl.OnPageChange += DataPager_OnPageChange;
                    }
                }
            }
        }


        ///<summary>Represents pager control.</summary>
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

                    if (Context != null)
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
                // handle external page change event
                if (mDataPager != null)
                {
                    mDataPager.OnPageChange += DataPager_OnPageChange;
                }
                mExternalPager = true;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSRepeater()
        {
            if (Context == null)
            {
                return;
            }

            ParentControl = this;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="treeProvider">Tree provider instance to be used for retrieving data</param>
        public CMSRepeater(TreeProvider treeProvider)
        {
            if (Context == null)
            {
                return;
            }

            ParentControl = this;
            TreeProvider = treeProvider;
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

            if (!pagerAdded)
            {
                Controls.AddAt(0, mDataPager);
                pagerAdded = true;
            }
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
        /// Handles OnFilterChanged event.
        /// </summary>
        private void DataSourceControl_OnFilterChanged()
        {
            ReloadData(true);
        }


        /// <summary>
        /// On item databound using for Nested repeater.
        /// </summary>
        private void CMSRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Bind the container
            if (e.Item.Controls.Count > 0)
            {
                Control itemControl = e.Item.Controls[0];

                List<string> controls = null;

                // Get controls IDs
                if (NestedControlsID != null && NestedControlsID.Trim() != "")
                {
                    // Create array list
                    controls = new List<string>();
                    controls.AddRange(NestedControlsID.Split(';'));
                }

                // Check whether at least one control is selected
                if (controls != null)
                {
                    // Loop through all controls
                    foreach (string controlId in controls)
                    {
                        // Get IDataControl
                        ICMSControlProperties ctrl = itemControl.FindControl(controlId) as ICMSControlProperties;

                        // Check whether control exists
                        if (ctrl != null)
                        {
                            // Get current alias path
                            if (ValidationHelper.GetString(((DataRowView)e.Item.DataItem).Row["NodeAliasPath"], "") != "")
                            {
                                // Set alias path
                                ctrl.Path = ValidationHelper.GetString(((DataRowView)e.Item.DataItem).Row["NodeAliasPath"], "");

                                // Set path to select all sub items
                                if (!ctrl.Path.EndsWithCSafe("/%"))
                                {
                                    ctrl.Path += "/%";
                                }

                                // Reload control data
                                ctrl.ReloadData(true);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// OnPage change event handler.
        /// </summary>
        private void DataPager_OnPageChange(object sender, EventArgs e)
        {
            ReloadData(true);
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
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[CMSRepeater: " + ID + "]");
                return;
            }

            bool isVisible = true;

            if (!StopProcessing)
            {
                if (!NoData)
                {
                    // Top DataPager 
                    if (EnablePaging && (PagerControl.PagerPosition == PagingPlaceTypeEnum.Top || PagerControl.PagerPosition == PagingPlaceTypeEnum.TopAndBottom) && !mExternalPager)
                    {
                        PagerControl.RenderControl(output);
                    }

                    if (EnablePaging)
                    {
                        isVisible = PagerControl.Visible;
                        PagerControl.Visible = false;
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
        /// Loads ItemTemplate according to the current values of properties.
        /// </summary>
        private void LoadItemTemplate()
        {
            if (TransformationName != "")
            {
                ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName, EditButtonsMode);
            }

            if (AlternatingTransformationName != "")
            {
                AlternatingItemTemplate = TransformationHelper.LoadTransformation(this, AlternatingTransformationName, EditButtonsMode);
            }
        }


        /// <summary>
        /// Loads data from the  according to the current values of properties.
        /// </summary>
        public override void ReloadData(bool forceReload)
        {
            this.CallHandled(() => ReloadDataInternal(forceReload));
        }


        /// <summary>
        /// Loads data from the  according to the current values of properties.
        /// </summary>
        protected void ReloadDataInternal(bool forceReload)
        {
            if (StopProcessing)
            {
                return;
            }

            RegisterHandlers();

            if (FilterControl != null)
            {
                FilterControl.InitDataProperties(mDataSource);
            }

            // Init DataSourceControl
            if (DataSourceControl != null)
            {
                DataSourceControl.InitDataProperties(mDataSource);

                // Check if DataSourceControl FilterName is set
                if (String.IsNullOrEmpty(DataSourceControl.FilterName))
                {
                    DataSourceControl.FilterName = ID;
                }
            }

            // If already loaded, exit
            if (mDataLoaded && !forceReload)
            {
                return;
            }

            SetContext();

            mDataLoaded = true;
            NoData = false;

            // Load Separator text
            if (!String.IsNullOrEmpty(ItemSeparator))
            {
                base.SeparatorTemplate = new GeneralTemplateClass(ItemSeparator);
            }

            // Indicates whether selected item is used
            bool isSelected = false;
            if (!String.IsNullOrEmpty(SelectedItemTransformationName))
            {
                isSelected = (DataSourceControl != null) && DataSourceControl.IsSelected;
                IsSelected = isSelected;
            }

            // Keep original class name
            string originalClassNames = ClassNames;

            // Set detail transformation if in detail mode
            if (isSelected)
            {
                EnablePaging = false;
                TransformationName = SelectedItemTransformationName;
                mDataSource.Properties.PathInternal = mDataSource.Properties.SelectedItemPath;

                // Set class names to current document only
                if (!String.IsNullOrEmpty(mDataSource.Properties.SelectedItemClass))
                {
                    ClassNames = mDataSource.Properties.SelectedItemClass;
                }
            }

            // Load the data from DataSource control
            if ((forceReload || useDataSourceControl) && (DataSourceControl != null))
            {
                DataSource = DataSourceControl.LoadData(forceReload);
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                DataSource = new DataSet().Tables.Add();
                NoData = true;
            }

            // Restore class names
            ClassNames = originalClassNames;

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
            }

            ReleaseContext();
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public virtual void ClearCache()
        {
            mDataSource.ClearCache();
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
            if (EnablePaging && !mExternalPager)
            {
                if (Context != null)
                {
                    pagerAdded = false;
                    AddPager();
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
                ItemDataBound += CMSRepeater_ItemDataBound;

                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                if ((EnablePaging) && (!pageEventSet))
                {
                    pageEventSet = true;
                    PagerControl.OnPageChange += DataPager_OnPageChange;
                }

                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }

                if (DataSourceControl != null)
                {
                    DataSourceControl.OnFilterChanged += DataSourceControl_OnFilterChanged;
                    if (DataSourceControl.UniPagerControl != null)
                    {
                        DataSourceControl.OnPageChanged += DataPager_OnPageChange;
                    }
                }

                handlersRegistered = true;
            }
        }

        #endregion
    }


    #region "Separator template"

    /// <summary>
    /// ITemplate class which convert selected code to the ITemplate.
    /// </summary>
    public class GeneralTemplateClass : ITemplate
    {
        private readonly string mHtml;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Template content</param>
        public GeneralTemplateClass(string text)
        {
            mHtml = text;
        }


        #region "ITemplate Members"

        /// <summary>
        /// InstantiateIn.
        /// </summary>
        /// <param name="container">Container control</param>
        public void InstantiateIn(Control container)
        {
            if (!String.IsNullOrEmpty(mHtml))
            {
                LiteralControl lit = new LiteralControl(mHtml);
                container.Controls.Add(lit);
            }
        }

        #endregion
    }

    #endregion
}

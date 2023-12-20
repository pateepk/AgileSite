using System;
using System.ComponentModel;
using System.Web.UI;
using System.Data;
using System.Web.UI.Design;
using System.Collections.Generic;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// UniView control that can be bounded to the CMS content.
    /// </summary>
    [ToolboxData("<{0}:CMSUniView runat=server></{0}:CMSUniView>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSUniView : BasicUniView, ICMSDataProperties
    {
        #region "Variables"

        /// <summary>
        /// Document data source.
        /// </summary>
        protected CMSDocumentsDataSource mDataSource = new CMSDocumentsDataSource { LoadPagesIndividually = true, LoadCurrentPageOnly = false };

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;


        /// <summary>
        /// Indicates whether event handlers are registered.
        /// </summary>
        private bool mHandlersRegistered;

        /// <summary>
        /// Item separator code.
        /// </summary>
        protected string mItemSeparator = String.Empty;

        /// <summary>
        /// Header item  code.
        /// </summary>
        protected string mHeaderItem = String.Empty;

        /// <summary>
        /// Footer item code.
        /// </summary>
        protected string mFooterItem = String.Empty;

        /// <summary>
        /// Current uni pager.
        /// </summary>
        protected UniPager mUniPager = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether data should be binded in default format
        /// or changed to hierarchical grouped dataset
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
                return ValidationHelper.GetString(ViewState["OrderBy"], "");
            }
            set
            {
                ViewState["OrderBy"] = value;
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
                return mDataSource.IsSelected;
            }
            private set
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
                return ValidationHelper.GetString(ViewState["TransformationName"], String.Empty);
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
                return ValidationHelper.GetString(ViewState["AlternatingTransformationName"], String.Empty);
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
        /// Transformation name for selected header item in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name for selected header item in format application.class.transformation.")]
        public string SelectedHeaderItemTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SelectedHeaderItemTransformationName"], String.Empty);
            }
            set
            {
                ViewState["SelectedHeaderItemTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name for selected footeritem in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name for selected footer item in format application.class.transformation.")]
        public string SelectedFootertemTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SelectedFootertemTransformationName"], String.Empty);
            }
            set
            {
                ViewState["SelectedFootertemTransformationName"] = value;
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

        #endregion


        #region "HierarchicalTransformations"

        /// <summary>
        /// Transformation name for hierarchical transformation collection in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name for hierarchical transformation collection in format application.class.transformation.")]
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


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSUniView()
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
        public CMSUniView(TreeProvider treeProvider)
        {
            if (Context == null)
            {
                return;
            }

            ParentControl = this;
            TreeProvider = treeProvider;
        }

        #endregion


        #region "Data methods"

        /// <summary>
        /// Loads data from the  according to the current values of properties.
        /// </summary>
        public override void ReloadData(bool forceReload)
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

            // Indicates whether selected item is used
            bool isSelected = false;
            if (!String.IsNullOrEmpty(SelectedItemTransformationName))
            {
                isSelected = mDataSource.IsSelected;
                IsSelected = isSelected;
            }

            // Keep original class name
            string originalClassNames = ClassNames;

            // Set detail transformation if in detail mode
            if (isSelected)
            {
                EnablePaging = false;
                HideHeaderAndFooterForSingleItem = false;
                // For selected item is not required single item template and header/footer hiding
                SingleTransformationName = String.Empty;
                
                TransformationName = SelectedItemTransformationName;
                HeaderTransformationName = SelectedHeaderItemTransformationName;
                FooterTransformationName = SelectedFootertemTransformationName;

                mDataSource.Properties.PathInternal = mDataSource.Properties.SelectedItemPath;

                // If selected item transformation is set => Disable hierarchical transformations
                HierarchicalTransformationName = String.Empty;

                // Set class names to current document only
                if (!String.IsNullOrEmpty(mDataSource.Properties.SelectedItemClass))
                {
                    ClassNames = mDataSource.Properties.SelectedItemClass;
                }
            }

            mDataSource.OrderBy = OrderBy;

            // Check whether hierarchical ordering is required
            if (LoadHierarchicalData && UseHierarchicalOrder)
            {
                string hierarchicalOrderBy = "NodeLevel, NodeOrder," + mDataSource.OrderBy;
                hierarchicalOrderBy = hierarchicalOrderBy.Trim().TrimEnd(',');
                mDataSource.OrderBy = hierarchicalOrderBy;
            }

            // Load the data from DataSource control if set
            if ((forceReload || useDataSourceControl) && (DataSourceControl != null))
            {
                DataSource = DataSourceControl.LoadData(forceReload);
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                DataSource = new DataSet().Tables.Add();
            }
            // Check whether hierarchical data are required
            // and if so, create grouped data source and setup relation column
            else if (LoadHierarchicalData)
            {
                DataSource = new GroupedDataSource(DataSource, "NodeParentID", "NodeLevel");
                RelationColumnID = "NodeID";
            }

            // Restore class names
            ClassNames = originalClassNames;

            if (DataBindByDefault || forceReload)
            {
                DataBind();
            }

            ReleaseContext();
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

                    // Set selected values
                    PageInfo pi = DocumentContext.CurrentPageInfo;
                    if (pi != null)
                    {
                        SelectedItemColumnName = "DocumentID";
                        SelectedItemValue = pi.DocumentID;
                    }
                }
                else
                {
                    ItemTemplate = TransformationHelper.LoadTransformation(this, HierarchicalTransformationName, EditButtonsMode);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(TransformationName))
                {
                    ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName, EditButtonsMode);
                }
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
        [Browsable(false)]
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


        #region "General methods"

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
        /// Registers event handlers and js scripts.
        /// </summary>
        private void RegisterHandlers()
        {
            if (!mHandlersRegistered)
            {
                OnItemDataBound += CMSUniView_OnItemDataBound;

                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
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
                        DataSourceControl.OnPageChanged += Pager_OnPageChange;
                    }
                }

                mHandlersRegistered = true;
            }
        }


        /// <summary>
        /// Event raised when page changed
        /// </summary>
        protected void Pager_OnPageChange(object sender, EventArgs ea)
        {
            ReloadData(true);
        }


        /// <summary>
        /// On item databound using for Nested repeater.
        /// </summary>
        private void CMSUniView_OnItemDataBound(object sender, UniViewItem item)
        {
            // Bind the container
            if (item.Controls.Count > 0)
            {
                Control itemControl = item.Controls[0];

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
                            if (ValidationHelper.GetString(((DataRowView)item.DataItem).Row["NodeAliasPath"], "") != "")
                            {
                                // Set alias path
                                ctrl.Path = ValidationHelper.GetString(((DataRowView)item.DataItem).Row["NodeAliasPath"], "");

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
        /// Handles OnFilterChanged event.
        /// </summary>
        private void DataSourceControl_OnFilterChanged()
        {
            ReloadData(true);
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[CMSUniView: " + ID + "]");
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
                        // Update the ID of the secondary pager control to ensure that the second pager does not render duplicated client IDs.
                        Control directPageControl = ControlsHelper.GetChildControl(this, typeof(System.Web.UI.WebControls.TextBox), PagerControl.DirectPageControlID);
                        directPageControl.ID += "secondary";

                        // Make sure that all the pager scripts are updated with the new ID.
                        PagerControl.ReloadData(true);
                    }

                    PagerControl.Visible = isVisible;
                    PagerControl.RenderControl(writer);
                }
            }
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public virtual void ClearCache()
        {
            mDataSource.ClearCache();
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

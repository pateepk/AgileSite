using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;

using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

using SystemTreeNode = System.Web.UI.WebControls.TreeNode;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSTreeView class.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:CMSTreeView runat=server></{0}:CMSTreeView>")]
    public class CMSTreeView : TreeView
    {
        #region "Variables"

        private readonly SafeDictionary<int, string> mClassIcons = new SafeDictionary<int, string>();

        private readonly SafeDictionary<int, List<DataRowView>> mDataRows = new SafeDictionary<int, List<DataRowView>>();

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl;


        /// <summary>
        /// Menu properties variable.
        /// </summary>        
        protected CMSMenuProperties mProperties = new CMSMenuProperties();

        private string mRootText = String.Empty;
        private string mRootImageUrl = String.Empty;
        private bool mInactiveRoot = true;
        private bool mDynamicBehavior = true;
        private string mNodeImageUrl = String.Empty;
        private string mOnClickAction = String.Empty;
        private bool mDisplayDocumentTypeImages = true;
        private bool mInactiveNodeImage = true;
        private bool mHighlightSelectedItem = true;
        private string mSelectedItemClass = String.Empty;
        private bool mExpandCurrentPath = true;
        private string mItemStyle = String.Empty;
        private string mSelectedItemStyle = String.Empty;
        private string mItemClass = String.Empty;
        private string mInactiveItemStyle = String.Empty;
        private string mInactiveItemClass = String.Empty;
        private MacroResolver mLocalResolver;

        // Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        private bool defaultLoadCalled;

        #endregion


        #region "Properties"

        /// <summary>
        /// Local instance of macro resolver.
        /// </summary>
        public MacroResolver LocalResolver
        {
            get
            {
                return mLocalResolver ?? (mLocalResolver = mProperties.ContextResolver.CreateChild());
            }
        }

        #endregion


        #region "CMS Control Properties"

        /// <summary>
        /// Text to be shown when the control is hidden by HideControlForZeroRows.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when the control is hidden by HideControlForZeroRows.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return mProperties.ZeroRowsText;
            }
            set
            {
                mProperties.ZeroRowsText = value;
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
                return mProperties.HideControlForZeroRows;
            }
            set
            {
                mProperties.HideControlForZeroRows = value;
            }
        }


        /// <summary>
        /// Indicates if text can be wrapped or space is replaced with non breakable space.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if text can be wrapped or space is replaced with &nbsp;.")]
        public virtual bool WordWrap
        {
            get
            {
                return mProperties.WordWrap;
            }
            set
            {
                mProperties.WordWrap = value;
            }
        }


        /// <summary>
        /// Stop processing.
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
        /// Indicates if highlighted images is not specified, use item image if exist.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if highlighted images is not specified, use item image if exist.")]
        public virtual bool UseItemImagesForHighlightedItem
        {
            get
            {
                return mProperties.UseItemImagesForHighlightedItem;
            }
            set
            {
                mProperties.UseItemImagesForHighlightedItem = value;
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
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue("/%"), Description("Path to the menu items that should be displayed in the site map.")]
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
        /// Gets or sets a DataSet containing values used to populate the items within the control. This value needn't be set.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new virtual DataSet DataSource
        {
            get
            {
                return mProperties.DataSource;
            }
            set
            {
                mProperties.DataSource = value;
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return mProperties.ControlTagKey;
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
        public CMSAbstractBaseFilterControl FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (!DataHelper.IsEmpty(FilterName))
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

        #endregion


        #region "CMSTreeView Control Properties"

        /// <summary>
        /// Fix broken lines.
        /// </summary>    
        [Category("Behavior"), DefaultValue(false), Description("Fix broken lines.")]
        public bool FixBrokenLines
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["FixBrokenLines"], false);
            }
            set
            {
                ViewState["FixBrokenLines"] = value;
            }
        }


        /// <summary>
        /// Expand all nodes in tree.
        /// </summary>    
        [Category("Behavior"), DefaultValue(false), Description("Expand all nodes in tree.")]
        public bool ExpandAllOnStartup
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ExpandAllOnStartup"], false);
            }
            set
            {
                ViewState["ExpandAllOnStartup"] = value;
            }
        }


        /// <summary>
        /// Specifies prefix of standard CMSMenu CSS classes. You can also use several values separated with semicolon (;) for particular levels.
        /// </summary>    
        [Category("Design"), DefaultValue(""), Description("Specifies prefix of standard CMSMenu CSS classes. You can also use several values separated with semicolon (;) for particular levels.")]
        public string CSSPrefix
        {
            get
            {
                return mProperties.CSSPrefix;
            }
            set
            {
                mProperties.CSSPrefix = value;
            }
        }


        /// <summary>
        /// If RootText is set to the some value, this value replace normal root node text.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("If RootText is set to the some value, this value replace normal root node text.")]
        public string RootText
        {
            get
            {
                return mRootText;
            }
            set
            {
                mRootText = value;
            }
        }


        /// <summary>
        /// If RootImageUrl is set to the some value, this image will be displayed for the root node.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("If RootImageUrl is set to the some value, this image will be displayed for the root node.")]
        public string RootImageUrl
        {
            get
            {
                return mRootImageUrl;
            }
            set
            {
                mRootImageUrl = value;
            }
        }


        /// <summary>
        /// If it is true, root node is not active.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If it is true, root node is not active.")]
        public bool InactiveRoot
        {
            get
            {
                return mInactiveRoot;
            }
            set
            {
                mInactiveRoot = value;
            }
        }


        /// <summary>
        /// Enable populate on demand and load child nodes dynamically.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Enable populate on demand and load child nodes dynamically.")]
        public bool DynamicBehavior
        {
            get
            {
                return mDynamicBehavior;
            }
            set
            {
                mDynamicBehavior = value;
            }
        }


        /// <summary>
        /// Sets or Get images for every node.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Set or Get images for every node.")]
        public string NodeImageUrl
        {
            get
            {
                return mNodeImageUrl;
            }
            set
            {
                mNodeImageUrl = value;
            }
        }


        /// <summary>
        /// Sets or Get onclick javascript action.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Set or Get onclick javascript action.")]
        public string OnClickAction
        {
            get
            {
                return mOnClickAction;
            }
            set
            {
                mOnClickAction = value;

                if (mOnClickAction.Contains("%%"))
                {
                    mOnClickAction = mOnClickAction.Replace("%%", "%");
                }
            }
        }


        /// <summary>
        /// Indicates if images are sets by doc type images.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if images are sets by doc type images.")]
        public bool DisplayDocumentTypeImages
        {
            get
            {
                return mDisplayDocumentTypeImages;
            }
            set
            {
                mDisplayDocumentTypeImages = value;
            }
        }


        /// <summary>
        /// Indicates if images are sets by doc type images.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if tooltips with node name are shown.")]
        public bool ShowToolTips
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if node image is inactive.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if node image is inactive.")]
        public bool InactiveNodeImage
        {
            get
            {
                return mInactiveNodeImage;
            }
            set
            {
                mInactiveNodeImage = value;
            }
        }


        /// <summary>
        /// If it is true, treeview will expand all items to the current item.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If it is true, treeview will expand all items to the current item.")]
        public bool ExpandCurrentPath
        {
            get
            {
                return mExpandCurrentPath;
            }
            set
            {
                mExpandCurrentPath = value;
            }
        }


        /// <summary>
        /// Inactivate selected item.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Inactivate selected item.")]
        public bool InactivateSelectedItem
        {
            get;
            set;
        }


        /// <summary>
        /// Inactivate all items in path.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Inactivate all items in path.")]
        public bool InactivateAllItemsInPath
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if selected item is highlighted.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if selected item is highlighted.")]
        public bool HiglightSelectedItem
        {
            get
            {
                return mHighlightSelectedItem;
            }
            set
            {
                mHighlightSelectedItem = value;
            }
        }


        /// <summary>
        /// Selected item style.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Selected item style.")]
        public string SelectedItemStyle
        {
            get
            {
                return mSelectedItemStyle;
            }
            set
            {
                mSelectedItemStyle = value;
            }
        }


        /// <summary>
        /// Selected item class.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Selected item class.")]
        public string SelectedItemClass
        {
            get
            {
                return mSelectedItemClass;
            }
            set
            {
                mSelectedItemClass = value;
            }
        }


        /// <summary>
        /// Item style.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Item style.")]
        public string ItemStyle
        {
            get
            {
                return mItemStyle;
            }
            set
            {
                mItemStyle = value;
            }
        }


        /// <summary>
        /// Item class.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Item class.")]
        public string ItemClass
        {
            get
            {
                return mItemClass;
            }
            set
            {
                mItemClass = value;
            }
        }


        /// <summary>
        /// Inactive item style.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Inactive item style.")]
        public string InactiveItemStyle
        {
            get
            {
                return mInactiveItemStyle;
            }
            set
            {
                mInactiveItemStyle = value;
            }
        }


        /// <summary>
        /// Inactive item class.
        /// </summary>
        [Category("Design"), DefaultValue(""), Description("Inactive item class.")]
        public string InactiveItemClass
        {
            get
            {
                return mInactiveItemClass;
            }
            set
            {
                mInactiveItemClass = value;
            }
        }


        /// <summary>
        /// Enable or disable system mode (ignore document show in navigation and e.g.).
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Enable or disable system mode (ignore document show in navigation and e.g.) .")]
        public bool SystemMode
        {
            get;
            set;
        }


        /// <summary>
        /// Ignore document menu action.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Ignore document menu action.")]
        public bool IgnoreDocumentMenuAction
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether root node is hidden.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether root node is hidden.")]
        public bool HideRootNode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether subtree under current item is expanded.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether subtree under current item is expanded.")]
        public bool ExpandSubTree
        {
            get;
            set;
        }


        /// <summary>
        /// Grouped data source.
        /// </summary>
        protected GroupedDataSource GroupedDS
        {
            get
            {
                return mProperties.GroupedDS;
            }
        }


        /// <summary>
        /// Indicates if menu caption should be HTML encoded.
        /// </summary>
        [Category("Behavior"), Description("Indicates if menu caption should be HTML encoded.")]
        public bool EncodeMenuCaption
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EncodeMenuCaption"], true);
            }
            set
            {
                ViewState["EncodeMenuCaption"] = value;
            }
        }

        #endregion


        #region "Javascript internal properties"

        private string mJSselectedObject = String.Empty;
        private string mJSoriginalStyle = String.Empty;
        private string mJSoriginalClass = String.Empty;
        private string mJSselectedStyle = String.Empty;
        private string mJSselectedClass = String.Empty;

        /// <summary>
        /// Selected object.
        /// </summary>
        public string JSselectedObject
        {
            get
            {
                return mJSselectedObject;
            }
            set
            {
                mJSselectedObject = value;
            }
        }


        /// <summary>
        /// Original item style.
        /// </summary>
        public string JSoriginalStyle
        {
            get
            {
                return mJSoriginalStyle;
            }
            set
            {
                mJSoriginalStyle = value;
            }
        }


        /// <summary>
        /// Original item class.
        /// </summary>
        public string JSoriginalClass
        {
            get
            {
                return mJSoriginalClass;
            }
            set
            {
                mJSoriginalClass = value;
            }
        }


        /// <summary>
        /// Selected item style.
        /// </summary>
        public string JSselectedStyle
        {
            get
            {
                return mJSselectedStyle;
            }
            set
            {
                mJSselectedStyle = value;
            }
        }


        /// <summary>
        /// Selected item class.
        /// </summary>
        public string JSselectedClass
        {
            get
            {
                return mJSselectedClass;
            }
            set
            {
                mJSselectedClass = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSTreeView()
        {
            mProperties.ParentControl = this;
            EnableViewState = false;
        }

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.InitComplete += Page_InitComplete;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            InitControl(true);
            base.OnLoad(e);
        }


        /// <summary>
        /// Init complete handler
        /// </summary>
        void Page_InitComplete(object sender, EventArgs e)
        {
            InitControl(false);
        }


        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected virtual void InitControl(bool loadPhase)
        {
            if (StopProcessing || defaultLoadCalled)
            {
                return;
            }

            defaultLoadCalled = true;

            if (FilterControl != null)
            {
                FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
            }

            ReloadData(true);
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceLoad">Indicates force load</param>
        public void ReloadData(bool forceLoad = false)
        {
            SetContext();

            if (!StopProcessing)
            {
                Nodes.Clear();
                GenerateTree();


                // Expand root node
                if ((Nodes.Count > 0) && (!HideRootNode))
                {
                    Nodes[0].Expand();
                }

                // Expand all if it is allowed
                if (ExpandAllOnStartup)
                {
                    ExpandAll();
                }
            }

            ReleaseContext();
        }


        /// <summary>
        /// OnTreeNodePopulate.
        /// </summary>
        /// <param name="e">TreeNodeEventArgs</param>
        protected override void OnTreeNodePopulate(TreeNodeEventArgs e)
        {
            base.OnTreeNodePopulate(e);

            int parentNodeId = ValidationHelper.GetInteger(e.Node.Value, 0);
            ProcessSubItem(e.Node, parentNodeId);
        }


        /// <summary>
        /// Creates the child controls collection.
        /// </summary>
        protected void GenerateTree()
        {
            if (StopProcessing)
            {
                return;
            }

            // Clear controls collection
            Controls.Clear();

            // Check if context exists
            if (Context != null)
            {
                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(mProperties);
                }

                DataSet itemsDS = mProperties.GetDataSource();
                DataSource = itemsDS;

                // Check if datasource contains any items
                if (!DataHelper.DataSourceIsEmpty(itemsDS))
                {
                    // Load first level
                    int firstParentNodeId = ValidationHelper.GetInteger(DataSource.Tables[0].Rows[0]["NodeParentID"], 0);

                    if (HideRootNode || firstParentNodeId == 0)
                    {
                        // Here we do not have only one root so we need to make sure about collapsing
                        // or expanding the "roots" properly

                        var items = GroupedDS.GetGroupView(firstParentNodeId);
                        if (items != null)
                        {
                            // Alias path of the current document
                            string itemsInPath = DocumentContext.OriginalAliasPath.ToLowerCSafe();

                            foreach (DataRowView dr in items)
                            {
                                DataRowContainer drc = new DataRowContainer(dr.Row);

                                // Create root TreeNode instance and set it from root datarow
                                SystemTreeNode parentNode = GetNewTreeNode(drc, false);

                                if (items.Count > 0)
                                {
                                    // Add to this treeview root node
                                    Nodes.Add(parentNode);
                                    ProcessSubItem(parentNode, drc);
                                }

                                // Make sure every "root" is expanded or collapsed according to the settings
                                if (!ExpandAllOnStartup)
                                {
                                    // If this "root" is not in the items path then collapse it
                                    string rootItemName = ValidationHelper.GetString(dr["NodeAliasPath"], String.Empty).ToLowerCSafe();

                                    if (ExpandCurrentPath)
                                    {
                                        if (!itemsInPath.StartsWithCSafe((rootItemName + "/")))
                                        {
                                            parentNode.Collapse();
                                        }
                                    }
                                    else
                                    {
                                        parentNode.Collapse();
                                    }

                                    // Ensure sub-tree expand
                                    if ((ExpandSubTree) && (itemsInPath.EqualsCSafe(rootItemName, true)))
                                    {
                                        parentNode.Expand();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Get the root node
                        string useCacheItemName = "cmstreeviewrootnode|" + CacheHelper.GetCacheItemName(CacheItemName, firstParentNodeId);
                        TreeNode rootNode = null;

                        // Try to get data from cache
                        using (var cs = new CachedSection<TreeNode>(ref rootNode, CacheMinutes, true, useCacheItemName))
                        {
                            if (cs.LoadData)
                            {
                                // Get from the database
                                rootNode = TreeProvider.SelectSingleNode(firstParentNodeId);

                                if (cs.Cached)
                                {
                                    // Save the result to the cache
                                    cs.CacheDependency = mProperties.GetCacheDependency();
                                }

                                cs.Data = rootNode;
                            }
                        }

                        // We need to get root node (there will be only one root)
                        if (rootNode != null)
                        {
                            SystemTreeNode parentNode = GetNewTreeNode(rootNode, true);
                            Nodes.Add(parentNode);
                            ProcessSubItem(parentNode, rootNode);
                        }
                    }
                }

                NodeWrap = WordWrap;
            }
        }


        /// <summary>
        /// Gets a node item and creates its subnodes.
        /// </summary>
        /// <param name="parentTreeNode">Parent menu item object</param>
        /// <param name="parentNodeId">Parent node ID</param>
        protected void ProcessSubItem(SystemTreeNode parentTreeNode, int parentNodeId)
        {
            int nodesCount = Nodes.Count;

            var childRows = GroupedDS.GetGroupView(parentNodeId);
            if (childRows != null)
            {
                foreach (DataRowView dr in childRows)
                {
                    // Assigns if current node is in path
                    bool isInPath = false;
                    bool selectedItem = false;

                    // If is enabled 'Expand current path' check if item is in path
                    if (ExpandCurrentPath)
                    {
                        // Check if actual item is in path
                        string newItemName = ValidationHelper.GetString(dr["NodeAliasPath"], String.Empty).ToLowerCSafe();
                        string itemsInPath = DocumentContext.OriginalAliasPath.ToLowerCSafe();

                        // Is in path if paths are equal
                        if (newItemName == itemsInPath)
                        {
                            selectedItem = true;
                            isInPath = true;
                        }

                        // Is in path if current alias path starts with current node path with added "/"
                        if (itemsInPath.StartsWithCSafe((newItemName + "/")))
                        {
                            isInPath = true;
                        }
                    }

                    DataRowContainer drc = new DataRowContainer(dr.Row);
                    SystemTreeNode newTreeNode = GetNewTreeNode(drc, false);

                    // Add current row levels to the hashtable and set populate on demand
                    // only if current item isn't in path with enabled ExpandCurrentPath 
                    if (DynamicBehavior && !ExpandAllOnStartup)
                    {
                        int nodeId = ValidationHelper.GetInteger(dr["NodeID"], 0);

                        //Check if datarow for selected node id exist in hashtable
                        var childItems = mDataRows[nodeId];
                        if (childItems == null)
                        {
                            childItems = GroupedDS.GetGroupView(nodeId) ?? new List<DataRowView>();

                            mDataRows[nodeId] = childItems;
                        }

                        // Check if exist some rows
                        if ((childItems.Count > 0) && (!ExpandAllOnStartup))
                        {
                            newTreeNode.PopulateOnDemand = true;

                            // If is current node in path, disable populate on demand
                            if (ExpandCurrentPath && isInPath)
                            {
                                newTreeNode.PopulateOnDemand = false;
                            }
                        }
                    }

                    // In deafult is node collapsed 
                    newTreeNode.Collapse();

                    // Expand current node if is ExpandCurrentPath enabled
                    if (ExpandCurrentPath && isInPath && !selectedItem)
                    {
                        newTreeNode.Expand();
                    }

                    // Expand node if it is selected node and Expand sub tree is enabled and exists some child nodes
                    if (selectedItem && ExpandSubTree && ValidationHelper.GetBoolean(dr["NodeHasChildren"], false))
                    {
                        newTreeNode.Expand();
                    }

                    // if nodes count is 0 => root node is not displayed
                    if (nodesCount > 0)
                    {
                        // Add current node to the parent node childnodes
                        parentTreeNode.ChildNodes.Add(newTreeNode);
                    }
                    else
                    {
                        Nodes.Add(newTreeNode);
                    }

                    // Call ProcessSubItem
                    if ((!DynamicBehavior || ExpandAllOnStartup) || (ExpandCurrentPath && isInPath))
                    {
                        drc = new DataRowContainer(dr.Row);
                        ProcessSubItem(newTreeNode, drc);
                    }
                }
            }
        }


        /// <summary>
        /// Gets a node item and creates its subnodes.
        /// </summary>
        /// <param name="parentTreeNode">Parent menu item object</param>
        /// <param name="data">Data representing parent menu item</param>
        protected void ProcessSubItem(SystemTreeNode parentTreeNode, IDataContainer data)
        {
            int parentNodeId = ValidationHelper.GetInteger(data.GetValue("NodeID"), 0);
            ProcessSubItem(parentTreeNode, parentNodeId);
        }


        /// <summary>
        /// Returns a new TreeNode object based on provided data row.
        /// </summary>
        /// <param name="data">Data row containing item description</param>
        /// <param name="rootNode">Indicates if current node is root</param>
        protected SystemTreeNode GetNewTreeNode(IDataContainer data, bool rootNode)
        {
            // Add current datarow to the resolver
            LocalResolver.SetAnonymousSourceData(data);

            // Creta new TreeNode instance
            bool isInPath = false;

            // Check if actual item is in path
            string newItemPath = ValidationHelper.GetString(data.GetValue("NodeAliasPath"), String.Empty).ToLowerCSafe();
            string itemsInPath = DocumentContext.OriginalAliasPath.ToLowerCSafe();
            bool selectedItem = false;

            // Is in path if paths are equal
            if (newItemPath == itemsInPath)
            {
                selectedItem = true;
                isInPath = true;
            }

            // Is in path if current alias path starts with current node path with added "/"
            if (itemsInPath.StartsWithCSafe((newItemPath + "/")))
            {
                isInPath = true;
            }

            // Create new menu item object
            SystemTreeNode newTreeNode = new SystemTreeNode();

            // Disable standard node select action, select action will be provided by custom link
            newTreeNode.SelectAction = TreeNodeSelectAction.None;

            // Prepare the item name. Disable encoding. Encoding depends on "EncodeMenuCaption" property
            LocalResolver.Settings.EncodeResolvedValues = false;
            string itemName = LocalResolver.ResolveMacros(TreePathUtils.GetMenuCaption(ValidationHelper.GetString(data.GetValue("DocumentMenuCaption"), String.Empty), ValidationHelper.GetString(data.GetValue("DocumentName"), String.Empty)));
            LocalResolver.Settings.EncodeResolvedValues = true;

            if (ShowToolTips)
            {
                newTreeNode.ToolTip = itemName;
            }

            // If current node is root and RootNodeText is set, set it to the node text
            if ((rootNode) && (RootText != String.Empty))
            {
                newTreeNode.Text = RootText;
            }
            else
            {
                newTreeNode.Text = itemName;

                // if current node is root and RootNodeText isn't set, set node text to site name
                if (newTreeNode.Text == String.Empty)
                {
                    newTreeNode.Text = SiteContext.CurrentSite.DisplayName;
                }
            }

            // HTML encode item name
            if (EncodeMenuCaption)
            {
                newTreeNode.Text = HTMLHelper.HTMLEncode(newTreeNode.Text);
            }

            // Replace " " to nbsp if wrapping is not enabled
            if (!WordWrap)
            {
                newTreeNode.Text = newTreeNode.Text.Replace(" ", "&nbsp;");
            }

            // Image url
            string imgUrl = String.Empty;
            string imgTag = String.Empty;
            string icon = String.Empty;

            // If current node is root and root image url is set, set it to the image
            if ((rootNode) && (RootImageUrl != String.Empty))
            {
                imgUrl = ResolveUrl(RootImageUrl);
            }

            // If image url is set, set it to the node
            if ((!rootNode) && (NodeImageUrl != String.Empty))
            {
                imgUrl = ResolveUrl(NodeImageUrl);
            }

            // Check if image URL was set
            if (!string.IsNullOrEmpty(imgUrl))
            {
                icon = UIHelper.GetAccessibleImageTag(Page, imgUrl);
            }
            // Check doctype icon
            else if (DisplayDocumentTypeImages)
            {
                // Check if icon isn't in icon hashtable
                if (mClassIcons[ValidationHelper.GetInteger(data.GetValue("NodeClassID"), 0)] == null)
                {
                    // Get class info
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(ValidationHelper.GetInteger(data.GetValue("NodeClassID"), 0));
                    if (dci != null)
                    {
                        var iconClass = ValidationHelper.GetString(dci.GetValue("ClassIconClass"), String.Empty);
                        icon = UIHelper.GetDocumentTypeIcon(Page, dci.ClassName, iconClass);
                        mClassIcons[ValidationHelper.GetInteger(data.GetValue("NodeClassID"), 0)] = icon;
                    }
                }
                else
                {
                    // Get icon from icon hashtable
                    icon = mClassIcons[ValidationHelper.GetInteger(data.GetValue("NodeClassID"), 0)].ToString();
                }
            }

            // Set image url
            if (icon != String.Empty)
            {
                imgTag = icon + "&nbsp;";
            }

            // Indicates if current item is inactive
            bool inactiveItem = (rootNode & InactiveRoot) | (InactivateSelectedItem & selectedItem) | (InactivateAllItemsInPath & isInPath);

            // Navigate node url
            string url = String.Empty;

            if (IgnoreDocumentMenuAction)
            {
                // Menu setting ignored, get only document URL
                var isLink = data.GetValue("NodeLinkedNodeID") != DBNull.Value;
                url = DocumentURLProvider.GetUrl(ValidationHelper.GetString(data.GetValue("NodeAliasPath"), String.Empty), isLink ? null : ValidationHelper.GetString(data.GetValue("DocumentUrlPath"), String.Empty));
                url = UrlResolver.ResolveUrl(url);
            }
            else
            {
                // Get URL or set inactive assign
                if (ValidationHelper.GetBoolean(data.GetValue("DocumentMenuItemInactive"), false))
                {
                    inactiveItem = true;
                }
                else
                {
                    url = DocumentURLProvider.GetNavigationUrl(data, LocalResolver);
                }
            }

            // Set inactive node image or inactive root node
            if (InactiveNodeImage || inactiveItem)
            {
                // Set inactive item
                if (inactiveItem)
                {
                    newTreeNode.Text = imgTag + "<span #####INACTIVE_STYLE_MACRO##### #####INACTIVE_CLASS_MACRO##### #####ID_MACRO##### #####ON_MOUSE_DOWN_MACRO#####>" + newTreeNode.Text + "</span>";
                }
                else
                {
                    newTreeNode.Text = imgTag + "<a href=\"" + url + "\" #####ON_MOUSE_DOWN_MACRO##### #####CMS_ONCLICK_MACRO##### #####TARGET_MACRO##### #####STYLE_MACRO##### #####CLASS_MACRO##### #####ID_MACRO#####>" + newTreeNode.Text + "</a>";
                }
            }
            else
            {
                newTreeNode.Text = "<a href=\"" + url + "\" #####CMS_ONCLICK_MACRO##### #####TARGET_MACRO##### style=\"text-decoration:none;\">" + imgTag + "</a><a href=\"" + url + "\" #####ON_MOUSE_DOWN_MACRO##### #####CMS_ONCLICK_MACRO##### #####TARGET_MACRO##### #####STYLE_MACRO##### #####CLASS_MACRO##### #####ID_MACRO#####>" + newTreeNode.Text + "</a>";
            }

            // Set on click action
            if (OnClickAction != String.Empty)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####CMS_ONCLICK_MACRO#####", " onclick=\"" + LocalResolver.ResolveMacros(OnClickAction) + "\"");
            }
            else
            {
                // Set document menu javascript if is defined
                if ((ValidationHelper.GetString(data.GetValue("DocumentMenuJavascript"), String.Empty) != String.Empty) && (!IgnoreDocumentMenuAction))
                {
                    newTreeNode.Text = newTreeNode.Text.Replace("#####CMS_ONCLICK_MACRO#####", " onclick=\"" + LocalResolver.ResolveMacros(ValidationHelper.GetString(data.GetValue("DocumentMenuJavascript"), String.Empty)) + "\"");
                }
            }

            // Set target if it is defined 
            if (Target != String.Empty)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####TARGET_MACRO#####", " target=\"" + Target + "\"");
            }

            // Set style/class if is defined
            if (HiglightSelectedItem && selectedItem)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####STYLE_MACRO#####", " style=\"" + SelectedItemStyle == String.Empty ? String.Empty : SelectedItemStyle + "\"");

                if (!String.IsNullOrEmpty(SelectedItemClass))
                {
                    newTreeNode.Text = newTreeNode.Text.Replace("#####CLASS_MACRO#####", " class=\"" + SelectedItemClass + "\"");
                }
            }

            if (selectedItem)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####ID_MACRO#####", " id=\"" + ClientID + "_CMSselectedNode\"");
            }

            if (ItemStyle != String.Empty)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####STYLE_MACRO#####", " style=\"" + ItemStyle + "\"");
            }

            if (ItemClass != String.Empty)
            {
                newTreeNode.Text = newTreeNode.Text.Replace("#####CLASS_MACRO#####", " class=\"" + ItemClass + "\"");
            }

            if (!rootNode)
            {
                if (InactiveItemClass != String.Empty)
                {
                    if ((selectedItem) && (HiglightSelectedItem) && (SelectedItemClass != String.Empty))
                    {
                        newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_CLASS_MACRO#####", " class=\"" + SelectedItemClass + "\"");
                    }
                    else
                    {
                        newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_CLASS_MACRO#####", " class=\"" + InactiveItemClass + "\"");
                    }
                }

                if (InactiveItemStyle != String.Empty)
                {
                    if ((selectedItem) && (HiglightSelectedItem) && (SelectedItemStyle != String.Empty))
                    {
                        newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_STYLE_MACRO#####", " style=\"" + SelectedItemStyle + "\"");
                    }
                    else
                    {
                        newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_STYLE_MACRO#####", " style=\"" + InactiveItemStyle + "\"");
                    }
                }
            }

            // Clear unusable macros
            newTreeNode.Text = newTreeNode.Text.Replace("#####ON_MOUSE_DOWN_MACRO#####", " onmousedown=\"" + GenerateOnClickAction(!inactiveItem) + "\"");
            newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_STYLE_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####INACTIVE_CLASS_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####ID_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####CLASS_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####STYLE_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####TARGET_MACRO#####", String.Empty);
            newTreeNode.Text = newTreeNode.Text.Replace("#####CMS_ONCLICK_MACRO#####", String.Empty);

            // Set node id like node value
            newTreeNode.Value = ValidationHelper.GetString(data.GetValue("NodeID"), "0");

            return newTreeNode;
        }


        /// <summary>
        /// Generate onClick action for node link.
        /// </summary>
        /// <param name="active">Indicates if current node is active</param>
        protected string GenerateOnClickAction(bool active)
        {
            if (active)
            {
                return "CMSTreeView_ItemSelect(this,'" + ClientID + "');";
            }

            return String.Empty;
        }


        /// <summary>
        /// Initialize javascript variables.
        /// </summary>
        protected string InitializeJavascript()
        {
            string jsPrefix = "<input type=\"hidden\" id=\"" + ClientID + "_";

            JSoriginalClass = jsPrefix + "originalClass\" value=\"" + ItemClass + "\" />";

            JSoriginalStyle = jsPrefix + "originalStyle\" value=\"" + ItemStyle + "\" />";

            JSselectedClass = jsPrefix + "selectedClass\" value=\"" + SelectedItemClass + "\" />";

            JSselectedStyle = jsPrefix + "selectedStyle\" value=\"" + SelectedItemStyle + "\" />";

            return JSoriginalClass + JSoriginalStyle + JSselectedClass + JSselectedStyle;
        }


        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Registers treeview.js script file
            ScriptHelper.RegisterScriptFile(Page, "Controls/treeview.js");

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ClientID,
                                                   ScriptHelper.GetScript(JSoriginalClass + JSoriginalStyle + JSselectedClass + JSselectedObject + JSselectedStyle));

            // Fix broken lines
            if (FixBrokenLines)
            {
                CssClass = "TreeViewBrokenLinesFix " + CssClass;
            }


            // Fix broken lines
            if (FixBrokenLines)
            {
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "TreeViewBrokenLines", ScriptHelper.GetScript("" +
                                                                                                                           "if (document.styleSheets.length == 0){document.createStyleSheet('extrastyle.css');}" +
                                                                                                                           "document.styleSheets[0].addRule('.TreeViewBrokenLinesFix td div','height: 20px !important');"));
            }

            // Encode image tooltips
            CollapseImageToolTip = HTMLHelper.HTMLEncode(CollapseImageToolTip);
            ExpandImageToolTip = HTMLHelper.HTMLEncode(ExpandImageToolTip);

            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (!StopProcessing)
            {
                if (Context == null)
                {
                    output.Write("[ CMSTreeView Control : " + ClientID + " ]");
                }
                else
                {
                    // Hide control for zero rows or display zero rows text
                    if (DataHelper.DataSourceIsEmpty(DataSource))
                    {
                        if (HideControlForZeroRows)
                        {
                            return;
                        }
                        else if (!String.IsNullOrEmpty(ZeroRowsText))
                        {
                            output.Write(ZeroRowsText);
                            return;
                        }
                    }

                    output.Write(InitializeJavascript());

                    base.Render(output);
                }
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
            ReloadData(true);
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return mProperties.GetDefaultCacheDependencies();
        }

        #endregion
    }
}

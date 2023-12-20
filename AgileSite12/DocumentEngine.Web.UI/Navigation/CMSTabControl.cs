using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System;
using System.Data;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.DocumentEngine.Web.UI.Configuration;
using CMS.Helpers;
using CMS.Base;
using CMS.MacroEngine;
using CMS.PortalEngine.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Tab control that can be bounded to the CMS content.
    /// </summary>
    [ToolboxData("<{0}:CMSTabControl runat=server></{0}:CMSTabControl>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSTabControl : BasicTabControl
    {
        #region "Variables"

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Menu properties variable.
        /// </summary>
        protected CMSMenuProperties mProperties = new CMSMenuProperties();

        /// <summary>
        /// Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        /// </summary>
        private bool defaultLoadCalled;

        #endregion


        #region "CMS Control Properties"

        /// <summary>
        /// Render the image alt attribute?
        /// </summary>
        [Category("Behavior"), Description("Render the alt attribute within the image.")]
        public bool RenderImageAlt
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderImageAlt"], true);
            }
            set
            {
                ViewState["RenderImageAlt"] = value;
            }
        }


        /// <summary>
        /// If is set true, first item will be selected by default if is not some other item selected.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("If is set true, first item will be selected by default if is not some other item selected.")]
        public override bool SelectFirstItemByDefault
        {
            get
            {
                if (ViewState["SelectFirstItemByDefault"] != null)
                {
                    ViewState["SelectFirstItemByDefault"] = base.SelectFirstItemByDefault;
                    return base.SelectFirstItemByDefault;
                }

                base.SelectFirstItemByDefault = false;
                return false;
            }
            set
            {
                ViewState["SelectFirstItemByDefault"] = value;
                base.SelectFirstItemByDefault = value;
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
        /// Indicates if apply document menu item properties.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if apply document menu item properties.")]
        public virtual bool ApplyMenuDesign
        {
            get
            {
                return mProperties.ApplyMenuDesign;
            }
            set
            {
                mProperties.ApplyMenuDesign = value;
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
        /// Path of the menu items to be displayed.
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
        /// ORDER BY expression.
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
        /// Level of nesting.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1), Description("Level of nesting. Value 1 returns only the node itself. Use -1 for unlimited recurrence.")]
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
        /// Gets or sets a DataSet containing values used to populate the items within the control. This value needn't be set.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataSet DataSource
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
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>  
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


        #region "CMS Menu Control Properties"

        /// <summary>
        /// Indicates if alternating styles should be used for even and odd items in the same level of the menu.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if alternating styles should be used for even and odd items in the same level of the menu.")]
        public virtual bool UseAlternatingStyles
        {
            get
            {
                return mProperties.UseAlternatingStyles;
            }
            set
            {
                mProperties.UseAlternatingStyles = value;
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

        #endregion // CMS Control Properties


        #region "Public properties"

        /// <summary>
        /// Indicates if data will be loaded automatically.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if data will be loaded automatically.")]
        public bool LoadDataAutomaticaly
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["LoadDataAutomaticaly"], true);
            }
            set
            {
                ViewState["LoadDataAutomaticaly"] = value;
            }
        }


        /// <summary>
        /// Path of the item that will be highlighted like it was selected. The path type must be the same as PathType. If you omit this value, the control automatically uses the current alias path from the "aliaspath" querystring parameter.
        /// </summary>
        [Category("Behavior"), Description("Path of the item that will be highlighted like it was selected. The path type must be the same as PathType. If you omit this value, the control automatically uses the current alias path from the aliaspath querystring parameter.")]
        public string HighlightedNodePath
        {
            get
            {
                if ((ViewState["HighlightedNodePath"] == null))
                {
                    if (!String.IsNullOrEmpty(DocumentContext.OriginalAliasPath))
                    {
                        ViewState["HighlightedNodePath"] = DocumentContext.OriginalAliasPath;
                    }
                    else
                    {
                        ViewState["HighlightedNodePath"] = QueryHelper.GetString("redirectto", String.Empty);
                    }
                }
                return ValidationHelper.GetString(ViewState["HighlightedNodePath"], String.Empty).Trim();
            }
            set
            {
                ViewState["HighlightedNodePath"] = value;
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

        #endregion // Public properties


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSTabControl()
        {
            mProperties.ParentControl = this;
        }


        /// <summary>
        /// Returns the layout mode based on the given string.
        /// </summary>
        /// <param name="controlLayout">String mode representation</param>
        public TabControlLayoutEnum GetTabControlLayout(string controlLayout)
        {
            return GetTabMenuLayout(controlLayout);
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
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
            if ((LoadDataAutomaticaly) && (!StopProcessing))
            {
                if (!defaultLoadCalled)
                {
                    defaultLoadCalled = true;

                    if (FilterControl != null)
                    {
                        FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                    }
                    ReloadData(false);
                }
            }
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Render only if is not stop processing or if is some HTML code generated
            if (!StopProcessing)
            {
                if (LoadDataAutomaticaly)
                {
                    ReloadData(false);
                }

                base.Render(output);
            }
        }


        /// <summary>
        /// ReloadData.
        /// </summary>
        /// <param name="forceLoad">Force reload of the data</param>
        public void ReloadData(bool forceLoad)
        {
            // Do not load data if is stop processing set
            if ((StopProcessing) || (Context == null))
            {
                return;
            }

            SetContext();

            // Load DataSource
            if ((DataSource == null) || (forceLoad))
            {
                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(mProperties);
                }

                DataSource = GetDataSource();
            }

            if (!DataHelper.DataSourceIsEmpty(DataSource))
            {
                TabItems = GetTabs();
                GenerateMenu();
            }

            ReleaseContext();
        }


        /// <summary>
        /// Returns an array of tabs according to the current settings.
        /// </summary>
        protected List<TabItem> GetTabs()
        {
            int i = 0;

            if (Path == String.Empty)
            {
                return null;
            }

            DataSet tabsDS = DataSource;

            if (!DataHelper.DataSourceIsEmpty(tabsDS))
            {
                List<TabItem> tabs = new List<TabItem>(tabsDS.Tables[0].Rows.Count);

                var localResolver = PortalUIHelper.GetControlResolver(Page);

                // Get sorted data rows
                foreach (DataRowView dr in tabsDS.Tables[0].DefaultView)
                {
                    // Add current datarow to the resolver
                    localResolver.SetAnonymousSourceData(dr.Row);

                    localResolver.Settings.EncodeResolvedValues = false;
                    string itemName = localResolver.ResolveMacros(TreePathUtils.GetMenuCaption(Convert.ToString(dr["DocumentMenuCaption"]), Convert.ToString(dr["DocumentName"])));
                    localResolver.Settings.EncodeResolvedValues = true;
                    TabItem tab = new TabItem();

                    // HTML encode item name
                    tab.Text = EncodeMenuCaption ? HTMLHelper.HTMLEncode(itemName) : itemName;

                    if (!WordWrap)
                    {
                        tab.Text = tab.Text.Replace(" ", "&nbsp;");
                    }

                    if (ValidationHelper.GetBoolean(dr["DocumentMenuItemInactive"], false))
                    {
                        tab.OnClientClick = "return false;";
                        tab.RedirectUrl = String.Empty;
                    }
                    else
                    {
                        string javaScript = ValidationHelper.GetString(dr["DocumentMenuJavascript"], String.Empty);
                        if (!String.IsNullOrEmpty(javaScript))
                        {
                            tab.OnClientClick = localResolver.ResolveMacros(javaScript);
                            tab.SuppressDefaultOnClientClick = true;
                        }

                        tab.RedirectUrl = DocumentURLProvider.GetNavigationUrl(new DataRowContainer(dr), localResolver);
                    }

                    tab.Tooltip = String.Empty;
                    string itemPath = Convert.ToString(dr["NodeAliasPath"]);

                    if (!string.IsNullOrEmpty(HighlightedNodePath) && (HighlightedNodePath.Trim().ToLowerCSafe() == itemPath.Trim().ToLowerCSafe()))
                    {
                        SelectedTab = i;
                    }
                    else
                    {
                        string currentAliasPath = DocumentContext.OriginalAliasPath;
                        string aliasPath = (currentAliasPath != String.Empty) ? currentAliasPath : QueryHelper.GetString("redirectto", String.Empty);

                        if ((!string.IsNullOrEmpty(aliasPath)) && (aliasPath.ToLowerCSafe().StartsWithCSafe(itemPath.Trim().ToLowerCSafe())))
                        {
                            SelectedTab = i;
                        }
                    }

                    // Menu images
                    tab.LeftItemImage = String.Empty;
                    tab.MiddleItemImage = String.Empty;
                    tab.RightItemImage = String.Empty;

                    bool isHighlight = false;

                    // Check if actual item is in path
                    if (!String.IsNullOrEmpty(HighlightedNodePath))
                    {
                        string itemsInPath = MacroResolver.ResolveCurrentPath(HighlightedNodePath).ToLowerCSafe();
                        string mItem = itemPath.ToLowerCSafe();

                        if (itemsInPath.TrimEnd('/') == mItem)
                        {
                            SelectedTab = i;
                        }
                        else if (itemsInPath.StartsWithCSafe(mItem + "/"))
                        {
                            SelectedTab = i;
                        }
                    }

                    if (!string.IsNullOrEmpty(HighlightedNodePath) && (HighlightedNodePath.Trim().ToLowerCSafe() == itemPath.Trim().ToLowerCSafe()))
                    {
                        isHighlight = true;
                    }

                    if (RenderImageAlt)
                    {
                        tab.ImageAlternativeText = "alt=\"" + HTMLHelper.HTMLEncode(itemName) + "\"";
                    }

                    if (ApplyMenuDesign)
                    {
                        if (!string.IsNullOrEmpty(HighlightedNodePath) && (HighlightedNodePath.Trim().ToLowerCSafe() == itemPath.Trim().ToLowerCSafe()))
                        {
                            isHighlight = true;

                            tab.LeftItemImage = UrlResolver.ResolveUrl(ValidationHelper.GetString(dr["DocumentMenuItemLeftImageHighlighted"], String.Empty));

                            tab.MiddleItemImage = UrlResolver.ResolveUrl(ValidationHelper.GetString(dr["DocumentMenuItemImageHighlighted"], String.Empty));

                            tab.RightItemImage = UrlResolver.ResolveUrl(ValidationHelper.GetString(dr["DocumentMenuItemRightImageHighlighted"], String.Empty));
                        }

                        // Set item images if not highlight or if Use images for highlight and it doesn't exist
                        if (!isHighlight || UseItemImagesForHighlightedItem)
                        {
                            var image = ValidationHelper.GetString(dr["DocumentMenuItemLeftImage"], String.Empty);
                            if (string.IsNullOrEmpty(tab.LeftItemImage) && (image != String.Empty))
                            {
                                tab.LeftItemImage = UrlResolver.ResolveUrl(image);
                            }

                            image = ValidationHelper.GetString(dr["DocumentMenuItemImage"], String.Empty);
                            if (string.IsNullOrEmpty(tab.MiddleItemImage) && (image != String.Empty))
                            {
                                tab.MiddleItemImage = UrlResolver.ResolveUrl(image);
                            }

                            image = ValidationHelper.GetString(dr["DocumentMenuItemRightImage"], String.Empty);
                            if (string.IsNullOrEmpty(tab.RightItemImage) && (image != String.Empty))
                            {
                                tab.RightItemImage = UrlResolver.ResolveUrl(image);
                            }
                        }
                    }

                    // Menu item style
                    if (ApplyMenuDesign && !isHighlight && (ValidationHelper.GetString(dr["DocumentMenuStyle"], String.Empty).Trim() != String.Empty))
                    {
                        tab.ItemStyle = dr["DocumentMenuStyle"].ToString();
                    }

                    // Highlighted menu item style
                    if (ApplyMenuDesign && isHighlight && (ValidationHelper.GetString(dr["DocumentMenuStyleHighlighted"], String.Empty).Trim() != String.Empty))
                    {
                        tab.ItemStyle = dr["DocumentMenuStyleHighlighted"].ToString();
                    }

                    // Use alternation styles for odd items.
                    if (UseAlternatingStyles && (i % 2 == 1))
                    {
                        tab.AlternatingCssSuffix = "Alt";
                    }
                    else
                    {
                        tab.AlternatingCssSuffix = String.Empty;
                    }

                    // Class for actual item
                    if (ApplyMenuDesign && !isHighlight && (ValidationHelper.GetString(dr["DocumentMenuClass"], String.Empty).Trim() != String.Empty))
                    {
                        tab.CssClass = dr["DocumentMenuClass"].ToString();
                    }

                    // Class for actual highlighted item
                    if (ApplyMenuDesign && isHighlight && (ValidationHelper.GetString(dr["DocumentMenuClassHighlighted"], String.Empty).Trim() != String.Empty))
                    {
                        tab.CssClass = dr["DocumentMenuClassHighlighted"].ToString();
                    }
                    tabs.Add(tab);
                    // Increment 'i'
                    i++;
                }

                return tabs;
            }

            return null;
        }


        /// <summary>
        /// Returns data - either from database or from cache.
        /// </summary>
        private DataSet GetDataSource()
        {
            return mProperties.GetDataSource();
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
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            return mProperties.GetCacheDependency();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI;

using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.Base;
using CMS.DataEngine;
using CMS.PortalEngine.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base menu controls properties.
    /// </summary>
    public abstract class CMSAbstractMenuProperties : CMSAbstractControlProperties, ICMSMenuProperties
    {
        #region "Variables"

        /// <summary>
        /// CSS Prefixes.
        /// </summary>
        protected string[] mCSSPrefixes;

        private GroupedDataSource mGroupedDS;

        // Indicates whether init call was fired (due to dynamically added control to the control collection after Init phase)
        private bool mDefaultLoadCalled;

        private MacroResolver mCurrentResolver;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Menu properties constructor.
        /// </summary>
        protected CMSAbstractMenuProperties()
        {
        }


        /// <summary>
        /// Menu properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        protected CMSAbstractMenuProperties(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        #endregion


        #region "Public properties"

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
        /// Indicates if text can be wrapped or space is replaced with 'nbsp' entity.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if text can be wrapped or space is replaced with &nbsp;.")]
        public virtual bool WordWrap
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["WordWrap"], true);
            }
            set
            {
                ViewState["WordWrap"] = value;
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
                return ValidationHelper.GetBoolean(ViewState["ApplyMenuDesign"], true);
            }
            set
            {
                ViewState["ApplyMenuDesign"] = value;
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
                return ValidationHelper.GetBoolean(ViewState["UseItemImagesForHighlightedItem"], false);
            }
            set
            {
                ViewState["UseItemImagesForHighlightedItem"] = value;
            }
        }

        
        /// <summary>
        /// Indicates if all items in the unfolded path should be displayed as highlighted.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if all items in the unfolded path should be displayed as highlighted.")]
        public virtual bool HighlightAllItemsInPath
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HiglightAllItemsInPath"], false);
            }
            set
            {
                ViewState["HiglightAllItemsInPath"] = value;
            }
        }


        /// <summary>
        /// Contains a path to image that will be used on the right of every item that contains sub items.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Contains a path to image that will be used on the right of every item that contains sub items.")]
        public virtual string SubmenuIndicator
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SubmenuIndicator"], "");
            }
            set
            {
                ViewState["SubmenuIndicator"] = value;
            }
        }


        /// <summary>
        /// Indicates if alternating styles should be used for even and odd items in the same level of the menu.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if alternating styles should be used for even and odd items in the same level of the menu.")]
        public virtual bool UseAlternatingStyles
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UseAlternatingStyles"], false);
            }
            set
            {
                ViewState["UseAlternatingStyles"] = value;
            }
        }


        /// <summary>
        /// Specifies prefix of standard CMSMenu CSS classes. You can also use several values separated with semicolon (;) for particular levels.
        /// </summary>    
        [Category("Behavior"), DefaultValue(""), Description("Specifies prefix of standard CMSMenu CSS classes. You can also use several values separated with semicolon (;) for particular levels.")]
        public string CSSPrefix
        {
            get
            {
                if (ViewState["CSSPrefix"] == null)
                {
                    ViewState["CSSPrefix"] = "";
                }
                return ValidationHelper.GetString(ViewState["CSSPrefix"], "");
            }
            set
            {
                ViewState["CSSPrefix"] = value;
            }
        }


        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue("/%"), Description("Path to the menu items that should be displayed in the site map.")]
        public override string Path
        {
            get
            {
                return ValidationHelper.GetString(ViewState["Path"], "/%");
            }

            set
            {
                ViewState["Path"] = value;
            }
        }


        /// <summary>
        /// Grouped data source.
        /// </summary>
        [ToolboxItem(false)]
        public GroupedDataSource GroupedDS
        {
            get
            {
                return mGroupedDS ?? (mGroupedDS = new GroupedDataSource(DataSource, "NodeParentID"));
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
        /// Gets the default columns which are used to retrieve data if Columns property not set to '*'.
        /// </summary>   
        [ToolboxItem(false)]
        public virtual string DefaultColumns
        {
            get
            {
                return "ClassName, DocumentCulture, DocumentGUID, DocumentModifiedWhen, DocumentMenuCaption, DocumentMenuClass, DocumentMenuClassHighLighted, DocumentShowInSiteMap, DocumentMenuItemHideInNavigation, DocumentMenuItemImage, DocumentMenuItemImageHighlighted, DocumentMenuItemInactive, DocumentMenuItemLeftImage, DocumentMenuItemLeftImageHighlighted, DocumentMenuItemRightImage, DocumentMenuItemRightImageHighlighted, DocumentMenuJavascript, DocumentMenuRedirectUrl, DocumentMenuRedirectToFirstChild, DocumentMenuStyle, DocumentMenuStyleHighlighted, DocumentName, DocumentUrlPath, NodeAliasPath, NodeID, NodeHasChildren, NodeClassID, NodeLevel, NodeLinkedNodeID, NodeParentID, NodeSiteID, NodeACLID, NodeSiteID, NodeOwner, NodeOrder, NodeName, DocumentSitemapSettings";
            }
        }

        /// <summary>
        /// Gets current macro resolver.
        /// </summary>
        [ToolboxItem(false)]
        public MacroResolver ContextResolver
        {
            get
            {
                if (mCurrentResolver == null)
                {
                    mCurrentResolver = PortalUIHelper.GetControlResolver(Page);
                    mCurrentResolver.Settings.EncodeResolvedValues = true;
                }
                return mCurrentResolver;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.InitComplete += Page_InitComplete;
        }


        /// <summary>
        /// Init complete handler
        /// </summary>
        void Page_InitComplete(object sender, EventArgs e)
        {
            InitControl(false);
        }


        /// <summary>
        /// Load event
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            InitControl(true);
            base.OnLoad(e);
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
                ReloadData(false);
            }
        }


        /// <summary>
        /// Reads sitemap items or menu items from the database and returns them as a DataSet.
        /// </summary>
        /// <param name="resolvedPath">Resolved path</param>
        /// <param name="siteMap">If true, sitemap items are read otherwise menu items are read</param>
        protected DataSet GetMenuItems(string resolvedPath, bool siteMap)
        {
            var liveSite = PortalContext.ViewMode.IsLiveSite();

            // Prepare where condition
            string where = WhereCondition;
            if (liveSite)
            {
                // Filter documents for navigation
                where = SqlHelper.AddWhereCondition(siteMap ? "DocumentShowInSiteMap = 1" : "DocumentMenuItemHideInNavigation = 0", where);
            }

            // Prepare order by expression
            string orderBy = String.IsNullOrEmpty(OrderBy) ? "NodeLevel, NodeOrder, DocumentName" : OrderBy;

            // Prepare class names
            var resolvedClassNames = DocumentTypeHelper.GetClassNames(ClassNames.Trim(';'));
            var classNameList = string.IsNullOrEmpty(resolvedClassNames) ? new[] { SystemDocumentTypes.MenuItem } : resolvedClassNames.Split(';');

            var query = DocumentHelper.GetDocuments()
                                      .PublishedVersion(liveSite)
                                      .Types(classNameList)
                                      .OnSite(SiteName)
                                      .Path(resolvedPath)
                                      .CombineWithDefaultCulture(CombineWithDefaultCulture)
                                      .Where(where)
                                      .OrderBy(orderBy)
                                      .NestingLevel(MaxRelativeLevel)
                                      .Published(liveSite && SelectOnlyPublished)
                                      .CheckPermissions(CheckPermissions)
                                      .Columns(GetColumns());

            // Get coupled data if there is a where condition or order by expression specified
            bool getCoupled = !string.IsNullOrEmpty(WhereCondition) || !string.IsNullOrEmpty(OrderBy);
            if (getCoupled)
            {
                // When columns defined explicitly, do not include all columns to the result
                // Include columns in the inner query to be able to define WHERE condition containing these columns
                query.WithCoupledColumns(query.SelectColumnsList.AnyColumnsDefined ? IncludeCoupledDataEnum.InnerQueryOnly : IncludeCoupledDataEnum.Complete);
            }
            else
            {
                query.WithCoupledColumns(IncludeCoupledDataEnum.None);
            }

            // Append culture
            HandleCulture(query);

            // Get documents
            var data = query.Result;
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                // Filter out documents not in navigation (handled in application because of versioned data)
                string filterWhere = siteMap ? "DocumentShowInSiteMap = 0" : "DocumentMenuItemHideInNavigation = 1";
                DataHelper.DeleteRows(data, filterWhere);
            }

            return data;
        }


        /// <summary>
        /// Returns CSS prefix for the specified menu level. If no particular prefix is specified for the level, it returns CSS class for the last defined level.
        /// </summary>
        /// <param name="level">Menu level. Use 0 for the top level</param>
        public string GetCSSPrefix(int level)
        {
            // returns CSS prefix for specified level of menu
            if (CSSPrefix.IndexOfCSafe(';') < 0)
            {
                return CSSPrefix;
            }
            if (mCSSPrefixes == null)
            {
                mCSSPrefixes = CSSPrefix.Split(';');
            }
            if (mCSSPrefixes.GetUpperBound(0) >= level)
            {
                return mCSSPrefixes[level];
            }
            return mCSSPrefixes[mCSSPrefixes.GetUpperBound(0)];
        }


        /// <summary>
        /// Returns data - either from database or from cache.
        /// </summary>
        /// <param name="siteMap">Set true for SiteMap</param>
        public DataSet GetDataSource(bool siteMap)
        {
            DataSet result = null;

            // Resolve given path.
            string resolvedPath = MacroResolver.ResolveCurrentPath(Path);

            // Try to get data from cache
            using (var cs = new CachedSection<DataSet>(ref result, CacheMinutes, true, CacheItemName, "cmsmenudatasource", CacheHelper.GetBaseCacheKey(CheckPermissions, true), SiteName, resolvedPath, CacheHelper.GetCultureCacheKey(CultureCode), CombineWithDefaultCulture, ClassNames, WhereCondition, OrderBy, MaxRelativeLevel, SelectOnlyPublished, SelectedColumns, siteMap, CheckPermissions))
            {
                if (cs.LoadData)
                {
                    // Get from the database
                    result = GetMenuItems(resolvedPath, siteMap);

                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = GetCacheDependency();
                    }

                    cs.Data = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns data - either from database or from cache.
        /// </summary>
        public DataSet GetDataSource()
        {
            mGroupedDS = null;
            return GetDataSource(false);
        }


        /// <summary>
        /// Returns list of columns which should be retrieved from database.
        /// If Columns property is not set, default columns are returned.
        /// If Columns property contains "*", "*" for all columns is returned
        /// Otherwise columns defined in Columns property are merged with default columns
        /// </summary>        
        public virtual List<string> GetColumns()
        {
            string columns;

            // Use default columns if columns property not set
            if (String.IsNullOrEmpty(SelectedColumns))
            {
                columns = DefaultColumns;
            }
            // If set to "*" get all table columns
            else if (SelectedColumns.Trim() == "*")
            {
                columns = SelectedColumns;
            }
            // Otherwise merge specified columns with default columns
            else
            {
                columns = SqlHelper.MergeColumns(SelectedColumns, DefaultColumns);
            }

            return SqlHelper.ParseColumnList(columns);
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Resolve given path.
            string resolvedPath = MacroResolver.ResolveCurrentPath(Path);

            // Create lower case path and remove trailing %
            string path = resolvedPath.TrimEnd('%').ToLowerCSafe();
            if (path != "/")
            {
                path = path.TrimEnd('/');
            }

            return "node|" + SiteName.ToLowerCSafe() + "|" + path + "|childnodes";
        }

        #endregion
    }
}
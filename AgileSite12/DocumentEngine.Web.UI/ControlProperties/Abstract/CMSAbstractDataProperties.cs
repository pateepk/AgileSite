using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Data;

using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.Base;
using CMS.Taxonomy;
using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Property container for the data display controls.
    /// </summary>
    public abstract class CMSAbstractDataProperties : CMSAbstractControlProperties, ICMSDataProperties
    {
        #region "Constants"

        private const string CMS_DATA_SOURCE_CACHE_NAME = "cmsdatasource";
        private const string CMS_DATA_SOURCE_DOCUMENT_CACHE_NAME = "cmsdatasourcedocument|";

        #endregion


        #region "Variables"

        /// <summary>
        /// Flag saying whether the data has been already loaded.
        /// </summary>
        protected bool mDataLoaded = false;


        /// <summary>
        /// Guid constant specifying current document.
        /// </summary>
        protected Guid CURRENT_DOCUMENT_GUID = new Guid("11111111-1111-1111-1111-111111111111");


        /// <summary>
        /// Filtered category code name.
        /// </summary>
        protected CategoryInfo mCategory = null;


        /// <summary>
        /// Class used for selected item
        /// </summary>
        internal string SelectedItemClass = String.Empty;


        /// <summary>
        /// Path used for selected item
        /// </summary>
        internal string SelectedItemPath = String.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether selected transformation should be used
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return IsSelectedItem(ref SelectedItemPath, ref SelectedItemClass);
            }
        }


        /// <summary>
        /// Returns true if path was automatically set.
        /// </summary>
        public bool IsAutomatic
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsAutomatic"], true);
            }
            set
            {
                ViewState["IsAutomatic"] = value;
            }
        }


        /// <summary>
        /// Returns true if path was automatically set for menu item path.
        /// </summary>
        private bool IsAutomaticMenuItemType
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsAutomaticMenuItemType"], false);
            }
            set
            {
                ViewState["IsAutomaticMenuItemType"] = value;
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
                return ValidationHelper.GetBoolean(ViewState["RelatedNodeIsOnTheLeftSide"], true);
            }
            set
            {
                ViewState["RelatedNodeIsOnTheLeftSide"] = value;
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
                return ValidationHelper.GetString(ViewState["RelationshipName"], "");
            }
            set
            {
                ViewState["RelationshipName"] = value;
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
                return ValidationHelper.GetGuid(ViewState["RelationshipWithNodeGuid"], Guid.Empty);
            }
            set
            {
                Guid guid = ValidationHelper.GetGuid(value, Guid.Empty);
                // If current document GUID set, use current document
                if (guid == CURRENT_DOCUMENT_GUID)
                {
                    PageInfo currentPageInfo = DocumentContext.CurrentPageInfo;
                    if (currentPageInfo != null)
                    {
                        guid = ValidationHelper.GetGuid(currentPageInfo.NodeGUID, Guid.Empty);
                    }
                }
                ViewState["RelationshipWithNodeGuid"] = guid;
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
                return ValidationHelper.GetString(ViewState["SelectedItemTransformationName"], "");
            }
            set
            {
                ViewState["SelectedItemTransformationName"] = value;
            }
        }


        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Path to the menu items that should be displayed in the site map.")]
        public override string Path
        {
            get
            {
                return PathInternal;
            }
            set
            {
                PathInternal = value;
                if (value != null)
                {
                    IsAutomatic = false;
                    IsAutomaticMenuItemType = false;
                }
            }
        }


        /// <summary>
        /// Path used for internal control settings. Do not disable IsAutomatic flag.
        /// </summary>
        internal string PathInternal
        {
            get
            {
                if (ViewState["Path"] == null)
                {
                    ViewState["Path"] = GetAutomaticPath();
                }
                return ValidationHelper.GetString(ViewState["Path"], "/%");
            }
            set
            {
                ViewState["Path"] = value;
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
                return ValidationHelper.GetInteger(ViewState["PageSize"], 0);
            }
            set
            {
                ViewState["PageSize"] = value;
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
        /// Gets the default column names which should be used always for CMS-related data.
        /// </summary>
        protected virtual string DefaultColumns
        {
            get
            {
                // Default data columns
                return "DocumentCulture, NodeID, NodeLinkedNodeID, NodeSiteID, ClassName, NodeLevel, NodeOrder, NodeParentID";
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
                return ValidationHelper.GetString(ViewState["CategoryName"], "");
            }
            set
            {
                ViewState["CategoryName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the class name of the current automatically used document
        /// </summary>
        internal string AutomaticClassName
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the automatic path value and set Internal statuses
        /// </summary>
        internal string GetAutomaticPath()
        {
            string path = String.Empty;
            if (CMSHttpContext.Current != null)
            {
                path = DocumentContext.CurrentAliasPath ?? String.Empty;
                if (!String.IsNullOrEmpty(path) && (DocumentContext.CurrentPageInfo != null))
                {
                    IsAutomatic = true;

                    AutomaticClassName = DocumentContext.CurrentPageInfo.ClassName;
                    if (TreePathUtils.IsMenuItemType(AutomaticClassName))
                    {
                        path = path.TrimEnd('/') + "/%";
                        IsAutomaticMenuItemType = true;
                    }
                    else
                    {
                        IsAutomaticMenuItemType = false;
                    }
                }
            }

            return path;
        }


        /// <summary>
        /// Indicates whether the currently displayed page (DocumentContext.CurrentPageInfo) is among the retrieved pages (defined by Path, WhereCondition etc.).
        /// </summary>
        /// <param name="currentNodePath">Returns the node alias path of the current page if the page is selected. Otherwise returns empty string</param>
        /// <param name="pageClass">Returns the item class name of the current page if the page is selected. Otherwise returns empty string.</param>
        private bool IsSelectedItem(ref string currentNodePath, ref string pageClass)
        {
            // Get current class
            PageInfo currentPage = DocumentContext.CurrentPageInfo;
            if (currentPage == null)
            {
                currentNodePath = String.Empty;
                return false;
            }

            // Get data from current page
            currentNodePath = currentPage.NodeAliasPath;
            pageClass = currentPage.ClassName;

            // Ensure for selected item the same behavior as for empty path
            string path = Path;
            if (path == "./%")
            {
                path = GetAutomaticPath();
            }

            // Resolve path
            string controlPath = MacroResolver.ResolveCurrentPath(path, true);
            string trimmedControlPath = controlPath.TrimEnd('%', '/');
            bool trimmedPathIsTheSame = trimmedControlPath.EqualsCSafe(currentNodePath, true);

            // The root path cannot be used as selected item path for sub items
            if ((IsAutomaticMenuItemType || !IsAutomatic) && controlPath.EndsWith("/%", StringComparison.Ordinal) && trimmedPathIsTheSame)
            {
                currentNodePath = String.Empty;
                return false;
            }

            // The path pointing out of current location cannot be selected
            if (!currentNodePath.StartsWithCSafe(trimmedControlPath, true))
            {
                currentNodePath = String.Empty;
                return false;
            }

            // Non-wildcard path cannot be selected if paths (control path & current path) aren't the same
            // EndsWithCSafe("%") is used for fast evaluation for most common scenarios. TreePathUtils.PathContainsWildcardChars uses regular expression => slower evaluation
            if (!IsAutomaticMenuItemType && !controlPath.EndsWith("%", StringComparison.Ordinal) && !controlPath.EqualsCSafe(currentNodePath, true) && !TreePathUtils.PathContainsWildcardChars(controlPath))
            {
                currentNodePath = String.Empty;
                return false;
            }

            // Indicates whether all classes are required
            bool allClasses = (String.IsNullOrEmpty(ClassNames) || (ClassNames == "*") || (ClassNames.EqualsCSafe(DataClassInfoProvider.ALL_CLASSNAMES, true)));

            string classNames = ";" + ClassNames + ";";
            if (!allClasses && ClassNames.Contains("*"))
            {
                classNames = ";" + DocumentTypeHelper.GetClassNames(ClassNames, SiteName) + ";";
            }


            if (trimmedPathIsTheSame)
            {
                // For current document, check the class name
                if (allClasses || classNames.IndexOfCSafe(";" + pageClass + ";", 0, true) >= 0)
                {
                    return true;
                }
            }
            else
            {
                // Use current page info class
                string currentClass = pageClass;

                // If where condition isn't empty try get data
                if (!String.IsNullOrEmpty(WhereCondition))
                {
                    currentClass = String.Empty;
                    string resolvedPath = currentNodePath;
                    if (IsAutomatic)
                    {
                        // Escape the path for selection in case of automatic path
                        resolvedPath = SqlHelper.EscapeLikeQueryPatterns(resolvedPath, true, false, false);
                    }
                    DataSet mds = GetDataSet(resolvedPath);
                    if (!DataHelper.DataSourceIsEmpty(mds) && (mds.Tables.Count == 1) && (mds.Tables[0].Rows.Count == 1))
                    {
                        currentClass = ValidationHelper.GetString(mds.Tables[0].Rows[0]["ClassName"], "").ToLowerCSafe();
                    }
                }

                if (!String.IsNullOrEmpty(currentClass))
                {
                    if (allClasses || classNames.IndexOfCSafe(";" + currentClass + ";", 0, true) >= 0)
                    {
                        return true;
                    }
                }
            }

            currentNodePath = string.Empty;
            return false;
        }


        /// <summary>
        /// Loads the data to the data source.
        /// </summary>
        /// <param name="dataSource">DataSource object where to load the data</param>
        /// <param name="forceReload">If true, the data is loaded even when already present</param>
        public void LoadData(ref object dataSource, bool forceReload)
        {
            int totalRecords = 0;

            LoadData(ref dataSource, forceReload, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Loads the data to the data source.
        /// </summary>
        /// <param name="dataSource">DataSource object where to load the data</param>
        /// <param name="forceReload">If true, the data is loaded even when already present</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        public void LoadData(ref object dataSource, bool forceReload, int offset, int maxRecords, ref int totalRecords)
        {
            // If already loaded, exit
            if ((mDataLoaded || (dataSource != null)) && !forceReload)
            {
                return;
            }

            // If no path specified, return
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            // Resolve given path.
            string resolvedPath = MacroResolver.ResolveCurrentPath(Path, true);

            // Ensure couple data for automatic path (non-menu item type)
            bool useCurrentClass = ((IsAutomatic && !IsAutomaticMenuItemType) && (String.IsNullOrEmpty(ClassNames)));

            if (useCurrentClass)
            {
                ClassNames = AutomaticClassName;
            }

            // Get the data
            var data = GetDataSet(resolvedPath, forceReload, offset, maxRecords, ref totalRecords);
            if (data != null)
            {
                if (data.Tables.Count == 1)
                {
                    // Single table - default view
                    dataSource = data.Tables[0].DefaultView;
                }
                else
                {
                    // Multiple tables - whole DataSet
                    dataSource = data;
                }
            }
            else if (forceReload)
            {
                dataSource = null;
            }

            if (useCurrentClass)
            {
                ClassNames = String.Empty;
            }
        }


        /// <summary>
        /// Retrieves DataSet with node according to the provided parameters.
        /// </summary>
        /// <param name="path">Node path</param>
        public DataSet GetDataSet(string path)
        {
            int totalRecords = 0;

            return GetDataSet(path, false, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Retrieves DataSet with node according to the provided parameters.
        /// </summary>
        /// <param name="path">Node path</param>
        /// <param name="forceReload">Force reload data (do not use cache)</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        public virtual DataSet GetDataSet(string path, bool forceReload, int offset, int maxRecords, ref int totalRecords)
        {
            string cacheItemName = null;
            string pagerCacheItemNamePart = (maxRecords > 0 ? "page|" + offset + "|" + maxRecords : null);

            // Prepare the cache item name
            if (String.IsNullOrWhiteSpace(CacheItemName))
            {
                // Generate default cache item name
                cacheItemName = CacheHelper.BuildCacheItemName(new object[] {
                    CMS_DATA_SOURCE_CACHE_NAME,
                    CacheHelper.GetBaseCacheKey(CheckPermissions, true),
                    SiteName,
                    path,
                    CacheHelper.GetCultureCacheKey(CultureCode),
                    CombineWithDefaultCulture,
                    ClassNames,
                    WhereCondition,
                    OrderBy,
                    MaxRelativeLevel,
                    SelectOnlyPublished,
                    RelationshipWithNodeGuid,
                    RelationshipName,
                    RelatedNodeIsOnTheLeftSide,
                    TopN,
                    SelectedColumns,
                    CheckPermissions,
                    CategoryName,
                    pagerCacheItemNamePart
                });
            }
            else
            {
                // Custom cache item name
                // Path and paging information needs to be added to the custom cache item name
                // as this method can be called multiple times during the control life cycle and the path information can vary.
                // (first call - load data, second call - check for IsSelected information with different path value)
                cacheItemName = CacheHelper.BuildCacheItemName(new string[] {
                    CacheItemName,
                    path,
                    pagerCacheItemNamePart
                });
            }

            // Try to get data from cache
            var resultData = CacheHelper.Cache(
                cs =>
                {
                    var dataQuery = GetQueryInternal(path, forceReload, offset, maxRecords);
                    var data = new object[2];
                    data[0] = dataQuery.Result;
                    data[1] = dataQuery.TotalRecords;

                    return data;
                },
                new CacheSettings(CacheMinutes, cacheItemName)
                {
                    BoolCondition = !forceReload,
                    CacheDependency = GetCacheDependency()
                }
            );

            totalRecords = (int)resultData[1];

            return (DataSet)resultData[0];
        }


        /// <summary>
        /// Returns a multi-document query which represents the data specified by the given parameters and other object properties (path, WhereCondition etc.)
        /// </summary>
        /// <param name="path">Node path</param>
        /// <param name="forceReload">Force reload data (do not use cache)</param>
        /// <param name="offset">Index of the first record to return (use for paging together with MaxRecords)</param>
        /// <param name="maxRecords">Maximum number of records to get (use for paging together with Offset). If maxRecords is zero or less, all records are returned (no paging is used)</param>
        protected virtual IMultiDocumentQuery GetQueryInternal(string path, bool forceReload, int offset, int maxRecords)
        {
            var columns = new List<string>();
            if (!string.IsNullOrEmpty(SelectedColumns) && (SelectedColumns.Trim() != SqlHelper.COLUMNS_ALL))
            {
                columns = SqlHelper.ParseColumnList(SqlHelper.MergeColumns(SelectedColumns, DefaultColumns));
            }
            var liveSite = PortalContext.ViewMode.IsLiveSite();
            var query = DocumentHelper.GetDocuments()
                                      .PublishedVersion(liveSite)
                                      .Types(ClassNames.Trim(';').Split(';'))
                                      .OnSite(SiteName)
                                      .Path(path)
                                      .CombineWithDefaultCulture(CombineWithDefaultCulture)
                                      .Where(WhereCondition)
                                      .OrderBy(OrderBy)
                                      .NestingLevel(MaxRelativeLevel)
                                      .Published(liveSite && SelectOnlyPublished)
                                      .InRelationWith(RelationshipWithNodeGuid, RelationshipName, RelatedNodeIsOnTheLeftSide ? RelationshipSideEnum.Left : RelationshipSideEnum.Right)
                                      .InEnabledCategories(CategoryName)
                                      .FilterDuplicates(FilterOutDuplicates)
                                      .TopN(TopN)
                                      .CheckPermissions(CheckPermissions)
                                      .Columns(columns);

            // When columns defined explicitly, do not include all columns to the result
            // Include columns in the inner query to be able to define WHERE condition containing these columns
            query.WithCoupledColumns(query.SelectColumnsList.AnyColumnsDefined ? IncludeCoupledDataEnum.InnerQueryOnly : IncludeCoupledDataEnum.Complete);

            // Don't modify order when order by is defined in webpart properties 
            if (string.IsNullOrEmpty(OrderBy))
            {
                // Ensure correct order for ad-hoc relationships 
                query = ApplyRelationshipOrder(query);
            }

            // Append culture
            HandleCulture(query);

            // Paging
            query.Offset = offset;
            query.MaxRecords = maxRecords;

            return query;
        }


        /// <summary>
        /// Returns document of specified path. It optionally uses cache.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        public TreeNode GetDocument(string aliasPath)
        {
            TreeNode result = null;

            // If not live site mode, use different approach - get the document version
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                result = TreeProvider.SelectSingleNode(SiteName, aliasPath, CultureCode, CombineWithDefaultCulture, null, false);
                if (result != null)
                {
                    result = DocumentHelper.GetDocument(result, TreeProvider);
                }
            }
            else
            {
                // Build the cache name
                string useCacheItemName = CMS_DATA_SOURCE_DOCUMENT_CACHE_NAME + CacheHelper.GetCacheItemName(CacheItemName, CacheHelper.GetBaseCacheKey(CheckPermissions, true), SiteName, aliasPath, CultureCode, CombineWithDefaultCulture, SelectOnlyPublished);

                // Try to get data from cache
                using (var cs = new CachedSection<TreeNode>(ref result, CacheMinutes, true, useCacheItemName))
                {
                    if (cs.LoadData)
                    {
                        // Get the data
                        result = TreeProvider.SelectSingleNode(SiteName, aliasPath, CultureCode, CombineWithDefaultCulture, null, SelectOnlyPublished);

                        // Save the result to the cache
                        if (cs.Cached)
                        {
                            cs.CacheDependency = GetCacheDependency();
                        }

                        cs.Data = result;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public void ClearCache()
        {
            // If no path specified, return
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            // Resolve given path.
            string path = MacroResolver.ResolveCurrentPath(Path);

            string where = WhereCondition;

            // Prepare the cache item name
            string useCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, CMS_DATA_SOURCE_CACHE_NAME, CacheHelper.GetBaseCacheKey(CheckPermissions, true), SiteName, path, CultureCode, CombineWithDefaultCulture, ClassNames, where, OrderBy, MaxRelativeLevel, SelectOnlyPublished, RelationshipWithNodeGuid, RelationshipName, RelatedNodeIsOnTheLeftSide, TopN, SelectedColumns);
            CacheHelper.ClearCache(useCacheItemName);

            // Remove cached document
            string nodesPath = Path.TrimEnd('%', '/');
            if (String.IsNullOrEmpty(nodesPath))
            {
                nodesPath = DocumentContext.CurrentAliasPath;
            }

            useCacheItemName = CMS_DATA_SOURCE_DOCUMENT_CACHE_NAME + CacheHelper.GetCacheItemName(CacheItemName, CacheHelper.GetBaseCacheKey(CheckPermissions, true), SiteName, nodesPath, CultureCode, CombineWithDefaultCulture, SelectOnlyPublished);
            CacheHelper.ClearCache(useCacheItemName);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Joins query with relationship objects to apply relationship order for ad-hoc (ordered) relationships.
        /// </summary>
        /// <param name="query">MultiDocumentQuery to be joined with ordered relationships.</param>
        private MultiDocumentQuery ApplyRelationshipOrder(MultiDocumentQuery query)
        {
            var relationshipNameInfo = ProviderHelper.GetInfoByName("cms.relationshipname", RelationshipName);

            if ((relationshipNameInfo != null) && (relationshipNameInfo.GetBooleanValue("RelationshipNameIsAdHoc", false)))
            {
                int nodeId = TreePathUtils.GetNodeIdByNodeGUID(RelationshipWithNodeGuid, SiteName);

                if (nodeId > 0)
                {
                    int relationshipNameId = relationshipNameInfo.GetIntegerValue("RelationshipNameID", 0);

                    query = RelationshipInfoProvider.ApplyRelationshipOrderData(query, nodeId, relationshipNameId);
                }
            }

            return query;
        }

        #endregion
    }
}
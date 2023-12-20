using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.DocumentEngine.Query;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document specific properties for document queries
    /// </summary>
    public class DocumentQueryProperties
    {
        #region "Variables"

        /// <summary>
        /// Setting indicating if prioritized queries should be automatically optimized for higher scalability. Defaults to true or value of web.config key CMSOptimizePrioritizedQueries
        /// </summary>
        private static readonly BoolAppSetting mOptimizePrioritizedQueries = new BoolAppSetting("CMSOptimizePrioritizedDocumentQueries", true);


        /// <summary>
        /// Setting indicating if non-prioritized queries should be automatically optimized for higher scalability. Defaults to false or value of web.config key CMSOptimizeNonPrioritizedQueries
        /// </summary>
        private static readonly BoolAppSetting mOptimizeNonPrioritizedQueries = new BoolAppSetting("CMSOptimizeNonPrioritizedDocumentQueries");


        private string mClassName;
        private List<string> mCultures;
        private HashSet<string> mPaths;
        private HashSet<string> mExcludedPaths;
        private HashSet<int> mSiteIDs;
        private string[] mCultureList;

        private TreeProvider mTreeProvider;

        private bool? mSelectOnlyPublished;

        private string mSiteName;
        private string mPreferredCultureCode;
        private string mDefaultCultureCode;

        private bool? mCombineWithDefaultCulture;
        private bool? mCombineWithAnyCulture;
        private bool? mEnsureExtraColumns;
        private bool? mIsLastVersion;
        private bool? mFilterDuplicates;
        private bool? mCheckPermissions;
        private bool? mAllCultures;

        private Guid? mRelationshipNodeGUID;
        private RelationshipSideEnum? mRelationshipSide;
        private int? mNestingLevel;
        private ICollection<IDocumentQueryFilter> mExternalFilters;

        internal bool? mIncludeCoupledColumns;


        /// <summary>
        /// Document instance of specified type.
        /// </summary>
        private TreeNode mDocument;


        private DocumentQuerySourceSettings mSourceSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Source settings
        /// </summary>
        internal DocumentQuerySourceSettings SourceSettings
        {
            get
            {
                return mSourceSettings ?? (mSourceSettings = new DocumentQuerySourceSettings());
            }
            set
            {
                mSourceSettings = value;
            }
        }


        /// <summary>
        /// Gets or sets if complex queries should be optimized for higher scalability.
        /// Defaults to false, or value in web.config key CMSOptimizeQueriesForScalability
        /// </summary>
        internal bool? OptimizeForScalability
        {
            get;
            set;
        }


        /// <summary>
        /// Document instance of the specified type.
        /// </summary>
        internal TreeNode Document
        {
            get
            {
                if (mDocument == null)
                {
                    CheckParentQuery();

                    // Get the document info
                    var className = IsMultiQuery ? null : TreeNodeProvider.GetClassName(ParentQuery.ObjectType);

                    mDocument = TreeNode.New(className, TreeProvider);
                    mDocument.IsLastVersion = IsLastVersion;
                }

                return mDocument;
            }
            set
            {
                mDocument = value;
            }
        }


        /// <summary>
        /// Class name representing page type
        /// </summary>
        internal string ClassName
        {
            get
            {
                if (mClassName != null)
                {
                    return mClassName;
                }

                CheckParentQuery();

                return ParentQuery.ClassName;
            }
            set
            {
                mClassName = value;
            }
        }


        /// <summary>
        /// Indicates if duplicate document are filtered from the result. This means that linked documents are not retrieved, if there is the original document already included in the results.
        /// </summary>
        public bool FilterDuplicates
        {
            get
            {
                return mFilterDuplicates.HasValue && mFilterDuplicates.Value;
            }
            set
            {
                mFilterDuplicates = value;
            }
        }


        /// <summary>
        /// Defines node GUID of the related document. Only document in relation with this document will be included in the results.
        /// </summary>
        public Guid RelationshipNodeGUID
        {
            get
            {
                return mRelationshipNodeGUID ?? Guid.Empty;
            }
            set
            {
                mRelationshipNodeGUID = value;
            }
        }


        /// <summary>
        /// Defines name of the relationship. If not provided documents from all relationships will be retrieved.
        /// </summary>
        public string RelationshipName
        {
            get;
            set;
        }


        /// <summary>
        /// Defines side of the related document within the relation. Both sides are used by default.
        /// </summary>
        public RelationshipSideEnum RelationshipSide
        {
            get
            {
                return mRelationshipSide ?? RelationshipSideEnum.Both;
            }
            set
            {
                mRelationshipSide = value;
            }
        }


        /// <summary>
        /// Limits documents to a specified nesting level. (Applies only when one node alias path for multiple documents is provided.)
        /// </summary>
        public int NestingLevel
        {
            get
            {
                return mNestingLevel ?? TreeProvider.ALL_LEVELS;
            }
            set
            {
                mNestingLevel = value;
            }
        }


        /// <summary>
        /// If true, only published documents are retrieved.
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return mSelectOnlyPublished.HasValue && mSelectOnlyPublished.Value;
            }
            set
            {
                mSelectOnlyPublished = value;
            }
        }


        /// <summary>
        /// If true, automatic extra columns are ensured for the query (column for version data application, security check required columns etc.).
        /// </summary>
        public bool EnsureExtraColumns
        {
            get
            {
                return !mEnsureExtraColumns.HasValue || mEnsureExtraColumns.Value;
            }
            set
            {
                mEnsureExtraColumns = value;
            }
        }


        /// <summary>
        /// If true, the last version of the documents is retrieved.
        /// </summary>
        public bool IsLastVersion
        {
            get
            {
                return mIsLastVersion.HasValue && mIsLastVersion.Value;
            }
            set
            {
                mIsLastVersion = value;
            }
        }


        /// <summary>
        /// Indicates if the documents should be filtered based on current user permissions.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions.HasValue && mCheckPermissions.Value;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Indicates if the documents should be combined with the default language version if the specific one doesn't exist.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                if (!mCombineWithDefaultCulture.HasValue)
                {
                    return GetCombineWithDefaultCultureInternal();
                }

                return mCombineWithDefaultCulture.Value;
            }
            set
            {
                mCombineWithDefaultCulture = value;
            }
        }


        /// <summary>
        /// Indicates if the documents should be combined with any language version if the specific one doesn't exist.
        /// </summary>
        public bool CombineWithAnyCulture
        {
            get
            {
                return mCombineWithAnyCulture.HasValue && mCombineWithAnyCulture.Value;
            }
            set
            {
                mCombineWithAnyCulture = value;
            }
        }


        /// <summary>
        /// Indicates if all culture versions of the documents should be returned in the result.
        /// </summary>
        public bool AllCultures
        {
            get
            {
                return mAllCultures.HasValue && mAllCultures.Value;
            }
            set
            {
                mAllCultures = value;
            }
        }


        /// <summary>
        /// Indicates if properties are used for a multi-query.
        /// </summary>
        public bool IsMultiQuery
        {
            get;
            protected set;
        }


        /// <summary>
        /// Prioritized list of document cultures which should be included in the result.
        /// </summary>
        public List<string> Cultures
        {
            get
            {
                return mCultures ?? (mCultures = new List<string>());
            }
            set
            {
                mCultures = value;
            }
        }


        /// <summary>
        /// List of document paths which should be included in the result.
        /// </summary>
        public HashSet<string> Paths
        {
            get
            {
                return mPaths ?? (mPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
            set
            {
                mPaths = value;
            }
        }


        /// <summary>
        /// List of site IDs on which should be the documents searched.
        /// </summary>
        public HashSet<int> SiteIDs
        {
            get
            {
                return mSiteIDs ?? (mSiteIDs = new HashSet<int>());
            }
            set
            {
                mSiteIDs = value;
            }
        }


        /// <summary>
        /// Provides site name for the current query (If there is only single site specified.)
        /// </summary>
        public string SiteName
        {
            get
            {
                if ((mSiteName == null) && (SiteIDs.Count == 1))
                {
                    // Use site name if there are documents only from one site
                    mSiteName = SiteInfoProvider.GetSiteName(SiteIDs.First());
                }

                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Provides default culture code for the current query
        /// </summary>
        public string DefaultCultureCode
        {
            get
            {
                return mDefaultCultureCode ?? GetDefaultCultureCodeInternal();
            }
            set
            {
                mDefaultCultureCode = value;
            }
        }


        /// <summary>
        /// Preferred culture code to use when none set.
        /// </summary>
        public string PreferredCultureCode
        {
            get
            {
                return mPreferredCultureCode ?? GetPreferredCultureInternal();
            }
            set
            {
                mPreferredCultureCode = value;
            }
        }


        /// <summary>
        /// List of document paths which should be excluded from the result.
        /// </summary>
        public HashSet<string> ExcludedPaths
        {
            get
            {
                return mExcludedPaths ?? (mExcludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
            set
            {
                mExcludedPaths = value;
            }
        }


        /// <summary>
        /// Instance of tree provider to use to parametrize documents retrieving.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// If true, the query gets coupled data
        /// </summary>
        internal bool IncludeCoupledColumns
        {
            get
            {
                if (mIncludeCoupledColumns != null)
                {
                    return mIncludeCoupledColumns.Value;
                }

                // Multi-query does not return coupled columns by default. For multi-query coupled columns are included when a type is specified through individual single queries.
                if (IsMultiQuery)
                {
                    return false;
                }

                // Coupled columns are included by default only by single query is class name is specified
                return IsTypeSpecific();
            }
            set
            {
                mIncludeCoupledColumns = value;
            }
        }


        /// <summary>
        /// Parent document query
        /// </summary>
        public IDocumentQuery ParentQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if default values of the query parameterization should be initialized from settings
        /// </summary>
        internal bool ApplyDefaultSettings
        {
            get;
            set;
        }


        /// <summary>
        /// List of versioned columns which should not be applied
        /// </summary>
        public IEnumerable<string> ExcludedVersionedColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of external filters represented by objects which return <see cref="IWhereCondition"/> from its inner state.
        /// Each type of the filter can be used only once.
        /// </summary>
        internal ICollection<IDocumentQueryFilter> ExternalFilters
        {
            get
            {
                return mExternalFilters ?? (mExternalFilters = new List<IDocumentQueryFilter>());
            }
        }

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Ordered and filtered list of cultures to use in query
        /// </summary>
        protected string[] CultureList
        {
            get
            {
                if (mCultureList == null)
                {
                    // Use local copy for modification
                    var cultures = new List<string>(Cultures.Where(c => !string.IsNullOrEmpty(c)));

                    // Use preferred culture, if none specified
                    EnsurePreferredCulture(cultures);

                    // Include default content culture
                    if (CombineWithDefaultCulture || CombineWithAnyCulture)
                    {
                        cultures.Add(DefaultCultureCode);
                    }
                    mCultureList = cultures.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                }

                return mCultureList;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isMultiQuery">If true, the properties are created within the multi-query instance</param>
        public DocumentQueryProperties(bool isMultiQuery = false)
        {
            IsMultiQuery = isMultiQuery;
        }

        #endregion


        #region "Path methods"

        /// <summary>
        /// Filters the data to include only documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        public void Path(params string[] paths)
        {
            Paths.AddRangeToSet(paths);
        }


        /// <summary>
        /// Filters the data to include only documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define selection scope</param>
        public void Path(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            // Ensure correct path format
            switch (type)
            {
                case PathTypeEnum.Single:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    Paths.Add(path);
                    break;

                case PathTypeEnum.Children:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    Paths.Add(TreePathUtils.EnsureChildPath(path));
                    break;

                case PathTypeEnum.Section:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    Paths.Add(TreePathUtils.EnsureChildPath(path));
                    Paths.Add(TreePathUtils.EnsureSinglePath(path));
                    break;

                default:
                    Paths.Add(path);
                    break;
            }
        }


        /// <summary>
        /// Filters the data to exclude documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        public void ExcludePath(params string[] paths)
        {
            ExcludedPaths.AddRangeToSet(paths);
        }


        /// <summary>
        /// Filters the data to exclude documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define excluded scope</param>
        public void ExcludePath(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            // Ensure correct path format
            switch (type)
            {
                case PathTypeEnum.Single:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    ExcludedPaths.Add(path);
                    break;

                case PathTypeEnum.Children:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    ExcludedPaths.Add(TreePathUtils.EnsureChildPath(path));
                    break;

                case PathTypeEnum.Section:
                    path = SqlHelper.EscapeLikeQueryPatterns(path);
                    ExcludedPaths.Add(TreePathUtils.EnsureChildPath(path));
                    ExcludedPaths.Add(TreePathUtils.EnsureSinglePath(path));
                    break;

                default:
                    ExcludedPaths.Add(path);
                    break;
            }
        }


        /// <summary>
        /// Gets where condition based on allowed paths
        /// </summary>
        public WhereCondition GetAllowedPathWhereCondition()
        {
            var where = new WhereCondition();
            var combined = (ExcludedPaths.Count > 0) || (Paths.Count > 1);
            foreach (var path in Paths)
            {
                where.Or().Where(TreePathUtils.GetAliasPathCondition(path, false, combined));
            }

            return where;
        }


        /// <summary>
        /// Gets where condition based on excluded paths
        /// </summary>
        public WhereCondition GetExcludedPathWhereCondition()
        {
            var where = new WhereCondition();
            foreach (var path in ExcludedPaths)
            {
                where.And().Where(TreePathUtils.GetAliasPathCondition(path, true));
            }

            return where;
        }

        #endregion


        #region "Culture methods"

        /// <summary>
        /// Filters the data to include only documents translated to given culture(s).
        /// </summary>
        /// <param name="cultures">List of document cultures</param>
        public void Culture(params string[] cultures)
        {
            Cultures.AddRange(cultures);
        }


        /// <summary>
        /// Gets where condition based on list of requested cultures
        /// </summary>
        public WhereCondition GetCultureWhereCondition()
        {
            // Prepare culture where
            var cultureWhere = new WhereCondition();

            // No condition for all cultures or when the result should be combined with any culture
            if (AllCultures || CombineWithAnyCulture)
            {
                return cultureWhere;
            }

            // Include all cultures
            foreach (var culture in CultureList)
            {
                cultureWhere.Or().Where("DocumentCulture", QueryOperator.Equals, culture);
            }

            return cultureWhere;
        }


        /// <summary>
        /// Returns a where condition which combines where conditions of all the external filters using AND operand.
        /// </summary>
        /// <exception cref="InvalidOperationException">Each type of an external filter can be used only once.</exception>
        private WhereCondition GetExternalFiltersWhereCondition()
        {
            WhereCondition where = null;

            if (ExternalFilters.Any())
            {
                var duplicatedFiltres = ExternalFilters
                                            .GroupBy(f => f.GetType())
                                            .Select(f => new {
                                                TypeName = f.Key,
                                                TypeCount = f.Count()
                                            })
                                            .Where(f => f.TypeCount > 1);

                var firstDuplicate = duplicatedFiltres.FirstOrDefault();
                if (firstDuplicate != null)
                {
                    string message = String.Format("Filter {0} can be used only once.", firstDuplicate.TypeName);
                    throw new InvalidOperationException(message);
                }

                where = new WhereCondition();

                // Get Where condition from external properties' state
                ExternalFilters.ToList().ForEach(
                    filter => where.And(filter.GetWhereCondition(this))
                );
            }

            return where;
        }


        /// <summary>
        /// Adds culture priority column to data query settings to get the most relevant document culture version
        /// </summary>
        /// <param name="settings">Settings</param>
        public void AddCulturePriorityColumn(DataQuerySettings settings)
        {
            var cultureRowNumber = GetCulturePriorityColumn(CultureList, "NodeID");

            settings.AddFilterColumn(cultureRowNumber);
        }


        /// <summary>
        /// Gets the culture priority column for the query. Prioritizes cultures by the given list.
        /// Rest of the cultures are prioritized alphabetically by culture name.
        /// </summary>
        internal static RowNumberColumn GetCulturePriorityColumn(IEnumerable<string> cultures, string partitionByColumn)
        {
            // List of culture where conditions for the priority column (in case of combine with default culture)
            var culturePriorityWheres = new List<string>();

            var cw = new WhereCondition();
            var parameters = new QueryDataParameters();

            // Prepare culture where for each requested culture
            foreach (var culture in cultures)
            {
                cw.NewWhere().Where("DocumentCulture", QueryOperator.Equals, culture);

                var where = cw.WhereCondition;
                where = parameters.IncludeDataParameters(cw.Parameters, where);

                culturePriorityWheres.Add(where);
            }

            // Add culture priority column
            var culturePriority = SqlHelper.GetCaseOrderBy(culturePriorityWheres);
            culturePriority = SqlHelper.AddOrderBy(culturePriority, "DocumentCulture");

            var cultureRowNumber = new RowNumberColumn(DocumentSystemColumns.CULTURE_PRIORITY, culturePriority)
            {
                PartitionBy = partitionByColumn
            };

            cultureRowNumber.OrderBy = cultureRowNumber.IncludeDataParameters(parameters, cultureRowNumber.OrderBy);

            return cultureRowNumber;
        }


        /// <summary>
        /// Adds duplicates priority column to data query settings to get only original documents
        /// </summary>
        /// <param name="settings">Settings</param>
        public void AddDuplicatesPriorityColumn(DataQuerySettings settings)
        {
            // Add culture priority column
            var duplicatesPriority = SqlHelper.GetCaseOrderBy("NodeLinkedNodeID IS NULL");
            var duplicatesRowNumber = new RowNumberColumn(SystemColumns.DUPLICATE_PRIORITY, duplicatesPriority)
            {
                PartitionBy = "DocumentNodeID, DocumentCulture"
            };

            settings.AddFilterColumn(duplicatesRowNumber);
        }

        #endregion


        #region "Site methods"

        /// <summary>
        /// Filters the data to include documents only on the given site(s).
        /// </summary>
        /// <param name="siteIds">Site ID(s)</param>
        public void OnSite(params int[] siteIds)
        {
            SiteIDs.AddRangeToSet(siteIds);
        }

        #endregion


        #region "Where condition methods"

        /// <summary>
        /// Gets where condition for page type representing container
        /// </summary>
        public WhereCondition GetContainerTypeWhereCondition()
        {
            // No condition for general query
            if (!IsTypeSpecific())
            {
                return null;
            }

            var type = DataClassInfoProvider.GetDataClassInfo(ClassName);

            // No condition for coupled class, condition is already included in the JOIN expression
            if (type.ClassIsCoupledClass)
            {
                return null;
            }

            return DocumentQuerySourceHelper.GetClassWhereCondition(ClassName);
        }


        /// <summary>
        /// Gets where condition for all restrictions based on tree node data
        /// </summary>
        public WhereCondition GetNodeDataWhereCondition()
        {
            var where =
                new WhereCondition()
                    .Where(GetPublishedWhereCondition())
                    .Where(GetContainerTypeWhereCondition())
                    .Where(GetAllowedPathWhereCondition())
                    .Where(GetExcludedPathWhereCondition())
                    .Where(GetNestingLevelWhereCondition())
                    .Where(GetRelationshipWhereCondition());

            return where;
        }


        /// <summary>
        /// Gets where condition to retrieve only documents in specified nesting level (The first specified path is taken as the referring one.)
        /// </summary>
        public WhereCondition GetNestingLevelWhereCondition()
        {
            // Nesting level defined
            if (NestingLevel >= 0)
            {
                // Use all documents path or first one specified
                var path = Paths.FirstOrDefault() ?? TreeProvider.ALL_DOCUMENTS;
                int baseLevel = path.Split('/').GetUpperBound(0);
                int nodeLevel = baseLevel + NestingLevel - 1;

                return new WhereCondition().WhereLessOrEquals("NodeLevel", nodeLevel);
            }

            return null;
        }


        /// <summary>
        /// Gets where condition to retrieve only published documents based on settings
        /// </summary>
        public WhereCondition GetPublishedWhereCondition()
        {
            if (SelectOnlyPublished)
            {
                return TreeProvider.GetPublishedWhereCondition();
            }

            return null;
        }


        /// <summary>
        /// Gets where condition to retrieve only documents in relationship defined by the properties.
        /// </summary>
        public WhereCondition GetRelationshipWhereCondition()
        {
            if (RelationshipNodeGUID == Guid.Empty)
            {
                return null;
            }

            var where = new WhereCondition();

            if ((RelationshipSide == RelationshipSideEnum.Both) || (RelationshipSide == RelationshipSideEnum.Left))
            {
                var q = new ObjectQuery<RelationshipInfo>()
                    .From(
                        new QuerySource("CMS_Relationship").LeftJoin("CMS_RelationshipName", "RelationshipNameID", "RelationshipNameID")
                    )
                    .Columns("RightNodeID")
                    .Where(new WhereCondition()
                        .WhereIn("LeftNodeID", new IDQuery(DocumentNodeDataInfo.OBJECT_TYPE, useObjectTypeCondition: false)
                            .WhereEquals("NodeGUID", RelationshipNodeGUID)));

                if (RelationshipName != null)
                {
                    q.WhereEquals("RelationshipName", RelationshipName);
                }

                where.WhereIn("NodeID", q);
            }

            if ((RelationshipSide == RelationshipSideEnum.Both) || (RelationshipSide == RelationshipSideEnum.Right))
            {
                var q = new ObjectQuery<RelationshipInfo>()
                    .From(
                        new QuerySource("CMS_Relationship").LeftJoin("CMS_RelationshipName", "RelationshipNameID", "RelationshipNameID")
                    )
                    .Columns("LeftNodeID")
                    .Where(new WhereCondition()
                        .WhereIn("RightNodeID", new IDQuery(DocumentNodeDataInfo.OBJECT_TYPE, useObjectTypeCondition: false)
                            .WhereEquals("NodeGUID", RelationshipNodeGUID)));

                if (RelationshipName != null)
                {
                    q.WhereEquals("RelationshipName", RelationshipName);
                }

                where.Or().WhereIn("NodeID", q);
            }

            return where;
        }


        /// <summary>
        /// Gets where condition based on multi-query settings and list of document types
        /// </summary>
        /// <param name="types">List of document types</param>
        public string GetClassesWhereCondition(IEnumerable<string> types)
        {
            var classes = types.ToList();
            if (classes.Count > 0)
            {
                return new WhereCondition()
                    .WhereIn("ClassName", classes)
                    .ToString(true);
            }

            return null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks if the parent query is set. If not, throws an exception
        /// </summary>
        private void CheckParentQuery()
        {
            if (ParentQuery == null)
            {
                throw new InvalidOperationException("[DocumentQueryProperties.ClassName]: ParentQuery property must be set.");
            }
        }


        /// <summary>
        /// Ensures that there is no restriction applied for the result.
        /// </summary>
        public void All()
        {
            AllCultures = true;
            FilterDuplicates = false;
            SelectOnlyPublished = false;
            NestingLevel = TreeProvider.ALL_LEVELS;
            RelationshipNodeGUID = Guid.Empty;
            CheckPermissions = false;

            mSiteName = null;

            Paths.Clear();
            ExcludedPaths.Clear();
            Cultures.Clear();
            SiteIDs.Clear();
            ExternalFilters.Clear();
        }


        /// <summary>
        /// Ensures default state of settings for the result.
        /// </summary>
        public void Default()
        {
            mAllCultures = null;
            mFilterDuplicates = null;
            mSelectOnlyPublished = null;
            mNestingLevel = null;

            mRelationshipNodeGUID = null;
            RelationshipName = null;
            mRelationshipSide = null;

            mEnsureExtraColumns = null;
            mIsLastVersion = null;
            mCheckPermissions = null;

            mSiteName = null;
            mDefaultCultureCode = null;
            mPreferredCultureCode = null;
            mCombineWithDefaultCulture = null;
            mCombineWithAnyCulture = null;

            Paths.Clear();
            ExcludedPaths.Clear();
            Cultures.Clear();
            SiteIDs.Clear();
            ExternalFilters.Clear();
        }


        /// <summary>
        /// Applies these properties to the given data query settings
        /// </summary>
        /// <param name="settings">Query settings</param>
        public void ApplyToSettings(DataQuerySettings settings)
        {
            if (!IsMultiQuery)
            {
                // Ensure required columns
                EnsurePriorityColumns(settings);
            }

            // Ensure where condition
            EnsureWhereCondition(settings);
        }


        /// <summary>
        /// Ensures where condition based on query properties
        /// </summary>
        /// <param name="settings">Query settings</param>
        public void EnsureWhereCondition(DataQuerySettings settings)
        {
            // Apply restrictions only for stand alone query
            if (IsMultiQuery)
            {
                return;
            }

            // Create complete where condition
            var where = GetNodeDataWhereCondition();
            where.And(GetCultureWhereCondition());
            where.And(GetExternalFiltersWhereCondition());

            settings.Where(where);
        }


        /// <summary>
        /// Retrieves the data and filters results by permissions if check permissions is allowed.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="baseGetDataMethod">Method which is used for retrieving data without check permissions</param>
        /// <param name="setTotalRecords">Action to set total records after results are filtred by permissions</param>
        internal DataSet GetDataInternal(IDocumentQuery query, Func<DataSet> baseGetDataMethod, Action<int> setTotalRecords)
        {
            DataSet data;

            if (!CheckPermissions)
            {
                data = baseGetDataMethod();
            }
            else
            {
                int totalRecords = 0;
                data = FilterResultsByPermissions(query, ref totalRecords);

                setTotalRecords(totalRecords);
            }

            // Apply versioned data
            ApplyVersionData(data);

            return data;
        }



        /// <summary>
        /// Filters query results by permissions based on settings
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns DataSet with results</returns>
        /// <param name="totalRecords">Returns the total records number</param>
        private DataSet FilterResultsByPermissions(IDocumentQuery query, ref int totalRecords)
        {
            DataSet data = null;

            if (CheckPermissions)
            {
                // Clone query for results
                var resultsQuery = CloneForPermissionFiltering(query);
                resultsQuery.Offset = 0;
                resultsQuery.MaxRecords = 0;

                // Get query maximal requested records
                var maxRecords = query.MaxRecords;

                // Handle top N records or paged query
                if ((query.TopNRecords > 0) || (maxRecords > 0))
                {
                    // Prepare ID query properties
                    var idsQuery = CloneForPermissionFiltering(query);
                    idsQuery.ExpandToFullSourceSet();
                    idsQuery.SelectColumnsList.Load(DocumentColumnLists.SECURITYCHECK_REQUIRED_COLUMNS);

                    var targetTopN = query.TopNRecords;

                    var documentIds = new List<int>();
                    var nodeIds = new List<int>();

                    // Top N is not specified, get all documents
                    if (targetTopN <= 0)
                    {
                        var idsData = idsQuery.Result;
                        idsData = TreeSecurityProvider.FilterDataSetByPermissions(idsData, NodePermissionsEnum.Read, TreeProvider.UserInfo);

                        // Get IDs
                        foreach (DataRow row in idsData.Tables[0].Rows)
                        {
                            documentIds.Add((int)row["DocumentID"]);
                            nodeIds.Add((int)row["NodeID"]);
                        }
                    }
                    // Get Top N documents in batches
                    else
                    {
                        var totalCount = 0;
                        var i = 0;

                        // Get twice more data (assumption) - data will be filtered
                        var records = targetTopN * 2;

                        // Maximal attempts to get complete data
                        const int MAX_ATTEMPTS = 3;

                        do
                        {
                            // Get batch data
                            var gap = targetTopN - totalCount;
                            idsQuery.Offset = i * records;
                            idsQuery.MaxRecords = records;

                            // Ensure default order by
                            if (string.IsNullOrEmpty(idsQuery.OrderByColumns))
                            {
                                idsQuery.OrderByColumns = "DocumentID";
                            }

                            var idsData = idsQuery.Result;
                            if (DataHelper.DataSourceIsEmpty(idsData))
                            {
                                // No additional items were found, no need to continue
                                break;
                            }

                            // Filter permissions
                            idsData = TreeSecurityProvider.FilterDataSetByPermissions(idsData, NodePermissionsEnum.Read, TreeProvider.UserInfo, topN: gap);
                            var count = DataHelper.GetItemsCount(idsData);
                            var limitResults = count > gap;
                            if (limitResults)
                            {
                                DataHelper.RestrictRows(idsData, gap);
                            }
                            totalCount += limitResults ? gap : count;

                            // Get IDs
                            foreach (DataRow row in idsData.Tables[0].Rows)
                            {
                                documentIds.Add((int)row["DocumentID"]);
                                nodeIds.Add((int)row["NodeID"]);
                            }

                            ++i;
                        } while ((i < MAX_ATTEMPTS) && (totalCount != targetTopN));
                    }

                    // Store total records count
                    totalRecords = documentIds.Count;

                    // Process paging
                    if (maxRecords > 0)
                    {
                        // Data not available for specified page
                        if (query.Offset >= documentIds.Count)
                        {
                            documentIds.Clear();
                            nodeIds.Clear();
                        }
                        // Get specific page
                        else
                        {
                            int maxIndex = documentIds.Count - 1;
                            int endIndex = query.Offset + maxRecords - 1;
                            int diff = endIndex - maxIndex;
                            int count = diff > 0 ? maxRecords - diff : maxRecords;

                            documentIds = documentIds.GetRange(query.Offset, count);
                            nodeIds = nodeIds.GetRange(query.Offset, count);
                        }
                    }

                    // Get data if there is at least one document
                    if (documentIds.Count > 0)
                    {
                        // Get new where condition - use document ID and node ID because of linked documents
                        var where = new WhereCondition().WhereIn("DocumentID", documentIds).WhereIn("NodeID", nodeIds);

                        var pageCondition = resultsQuery.IncludeDataParameters(where.Parameters, where.WhereCondition);
                        resultsQuery.WhereCondition = SqlHelper.AddWhereCondition(resultsQuery.WhereCondition, pageCondition);

                        data = resultsQuery.Result;
                    }
                }
                else
                {
                    // Ensure columns required to check the permissions
                    var columns = resultsQuery.SelectColumnsList;

                    if (!columns.ReturnsAllColumns)
                    {
                        columns.AddRangeUnique(SqlHelper.ParseColumnList(DocumentColumnLists.SECURITYCHECK_REQUIRED_COLUMNS), false);
                    }

                    // Check permissions
                    data = TreeSecurityProvider.FilterDataSetByPermissions(resultsQuery.Result, NodePermissionsEnum.Read, TreeProvider.UserInfo);
                    SqlHelper.ProcessPagedResults(data, ref totalRecords);
                }
            }
            else
            {
                // Get complete result
                data = query.Result;
                totalRecords = query.TotalRecords;
            }

            return data;
        }


        /// <summary>
        /// Applies versioned data
        /// </summary>
        /// <param name="data">source data</param>
        internal void ApplyVersionData(DataSet data)
        {
            if (!IsLastVersion)
            {
                return;
            }

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            var manager = VersionManager.GetInstance(TreeProvider);

            // Coupled columns may be included in result if either included explicitly or not specified explicitly in multi-query
            var containsCoupledColumns = IncludeCoupledColumns || (mIncludeCoupledColumns == null) && IsMultiQuery;

            manager.ApplyVersionData(data, containsCoupledColumns, ExcludedVersionedColumns);
        }


        /// <summary>
        /// Gets clone of the original query for results
        /// </summary>
        /// <param name="query">Original query</param>
        private IDataQuery CloneForPermissionFiltering(IDocumentQuery query)
        {
            var newQuery = (IDocumentQuery)query.CloneObject();

            if (newQuery != null)
            {
                var properties = newQuery.Properties;

                // Disable check permissions to avoid re-checking of permissions on the filtering data
                properties.CheckPermissions = false;

                // Turn off applying versioned data
                properties.IsLastVersion = false;

                // Disable ensuring of extra columns to not include columns unnecessary for permission filtering
                properties.EnsureExtraColumns = false;
            }

            return newQuery;
        }


        /// <summary>
        /// Ensures required columns based on query properties
        /// </summary>
        /// <param name="settings">Data query settings</param>
        internal void EnsurePriorityColumns(DataQuerySettings settings)
        {
            if (CultureIsPrioritized())
            {
                // Add culture priority column to get most relevant document culture version
                AddCulturePriorityColumn(settings);
            }

            if (FilterDuplicates)
            {
                // Add duplicates priority column to get only original documents
                AddDuplicatesPriorityColumn(settings);
            }
        }


        /// <summary>
        /// Adds required columns to the columns list based on query properties
        /// </summary>
        /// <param name="settings">Data query settings</param>
        internal void EnsureRequiredColumns(IDataQuerySettings settings)
        {
            var columns = settings.SelectColumnsList;

            // Do not include extra columns when query is sub-query, DISTINCT or GROUP BY expressions are used since they could change the expected behavior
            if (settings.IsSubQuery || !EnsureExtraColumns || settings.SelectDistinct || settings.HasGroupBy)
            {
                return;
            }

            if (columns.Any(x => x is AggregatedColumn))
            {
                return;
            }

            if (!columns.ReturnsAllColumns)
            {
                columns.AddUnique(new QueryColumn("ClassName"), false);
            }

            if (IsLastVersion && !columns.ReturnsAllColumns)
            {
                columns.AddUnique(new QueryColumn("DocumentCheckedOutVersionHistoryID"), false);
            }

            // Ensure columns for permissions check, if columns specified
            if (CheckPermissions && !columns.ReturnsAllColumns)
            {
                columns.AddRangeUnique(SqlHelper.ParseColumnList(DocumentColumnLists.SECURITYCHECK_REQUIRED_COLUMNS), false);
            }
        }


        /// <summary>
        /// Gets modified single query to execute
        /// </summary>
        /// <param name="q">Original query</param>
        public IDataQuery GetSingleQuery(IDataQuery q)
        {
            var prioritizationSettings = GetPrioritizationSettings();

            var docQuery = (q as IDocumentQuery) ?? ParentQuery;

            // Get optimizations settings to provide output columns from within the outer join
            // Optimize only when prioritization is applied
            var optimizeSelect = OptimizeQuery(docQuery);
            var optimizationSettings = optimizeSelect ? GetOptimizationSettings(q) : null;

            // Ensure columns based on settings
            if (!optimizeSelect)
            {
                EnsureRequiredColumns(q);
            }

            // If properties define outer settings, propagate query properties to the outer settings
            if (prioritizationSettings != null)
            {
                q = ApplyPrioritization(q, prioritizationSettings);
            }

            if (optimizeSelect)
            {
                // Apply the optimization to the query
                q = ApplyOptimization(q, optimizationSettings, docQuery);

                // Ensure required columns for outer most query
                EnsureRequiredColumns(q);
            }

            return q;
        }


        /// <summary>
        /// Applies the prioritization to the given query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="prioritizationSettings">Prioritization settings</param>
        private IDataQuery ApplyPrioritization(IDataQuery q, DataQuerySettings prioritizationSettings)
        {
            PropagateSettings(q, prioritizationSettings);

            q = q.AsNested<DataQuery>();

            q.ApplySettings(prioritizationSettings);

            return q;
        }


        /// <summary>
        /// Applies the optimization to the executing query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="optimizationSettings">Optimization settings</param>
        /// <param name="docQuery">Original document query</param>
        private static IDataQuery ApplyOptimization(IDataQuery q, DataQuerySettings optimizationSettings, IDocumentQuery docQuery)
        {
            // Propagate aggregated columns to the base query level to provide correct aggregation
            var cols = optimizationSettings.SelectColumnsList;
            for (int i = 0; i < cols.Count; i++)
            {
                var col = cols[i];
                if (col is AggregatedColumn)
                {
                    q.SelectColumnsList.AddUnique(col);

                    if (cols.ReturnsAllColumns)
                    {
                        // If outer settings include all columns, get rid of this column to avoid duplicity because it will be automatically included through all columns
                        cols.Remove(col);
                        i--;
                    }
                    else
                    {
                        // Keep the column explicitly as alias
                        cols[i] = col.AsAlias();
                    }
                }
            }

            // We don't want to ensure order by columns at this level, because order by will be applied on its own in the outer query which brings back the full data
            var nestedSettings = new NestedQuerySettings { EnsureOrderByColumns = false };

            q = q
                .AsNested<DataQuery>(nestedSettings)
                // Explicitly switch to all columns, so that optimization settings without columns do not stay with just the system columns
                .Columns(SqlHelper.COLUMNS_ALL)
                .WithSettings(optimizationSettings);

            // Prepare new source joined with the original data to provide all the original columns for data selection
            if (docQuery != null)
            {
                docQuery.Properties.JoinWithFullData(q);
            }

            return q;
        }


        /// <summary>
        /// Gets the query optimization settings
        /// </summary>
        /// <param name="q">Query which should be optimized</param>
        private DataQuerySettings GetOptimizationSettings(IDataQuery q)
        {
            // Check if the query should be optimized for faster evaluation
            DataQuerySettings optimizationSettings = new DataQuerySettings();

            PropagateColumns(q, optimizationSettings, false);

            optimizationSettings.OrderByColumns = q.OrderByColumns;

            // Set inner columns to required minimum
            var cols = q.SelectColumnsList;

            cols.Load(new[]
            {
                new QueryColumn("NodeID").As(DocumentSystemColumns.NODE_ID),
                new QueryColumn("DocumentCulture").As(DocumentSystemColumns.DOCUMENT_CULTURE)
            });

            return optimizationSettings;
        }


        /// <summary>
        /// Returns true if the given query should be optimized
        /// </summary>
        /// <param name="docQuery">Document query</param>
        internal bool OptimizeQuery(IDocumentQuery docQuery)
        {
            // If document query is not known, cannot optimize
            if (docQuery == null)
            {
                return false;
            }

            // If optimization is set explicitly, apply it as the setting says
            if (OptimizeForScalability != null)
            {
                return OptimizeForScalability.Value && AllowsOptimization(docQuery);
            }

            var optimizeSelect = ShouldOptimize(docQuery);

            return optimizeSelect;
        }


        /// <summary>
        /// Returns true if the query has good selectivity of data, and therefore can benefit from optimization
        /// </summary>
        /// <param name="query">Query to check</param>
        private static bool ShouldOptimize(IDocumentQuery query)
        {
            // Get the fact if optimization is enabled for prioritized / non-prioritized queries
            var prioritized = query.Properties.CultureIsPrioritized();
            var enabled = prioritized ? mOptimizePrioritizedQueries.Value : mOptimizeNonPrioritizedQueries.Value;

            return
                enabled &&
                // Only TOPN and paged queries have required selectivity which can benefit from optimization
                ((query.TopNRecords > 0) || (query.MaxRecords > 0)) &&
                // Also check if the query allows optimization
                AllowsOptimization(query);
        }


        /// <summary>
        /// Returns true if the query can be executed using data optimization based on its settings
        /// </summary>
        /// <param name="query">Query to check</param>
        private static bool AllowsOptimization(IDataQuery query)
        {
            return
                String.IsNullOrEmpty(query.QueryName) && // Queries using other than default query cannot be optimized (custom query may not match the default structure)
                !query.HasDataSource && // Queries with data source cannot be optimized, data is retrieved non-standard way
                !query.ReturnsSingleColumn && // Single column query is light-weight, no need for optimization
                !query.SelectDistinct && // Distinct query needs to do distinct on full set of data, no need for optimization
                !query.HasGroupBy; // Group by query needs to do grouping on full set of data, no need for optimization
        }


        /// <summary>
        /// Joins the result of the given query with the full data
        /// </summary>
        /// <param name="q">Query</param>
        private void JoinWithFullData(IDataQuery q)
        {
            var src = q.GetSource().Join(
                DocumentQuerySourceHelper.GetBaseSourceTable(SourceSettings),
                new WhereCondition()
                    .WhereEquals(DocumentSystemColumns.NODE_ID, (DocumentQueryAliases.BASE_VIEW + ".NodeID").AsExpression())
                    .WhereEquals(DocumentSystemColumns.DOCUMENT_CULTURE, (DocumentQueryAliases.BASE_VIEW + ".DocumentCulture").AsExpression())
                );

            src = JoinExtraData(src, Document);

            q.QuerySource = src;
        }


        /// <summary>
        /// Gets the default source for this query
        /// </summary>
        /// <param name="document">Document object</param>
        internal QuerySource GetDefaultSource(TreeNode document)
        {
            var source = DocumentQuerySourceHelper.GetBaseSource(SourceSettings);

            source = JoinExtraData(source, document);

            return source;
        }


        /// <summary>
        /// Joins the extra data to the original data
        /// </summary>
        /// <param name="source">Original source</param>
        /// <param name="document">Document object</param>
        private QuerySource JoinExtraData(QuerySource source, TreeNode document)
        {
            // Join coupled data
            if (ParentQuery != null)
            {
                source = DocumentQuerySourceHelper.JoinCoupledData(source, document, SourceSettings);
            }

            // Join SKU data
            source = DocumentQuerySourceHelper.JoinSKUData(source, SourceSettings);

            return source;
        }


        /// <summary>
        /// Gets outer settings for data query based on properties
        /// </summary>
        private DataQuerySettings GetPrioritizationSettings()
        {
            DataQuerySettings settings = null;

            if (CultureIsPrioritized())
            {
                settings = new DataQuerySettings();

                // Get most relevant document culture version
                settings.WhereEquals(DocumentSystemColumns.CULTURE_PRIORITY, 1);
            }

            if (FilterDuplicates)
            {
                // Ensure settings
                if (settings == null)
                {
                    settings = new DataQuerySettings();
                }

                // Get only original documents
                settings.WhereEquals(SystemColumns.DUPLICATE_PRIORITY, 1);
            }

            return settings;
        }


        /// <summary>
        /// Indicates that multiple specified culture versions will be combined in the result.
        /// </summary>
        private bool CultureIsPrioritized()
        {
            return ((CombineWithDefaultCulture && CultureList.Length > 1) || CombineWithAnyCulture) && !AllCultures;
        }


        /// <summary>
        /// Applies properties to the given target instance.
        /// Does not apply properties which are already explicitly set in the target instance, and which values cannot be combined from both instances.
        /// </summary>
        /// <param name="p">Properties to copy the settings to</param>
        public void ApplyProperties(DocumentQueryProperties p)
        {
            // Combine current values with the ones from given properties, if not already set
            if (!p.mRelationshipNodeGUID.HasValue)
            {
                p.RelationshipNodeGUID = RelationshipNodeGUID;
            }
            if (p.RelationshipName == null)
            {
                p.RelationshipName = RelationshipName;
            }
            if (!p.mRelationshipSide.HasValue)
            {
                p.mRelationshipSide = mRelationshipSide;
            }
            if (!p.mNestingLevel.HasValue)
            {
                p.mNestingLevel = mNestingLevel;
            }
            if (!p.mSelectOnlyPublished.HasValue)
            {
                p.mSelectOnlyPublished = mSelectOnlyPublished;
            }
            if (!p.mEnsureExtraColumns.HasValue)
            {
                p.mEnsureExtraColumns = mEnsureExtraColumns;
            }
            if (!p.mIsLastVersion.HasValue)
            {
                p.mIsLastVersion = mIsLastVersion;
            }
            if (!p.mFilterDuplicates.HasValue)
            {
                p.mFilterDuplicates = mFilterDuplicates;
            }
            if (!p.mCheckPermissions.HasValue)
            {
                p.mCheckPermissions = mCheckPermissions;
            }
            if (!p.mAllCultures.HasValue)
            {
                p.mAllCultures = mAllCultures;
            }
            if (p.mDefaultCultureCode == null)
            {
                p.mDefaultCultureCode = mDefaultCultureCode;
            }
            if (p.mPreferredCultureCode == null)
            {
                p.mPreferredCultureCode = mPreferredCultureCode;
            }
            if (!p.mCombineWithDefaultCulture.HasValue)
            {
                p.mCombineWithDefaultCulture = mCombineWithDefaultCulture;
            }
            if (!p.mCombineWithAnyCulture.HasValue)
            {
                p.mCombineWithAnyCulture = mCombineWithAnyCulture;
            }
            if (!p.mIncludeCoupledColumns.HasValue)
            {
                p.mIncludeCoupledColumns = mIncludeCoupledColumns;
            }
            if (p.ExcludedVersionedColumns == null)
            {
                p.ExcludedVersionedColumns = ExcludedVersionedColumns;
            }
            if (p.OptimizeForScalability == null)
            {
                p.OptimizeForScalability = OptimizeForScalability;
            }

            p.ApplyDefaultSettings = ApplyDefaultSettings;

            if ((p.mSourceSettings == null) && (mSourceSettings != null))
            {
                p.mSourceSettings = mSourceSettings.Clone();
            }

            // Propagate tree provider
            p.TreeProvider = TreeProvider;

            // Combine lists
            p.Cultures.AddRange(Cultures);
            p.SiteIDs.AddRangeToSet(SiteIDs);
            p.Paths.AddRangeToSet(Paths);
            p.ExcludedPaths.AddRangeToSet(ExcludedPaths);

            // Reset dynamic properties
            p.mSiteName = null;
            p.mCultureList = null;

            // Propagate external filters
            if (mExternalFilters != null)
            {
                p.mExternalFilters = new List<IDocumentQueryFilter>(mExternalFilters);
            }
        }


        /// <summary>
        /// Clones object
        /// </summary>
        public DocumentQueryProperties Clone()
        {
            var clone = new DocumentQueryProperties(IsMultiQuery)
            {
                mRelationshipNodeGUID = mRelationshipNodeGUID,
                RelationshipName = RelationshipName,
                mRelationshipSide = mRelationshipSide,
                mNestingLevel = mNestingLevel,
                mSelectOnlyPublished = mSelectOnlyPublished,
                mEnsureExtraColumns = mEnsureExtraColumns,
                mIsLastVersion = mIsLastVersion,
                mFilterDuplicates = mFilterDuplicates,
                mCheckPermissions = mCheckPermissions,
                mAllCultures = mAllCultures,
                mCombineWithDefaultCulture = mCombineWithDefaultCulture,
                mCombineWithAnyCulture = mCombineWithAnyCulture,
                mPreferredCultureCode = mPreferredCultureCode,
                mDefaultCultureCode = mDefaultCultureCode,
                mSiteName = mSiteName,
                mTreeProvider = mTreeProvider,
                mIncludeCoupledColumns = mIncludeCoupledColumns,
                OptimizeForScalability = OptimizeForScalability,
                ApplyDefaultSettings = ApplyDefaultSettings
            };

            if (mSourceSettings != null)
            {
                clone.mSourceSettings = mSourceSettings.Clone();
            }

            clone.Cultures.AddRange(Cultures);
            clone.SiteIDs.AddRangeToSet(SiteIDs);
            clone.Paths.AddRangeToSet(Paths);
            clone.ExcludedPaths.AddRangeToSet(ExcludedPaths);

            if (mExternalFilters != null)
            {
                clone.mExternalFilters = new List<IDocumentQueryFilter>(mExternalFilters);
            }

            return clone;
        }


        /// <summary>
        /// Indicates if query is type-specific
        /// </summary>
        private bool IsTypeSpecific()
        {
            var className = ClassName;
            return !String.IsNullOrEmpty(className) && !TreeNodeProvider.IsGeneralDocumentClass(className);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets default value for combine with default culture
        /// </summary>
        protected virtual bool GetCombineWithDefaultCultureInternal()
        {
            if (!ApplyDefaultSettings)
            {
                return false;
            }

            return TreeProvider.GetCombineWithDefaultCulture(SiteName);
        }


        /// <summary>
        /// Gets default value for preferred culture
        /// </summary>
        protected virtual string GetPreferredCultureInternal()
        {
            var culture = CultureHelper.GetPreferredCulture();
            return string.IsNullOrEmpty(culture) ? DefaultCultureCode : culture;
        }


        /// <summary>
        /// Gets default value for default culture
        /// </summary>
        protected virtual string GetDefaultCultureCodeInternal()
        {
            return CultureHelper.GetDefaultCultureCode(SiteName);
        }


        /// <summary>
        /// Ensures preferred culture if none specified
        /// </summary>
        /// <param name="cultures">Current list of cultures</param>
        private void EnsurePreferredCulture(List<string> cultures)
        {
            if ((cultures.Count != 0) || !ApplyDefaultSettings)
            {
                return;
            }

            cultures.Add(PreferredCultureCode);
        }

        #endregion


        #region "Settings propagation"

        /// <summary>
        /// Propagates settings from the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        private void PropagateSettings(IDataQuerySettings inner, DataQuerySettings outer)
        {
            // Propagate DISTINCT or GROUP BY expressions to outer query
            if (inner.SelectDistinct || inner.HasGroupBy)
            {
                PropagateDistinct(inner, outer);

                PropagateGroupBy(inner, outer);

                // In this case all order by columns are available (must be in the select list), and no extra columns are wanted
                PropagateOrderBy(inner, outer, false);
            }
            else
            {
                // Propagate the order by only if DISTINCT or GROUP BY expressions are not used due to their limitations. (Original column is kept in the order by expression, because 
                // the columns aliases would need to be included in the select statement as well.) 
                PropagateOrderBy(inner, outer, true);
            }

            PropagateRowRestrictions(inner, outer);

            // Propagate columns as aliases so that all data is properly available
            PropagateColumns(inner, outer, true);
        }


        /// <summary>
        /// Propagates the distinct of the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        private static void PropagateDistinct(IDataQuerySettings inner, IDataQuerySettings outer)
        {
            if (inner.SelectDistinct)
            {
                outer.SelectDistinct = true;
                inner.SelectDistinct = false;
            }
        }


        /// <summary>
        /// Propagates the columns of the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        /// <param name="asAliases">If true, the columns are propagated as original query column aliases to make the data available. If false, full column list is propagated to outer query expecting that all necessary data is available in the outer query.</param>
        private void PropagateColumns(IDataQuerySettings inner, IDataQuerySettings outer, bool asAliases)
        {
            if (inner.SelectColumnsList.AnyColumnsDefined)
            {
                if (asAliases)
                {
                    // Distribute columns to outer settings
                    outer.SelectColumnsList = inner.SelectColumnsList.AsAliases();
                }
                else
                {
                    // Propagate the full column list leaving the original query columns unspecified
                    var cols = inner.SelectColumnsList;

                    inner.SelectColumnsList = new QueryColumnList();

                    outer.SelectColumnsList = cols;
                }
            }
        }


        /// <summary>
        /// Propagates the order by of the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        /// <param name="useRowNumber">If true, extra row number columns is used for order by propagation to cover cases where columns may not be externally available</param>
        /// <param name="includeRowNumberToColumns">If true, row number is included to the result columns, otherwise it is added only to filter columns</param>
        private static void PropagateOrderBy(IDataQuerySettings inner, IDataQuerySettings outer, bool useRowNumber, bool includeRowNumberToColumns = false)
        {
            if (useRowNumber)
            {
                // Use special row number column for propagation to cover cases where columns may not be externally available
                var orderBy = inner.OrderByColumns;

                SqlHelper.HandleEmptyColumns(ref orderBy);

                if (!String.IsNullOrEmpty(orderBy))
                {
                    var cols = includeRowNumberToColumns ? inner.SelectColumnsList : inner.FilterColumns;

                    cols.AddUnique(new RowNumberColumn(SystemColumns.ORDER_ROW_NUMBER, orderBy));

                    inner.OrderByColumns = SqlHelper.NO_COLUMNS;

                    outer.OrderByColumns = SystemColumns.ORDER_ROW_NUMBER;
                }
            }
            else
            {
                // Just propagate current order by, columns will be available
                outer.OrderByColumns = inner.OrderByColumns;
                inner.OrderByColumns = SqlHelper.NO_COLUMNS;
            }
        }


        /// <summary>
        /// Propagates the group by and having of the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        private static void PropagateGroupBy(IDataQuerySettings inner, IDataQuerySettings outer)
        {
            if (inner.HasGroupBy)
            {
                outer.GroupByColumns = inner.GroupByColumns;
                outer.HavingCondition = inner.HavingCondition;

                inner.GroupByColumns = null;
                inner.HavingCondition = null;
            }
        }


        /// <summary>
        /// Propagates the row restrictions of the inner settings to the outer settings
        /// </summary>
        /// <param name="inner">Inner settings</param>
        /// <param name="outer">Outer settings</param>
        private static void PropagateRowRestrictions(IDataQuerySettings inner, IDataQuerySettings outer)
        {
            outer.TopNRecords = inner.TopNRecords;
            inner.TopNRecords = 0;

            outer.Offset = inner.Offset;
            inner.Offset = 0;

            outer.MaxRecords = inner.MaxRecords;
            inner.MaxRecords = 0;
        }

        #endregion
    }
}

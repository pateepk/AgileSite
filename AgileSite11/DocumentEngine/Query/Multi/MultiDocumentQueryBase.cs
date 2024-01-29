using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.DocumentEngine.Query;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Base class for multiple documents query.
    /// </summary>
    public class MultiDocumentQueryBase<TQuery, TInnerQuery, TObject> :
        MultiObjectQueryBase<TQuery, TInnerQuery, TObject>,
        IMultiDocumentQuery<TQuery, TInnerQuery, TObject>
        where TQuery : MultiDocumentQueryBase<TQuery, TInnerQuery, TObject>, new()
        where TObject : TreeNode, new()
        where TInnerQuery : AbstractQueryObject, IDocumentQuery<TInnerQuery, TObject>, new()
    {
        #region "Variables"

        /// <summary>
        /// Document query properties
        /// </summary>
        private DocumentQueryProperties mProperties;

        #endregion


        #region "Properties"

        /// <summary>
        /// Document query properties.
        /// </summary>
        public DocumentQueryProperties Properties
        {
            get
            {
                return mProperties ?? (mProperties = CreateProperties());
            }
            set
            {
                // Clone the properties if already owned by another query
                if (value.ParentQuery != null)
                {
                    value = value.Clone();
                }

                value.ParentQuery = this;
                mProperties = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public MultiDocumentQueryBase()
        {
            DefaultQuery = new TInnerQuery();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if distribution of TOP N to inner queries is allowed
        /// </summary>
        protected override bool AllowTopNDistribution()
        {
            return
                // Only distribute TopN to a multi-query with multiple queries. When only single query is used, the distribution doesn't provide enough effect
                (GetSingleInnerQuery() == null) &&
                // Do not apply this if result order by is defined, as it may influence the final order, and may not be compatible to distribute to inner query
                String.IsNullOrEmpty(OrderByResultColumns) &&
                // Do not apply if both paging and result columns are defined, because sub-totals could not be passed to outer query
                !(IsPagedQuery && SelectResultColumnsList.AnyColumnsDefined);
        }


        /// <summary>
        /// Creates the query properties
        /// </summary>
        private DocumentQueryProperties CreateProperties()
        {
            return new DocumentQueryProperties(true)
            {
                ParentQuery = this
            };
        }


        /// <summary>
        /// Initializes the typed result
        /// </summary>
        /// <param name="typedResult">Result</param>
        protected override void InitTypedResult(InfoDataSet<TObject> typedResult)
        {
            // Provide initialization using document factory
            typedResult.Items.ObjectInitializer = data =>
            {
                // Get class name
                string className = DataHelper.GetStringValue(data, "ClassName", null);

                // Create document instance
                var document = TreeNode.New<TObject>(className, data, Properties.TreeProvider);
                document.IsLastVersion = Properties.IsLastVersion;

                return document;
            };
        }


        /// <summary>
        /// Resolves the given type into corresponding types
        /// </summary>
        /// <param name="type">Source type</param>
        protected override List<string> ResolveType(string type)
        {
            // Resolve class names
            var types = DocumentTypeHelper.GetClassNames(type, Properties.SiteName);
            if (!string.IsNullOrEmpty(types))
            {
                return types.Split(';').ToList();
            }

            return base.ResolveType(type);
        }


        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        public override IDataQuery GetExecutingQuery(DataQuerySettings settings = null)
        {
            var queries = QueriesList;

            // If type columns are requested, try to include all types if none defined
            if ((queries.Count == 0) && UseTypeColumns)
            {
                var allTypes = GetClassNames();
                if (allTypes.Count > 0)
                {
                    // Create queries for all types, and return them, as they are not referenced by multi-query
                    queries = allTypes.Select(type => CreateQuery(type)).ToList();
                    var query = BuildMultiQueryFrom(queries, settings);

                    return query;
                }
            }

            return base.GetExecutingQuery(settings);
        }



        /// <summary>
        /// Gets all types available for the current query
        /// </summary>
        private IList<string> GetClassNames()
        {
            // Get available class names through inner single query
            var query = GetSingleQuery(null);
            if (query == null)
            {
                return new List<string>();
            }

            query.ExpandToFullSourceSet();

            query.SelectColumnsList.Load(new[] { "ClassName" });
            query.SelectDistinct = true;
            query.OrderByColumns = "ClassName";

            return query.GetListResult<string>();
        }
        

        /// <summary>
        /// Ensures the query with the given type
        /// </summary>
        /// <param name="type">Query type</param>
        protected override TInnerQuery EnsureQuery(string type)
        {
            // Include coupled columns if not explicitly defined
            if (Properties.mIncludeCoupledColumns == null)
            {
                Properties.IncludeCoupledColumns = true;
            }

            return base.EnsureQuery(type);
        }


        /// <summary>
        /// Applies main query properties to the given query to ensure synchronized state before execution
        /// </summary>
        /// <param name="query">Query to prepare</param>
        /// <param name="multiQuery">If true, the query is an inner query within multi-query</param>
        protected override void ApplyProperties(IDataQuery query, bool multiQuery = false)
        {
            base.ApplyProperties(query, multiQuery);

            // Disable columns of inner query if coupled columns are not allowed
            if (multiQuery && !Properties.IncludeCoupledColumns && !query.SelectColumnsList.AnyColumnsDefined)
            {
                query.SelectColumnsList.Load(SqlHelper.NO_COLUMNS);
            }

            var docQuery = query as IDocumentQuery;
            if (docQuery != null)
            {
                Properties.ApplyProperties(docQuery.Properties);
            }
        }


        /// <summary>
        /// Returns the where condition which filters the default query data for specific types
        /// </summary>
        /// <param name="types">List of types for which create the where condition</param>
        protected override string GetTypesWhereCondition(IEnumerable<string> types)
        {
            return Properties.GetClassesWhereCondition(types);
        }


        /// <summary>
        /// Creates query for the given type
        /// </summary>
        /// <param name="type">Query type</param>
        protected override TInnerQuery CreateQuery(string type)
        {
            // Get document object type based on document type name
            var objectType = TreeNodeProvider.GetObjectType(type);
            return base.CreateQuery(objectType);
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        protected override DataSet GetDataFromDB()
        {
            return Properties.GetDataInternal(this, () => base.GetDataFromDB(), t => TotalRecords = t);
        }


        /// <summary>
        /// Executes the current over data source and returns it's results as a DataSet
        /// </summary>
        protected override DataSet GetDataFromDataSource()
        {
            return Properties.GetDataInternal(this, () => base.GetDataFromDataSource(), t => TotalRecords = t);
        }


        /// <summary>
        /// Attempts to get a single query for the whole result based on the current state of the query object
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        protected override IDataQuery GetSingleQuery(DataQuerySettings settings)
        {
            var q = base.GetSingleQuery(settings);
            if (q != null)
            {
                // Ensure proper explicit result columns
                if (SelectResultColumnsList.AnyColumnsDefined)
                {
                    q.SelectColumnsList = SelectResultColumnsList;
                }
            }

            return q;
        }


        /// <summary>
        /// Gets the list of all available columns for this query
        /// </summary>
        protected override List<string> GetAvailableColumns()
        {
            return Properties.Document.ColumnNames;
        }


        /// <summary>
        /// Gets the ID column for this query
        /// </summary>
        protected override string GetIDColumn()
        {
            var idColumn = Properties.Document.TypeInfo.IDColumn;
            if (idColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return idColumn;
            }

            return base.GetIDColumn();
        }


        /// <summary>
        /// Sets class name for current query
        /// </summary>
        /// <param name="value">New class name value</param>
        protected override void SetClassName(string value)
        {
            base.SetClassName(value);

            // Update object type with relevant value
            ObjectType = TreeNodeProvider.GetObjectType(value);
        }


        /// <summary>
        /// Copies the properties to the target query.
        /// </summary>
        /// <param name="target">Target query</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IMultiDocumentQuery<TQuery, TInnerQuery, TObject>;
            if (t != null)
            {
                t.Properties = Properties;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Gets the default order by columns
        /// </summary>
        protected override string GetDefaultOrderBy()
        {
            return TreeNode.TYPEINFO.DefaultOrderBy;
        }

        #endregion


        #region "Setup methods"

        #region "General methods"

        /// <summary>
        /// Ensures that there is no restriction applied for the result.
        /// </summary>
        public TQuery All()
        {
            var result = GetTypedQuery();
            result.NewWhere();
            result.Properties.All();

            return result;
        }


        /// <summary>
        /// Ensures default restrictions for the result.
        /// </summary>
        public TQuery Default()
        {
            var result = GetTypedQuery();
            result.NewWhere();
            result.Properties.Default();

            return result;
        }


        /// <summary>
        /// Ensures that latest version of the documents is retrieved.
        /// </summary>
        /// <param name="latest">If true, the latest (edited) version is retrieved, otherwise published version is retrieved</param>
        public TQuery LatestVersion(bool latest = true)
        {
            var result = GetTypedQuery();
            result.Properties.IsLastVersion = latest;

            return result;
        }


        /// <summary>
        /// Ensures that published version of the documents is retrieved.
        /// </summary>
        /// <param name="published">If true, published version is retrieved, otherwise latest (edited) version is retrieved</param>
        public TQuery PublishedVersion(bool published = true)
        {
            return LatestVersion(!published);
        }


        /// <summary>
        /// Ensures that only documents published on a live site are retrieved.
        /// </summary>
        /// <param name="published">If true, only published documents are retrieved, otherwise all documents are retrieved</param>
        public TQuery Published(bool published = true)
        {
            var result = GetTypedQuery();
            result.Properties.SelectOnlyPublished = published;

            return result;
        }


        /// <summary>
        /// Ensures that the result will be filtered based on user Read permission.
        /// </summary>
        /// <param name="check">If true, the permission check is enabled, otherwise disabled</param>
        public TQuery CheckPermissions(bool check = true)
        {
            var result = GetTypedQuery();
            result.Properties.CheckPermissions = check;

            return result;
        }


        /// <summary>
        /// Ensures that duplicate document are filtered from the result. This means that linked documents are not retrieved, if there is the original document already included in the results.
        /// </summary>
        /// <param name="filter">If true, the permission check is enabled, otherwise disabled</param>
        public TQuery FilterDuplicates(bool filter = true)
        {
            var result = GetTypedQuery();
            result.Properties.FilterDuplicates = filter;

            return result;
        }


        /// <summary>
        /// Ensures that coupled columns will be included in the results.
        /// </summary>
        /// <param name="include">Indicates if coupled columns should be included</param>
        public TQuery WithCoupledColumns(bool include = true)
        {
            return WithCoupledColumns(include ? IncludeCoupledDataEnum.Complete : IncludeCoupledDataEnum.None);
        }


        /// <summary>
        /// Defines how coupled columns will be included in the results.
        /// </summary>
        /// <param name="type">Type of the inclusion</param>
        public TQuery WithCoupledColumns(IncludeCoupledDataEnum type)
        {
            var result = GetTypedQuery();

            // Include coupled columns in the query final result if required
            result.Properties.IncludeCoupledColumns = type != IncludeCoupledDataEnum.None;
            if (type != IncludeCoupledDataEnum.None)
            {
                UseDefaultQuery = UseDefaultQueryEnum.NotAllowed;
            }

            // Set whether type columns should be explicitly included or excluded
            UseTypeColumns = (type == IncludeCoupledDataEnum.Complete);

            return result;
        }


        /// <summary>
        /// Ensures that only coupled columns will be included in the results.
        /// </summary>
        public TQuery OnlyCoupledColumns()
        {
            return WithCoupledColumns().NoDefaultColumns();
        }

        #endregion


        #region "Relationship methods"

        /// <summary>
        /// Ensures that only documents in relationship with a specified document are retrieved.
        /// </summary>
        /// <param name="nodeGuid">Node GUID of the related document</param>
        /// <param name="relationshipName">Name of the relationship. If not provided documents from all relationships will be retrieved.</param>
        /// <param name="side">Side of the related document within the relation</param>
        public TQuery InRelationWith(Guid nodeGuid, string relationshipName = null, RelationshipSideEnum side = RelationshipSideEnum.Both)
        {
            var result = GetTypedQuery();
            result.Properties.RelationshipNodeGUID = nodeGuid;
            result.Properties.RelationshipName = relationshipName;
            result.Properties.RelationshipSide = side;

            return result;
        }

        #endregion


        #region "Nesting level methods"

        /// <summary>
        /// Ensures that only documents within specified nesting level are retrieved.
        /// </summary>
        /// <param name="level">Nesting level</param>
        public TQuery NestingLevel(int level)
        {
            var result = GetTypedQuery();
            result.Properties.NestingLevel = level;

            return result;
        }

        #endregion


        #region "Path methods"

        /// <summary>
        /// Filters the data to include only documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        public TQuery Path(params string[] paths)
        {
            var result = GetTypedQuery();
            result.Properties.Path(paths);

            return result;
        }


        /// <summary>
        /// Filters the data to include only documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define selection scope</param>
        public TQuery Path(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            var result = GetTypedQuery();
            result.Properties.Path(path, type);

            return result;
        }


        /// <summary>
        /// Filters the data to exclude documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        public TQuery ExcludePath(params string[] paths)
        {
            var result = GetTypedQuery();
            result.Properties.ExcludePath(paths);

            return result;
        }


        /// <summary>
        /// Filters the data to exclude documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define excluded scope</param>
        public TQuery ExcludePath(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            var result = GetTypedQuery();
            result.Properties.ExcludePath(path, type);

            return result;
        }

        #endregion


        #region "Culture methods"

        /// <summary>
        /// Filters the data to include only documents translated to given culture(s).
        /// </summary>
        /// <param name="cultures">List of document cultures</param>
        public TQuery Culture(params string[] cultures)
        {
            var result = GetTypedQuery();
            result.Properties.Culture(cultures);

            return result;
        }


        /// <summary>
        /// The data will be combined with documents in site default culture if not translated to the requested one(s). When several cultures are required, the order is taken in account as a priority for documents with several matching language versions.
        /// </summary>
        /// <param name="combine">If true, documents will be combined with default culture, otherwise only documents translated to one of the requested culture(s) are retrieved</param>
        public TQuery CombineWithDefaultCulture(bool combine = true)
        {
            var result = GetTypedQuery();
            result.Properties.CombineWithDefaultCulture = combine;

            return result;
        }


        /// <summary>
        /// The data will be combined with documents in any culture if not translated to the requested one(s). When several cultures are required, the order is taken in account as a priority for documents with several matching language versions followed by any other language version.
        /// If no culture specified, the highest priority has site preferred culture, second the default culture followed by the rest of site cultures.
        /// </summary>
        /// <param name="combine">If true, documents will be combined with any other culture(s), otherwise only documents translated to one of the requested culture(s) are retrieved</param>
        public TQuery CombineWithAnyCulture(bool combine = true)
        {
            var result = GetTypedQuery();
            result.Properties.CombineWithAnyCulture = combine;

            return result;
        }


        /// <summary>
        /// Ensures that all culture versions of the documents are retrieved.
        /// </summary>
        /// <param name="all">If true, all culture versions are retrieved, otherwise documents of specified culture(s) are retrieved</param>
        public TQuery AllCultures(bool all = true)
        {
            var result = GetTypedQuery();
            result.Properties.AllCultures = all;

            return result;
        }

        #endregion


        #region "Site methods"

        /// <summary>
        /// Filters the documents to include only records on the given site.
        /// </summary>
        /// <param name="siteIdentifier">Site identifier</param>
        /// <param name="includeGlobal">If true, includes the global objects in case the object type covers both site and global objects</param>
        public override TQuery OnSite(SiteInfoIdentifier siteIdentifier, bool includeGlobal = false)
        {
            var siteId = (siteIdentifier != null) ? siteIdentifier.ObjectID : 0;

            if (siteId == ProviderHelper.ALL_SITES)
            {
                // Do not add site condition if ALL_SITES is used
                return GetTypedQuery();
            }

            var result = WhereEquals("NodeSiteID", siteId);

            // Store site ID in properties
            result.Properties.OnSite(siteId);

            return result;
        }


        /// <summary>
        /// Filters the data to include only records on the current site.
        /// </summary>
        public TQuery OnCurrentSite()
        {
            return OnSite(SiteContext.CurrentSiteID);
        }

        #endregion

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Predefined query returning given document type.
    /// </summary>
    public abstract class DocumentQueryBase<TQuery, TObject> :
        ObjectQueryBase<TQuery, TObject>,
        IDocumentQuery<TQuery, TObject>
        where TQuery : DocumentQueryBase<TQuery, TObject>, new()
        where TObject : TreeNode, new()
    {
        #region "Variables"

        /// <summary>
        /// Document query properties
        /// </summary>
        private DocumentQueryProperties mProperties;

        #endregion


        #region "Properties"

        /// <summary>
        /// Represents a full query name of the query
        /// </summary>
        public override string FullQueryName
        {
            get
            {
                var queryName = !String.IsNullOrEmpty(QueryName) ? QueryName : DataEngine.QueryName.GENERALSELECT;

                return String.Format("{0}.{1}", ClassName, queryName);
            }
        }


        /// <summary>
        /// Document instance of the specified type.
        /// </summary>
        protected TreeNode Document
        {
            get
            {
                return Properties.Document;
            }
            set
            {
                Properties.Document = value;
                Object = value;
            }
        }


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


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="className">Class name</param>
        protected DocumentQueryBase(string className)
            : base(TreeNodeProvider.GetObjectType(className))
        {
            UseObjectQuery = false;
        }


        /// <summary>
        /// Updates the query class name based on the current status
        /// </summary>
        protected override void TypeUpdated()
        {
            Properties.Document = null;

            if (!String.IsNullOrEmpty(ObjectType))
            {
                var doc = Properties.Document;
                if (doc != null)
                {
                    ClassName = string.IsNullOrEmpty(doc.ClassName) ? doc.TypeInfo.ObjectClassName : doc.ClassName;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if distribution of TOP N to inner queries is allowed
        /// </summary>
        protected override bool AllowTopNDistribution()
        {
            // Only distribute Top N if the query is optimized (complex enough to gain from distribution)
            return Properties.OptimizeQuery(this);
        }


        /// <summary>
        /// Creates the query properties
        /// </summary>
        private DocumentQueryProperties CreateProperties()
        {
            return new DocumentQueryProperties
            {
                ParentQuery = this
            };
        }


        /// <summary>
        /// Gets the default source for this query
        /// </summary>
        protected override QuerySource GetDefaultSource()
        {
            return Properties.GetDefaultSource(Document);
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
        /// Gets the list of all available columns for this query
        /// </summary>
        protected override List<string> GetAvailableColumns()
        {
            // Return all columns for standalone query
            if (!IsNested)
            {
                return Document.ColumnNames;
            }

            // Get only type specific column names
            return Document.GetTypeSpecificColumnNames().ToList();
        }


        /// <summary>
        /// Gets the complete parameters for the query execution. The parameters are always a new instance of DataQuerySettings which can be further modified without any impact to the query itself.
        /// </summary>
        /// <param name="executingQuery">Executing query for which the parameters are retrieved</param>
        public override DataQuerySettings GetCompleteSettings(IDataQuery executingQuery = null)
        {
            var settings = base.GetCompleteSettings(executingQuery);

            // Apply properties to query settings
            Properties.ApplyToSettings(settings);

            return settings;
        }


        /// <summary>
        /// Copies the properties to the target query.
        /// </summary>
        /// <param name="target">Target query</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IDocumentQuery;
            if (t != null)
            {
                // Copy properties
                t.Properties = Properties;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        protected override DataSet GetDataFromDB()
        {
            return Properties.GetDataInternal(this, () => base.GetDataFromDB(), (t) => TotalRecords = t);
        }


        /// <summary>
        /// Executes the current over data source and returns it's results as a DataSet
        /// </summary>
        protected override DataSet GetDataFromDataSource()
        {
            return Properties.GetDataInternal(this, () => base.GetDataFromDataSource(), (t) => TotalRecords = t);
        }

               
        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        public override IDataQuery GetExecutingQuery(DataQuerySettings settings = null)
        {
            var q = base.GetExecutingQuery(settings);

            // Apply properties to the query
            q = Properties.GetSingleQuery(q);

            return q;
        }


        /// <summary>
        /// Gets data set returned by document query or custom data set. Data set can be modified by calling DocumentEvents.GetData event.
        /// </summary>
        protected override DataSet GetData()
        {
            // Handle the event
            using (var h = DocumentEvents.GetData.StartEvent(null, this, -1))
            {
                DocumentDataEventArgs e = h.EventArguments;

                if (h.CanContinue())
                {
                    // If data set returned by GetData.Before event is null, default document query is called
                    if (e.Data == null)
                    {
                        // Ensure data for GetData after event
                        e.Data = base.GetData();
                        e.TotalRecords = TotalRecords;
                    }
                    else
                    {
                        // Count data in dataset created in GetData.Before event. Property MUST be set to prevent endless loop (GetData() would be called to get TotalRecords value)
                        TotalRecords = (e.TotalRecords < 0) ? DataHelper.GetItemsCount(e.Data) : e.TotalRecords;
                    }
                }

                h.FinishEvent();

                // Update total records if they were changed by GetData.After event
                if (TotalRecords != e.TotalRecords)
                {
                    TotalRecords = (e.TotalRecords < 0) ? DataHelper.GetItemsCount(e.Data) : e.TotalRecords;
                }

                return e.Data;
            }
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


        #region "Site methods"

        /// <summary>
        /// Filters the data to include only records on the given site. If site binding is defined, filters the global objects by the site ID
        /// </summary>
        /// <param name="siteIdentifier">Site identifier</param>
        /// <param name="includeGlobal">If true, includes the global objects in case the object type covers both site and global objects</param>
        public override TQuery OnSite(SiteInfoIdentifier siteIdentifier, bool includeGlobal = false)
        {
            var siteId = siteIdentifier.ObjectID;

            if (siteId == ProviderHelper.ALL_SITES)
            {
                // Do not add site condition if ALL_SITES is used
                return GetTypedQuery();
            }

            var result = WhereEquals("NodeSiteID", siteId);

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

        #endregion
    }
}

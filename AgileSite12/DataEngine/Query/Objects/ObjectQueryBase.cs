using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Predefined query returning given object type. Uses the .selectall query internally.
    /// </summary>
    public abstract class ObjectQueryBase<TQuery, TObject> :
        DataQueryBase<TQuery>,
        ICMSQueryable<TObject>,
        IObjectQuery<TQuery, TObject>
        where TQuery : ObjectQueryBase<TQuery, TObject>, new()
        where TObject : BaseInfo
    {
        #region "Variables"

        /// <summary>
        /// Object type.
        /// </summary>
        private string mObjectType;

        /// <summary>
        /// Object instance of specified type.
        /// </summary>
        private BaseInfo mObject;

        private IQueryProvider mProvider;
        private Expression mExpression;

        private ObjectTypeInfo mTypeInfo;

        private IExecutingQueryProvider mExecutingProvider;

        #endregion


        #region "Properties"

        /// <summary>
        /// Object name, empty by default
        /// </summary>
        public override string Name
        {
            get
            {
                return ObjectType;
            }
        }


        /// <summary>
        /// If true, the object query is used as default, otherwise, standard DataQuery is used
        /// </summary>
        protected bool UseObjectQuery
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;

                mObject = null;
                mTypeInfo = null;

                // Init class name from object
                TypeUpdated();
            }
        }


        /// <summary>
        /// If true, the query uses the object type condition.
        /// </summary>
        public bool UseObjectTypeCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Updates the query class name based on the current status
        /// </summary>
        protected virtual void TypeUpdated()
        {
            ClassName = null;

            mExecutingProvider?.Refresh(TypeInfo);
        }


        /// <summary>
        /// Object instance of the specified type.
        /// </summary>
        protected BaseInfo Object
        {
            get
            {
                if (mObject == null)
                {
                    mObject = ModuleManager.GetReadOnlyObject(ObjectType);

                    if (mObject == null)
                    {
                        throw new InvalidOperationException("Object type '" + ObjectType + "' not found.");
                    }
                }

                return mObject;
            }
            set
            {
                mObject = value;

                // Set object type and type info
                mObjectType = value?.TypeInfo.ObjectType;
                mTypeInfo = value?.TypeInfo;

                TypeUpdated();
            }
        }


        /// <summary>
        /// Type info of the specified type
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo ?? (mTypeInfo = ObjectTypeManager.GetTypeInfo(ObjectType) ?? Object.TypeInfo);
            }
            internal set
            {
                mTypeInfo = value;
            }
        }


        /// <summary>
        /// Typed result
        /// </summary>
        public InfoDataSet<TObject> TypedResult
        {
            get
            {
                return EnsureTypedResult();
            }
        }


        /// <summary>
        /// Returns first object from the query result DataSet. The property does not limit the actual number of fetched rows.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Accessing this property effectively results in performing the query (i.e. fetching all rows the query selects, if not already fetched)
        /// and returning the first object from the query result DataSet.
        /// </para>
        /// <para>
        /// To limit the number of fetched rows use <see cref="DataQuerySettingsBase{TQuery}.TopN"/>.
        /// </para>
        /// </remarks>
        [Obsolete("Use System.Linq.Enumerable.FirstOrDefault<TSource>(this System.Collections.Generic.IEnumerable<TSource>) extension method instead.")]
        public TObject FirstObject
        {
            get
            {
                return TypedResult.FirstOrDefault();
            }
        }


        /// <summary>
        /// Gets or sets the provider of executing query
        /// </summary>
        /// <remarks>
        /// By default returns <see cref="InfoObjectExecutingQueryProvider"/>
        /// </remarks>
        internal IExecutingQueryProvider ExecutingQueryProvider
        {
            get
            {
                return mExecutingProvider ?? (mExecutingProvider = new InfoObjectExecutingQueryProvider(TypeInfo));
            }
            set
            {
                mExecutingProvider = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        protected ObjectQueryBase(string objectType)
            : base(null, null)
        {
            ObjectType = objectType;
        }


        /// <summary>
        /// Initializes the query from the given type
        /// </summary>
        protected void InitFromType<T>()
            where T : BaseInfo
        {
            Object = ObjectFactory<T>.New();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a new empty query
        /// </summary>
        protected override TQuery NewEmptyQuery()
        {
            var result = base.NewEmptyQuery();

            // Set to the same object type as this query
            result.ObjectType = ObjectType;

            return result;
        }

        /// <summary>
        /// Ensures that the result is strongly typed, and returns the result
        /// </summary>
        private InfoDataSet<TObject> EnsureTypedResult()
        {
            var result = Result;

            // Ensure proper result type
            var typedResult = result as InfoDataSet<TObject>;
            if (typedResult == null)
            {
                typedResult = result.As<TObject>();
                Result = typedResult;
            }

            // Ensure typed result initialization
            InitTypedResult(typedResult);

            return typedResult;
        }


        /// <summary>
        /// Initializes the typed result
        /// </summary>
        /// <param name="typedResult">Result</param>
        protected virtual void InitTypedResult(InfoDataSet<TObject> typedResult)
        {
            typedResult.Object = (TObject)Object;
        }


        /// <summary>
        /// Initializes the ID query
        /// </summary>
        /// <param name="resultColumn">Resulting column</param>
        protected void InitIDQuery(string resultColumn)
        {
            OrderByColumns = SqlHelper.NO_COLUMNS;

            string idColumn = null;

            if (!String.IsNullOrEmpty(resultColumn))
            {
                // Use explicitly defined result
                idColumn = resultColumn;
            }
            else if (!TypeInfo.IsBinding)
            {
                // Set the ID column if not binding
                idColumn = Object.TypeInfo.IDColumn;
            }
            else if (TypeInfo.IsSiteBinding)
            {
                // For site binding, get the parent ID column as the default one
                idColumn = Object.TypeInfo.ParentIDColumn;
            }

            if (idColumn == null)
            {
                throw new NotSupportedException("ID query over binding object must have the result column defined. Use constructor with resultColumn parameter.");
            }

            Column(idColumn);
        }


        /// <summary>
        /// Gets the unique query source ID
        /// </summary>
        protected override string GetDataSourceName()
        {
            if (HasDataSource)
            {
                // If data source is set, get it's source ID
                return DataSource.DataSourceName;
            }

            // Get source ID of the source query
            var query = GetSourceQuery();
            return query.DataSourceName;
        }


        /// <summary>
        /// Gets the query that provides the source of data
        /// </summary>
        private IDataQuery GetSourceQuery()
        {
            IDataQuery query;

            if (HasDataSource)
            {
                // Create fake query to allow generate query text
                query = new DataQuery();
            }
            else if (!UseObjectQuery)
            {
                // Use specific data query
                query = new DataQuery(FullQueryName);
            }
            else
            {
                query = ExecutingQueryProvider.GetExecutionQuery();
                query.IncludeBinaryData = IncludeBinaryData;

                // Apply the custom query name
                if (!String.IsNullOrEmpty(QueryName))
                {
                    query.QueryName = QueryName;
                }
            }

            // Add type where condition
            if (UseObjectTypeCondition)
            {
                query.WhereCondition = SqlHelper.AddWhereCondition(query.WhereCondition, TypeInfo.WhereCondition);
            }

            return query;
        }


        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        public override IDataQuery GetExecutingQuery(DataQuerySettings settings = null)
        {
            var query = GetSourceQuery();

            // Get the complete parameters for execution
            if (settings == null)
            {
                settings = GetCompleteSettings(query);
            }

            // Apply query settings
            query.ApplySettings(settings);

            if (!String.IsNullOrEmpty(CustomQueryText))
            {
                query.CustomQueryText = CustomQueryText;
            }

            ApplyProperties(query);

            return query;
        }


        /// <summary>
        /// Gets results from executing query
        /// </summary>
        /// <param name="query">Executing query</param>
        /// <param name="totalRecords">Returns the total records number</param>
        protected override DataSet GetResults(IDataQuery query, ref int totalRecords)
        {
            var results = base.GetResults(query, ref totalRecords);

            return results.As<TObject>();
        }


        /// <summary>
        /// Gets data set returned by object query or custom data set. Data set can be modified by calling ObjectEvents.GetData event.
        /// </summary>
        protected override DataSet GetData()
        {
            // Handle the event
            using (var h = TypeInfo.Events.GetData.StartEvent(null, this, -1))
            {
                ObjectDataEventArgs e = h.EventArguments;

                if (h.CanContinue())
                {
                    // If GetData.Before event returns null, default object query is called
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


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        /// <param name="target">Target class</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IObjectQuery;
            if (t != null)
            {
                t.ObjectType = ObjectType;
                t.UseObjectTypeCondition = UseObjectTypeCondition;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Executes the given action for each item (TObject) in the result. Processes the items in batches of the given size.
        /// </summary>
        /// <param name="objAction">Object action</param>
        /// <param name="batchSize">Batch size. 0 means no batch processing. By default uses current paging settings.</param>
        public void ForEachObject(Action<TObject> objAction, int batchSize = -1)
        {
            if (objAction == null)
            {
                return;
            }

            try
            {
                ForEachPage(q =>
                {
                    // Process all objects on the page
                    foreach (var obj in q)
                    {
                        objAction(obj);
                    }
                }, batchSize);
            }
            catch (ActionCancelledException)
            {
                // Enumeration was cancelled
            }
        }


        /// <summary>
        /// Gets the class structure info for this query
        /// </summary>
        protected override ClassStructureInfo GetClassStructureInfo()
        {
            return TypeInfo.ClassStructureInfo;
        }


        /// <summary>
        /// Gets the class name for current query
        /// </summary>
        protected override string GetClassName()
        {
            return TypeInfo.ObjectClassName;
        }


        /// <summary>
        /// Gets the default order by columns
        /// </summary>
        protected override string GetDefaultOrderBy()
        {
            return TypeInfo.DefaultOrderBy;
        }


        /// <summary>
        /// Gets the ID column for this query
        /// </summary>
        protected override string GetIDColumn()
        {
            var idColumn = TypeInfo.IDColumn;
            if (idColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return idColumn;
            }

            return base.GetIDColumn();
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            return GetExecutingQuery().ToString(expand);
        }


        /// <summary>
        /// Creates a nested query from the given query
        /// </summary>
        public ObjectQuery<TObject> AsNested()
        {
            var nested = AsNested<ObjectQuery<TObject>>();
            nested.ObjectType = ObjectType;

            return nested;
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Filters the data to include only global objects.
        /// </summary>
        public TQuery OnlyGlobal()
        {
            return OnSite(0, true);
        }


        /// <summary>
        /// Filters the data to include only site objects, but not global objects
        /// </summary>
        public TQuery ExceptGlobal()
        {
            return OnSite(ProviderHelper.ALL_SITES);
        }


        /// <summary>
        /// Filters the data to include only records on the given site. If site binding is defined, filters the global objects by the site ID
        /// </summary>
        /// <param name="siteIdentifier">Site identifier</param>
        /// <param name="includeGlobal">If true, includes the global objects in case the object type covers both site and global objects</param>
        public virtual TQuery OnSite(SiteInfoIdentifier siteIdentifier, bool includeGlobal = false)
        {
            var where = TypeInfo.GetSiteWhereCondition(siteIdentifier, includeGlobal);

            return Where(where);
        }


        /// <summary>
        /// Sets the where condition for a specific object ID
        /// </summary>
        /// <param name="objectId">Object ID</param>
        public TQuery WithID(int objectId)
        {
            return Where(TypeInfo.IDColumn, QueryOperator.Equals, objectId);
        }


        /// <summary>
        /// Sets the where condition for a specific object code name
        /// </summary>
        /// <param name="codeName">Code name</param>
        public TQuery WithCodeName(string codeName)
        {
            return Where(TypeInfo.CodeNameColumn, QueryOperator.Equals, codeName);
        }


        /// <summary>
        /// Sets the where condition for a specific object GUID
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public TQuery WithGuid(Guid guid)
        {
            return Where(TypeInfo.GUIDColumn, QueryOperator.Equals, guid);
        }


        /// <summary>
        /// Sets the given DataSet as the source of the data query
        /// </summary>
        /// <param name="items">Source items</param>
        public TQuery WithSource(params TObject[] items)
        {
            return WithSource(new InfoDataSet<TObject>(items));
        }


        /// <summary>
        /// Changes the query to use the given object type. Applies the object type condition to the query. The new object type must have the same class name as the original one.
        /// </summary>
        /// <param name="objectType">Object type for the query</param>
        public TQuery WithObjectType(string objectType)
        {
            var newTypeInfo = ObjectTypeManager.GetTypeInfo(objectType, true);
            var typeInfo = TypeInfo;

            // Match the class name to ensure that only types with same class definition (therefore DB table) are used
            if (!newTypeInfo.ObjectClassName.Equals(typeInfo.ObjectClassName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException($"New object type for the query must have the same class name as the original object type. Requested object type was {newTypeInfo.ObjectType} with class name {newTypeInfo.ObjectClassName}. Original object type was {typeInfo.ObjectType} with class name {typeInfo.ObjectClassName}.");
            }

            var result = GetTypedQuery();

            result.ObjectType = objectType;
            result.UseObjectTypeCondition = true;

            return result;
        }

        #endregion


        #region "IEnumerable members"

        /// <summary>
        /// Gets the typed enumerator
        /// </summary>
        public IEnumerator<TObject> GetEnumerator()
        {
            return TypedResult.GetEnumerator();
        }


        /// <summary>
        /// Gets the enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return TypedResult.GetEnumerator();
        }

        #endregion


        #region "IQueryable Members"

        /// <summary>
        /// Returns the element type.
        /// </summary>
        public Type ElementType
        {
            get
            {
                return typeof(TObject);
            }
        }


        /// <summary>
        /// Query expression
        /// </summary>
        public Expression Expression
        {
            get
            {
                return mExpression ?? (mExpression = Expression.Constant(this));
            }
        }


        /// <summary>
        /// Query provider
        /// </summary>
        public IQueryProvider Provider
        {
            get
            {
                return mProvider ?? (mProvider = new CMSQueryProvider<TObject>(this));
            }
        }

        #endregion


        #region "ICMSQueryable members"

        /// <summary>
        /// Returns true if the given collection is offline (disconnected from the database)
        /// </summary>
        public bool IsOffline
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Creates the child collection based on the given provider
        /// </summary>
        /// <param name="settings">Query parameters</param>
        public ICMSQueryable<TObject> CreateChild(IDataQuerySettings settings)
        {
            var col = Clone();

            // Backup original columns
            var columns = col.SelectColumnsList;
            // Apply settings
            settings.ApplyParametersTo(col);

            // Also add columns from parent query
            columns.AddRangeUnique(settings.SelectColumnsList, false);
            col.SelectColumnsList = columns;

            return col;
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit operator for conversion from DataQuery class to DataSet
        /// </summary>
        /// <param name="query">Query object</param>
        public static explicit operator InfoDataSet<TObject>(ObjectQueryBase<TQuery, TObject> query)
        {
            if (query == null)
            {
                return null;
            }

            return query.TypedResult;
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for multiple objects query
    /// </summary>
    public class MultiObjectQueryBase<TQuery, TInnerQuery, TObject> :
        MultiQueryBase<TQuery, TInnerQuery>,
        IMultiObjectQuery<TQuery, TInnerQuery, TObject>,
        ICMSQueryable<TObject>
        where TQuery : MultiObjectQueryBase<TQuery, TInnerQuery, TObject>, new()
        where TObject : BaseInfo
        where TInnerQuery : AbstractQueryObject, IObjectQuery<TInnerQuery, TObject>, new()
    {
        #region "Variables"

        private IQueryProvider mProvider;
        private Expression mExpression;

        #endregion


        #region "Properties"

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
        public TObject FirstObject
        {
            get
            {
                return TypedResult.FirstOrDefault<TObject>();
            }
        }


        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return GetObjectType();
            }
            set
            {
                ModifySingleInnerQuery(q => q.ObjectType = value);
            }
        }


        /// <summary>
        /// If true, the query uses the object type condition.
        /// </summary>
        public bool UseObjectTypeCondition
        {
            get
            {
                // Set the object type of a single inner query if possible
                var q = GetSingleInnerQuery();
                if (q != null)
                {
                    return q.UseObjectTypeCondition;
                }

                throw new NotSupportedException("[MultiObjectQueryBase.UseObjectTypeCondition]: Cannot get flag use object type condition of a multi query that doesn't represent a single query.");
            }
            set
            {
                ModifySingleInnerQuery(q => q.UseObjectTypeCondition = value);
            }
        }

        #endregion


        #region "Methods"

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
            // If query is using single query, get object time from the query. 
            var singleObjectType = GetObjectType(false);

            // Provide initialization for info objects
            typedResult.Items.ObjectInitializer = data =>
            {
                // Get object type
                // If query is single query, use it's object type
                // If query is multi query, get object type of current data row from special type column
                // Otherwise create empty default info (depends on TObject) - info object collection creates empty objects as substitution for removed items.  
                var objectType = singleObjectType ?? DataHelper.GetStringValue(data, SystemColumns.SOURCE_TYPE, null) ?? typedResult.Items.ObjectType;

                // Create object
                return (TObject)ModuleManager.GetObject(data, objectType, true);
            };
        }


        /// <summary>
        /// Creates query for the given type
        /// </summary>
        /// <param name="type">Query type</param>
        protected override TInnerQuery CreateQuery(string type)
        {
            var q = new TInnerQuery();

            q.IsNested = true;

            // Initialize the inner query, and use type condition to match only that given object type
            q.ObjectType = type;
            q.UseObjectTypeCondition = true;

            return q;
        }


        /// <summary>
        /// Creates a new DataSet for the query results
        /// </summary>
        protected override DataSet NewDataSet()
        {
            return new InfoDataSet<TObject>();
        }


        /// <summary>
        /// Applies main query properties to the given query to ensure synchronized state before execution
        /// </summary>
        /// <param name="query">Query to prepare</param>
        /// <param name="multiQuery">If true, the query is an inner query within multi-query</param>
        protected override void ApplyProperties(IDataQuery query, bool multiQuery = false)
        {
            base.ApplyProperties(query, multiQuery);

            query.IncludeBinaryData = IncludeBinaryData;
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
        /// Modifies all inner queries with the given parameters
        /// </summary>
        /// <param name="p">Parameters for inner queries</param>
        protected TQuery ModifyAllInnerQueries(Action<TInnerQuery> p)
        {
            var result = GetTypedQuery();

            // Process all inner queries
            foreach (var query in result.QueriesList)
            {
                p(query);
            }

            // Process default query
            var defaultQuery = result.DefaultQuery;
            if (defaultQuery != null)
            {
                p(defaultQuery);
            }

            return result;
        }


        /// <summary>
        /// Modifies single inner query with the given parameters. Operation is not supported if multi query doesn't represent a single query source
        /// </summary>
        /// <param name="p">Parameters for inner query</param>
        protected TQuery ModifySingleInnerQuery(Action<TInnerQuery> p)
        {
            var result = GetTypedQuery();

            // Get single inner query
            var q = result.GetSingleInnerQuery();
            if (q != null)
            {
                p(q);
            }

            if (result.QueriesList.Count > 1)
            {
                throw new NotSupportedException("[MultiObjectQueryBase.ModifySingleInnerQuery]: This operation is not allowed for queries that don't represent a single source query.");
            }

            return result;
        }


        /// <summary>
        /// Filters the data to include only global objects.
        /// </summary>
        public TQuery OnlyGlobal()
        {
            return ModifyAllInnerQueries(q => q.OnlyGlobal());
        }


        /// <summary>
        /// Filters the data to include only site objects, but not global objects
        /// </summary>
        public TQuery ExceptGlobal()
        {
            return ModifyAllInnerQueries(q => q.ExceptGlobal());
        }


        /// <summary>
        /// Filters the data to include only records on the given site. If site binding is defined, filters the global objects by the site ID
        /// </summary>
        /// <param name="siteIdentifier">Site identifier</param>
        /// <param name="includeGlobal">If true, includes the global objects in case the object type covers both site and global objects</param>
        public virtual TQuery OnSite(SiteInfoIdentifier siteIdentifier, bool includeGlobal = false)
        {
            return ModifyAllInnerQueries(q => q.OnSite(siteIdentifier, includeGlobal));
        }


        /// <summary>
        /// Sets the where condition for a specific object ID
        /// </summary>
        /// <param name="objectId">Object ID</param>
        public TQuery WithID(int objectId)
        {
            return ModifyAllInnerQueries(q => q.WithID(objectId));

        }


        /// <summary>
        /// Sets the where condition for a specific object code name
        /// </summary>
        /// <param name="codeName">Code name</param>
        public TQuery WithCodeName(string codeName)
        {
            return ModifyAllInnerQueries(q => q.WithCodeName(codeName));
        }


        /// <summary>
        /// Sets the where condition for a specific object GUID
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public TQuery WithGuid(Guid guid)
        {
            return ModifyAllInnerQueries(q => q.WithGuid(guid));
        }


        /// <summary>
        /// Changes the query to use the given object type. Applies the object type condition to the query. The new object type must have the same class name as the original one.
        /// </summary>
        /// <param name="objectType">Object type for the query</param>
        public TQuery WithObjectType(string objectType)
        {
            return ModifySingleInnerQuery(q => q.WithObjectType(objectType));
        }


        /// <summary>
        /// Returns object type of objects returned by this query
        /// </summary>
        /// <param name="throwException">Throws exception when object type cannot be obtained 
        /// because multi query that doesn't represent a single query</param>
        /// <exception cref="NotSupportedException">When throwException is true and  multi query that doesn't represent a single query.</exception>
        protected string GetObjectType(bool throwException = true)
        {
            // Get the object type of a single inner query if possible
            var q = GetSingleInnerQuery();
            if (q != null)
            {
                return q.ObjectType;
            }

            if (throwException)
            {
                throw new NotSupportedException("[MultiObjectQueryBase.ObjectType]: Cannot get object type of a multi query that doesn't represent a single query.");
            }

            return null;
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
            settings.ApplyParametersTo(col);

            return col;
        }

        #endregion
    }
}

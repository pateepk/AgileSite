using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for the object query for a specific query type
    /// </summary>
    public interface IObjectQuery<TQuery, TObject> :
        IDataQuery<TQuery>,
        IObjectQuery
        where TObject : BaseInfo
    {
        /// <summary>
        /// Typed result
        /// </summary>
        InfoDataSet<TObject> TypedResult
        {
            get;
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
        /// To limit the number of fetched rows use <see cref="IDataQuerySettings{TQuery}.TopN"/>.
        /// </para>
        /// </remarks>
        [Obsolete("Use System.Linq.Enumerable.FirstOrDefault<TSource>(this System.Collections.Generic.IEnumerable<TSource>) extension method instead.")]
        TObject FirstObject
        {
            get;
        }

        /// <summary>
        /// Filters the data to include only global objects.
        /// </summary>
        TQuery OnlyGlobal();

        /// <summary>
        /// Filters the data to include only site objects, but not global objects
        /// </summary>
        TQuery ExceptGlobal();

        /// <summary>
        /// Filters the data to include only records on the given site. If site binding is defined, filters the global objects by the site ID
        /// </summary>
        /// <param name="siteIdentifier">Site identifier</param>
        /// <param name="includeGlobal">If true, includes the global objects in case the object type covers both site and global objects</param>
        TQuery OnSite(SiteInfoIdentifier siteIdentifier, bool includeGlobal = false);

        /// <summary>
        /// Sets the where condition for a specific object ID
        /// </summary>
        /// <param name="objectId">Object ID</param>
        TQuery WithID(int objectId);

        /// <summary>
        /// Sets the where condition for a specific object code name
        /// </summary>
        /// <param name="codeName">Code name</param>
        TQuery WithCodeName(string codeName);

        /// <summary>
        /// Sets the where condition for a specific object GUID
        /// </summary>
        /// <param name="guid">Object GUID</param>
        TQuery WithGuid(Guid guid);

        /// <summary>
        /// Changes the query to use the given object type. Applies the object type condition to the query. The new object type must have the same class name as the original one.
        /// </summary>
        /// <param name="objectType">Object type for the query</param>
        TQuery WithObjectType(string objectType);

        /// <summary>
        /// Creates a nested query from the given query
        /// </summary>
        ObjectQuery<TObject> AsNested();
    }


    /// <summary>
    /// Interface for the object query
    /// </summary>
    public interface IObjectQuery : IDataQuery
    {
        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the query uses the object type condition.
        /// </summary>
        bool UseObjectTypeCondition
        {
            get;
            set;
        }
    }
}
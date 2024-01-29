using CMS.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for info object collection
    /// </summary>
    public interface IInfoObjectCollection : IComparable, INamedEnumerable, IIndexable, INameIndexable, IVirtualTypedCollection, ICMSQueryable, IMacroSecurityCheckPermissions
    {
        #region "Properties"
        /// <summary>
        /// Collection name
        /// </summary>
        string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to load binary data into the collections.
        /// </summary>
        bool LoadBinaryData
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        WhereCondition Where
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to get.
        /// </summary>
        string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Columns list of SQL order by expression (e.g.: 'UserName, UserID DESC')
        /// </summary>
        new string OrderByColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N objects.
        /// </summary>
        int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the read only access to the data is enforced within the transaction.
        /// </summary>
        bool EnforceReadOnlyDataAccess
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the collection uses default cache dependencies to flush it's content
        /// </summary>
        bool UseDefaultCacheDependencies
        {
            get;
            set;
        }
        
       
        /// <summary>
        /// Site ID to filter the collection to a particular site
        /// </summary>
        int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the document is the last version (retrieved using DocumentHelper.GetDocument).
        /// </summary>
        bool IsLastVersion
        {
            get;
            set;
        }

        
        /// <summary>
        /// Name column name
        /// </summary>
        string NameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Source data for the collection
        /// </summary>
        DataSet SourceData
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the collection uses the type condition to get the data
        /// </summary>
        bool UseObjectTypeCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        string ObjectType
        {
            get;
        }


        /// <summary>
        /// If true, the paging of the data is allowed (data is loaded in batches).
        /// </summary>
        bool AllowPaging
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the collection check license when getting data
        /// </summary>
        bool CheckLicense
        {
            get;
            set;
        }


        /// <summary>
        /// Type info for the collection object.
        /// </summary>
        ObjectTypeInfo TypeInfo
        {
            get;
        }

        
        /// <summary>
        /// Dynamic where condition.
        /// </summary>
        Func<string> DynamicWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Parent object. Instance of object which contains this collection as it's inner field. 
        /// </summary>
        BaseInfo ParentObject
        {
            get;
        }


        /// <summary>
        /// Object instance of the specified type.
        /// </summary>
        BaseInfo Object
        {
            get;
            set;
        }


        /// <summary>
        /// Fields wrappers
        /// </summary>
        CollectionPropertyTransformation<CollectionPropertyWrapper<BaseInfo>> ItemsAsFields
        {
            get;
        }


        /// <summary>
        /// Related objects wrappers
        /// </summary>
        CollectionPropertyTransformation<IInfoObjectCollection> FieldsAsObjects
        {
            get;
        }


        /// <summary>
        /// Collection of the object display names
        /// </summary>
        CollectionPropertyWrapper<BaseInfo> DisplayNames
        {
            get;
        }


        /// <summary>
        /// Collection of the object code names
        /// </summary>
        CollectionPropertyWrapper<BaseInfo> CodeNames
        {
            get;
        }


        /// <summary>
        /// Collection of the object GUIDs
        /// </summary>
        CollectionPropertyWrapper<BaseInfo> GUIDs
        {
            get;
        }


        /// <summary>
        /// Collection of the object IDs
        /// </summary>
        CollectionPropertyWrapper<BaseInfo> IDs
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        void SubmitChanges();


        /// <summary>
        /// Disconnects the collection from the database
        /// </summary>
        void Disconnect();

        
        /// <summary>
        /// Clears the collection cache
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Creates the clone of this collection.
        /// </summary>
        IInfoObjectCollection CloneCollection();
        

        /// <summary>
        /// Gets the collection of objects that are referenced by the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        IInfoObjectCollection GetFieldsAsObjects(string propertyName);


        /// <summary>
        /// Makes a wrapper of the collection with specified type of the items.
        /// </summary>
        /// <typeparam name="TType">Target type of the items</typeparam>
        IQueryable<TType> As<TType>() 
            where TType : BaseInfo;
        

        /// <summary>
        /// Returns the updatable collection of fields of collection items
        /// </summary>
        /// <param name="propertyName">Name of the properties to extract</param>
        CollectionPropertyWrapper<BaseInfo> GetItemsAsFields(string propertyName);


        /// <summary>
        /// Clears the data in the collection and loads objects from the given list.
        /// </summary>
        /// <param name="objects">Objects data to load</param>
        void Load(IEnumerable<BaseInfo> objects);


        /// <summary>
        /// Changes the parent of the collection
        /// </summary>
        /// <param name="parentObject">Parent object</param>
        /// <param name="parentStorage">Parent storage</param>
        void ChangeParent(BaseInfo parentObject, ICMSStorage parentStorage);

        
        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Object to add</param>
        void Add(IEnumerable<BaseInfo> objects);


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Objects to add</param>
        void Add(params BaseInfo[] objects);

        #endregion
    }


    /// <summary>
    /// Interface for generic info object collection
    /// </summary>
    public interface IInfoObjectCollection<out TInfo> : 
        IInfoObjectCollection, 
        ICMSQueryable<TInfo>,
        INamedEnumerable<TInfo>,
        IIndexable<TInfo>
        where TInfo : BaseInfo
    {
        #region "Properties"

        /// <summary>
        /// Returns the first item of the collection
        /// </summary>
        TInfo FirstItem
        {
            get;
        }


        /// <summary>
        /// Returns the last item of the collection
        /// </summary>
        TInfo LastItem
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the clone of the collection with specified column not being empty
        /// </summary>
        /// <param name="columnName">Column name</param>
        IInfoObjectCollection<TInfo> GetSubsetWhereNotEmpty(string columnName);


        /// <summary>
        /// Returns the clone of the collection with specified where condition applied
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        IInfoObjectCollection<TInfo> GetSubsetWhere(string whereCondition);

        #endregion
    }
}

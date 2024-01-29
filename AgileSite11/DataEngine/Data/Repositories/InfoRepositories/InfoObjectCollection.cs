using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// General info object collection
    /// </summary>
    public class InfoObjectCollection : InfoObjectCollection<BaseInfo>
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public InfoObjectCollection(string objectType)
            : base(objectType)
        {
            Init(objectType);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static IInfoObjectCollection New(string objectType)
        {
            return new InfoObjectCollection(objectType);
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<BaseInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = new InfoObjectCollection(ObjectType);
            CopyPropertiesTo(result);

            return result;
        }
        
        #endregion
    }


    /// <summary>
    /// Generic info object collection.
    /// </summary>
    [DebuggerDisplay("{GetType()}: {ObjectType}")]
    public class InfoObjectCollection<TInfo> : AbstractHierarchicalObject<InfoObjectCollection<TInfo>>, IInfoObjectCollection<TInfo>, IDisposable
        where TInfo : BaseInfo
    {
        #region "Variables"

        /// <summary>
        /// Number of disconnected references for this collection 
        /// </summary>
        private int mDisconnectedCount;


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        private bool mIsCachedObject;


        /// <summary>
        /// Collection name
        /// </summary>
        private string mName;


        /// <summary>
        /// Object type.
        /// </summary>
        private string mObjectType;


        /// <summary>
        /// Instance GUID
        /// </summary>
        private readonly Guid mInstanceGuid = Guid.NewGuid();


        /// <summary>
        /// Object instance of specified type.
        /// </summary>
        private BaseInfo mObject;


        /// <summary>
        /// Parent object instance
        /// </summary>
        private BaseInfo mParentObject;


        /// <summary>
        /// Where condition
        /// </summary>
        private WhereCondition mWhere;


        /// <summary>
        /// Order by expression.
        /// </summary>
        private string mOrderBy;


        /// <summary>
        /// Related objects wrappers
        /// </summary>
        private CollectionPropertyTransformation<IInfoObjectCollection> mFieldsAsObjects;


        /// <summary>
        /// Fields wrappers
        /// </summary>
        private CollectionPropertyTransformation<CollectionPropertyWrapper<BaseInfo>> mItemsAsFields;


        /// <summary>
        /// Flag indicating if the cache callback is already registered
        /// </summary>
        private bool mCacheCallbackRegistered;


        /// <summary>
        /// If true, the collection uses default cache dependencies to flush it's content
        /// </summary>
        private bool mUseDefaultCacheDependencies = true;


        /// <summary>
        /// Total number of items.
        /// </summary>
        private int mCount = -1;

        
        /// <summary>
        /// List of custom cache dependencies
        /// </summary>
        private List<string> mCustomCacheDependencies;


        /// <summary>
        /// Name column
        /// </summary>
        private string mNameColumn = ObjectTypeInfo.COLUMN_NAME_UNKNOWN;


        /// <summary>
        /// Indicates whether to load binary data into the collections.
        /// </summary>
        private bool mLoadBinaryData;


        /// <summary>
        /// If true, the paging of the data is allowed (data is loaded in batches).
        /// </summary>
        private bool mAllowPaging = true;


        /// <summary>
        /// Items collection.
        /// </summary>
        private List<TInfo> mItems;


        /// <summary>
        /// Objects by name.
        /// </summary>
        private Hashtable mObjectsByName;


        /// <summary>
        /// Difference in count of the items from data source and current number of items
        /// </summary>
        private int mCountDifference;


        /// <summary>
        /// Object for locking this instance context
        /// </summary>
        private readonly object lockObject = new object();


        /// <summary>
        /// Removed object
        /// </summary>
        private TInfo mRemovedObject;
        

        /// <summary>
        /// Deleted items removed from the collection.
        /// </summary>
        private List<TInfo> mDeletedItems;


        /// <summary>
        /// New items added to the collection.
        /// </summary>
        private List<TInfo> mNewItems;


        /// <summary>
        /// Number of enumerators accessing the items collection
        /// </summary>
        private int mEnumerators;


        /// <summary>
        /// If true, the collection will be cleared after the last enumeration finishes
        /// </summary>
        private bool mClearAfterEnumeration;


        /// <summary>
        /// Query provider
        /// </summary>
        private IQueryProvider mProvider;
        

        /// <summary>
        /// Expression
        /// </summary>
        private Expression mExpression;


        /// <summary>
        /// Provides a way how to initialize the object instance based on given data 
        /// </summary>
        public Func<DataRow, TInfo> ObjectInitializer;

        #endregion


        #region "Properties"


        /// <summary>
        /// If true, the read only access to the data is enforced within the transaction.
        /// </summary>
        public virtual bool EnforceReadOnlyDataAccess
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the document is the last version (retrieved using DocumentHelper.GetDocument).
        /// </summary>
        public virtual bool IsLastVersion
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N objects.
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to get.
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        public WhereCondition Where
        {
            get
            {
                return mWhere ?? (mWhere = new WhereCondition());
            }
            set
            {
                mWhere = value;
            }
        }



        /// <summary>
        /// Dynamic where condition.
        /// </summary>
        public Func<string> DynamicWhereCondition
        {
            get;
            set;
        }

        
        /// <summary>
        /// Result type
        /// </summary>
        public Type ItemType
        {
            get
            {
                return typeof(TInfo);
            }
        }


        /// <summary>
        /// Returns true, if the collection is offline (not backed up by the database but by the source data)
        /// </summary>
        public bool IsOffline
        {
            get
            {
                return (SourceData != null);
            }
        }


        /// <summary>
        /// Returns the first item of the collection
        /// </summary>
        public TInfo FirstItem
        {
            get
            {
                return this[0];
            }
        }


        /// <summary>
        /// Returns the last item of the collection
        /// </summary>
        public TInfo LastItem
        {
            get
            {
                var count = Count;
                if (count > 0)
                {
                    // Get the last item
                    return this[Count - 1];
                }

                return default(TInfo);
            }
        }


        /// <summary>
        /// Returns the number of items.
        /// </summary>
        public int Count
        {
            get
            {
                // Return the number of items
                return InternalCount + mCountDifference;
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
                return mProvider ?? (mProvider = CreateQueryProvider());
            }
        }


        /// <summary>
        /// Related objects wrappers
        /// </summary>
        public CollectionPropertyTransformation<IInfoObjectCollection> FieldsAsObjects
        {
            get
            {
                return mFieldsAsObjects ?? (mFieldsAsObjects = new CollectionPropertyTransformation<IInfoObjectCollection>(this, (col, field) => col.GetFieldsAsObjects(field), col => col.Object.TypeInfo.ReferenceColumnNames));
            }
        }


        /// <summary>
        /// Fields wrappers
        /// </summary>
        public CollectionPropertyTransformation<CollectionPropertyWrapper<BaseInfo>> ItemsAsFields
        {
            get
            {
                return mItemsAsFields ?? (mItemsAsFields = new CollectionPropertyTransformation<CollectionPropertyWrapper<BaseInfo>>(this, (col, field) => col.GetItemsAsFields(field), col => col.Object.ColumnNames));
            }
        }


        /// <summary>
        /// Collection of the object display names
        /// </summary>
        public CollectionPropertyWrapper<BaseInfo> DisplayNames
        {
            get
            {
                var ti = TypeInfo;

                return (ti.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? ItemsAsFields[ti.DisplayNameColumn] : null;
            }
        }


        /// <summary>
        /// Collection of the object code names
        /// </summary>
        public CollectionPropertyWrapper<BaseInfo> CodeNames
        {
            get
            {
                var ti = TypeInfo;

                return (ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? ItemsAsFields[ti.CodeNameColumn] : null;
            }
        }


        /// <summary>
        /// Returns the element type.
        /// </summary>
        public Type ElementType
        {
            get
            {
                return typeof(TInfo);
            }
        }


        /// <summary>
        /// Collection of the object GUIDs
        /// </summary>
        public CollectionPropertyWrapper<BaseInfo> GUIDs
        {
            get
            {
                var ti = TypeInfo;

                return (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? ItemsAsFields[ti.GUIDColumn] : null;
            }
        }


        /// <summary>
        /// Collection of the object IDs
        /// </summary>
        public CollectionPropertyWrapper<BaseInfo> IDs
        {
            get
            {
                var ti = TypeInfo;

                return (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) ? ItemsAsFields[ti.IDColumn] : null;
            }
        }


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        public bool IsCachedObject
        {
            get
            {
                if ((ParentObject != null) && (ParentObject.Generalized.IsCachedObject))
                {
                    return true;
                }

                if ((ParentStorage != null) && ParentStorage.IsCachedObject)
                {
                    return true;
                }

                return mIsCachedObject;
            }
            set
            {
                mIsCachedObject = value;
            }
        }



        /// <summary>
        /// Returns true if the collection is disconnected from the data source
        /// </summary>
        public virtual bool IsDisconnected
        {
            get
            {
                if ((ParentObject != null) && (ParentObject.Generalized.IsDisconnected))
                {
                    return true;
                }

                if ((ParentStorage != null) && ParentStorage.IsDisconnected)
                {
                    return true;
                }

                return ((mDisconnectedCount > 0) || CMSActionContext.CurrentDisconnected);
            }
        }


        /// <summary>
        /// Parent object. Instance of object which contains this collection as it's inner field. 
        /// </summary>
        public virtual BaseInfo ParentObject
        {
            get
            {
                return mParentObject;
            }
            set
            {
                if (value != null)
                {
                    var expected = Object.TypeInfo.ParentObjectType;

                    // Check parent object type. 
                    if (!value.TypeInfo.RepresentsType(expected))
                    {
                        throw new Exception($"Given parent object type {value.TypeInfo.OriginalObjectType} cannot be assigned to this collection because it is not configured as parent object of type {ObjectType}. " +
                                            $"Expected object type is {expected}. Configure the object as parent storage if you want to connect the collection to the object hierarchy.");
                    }
                }

                mParentObject = value;
            }
        }


        /// <summary>
        /// Parent storage
        /// </summary>
        public ICMSStorage ParentStorage
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns the object type of the objects stored within the collection.
        /// </summary>
        public virtual string ObjectType
        {
            get
            {
                return mObjectType;
            }
        }


        /// <summary>
        /// Object instance of the specified type.
        /// </summary>
        public virtual BaseInfo Object
        {
            get
            {
                if (mObject != null)
                {
                    return mObject;
                }

                // Get new empty object from ObjectType if specified otherwise use generics
                mObject = !String.IsNullOrEmpty(ObjectType) ? ModuleManager.GetObject(ObjectType) : ObjectFactory<TInfo>.New();

                return mObject;
            }
            set
            {
                mObject = value;

                if (mObject != null)
                {
                    mObjectType = mObject.TypeInfo.ObjectType;
                }
            }
        }


        /// <summary>
        /// Returns the items collection.
        /// </summary>
        protected virtual List<TInfo> Items
        {
            get
            {
                return mItems ?? (mItems = new List<TInfo>());
            }
        }


        /// <summary>
        /// Returns the items collection.
        /// </summary>
        protected List<TInfo> DeletedItems
        {
            get
            {
                return mDeletedItems ?? (mDeletedItems = new List<TInfo>());
            }
            set
            {
                mDeletedItems = value;
            }
        }


        /// <summary>
        /// Returns the items collection.
        /// </summary>
        protected List<TInfo> NewItems
        {
            get
            {
                return mNewItems ?? (mNewItems = new List<TInfo>());
            }
            set
            {
                mNewItems = value;
            }
        }


        /// <summary>
        /// Defines the removed object within collection
        /// </summary>
        protected TInfo RemovedObject
        {
            get
            {
                if (mRemovedObject == null)
                {
                    lock (lockObject)
                    {
                        if (mRemovedObject == null)
                        {
                            mRemovedObject = CreateNewObject(null);
                        }
                    }
                }

                return mRemovedObject;
            }
        }



        /// <summary>
        /// Objects by name collection.
        /// </summary>
        protected virtual Hashtable ObjectsByName
        {
            get
            {
                return mObjectsByName ?? (mObjectsByName = new Hashtable());
            }
        }


        /// <summary>
        /// Order by expression.
        /// </summary>
        public string OrderByColumns
        {
            get
            {
                return mOrderBy ?? (mOrderBy = TypeInfo.DefaultOrderBy);
            }
            set
            {
                mOrderBy = value;
            }
        }


        /// <summary>
        /// Type info for the collection object.
        /// </summary>
        public virtual ObjectTypeInfo TypeInfo
        {
            get
            {
                return Object?.TypeInfo;
            }
        }


        /// <summary>
        /// Collection name. Returns object type if name is not defined
        /// </summary>
        public string Name
        {
            get
            {
                return mName ?? ObjectType;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Name column name
        /// </summary>
        public virtual string NameColumn
        {
            get
            {
                if (mNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    mNameColumn = Object.Generalized.CodeNameColumn;
                }

                return mNameColumn;
            }
            set
            {
                mNameColumn = value;

                AutomaticNameColumn = (value == ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || string.Equals(value, Object.Generalized.CodeNameColumn, StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// If true, the automatic name column is used within this collection
        /// </summary>
        protected bool AutomaticNameColumn
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the paging of the data is allowed (data is loaded in batches).
        /// </summary>
        public bool AllowPaging
        {
            get
            {
                return mAllowPaging;
            }
            set
            {
                if (mAllowPaging & !value & !IsDisconnected)
                {
                    // Delete the collection when changing from batch to whole load
                    mItems = null;
                }
                mAllowPaging = value;
            }
        }


        /// <summary>
        /// Page size for loading of the items.
        /// </summary>
        public int PageSize
        {
            get;
            set;
        } = 100;


        /// <summary>
        /// Source data for the collection
        /// </summary>
        public DataSet SourceData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to load binary data into the collections.
        /// </summary>
        public bool LoadBinaryData
        {
            get
            {
                return mLoadBinaryData;
            }
            set
            {
                if ((mLoadBinaryData != value) && !IsDisconnected)
                {
                    // If the value changed, delete the collection
                    mItems = null;
                }

                mLoadBinaryData = value;
            }
        }


        /// <summary>
        /// Site ID to filter the collection to a particular site
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the names in enumeration are sorted
        /// </summary>
        public bool SortNames
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the items in the collection have names
        /// </summary>
        public bool ItemsHaveNames
        {
            get
            {
                return (NameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !String.IsNullOrEmpty(NameColumn);
            }
        }


        /// <summary>
        /// Internal count of the items
        /// </summary>
        protected int InternalCount
        {
            get
            {
                // Ensure the clear cache callback
                RegisterClearCacheCallback();

                if (mCount >= 0)
                {
                    return mCount;
                }

                // Load some data
                GetItem(0);

                // Make sure the number of items is set
                if (mCount < 0)
                {
                    mCount = 0;
                }

                return mCount;
            }
        }


        /// <summary>
        /// If true, the collection uses default cache dependencies to flush it's content
        /// </summary>
        public bool UseDefaultCacheDependencies
        {
            get
            {
                return mUseDefaultCacheDependencies;
            }
            set
            {
                mUseDefaultCacheDependencies = value;

                CacheDependenciesChanged();
            }
        }


        /// <summary>
        /// If true, the collection check license when getting data
        /// </summary>
        public bool CheckLicense
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the collection uses the type condition to get the data
        /// </summary>
        public bool UseObjectTypeCondition
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public virtual IInfoObjectCollection<TInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = (InfoObjectCollection<TInfo>)Activator.CreateInstance(GetType());
            CopyPropertiesTo(result);

            return result;
        }

        /// <summary>
        /// Creates the clone of this collection.
        /// </summary>
        public IInfoObjectCollection CloneCollection()
        {
            return Clone();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public InfoObjectCollection()
        {
            Init(null);

            InitObjectType();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        protected InfoObjectCollection(string objectType)
        {
            Init(objectType);
        }


        /// <summary>
        /// Constructor. Creates a static collection populated from DataSet
        /// </summary>
        /// <param name="sourceData">Source DataSet</param>
        public InfoObjectCollection(DataSet sourceData)
        {
            Init(null);

            UseData(sourceData);

            InitObjectType();
        }


        /// <summary>
        /// Inits the correct object type from default object
        /// </summary>
        private void InitObjectType()
        {
            var defaultObject = Object;
            if (defaultObject != null)
            {
                mObjectType = defaultObject.TypeInfo.ObjectType;
            }
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            RemoveClearCacheCallback();
        }


        /// <summary>
        /// Initializes the collection to use specific object type
        /// </summary>
        /// <param name="objectType">Object type to use</param>
        protected void Init(string objectType)
        {
            mObjectType = objectType;

            if (objectType != null)
            {
                mName = TranslationHelper.GetSafeClassName(mObjectType);

                // If object type is explicitly defined, use its condition to filter objects
                UseObjectTypeCondition = true;
            }

            SiteID = -1;
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// Disconnects the collection from the database
        /// </summary>
        public void Disconnect()
        {
            Interlocked.Increment(ref mDisconnectedCount);
        }


        /// <summary>
        /// Reconnects the collection to the database
        /// </summary>
        public void Reconnect()
        {
            Interlocked.Decrement(ref mDisconnectedCount);

            if (mDisconnectedCount < 0)
            {
                mDisconnectedCount = 0;
            }
        }


        /// <summary>
        /// Gets the data set of the data behind this collection
        /// </summary>
        protected DataSet GetFullData()
        {
            if (SourceData != null)
            {
                return SourceData;
            }

            // Query the database
            int totalRecords = 0;

            return GetData(null, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Gets an empty object for this collection
        /// </summary>
        public object GetEmptyObject()
        {
            return CreateNewObject(null);
        }


        /// <summary>
        /// Makes the collection empty.
        /// </summary>
        public void MakeEmpty()
        {
            mCount = 0;
        }


        /// <summary>
        /// Initializes the collection with the given source data
        /// </summary>
        /// <param name="sourceData">Source data for the collection</param>
        protected void UseData(DataSet sourceData)
        {
            SourceData = sourceData;
        }


        /// <summary>
        /// Creates a LINQ query provider for this collection
        /// </summary>
        protected IQueryProvider CreateQueryProvider()
        {
            return new CMSQueryProvider<TInfo>(this);
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("ItemsAsFields", m => ((IInfoObjectCollection<TInfo>)m).ItemsAsFields);
            RegisterProperty("FieldsAsObjects", m => ((IInfoObjectCollection<TInfo>)m).FieldsAsObjects);

            // Specific property lists
            if (Object.TypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("CodeNames", m => ((IInfoObjectCollection<TInfo>)m).CodeNames);
            }
            if (Object.TypeInfo.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("DisplayNames", m => ((IInfoObjectCollection<TInfo>)m).DisplayNames);
            }
            if (Object.TypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("GUIDs", m => ((IInfoObjectCollection<TInfo>)m).GUIDs);
            }
            if (Object.TypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                RegisterProperty("IDs", m => ((IInfoObjectCollection<TInfo>)m).IDs);
            }

            RegisterProperty<TInfo>("FirstItem", m => ((IInfoObjectCollection<TInfo>)m).FirstItem);
            RegisterProperty<TInfo>("LastItem", m => ((IInfoObjectCollection<TInfo>)m).LastItem);
        }


        /// <summary>
        /// Registers the Columns of this object
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("Count", m => ((IInfoObjectCollection<TInfo>)m).Count);
        }


        /// <summary>
        /// Changes the parent of the collection
        /// </summary>
        /// <param name="parentObject">Parent object</param>
        /// <param name="parentStorage">Parent storage</param>
        public void ChangeParent(BaseInfo parentObject, ICMSStorage parentStorage)
        {
            ParentStorage = parentStorage;
            ParentObject = parentObject;

            if ((parentObject != null) && (parentObject.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                SiteID = parentObject.Generalized.ObjectSiteID;
            }
        }


        /// <summary>
        /// Creates new instance of the encapsulated object.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        public virtual TInfo CreateNewObject(DataRow dr)
        {
            TInfo result = (ObjectInitializer != null) ? ObjectInitializer(dr) : (TInfo)ModuleManager.GetObject(dr, ObjectType, true);

            // Ensure binary data
            if ((dr != null) && LoadBinaryData)
            {
                result.Generalized.EnsureBinaryData();
            }

            return result;
        }


        /// <summary>
        /// Gets the unique object name from the given object.
        /// </summary>
        /// <param name="obj">Object</param>
        public string GetObjectName(object obj)
        {
            return GetObjectName((TInfo)obj);
        }


        /// <summary>
        /// Gets the unique object name from the given object.
        /// </summary>
        /// <param name="obj">Object</param>
        public virtual string GetObjectName(TInfo obj)
        {
            GeneralizedInfo generalizedInfo = obj.Generalized;

            // Register the object by name
            string nameColumn = NameColumn;
            if (nameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // If code name exists, the name in this collection is code name
                return ValidationHelper.GetString(generalizedInfo.GetValue(nameColumn), null);
            }

            if (generalizedInfo.TypeInfo.IsSiteBinding)
            {
                // If the object is site binding, the name is site name of the bound site
                return generalizedInfo.ObjectSiteName;
            }

            return null;
        }


        /// <summary>
        /// Gets the complete where condition for the collection
        /// </summary>
        public virtual WhereCondition GetCompleteWhereCondition()
        {
            var where = new WhereCondition(Where);

            // Add dynamic where condition if set
            if (DynamicWhereCondition != null)
            {
                where.Where(DynamicWhereCondition());
            }

            // Handle the site
            if (!TypeInfo.IsSiteBinding && (TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                if (SiteID == 0)
                {
                    // Global objects only
                    where.WhereNull(TypeInfo.SiteIDColumn);
                }
                else if (SiteID > 0)
                {
                    // Site objects
                    where.WhereEquals(TypeInfo.SiteIDColumn, SiteID);
                }
            }

            return where;
        }


        /// <summary>
        /// Gets the where condition for the given object name.
        /// </summary>
        /// <param name="name">Object name</param>
        public virtual IWhereCondition GetNameWhereCondition(string name)
        {
            if (NameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Get default where condition
                return new WhereCondition().WhereEquals(NameColumn, name);
            }

            GeneralizedInfo obj = Object;

            var ti = obj.TypeInfo;
            if (ti.IsSiteBinding)
            {
                // If the object is site binding, query by site ID
                int siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, name);
                if (siteId > 0)
                {
                    return new WhereCondition().WhereEquals(ti.SiteIDColumn, siteId);
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the object by its name.
        /// </summary>
        /// <param name="name">Name of the object</param>
        private TInfo GetObjectByName(string name)
        {
            // No name, no object
            if (String.IsNullOrEmpty(name))
            {
                return default(TInfo);
            }

            // Get the registered object
            return (TInfo)ObjectsByName[name.ToLowerInvariant()] ?? GetObjectByNameInternal(name);
        }


        /// <summary>
        /// Gets the object by its name, internal representation that gets data from database
        /// </summary>
        /// <param name="name">Name of the object</param>
        protected virtual TInfo GetObjectByNameInternal(string name)
        {
            TInfo result = null;
            var ti = TypeInfo;

            if (AutomaticNameColumn && (ti.ProviderType != null) && !ti.IsSiteBinding && Where.WhereIsEmpty && (DynamicWhereCondition == null))
            {
                if (NameColumn == ti.GUIDColumn)
                {
                    // Convert name to guid
                    Guid guid = ValidationHelper.GetGuid(name, Guid.Empty);
                    if (guid != Guid.Empty)
                    {
                        // Get through the provider object to handle hash tables
                        if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            // Site object
                            result = (TInfo)ProviderHelper.GetInfoByGuid(ObjectType, guid, SiteID);
                        }
                        else
                        {
                            // Global object
                            result = (TInfo)ProviderHelper.GetInfoByGuid(ObjectType, guid);
                        }
                    }
                }
                else
                {
                    // Get through the provider object to handle hash tables
                    if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Site object
                        result = (TInfo)ProviderHelper.GetInfoByName(ObjectType, name, SiteID);
                    }
                    else
                    {
                        // Global object
                        result = (TInfo)ProviderHelper.GetInfoByName(ObjectType, name);
                    }
                }
            }
            else
            {
                // If result not found, get the object from database
                var where = GetNameWhereCondition(name);
                int total = 0;

                DataSet ds = GetData(where, 0, 0, ref total);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Create the object
                    result = CreateNewObject(ds.Tables[0].Rows[0]);
                }
            }

            // Validate result parent
            if ((result != null) && (ParentObject != null))
            {
                // If the child type is correct, check the parent object to restrict access only to children
                if ((result.TypeInfo.ParentObjectType == ParentObject.TypeInfo.ObjectType) && !result.Generalized.IsChildOf(ParentObject))
                {
                    result = null;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the data for the collection.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="offset">Starting offset for the data</param>
        /// <param name="maxRecords">Maximum number of records to get</param>
        /// <param name="totalRecords">Returning total number of records</param>
        protected virtual DataSet GetData(IWhereCondition where, int offset, int maxRecords, ref int totalRecords)
        {
            // No results if disconnected
            if (IsDisconnected)
            {
                return null;
            }

            // Get where
            var completeWhere = GetCompleteWhereCondition().Where(where);

            // Setup the query
            var q = Object.Generalized.GetDataQuery(
                UseObjectTypeCondition,
                s => s
                    .Where(completeWhere)
                    .OrderBy(OrderByColumns)
                    .TopN(TopN)
                    .Columns(Columns),
                CheckLicense
            );

            q.IncludeBinaryData = LoadBinaryData;

            q.Offset = offset;
            q.MaxRecords = maxRecords;

            // Get results
            var result = q.Result;
            totalRecords = q.TotalRecords;

            return result;
        }


        /// <summary>
        /// Copies the properties of this collection to the other collection
        /// </summary>
        /// <param name="col">Target collection</param>
        protected virtual void CopyPropertiesTo(IInfoObjectCollection col)
        {
            // Do not copy instance specific properties (e.g. ParentObject, IsCachedObject, IsDisconnected).

            col.Object = Object;

            col.IsLastVersion = IsLastVersion;
            col.EnforceReadOnlyDataAccess = EnforceReadOnlyDataAccess;
            col.CheckLicense = CheckLicense;
            col.UseDefaultCacheDependencies = UseDefaultCacheDependencies;

            col.Columns = Columns;
            col.Where = Where.Clone();
            col.DynamicWhereCondition = DynamicWhereCondition;
            col.OrderByColumns = OrderByColumns;
            col.TopN = TopN;

            col.Name = mName;
            col.NameColumn = NameColumn;
            col.SiteID = SiteID;
            col.UseObjectTypeCondition = UseObjectTypeCondition;

            col.LoadBinaryData = LoadBinaryData;
            col.AllowPaging = AllowPaging;
            col.SourceData = SourceData;
        }


        /// <summary>
        /// Ensures the proper values for the given object
        /// </summary>
        /// <param name="item">Item in which ensure the values</param>
        protected virtual void EnsureObjectValues(BaseInfo item)
        {
            // Ensure the parent object ID
            if (ParentObject != null)
            {
                item.Generalized.ObjectParent = ParentObject;
            }
        }


        /// <summary>
        /// Clears the current items in the collection.
        /// </summary>
        public virtual void Clear()
        {
            lock (lockObject)
            {
                if (mEnumerators > 0)
                {
                    // If enumeration occurs, clear afterwards
                    mClearAfterEnumeration = true;
                }
                else
                {
                    // Clear now
                    mItems = null;
                    mCount = -1;
                    mCountDifference = 0;
                }
            }
        }


        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public virtual void SubmitChanges()
        {
            bool clearItems = false;

            // Submit changed items
            if (mItems != null)
            {
                foreach (var item in mItems)
                {
                    if (item != null)
                    {
                        // Ensure the object values
                        EnsureObjectValues(item);

                        item.SubmitChanges(true);
                    }
                }
            }

            // Delete the deleted items
            if (mDeletedItems != null)
            {
                foreach (var item in mDeletedItems)
                {
                    if (item != null)
                    {
                        // Ensure the object values
                        EnsureObjectValues(item);

                        // Delete the object
                        item.Generalized.DeleteObject();
                        clearItems = true;
                    }
                }

                mDeletedItems = null;
            }

            // Add the new items
            if (mNewItems != null)
            {
                foreach (var item in mNewItems)
                {
                    if (item != null)
                    {
                        // Ensure the object values
                        EnsureObjectValues(item);

                        // Save the object
                        item.SubmitChanges(true);
                        clearItems = true;
                    }
                }

                mNewItems = null;
            }

            // Clear the items if some changes are done
            if (clearItems)
            {
                Clear();
            }
        }


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Objects to add</param>
        public virtual void Add(params BaseInfo[] objects)
        {
            if (objects == null)
            {
                return;
            }

            // Add all objects
            foreach (var infoObj in objects)
            {
                if (infoObj != null)
                {
                    infoObj.Generalized.ObjectID = 0;

                    Add(new[] { infoObj } as IEnumerable<BaseInfo>);
                }
            }
        }


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Object to add</param>
        public void Add(IEnumerable<BaseInfo> objects)
        {
            if (objects == null)
            {
                return;
            }

            if (mNewItems == null)
            {
                mNewItems = new List<TInfo>();
            }

            // Add all the objects
            foreach (BaseInfo obj in objects)
            {
                if (obj != null)
                {
                    ValidateItem(obj);

                    mNewItems.Add((TInfo)obj);

                    mCountDifference++;
                }
            }
        }


        /// <summary>
        /// Removes the specified object from the collection.
        /// </summary>
        /// <param name="objects">Object to remove</param>
        public virtual void Remove(params TInfo[] objects)
        {
            Remove((IEnumerable<TInfo>)objects);
        }


        /// <summary>
        /// Removes the specified object from the collection.
        /// </summary>
        /// <param name="objects">Object to remove</param>
        public void Remove(IEnumerable<TInfo> objects)
        {
            if (objects == null)
            {
                return;
            }

            // Ensure deleted items collection
            if (mDeletedItems == null)
            {
                mDeletedItems = new List<TInfo>();
            }

            // Add all the objects
            foreach (TInfo obj in objects)
            {
                if (obj != null)
                {
                    ValidateItem(obj);

                    mDeletedItems.Add(obj);

                    // Remove the item from existing collections
                    bool removed = RemoveItem(mItems, obj) || RemoveItem(mNewItems, obj);
                    if (removed)
                    {
                        mCountDifference--;
                    }
                }
            }
        }


        /// <summary>
        /// Removes the item from the given list of items
        /// </summary>
        /// <param name="items">List of items</param>
        /// <param name="obj">Object to remove</param>
        private bool RemoveItem(List<TInfo> items, TInfo obj)
        {
            if (items == null)
            {
                return false;
            }

            // Remove from current items if present
            int index = items.IndexOf(obj);
            if (index >= 0)
            {
                items[index] = RemovedObject;

                return true;
            }

            return false;
        }


        /// <summary>
        /// Validates whether the item can be member of the collection (collection can work with it). Returns true if item is valid.
        /// </summary>
        /// <param name="item">Item to validate</param>
        /// <param name="throwException">If true, the method throws exception in case validation fails</param>
        protected bool ValidateItem(BaseInfo item, bool throwException = true)
        {
            var ti = item.TypeInfo;

            // Check valid object type
            var valid = (ti.ObjectType == ObjectType);
            if (!valid && throwException)
            {
                throw new Exception($"Object '{item.GetType().Name}' of type '{ti.ObjectType}' cannot be used with this collection. Expected object type is '{ObjectType}'.");
            }

            return valid;
        }


        /// <summary>
        /// Makes a wrapper of the collection with specified type of the items.
        /// </summary>
        /// <typeparam name="TType">Target type of the items</typeparam>
        public virtual IQueryable<TType> As<TType>()
            where TType : BaseInfo
        {
            if (Object is TType)
            {
                // Convert to typed collection
                var result = new InfoObjectCollection<TType>();
                CopyPropertiesTo(result);

                return result;
            }

            throw new NotSupportedException("Cannot convert the collection to other object type.");
        }


        /// <summary>
        /// Returns the updatable collection of fields of collection items
        /// </summary>
        /// <param name="propertyName">Name of the properties to extract</param>
        public CollectionPropertyWrapper<BaseInfo> GetItemsAsFields(string propertyName)
        {
            return new CollectionPropertyWrapper<BaseInfo>(this, propertyName);
        }


        /// <summary>
        /// Gets the collection of objects that are referenced by the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public IInfoObjectCollection GetFieldsAsObjects(string propertyName)
        {
            string objectType = TypeInfo.GetObjectTypeForColumn(propertyName);
            if (string.IsNullOrEmpty(objectType))
            {
                return null;
            }

            return GetFieldsAsObjects(propertyName, objectType);
        }


        /// <summary>
        /// Gets the collection of objects that are referenced by the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="objectType">Object type of the target objects</param>
        protected IInfoObjectCollection GetFieldsAsObjects(string propertyName, string objectType)
        {
            var genObj = Object.Generalized;

            // Create the collection of target objects with nested where condition
            var result = new InfoObjectCollection(objectType);

            // Dependency object type is obtained from ObjectTypeInfo.GetObjectTypeForColumn().
            // This method returns only primary object type and doesn't take into consideration relative types, that are defined by 
            // DependsOn, RegisterAsChildTo, RegisterAsBindingTo properties. Therefore disable type condition and allow all relative types.
            result.UseObjectTypeCondition = false;

            // Prepare the where condition
            var completeWhereCondition = GetCompleteWhereCondition();
            var where = new WhereCondition();
            where.DataSourceName = result.Object.GetDataQuery(false, settings => { }, false).DataSourceName;

            where.WhereIn(
                result.TypeInfo.IDColumn,
                genObj.GetDataQuery(
                    UseObjectTypeCondition,
                    q => q
                        .Column(propertyName)
                        .Where(completeWhereCondition)
                        .Distinct(),
                    false
                )
            );

            result.Where = where;

            return result;
        }


        /// <summary>
        /// Clears the data in the collection and loads objects from the given list.
        /// </summary>
        /// <param name="objects">Objects data to load</param>
        public virtual void Load(IEnumerable<BaseInfo> objects)
        {
            // Clear the collection
            Clear();

            // Add given items to the collection
            if (objects == null)
            {
                return;
            }

            foreach (var info in objects)
            {
                Items.Add(info as TInfo);

                string name = GetObjectName(info);
                if (!string.IsNullOrEmpty(name))
                {
                    ObjectsByName[name.ToLowerInvariant()] = info;
                }
            }
            mCount = Items.Count;
        }


        /// <summary>
        /// Registers supported properties
        /// </summary>
        /// <param name="properties">List with supported properties</param>
        public virtual void RegisterProperties(List<string> properties)
        {
            properties.Add("FieldsAsObjects");
            properties.Add("ItemsAsFields");
            properties.Add("Count");
        }


        /// <summary>
        /// Returns the clone of the collection with specified order by applied
        /// </summary>
        /// <param name="orderBy">Order By expression</param>
        public virtual IInfoObjectCollection GetSubsetOrderBy(string orderBy)
        {
            // Clone and combine the where conditions
            var result = Clone();
            result.OrderByColumns = orderBy;

            return result;
        }


        /// <summary>
        /// Provides a string representation of the collection
        /// </summary>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return $"{GetType().FullName} ({ObjectType})";
        }

        #endregion


        #region "Public Macro Methods"

        /// <summary>
        /// Gets an object for which to perform the permissions check.
        /// </summary>
        public object GetObjectToCheck()
        {
            return this;
        }

        #endregion


        #region "Cache dependencies"

        /// <summary>
        /// Adds the given list of cache dependencies to the collection
        /// </summary>
        /// <param name="keys">Keys to add</param>
        public virtual void AddCacheDependencies(params string[] keys)
        {
            if (mCustomCacheDependencies == null)
            {
                mCustomCacheDependencies = new List<string>();
            }

            mCustomCacheDependencies.AddRange(keys);

            CacheDependenciesChanged();
        }


        /// <summary>
        /// Ensures the actions when the cache dependencies have changed
        /// </summary>
        protected void CacheDependenciesChanged()
        {
            if (mCacheCallbackRegistered)
            {
                RemoveClearCacheCallback();
                RegisterClearCacheCallback();
            }
        }


        /// <summary>
        /// Removes the callback to clear the collection cache
        /// </summary>
        protected void RemoveClearCacheCallback()
        {
            if (mCacheCallbackRegistered)
            {
                lock (this)
                {
                    if (mCacheCallbackRegistered)
                    {
                        // Get cache dependencies and key
                        var key = GetCacheCallbackKey();

                        // Register the callback
                        CacheHelper.RemoveDependencyCallback(key);

                        mCacheCallbackRegistered = false;
                    }
                }
            }
        }


        /// <summary>
        /// Registers the callback to clear the collection cache
        /// </summary>
        protected void RegisterClearCacheCallback()
        {
            if (!mCacheCallbackRegistered && IsCachedObject)
            {
                lock (this)
                {
                    if (!mCacheCallbackRegistered)
                    {
                        // Get cache dependencies and key
                        var key = GetCacheCallbackKey();
                        var keys = GetCacheDependencyKeys();
                        var cd = CacheHelper.GetCacheDependency(keys);

                        // Register the callback
                        CacheHelper.RegisterDependencyCallback(key, cd, this, ClearCacheCallback, null, false);

                        mCacheCallbackRegistered = true;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the key for the cache callback
        /// </summary>
        private string GetCacheCallbackKey()
        {
            var key = CacheHelper.GetCacheItemName(null, "cachecallback|advancedinfoobjectcollection|", ObjectType, "|", mInstanceGuid);

            return key;
        }


        /// <summary>
        /// Gets the list of cache dependency keys on which this collection depends
        /// </summary>
        protected virtual List<string> GetCacheDependencyKeys()
        {
            List<string> keys = new List<string>();

            if (UseDefaultCacheDependencies)
            {
                var parent = ParentObject;
                if (parent != null)
                {
                    // Create dependency on parent object
                    string key = $"{parent.TypeInfo.ObjectType}|byid|{parent.Generalized.ObjectID}|children|{ObjectType}";

                    keys.Add(key);
                }
                else
                {
                    // Create dependency on any object
                    keys.Add(ObjectType + "|all");
                }
            }

            // Add custom dependencies
            if (mCustomCacheDependencies != null)
            {
                keys.AddRange(mCustomCacheDependencies);
            }

            return keys;
        }


        /// <summary>
        /// Clears the collection cache
        /// </summary>
        public void ClearCache()
        {
            Clear();
            mCacheCallbackRegistered = false;
        }


        /// <summary>
        /// Clears the collection cache
        /// </summary>
        /// <param name="col">Target collection</param>
        /// <param name="parameter">Callback parameter</param>
        protected static void ClearCacheCallback(IInfoObjectCollection col, object parameter)
        {
            col.ClearCache();
        }


        /// <summary>
        /// Gets the item on the specified index.
        /// </summary>
        /// <param name="index">Item index to get</param>
        protected TInfo GetItem(int index)
        {
            // Ensure the clear cache callback
            RegisterClearCacheCallback();

            var result = GetItemInternal(index);

            // Mark result as cached object
            if (result != null)
            {
                result.Generalized.IsCachedObject = true;
            }

            return result;
        }


        /// <summary>
        /// Gets the item on the specified index.
        /// </summary>
        /// <param name="index">Item index to get</param>
        private TInfo GetItemInternal(int index)
        {
            // Handle the negative item index (take items from the end)
            if (index < 0)
            {
                int count = InternalCount;
                index = count - index;

                if (index < 0)
                {
                    // Too high, do not return anything
                    return default(TInfo);
                }
            }

            if ((mCount >= 0) && (index >= mCount))
            {
                // If index is higher than the total known count, do not return anything
                return default(TInfo);
            }

            // Try to get if already loaded
            TInfo result = Items.ElementAtOrDefault(index);

            // If the object was removed, return null
            if (IsRemoved(result))
            {
                return default(TInfo);
            }

            if (result == null)
            {
                if (SourceData != null)
                {
                    if (mCount < 0)
                    {
                        // Get the total number of items
                        mCount = DataHelper.GetItemsCount(SourceData);
                    }

                    if (index >= mCount)
                    {
                        // If index is higher than the total known count, do not return anything
                        return default(TInfo);
                    }

                    // Get the data from source DataSet
                    DataRow dr = null;

                    int tableIndex = 0;
                    int itemIndex = index;

                    // Find the particular row in the source DataSet
                    while (tableIndex < SourceData.Tables.Count)
                    {
                        DataTable dt = SourceData.Tables[tableIndex];
                        if (dt.Rows.Count > itemIndex)
                        {
                            // Row found
                            dr = dt.Rows[itemIndex];
                            break;
                        }
                        else
                        {
                            // Move to the next table
                            tableIndex++;
                            itemIndex -= dt.Rows.Count;
                        }
                    }

                    // Create the item
                    result = CreateNewObject(dr);

                    // Save new item
                    EnsureItems(index + 1);
                    Items[index] = result;
                }
                else
                {
                    // Get the data from database
                    DataSet ds;
                    int pageIndex = 0;
                    int pageSize = 0;

                    if (mAllowPaging)
                    {
                        pageIndex = index / PageSize;
                        pageSize = PageSize;
                    }
                    else
                    {
                        mCount = 0;
                    }

                    // Enforce not committed transaction if there is potentially harmful SQL code
                    IDisposable tr = null;
                    if (EnforceReadOnlyDataAccess)
                    {
                        tr = new CMSTransactionScope();
                    }

                    using (tr)
                    {
                        // Load the data
                        ds = GetData(null, pageIndex * pageSize, pageSize, ref mCount);
                    }

                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        int i = pageIndex * PageSize;

                        // Process all data
                        foreach (DataTable dt in ds.Tables)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                // Load the item
                                TInfo newItem = CreateNewObject(dr);

                                // Fill in missing items
                                EnsureItems(i + 1);
                                // Add new item only if it doesn't exist.
                                Items[i] = Items[i] ?? newItem;

                                // Register the object within collection
                                string name = GetObjectName(newItem);
                                if (!String.IsNullOrEmpty(name))
                                {
                                    this[name] = newItem;
                                }

                                i++;
                            }
                        }

                        // Set the total items if paging not available
                        if (!mAllowPaging)
                        {
                            mCount = i;
                        }
                    }

                    result = Items.ElementAtOrDefault(index);
                }
            }

            return result;
        }


        /// <summary>
        /// Ensures the specified number of items in the item list.
        /// </summary>
        /// <param name="count">Number of items</param>
        protected void EnsureItems(int count)
        {
            while (Items.Count < count)
            {
                Items.Add(default(TInfo));
            }
        }


        /// <summary>
        /// Returns true if the given value  represents a removed object
        /// </summary>
        /// <param name="obj">Object to check</param>
        private bool IsRemoved(TInfo obj)
        {
            return obj == RemovedObject;
        }


        /// <summary>
        /// Returns the clone of the collection with specified column not being empty
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual IInfoObjectCollection<TInfo> GetSubsetWhereNotEmpty(string columnName)
        {
            string where = columnName + " <> ''";

            return GetSubsetWhere(where);
        }


        /// <summary>
        /// Returns the clone of the collection with specified where condition applied
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        public virtual IInfoObjectCollection<TInfo> GetSubsetWhere(string whereCondition)
        {
            // Clone and combine the where conditions
            var result = Clone();

            result.Where.Where(whereCondition);

            return result;
        }

        #endregion


        #region "Indexation"

        /// <summary>
        /// Gets or sets the object on specific index.
        /// </summary>
        /// <param name="index">Object index to get</param>
        public TInfo this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                // Check if item exists
                if (index >= mCount)
                {
                    // If item doesn't exist check if it is present in source data
                    if (SourceData != null)
                    {
                        // Get new count from source data in case that source data were modified.
                        var sourceDataCount = DataHelper.GetItemsCount(SourceData);

                        // Item is not in source data
                        if ((index >= sourceDataCount))
                        {
                            throw new IndexOutOfRangeException();
                        }

                        // Update count in case that source data was modified (InfoDataSet.AddItems).
                        mCount = sourceDataCount;
                    }
                    // If mCount isn't initialized, check if item exists in database 
                    else if (mCount < 0)
                    {
                        // Execute query
                        GetData(null, 0, 1, ref mCount);

                        if (index >= mCount)
                        {
                            throw new IndexOutOfRangeException();
                        }
                    }
                }

                // Ensure the number of items
                EnsureItems(index + 1);

                // Update item
                Items[index] = value;
            }
        }


        /// <summary>
        /// Returns the object registered by the specific name.
        /// </summary>
        /// <param name="name">Object name (indexer)</param>
        public new TInfo this[string name]
        {
            get
            {
                return GetObjectByName(name);
            }
            set
            {
                // No name, no assignment
                if (String.IsNullOrEmpty(name))
                {
                    return;
                }

                // Register
                ObjectsByName[name.ToLowerInvariant()] = value;
            }
        }


        /// <summary>
        /// Object indexer
        /// </summary>
        /// <param name="index">Index</param>
        object IIndexable.this[int index]
        {
            get
            {
                return this[index];
            }
        }


        /// <summary>
        /// Object indexer
        /// </summary>
        /// <param name="name">Name</param>
        object INameIndexable.this[string name]
        {
            get
            {
                return this[name];
            }
        }

        #endregion


        #region IEnumerable<InfoType> Members


        /// <summary>
        /// Creates the child collection based on the given provider
        /// </summary>
        /// <param name="settings">Query parameters</param>
        public ICMSQueryable<TInfo> CreateChild(IDataQuerySettings settings)
        {
            var col = Clone();

            // Merge where conditions
            col.Where = new WhereCondition(col.Where, settings);

            var columns = settings.SelectColumnsList.GetColumns(null, true);

            if (!String.IsNullOrEmpty(columns))
            {
                col.Columns = SqlHelper.MergeColumns(col.Columns, columns, null, false);
            }

            if (settings.TopNRecords > 0)
            {
                col.TopN = settings.TopNRecords;
            }

            var orderBy = settings.Expand(settings.OrderByColumns);
            if (!String.IsNullOrEmpty(orderBy))
            {
                col.OrderByColumns = orderBy;
            }

            return col;
        }

        /// <summary>
        /// Gets the general enumerator for the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Gets the  enumerator for the collection
        /// </summary>
        public IEnumerator<TInfo> GetEnumerator()
        {
            // Encapsulate the enumerator and cast the results
            var baseEnum = GetEnumeratorInternal();

            while (baseEnum.MoveNext())
            {
                yield return baseEnum.Current;
            }
        }


        /// <summary>
        /// Gets the enumerator for the collection.
        /// </summary>
        public IEnumerator<TInfo> GetEnumeratorInternal()
        {
            try
            {
                lock (lockObject)
                {
                    mEnumerators++;
                }

                // Enumerate through all the items present in the collection
                var internalCount = InternalCount;
                for (int i = 0; i < internalCount; i++)
                {
                    TInfo obj = GetItem(i);
                    if (obj != null)
                    {
                        yield return obj;
                    }
                }

                // Enumerate through all the new items
                if (mNewItems != null)
                {
                    var newCount = mNewItems.Count;
                    for (int i = 0; i < newCount; i++)
                    {
                        TInfo obj = mNewItems[i];
                        if ((obj != null) && !IsRemoved(obj))
                        {
                            yield return obj;
                        }
                    }
                }
            }
            finally
            {
                lock (lockObject)
                {
                    // Decrement enumerators and clear if requested
                    if ((--mEnumerators) <= 0)
                    {
                        if (mClearAfterEnumeration)
                        {
                            Clear();
                            mClearAfterEnumeration = false;
                        }
                        mEnumerators = 0;
                    }
                }
            }
        }

        #endregion


        #region "IComparable Members"

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        public int CompareTo(object obj)
        {
            var col = obj as IInfoObjectCollection;
            if (col != null)
            {
                return string.Compare(Name, col.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            throw new Exception("Cannot compare collection with other type.");
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit operator for conversion from ObjectCollection class to DataSet
        /// </summary>
        /// <param name="col">Collection</param>
        public static implicit operator DataSet(InfoObjectCollection<TInfo> col)
        {
            return col.GetFullData();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

using CMS.Core;
using CMS.Base;

namespace CMS.DataEngine
{
    using NamesDictionary = StringSafeDictionary<string>;
    using SettingsByIndexDictionary = SafeDictionary<int, InfoCollectionSettings>;

    /// <summary>
    /// Repository for info objects.
    /// </summary>
    public class InfoObjectRepository : InfoObjectRepository<IInfoObjectCollection<BaseInfo>, BaseInfo, InfoCollectionSettings>, ICMSStorage
    {
        #region "Properties"

        /// <summary>
        /// Parent object.
        /// </summary>
        public BaseInfo ParentObject
        {
            get;
            protected set;
        }


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        public override bool IsCachedObject
        {
            get
            {
                if ((ParentObject != null) && ParentObject.Generalized.IsCachedObject)
                {
                    return true;
                }

                return base.IsCachedObject;
            }
            set
            {
                base.IsCachedObject = value;
            }
        }


        /// <summary>
        /// Returns true if the repository is disconnected from the data source
        /// </summary>
        public override bool IsDisconnected
        {
            get
            {
                if ((ParentObject != null) && ParentObject.Generalized.IsDisconnected)
                {
                    return true;
                }

                return base.IsDisconnected;
            }
        }


        /// <summary>
        /// New collection delegate
        /// </summary>
        /// <param name="type">Object type of the collection</param>
        /// <param name="parentObject">Parent object</param>
        /// <param name="repository">Parent repository</param>
        public delegate IInfoObjectCollection<BaseInfo> OnNewCollectionHandler(string type, BaseInfo parentObject, InfoObjectRepository repository);


        /// <summary>
        /// Event fired when new collection instance is requested
        /// </summary>
        public event OnNewCollectionHandler OnNewCollection;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public InfoObjectRepository()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentStorage">Parent storage object</param>
        public InfoObjectRepository(ICMSStorage parentStorage)
        {
            ParentStorage = parentStorage;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentObject">Parent object</param>
        public InfoObjectRepository(BaseInfo parentObject)
        {
            ParentObject = parentObject;
        }


        /// <summary>
        /// Loads the given collection.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        protected override IInfoObjectCollection<BaseInfo> LoadCollection(InfoCollectionSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            IInfoObjectCollection<BaseInfo> result;
            InfoCollectionSettings s = settings;

            // Create a new collection
            if (s.CollectionFactory != null)
            {
                // Create with factory
                var newCollection = s.CollectionFactory.CreateNewObject();

                result = (IInfoObjectCollection<BaseInfo>)newCollection;
                result.ChangeParent(ParentObject, this);
            }
            else
            {
                // Let the repository create it automatically
                result = NewCollection(s.ObjectType);
            }

            SetupCollection(result, s);
            StoreCollection(result, s);

            return result;
        }


        /// <summary>
        /// Creates new collection for the data.
        /// </summary>
        /// <param name="type">Object type of the collection</param>
        public override IInfoObjectCollection<BaseInfo> NewCollection(string type)
        {
            IInfoObjectCollection<BaseInfo> result = null;

            // Try to get collection with handler
            if (OnNewCollection != null)
            {
                result = OnNewCollection(type, ParentObject, this);
            }

            if (result != null)
            {
                return result;
            }

            // Create a new empty collection
            result = NewCollectionInternal(type);
            result.ChangeParent(ParentObject, this);

            return result;
        }


        /// <summary>
        /// Creates a new collection
        /// </summary>
        /// <param name="type">Collection type</param>
        protected virtual InfoObjectCollection NewCollectionInternal(string type)
        {
            return new InfoObjectCollection(type);
        }


        /// <summary>
        /// Creates new combined collection for the data.
        /// </summary>
        public override CombinedInfoObjectCollection<IInfoObjectCollection<BaseInfo>, BaseInfo> NewCombinedCollection()
        {
            return new CombinedInfoObjectCollection();
        }
        

        /// <summary>
        /// Registers the given collection of objects within the repository.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        public InfoCollectionSettings AddCollection<CollectionType>(InfoCollectionSettings settings)
            where CollectionType : IInfoObjectCollection
        {
            AddCollection(settings);
            settings.CollectionFactory = new ObjectFactory<CollectionType>();

            return settings;
        }



        /// <summary>
        /// Registers the given collection of objects within the repository. Includes all the objects of given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public void AddCollection(string objectType)
        {
            AddCollection(new InfoCollectionSettings(null, objectType));
        }

        #endregion
    }


    /// <summary>
    /// Repository for info objects.
    /// </summary>
    public abstract class InfoObjectRepository<TCollection, TObject, TSettings> : IInfoObjectRepository<TCollection>, INamedEnumerable<TCollection>, IHierarchicalObject
        where TCollection : class, IInfoObjectCollection, IEnumerable<TObject>
        where TSettings : BaseCollectionSettings
        where TObject : BaseInfo
    {
        #region "Events"

        /// <summary>
        /// Delegate to define the handle to load the collection of this type.
        /// </summary>
        /// <param name="repository">Repository into which the collection loads</param>
        /// <param name="name">Collection name</param>
        public delegate TCollection LoadCollectionHandler(IInfoObjectRepository<TCollection> repository, string name);


        /// <summary>
        /// Fires when the collection with specified name is requested.
        /// </summary>
        public event LoadCollectionHandler OnLoadCollection = null;

        #endregion


        #region "Variables"

        /// <summary>
        /// Table of collections [name -> CollectionType]
        /// </summary>
        private StringSafeDictionary<TCollection> mCollections = new StringSafeDictionary<TCollection>();


        /// <summary>
        /// Translation of collection nice names to a normal names [niceName -> name]
        /// </summary>
        private NamesDictionary mNiceNames = new NamesDictionary();


        /// <summary>
        /// Collection of all underlying objects.
        /// </summary>
        private CombinedInfoObjectCollection<TCollection, TObject> mAll;


        /// <summary>
        /// List of all collections (sorted by the order in which the collections were added).
        /// </summary>
        private List<TCollection> mCollectionsList = new List<TCollection>();


        /// <summary>
        /// Table of collection settings [name -> InfoCollectionSettings]
        /// </summary>
        /// <seealso cref="CollectionSettingsLock"/>
        private StringSafeDictionary<TSettings> mCollectionSettings = new StringSafeDictionary<TSettings>();


        /// <summary>
        /// Table of collection settings by index [index -> InfoCollectionSettings]
        /// </summary>
        /// <seealso cref="CollectionSettingsLock"/>
        private SafeDictionary<int, TSettings> mCollectionSettingsByIndex = new SafeDictionary<int, TSettings>();


        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        private List<string> mProperties;


        /// <summary>
        /// Indicates whether to load binary data into the collections of the repository.
        /// </summary>
        private bool mLoadBinaryData;


        /// <summary>
        /// Number of disconnected references for this collection 
        /// </summary>
        private int mDisconnectedCount;


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        private bool mIsCachedObject;


        /// <summary>
        /// List of dynamic names of the collections
        /// </summary>
        private IEnumerable<string> mDynamicNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets lock object for synchronization of modification of <see cref="CollectionSettings"/> and <see cref="CollectionSettingsByIndex"/>.
        /// </summary>
        protected object CollectionSettingsLock
        {
            get;
        } = new object();


        /// <summary>
        /// List of all collections (sorted by the order in which the collections were added).
        /// </summary>
        protected List<TCollection> CollectionsList
        {
            get
            {
                return mCollectionsList;
            }
        }
        

        /// <summary>
        /// Table of collections [name -> CollectionType]
        /// </summary>
        protected StringSafeDictionary<TCollection> Collections
        {
            get
            {
                return mCollections;
            }
        }


        /// <summary>
        /// Table of collection settings [name -> InfoCollectionSettings]
        /// </summary>
        /// <seealso cref="CollectionSettingsLock"/>
        protected StringSafeDictionary<TSettings> CollectionSettings
        {
            get
            {
                return mCollectionSettings;
            }
        }


        /// <summary>
        /// Table of collection settings by index [index -> InfoCollectionSettings]
        /// </summary>
        /// <seealso cref="CollectionSettingsLock"/>
        protected SafeDictionary<int, TSettings> CollectionSettingsByIndex
        {
            get
            {
                return mCollectionSettingsByIndex;
            }
        }


        /// <summary>
        /// Function that provides the dynamic names of collections to the repository
        /// </summary>
        public Func<IEnumerable<string>> GetDynamicNames
        {
            get;
            set;
        }


        /// <summary>
        /// List of the dynamic names of the collections
        /// </summary>
        public IEnumerable<string> DynamicNames
        {
            get
            {
                return mDynamicNames ?? (mDynamicNames = (GetDynamicNames != null) ? GetDynamicNames() : new List<string>());
            }
            set
            {
                mDynamicNames = value;
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
        /// If true, the repository allows nice names of the collections, e.g. for "CMS.User" uses "Users"
        /// </summary>
        public bool AllowNiceNames
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        public virtual bool IsCachedObject
        {
            get
            {
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
        /// Returns true if this collection is disconnected from the database
        /// </summary>
        public virtual bool IsDisconnected
        {
            get
            {
                if ((ParentStorage != null) && ParentStorage.IsDisconnected)
                {
                    return true;
                }

                return ((mDisconnectedCount > 0) || CMSActionContext.CurrentDisconnected);
            }
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to load binary data into the collections.
        /// </summary>
        public virtual bool LoadBinaryData
        {
            get
            {
                return mLoadBinaryData;
            }
            set
            {
                mLoadBinaryData = value;
                foreach (TCollection col in this)
                {
                    col.LoadBinaryData = value;
                }
            }
        }


        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        public virtual List<string> Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = new List<string>();

                    // All item
                    mProperties.Add("All");

                    // Count of items
                    mProperties.Add("Count");

                    // Sort the properties alphabetically
                    mProperties.Sort();
                }
                return mProperties;
            }
        }


        /// <summary>
        /// Returns the number of items.
        /// </summary>
        public int Count
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns list of collection names in the repository.
        /// </summary>
        public List<string> CollectionNames
        {
            get
            {
                var names = new List<string>();

                foreach (var collection in mCollectionSettings.TypedValues)
                {
                    names.Add(collection.NiceName ?? collection.Name);
                }

                return names;
            }
        }


        /// <summary>
        /// Returns the collection of objects indexed by object type, e.g. "cms.user".
        /// </summary>
        /// <param name="name">Name of the inner collection</param>
        public TCollection this[string name]
        {
            get
            {
                return GetCollection(name);
            }
        }


        /// <summary>
        /// Returns the collection of objects.
        /// </summary>
        /// <param name="index">Index of the collection</param>
        public TCollection this[int index]
        {
            get
            {
                return GetCollection(index);
            }
        }


        /// <summary>
        /// All items from all underlying collections.
        /// </summary>
        public virtual CombinedInfoObjectCollection<TCollection, TObject> All
        {
            get
            {
                if (mAll == null)
                {
                    lock (this)
                    {
                        // Combine all collections into one
                        mAll = NewCombinedCollection();

                        foreach (TCollection collection in this)
                        {
                            mAll.Add(collection);
                        }
                    }
                }

                return mAll;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public InfoObjectRepository()
        {
            AllowNiceNames = true;
        }


        /// <summary>
        /// Loads the given collection.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        protected abstract TCollection LoadCollection(TSettings settings);


        /// <summary>
        /// Creates new collection for the data.
        /// </summary>
        /// <param name="type">Type of the collection</param>
        public abstract TCollection NewCollection(string type);


        /// <summary>
        /// Creates new combined collection for the data.
        /// </summary>
        public abstract CombinedInfoObjectCollection<TCollection, TObject> NewCombinedCollection();


        /// <summary>
        /// Disconnects the collections from the database
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
        }


        /// <summary>
        /// Gets the property name for the original name of the collection
        /// </summary>
        /// <param name="name">Original name</param>
        public string GetNicePropertyName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (!AllowNiceNames)
                {
                    return TranslationHelper.GetSafeClassName(name);
                }

                // Get the nice name from the given object type
                name = TypeHelper.GetNiceName(name);

                name = TypeHelper.GetPlural(name);
            }

            return name;
        }


        /// <summary>
        /// Registers the nice name of the collection
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="niceName">Nice name</param>
        protected void RegisterNiceName(string name, string niceName)
        {
            if (niceName != null)
            {
                mNiceNames[niceName] = name;
            }
        }


        /// <summary>
        /// Registers the given collection of objects within the repository.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        public TSettings ReplaceCollection(TSettings settings)
        {
            var name = settings.Name;
            var niceName = GetNicePropertyName(name);

            name = TranslationHelper.GetSafeClassName(settings.ObjectTypeInternal);

            lock (CollectionSettingsLock)
            {
                // Check if already registered
                var s = CollectionSettings[name];
                if (s == null)
                {
                    throw new ArgumentException("The collection with name '" + settings.Name + "' does not exist.");
                }

                settings.Name = name;
                settings.NiceName = niceName;
                settings.Index = s.Index;

                // Remove the collection from existing ones
                Collections[settings.Name] = null;
                CollectionSettingsByIndex[settings.Index] = settings;

                return s;
            }           
        }

        
        /// <summary>
        /// Registers the given collection of objects within the repository.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        public virtual TSettings AddCollection(TSettings settings)
        {
            var name = settings.Name;
            string niceName = GetNicePropertyName(name);

            // Sanitize the name
            string originalName = name;
            name = TranslationHelper.GetSafeClassName(settings.ObjectTypeInternal);

            lock (CollectionSettingsLock)
            {
                // Check if object type name is already registered
                if (CollectionSettings.Contains(name))
                {
                    name = TranslationHelper.GetSafeClassName(originalName).ToLowerCSafe();

                    if (CollectionSettings.Contains(name))
                    {
                        throw new ArgumentException("The collection with name '" + name + "' is already registered.");
                    }
                }

                settings.Name = name;
                settings.NiceName = niceName;
                settings.Index = Count;

                CollectionSettings[name] = settings;
                CollectionSettingsByIndex[Count] = settings;

                // Register under the nice name
                RegisterNiceName(name, niceName);

                Count++;
            }

            return settings;
        }


        /// <summary>
        /// Submits the changes to the database.
        /// </summary>
        public void SubmitChanges()
        {
            if (All != null)
            {
                // Submit through all collection
                All.SubmitChanges();
            }
            else
            {
                // Submit all collections individually
                foreach (var collection in CollectionsList)
                {
                    if (collection != null)
                    {
                        collection.SubmitChanges();
                    }
                }
            }
        }


        /// <summary>
        /// Adds new object to the collection.
        /// </summary>
        /// <param name="objects">Objects to add</param>
        public virtual void Add(params TObject[] objects)
        {
            if (objects == null)
            {
                return;
            }

            // Add all objects
            foreach (TObject infoObj in objects)
            {
                if (infoObj != null)
                {
                    // Get the target collection
                    var ti = infoObj.TypeInfo;

                    var collection = this[ti.ObjectType];
                    if (collection == null)
                    {
                        throw new InvalidOperationException("Object of type '" + ti.ObjectType + "' cannot be added to this repository.");
                    }

                    // Add to the collection
                    collection.Add(infoObj);
                }
            }
        }

        #endregion


        #region "IEnumerable<InfoObjectCollection> Members"

        /// <summary>
        /// Gets the enumerator for the collection.
        /// </summary>
        public virtual IEnumerator<TCollection> GetEnumerator()
        {
            // Enumerate through all the registered items
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }

            // Enumerate through the dynamic names
            foreach (string name in DynamicNames)
            {
                yield return this[name];
            }
        }

        #endregion


        #region "IEnumerable Members"

        /// <summary>
        /// Gets the general enumerator for the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region "IHierarchicalObject Members"

        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetProperty(string columnName, out object value)
        {
            if (columnName.EqualsCSafe("All", true))
            {
                // All items
                value = All;
            }
            else if (columnName.EqualsCSafe("Count", true))
            {
                // Count
                value = Count;
            }
            else
            {
                // Get the collection
                value = this[columnName];
            }

            return (value != null);
        }


        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetProperty(string columnName)
        {
            // Get the value
            object value;
            TryGetProperty(columnName, out value);

            return value;
        }


        /// <summary>
        /// Returns the collection of objects indexed by object type, e.g. "cms.user".
        /// </summary>
        /// <param name="name">Name of the inner collection</param>
        protected virtual TCollection GetCollection(string name)
        {
            // Try to get the name by a nice name first
            name = mNiceNames[name] ?? name;

            // Try to get if already loaded
            string key = TranslationHelper.GetSafeClassName(name);
            TCollection result = mCollections[key];
            if (result != null)
            {
                return result;
            }

            try
            {
                lock (mCollectionsList)
                {
                    result = mCollections[key];

                    if (result == null)
                    {
                        // Try to load the collection
                        var settings = mCollectionSettings[key];
                        if (settings != null)
                        {
                            result = LoadCollection(settings);
                        }

                        // Try to get via external handler
                        if ((result == null) && (OnLoadCollection != null))
                        {
                            result = OnLoadCollection(this, name);
                            mCollections[key] = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("ObjectRepository", "LOADCOLLECTION", ex);

                return default(TCollection);
            }

            return result;
        }


        /// <summary>
        /// Returns the collection of objects.
        /// </summary>
        /// <param name="index">Index of the collection</param>
        private TCollection GetCollection(int index)
        {
            // Try to get if already loaded
            TCollection result = GetCollectionResult(index);

            if (result == null)
            {
                lock (mCollectionsList)
                {
                    result = GetCollectionResult(index);

                    if (result == null)
                    {
                        // Try to load the collection
                        var settings = mCollectionSettingsByIndex[index];
                        result = LoadCollection(settings);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the collection of objects from Collection list. Does not load collections
        /// </summary>
        /// <param name="index">Index of the collection</param>
        private TCollection GetCollectionResult(int index)
        {
            TCollection result = default(TCollection);
            if (index < mCollectionsList.Count)
            {
                result = mCollectionsList[index];
            }

            return result;
        }


        /// <summary>
        /// Stores the collection into inner lists
        /// </summary>
        /// <param name="collection">Collection to store</param>
        /// <param name="settings">Collection settings</param>
        protected void StoreCollection(TCollection collection, InfoCollectionSettings settings)
        {
            // Save to the indexed tables
            Collections[settings.Name] = collection;

            // Ensure the index and assign
            while (CollectionsList.Count <= settings.Index)
            {
                CollectionsList.Add(null);
            }
            CollectionsList[settings.Index] = collection;
        }


        /// <summary>
        /// Sets up the collection using the given settings
        /// </summary>
        /// <param name="collection">Collection to set</param>
        /// <param name="setttings">Collection settings</param>
        protected void SetupCollection(TCollection collection, TSettings setttings)
        {
            // Prepare the parameters for the data
            collection.UseObjectTypeCondition = true;
            collection.Where = setttings.WhereCondition;
            collection.DynamicWhereCondition = setttings.DynamicWhere;
            collection.OrderByColumns = setttings.OrderBy;
            collection.TopN = setttings.TopN;
            collection.Columns = setttings.Columns;

            if (setttings.SiteID >= 0)
            {
                collection.Object.Generalized.ObjectSiteID = setttings.SiteID;
            }
            collection.SiteID = setttings.SiteID;

            if (!String.IsNullOrEmpty(setttings.NiceName))
            {
                collection.Name = setttings.NiceName;
            }
            if (setttings.NameColumn != null)
            {
                collection.NameColumn = setttings.NameColumn;
            }
        }

        #endregion


        #region "INameIndexable Members"

        /// <summary>
        /// Returns the object registered by the specific name.
        /// </summary>
        /// <param name="name">Object name (indexer)</param>
        object INameIndexable.this[string name]
        {
            get
            {
                return this[name];
            }
        }

        #endregion


        #region "INamedEnumerable Members"

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
                return true;
            }
        }


        /// <summary>
        /// Returns the name of the given object
        /// </summary>
        /// <param name="obj">Object for which to get the name</param>
        public string GetObjectName(object obj)
        {
            return ((TCollection)obj).Name;
        }

        #endregion
    }
}
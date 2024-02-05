using System;
using System.Linq;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using System.Threading;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class with various types of object type lists
    /// </summary>
    public class ObjectTypeManager : AbstractHierarchicalObject<ObjectTypeManager>
    {
        #region "Variables"

        // Singleton instance of the manager.
        private static ObjectTypeManager mInstance;

        // Registered object types [ObjectType] -> [RegisterObjectTypeAttribute].
        private readonly StringSafeDictionary<ObjectTypeInfo> mTypesByObjectType = new StringSafeDictionary<ObjectTypeInfo>();

        // All type infos existing in the system for the given type [Type] -> [List[RegisterObjectTypeAttribute]].
        private readonly IGeneralIndexable<Type, IList<ObjectTypeInfo>> mTypeInfosByType = new SafeDictionary<Type, IList<ObjectTypeInfo>>();

        private List<string> mBindingObjectTypes;

        // List of all listing (inherited) object types.
        private List<string> mListObjectTypes;

        // List of all "main" object types (= not child of other object) retrieved from modules.
        private List<string> mMainObjectTypes;

        // List of all object types without inherited object types.
        private List<string> mAllObjectTypes;

        // List of all object types without inherited and binding object types.
        private List<string> mAllExceptBindingObjectTypes;

        private List<string> mObjectTypesWithMacros;

        private List<string> mObjectTypesWithDynamicDependency;

        private List<string> mContinuousIntegrationSupportedObjectTypes; 

        // List of all existing object types retrieved from modules (including inherited object types).
        private readonly List<string> mExistingObjectTypes = new List<string>();

        private ObjectGenerator<BaseInfo> mObjectGenerator;

        private static readonly object lockObject = new object();
        private static bool mObjectTypesBeingEnsured;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns all registered types
        /// </summary>
        public static IEnumerable<ObjectTypeInfo> RegisteredTypes
        {
            get
            {
                return Instance.mTypesByObjectType.TypedValues;
            }
        }


        /// <summary>
        /// Object generator
        /// </summary>
        private static ObjectGenerator<BaseInfo> ObjectGenerator
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mObjectGenerator, CreateGenerator, lockObject);
            }
        }


        /// <summary>
        /// Returns singleton instance of the ObjectTypeManager.
        /// </summary>
        public static ObjectTypeManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new ObjectTypeManager(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// Returns the list of all existing object types available in the system in a not guaranteed order (includes also inherited, list infos).
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> ExistingObjectTypes
        {
            get
            {
                EnsureObjectTypes();

                return Instance.mExistingObjectTypes;
            }
        }


        /// <summary>
        /// Returns the list of all the object types available in the system in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> AllObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mAllObjectTypes, GetAllObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// Returns the list of all the object types available in the system in a not guaranteed order. Does not contain inherited object types and binding object types.
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> AllExceptBindingObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mAllExceptBindingObjectTypes, GetAllExceptBindingObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// List of all "main" object types (= not child of other object) retrieved from modules.
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> MainObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mMainObjectTypes, GetMainObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// List of all binding object types.
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> BindingObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mBindingObjectTypes, GetBindingObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// Returns list of all listing (inherited) object types.
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> ListObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mListObjectTypes, GetListObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// Returns the list of all the object types available in the system that can contain macros in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> ObjectTypesWithMacros
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mObjectTypesWithMacros, GetObjectTypesWithMacros, lockObject);
            }
        }


        /// <summary>
        /// Returns the list of all the object types available in the system that has dynamic dependency in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        [RegisterProperty]
        public static IEnumerable<string> ObjectTypesWithDynamicDependency
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mObjectTypesWithDynamicDependency, GetObjectTypesWithDynamicDependency, lockObject);
            }
        }


        /// <summary>
        /// Returns the list of all object types that are supported by continuous integration.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public static IEnumerable<string> ContinuousIntegrationSupportedObjectTypes
        {
            get
            {
                return LockHelper.Ensure(ref Instance.mContinuousIntegrationSupportedObjectTypes, GetContinuousIntegrationSupportedObjectTypes, lockObject);
            }
        }


        /// <summary>
        /// If true, all object types were already registered.
        /// </summary>
        internal static bool ObjectTypesRegistered
        {
            get;
            private set;
        }

        #endregion


        #region "Object type list methods"

        /// <summary>
        /// Returns the list of all the object types available in the system in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        private static List<string> GetAllObjectTypes()
        {
            return GetObjectTypes(ExistingObjectTypes, x => !x.IsListingObjectTypeInfo);
        }


        /// <summary>
        /// Returns the list of all the object types available in the system in a not guaranteed order. Does not contain inherited object types and binding object types.
        /// </summary>
        private static List<string> GetAllExceptBindingObjectTypes()
        {
            return GetObjectTypes(AllObjectTypes, x => !x.IsBinding);
        }


        /// <summary>
        /// Returns the list of all the object types available in the system that can contain macros in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        private static List<string> GetObjectTypesWithMacros()
        {
            return GetObjectTypes(ExistingObjectTypes, x => !x.IsListingObjectTypeInfo && x.ContainsMacros);
        }


        /// <summary>
        /// Returns the list of all the object types available in the system that has dynamic dependency in a not guaranteed order. Does not contain inherited object types
        /// </summary>
        private static List<string> GetObjectTypesWithDynamicDependency()
        {
            return GetObjectTypes(AllObjectTypes, x => x.ObjectDependencies.Any(dep => dep.HasDynamicObjectType()));
        }


        /// <summary>
        /// Returns the list of all object types that are supported by continuous integration.
        /// </summary>
        private static List<string> GetContinuousIntegrationSupportedObjectTypes() 
        {
            return GetObjectTypes(AllObjectTypes, x => x.ContinuousIntegrationSettings.Enabled);
        }


        /// <summary>
        /// Returns list of all "main" object types (= not child of other object) retrieved from modules.
        /// </summary>
        private static List<string> GetMainObjectTypes()
        {
            return GetObjectTypes(AllObjectTypes, x => x.IsMainObject);
        }


        /// <summary>
        /// Returns list of all binding object types.
        /// </summary>
        private static List<string> GetBindingObjectTypes()
        {
            return GetObjectTypes(AllObjectTypes, x => x.IsBinding);
        }


        /// <summary>
        /// Returns list of all listing (inherited) object types.
        /// </summary>
        private static List<string> GetListObjectTypes()
        {
            return GetObjectTypes(ExistingObjectTypes, x => x.IsListingObjectTypeInfo);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates the object generator
        /// </summary>
        private static ObjectGenerator<BaseInfo> CreateGenerator()
        {
            var gen = new ObjectGenerator<BaseInfo>(null);

            gen.RegisterObjectType<GeneralInfo>(GeneralInfo.OBJECT_TYPE);

            return gen;
        }


        /// <summary>
        /// Gets list of columns from given object types
        /// </summary>
        /// <param name="objectTypes">Object types</param>
        public static List<string> GetColumnNames(params string[] objectTypes)
        {
            var columns = new List<string>();
            foreach (var objectType in objectTypes)
            {
                var obj = ModuleManager.GetReadOnlyObject(objectType);
                if (obj != null)
                {
                    columns.AddRange(obj.ColumnNames);
                }
            }

            return columns;
        }


        /// <summary>
        /// Filters given source collection of object types according to the condition specified.
        /// </summary>
        /// <param name="sourceCollection">Source collection to filter</param>
        /// <param name="condition">Condition to filter with</param>
        public static IEnumerable<ObjectTypeInfo> GetTypeInfos(IEnumerable<string> sourceCollection, Func<ObjectTypeInfo, bool> condition)
        {
            foreach (string objType in sourceCollection)
            {
                var typeInfo = GetTypeInfo(objType);

                if ((condition == null) || condition(typeInfo))
                {
                    yield return typeInfo;
                }
            }
        }


        /// <summary>
        /// Filters given source collection of object types according to the condition specified.
        /// </summary>
        /// <param name="sourceCollection">Source collection to filter</param>
        /// <param name="condition">Condition to filter with</param>
        public static List<string> GetObjectTypes(IEnumerable<string> sourceCollection, Func<ObjectTypeInfo, bool> condition)
        {
            return GetTypeInfos(sourceCollection, condition).Select(t => t.ObjectType).ToList();
        }


        /// <summary>
        /// Ensures that the object types are properly registered
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when method is used prior to application pre-initialization.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a loop is detected.</exception>
        internal static void EnsureObjectTypes()
        {
            if (!ObjectTypesRegistered)
            {
                lock (lockObject)
                {
                    if (!ObjectTypesRegistered)
                    {
                        try
                        {
                            if (!TypeManager.PreInitialized)
                            {
                                throw new InvalidOperationException("EnsureObjectTypes cannot be called prior to application pre-initialization.");
                            }

                            if (mObjectTypesBeingEnsured)
                            {
                                throw new InvalidOperationException("A loop occurred within object type initialization.");
                            }

                            mObjectTypesBeingEnsured = true;

                            EnsureObjectTypeDynamicListsInternal(Instance.mExistingObjectTypes);

                            ObjectTypesRegistered = true;
                        }
                        finally
                        {
                            mObjectTypesBeingEnsured = false;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Registers the specified type info to all lists where it belongs to (ObjectDependencies, ChildObjectTypes, BindingObjectTypes, OtherBindingObjectTypes).
        /// </summary>
        /// <param name="typeInfo">Object type info to process</param>
        public static void EnsureObjectTypeInfoDynamicList(ObjectTypeInfo typeInfo)
        {
            // Ensure object dependencies
            EnsureObjectTypeInfoDependencies(typeInfo);

            // Ensure ChildObjectTypes, BindingObjectTypes and OtherBindingObjectTypes lists
            // This HAS to be called after the ObjectDependencies are computed!
            EnsureObjectTypeInfoList(typeInfo);
        }


        /// <summary>
        /// Registers the specified types to all lists where it belongs to (ObjectDependencies, ChildObjectTypes, BindingObjectTypes, OtherBindingObjectTypes).
        /// </summary>
        /// <param name="objectTypes">Object types to process</param>
        internal static void EnsureObjectTypeDynamicListsInternal(List<string> objectTypes)
        {
            // Ensure object dependencies
            EnsureObjectTypeDependencies(objectTypes);

            // Ensure ChildObjectTypes, BindingObjectTypes and OtherBindingObjectTypes lists
            // This HAS to be called after the ObjectDependencies are computed!
            EnsureObjectTypeLists(objectTypes);
        }


        /// <summary>
        /// Computes ObjectDependencies from Extends field of TypeInfo of objects in the system.
        /// </summary>
        /// <param name="objectTypes">Object types to process</param>
        private static void EnsureObjectTypeDependencies(IEnumerable<string> objectTypes)
        {
            foreach (var objectType in objectTypes)
            {
                var typeInfo = GetTypeInfo(objectType);
                if (typeInfo != null)
                {
                    EnsureObjectTypeInfoDependencies(typeInfo);
                }
            }
        }


        /// <summary>
        /// Computes ObjectDependencies from Extends field of TypeInfo of objects in the system.
        /// </summary>
        /// <param name="typeInfo">Object type info to process</param>
        private static void EnsureObjectTypeInfoDependencies(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                // No type info provided
                return;
            }

            UpdateExtendedObjectTypes(typeInfo);

            UpdateCompositeObjectType(typeInfo);
        }


        /// <summary>
        /// Adds object dependency for all type infos that are extended by the <paramref name="extendingTypeInfo"/>.
        /// </summary>
        private static void UpdateExtendedObjectTypes(ObjectTypeInfo extendingTypeInfo)
        {
            if (extendingTypeInfo.Extends == null)
            {
                // Given type info does not extend any other type infos
                return;
            }

            // Update each extended type info
            var objectType = extendingTypeInfo.ObjectType;
            foreach (var extraColumn in extendingTypeInfo.Extends)
            {
                var extendedInfo = GetTypeInfo(extraColumn.ExtendedObjectType);
                if (extendedInfo != null)
                {
                    AddObjectDependency(extendedInfo, new ObjectDependency(extraColumn.ColumnName, objectType, extraColumn.DependencyType));

                    // Add extended dependency for composite type info - composite merges dependencies its sub-type infos
                    var compositeTypeInfo = GetCompositeTypeInfo(extendedInfo);
                    if (compositeTypeInfo != null)
                    {
                        AddObjectDependency(compositeTypeInfo, new ObjectDependency(extraColumn.ColumnName, objectType, extraColumn.DependencyType));
                    }
                }
            }
        }


        /// <summary>
        /// Adds <paramref name="componentTypeInfo"/> into the <see cref="ObjectTypeInfo.ConsistsOf"/> collection
        /// of the composite object type (set in <see cref="ObjectTypeInfo.CompositeObjectType"/>
        /// property of the <paramref name="componentTypeInfo"/>), if there is any.
        /// </summary>
        private static void UpdateCompositeObjectType(ObjectTypeInfo componentTypeInfo)
        {
            if (componentTypeInfo is DynamicObjectTypeInfo)
            {
                // Dynamic types are not part of global composite objects
                return;
            }

            var compositeInfo = GetCompositeTypeInfo(componentTypeInfo);
            if (compositeInfo == null)
            {
                // Given type info is not hosted in another type info
                return;
            }

            var componentObjectType = componentTypeInfo.ObjectType;

            if (compositeInfo.ConsistsOf.Contains(componentObjectType))
            {
                // Component object type is already present in its composite object type
                return;
            }

            // Update composite object type collection.
            // New collection is created in order not to break enumeration of the original collection anywhere in the system.
            var components = new HashSet<string>(compositeInfo.ConsistsOf, StringComparer.InvariantCultureIgnoreCase);
            components.Add(componentObjectType);
            compositeInfo.ConsistsOf = components;
        }


        /// <summary>
        /// Returns composite object type for given <paramref name="typeInfo"/> or <see langword="null"/> if the info is not hosted in any other type info.
        /// </summary>
        /// <remarks>
        /// <see langword="Null"/> is also returned when composite info could not be retrieved.
        /// <para>As a side effect this method makes returned object type a composite (see <see cref="ObjectTypeInfo.IsComposite"/>)</para>
        /// </remarks>
        private static ObjectTypeInfo GetCompositeTypeInfo(ObjectTypeInfo typeInfo)
        {
            var composite = typeInfo.CompositeObjectType;
            if (composite == null)
            {
                // No composite object type set for the info
                return null;
            }

            var compositeInfo = GetTypeInfo(composite);
            if (compositeInfo == null)
            {
                // No composite object type info could be retrieved
                return null;
            }

            if (compositeInfo.ConsistsOf == null)
            {
                // Initialize composite object collection of hosted component object types
                compositeInfo.ConsistsOf =  new string[0];
            }

            return compositeInfo;
        }


        /// <summary>
        /// Gets the type info for the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="exceptionIfNotFound">If true, an exception is fired if the given object type is not found</param>
        public static ObjectTypeInfo GetTypeInfo(string objectType, bool exceptionIfNotFound = false)
        {
            var typeInfo = GetRegisteredTypeInfo(objectType);
            if (typeInfo == null)
            {
                // Try to initialize from object
                var info = ModuleManager.GetReadOnlyObject(objectType, exceptionIfNotFound);
                if (info != null)
                {
                    typeInfo = info.TypeInfo;
                }
            }

            return typeInfo;
        }
        

        /// <summary>
        /// Adds object dependency to the specified TypeInfo if it's not present yet.
        /// </summary>
        /// <param name="typeInfo">TypeInfo where to add dependencies</param>
        /// <param name="dependency">Dependency to add</param>
        private static void AddObjectDependency(ObjectTypeInfo typeInfo, ObjectDependency dependency)
        {
            if (!typeInfo.ObjectDependenciesInternal.Contains(dependency))
            {
                typeInfo.ObjectDependenciesInternal.Add(dependency);
            }
        }


        /// <summary>
        /// Registers the specified types to all lists where it belongs to (ChildObjectTypes, BindingObjectTypes, OtherBindingObjectTypes).
        /// </summary>
        /// <param name="typeInfo">Object type info to process</param>
        private static void EnsureObjectTypeInfoList(ObjectTypeInfo typeInfo)
        {
            if ((typeInfo != null) && !typeInfo.IsListingObjectTypeInfo)
            {
                AddToObjectTypeLists(typeInfo.ObjectType, typeInfo);
            }
        }


        /// <summary>
        /// Registers the specified types to all lists where it belongs to (ChildObjectTypes, BindingObjectTypes, OtherBindingObjectTypes).
        /// </summary>
        /// <param name="objectTypes">Object types to process</param>
        private static void EnsureObjectTypeLists(IEnumerable<string> objectTypes)
        {
            // Ensure ChildObjectTypes, BindingObjectTypes and OtherBindingObjectTypes lists
            // This HAS to be called after the ObjectDependencies are computed!
            foreach (var objType in objectTypes)
            {
                if (!String.IsNullOrEmpty(objType))
                {
                    ObjectTypeInfo typeInfo = GetTypeInfo(objType);
                    EnsureObjectTypeInfoList(typeInfo);
                }
            }
        }


        /// <summary>
        /// Registers the specified type to all lists where it belongs to (ChildObjectTypes, BindingObjectTypes, OtherBindingObjectTypes).
        /// </summary>
        /// <param name="objectType">Object type to register</param>
        /// <param name="typeInfo">Type info of the type to which it should register</param>
        private static void AddToObjectTypeLists(string objectType, ObjectTypeInfo typeInfo)
        {
            bool processChildren = (typeInfo.RegisterAsChildToObjectTypes == null);
            bool processBindings = (typeInfo.RegisterAsBindingToObjectTypes == null);
            bool processOtherBindings = (typeInfo.RegisterAsOtherBindingToObjectTypes == null);

            // Children
            if (!processChildren)
            {
                foreach (var parentObjType in typeInfo.RegisterAsChildToObjectTypes)
                {
                    var parent = GetTypeInfo(parentObjType);
                    if (parent != null)
                    {
                        AddToObjectTypesList(objectType, parent, ti => ti.ChildObjectTypes);
                    }
                }
            }

            // Bindings
            if (!processBindings)
            {
                foreach (var parentObjType in typeInfo.RegisterAsBindingToObjectTypes)
                {
                    var parent = GetTypeInfo(parentObjType);
                    if (parent != null)
                    {
                        AddToObjectTypesList(objectType, parent, ti => ti.BindingObjectTypes);
                    }
                }
            }

            // Other bindings
            if (!processOtherBindings)
            {
                foreach (var parentObjType in typeInfo.RegisterAsOtherBindingToObjectTypes)
                {
                    var parent = GetTypeInfo(parentObjType);
                    if (parent != null)
                    {
                        AddToObjectTypesList(objectType, parent, ti => ti.OtherBindingObjectTypes);
                    }
                }
            }

            // Automatic process beginning
            if ((processBindings || processChildren) && !String.IsNullOrEmpty(typeInfo.ParentObjectType))
            {
                var parent = GetTypeInfo(typeInfo.ParentObjectType);
                if (parent != null)
                {
                    if (typeInfo.IsBinding)
                    {
                        if (processBindings)
                        {
                            // Parent object type of binding means to register the object type as a binding
                            // The rest of the binding foreign keys are other bindings
                            AddToObjectTypesList(objectType, parent, ti => ti.BindingObjectTypes);
                        }
                    }
                    else
                    {
                        if (processChildren)
                        {
                            // Object is standard child object (= not binding and has parent), register it as a child object type
                            AddToObjectTypesList(objectType, parent, ti => ti.ChildObjectTypes);
                        }
                    }
                }
            }

            // Register other bindings
            if (processOtherBindings && (typeInfo.ObjectDependencies != null))
            {
                foreach (var dep in typeInfo.ObjectDependencies)
                {
                    if (dep.DependencyType == ObjectDependencyEnum.Binding)
                    {
                        var depObj = GetTypeInfo(dep.DependencyObjectType);
                        if (depObj != null)
                        {
                            AddToObjectTypesList(objectType, depObj, ti => ti.OtherBindingObjectTypes);
                        }
                    }
                }
            }

            // Register site bindings
            if (typeInfo.IsSiteBinding)
            {
                var site = GetTypeInfo(PredefinedObjectType.SITE);
                if (site != null)
                {
                    AddToObjectTypesList(objectType, site, ti => ti.OtherBindingObjectTypes);
                }
            }
        }


        /// <summary>
        /// Adds specified object type to the object type collection if type is not already present.
        /// </summary>
        /// <param name="objectType">Object type to add</param>
        /// <param name="typeInfo">Type info object that is owner of the collection</param>
        /// <param name="collectionSelector">Function that selects collection from type info object</param>
        private static void AddToObjectTypesList(string objectType, ObjectTypeInfo typeInfo, Func<ObjectTypeInfo, ICollection<string>> collectionSelector)
        {
            var collection = collectionSelector(typeInfo);

            if (!collection.Contains(objectType))
            {
                collection.Add(objectType);
            }

            var compositeTypeInfo = typeInfo.CompositeObjectType != null ? GetTypeInfo(typeInfo.CompositeObjectType) : null;
            if (compositeTypeInfo != null)
            {
                collection = collectionSelector(compositeTypeInfo);
                if (!collection.Contains(objectType))
                {
                    collection.Add(objectType);
                }
            }
        }


        /// <summary>
        /// Adds the object type to the list of object types
        /// </summary>
        /// <param name="objectType">Object type to add</param>
        private static void AddObjectType(string objectType)
        {
            if (!Instance.mExistingObjectTypes.Contains(objectType))
            {
                Instance.mExistingObjectTypes.Add(objectType);
            }
        }


        /// <summary>
        /// Creates an empty object of the given type
        /// </summary>
        /// <param name="objectType">Object type</param>
        internal static BaseInfo GetEmptyObject(string objectType)
        {
            return ObjectGenerator.CreateNewObject(objectType);
        }


        /// <summary>
        /// Registers the object type
        /// </summary>
        /// <param name="objectType">Object type to be registered.</param>
        /// <param name="type">Type to be registered.</param>
        /// <param name="typeInfo">TypeInfo configuration of the registered object type.</param>
        /// <exception cref="ArgumentException">Thrown when object type is either null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either type info or type is null.</exception>
        public static void RegisterObjectType(string objectType, ObjectTypeInfo typeInfo, Type type)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentException("Object type cannot neither be null nor empty.", "objectType");
            }
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Instance.mTypesByObjectType[objectType] = typeInfo;

            // Register factory
            var factory = new ObjectFactory(type)
                {
                    Initializer = (o => InitializeInfo(o, typeInfo))
                };

            ObjectGenerator.RegisterObjectType(objectType, factory);

            // Add to the list for given type
            var list = Instance.mTypeInfosByType[type];
            if (list == null)
            {
                list = new List<ObjectTypeInfo>();
                Instance.mTypeInfosByType[type] = list;
            }

            list.Add(typeInfo);

            AddObjectType(objectType);
        }


        /// <summary>
        /// Initializes the given info object with the given type info
        /// </summary>
        /// <param name="o">Object to initialize</param>
        /// <param name="typeInfo">Type info of the initialized object.</param>
        private static object InitializeInfo(object o, ObjectTypeInfo typeInfo)
        {
            var info = (BaseInfo)o;
            info.TypeInfo = typeInfo;

            // Check that constructor didn't set any properties
            if (info.Generalized.HasData)
            {
                throw new Exception("Class '" + info.GetType().FullName + "' set some data properties within it's default constructor, which caused too early initialization of the object data. Default properties must be set in method LoadDefaultData, or in special cases (in case you don't want to make that data default), you need to call method EnsureData before setting the properties.");
            }

            return info;
        }


        /// <summary>
        /// Registers the specified types to object type tree
        /// </summary>
        /// <param name="tree">Object type tree</param>
        /// <param name="getLocations">Function to provide locations for the given object type</param>
        public static void RegisterTypesToObjectTree(ObjectTypeTreeNode tree, Func<ObjectTypeInfo, IEnumerable<ObjectTreeLocation>> getLocations)
        {
            foreach (var objectType in ExistingObjectTypes)
            {
                var typeInfo = GetTypeInfo(objectType);
                if (typeInfo != null)
                {
                    var locations = getLocations(typeInfo);
                    if (locations != null)
                    {
                        foreach (var location in locations)
                        {
                            // Find parent node
                            var parent = tree.EnsureNode(location.ParentPath);

                            // If already exists, do not add
                            var newType = location.ObjectType ?? typeInfo.ObjectType;
                            if (parent.FindNode(newType, parent.Site) == null)
                            {
                                // Create new node
                                var node = new ObjectTypeTreeNode(newType, location, parent.Site);

                                parent.ChildNodes.Add(node);
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region "TypeInfo management methods"

        /// <summary>
        /// Gets the specific type info based on the object type name.
        /// </summary>
        /// <param name="objectType">Object type</param>
        private static ObjectTypeInfo GetRegisteredTypeInfo(string objectType)
        {
            if (!String.IsNullOrEmpty(objectType))
            {
                var typeInfo = Instance.mTypesByObjectType[objectType];
                if (typeInfo != null)
                {
                    return typeInfo;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets all the type infos registered for the given system type
        /// </summary>
        /// <param name="type">System type</param>
        public static IEnumerable<ObjectTypeInfo> GetTypeInfos(Type type)
        {
            EnsureObjectTypes();

            var list = Instance.mTypeInfosByType[type];
            if (list != null)
            {
                foreach (var typeInfo in list)
                {
                    yield return typeInfo;
                }
            }
        }

        #endregion
    }
}
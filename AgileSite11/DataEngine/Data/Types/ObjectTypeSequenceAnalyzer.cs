using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides the sequence of object types ordered by their dependencies.
    /// Dependencies are returned before the object type on which they depend on.
    /// </summary>
    public class ObjectTypeSequenceAnalyzer
    {
        /// <summary>
        /// Set of object types that are already processed. Types that are not returned in output sequence are added to this collection as well. Prevents infinite loops.
        /// </summary>
        private HashSet<string> mProcessedObjectTypes;

        /// <summary>
        /// Set of object types that are already part of the final output sequence.
        /// </summary>
        private HashSet<string> mReturnedObjectTypes;

        /// <summary>
        /// Set of object types that are designated as output types.
        /// </summary>
        private readonly HashSet<string> mOutputObjectTypes;

        private Stack<string> mCurrentStack;
        private Stack<string> mOutputStack;

        private List<string> mDependantObjectTypes;
        private List<string> mDynamicDependantObjectTypes;

        /// <summary>
        /// Object types that are processed as the first for every sequence. 
        /// </summary>
        /// <remarks>
        /// Every object type depends on its <see cref="DataClassInfo.OBJECT_TYPE"/> and <see cref="QueryInfo.OBJECT_TYPE"/> for data manipulation 
        /// so this object types are processed at the start of the sequence.
        /// </remarks>
        private readonly List<string> mSystemRootObjectTypes = new List<string>() { DataClassInfo.OBJECT_TYPE, QueryInfo.OBJECT_TYPE };


        /// <summary>
        /// List of input object types
        /// </summary>
        private IEnumerable<string> ObjectTypes
        {
            get;
            set;
        }


        private IObjectTypeFilter ObjectTypeFilter
        {
            get;
            set;
        }

        /// <summary>
        /// List of available object types
        /// </summary>
        private IEnumerable<string> AvailableTypes
        {
            get;
            set;
        }


        /// <summary>
        /// Log action for individual items. The action is called with three parameters - the log message, indentation level and whether a cycle has been detected.
        /// </summary>
        public Action<string, int, bool> Log
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">Filtering logic for the output collection.</param>
        public ObjectTypeSequenceAnalyzer(IObjectTypeFilter filter)
            : this(filter, ObjectTypeManager.AllObjectTypes)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">Filtering logic for the output collection.</param>
        /// <param name="types">Object types to be ordered.</param>
        internal ObjectTypeSequenceAnalyzer(IObjectTypeFilter filter, IEnumerable<string> types)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            ObjectTypeFilter = filter;
            AvailableTypes = types;

            // Some custom table object types do not have to be known at the beginning of the enumeration
            // as their metadata are yet to be created
            var customTableObjectTypes = new String[0];

            var filterImplementation = filter as EnumerationObjectTypeFilter;
            if (filterImplementation != null)
            {
                customTableObjectTypes = filterImplementation.ObjectTypes
                    .Intersect(AvailableTypes, StringComparer.InvariantCultureIgnoreCase)
                    .Where(x => x.StartsWith(PredefinedObjectType.CUSTOM_TABLE_ITEM_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            }

            var availableTypesWithoutCustomTableTypes = AvailableTypes.Except(customTableObjectTypes, StringComparer.InvariantCultureIgnoreCase);
            var objectTypesWithoutCustomTableTypes = ObjectTypeManager.GetObjectTypes(availableTypesWithoutCustomTableTypes, ObjectTypeFilter.IsIncludedType);
            ObjectTypes = objectTypesWithoutCustomTableTypes.Union(customTableObjectTypes, StringComparer.InvariantCultureIgnoreCase);

            mOutputObjectTypes = new HashSet<string>(ObjectTypes, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets the ordered sequence of object types
        /// </summary>
        public IEnumerable<ObjectTypeListItem> GetSequence()
        {
            mProcessedObjectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mReturnedObjectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            mCurrentStack = new Stack<string>();
            mOutputStack = new Stack<string>();
            mDependantObjectTypes = new List<string>();
            mDynamicDependantObjectTypes = new List<string>();

            // Normalize initial order
            var objectTypes = ObjectTypes.OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
            Func<ObjectTypeListItem, ObjectTypeListItem> AddToReturnedTypesAndReturn = listItem =>
            {
                mReturnedObjectTypes.Add(listItem.ObjectType);
                return listItem;
            };

            // Return system root object types sequence.
            foreach (var startingType in GetSequence(mSystemRootObjectTypes, 0).SelectMany(GetObjectTypeListItems))
            {
                yield return AddToReturnedTypesAndReturn(startingType);
            }

            // Return root object types (with no dependencies) are returned with priority 
            foreach (var rootType in GetSequenceWithBindings(GetRootSequence(objectTypes)).SelectMany(GetObjectTypeListItems))
            {
                yield return AddToReturnedTypesAndReturn(rootType);
            }

            // Return dependent object types
            foreach (var dependantType in GetSequenceWithBindings(GetSequence(mDependantObjectTypes, 0)).SelectMany(GetObjectTypeListItems))
            {
                yield return AddToReturnedTypesAndReturn(dependantType);
            }

            // Object types with dynamic dependencies are returned after all other types
            foreach (var dynamicDependantType in GetSequenceWithBindings(GetSequence(mDynamicDependantObjectTypes, 0)).SelectMany(GetObjectTypeListItems))
            {
                yield return AddToReturnedTypesAndReturn(dynamicDependantType);
            }
        }


        /// <summary>
        /// Returns each element of given sequence along with its bindings that do not have any unprocessed dependency.
        /// </summary>
        /// <param name="sequence">Sequence of ordered object type infos.</param>
        private IEnumerable<ObjectTypeInfo> GetSequenceWithBindings(IEnumerable<ObjectTypeInfo> sequence)
        {
            foreach (var typeInfo in sequence)
            {
                yield return typeInfo;

                foreach (var processedBinding in GetRelatedBindingsWithProcessedDependencies(typeInfo))
                {
                    yield return processedBinding;
                }
            }
        }


        /// <summary>
        /// Gets all bindings related to given object type that have all their dependencies already processed.
        /// </summary>
        /// <param name="typeInfo">Type info which bindings are returned</param>
        private IEnumerable<ObjectTypeInfo> GetRelatedBindingsWithProcessedDependencies(ObjectTypeInfo typeInfo)
        {
            var bindingTypesWithProcessedDependencies = typeInfo.BindingObjectTypes.Concat(typeInfo.OtherBindingObjectTypes)
                .Where(bindingType => !mReturnedObjectTypes.Contains(bindingType) && IsOutputType(bindingType))
                .Select(GetTypeInfo).Where(ti => !HasObjectTypeUnprocessedDependencies(ti));

            foreach (var bindingType in bindingTypesWithProcessedDependencies)
            {
                mProcessedObjectTypes.Add(bindingType.ObjectType);

                yield return bindingType;
            }
        }


        /// <summary>
        /// Returns true if one or more dependency types of given type info haven't been processed yet.
        /// </summary>
        /// <param name="typeInfo">Type info which dependencies are checked</param>
        private bool HasObjectTypeUnprocessedDependencies(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                return false;
            }

            var dependencyTypes = GetDependencyTypes(typeInfo, true);

            // Get dependencies for children that are processed together with the parent
            var childTypes = typeInfo.ChildObjectTypes.Select(childType => ObjectTypeManager.GetTypeInfo(childType)).Where(info => info != null).Where(ObjectTypeFilter.IsChildIncludedToParent).Select(info => info.ObjectType);

            // Get dependencies from bindings that are processed together with the parent
            var bindingTypes = typeInfo.BindingObjectTypes.Select(childType => ObjectTypeManager.GetTypeInfo(childType)).Where(info => info != null).Where(ObjectTypeFilter.IsBindingIncludedToParent).Select(info => info.ObjectType);

            var unprocessedDependencies = dependencyTypes.Concat(childTypes).Concat(bindingTypes).Where(dep => !mReturnedObjectTypes.Contains(dep)).ToList();

            // Any unprocessed dependency of output type ends any further investigation. 
            // Unprocessed dependencies that are not output types needs to be investigated - if they do not depend on any other output type they are considered as processed dependencies.
            return unprocessedDependencies.Where(IsOutputType).Any() || unprocessedDependencies.Any(dep => HasObjectTypeUnprocessedOutputDependencyInDependencyHierarchy(GetTypeInfo(dep)));
        }


        /// <summary>
        /// Returns true if given type info has any unprocessed output object type in its dependency hierarchy.
        /// </summary>
        /// <param name="typeInfo">Type info which dependencies are checked</param>
        private bool HasObjectTypeUnprocessedOutputDependencyInDependencyHierarchy(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                return false;
            }

            var subAnalyzer = InstantiateAnalyzer(mReturnedObjectTypes);
            var firstDependency = subAnalyzer.GetSequence(typeInfo, 0).FirstOrDefault();

            return (firstDependency != null) && (firstDependency.ObjectType != typeInfo.ObjectType);
        }


        /// <summary>
        /// Gets the ordered sequence of root object types that have no dependency.
        /// Unprocessed dependent object types will be stored in <see cref="mDependantObjectTypes"/> collection
        /// when enumeration of all returned items is finished.
        /// Unprocessed object types with dynamic dependency will be stored in <see cref="mDynamicDependantObjectTypes"/> collection
        /// when enumeration of all returned items is finished.
        /// </summary>
        /// <param name="objectTypes">Object types to process</param>
        private IEnumerable<ObjectTypeInfo> GetRootSequence(IEnumerable<string> objectTypes)
        {
            var subAnalyzer = InstantiateAnalyzer(Enumerable.Empty<string>());

            foreach (var objectType in objectTypes)
            {
                subAnalyzer.mProcessedObjectTypes.Clear();
                subAnalyzer.mOutputStack.Clear();

                var ti = ObjectTypeManager.GetTypeInfo(objectType);

                if (ti == null)
                {
                    var message = 
                        $"Cannot instantiate {typeof(ObjectTypeInfo).FullName} from given object type '{objectType}'.\n" +
                        $"The object type is not known in the system.\n" +
                        $"This error can be also caused by materialization of elements from the tail of the sequence when metadata from head elements of the sequence were not processed.";
                    throw new InvalidOperationException(message);
                }

                if (HasDynamicDependency(ti))
                {
                    // Object types with dynamic dependencies will be processed later
                    mDynamicDependantObjectTypes.Add(objectType);
                }
                else
                {
                    // Using statement is used to dispose partial enumeration
                    using (var sequenceEnum = subAnalyzer.GetSequence(ti, 0).GetEnumerator())
                    {
                        if (!sequenceEnum.MoveNext())
                        {
                            continue;
                        }

                        var first = sequenceEnum.Current;

                        if (first.ObjectType == objectType)
                        {
                            if (!mProcessedObjectTypes.Add(objectType))
                            {
                                continue;
                            }

                            // Object doesn't have dependencies 
                            if (Log != null)
                            {
                                Log(objectType, 0, false);
                            }

                            yield return first;
                            while (sequenceEnum.MoveNext())
                            {
                                yield return sequenceEnum.Current;
                            }
                        }
                        else
                        {
                            // Add object to list of dependent objects that need additional processing
                            mDependantObjectTypes.Add(objectType);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Instantiate analyzer that can be used for inspecting object type dependencies and has its own state.
        /// </summary>
        /// <param name="processedObjectTypes">List of processed object types to be used by the analyzer instance</param>
        private ObjectTypeSequenceAnalyzer InstantiateAnalyzer(IEnumerable<string> processedObjectTypes)
        {
            var subAnalyzer = new ObjectTypeSequenceAnalyzer(ObjectTypeFilter, AvailableTypes);
            subAnalyzer.mProcessedObjectTypes = new HashSet<string>(processedObjectTypes, StringComparer.OrdinalIgnoreCase);
            subAnalyzer.mCurrentStack = new Stack<string>();
            subAnalyzer.mOutputStack = new Stack<string>();

            return subAnalyzer;
        }


        /// <summary>
        /// Gets the ordered sequence of object types
        /// </summary>
        /// <param name="objectTypes">Object types to process</param>
        /// <param name="indent">Current indentation</param>
        /// <param name="condition">Additional condition for the given type</param>
        /// <param name="isChild">If true, the processed object type is in child relationship to the object type processed above</param>
        private IEnumerable<ObjectTypeInfo> GetSequence(IEnumerable<string> objectTypes, int indent, Func<ObjectTypeInfo, bool> condition = null, bool isChild = false)
        {
            return objectTypes.Select(GetTypeInfo).Where(ti => condition == null || condition(ti)).SelectMany(ti => GetSequence(ti, indent, isChild));
        }


        /// <summary>
        /// Gets a dependency sequence for the given object type
        /// </summary>
        /// <param name="typeInfo">Object type info</param>
        /// <param name="indent">Current indentation</param>
        /// <param name="isChild">If true, the processed object type is in child relationship to the object type processed above</param>
        private IEnumerable<ObjectTypeInfo> GetSequence(ObjectTypeInfo typeInfo, int indent, bool isChild = false)
        {
            // Get type info
            if (typeInfo == null)
            {
                yield break;
            }
            var objectType = typeInfo.ObjectType;

            // Detect cycle
            var isOutputType = IsOutputType(objectType);
            if (isOutputType &&
                (mOutputStack.Count > 1) &&
                (mOutputStack.Peek() != objectType) &&
                mOutputStack.Contains(objectType))
            {
                if (Log != null)
                {
                    Log(String.Join(" > ", mCurrentStack.Reverse()) + " > " + objectType, indent, true);
                }
            }

            // Only process if the given object type was not yet processed
            if (mProcessedObjectTypes.Add(objectType))
            {
                // Add to processing stacks for cycle logging
                if (Log != null)
                {
                    mCurrentStack.Push(objectType);
                }

                if (isOutputType)
                {
                    mOutputStack.Push(objectType);
                }

                // Get all regular object dependencies
                var dependencies = GetDependencyTypes(typeInfo, !isChild);
                var dependencySequence = GetSequence(dependencies, indent + 1);

                // Get dependencies for children that are processed together with the parent
                var childSequence = GetSequence(typeInfo.ChildObjectTypes, indent + 1, ObjectTypeFilter.IsChildIncludedToParent, true);

                // Get dependencies from bindings that are processed together with the parent
                var bindingSequence = GetSequence(typeInfo.BindingObjectTypes, indent + 1, ObjectTypeFilter.IsBindingIncludedToParent, true);

                // Place type dependencies before the type itself.
                foreach (var dependencyType in dependencySequence.Concat(childSequence).Concat(bindingSequence))
                {
                    yield return dependencyType;
                }

                // Output only types which are in the input collection
                if (isOutputType)
                {
                    if (Log != null)
                    {
                        // Log the item
                        var message = objectType;
                        if (indent > 0)
                        {
                            message += " through " + String.Join(" > ", mCurrentStack.Skip(1).Reverse().Skip(1));
                        }

                        Log(message, indent, false);
                    }

                    yield return typeInfo;

                    mOutputStack.Pop();
                }

                if (Log != null)
                {
                    mCurrentStack.Pop();
                }
            }
        }


        private bool IsOutputType(string objectType)
        {
            return mOutputObjectTypes.Contains(objectType);
        }


        /// <summary>
        /// Collects all dependencies of given type, including the parent and site.
        /// </summary>
        /// <param name="typeInfo">TypeInfo to  collect dependencies from.</param>
        /// <param name="getParentTypes">Indicates whether the parent object type(s) should be returned.</param>
        private static IEnumerable<string> GetDependencyTypes(ObjectTypeInfo typeInfo, bool getParentTypes)
        {
            foreach (var indirectDependency in typeInfo.DependsOnIndirectly)
            {
                yield return indirectDependency;
            }

            // Get dependencies from other foreign keys
            var dependencies = typeInfo.ObjectDependencies;
            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                {
                    yield return dep.DependencyObjectType;
                }
            }

            // Add dependency on site
            if (typeInfo.IsSiteObject)
            {
                yield return PredefinedObjectType.SITE;
            }

            if (getParentTypes)
            {
                if (typeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    yield return typeInfo.ParentObjectType;
                }

                if (typeInfo.RegisterAsChildToObjectTypes != null)
                {
                    foreach (var parentType in typeInfo.RegisterAsChildToObjectTypes)
                    {
                        yield return parentType;
                    }
                }

                if (typeInfo.RegisterAsBindingToObjectTypes != null)
                {
                    foreach (var parentType in typeInfo.RegisterAsBindingToObjectTypes)
                    {
                        yield return parentType;
                    }
                }

                if (typeInfo.RegisterAsOtherBindingToObjectTypes != null)
                {
                    foreach (var parentType in typeInfo.RegisterAsOtherBindingToObjectTypes)
                    {
                        yield return parentType;
                    }
                }
            }

            // Object type is part of bigger composite object. 
            // Composite type must be placed before dependent objects of component type. 
            if (typeInfo.CompositeObjectType != null)
            {
                yield return typeInfo.CompositeObjectType;
            }

            // If type is composite also get dependencies from component types
            if (typeInfo.IsComposite)
            {
                var componentTypes = typeInfo
                    .ConsistsOf
                    .Select(objectType => ObjectTypeManager.GetTypeInfo(objectType, true));

                foreach (var component in componentTypes)
                {
                    yield return component.ObjectType;
                }
            }
        }


        /// <summary>
        /// Creates object type list items for the given object type.
        /// </summary>
        /// <param name="typeInfo">Object type info</param>
        private static IEnumerable<ObjectTypeListItem> GetObjectTypeListItems(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                yield break;
            }

            bool isSite = false;
            bool hasDynamicDependency = HasDynamicDependency(typeInfo);

            if (IsSiteObject(typeInfo))
            {
                if (typeInfo.SupportsGlobalObjects)
                {
                    // Register the object again in global variant
                    var siteObj = new ObjectTypeListItem
                    {
                        ObjectType = typeInfo.ObjectType,
                        IsSite = false,
                        HasDynamicDependency = hasDynamicDependency
                    };

                    yield return siteObj;
                }

                isSite = true;
            }

            var globalObj = new ObjectTypeListItem
            {
                ObjectType = typeInfo.ObjectType,
                IsSite = isSite,
                HasDynamicDependency = hasDynamicDependency
            };

            yield return globalObj;
        }


        /// <summary>
        /// Returns true if the object is site object.
        /// </summary>
        /// <param name="typeInfo">Object type info to check</param>
        private static bool IsSiteObject(ObjectTypeInfo typeInfo)
        {
            // Special case for site object - backward compatibility
            if (typeInfo.ObjectType == PredefinedObjectType.SITE)
            {
                return true;
            }

            return typeInfo.IsSiteObject;
        }


        /// <summary>
        /// Returns true if the given object type has any dynamic dependency.
        /// </summary>
        /// <param name="typeInfo">Object type info to check</param>
        private static bool HasDynamicDependency(ObjectTypeInfo typeInfo)
        {
            return typeInfo.ObjectDependencies.Any(objectDependency => objectDependency.HasDynamicObjectType());
        }
        

        private static ObjectTypeInfo GetTypeInfo(string objectType)
        {
            return ObjectTypeManager.GetTypeInfo(objectType);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;

using ObjectTypeTransformation = System.Func<string, string>;
using IsObjectTypeIncludedCondition = System.Func<string, CMS.ContinuousIntegration.Internal.FileSystemRepositoryConfiguration, bool>;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class responsible for evaluating <see cref="FileSystemRepositoryConfiguration"/>.
    /// </summary>
    public class RepositoryConfigurationEvaluator : AbstractHelper<RepositoryConfigurationEvaluator>
    {
        private static volatile List<IsObjectTypeIncludedCondition> mIsObjectTypeIncludedConditions = new List<IsObjectTypeIncludedCondition>
        {
            IsObjectTypeIncludedDefaultCondition
        };
        private static volatile Dictionary<string, ObjectTypeTransformation> mObjectTypeTypeTransformations = new Dictionary<string, ObjectTypeTransformation>();
        private static readonly object mCollectionsLock = new object();


        /// <summary>
        /// Registers a new condition which evaluates whether an object type is to be included in the repository based on repository configuration.
        /// </summary>
        /// <param name="condition">Condition to be registered.</param>
        public static void AddIsObjectTypeIncludedCondition(IsObjectTypeIncludedCondition condition)
        {
            lock (mCollectionsLock)
            {
                // The collection of conditions is designed for lock-free reading
                var conditions = new List<IsObjectTypeIncludedCondition>(mIsObjectTypeIncludedConditions.Count + 1);
                conditions.Add(condition);
                conditions.AddRange(mIsObjectTypeIncludedConditions);

                // Volatile variable assignment does not prevent write instructions reordering
                Thread.MemoryBarrier();
                mIsObjectTypeIncludedConditions = conditions;
            }
        }


        /// <summary>
        /// Registers a new transformation function which processes given object type string into another string that will be used in repository configuration evaluations instead of the original one.
        /// </summary>
        /// <param name="prefix">Transformation function will be executed only on object type strings that match this prefix.</param>
        /// <param name="transformation">Transformation function to be registered.</param>
        public static void AddObjectTypeTransformation(string prefix, ObjectTypeTransformation transformation)
        {
            lock (mCollectionsLock)
            {
                // The collection of conditions is designed for lock-free reading
                var transformations = new Dictionary<string, ObjectTypeTransformation>(mObjectTypeTypeTransformations) { { prefix, transformation } };

                // Volatile variable assignment does not prevent write instructions reordering
                Thread.MemoryBarrier();
                mObjectTypeTypeTransformations = transformations;
            }
        }


        /// <summary>
        /// Indicates whether specified object type should be included in the repository based on repository configuration.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <param name="configuration">File system repository configuration.</param>
        public static bool IsObjectTypeIncluded(string objectType, FileSystemRepositoryConfiguration configuration)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                // Undefined object type cannot be included
                return false;
            }

            foreach (var condition in mIsObjectTypeIncludedConditions)
            {
                if (condition(objectType, configuration))
                {
                    return true;
                }
            }

            // Provided object type met none of included conditions thus cannot be included
            return false;
        }



        /// <summary>
        /// Returns object type string that should be used instead of passed <paramref name="objectType"/> in repository configuration evaluations.
        /// </summary>
        /// <param name="objectType">Object type to be converted to repository object type.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="objectType"/> is null.</exception>
        internal static string GetRepositoryObjectType(string objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            string transformedObjectType = null;
            var matchedTransformations = mObjectTypeTypeTransformations
                .Where(pair => objectType.StartsWith(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                .Take(1);

            foreach (var transformation in matchedTransformations)
            {
                transformedObjectType = transformation.Value(objectType);
            }

            return transformedObjectType ?? objectType;
        }


        /// <summary>
        /// Default condition which checks whether an object type is explicitly named in
        /// the <paramref name="configuration"/>'s <see cref="FileSystemRepositoryConfiguration.ObjectTypes"/> set.
        /// </summary>
        private static bool IsObjectTypeIncludedDefaultCondition(string objectType, FileSystemRepositoryConfiguration configuration)
        {
            var repositoryObjectType = GetRepositoryObjectType(objectType);
            return configuration.ObjectTypes.Contains(repositoryObjectType);
        }


        /// <summary>
        /// Returns true if object is to be included in the repository based on repository configuration.
        /// </summary>
        /// <param name="info">Info object to be processed</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <remarks>
        /// Returns true if root parent of given <paramref name="info"/> object is to be included.
        /// </remarks>
        public static bool IsObjectIncluded(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            return HelperObject.IsObjectIncludedInternal(info, configuration, translationHelper);
        }


        /// <summary>
        /// Returns true if object is to be included in the repository based on repository configuration.
        /// </summary>
        /// <param name="info">Info object to be processed</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <remarks>
        /// Returns true if root parent of given <paramref name="info"/> object is to be included.
        /// </remarks>
        protected virtual bool IsObjectIncludedInternal(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var typeInfo = info.TypeInfo;
            var objectType = typeInfo.ObjectType;

            if (!IsObjectTypeIncluded(objectType, configuration))
            {
                // Object type itself is not included in the configuration
                return false;
            }

            translationHelper = translationHelper ?? new ContinuousIntegrationTranslationHelper();

            if (IsNewObject(info, translationHelper))
            {
                // For new object, just check that object meets codename filter
                if (!IsObjectIncludedByCodenameFilter(info, configuration))
                {
                    return false;
                }
            }
            else
            {
                // For existing object, check that object meets the whole type filter condition
                if (!IsObjectIncludedByFilterCondition(info, configuration, translationHelper))
                {
                    return false;
                }
            }

            if (typeInfo.IsBinding)
            {
                return IsBindingIncluded(info, configuration, translationHelper);
            }

            return IsObjectIncludedByRelatedObjects(info, configuration, translationHelper, false);
        }


        /// <summary>
        /// Returns true if given object is a new object to be inserted to database, i.e. its ID is equal to zero.
        /// </summary>
        private bool IsNewObject(BaseInfo info, ContinuousIntegrationTranslationHelper translationHelper)
        {
            return (GetObjectId(info, translationHelper) == 0);
        }


        /// <summary>
        /// Returns true if the given object is not filtered out by any of its related objects (parent, category, ...).
        /// The method does not check binding dependencies.
        /// </summary>
        /// <param name="info">Info object to be processed</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <param name="includeIfParentObjectTypeExcluded">If true, excluded parent object type does not exclude the info object</param>
        private static bool IsObjectIncludedByRelatedObjects(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper, bool includeIfParentObjectTypeExcluded = true)
        {
            var category = info.Generalized.ObjectCategory;
            if ((category != null) && !IsObjectIncludedByReference(category.TypeInfo.ObjectType, category.Generalized.ObjectID, configuration, translationHelper))
            {
                // Object is filtered out by its category
                return false;
            }

            if (!IsObjectIncludedByReference(info.Generalized.ParentObjectType, info.Generalized.ObjectParentID, configuration, translationHelper, includeIfParentObjectTypeExcluded))
            {
                // Object is filtered out by its parent
                return false;
            }

            if ((info.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !IsObjectIncludedByReference(PredefinedObjectType.SITE, info.Generalized.ObjectSiteID, configuration, translationHelper))
            {
                // Object is filtered out by its site
                return false;
            }

            if (!IsObjectIncludedByReference(PredefinedObjectType.GROUP, info.Generalized.ObjectGroupID, configuration, translationHelper))
            {
                // Object is filtered out by its group
                return false;
            }

            if (!IsObjectIncludedByFilterDependencies(info, configuration, translationHelper))
            {
                // Object is filtered out by another dependency
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true if <paramref name="info"/> object type is binding and no dependency is filtered out.
        /// </summary>
        private static bool IsBindingIncluded(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper)
        {
            var typeInfo = info.TypeInfo;
            if (!typeInfo.IsBinding)
            {
                // Info is not binding, it cannot be included as binding
                return false;
            }

            // Check if the binding object is filtered out by its related objects (parent, category, ...)
            if (!IsObjectIncludedByRelatedObjects(info, configuration, translationHelper))
            {
                return false;
            }

            var dependencies = typeInfo.ObjectDependencies.Where(d => d.DependencyType == ObjectDependencyEnum.Binding);

            // At least one of the bound object types have to be included, so check the parent object type
            var parentId = info.Generalized.ObjectParentID;
            if (parentId <= 0)
            {
                throw new InvalidOperationException("Binding '" + info + "' is missing the parent ID.");
            }

            var parentObject = translationHelper.TranslationReferenceLoader.LoadExtendedFromDatabase(info.Generalized.ParentObjectType, parentId);
            if (parentObject == null)
            {
                return false;
            }

            var isAnyObjectTypeIncluded = IsSuperiorObjectTypeIncluded(parentObject.ObjectType, configuration);

            foreach (var dependency in dependencies)
            {
                var dependencyObject = translationHelper.TranslationReferenceLoader.LoadExtendedFromDatabase(dependency.DependencyObjectType, info.GetIntegerValue(dependency.DependencyColumn, 0));
                if (dependencyObject == null)
                {
                    continue;
                }

                // Check each dependency object type if it is included
                isAnyObjectTypeIncluded |= IsSuperiorObjectTypeIncluded(dependencyObject.ObjectType, configuration);

                if (!IsIncludedByObject(dependencyObject, configuration, translationHelper))
                {
                    // Do not include binding if any of the bound objects is filtered out
                    return false;
                }
            }

            if (typeInfo.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // Group binding can be included by group
                isAnyObjectTypeIncluded |= IsObjectTypeIncluded(PredefinedObjectType.GROUP, configuration);
            }

            if (typeInfo.IsSiteBinding)
            {
                // Site binding can be included by site
                isAnyObjectTypeIncluded |= IsObjectTypeIncluded(PredefinedObjectType.SITE, configuration);
            }

            // At least one of the bound objects have to be included to include binding
            return isAnyObjectTypeIncluded;
        }


        /// <summary>
        /// Returns true if given reference does not filter out its dependent object.
        /// </summary>
        /// <param name="referenceObjectType">Referenced object's type</param>
        /// <param name="referenceId">Referenced object's ID</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <param name="includeIfExcludedObjectType">Indicates if dependent object is included even if reference object type is excluded</param>

        private static bool IsObjectIncludedByReference(string referenceObjectType, int referenceId, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper, bool includeIfExcludedObjectType = true)
        {
            if (referenceId <= 0)
            {
                return true;
            }

            var reference = translationHelper.TranslationReferenceLoader.LoadExtendedFromDatabase(referenceObjectType, referenceId);
            return IsObjectIncludedByReference(reference, configuration, translationHelper, includeIfExcludedObjectType);
        }


        /// <summary>
        /// Returns true if given reference does not filter out its dependent object.
        /// </summary>
        /// <param name="reference">Reference to check</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <param name="includeIfExcludedObjectType">Indicates if dependent object is included even if reference object type is excluded</param>
        private static bool IsObjectIncludedByReference(ExtendedTranslationReference reference, FileSystemRepositoryConfiguration configuration, TranslationHelper translationHelper, bool includeIfExcludedObjectType = true)
        {
            if (reference == null)
            {
                return true;
            }

            if (!IsSuperiorObjectTypeIncluded(reference.ObjectType, configuration))
            {
                if (!includeIfExcludedObjectType)
                {
                    return false;
                }

                // If site filter is defined, we need to pass the evaluation to IsIncludedByObject, because even that parent object is not excluded, we need to filter objects by site
                var siteFilter = GetObjectTypeFilterCondition(SiteInfo.OBJECT_TYPE, configuration);
                if (siteFilter == null)
                {
                    return true;
                }
            }

            return IsIncludedByObject(reference, configuration, translationHelper, false);
        }


        /// <summary>
        /// Returns true if the given object is not filtered out by its dependencies defined in <see cref="ContinuousIntegrationSettings.FilterDependencies"/>.
        /// </summary>
        private static bool IsObjectIncludedByFilterDependencies(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper)
        {
            foreach (var dependency in info.TypeInfo.ContinuousIntegrationSettings.FilterDependencies)
            {
                var dependencyId = info.GetIntegerValue(dependency.DependencyColumn, 0);
                if (!IsObjectIncludedByReference(dependency.DependencyObjectType, dependencyId, configuration, translationHelper))
                {
                    // Any filtered dependency filters out the object
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if the given object does not restrict its children.
        /// </summary>
        /// <param name="checkedObject">Translation reference of checked object</param>
        /// <param name="configuration">File system repository configuration</param>
        /// <param name="translationHelper">Translation helper object. Used to optimize database calls when asked for the same information multiple times.</param>
        /// <param name="includeIfParentObjectTypeExcluded">If true, excluded parent object type does not exclude the object</param>
        private static bool IsIncludedByObject(ExtendedTranslationReference checkedObject, FileSystemRepositoryConfiguration configuration, TranslationHelper translationHelper, bool includeIfParentObjectTypeExcluded = true)
        {
            if (checkedObject == null)
            {
                return true;
            }

            // Check category
            if (!IsObjectIncludedByReference(checkedObject.ExtendedCategory, configuration, translationHelper))
            {
                return false;
            }

            // Check parent
            if (!IsObjectIncludedByReference(checkedObject.ExtendedParent, configuration, translationHelper, includeIfParentObjectTypeExcluded))
            {
                return false;
            }

            // Check site
            if (!IsObjectIncludedByReference(checkedObject.ExtendedSite, configuration, translationHelper))
            {
                return false;
            }

            // Check group
            if (!IsObjectIncludedByReference(checkedObject.ExtendedGroup, configuration, translationHelper))
            {
                return false;
            }

            // check other filter dependencies
            if (!IsIncludedByFilterDependencies(checkedObject, configuration, translationHelper))
            {
                return false;
            }

            // Check filter 
            return IsObjectIncludedByFilterCondition(checkedObject, configuration, translationHelper);
        }


        /// <summary>
        ///  Returns true if the given object is not restricted by its filter dependencies.
        /// </summary>
        private static bool IsIncludedByFilterDependencies(ExtendedTranslationReference checkedObject, FileSystemRepositoryConfiguration configuration, TranslationHelper translationHelper)
        {
            foreach (var dependency in checkedObject.ExtendedFilterDependencies)
            {
                if (!IsObjectIncludedByReference(dependency, configuration, translationHelper))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if <paramref name="parentObjectType"/> is included in provided <paramref name="configuration"/> or
        /// it is a component object type and its composite object type is included in the <paramref name="configuration"/>.
        /// </summary>
        /// <remarks>
        /// This method only makes sense for types participating in an type relation as the master objects
        /// (either parent object types or object type other depend on in any way), hence the name.
        /// </remarks>
        private static bool IsSuperiorObjectTypeIncluded(string parentObjectType, FileSystemRepositoryConfiguration configuration)
        {
            var isIncluded = IsObjectTypeIncluded(parentObjectType, configuration);
            if (isIncluded)
            {
                // Object type itself is included, not further investigation required
                return true;
            }

            // Object type itself is not included, but its composite might be (if exists)
            var parentObjectTypeComposite = ObjectTypeManager.GetTypeInfo(parentObjectType).CompositeObjectType;
            isIncluded = IsObjectTypeIncluded(parentObjectTypeComposite, configuration);

            return isIncluded;
        }


        /// <summary>
        /// Checks if object's codename meets the codename filter specified by repository configuration.
        /// </summary>
        private static bool IsObjectIncludedByCodenameFilter(BaseInfo info, FileSystemRepositoryConfiguration configuration)
        {
            CodenameFilter codenameFilter;
            if (configuration.ObjectTypesCodenameFilter.TryGetValue(info.TypeInfo.ObjectType, out codenameFilter))
            {
                return codenameFilter.IsObjectIncluded(info);
            }

            return true;
        }


        /// <summary>
        /// Checks if object meets the object type filter condition.
        /// </summary>
        private static bool IsObjectIncludedByFilterCondition(BaseInfo info, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper)
        {
            var objectType = info.TypeInfo.ObjectType;

            // Filter out objects that don't meet the object type's filter conditions
            var objectTypeCondition = GetObjectTypeFilterCondition(objectType, configuration);
            if (objectTypeCondition != null)
            {
                var where = new WhereCondition().WhereEquals(info.TypeInfo.IDColumn, GetObjectId(info, translationHelper));

                if (!DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(objectType, configuration, where).Any())
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Checks if the object represented by the given translation reference meets filter condition.
        /// Returns true if the object can be stored.
        /// </summary>
        private static bool IsObjectIncludedByFilterCondition(ExtendedTranslationReference reference, FileSystemRepositoryConfiguration configuration, TranslationHelper translationHelper)
        {
            var included = true;
            var filteredObjectType = GetObjectTypeForFiltering(reference.ObjectType);
            var objectTypeCondition = GetObjectTypeFilterCondition(filteredObjectType, configuration);
            if (objectTypeCondition != null)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(reference.ObjectType);
                if (typeInfo == null)
                {
                    return false;
                }

                var where = new WhereCondition();

                if (reference.GUID != Guid.Empty)
                {
                    where = where.WhereEquals(typeInfo.GUIDColumn, reference.GUID);
                }
                if (!string.IsNullOrEmpty(reference.CodeName))
                {
                    where = where.WhereEquals(typeInfo.CodeNameColumn, reference.CodeName);
                }
                if (reference.Site != null)
                {
                    where = where.WhereEquals(typeInfo.SiteIDColumn, SiteInfoProvider.GetSiteID(reference.Site.CodeName));
                }

                var whereCondition = where.ToString(true);

                var filterConditionCache = translationHelper as IFilterConditionCache;
                if (filterConditionCache == null || !filterConditionCache.TryGet(filteredObjectType, whereCondition, out included))
                {
                    included = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(filteredObjectType, configuration, where).Any();

                    filterConditionCache?.Add(filteredObjectType, whereCondition, included);
                }
            }

            return included;
        }


        /// <summary>
        /// Returns object type that should be used for filtering. The method takes into account composite object types.
        /// </summary>
        internal static string GetObjectTypeForFiltering(string objectType)
        {
            var repositoryObjectType = GetRepositoryObjectType(objectType);
            var compositeObjectType = ObjectTypeManager.GetTypeInfo(repositoryObjectType).CompositeObjectType;

            return String.IsNullOrEmpty(compositeObjectType) ? repositoryObjectType : compositeObjectType;
        }


        /// <summary>
        /// Returns ID of info object if set, otherwise it returns ID loaded through translation helper.
        /// This method ensures that correct object ID is used, because i.e. object inserted by bulk insert doesn't have set ID and must be reloaded to get the real ID. 
        /// </summary>
        private static int GetObjectId(BaseInfo info, ContinuousIntegrationTranslationHelper translationHelper)
        {
            var generalizedInfo = info.Generalized;
            var objectId = generalizedInfo.ObjectID;

            if (objectId > 0)
            {
                return objectId;
            }

            var parameters = new TranslationParameters(info.TypeInfo.ObjectType)
            {
                Guid = generalizedInfo.ObjectGUID,
                CodeName = generalizedInfo.ObjectCodeName,
                ParentId = generalizedInfo.ObjectParentID,
                GroupId = generalizedInfo.ObjectGroupID,
                SiteName = generalizedInfo.ObjectSiteName
            };

            return translationHelper.GetIDWithFallback(parameters, generalizedInfo.ObjectSiteID);
        }

        
        /// <summary>
        /// Returns filter condition of given object type.
        /// </summary>
        /// <param name="objectType">Object type which condition will be returned</param>
        /// <param name="configuration">Repository configuration</param>
        internal static IWhereCondition GetObjectTypeFilterCondition(string objectType, FileSystemRepositoryConfiguration configuration)
        {
            var repositoryObjectType = GetRepositoryObjectType(objectType);

            var conditions = configuration.ObjectTypesFilterConditions;
            if (configuration.ObjectTypesFilterConditions == null)
            {
                return null;
            }

            IWhereCondition objectTypeCondition;
            if (conditions.TryGetValue(repositoryObjectType, out objectTypeCondition))
            {
                return objectTypeCondition;
            }

            return null;
        }
    }
}

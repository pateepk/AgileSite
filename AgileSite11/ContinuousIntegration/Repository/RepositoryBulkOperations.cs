using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Contains methods allowing caller to perform bulk operation on CI repository.
    /// </summary>
    public static class RepositoryBulkOperations
    {
        /// <summary>
        /// Deletes all objects of the given type matching the where condition from CI repository.
        /// </summary>
        /// <remarks>Also processes and deletes all objects that depend on objects specified by passed where condition.</remarks>
        /// <param name="typeInfo">Type info of the object type to delete from CI repository</param>
        /// <param name="where">Where condition selecting objects to delete</param>
        /// <param name="deleteDependentObjects">Indicates whether to delete all dependent objects of actually deleted objects</param>
        /// <returns>IDs collection of deleted objects</returns>
        public static ICollection<int> DeleteObjects(ObjectTypeInfo typeInfo, IWhereCondition where, bool deleteDependentObjects = true)
        {
            // Object type is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (!ObjectsCanBeStored())
            {
                return new int[] { };
            }
            var configuration = FileSystemRepositoryManager.GetInstance().CachedConfiguration;
            var translationHelper = new ContinuousIntegrationTranslationHelper();

            // Get objects matching the where condition
            var infos = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(typeInfo.ObjectType, configuration, where);

            ICollection<int> infoIds;

            // Check if type is supported in current configuration
            if (RepositoryConfigurationEvaluator.IsObjectTypeIncluded(typeInfo.ObjectType, configuration))
            {
                infoIds = new List<int>();
                infos = infos.Select(info =>
                {
                    infoIds.Add(info.Generalized.ObjectID);
                    return info;
                });

                // Delete all stored information based on the where condition
                FileSystemRepositoryManager.Delete(typeInfo, infos, translationHelper);
            }
            else
            {
                infoIds = infos.Select(info => info.Generalized.ObjectID).ToList();
            }

            if (deleteDependentObjects)
            {
                // Delete all dependent objects from CI repository
                var depWhereConditions = ContinuousIntegrationHelper.GetDependentObjectsWhereConditions(typeInfo, where);
                ContinuousIntegrationHelper.ProcessDependentObjects(depWhereConditions, (depTypeInfo, depObjects) => FileSystemRepositoryManager.Delete(depTypeInfo, depObjects, translationHelper));
            }

            return infoIds;
        }


        /// <summary>
        /// Stores all objects of the given type matching the where condition to CI repository.
        /// </summary>
        /// <remarks>Also processes and stores all objects that depend on objects specified by passed where condition.</remarks>
        /// <param name="typeInfo">Type info of the object type to store</param>
        /// <param name="where">Where condition selecting objects to store</param>
        /// <param name="storeDependentObjects">Indicates whether to store all dependent objects of actually stored objects</param>
        /// <param name="rebuildConfiguration">
        /// Indicates whether current CI configuration should be rebuilt prior the store operation.
        /// Pass true value only when any change of <see cref="DataClassInfo"/> leads to store operation.
        /// </param>
        public static void StoreObjects(ObjectTypeInfo typeInfo, IWhereCondition where, bool storeDependentObjects = true, bool rebuildConfiguration = false)
        {
            // Object type is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (!ObjectsCanBeStored())
            {
                return;
            }

            if (rebuildConfiguration)
            {
                FileSystemRepositoryManager.GetInstance().RebuildConfiguration();
            }

            var configuration = FileSystemRepositoryManager.GetInstance().CachedConfiguration;
            var translationHelper = new ContinuousIntegrationTranslationHelper();
            var objects = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(typeInfo.ObjectType, configuration, where);

            StoreObjectsInternal(typeInfo, objects, translationHelper);

            if (storeDependentObjects)
            {
                // Store all dependent objects to CI repository
                var depWhereConditions = ContinuousIntegrationHelper.GetDependentObjectsWhereConditions(typeInfo, where);
                ContinuousIntegrationHelper.ProcessDependentObjects(depWhereConditions, (depTypeInfo, depObjects) => FileSystemRepositoryManager.Store(depTypeInfo, depObjects, translationHelper));
            }
        }


        /// <summary>
        /// Stores given objects to CI repository.
        /// </summary>
        /// <remarks>The method does not process objects that depend on given objects.</remarks>
        /// <param name="typeInfo">Type info object that describes objects in <paramref name="objects"/> collection.</param>
        /// <param name="objects">Objects to store. All objects must be of same type as <paramref name="typeInfo"/>.</param>
        public static void StoreObjects(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> objects)
        {
            // Object type is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (!ObjectsCanBeStored())
            {
                return;
            }

            StoreObjectsInternal(typeInfo, objects);
        }


        /// <summary>
        /// Determines whether objects can be to the CI repository.
        /// </summary>
        private static bool ObjectsCanBeStored()
        {
            return ContinuousIntegrationHelper.IsObjectSerializationEnabled && !RepositoryActionContext.CurrentIsRestoreOperationRunning;
        }


        /// <summary>
        /// Stores given objects to the CI repository.
        /// </summary>
        /// <param name="typeInfo">Type info object that describes objects in <paramref name="objects"/> collection.</param>
        /// <param name="objects">Objects to store. All objects must be of same type as <paramref name="typeInfo"/>.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        private static void StoreObjectsInternal(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> objects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            // Check if type is supported in current configuration
            var configuration = FileSystemRepositoryManager.GetInstance().CachedConfiguration;
            if (RepositoryConfigurationEvaluator.IsObjectTypeIncluded(typeInfo.ObjectType, configuration))
            {
                // Store objects
                FileSystemRepositoryManager.Store(typeInfo, objects, translationHelper ?? new ContinuousIntegrationTranslationHelper());
            }
        }
    }
}

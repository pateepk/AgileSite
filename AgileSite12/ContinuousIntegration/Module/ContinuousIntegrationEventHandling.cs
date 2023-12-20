using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Handles object change events for the purpose of CI.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public static class ContinuousIntegrationEventHandling
    {
        /// <summary>
        /// Column name containing form defition
        /// </summary>
        private const string FORM_DEFINITION_COLUMN_NAME = "ClassFormDefinition";

        private static readonly Lazy<HashSet<string>> ObjectTypesWithBindingRemovalOptimalization = new Lazy<HashSet<string>>(() => ObjectTypeManager.AllObjectTypes
            .Select(i => ObjectTypeManager.GetTypeInfo(i))
            .Where(i => i.IsBinding)
            .Select(i => i.ParentObjectType)
            .ToHashSetCollection());


        #region "Fields"

        private static int lastBulkDeleteExceptionHash;
        private static int lastBulkInsertExceptionHash;
        private static int lastBulkUpdateExceptionHash;
        private static int lastDeleteExceptionHash;
        private static int lastStoreExceptionHash;

        #endregion


        #region "Event handling methods"

        /// <summary>
        /// Handles necessary actions after inserting a <see cref="BaseInfo"/>.
        /// </summary>
        /// <param name="baseInfo">Object being inserted.</param>
        public static void BaseInfoInsertAfter(BaseInfo baseInfo)
        {
            if (!RepositoryActionContext.CurrentIsRestoreOperationRunning && ContinuousIntegrationHelper.IsObjectSerializationEnabled)
            {
                StoreBaseInfo(baseInfo);
            }
        }


        /// <summary>
        /// Handles necessary actions before updating a <see cref="BaseInfo"/>.
        /// </summary>
        /// <param name="baseInfo">Object being updated.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public static void BaseInfoUpdateBefore(BaseInfo baseInfo, CMSEventArgs eventArgs)
        {
            // Do not store the object if CI is disabled or if there is no relevant change.
            if (RepositoryActionContext.CurrentIsRestoreOperationRunning || !ContinuousIntegrationHelper.IsObjectSerializationEnabled || !HasObjectSignificantlyChanged(baseInfo))
            {
                return;
            }

            var objectId = baseInfo.Generalized.ObjectID;
            var handleDependencies = DependencyChanged(baseInfo) && (objectId > 0);

            Action<BaseInfo, Action<ObjectTypeInfo, ICollection<BaseInfo>>> processDependentObjects = (info, action) =>
            {
                if (handleDependencies)
                {
                    var typeInfo = info.TypeInfo;
                    var dependencyWhere = typeInfo.CreateWhereCondition().WhereEquals(typeInfo.IDColumn, baseInfo.Generalized.ObjectID);
                    var depWhereConditions = ContinuousIntegrationHelper.GetDependentObjectsWhereConditions(typeInfo, dependencyWhere);
                    ContinuousIntegrationHelper.ProcessDependentObjects(depWhereConditions, action);
                }
            };

            var translationHelper = new ContinuousIntegrationTranslationHelper();

            // Remove old files
            baseInfo.ExecuteWithOriginalData(() =>
            {
                DeleteBaseInfo(baseInfo);

                // Delete dependent objects from CI repository
                processDependentObjects(baseInfo, (depTypeInfo, depObjects) => DeleteBaseInfoObjects(depTypeInfo, depObjects, translationHelper));
            });

            // Cached translations from 'update before' may change during update, do not use them in 'update after'
            translationHelper.Clear();

            // Store new files
            eventArgs.CallWhenFinished(() =>
            {
                if (!ContinuousIntegrationHelper.IsObjectSerializationEnabled)
                {
                    return;
                }

                StoreBaseInfo(baseInfo);
                // Store dependent objects
                processDependentObjects(baseInfo, (depTypeInfo, depObjects) => StoreBaseInfoObjects(depTypeInfo, depObjects, translationHelper));
            });
        }


        /// <summary>
        /// Handles necessary actions after deleting a <see cref="BaseInfo"/>.
        /// </summary>
        /// <param name="baseInfo">Object being deleted.</param>
        public static void BaseInfoDeleteAfter(BaseInfo baseInfo)
        {
            if (!RepositoryActionContext.CurrentIsRestoreOperationRunning && ContinuousIntegrationHelper.IsObjectSerializationEnabled)
            {
                DeleteBaseInfo(baseInfo);
            }
        }


        /// <summary>
        /// Handles necessary actions before bulk update, see <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.UpdateData(string, QueryDataParameters, string)"/>
        /// and <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.UpdateData(IWhereCondition, IEnumerable{KeyValuePair{string, object}}, bool)"/>.
        /// </summary>
        /// <param name="eventArgs">Bulk update event arguments.</param>
        internal static void BulkUpdateBefore(BulkUpdateEventArgs eventArgs)
        {
            var serializationTypeInfo = GetTypeInfoForSerialization(eventArgs.TypeInfo);

            // Object type is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (!ContinuousIntegrationHelper.IsSupportedForObjectType(serializationTypeInfo) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            try
            {
                var configuration = FileSystemRepositoryManager.GetInstance().CachedConfiguration;

                if (serializationTypeInfo.IsBinding)
                {
                    var infos = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(serializationTypeInfo.ObjectType, configuration, eventArgs.WhereCondition);
                    eventArgs.CallWhenFinished(() => FileSystemRepositoryManager.GetInstance().RunStoreJob(serializationTypeInfo, infos, configuration));
                }
                else
                {
                    // Identifiers of objects affected by the bulk update
                    ICollection<int> ids;
                    bool storeDependentObjects = true;

                    if ((eventArgs.ChangedColumns != null) && eventArgs.ChangedColumns.Any() && !GetDependencyColumns(eventArgs.TypeInfo).Intersect(eventArgs.ChangedColumns, StringComparer.InvariantCultureIgnoreCase).Any())
                    {
                        // No dependency column is affected by the update - only changed objects themselves will be stored after the update. There is no need to delete already stored files before the update.
                        var infos = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(serializationTypeInfo.ObjectType, configuration, eventArgs.WhereCondition);
                        ids = infos.Select(info => info.Generalized.ObjectID).ToList();
                        storeDependentObjects = false;
                    }
                    else
                    {
                        // At least one dependency column can be affected by the update - all stored files of changed objects and their dependent objects are deleted before the update and stored again after the update
                        ids = RepositoryBulkOperations.DeleteObjects(serializationTypeInfo, eventArgs.WhereCondition);
                    }

                    IWhereCondition updatedWhere = serializationTypeInfo.CreateWhereCondition().WhereIn(serializationTypeInfo.IDColumn, ids);
                    eventArgs.CallWhenFinished(() => RepositoryBulkOperations.StoreObjects(serializationTypeInfo, updatedWhere, storeDependentObjects));
                }

                Volatile.Write(ref lastBulkUpdateExceptionHash, 0);
            }
            catch (LicenseException ex)
            {
                // Disable CI to ensure that exception is not thrown anymore
                DisableContinuousIntegration();

                EventLogProvider.LogException("ContinuousIntegration", "BULKUPDATE", ex, 0, "Object tracking was disabled and needs to be manually enabled to recover CI functionality.");
            }
            catch (RepositoryConfigurationException ex)
            {
                // Throw the exception only the first time it occurs
                int exceptionHash = GetExceptionHash(ex);
                int previousHash = Interlocked.Exchange(ref lastBulkUpdateExceptionHash, exceptionHash);
                if (previousHash != exceptionHash)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "BULKUPDATE", ex, 0, "Bulk update changes could not be stored into repository. The repository state may not be up to date. This exception is logged only once per application run. After resolving the issue, store all objects to the repository again.");
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContinuousIntegration", "BULKUPDATE", ex, 0, "Bulk update changes could not be stored into repository. The repository state may not be up to date. To fix this, you can try to perform storing of all objects to the repository.");
            }
        }


        /// <summary>
        /// Handles necessary actions before bulk delete, see <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.BulkDelete(IWhereCondition, BulkDeleteSettings)"/>
        /// </summary>
        /// <param name="eventArgs">Bulk delete event arguments.</param>
        internal static void BulkDeleteBefore(BulkDeleteEventArgs eventArgs)
        {
            var serializationTypeInfo = GetTypeInfoForSerialization(eventArgs.TypeInfo);

            if (!ContinuousIntegrationHelper.IsSupportedForObjectType(serializationTypeInfo) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            try
            {
                RepositoryBulkOperations.DeleteObjects(serializationTypeInfo, eventArgs.WhereCondition);

                Volatile.Write(ref lastBulkDeleteExceptionHash, 0);
            }
            catch (LicenseException ex)
            {
                // Disable CI to ensure that exception is not thrown anymore
                DisableContinuousIntegration();

                EventLogProvider.LogException("ContinuousIntegration", "BULKDELETE", ex, 0, "Object tracking was disabled and needs to be manually enabled to recover CI functionality.");
            }
            catch (RepositoryConfigurationException ex)
            {
                // Throw the exception only the first time it occurs
                int exceptionHash = GetExceptionHash(ex);
                int previousHash = Interlocked.Exchange(ref lastBulkDeleteExceptionHash, exceptionHash);
                if (previousHash != exceptionHash)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "BULKDELETE", ex, 0, "Bulk delete changes could not be stored into repository. The repository state may not be up to date. This exception is logged only once per application run. After resolving the issue, store all objects to the repository again.");
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContinuousIntegration", "BULKDELETE", ex, 0, "Bulk delete changes could not be stored into repository. The repository state may not be up to date. To fix this, you can try to perform storing of all objects to the repository.");
            }
        }


        /// <summary>
        /// Handles necessary actions after bulk insert, see <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.BulkInsertInfos(IEnumerable{BaseInfo})"/> or
        ///  <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.BulkInsertInfos(IEnumerable{TInfo},BulkInsertSettings)"/>"/>.
        /// </summary>
        /// <param name="eventArgs">Bulk insert event arguments.</param>
        internal static void BulkInsertAfter(BulkInsertEventArgs eventArgs)
        {
            var serializationTypeInfo = GetTypeInfoForSerialization(eventArgs.TypeInfo);

            if (!ContinuousIntegrationHelper.IsSupportedForObjectType(serializationTypeInfo) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            try
            {
                if (eventArgs.InsertedObjects != null)
                {
                    RepositoryBulkOperations.StoreObjects(serializationTypeInfo, eventArgs.InsertedObjects.Select(x => x as BaseInfo));

                    Volatile.Write(ref lastBulkInsertExceptionHash, 0);
                }
            }
            catch (LicenseException ex)
            {
                // Disable CI to ensure that exception is not thrown anymore
                DisableContinuousIntegration();

                EventLogProvider.LogException("ContinuousIntegration", "BULKINSERT", ex, 0, "Object tracking was disabled and needs to be manually enabled to recover CI functionality.");
            }
            catch (RepositoryConfigurationException ex)
            {
                // Throw the exception only the first time it occurs
                int exceptionHash = GetExceptionHash(ex);
                int previousHash = Interlocked.Exchange(ref lastBulkInsertExceptionHash, exceptionHash);
                if (previousHash != exceptionHash)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "BULKINSERT", ex, 0, "Bulk insert changes could not be stored into repository. The repository state may not be up to date. This exception is logged only once per application run. After resolving the issue, store all objects to the repository again.");
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContinuousIntegration", "BULKINSERT", ex, 0, "Bulk insert changes could not be stored into repository. The repository state may not be up to date. To fix this, you can try to perform storing of all objects to the repository.");
            }
        }


        /// <summary>
        /// Removes dependent bindings as single operation for given <see cref="BaseInfo"/> object.
        /// </summary>
        internal static void RemoveBindingsWithOptimalization(BaseInfo baseInfo)
        {
            if (!IsOptimalizationRequired(baseInfo.TypeInfo))
            {
                return;
            }

            foreach (var dependency in GetDependencies(baseInfo))
            {
                var items = new ObjectDependenciesRemover().GetDependencyInfoObjects(dependency);
                DeleteBaseInfoObjects(ObjectTypeManager.GetTypeInfo(dependency.ObjectType), items.OfType<BaseInfo>());
            }
        }


        /// <summary>
        /// Rebuilds CI configuration and optionally logs a web farm task to propagate the change.
        /// </summary>
        /// <param name="logTask">A value indicating whether to log a web farm task.</param>
        internal static void RebuildConfiguration(bool logTask = true)
        {
            FileSystemRepositoryManager.GetInstance().RebuildConfiguration();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new RebuildConfigurationWebFarmTask());
            }
        }

        #endregion


        #region "File system repository methods"

        /// <summary>
        /// Stores info object to CI repository if configuration allows it.
        /// </summary>
        internal static void StoreBaseInfo(BaseInfo info)
        {
            StoreBaseInfoObjects(info.TypeInfo, new[] { info });
        }


        /// <summary>
        /// Stores info objects to CI repository if configuration allows it.
        /// </summary>
        /// <param name="typeInfo">Type info object describing objects in <paramref name="infoObjects"/> collection.</param>
        /// <param name="infoObjects">Collection of info objects to store.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        internal static void StoreBaseInfoObjects(ObjectTypeInfo typeInfo, ICollection<BaseInfo> infoObjects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            // EventLogInfo is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (String.Equals(typeInfo.ObjectType, EventLogInfo.OBJECT_TYPE, StringComparison.InvariantCultureIgnoreCase) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            try
            {
                FileSystemRepositoryManager.Store(typeInfo, infoObjects, translationHelper);

                // Reset last exception
                Volatile.Write(ref lastStoreExceptionHash, 0);
            }
            catch (LicenseException ex)
            {
                // Disable CI to ensure that exception is not thrown anymore
                DisableContinuousIntegration();

                EventLogProvider.LogException("ContinuousIntegration", "STORE", ex, 0, "Object tracking was disabled and needs to be manually enabled to recover CI functionality.");
            }
            catch (RepositoryConfigurationException ex)
            {
                // Throw the exception only the first time it occurs
                int exceptionHash = GetExceptionHash(ex);
                int previousHash = Interlocked.Exchange(ref lastStoreExceptionHash, exceptionHash);
                if (previousHash != exceptionHash)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "STORE", ex, 0, "Object change could not be stored in the repository. The repository state may not be up to date. This exception is logged only once per application run. After resolving the issue, store all objects to the repository again.");
                }
            }
            catch (Exception ex)
            {
                // Check if there is only one object or many
                var firstTwo = infoObjects.Take(2).ToList();
                if (firstTwo.Count != 1)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "DELETE", ex, 0, "Changes of multiple objects could not be stored in the repository. The repository state may not be up to date. To fix this, you can try to perform storing of all objects to the repository.");
                }
                else
                {
                    EventLogProvider.LogException("ContinuousIntegration", "STORE", ex, 0, "Object change could not be stored in the repository. The repository state may not be up to date. To fix this, you can try to resave the object changes. Object: " + Environment.NewLine + firstTwo[0]);
                }
            }
        }


        /// <summary>
        /// Deletes info object from CI repository if configuration allows it.
        /// </summary>
        internal static void DeleteBaseInfo(BaseInfo info)
        {
            DeleteBaseInfoObjects(info.TypeInfo, new[] { info });
        }


        /// <summary>
        /// Deletes info objects from CI repository if configuration allows it.
        /// </summary>
        /// <param name="typeInfo">Type info object describing objects in <paramref name="infoObjects"/> collection.</param>
        /// <param name="infoObjects">Collection of info objects to delete.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        internal static void DeleteBaseInfoObjects(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            // EventLogInfo is not supported for CI and this early check prevents infinite loop of event logging which may occur under certain conditions
            if (String.Equals(typeInfo.ObjectType, EventLogInfo.OBJECT_TYPE, StringComparison.InvariantCultureIgnoreCase) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            try
            {
                FileSystemRepositoryManager.Delete(typeInfo, infoObjects, translationHelper);

                // Reset last exception
                Volatile.Write(ref lastDeleteExceptionHash, 0);
            }
            catch (LicenseException ex)
            {
                // Disable CI to ensure that exception is not thrown anymore
                DisableContinuousIntegration();

                EventLogProvider.LogException("ContinuousIntegration", "DELETE", ex, 0, "Object tracking was disabled and needs to be manually enabled to recover CI functionality.");
            }
            catch (RepositoryConfigurationException ex)
            {
                // Throw the exception only the first time it occurs
                int exceptionHash = GetExceptionHash(ex);
                int previousHash = Interlocked.Exchange(ref lastDeleteExceptionHash, exceptionHash);
                if (previousHash != exceptionHash)
                {
                    EventLogProvider.LogException("ContinuousIntegration", "DELETE", ex, 0, "Object could not be deleted from the repository. The repository state may not be up to date. This exception is logged only once per application run. After resolving the issue, store all objects to the repository again.");
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContinuousIntegration", "DELETE", ex, 0, "Objects could not be deleted from the repository. The repository state may not be up to date. To fix this, you can try to perform storing of all objects to the repository.");
            }
        }

        #endregion


        #region "Helper methods"

        private static int GetExceptionHash(Exception exception)
        {
            unchecked
            {
                int hash = exception.Message.GetHashCode() + exception.StackTrace.GetHashCode();

                return (exception.InnerException != null) ? hash + (17 * GetExceptionHash(exception.InnerException)) : hash;
            }
        }


        /// <summary>
        /// Returns true when passed object has changed an important column, that is used in CI translations of object references.
        /// </summary>
        private static bool DependencyChanged(BaseInfo baseInfo)
        {
            if (baseInfo == null)
            {
                return false;
            }

            var dependencyColumns = GetDependencyColumns(baseInfo.TypeInfo);
            return baseInfo.ChangedColumns().Intersect(dependencyColumns, StringComparer.InvariantCultureIgnoreCase).Any();
        }


        /// <summary>
        /// Returns the collection of all dependency columns of given type info.
        /// </summary>
        private static IEnumerable<string> GetDependencyColumns(ObjectTypeInfo typeInfo)
        {
            var dependencyColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                typeInfo.CodeNameColumn,
                typeInfo.SiteIDColumn,
                typeInfo.GroupIDColumn,
                typeInfo.GUIDColumn,
                typeInfo.ObjectPathColumn,
                typeInfo.PossibleParentIDColumn,
            };

            if (typeInfo.TypeCondition != null)
            {
                dependencyColumns.AddRangeToSet(typeInfo.TypeCondition.ConditionColumns);
            }

            dependencyColumns.AddRangeToSet(typeInfo.ContinuousIntegrationSettings.DependencyColumns);
            dependencyColumns.AddRangeToSet(typeInfo.ContinuousIntegrationSettings.ObjectFileNameFields);

            return dependencyColumns.Where(column => column != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
        }


        private static void DisableContinuousIntegration()
        {
            SettingsKeyInfoProvider.SetGlobalValue(ContinuousIntegrationHelper.ENABLED_CI_KEY, false, false);
        }


        /// <summary>
        /// Gets the correct type info which should handle the object serialization
        /// </summary>
        /// <param name="ti">Current object type info</param>
        private static ObjectTypeInfo GetTypeInfoForSerialization(ObjectTypeInfo ti)
        {
            // When type info has wrapper, pass the serialization responsibility to that wrapper
            while (ti.CompositeObjectType != null)
            {
                ti = ObjectTypeManager.GetTypeInfo(ti.CompositeObjectType);

                if (ti.ContinuousIntegrationSettings.Enabled)
                {
                    break;
                }
            }

            var repositoryObjectType = RepositoryConfigurationEvaluator.GetRepositoryObjectType(ti.ObjectType);
            if (!repositoryObjectType.Equals(ti.ObjectType, StringComparison.InvariantCultureIgnoreCase))
            {
                ti = ObjectTypeManager.GetTypeInfo(repositoryObjectType);
            }

            return ti;
        }


        /// <summary>
        /// Returns true when passed object has changed any column that affects stored file.
        /// </summary>
        /// <remarks>
        /// Method returns true also if <see cref="BaseInfo.ChangedColumns"/> returns empty collection (to ensure that the object
        /// is stored in case that <see cref="BaseInfo.ChangedColumns"/> does not track changes)
        /// </remarks>
        private static bool HasObjectSignificantlyChanged(BaseInfo info)
        {
            var changed = info.ChangedColumns();
            if (changed.Any())
            {
                var irrelevantColumns = info.TypeInfo.SerializationSettings.ExcludedFieldNames.Except(GetDependencyColumns(info.TypeInfo));
                if (!changed.Except(irrelevantColumns).Any())
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true when passed object has changed any column containing form definition.
        /// </summary>
        internal static bool HasObjectFormDefinitionChanged(IAdvancedDataContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            return container.ItemChanged(FORM_DEFINITION_COLUMN_NAME);
        }


        /// <summary>
        /// Returns true when form definition change in passed object requires CI repository refresh.
        /// This includes: addition or removal of a field, field's name or data type change.
        /// </summary>
        public static bool RequiresFormDefinitionChangeRepositoryRefresh(IAdvancedDataContainer info)
        {
            if (!HasObjectFormDefinitionChanged(info))
            {
                return false;
            }

            const string FORM_DEFINITION_COLUMN = "ClassFormDefinition";

            var newFormDefinition = info.GetValue(FORM_DEFINITION_COLUMN)?.ToString();
            var originalFormDefinition = info.GetOriginalValue(FORM_DEFINITION_COLUMN)?.ToString();

            var newDefinition = new DataDefinition(newFormDefinition);
            var originalDefinition = new DataDefinition(originalFormDefinition);

            // Get fields with database representation
            var newFields = newDefinition.ItemsList.OfType<FieldInfo>().Where(f => !f.IsDummyField && !f.External).ToList();
            var originalFields = originalDefinition.ItemsList.OfType<FieldInfo>().Where(f => !f.IsDummyField && !f.External).ToList();
            var originalFieldsByGuid = originalFields.ToDictionary(field => field.Guid);
            var originalFieldsByName = originalFields.ToDictionary(field => field.Name, StringComparer.OrdinalIgnoreCase);

            if (originalFields.Count != newFields.Count)
            {
                // Field was added or removed -> refresh CI repository
                return true;
            }

            foreach (var field in newFields)
            {
                // Try get field from original definition based on GUID or name
                FieldInfo originalField;
                if (!originalFieldsByGuid.TryGetValue(field.Guid, out originalField))
                {
                    originalFieldsByName.TryGetValue(field.Name, out originalField);
                }

                if (!string.Equals(field.Name, originalField?.Name, StringComparison.OrdinalIgnoreCase) || !string.Equals(field.DataType, originalField?.DataType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true when passed object has changed column containing code name.
        /// </summary>
        public static bool HasObjectCodeNameChanged(IInfo container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            return container.ItemChanged(container.TypeInfo.CodeNameColumn);
        }


        private static bool IsOptimalizationRequired(ObjectTypeInfo objectType)
        {
            return ObjectTypesWithBindingRemovalOptimalization.Value.Contains(objectType.ObjectType)
                || ObjectTypesWithBindingRemovalOptimalization.Value.Contains(objectType.OriginalObjectType);
        }


        private static IEnumerable<RemoveDependencyInfo> GetDependencies(BaseInfo info)
        {
            return new ObjectDependenciesRemover()
                .GetRemoveDependenciesList(info, false)
                .Where(i => i.UseApi && i.RemoveWithApiSettings.Operation == RemoveDependencyOperationEnum.Delete);
        }

        #endregion
    }
}

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.CustomTables
{
    /// <summary>
    /// Continuous integration handlers for custom tables
    /// </summary>
    internal static class ContinuousIntegrationHandlers
    {
        /// <summary>
        /// Registers continuous integration handlers.
        /// </summary>
        internal static void Init()
        {
            var customTableEvents = CustomTableInfo.TYPEINFOCUSTOMTABLE.Events;

            // Refreshes CI repository since the provider object is removed after the delete
            customTableEvents.Delete.Before += RemoveItemsFromRepository;
            customTableEvents.Update.Before += UpdateItemsInRepository;
        }


        /// <summary>
        /// Updates repository depending on change
        /// </summary>
        private static void UpdateItemsInRepository(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            if (classInfo == null)
            {
                return;
            }

            var definitionChanged = ContinuousIntegrationEventHandling.RequiresFormDefinitionChangeRepositoryRefresh(classInfo);
            var classNameChanged = ContinuousIntegrationEventHandling.HasObjectCodeNameChanged(classInfo);
            if (!definitionChanged && !classNameChanged)
            {
                return;
            }

            HandleDataClassUpdate(classInfo, e, classNameChanged);
        }


        /// <summary>
        /// Handles both form definition and classname change for given <see cref="DataClassInfo" />
        /// Repository for original name is deleted directly, but the store is performed during finishing the event. 
        /// </summary>
        /// <param name="classInfo">Info from before event</param>
        /// <param name="e">Event arguments</param>
        /// <param name="classNameChanged">true when classname is changed</param>
        private static void HandleDataClassUpdate(DataClassInfo classInfo, ObjectEventArgs e, bool classNameChanged)
        {
            var originalName = (string)classInfo.GetOriginalValue(classInfo.TypeInfo.CodeNameColumn);
            var originalTypeInfo = CustomTableItemProvider.GetTypeInfo(originalName);
            if (!originalTypeInfo.SupportsContinuousIntegration())
            {
                return;
            }

            // ClassName is changed, original CI repository folder must be deleted when original type info exists and configuration has to be rebuilt once event is finished.
            if (classNameChanged)
            {
                RepositoryBulkOperations.DeleteObjects(originalTypeInfo, new WhereCondition());
            }

            e.CallWhenFinished(() =>
            {
                var customTableClassInfo = CustomTableItemProvider.GetTypeInfo(classInfo.ClassName);
                if (customTableClassInfo.SupportsContinuousIntegration())
                {
                    // All objects are stored, empty condition is sufficient. No need to delete objects.
                    RepositoryBulkOperations.StoreObjects(customTableClassInfo, new WhereCondition(), rebuildConfiguration: true);
                }
            });
        }


        /// <summary>
        /// Removes custom table items from repository.
        /// </summary>
        private static void RemoveItemsFromRepository(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var customTableClassInfo = CustomTableItemProvider.GetTypeInfo(classInfo.ClassName);

            if (!customTableClassInfo.SupportsContinuousIntegration())
            {
                return;
            }

            // All objects needs to be deleted, empty condition.
            RepositoryBulkOperations.DeleteObjects(customTableClassInfo, new WhereCondition());
        }
    }
}
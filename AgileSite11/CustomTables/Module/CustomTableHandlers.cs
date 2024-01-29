using CMS.DataEngine;

namespace CMS.CustomTables
{
    /// <summary>
    /// Provides handlers for custom tables
    /// </summary>
    internal static class CustomTableHandlers
    {
        /// <summary>
        /// Initializes the document handlers
        /// </summary>
        public static void Init()
        {
            var customTableEvents = CustomTableInfo.TYPEINFOCUSTOMTABLE.Events;

            // To create WF task after inserting new dynamic TypeInfo with possible cached zombie TI on another farms due to updating class name value
            customTableEvents.Insert.After += Insert;
            customTableEvents.Update.After += Update;
            customTableEvents.Delete.After += Delete;
            
            // Clear data before the delete since the provider object is removed after the delete
            // Refreshes CI repository since the provider object is removed after the delete
            // It needs to be used to properly delete the cached data
            customTableEvents.Delete.Before += BeforeDelete;
        }


        /// <summary>
        /// Handles additional actions when updating a table
        /// </summary>
        private static void Update(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = CustomTableItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            CustomTableItemProvider.InvalidateProvider(objectType);
            // Update cached TypeInfo
            CustomTableItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }


        /// <summary>
        /// Handles additional actions when inserting a table
        /// </summary>
        private static void Insert(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = CustomTableItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            CustomTableItemProvider.InvalidateProvider(objectType);
            // Remove cached TypeInfo
            CustomTableItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }


        /// <summary>
        /// Handles additional actions when deleting a table
        /// </summary>
        private static void Delete(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = CustomTableItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            CustomTableItemProvider.InvalidateProvider(objectType);
            // Remove cached TypeInfo
            CustomTableItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }


        /// <summary>
        /// Handles before delete event on custom tables
        /// </summary>
        private static void BeforeDelete(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;

            ClearHashtables(className);
        }


        /// <summary>
        /// Clears data in hashtables
        /// </summary>
        private static void ClearHashtables(string className)
        {
            var objectType = CustomTableItemProvider.GetObjectType(className);

            // Clear data in hashtables since the data structure can be changed
            ProviderHelper.ClearHashtables(objectType, true);
        }
    }
}
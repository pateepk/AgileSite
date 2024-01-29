using CMS.DataEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides handlers for document types.
    /// </summary>
    internal static class DocumentTypeHandlers
    {
        /// <summary>
        /// Initializes the document handlers
        /// </summary>
        public static void Init()
        {
            var docTypeEvents = DocumentTypeInfo.TYPEINFODOCUMENTTYPE.Events;
            // To create WF task after inserting new dynamic TypeInfo with possible cached zombie TI on another farms due to updating class name value
            docTypeEvents.Insert.After += RemoveClassInformation;
            docTypeEvents.Update.After += UpdateClassInformation;
            docTypeEvents.Delete.After += RemoveClassInformation;
        }


        private static void UpdateClassInformation(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;

            ClearClassRelatedCachedObjects(classInfo);
        }


        private static void RemoveClassInformation(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;

            ClearClassRelatedCachedObjects(classInfo);
        }


        private static void ClearClassRelatedCachedObjects(DataClassInfo classInfo)
        {
            var className = classInfo.ClassName;
            var fieldstObjectType = DocumentFieldsInfoProvider.GetObjectType(className);
            var nodeObjectType = TreeNodeProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            DocumentFieldsInfoProvider.InvalidateProvider(fieldstObjectType);
            TreeNodeProvider.InvalidateProvider(nodeObjectType);

            // Remove cached TypeInfo
            DocumentFieldsInfoProvider.InvalidateTypeInfo(className, true);
            TreeNodeProvider.InvalidateTypeInfo(className, true);

            // Clear the resolved class names
            DocumentTypeHelper.ClearClassNames(true);

            // Remove read only objects
            ModuleManager.RemoveReadOnlyObject(fieldstObjectType, true);
            ModuleManager.RemoveReadOnlyObject(nodeObjectType, true);
        }
    }
}
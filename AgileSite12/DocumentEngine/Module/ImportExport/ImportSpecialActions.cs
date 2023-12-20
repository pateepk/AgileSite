using System;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        private const string URL_PREFIX_MVC = "MVC:";

        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetEmptyObject.Execute += GetImportEmptyObject_Execute;
            SpecialActionsEvents.ImportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;
            SpecialActionsEvents.GetObjectTypeFolder.Execute += GetObjectTypeFolder_Execute;
            SpecialActionsEvents.ProcessObjectData.After += ProcessObjectData_After;

            SpecialActionsEvents.PrepareDataStructure.Execute += PrepareDataStructure_Execute;

            ImportExportEvents.ImportObject.After += ImportObject_After;
            ImportExportEvents.ImportObject.Before += ImportObject_Before;
            DocumentImportExportEvents.ImportDocument.Before += ImportDocument_Before;
        }


        private static void ProcessObjectData_After(object sender, ImportEventArgs e)
        {
            var parameters = e.Parameters;
            var infoObj = e.Object;
            var settings = e.Settings;

            using (new ImportSpecialCaseContext(settings))
            {
                if (parameters.ObjectProcessType != ProcessObjectEnum.All)
                {
                    return;
                }

                var classInfo = infoObj as DocumentTypeInfo;
                if (classInfo == null)
                {
                    return;
                }

                int originalInheritedClassId = parameters.GetValue("originalInheritedClassId", 0);

                UpdateClassInheritance(classInfo, originalInheritedClassId);
            }
        }


        private static void UpdateClassInheritance(DocumentTypeInfo importedClassInfo, int originalInheritedClassId)
        {
            int inherits = importedClassInfo.ClassInheritsFromClassID;

            // Ensure (update) the inheritance
            if (inherits > 0)
            {
                // Update the inherited fields
                var parentClass = DataClassInfoProvider.GetDataClassInfo(inherits);
                if (parentClass != null)
                {
                    FormHelper.UpdateInheritedClass(parentClass, importedClassInfo);
                }
            }
            else if (originalInheritedClassId > 0)
            {
                // Remove the inherited fields
                FormHelper.RemoveInheritance(importedClassInfo, false);
            }
            else
            {
                FormHelper.UpdateInheritedClasses(importedClassInfo);
            }
        }


        private static void PrepareDataStructure_Execute(object sender, ImportGetDataEventArgs e)
        {
            if (e.Settings.Version.StartsWith("8", StringComparison.Ordinal) && (e.ObjectType == AclInfo.OBJECT_TYPE))
            {
                // Ensure column ACLOwnerNodeID for backward compatibility
                e.Data.Tables[0].Columns.Add("ACLOwnerNodeID", typeof(int));
            }
        }


        private static void GetObjectTypeFolder_Execute(object sender, GetObjectTypeFolderArgs e)
        {
            if (IsDocumentObjectType(e.ObjectType))
            {
                e.Folder = ImportExportHelper.DOCUMENTS_FOLDER;
            }
        }


        /// <summary>
        /// Returns true if object type is from documents data.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        private static bool IsDocumentObjectType(string objectType)
        {
            switch (objectType)
            {
                case AttachmentHistoryInfo.OBJECT_TYPE:
                case VersionHistoryInfo.OBJECT_TYPE:
                case AttachmentInfo.OBJECT_TYPE:
                case TreeNode.OBJECT_TYPE:
                case AclInfo.OBJECT_TYPE:
                case RelationshipInfo.OBJECT_TYPE:
                case RelationshipInfo.OBJECT_TYPE_ADHOC:
                case DocumentCategoryInfo.OBJECT_TYPE:
                case DocumentAliasInfo.OBJECT_TYPE:
                    return true;
            }

            return false;
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            var objectType = e.ObjectType;
            var siteId = e.Settings.SiteId;
            if (objectType == PersonalizationInfo.OBJECT_TYPE)
            {
                e.Where.Where("PersonalizationDocumentID IN (SELECT DocumentID FROM " + SystemViewNames.View_CMS_Tree_Joined + " WHERE NodeSiteID = " + siteId + ")");
                e.CombineWhereCondition = false;
            }
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == PersonalizationInfo.OBJECT_TYPE_DASHBOARD)
            {
                var settings = e.Settings;
                e.Select = ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_USER_DASHBOARDS), false);
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_USER_SITE_DASHBOARDS), false);
            }
            else if (objectType == PersonalizationInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_USER_PERSONALIZATIONS), false);
            }
        }


        private static void LoadDefaultSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;

            switch (objectType)
            {
                // Documents - No selection, special case
                case TreeNode.OBJECT_TYPE:
                    ProcessObjectEnum importDocuments = GetDocumentImportSettings(settings);
                    settings.SetObjectsProcessType(importDocuments, objectType, true);

                    // Cancel default selection
                    e.Select = false;
                    break;
            }
        }


        private static ProcessObjectEnum GetDocumentImportSettings(SiteImportSettings settings)
        {
            // If no pages are included in import package then don't process this object type.
            // If TreeNode would be marked to process in this case then site root won't be created.
            if (!settings.IsIncluded(TreeNode.OBJECT_TYPE))
            {
                return ProcessObjectEnum.None;
            }

            return settings.ImportType != ImportTypeEnum.None ? ProcessObjectEnum.All : ProcessObjectEnum.None;
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType == TreeNode.OBJECT_TYPE)
            {
                // No object representation for import
                infoObj = InfoHelper.EmptyInfo;
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }


        private static void ImportObject_After(object sender, ImportEventArgs e)
        {
            // Synchronize the site bindings with the resource - in case the page type already exists, we need to use the preexisting one because the new one might not contain ID
            DocumentTypeHelper.SynchronizeSiteBindingsWithResource(e.Parameters.ExistingObject as DocumentTypeInfo ?? e.Object as DocumentTypeInfo);
        }


        private static void ImportObject_Before(object sender, ImportEventArgs e)
        {
            if (e.Settings.IsLowerVersion("12.0") && IsMvcAlias(e.Object))
            {
                e.Cancel();
            }
        }


        private static bool IsMvcAlias(BaseInfo infoObj)
        {
            if (infoObj.TypeInfo.ObjectType == DocumentAliasInfo.OBJECT_TYPE)
            {
                return ((DocumentAliasInfo)infoObj).AliasURLPath.StartsWith(URL_PREFIX_MVC, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }


        private static void ImportDocument_Before(object sender, DocumentImportEventArgs e)
        {
            if (e.Settings.IsLowerVersion("12.0"))
            {
                var doc = e.Document;

                ClearDocumentMvcDocumentUrlPath(doc);
            }
        }


        private static void ClearDocumentMvcDocumentUrlPath(TreeNode doc)
        {
            if (doc.DocumentUrlPath.StartsWith(URL_PREFIX_MVC, StringComparison.InvariantCultureIgnoreCase))
            {
                doc.DocumentUrlPath = null;
            }
        }

        #endregion
    }
}
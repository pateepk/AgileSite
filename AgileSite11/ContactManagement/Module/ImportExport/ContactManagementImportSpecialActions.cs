using System;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ContactManagementImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
            ImportExportEvents.ImportBindingType.Before += ImportBindingType_Before;
        }


        private static void ImportBindingType_Before(object sender, ImportObjectTypeArgs e)
        {
            if (e.Settings.IsLowerVersion("10.0") && (e.ObjectType == RolePermissionInfo.OBJECT_TYPE))
            {
                ExcludeCMRolePermissionBindings(e.TranslationHelper, e.Parameters.Data);
            }
        }


        /// <summary>
        /// Do not import permission to read or modify configuration for contact management.
        /// Permissions with ReadConfiguration and ModifyConfiguration codeName have slightly different meaning in version 10.
        /// </summary>
        /// <param name="th">Translation helper object</param>
        /// <param name="data">Import data</param>
        private static void ExcludeCMRolePermissionBindings(TranslationHelper th, DataSet data)
        {
            var permissionNames = new[] { "ReadConfiguration", "ModifyConfiguration" };

            // Get the contact management resource
            var cmResource = th.GetRecord(new TranslationParameters(PredefinedObjectType.RESOURCE)
            {
                CodeName = ModuleName.CONTACTMANAGEMENT
            });
            var cmResourceID = DataHelper.GetIntValue(cmResource, TranslationHelper.RECORD_ID_COLUMN);

            // Select permission IDs which should not be granted for the imported role
            var permissionIDs = from row in th.TranslationTable.AsEnumerable()
                                where
                                    row.Field<string>(TranslationHelper.RECORD_OBJECT_TYPE_COLUMN).Equals(PermissionNameInfo.OBJECT_TYPE_RESOURCE, StringComparison.CurrentCultureIgnoreCase) &&
                                    (row.Field<int>(TranslationHelper.RECORD_PARENT_ID_COLUMN) == cmResourceID) &&
                                    permissionNames.Contains(row.Field<string>(TranslationHelper.RECORD_CODE_NAME_COLUMN), StringComparer.InvariantCultureIgnoreCase)
                                select row.Field<int>(TranslationHelper.RECORD_ID_COLUMN);

            // Exclude RolePermission bindings
            var rolePermission = data.Tables["CMS_RolePermission"];

            if (!DataHelper.IsEmpty(rolePermission) || !permissionIDs.Any())
            {
                var rolePermissinDelete = rolePermission.AsEnumerable().Where(r => permissionIDs.Contains(r.Field<int>("PermissionID")));
                rolePermissinDelete.ToList().ForEach(r => r.Delete());
                rolePermission.AcceptChanges();
            }
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;
            var parameters = e.Parameters;

            using (new ImportSpecialCaseContext(settings))
            {
                if (objectType == ScoreInfo.OBJECT_TYPE)
                {
                    if (!parameters.SkipObjectUpdate && (parameters.ObjectProcessType == ProcessObjectEnum.All))
                    {
                        infoObj.SetValue("ScoreStatus", 2);
                        infoObj.SetValue("ScoreScheduledTaskID", null);
                    }
                }
            }
        }

        #endregion
    }
}
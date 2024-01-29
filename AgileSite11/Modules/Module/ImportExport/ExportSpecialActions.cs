using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Modules
{
    /// <summary>
    /// Handles special actions during the export process.
    /// </summary>
    internal static class ExportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;
            SpecialActionsEvents.ExportEnsureAutomaticSelection.Execute += ExportEnsureAutomaticSelection_Execute;

            ImportExportEvents.GetExportData.After += GetExportData_After;
        }


        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            if (!e.SelectionOnly && (e.ObjectType == PredefinedObjectType.RESOURCE))
            {
                var resourceTable = e.Data.Tables[ImportExportHelper.GetSafeObjectTypeName(PredefinedObjectType.RESOURCE)];
                if (ValidationHelper.GetBoolean(e.Settings.GetSettings(ImportExportHelper.SETTINGS_SEAL_EXPORTED_MODULES), true))
                {
                    SealModules(resourceTable);
                }

                var uiElementTable = e.Data.Tables[ImportExportHelper.GetSafeObjectTypeName(UIElementInfo.OBJECT_TYPE)];
                SealUIElements(uiElementTable, resourceTable);
            }
        }
        

        private static void ExportEnsureAutomaticSelection_Execute(object sender, ExportLoadSelectionArgs e)
        {
            if (e.ObjectType == DataClassInfo.OBJECT_TYPE)
            {
                e.DependencyIDColumn = "ClassResourceID";
                e.DependencyObjectType = ResourceInfo.OBJECT_TYPE;
                e.DependencyObject = ModuleManager.GetReadOnlyObject(ResourceInfo.OBJECT_TYPE);
            }
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            if (e.ObjectType == DataClassInfo.OBJECT_TYPE)
            {
                // Only export classes of those modules which are being actively developed on current instance
                e.Where.Where("ClassResourceID IN (SELECT ResourceID FROM CMS_Resource WHERE (ISNULL(ResourceIsInDevelopment, 0) = 1) OR ResourceName='" + ModuleName.CUSTOMSYSTEM + "')");
            }
        }


        /// <summary>
        /// Seals the exported modules.
        /// </summary>
        /// <param name="modules">Table with modules.</param>
        private static void SealModules(DataTable modules)
        {
            var resourceTypeInfo = ObjectTypeManager.GetTypeInfo(PredefinedObjectType.RESOURCE);
            if (resourceTypeInfo == null)
            {
                return;
            }

            foreach (DataRow row in modules.Rows)
            {
                var resourceName = (string)row[resourceTypeInfo.CodeNameColumn];
                row["ResourceIsInDevelopment"] = IsCustomSystemModule(resourceName);
            }
        }


        /// <summary>
        /// Seals exported UI elements based on module's 'is in development' flag.
        /// </summary>
        /// <param name="uiElements">Table with UI elements.</param>
        /// <param name="modules">Table with modules.</param>
        private static void SealUIElements(DataTable uiElements, DataTable modules)
        {
            if (uiElements == null)
            {
                return;
            }

            var resourceIsInDevelopmentDictionary = modules != null ? GetResourceIsInDevelopmentDictionary(modules) : new Dictionary<int, bool>();

            foreach (DataRow uiElementRow in uiElements.Rows)
            {
                int uiElementResourceId = (int)uiElementRow["ElementResourceID"];
                uiElementRow["ElementIsCustom"] = resourceIsInDevelopmentDictionary.ContainsKey(uiElementResourceId) && resourceIsInDevelopmentDictionary[uiElementResourceId];
            }
        }


        private static Dictionary<int, bool> GetResourceIsInDevelopmentDictionary(DataTable modules)
        {
            return modules.Rows.Cast<DataRow>().ToDictionary(resourceRow => (int)resourceRow["ResourceID"], GetResourceIsInDevelopmentValue);
        }


        private static bool GetResourceIsInDevelopmentValue(DataRow resourceRow)
        {
            var resourceIsInDevelopment = resourceRow["ResourceIsInDevelopment"];
            if (resourceIsInDevelopment == DBNull.Value)
            {
                var resourceName = ValidationHelper.GetString(resourceRow["ResourceName"], String.Empty);
                return IsCustomSystemModule(resourceName);
            }

            return ValidationHelper.GetBoolean(resourceIsInDevelopment, false);
        }


        private static bool IsCustomSystemModule(string resourceName)
        {
            return resourceName.Equals(ModuleName.CUSTOMSYSTEM, StringComparison.InvariantCultureIgnoreCase);
        }
        
        #endregion
    }
}
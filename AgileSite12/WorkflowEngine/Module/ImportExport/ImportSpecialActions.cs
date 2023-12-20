using System;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ImportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
            SpecialActionsEvents.GetObjectTypeFolder.Execute += GetObjectTypeFolder_Execute;
        }


        private static void GetObjectTypeFolder_Execute(object sender, GetObjectTypeFolderArgs e)
        {
            if (e.ObjectType == WorkflowHistoryInfo.OBJECT_TYPE)
            {
                e.Folder = ImportExportHelper.DOCUMENTS_FOLDER;
            }
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == WorkflowScopeInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_WORKFLOW_SCOPES), false);
            }
        }

        private static void LoadDefaultSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            var siteObject = e.SiteObject;
            var settings = e.Settings;

            switch (objectType)
            {
                // Do not preselect for older version
                case WorkflowActionInfo.OBJECT_TYPE_AUTOMATION:
                    DefaultSelectionParameters parameters = new DefaultSelectionParameters()
                    {
                        ObjectType = objectType,
                        SiteObjects = siteObject
                    };

                    if (!settings.IsOlderHotfixVersion || (settings.ImportType == ImportTypeEnum.None) || (settings.ImportType == ImportTypeEnum.AllForced))
                    {
                        parameters.ImportType = ImportTypeEnum.Default;
                    }
                    else
                    {
                        parameters.ImportType = ImportTypeEnum.New;
                    }
                    settings.LoadDefaultSelection(parameters);

                    // Cancel default selection
                    e.Select = false;
                    break;
            }
        }

        #endregion
    }
}
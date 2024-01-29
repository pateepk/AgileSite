using System;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WorkflowEngine;

namespace CMS.Automation
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
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
        }
        

        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == ObjectWorkflowTriggerInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_WORKFLOW_TRIGGERS), false);
            }
        }

        #endregion
    }
}
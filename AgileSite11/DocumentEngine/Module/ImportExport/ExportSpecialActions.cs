using System;

using CMS.CMSImportExport;

namespace CMS.DocumentEngine
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
            SpecialActionsEvents.ExportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
        }


        private static void LoadDefaultSelection_Execute(object sender, ExportLoadSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;

            switch (objectType)
            {
                // Documents - No selection, special case
                case TreeNode.OBJECT_TYPE:
                    ProcessObjectEnum exportDocuments = (settings.ExportType != ExportTypeEnum.None) ? ProcessObjectEnum.All : ProcessObjectEnum.None;
                    settings.SetObjectsProcessType(exportDocuments, objectType, true);

                    // Cancel default selection
                    e.Select = false;
                    break;
            }
        }

        #endregion
    }
}
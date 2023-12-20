using System;

using CMS.Base;
using CMS.CMSImportExport;

namespace CMS.WorkflowEngine
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
            SpecialActionsEvents.GetSelectionWhereCondition.Execute += ExportSelection_Execute;
        }


        private static void ExportSelection_Execute(object sender, ExportSelectionArgs e)
        {
            var objectType = e.ObjectType;

            if (e.Select)
            {
                // Add additional where condition due to depending objects
                switch (objectType.ToLowerCSafe())
                {
                    // Special cases
                    case WorkflowActionInfo.OBJECT_TYPE:
                        e.IncludeDependingObjects = false;
                        break;
                }
            }
        }

        #endregion
    }
}
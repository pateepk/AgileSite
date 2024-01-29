using System;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;

namespace CMS.Taxonomy
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
            var settings = e.Settings;
            var objectType = e.ObjectType;

            // Don't preselect object which are not used/allowed by site
            string appendGlobalObjectsKeyName = null;
            switch (objectType.ToLowerCSafe())
            {
                case CategoryInfo.OBJECT_TYPE:
                    appendGlobalObjectsKeyName = "CMSAllowGlobalCategories";
                    break;
            }

            if (!string.IsNullOrEmpty(appendGlobalObjectsKeyName))
            {
                e.Select &= SettingsKeyInfoProvider.GetBoolValue(settings.SiteName + "." + appendGlobalObjectsKeyName);
            }

            if (e.Select)
            {
                // Add additional where condition due to depending objects
                switch (objectType.ToLowerCSafe())
                {
                    // Special cases
                    case CategoryInfo.OBJECT_TYPE:
                        e.IncludeDependingObjects = false;
                        break;
                }
            }
        }

        #endregion
    }
}
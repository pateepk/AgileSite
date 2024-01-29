using CMS.CMSImportExport;
using CMS.Helpers;

namespace CMS.PortalEngine
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
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == PageTemplateScopeInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                e.ProcessDependency = !ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_PAGETEMPLATE_SCOPES), false);
            }

            var dependencyObject = e.DependencyObject;

            // Ad-hoc templates need separate TypeInfo
            if ((dependencyObject != null) && (dependencyObject.TypeInfo.ObjectType == PageTemplateInfo.OBJECT_TYPE))
            {
                e.DependencyIsSiteObject = false;
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
                case WebPartInfo.OBJECT_TYPE:
                case WidgetInfo.OBJECT_TYPE:
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
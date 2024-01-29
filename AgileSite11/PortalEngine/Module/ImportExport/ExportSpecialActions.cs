using System;
using System.Collections;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
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
            SpecialActionsEvents.ExportEnsureAutomaticSelection.Execute += EnsureAutomaticSelection_Execute;
            SpecialActionsEvents.GetSelectionWhereCondition.Execute += ExportSelection_Execute;
            ImportExportEvents.ExportObjects.After += ExportObjects_After;
            ImportExportEvents.Export.After += Export_After;
            ImportExportEvents.GetExportData.After += GetExportData_After;
        }


        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;
            var ds = e.Data;

            if ((objectType == PredefinedObjectType.DOCUMENTTYPE) || (objectType == PredefinedObjectType.CUSTOMTABLECLASS))
            {
                // ### Special cases - additionally modify the data for web template export
                if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_WEBTEMPLATE_EXPORT), false))
                {
                    return;
                }

                if (DataHelper.DataSourceIsEmpty(ds))
                {
                    return;
                }

                GeneralizedInfo transformation = ModuleManager.GetReadOnlyObject(TransformationInfo.OBJECT_TYPE);

                var dt = ObjectHelper.GetTable(ds, transformation);
                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    DataHelper.ChangeValuesToNull(dt, "TransformationPreferredDocument", null);
                }
            }
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var settings = e.Settings;

            // Copy ASPX templates folder
            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_COPY_ASPX_TEMPLATES_FOLDER), true))
            {
                CopyASPXTemplatesFolder(settings);
            }
        }


        /// <summary>
        /// Copy ASPX templates folder.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void CopyASPXTemplatesFolder(SiteExportSettings settings)
        {
            // Copy the folder only if some page template is selected
            if (settings.IsObjectTypeProcessed(PageTemplateInfo.OBJECT_TYPE, false, ProcessObjectEnum.Default))
            {
                try
                {
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.CopyingASPXTemplatesFolder", "Copying ASPX templates folder"));

                    string sourcePath = DirectoryHelper.CombinePath(settings.WebsitePath, ImportExportHelper.SRC_TEMPLATES_FOLDER, settings.SiteName);
                    string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.GetSafeObjectTypeName(PageTemplateInfo.OBJECT_TYPE), ImportExportHelper.SRC_TEMPLATES_FOLDER, settings.SiteName);

                    // Copy page templates folder
                    CopyASPXTemplates(sourcePath, targetPath, settings.ExcludedNames, settings.WebsitePath);
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ExportProvider.LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorCopyingASPXTemplatesFolder", "Error copying ASPX templates folder."), ex);
                    throw;
                }
            }
        }


        /// <summary>
        /// Copy ASPX templates folder.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="excludedNames">Excluded names</param>
        /// <param name="websitePath">Web site path</param>
        private static void CopyASPXTemplates(string sourcePath, string targetPath, string[] excludedNames, string websitePath)
        {
            if (Directory.Exists(sourcePath))
            {
                DirectoryInfo sourceFolder = DirectoryInfo.New(sourcePath);
                // Create the directory if not exists
                if (!(Directory.Exists(targetPath)))
                {
                    DirectoryHelper.CreateDirectory(targetPath);
                }

                // Copy sub folders
                foreach (string subFolderName in ExportProvider.GetDirectories(sourceFolder))
                {
                    bool copyFolder = true;

                    // Decide if the folder should be copied
                    if (excludedNames != null)
                    {
                        foreach (string exName in excludedNames)
                        {
                            copyFolder &= !subFolderName.ToLowerCSafe().StartsWithCSafe(exName.ToLowerCSafe());
                        }
                    }

                    // Copy folder
                    if (copyFolder)
                    {
                        // None file has been processed in the folder
                        CopyASPXTemplates(sourcePath + @"\" + subFolderName, targetPath + @"\" + subFolderName, excludedNames, websitePath);
                    }
                }

                // Copy files
                Hashtable files = ExportProvider.GetFiles(sourceFolder);
                if (files != null)
                {
                    foreach (string sourceFileName in files.Keys)
                    {
                        string sourceFilePath = sourceFileName.ToLowerCSafe();
                        bool copyFile = true; //!sourceFilePath.EndsWithCSafe(".aspx") && !sourceFilePath.EndsWithCSafe(".aspx.cs") && !sourceFilePath.EndsWithCSafe(".aspx.vb");

                        // Decide if the file should be copied
                        if (excludedNames != null)
                        {
                            foreach (string exName in excludedNames)
                            {
                                copyFile &= !sourceFilePath.StartsWithCSafe(exName.ToLowerCSafe());
                            }
                        }

                        // Copy file
                        if (copyFile)
                        {
                            string targetFilePath = targetPath + @"\" + sourceFileName;
                            ExportProvider.CopyFile((string)files[sourceFileName], targetFilePath, websitePath);
                        }
                    }
                }
            }
        }


        private static void ExportObjects_After(object sender, ExportEventArgs e)
        {
            var settings = e.Settings;
            var data = e.Data;
            var objectType = e.ObjectType;

            switch (objectType)
            {
                case LayoutInfo.OBJECT_TYPE:
                    // Page layout
                    ExportProvider.CopyComponentFile(settings, data, objectType, "Layouts");
                    break;

                case PageTemplateInfo.OBJECT_TYPE:
                    // Page template
                    ExportProvider.CopyComponentFile(settings, data, objectType, "PageTemplates");
                    break;

                case WebPartInfo.OBJECT_TYPE:
                    // Web part + layouts
                    ExportProvider.CopyComponentFile(settings, data, objectType, "WebParts");
                    break;

                case DeviceProfileInfo.OBJECT_TYPE:
                    // Device profiles
                    ExportProvider.CopyComponentFile(settings, data, objectType, "DeviceProfile");
                    break;
            }
        }


        private static void ExportSelection_Execute(object sender, ExportSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;

            if (e.Select)
            {
                #region "Special cases"

                // Add additional where condition for special cases
                switch (objectType.ToLowerCSafe())
                {
                    // Page template - only from site documents 
                    case PageTemplateInfo.OBJECT_TYPE:
                        {
                            string templateWhere = String.Format("PageTemplateID IN (SELECT DISTINCT DocumentPageTemplateID FROM " + SystemViewNames.View_CMS_Tree_Joined + " WHERE NodeSiteID = {0}) OR PageTemplateID IN (SELECT DISTINCT NodeTemplateID FROM View_CMS_Tree_Joined WHERE NodeSiteID = {0})", settings.SiteId);
                            e.Where.Or().Where(templateWhere);
                        }
                        break;

                    // CSS Stylesheet - only from site documents or site settings
                    case CssStylesheetInfo.OBJECT_TYPE:
                        {
                            string cssWhere = String.Format("StylesheetID IN ({0}, {1}) OR StylesheetID IN (SELECT DISTINCT DocumentStylesheetID FROM " + SystemViewNames.View_CMS_Tree_Joined + " WHERE NodeSiteID = {2})", settings.SiteInfo.SiteDefaultEditorStylesheet, settings.SiteInfo.SiteDefaultStylesheetID, settings.SiteId);
                            e.Where.Or().Where(cssWhere);
                        }
                        break;
                }

                #endregion

                // Add additional where condition due to depending objects
                switch (objectType.ToLowerCSafe())
                {
                    // Special cases
                    case PredefinedObjectType.REPORT:
                    case WebPartInfo.OBJECT_TYPE:
                    case WidgetInfo.OBJECT_TYPE:
                        e.IncludeDependingObjects = false;
                        break;
                }
            }
        }


        private static void EnsureAutomaticSelection_Execute(object sender, ExportLoadSelectionArgs e)
        {
            var dependencyObject = e.DependencyObject;

            // Ad-hoc templates need separate TypeInfo
            if ((dependencyObject != null) && (dependencyObject.TypeInfo.ObjectType == PageTemplateInfo.OBJECT_TYPE))
            {
                e.DependencyIsSiteObject = false;
            }
        }

        #endregion
    }
}
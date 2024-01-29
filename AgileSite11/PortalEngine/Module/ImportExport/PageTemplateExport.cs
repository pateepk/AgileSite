using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Page template export process.
    /// </summary>
    internal static class PageTemplateExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
            ImportExportEvents.SingleExportSelection.Execute += SingleExportSelection_Execute;
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == PageTemplateInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                if (!settings.CopyFiles)
                {
                    return;
                }

                // Get info object
                var infoObj = ModuleManager.GetReadOnlyObject(objectType);
                
                var data = e.Data;

                var dt = ObjectHelper.GetTable(data, infoObj);

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(dt))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                string webSitePath = settings.WebsitePath;
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
                string targetFilesPath = targetPath;
                string sourcePath = webSitePath;

                targetPath = DirectoryHelper.CombinePath(targetFilesPath, safeObjectType) + "\\";

                // Log process
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                foreach (DataRow dr in dt.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Prepare the data
                    PageTemplateTypeEnum templateType = DataHelper.GetStringValue(dr, "PageTemplateType").ToEnum<PageTemplateTypeEnum>();
                    bool isAspx = (templateType == PageTemplateTypeEnum.Aspx) || (templateType == PageTemplateTypeEnum.AspxPortal);
                    string objectFileName = DataHelper.GetStringValue(dr, "PageTemplateFile");

                    // Only for ASPX templates
                    if (isAspx)
                    {
                        string targetObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, PageTemplateInfoProvider.TemplatesDirectory, targetPath);
                        string sourceObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, PageTemplateInfoProvider.TemplatesDirectory, sourcePath);

                        try
                        {
                            // Source files
                            ExportProvider.CopySourceFiles(sourceObjectPath, targetObjectPath, webSitePath);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }


        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            var infoObj = e.InfoObject;
            var settings = e.Settings;

            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == PageTemplateInfo.OBJECT_TYPE)
            {
                // Select also all template scopes
                settings.SetObjectsProcessType(ProcessObjectEnum.Selected, PageTemplateScopeInfo.OBJECT_TYPE, false);
            }
        }

        #endregion
    }
}
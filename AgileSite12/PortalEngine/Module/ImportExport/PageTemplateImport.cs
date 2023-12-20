using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Page template import process.
    /// </summary>
    internal static class PageTemplateImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;

            SpecialActionsEvents.PrepareDataStructure.Execute += PrepareDataStructure_Execute;
            SpecialActionsEvents.ProcessAdditionalActions.After += ProcessAdditionalActions_After;
        }


        private static void ProcessAdditionalActions_After(object sender, ImportBaseEventArgs e)
        {
            var settings = e.Settings;

            if (settings.ImportSite)
            {
                // Add ASPX templates folder
                AddASPXTemplatesFolder(settings);
            }
        }


        private static void PrepareDataStructure_Execute(object sender, ImportGetDataEventArgs e)
        {
            var selectionOnly = e.SelectionOnly;
            var data = e.Data;
            var objectType = e.ObjectType;

            if (objectType == PageTemplateInfo.OBJECT_TYPE)
            {
                if (selectionOnly)
                {
                    data.Tables[0].Columns.Add("PageTemplateIsReusable", typeof(bool));
                }
            }
        }


        static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == PageTemplateInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_PageTemplate"]))
                {
                    return;
                }

                // Add the source files
                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + "\\";
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

                var table = data.Tables["CMS_PageTemplate"];
                
                foreach (DataRow dr in table.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    string objectName = dr["PageTemplateCodeName"].ToString();
                    PageTemplateTypeEnum templateType = DataHelper.GetStringValue(dr, "PageTemplateType").ToEnum<PageTemplateTypeEnum>();
                    bool isAspx = (templateType == PageTemplateTypeEnum.Aspx) || (templateType == PageTemplateTypeEnum.AspxPortal);
                    string objectFileName = DataHelper.GetStringValue(dr, "PageTemplateFile");

                    // Copy files if object is processed
                    if (!settings.IsProcessed(PageTemplateInfo.OBJECT_TYPE, objectName, siteObjects))
                    {
                        continue;
                    }

                    // Only for ASPX templates
                    if (isAspx)
                    {
                        string sourceObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, PageTemplateInfoProvider.TemplatesDirectory, sourcePath);
                        string targetObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, PageTemplateInfoProvider.TemplatesDirectory, targetPath);

                        // Source files
                        ImportProvider.AddSourceFiles(PageTemplateInfo.OBJECT_TYPE, settings, sourceObjectPath, targetObjectPath);
                    }
                }

                // Add the component files
                ImportProvider.AddComponentFiles(objectType, settings, siteObjects, data, "PageTemplates");
            }
        }


        /// <summary>
        /// Add the ASPX templates folder to import file list.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        public static void AddASPXTemplatesFolder(SiteImportSettings settings)
        {
            // Proceed only if site name is set
            if (settings.SiteName != null)
            {
                try
                {
                    string webSitePath = settings.WebsitePath;

                    // Add target path for site (testing and conversion)
                    webSitePath = ImportProvider.GetTargetPath(settings, webSitePath);

                    string targetPath = DirectoryHelper.CombinePath(webSitePath, ImportExportHelper.SRC_TEMPLATES_FOLDER);
                    string sourcePath = settings.TemporaryFilesPath + DirectoryHelper.CombinePath(ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.GetSafeObjectTypeName(PageTemplateInfo.OBJECT_TYPE), ImportExportHelper.SRC_TEMPLATES_FOLDER);

                    // Copy folder
                    if (Directory.Exists(sourcePath))
                    {
                        settings.FileOperations.Add(PageTemplateInfo.OBJECT_TYPE, sourcePath, targetPath, FileOperationEnum.CopyDirectory);
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ImportProvider.LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorPreparingASPXTemplatesFolder", "Error preparing ASPX templates folder"), ex);
                    throw;
                }
            }
        }

        #endregion
    }
}
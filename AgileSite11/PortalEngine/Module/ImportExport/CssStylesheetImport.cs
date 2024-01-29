using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the CSS stylesheet import process.
    /// </summary>
    internal static class CssStylesheetImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
        }


        static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == CssStylesheetInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // Prepare the paths
                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath) + ImportExportHelper.SRC_SKINS_FOLDER + "\\";
                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + Path.Combine(safeObjectType, ImportExportHelper.SRC_SKINS_FOLDER) + "\\";

                var infoObj = ModuleManager.GetReadOnlyObject(objectType);

                var dt = ObjectHelper.GetTable(data, infoObj);
                if (DataHelper.DataSourceIsEmpty(dt))
                {
                    return;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    var nameColumn = infoObj.Generalized.CodeNameColumn;
                    string objectName = dr[nameColumn].ToString();

                    // Copy files if object is processed
                    if (!settings.IsProcessed(objectType, objectName, siteObjects))
                    {
                        continue;
                    }

                    string targetObjectPath = Path.Combine(targetPath, objectName);
                    string sourceObjectPath = Path.Combine(sourcePath, objectName);

                    settings.FileOperations.Add(objectType, sourceObjectPath, targetObjectPath, FileOperationEnum.CopyDirectory);
                }
            }
        }

        #endregion
    }
}
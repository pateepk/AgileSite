using System;
using System.Data;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the CSS stylesheet export process.
    /// </summary>
    internal static class CssStylesheetExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += ExportObjects_After;
        }


        private static void ExportObjects_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == CssStylesheetInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var data = e.Data;

                // Copy files only if required
                if (!settings.CopyFiles)
                {
                    return;
                }

                // Prepare the paths
                string sourcePath = settings.WebsitePath + ImportExportHelper.SRC_SKINS_FOLDER + "\\";
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + Path.Combine(ImportExportHelper.GetSafeObjectTypeName(objectType), ImportExportHelper.SRC_SKINS_FOLDER) + "\\";

                var infoObj = ModuleManager.GetReadOnlyObject(objectType);
                
                var dt = ObjectHelper.GetTable(data, infoObj);
                if (DataHelper.DataSourceIsEmpty(dt))
                {
                    return;
                }

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
                    var nameColumn = infoObj.Generalized.CodeNameColumn;
                    string objectName = dr[nameColumn].ToString();

                    string targetObjectPath = Path.Combine(targetPath, objectName);
                    string sourceObjectPath = Path.Combine(sourcePath, objectName);

                    try
                    {
                        // Skin folder
                        if (Directory.Exists(sourceObjectPath))
                        {
                            ExportProvider.CopyDirectory(sourceObjectPath, targetObjectPath, settings.WebsitePath);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion
    }
}
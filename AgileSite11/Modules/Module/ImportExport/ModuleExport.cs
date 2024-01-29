using System;
using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Handles special actions during the Module export process.
    /// </summary>
    internal static class ModuleExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == ResourceInfo.OBJECT_TYPE)
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
                    // The new module development practice encourages dots in folder names whereas the old one underscores
                    string resourceName = ValidationHelper.GetString(dr["ResourceName"], "");
                    string exportResourceName = resourceName.Replace(".", "_");
                    var resourceNames = new[] { resourceName, exportResourceName };

                    try
                    {
                        // Initialize sub paths
                        List<string> subPaths = new List<string>();

                        subPaths.Add(ImportExportHelper.AppCodeFolder + "\\CMSModules\\");
                        subPaths.Add("App_Data\\CMSModules\\");
                        subPaths.Add("CMSModules\\");

                        // Copy all folders
                        foreach (string subPath in subPaths)
                        {
                            // Export process canceled
                            if (settings.ProcessCanceled)
                            {
                                ExportProvider.ExportCanceled();
                            }

                            foreach (string resName in resourceNames)
                            {
                                string srcPath = sourcePath + subPath + resName;
                                string trgPath = DirectoryHelper.CombinePath(targetPath, exportResourceName, subPath, resName);

                                if (Directory.Exists(srcPath))
                                {
                                    ExportProvider.CopyDirectory(srcPath, trgPath, webSitePath);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        CoreServices.EventLog.LogException("ModuleExport", "EXPORT", ex);
                    }
                }
            }
        }

        #endregion
    }
}
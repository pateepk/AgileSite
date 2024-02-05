using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Web part export process.
    /// </summary>
    internal static class WebPartExport
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
            if (objectType == WebPartInfo.OBJECT_TYPE)
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

                // Ensure loading of all the web parts into the hashtable to optimize access to full physical path through inherited web part
                WebPartInfoProvider.LoadAllWebParts();

                foreach (DataRow dr in dt.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Prepare the data
                    string objectName = dr["WebPartName"].ToString();
                    string objectFileName = dr["WebPartFileName"].ToString();
                    int objectParentId = DataHelper.GetIntValue(dr, "WebPartParentID");
                    string filesFolderName = objectName + "_files";

                    string targetObjectPath = WebPartInfoProvider.GetFullPhysicalPath(objectFileName, objectParentId, targetPath);
                    string targetObjectFolder = Path.GetDirectoryName(targetObjectPath);

                    string sourceObjectPath = WebPartInfoProvider.GetFullPhysicalPath(objectFileName, objectParentId, sourcePath);
                    string sourceObjectFolder = Path.GetDirectoryName(sourceObjectPath);

                    try
                    {
                        // Support files
                        if (Directory.Exists(DirectoryHelper.CombinePath(sourceObjectFolder, filesFolderName)))
                        {
                            ExportProvider.CopyDirectory(DirectoryHelper.CombinePath(sourceObjectFolder, filesFolderName), DirectoryHelper.CombinePath(targetObjectFolder, filesFolderName), webSitePath);
                        }

                        // Source files
                        ExportProvider.CopySourceFiles(sourceObjectPath, targetObjectPath, webSitePath);
                    }
                    catch
                    {
                    }
                }
            }
        }


        private static void SingleExportSelection_Execute(object sender, SingleExportSelectionEventArgs e)
        {
            var infoObj = e.InfoObject;

            if (infoObj.TypeInfo.ObjectType.ToLowerCSafe() == WebPartInfo.OBJECT_TYPE)
            {
                e.SelectedObjects.AddRange(GetInheritedWebParts(infoObj));
            }
        }


        /// <summary>
        /// Get list of dependent web parts for widget object
        /// </summary>
        /// <param name="infoObj">Widget object</param>
        internal static List<string> GetDependentWebPart(GeneralizedInfo infoObj)
        {
            var webParts = new List<string>();
            int webPartId = ValidationHelper.GetInteger(infoObj.GetValue("WidgetWebPartID"), 0);
            if (webPartId != 0)
            {
                var wpi = WebPartInfoProvider.GetWebPartInfo(webPartId);
                if (wpi != null)
                {
                    webParts.Add(wpi.WebPartName);
                    webParts.AddRange(GetInheritedWebParts(wpi));
                }
            }

            return webParts;
        }


        /// <summary>
        /// Get list of inherited web parts for web part object
        /// </summary>
        /// <param name="infoObj">Web part object</param>
        private static List<string> GetInheritedWebParts(GeneralizedInfo infoObj)
        {
            var webParts = new List<string>();
            int parentWebPartId = ValidationHelper.GetInteger(infoObj.GetValue("WebPartParentID"), 0);
            if (parentWebPartId != 0)
            {
                var wpi = WebPartInfoProvider.GetWebPartInfo(parentWebPartId);
                if (wpi != null)
                {
                    webParts.Add(wpi.WebPartName);
                    webParts.AddRange(GetInheritedWebParts(wpi));
                }
            }

            return webParts;
        }

        #endregion
    }
}
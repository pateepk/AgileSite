using System;
using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Web part import process.
    /// </summary>
    internal static class WebPartImport
    {
        /// <summary>
        /// List of removed web part GUIDs.
        /// Web part in this list won't be imported.
        /// </summary>
        private static readonly HashSet<Guid> RemovedWebpartGUIDsInV11 = new HashSet<Guid>
        {
            new Guid("677B2F6B-2F08-4FF1-8F86-091CAF784845"), // Newsletter archive
            new Guid("8fec019c-5e61-4799-8e24-4a154f01ef60"), // Video
            new Guid("2efc51de-12d4-48e6-8ff1-06339b13080a"), // Quick time
            new Guid("8777e045-31a8-43cb-9181-8fabe357857a"), // Search engine results highlighter
            new Guid("03387dc5-4b7a-41d2-b725-5f88cc1bd1bd"), // Search accelerator (for IE8 and higher)
            new Guid("6766B831-4D11-4D7A-B0BF-E0FBF11A6052")  // Flash
        };


        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObject.Before += SkipRemovedWebParts;
            ImportExportEvents.ImportObjectType.After += Import_After;
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == WebPartInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var th = e.TranslationHelper;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_WebPart"]))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + "\\";
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

                var table = data.Tables["CMS_WebPart"];
                foreach (DataRow dr in table.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    string objectName = dr["WebPartName"].ToString();

                    // Check if object is processed
                    if (!settings.IsProcessed(WebPartInfo.OBJECT_TYPE, objectName, siteObjects))
                    {
                        continue;
                    }

                    // Ensure correct ID for parent web part (to be able to resolve file path of the web part)
                    int objectParentId = DataHelper.GetIntValue(dr, "WebPartParentID");
                    if (objectParentId > 0)
                    {
                        int newId = th.GetNewID(WebPartInfo.OBJECT_TYPE, objectParentId, "WebPartName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, "WebPartParentID", ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
                        if (newId > 0)
                        {
                            objectParentId = newId;
                        }
                        else
                        {
                            throw new Exception("[ImportProvider.AddImportFiles]: Cannot translate column 'WebPartParentID', import the dependent web part first.");
                        }
                    }

                    Guid webPartGuid;
                    Guid.TryParse(dr["WebPartGUID"].ToString(), out webPartGuid);

                    if (IsObsoleteWebPartToV11(webPartGuid, e))
                    {
                        // We don't want to import obsolete webpart files. Those webparts were deleted to version 11 and users 
                        // must resolve those issues on their own (import obsolete webparts package, remove webparts usages
                        // or make their own webparts).
                        continue;
                    }

                    string filesFolderName = objectName + "_files";
                    string webPartFileName = dr["WebPartFileName"].ToString();
                    string sourceObjectPath = WebPartInfoProvider.GetFullPhysicalPath(webPartFileName, objectParentId, sourcePath);
                    string sourceObjectFolder = Path.GetDirectoryName(sourceObjectPath);
                    string targetObjectPath = WebPartInfoProvider.GetFullPhysicalPath(webPartFileName, objectParentId, targetPath);
                    string targetObjectFolder = Path.GetDirectoryName(targetObjectPath);

                    // Support files
                    if (Directory.Exists(DirectoryHelper.CombinePath(sourceObjectFolder, filesFolderName)))
                    {
                        settings.FileOperations.Add(WebPartInfo.OBJECT_TYPE, DirectoryHelper.CombinePath(sourceObjectFolder, filesFolderName), DirectoryHelper.CombinePath(targetObjectFolder, filesFolderName), FileOperationEnum.CopyDirectory);
                    }

                    // Source files
                    ImportProvider.AddSourceFiles(WebPartInfo.OBJECT_TYPE, settings, sourceObjectPath, targetObjectPath);
                }

                // Component files
                ImportProvider.AddComponentFiles(WebPartInfo.OBJECT_TYPE, settings, siteObjects, data, "WebParts");
            }
        }


        /// <summary>
        /// Filters-out old web parts which were removed from the product in version 11.
        /// That way these web parts won't be imported back to the database even if an older import package is being imported.
        /// </summary>
        private static void SkipRemovedWebParts(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == WebPartInfo.OBJECT_TYPE)
            {
                if (IsObsoleteWebPartToV11(infoObj.Generalized.ObjectGUID, e))
                {
                    e.Cancel();
                }
            }
        }


        private static bool IsObsoleteWebPartToV11(Guid webPartGuid, ImportBaseEventArgs e)
        {
            return e.Settings.IsLowerVersion("11.0") && RemovedWebpartGUIDsInV11.Contains(webPartGuid);
        }
    }
}
using System.Data;

using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Handles special actions during the Module import process.
    /// </summary>
    public static class ModuleImport
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
            if (objectType == ResourceInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Resource"]))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + "\\";
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

                var table = data.Tables["CMS_Resource"];
                foreach (DataRow dr in table.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    string objectName = dr["ResourceName"].ToString();

                    // Copy files if object is processed
                    if (settings.IsProcessed(ResourceInfo.OBJECT_TYPE, objectName, siteObjects))
                    {
                        string path = sourcePath + objectName.Replace(".", "_");
                        if (Directory.Exists(path))
                        {
                            // Custom module folder
                            settings.FileOperations.Add(ResourceInfo.OBJECT_TYPE, path, targetPath, FileOperationEnum.CopyDirectory);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
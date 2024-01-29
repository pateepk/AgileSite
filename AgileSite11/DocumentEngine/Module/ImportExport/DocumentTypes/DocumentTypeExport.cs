using System.Data;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Document type export process.
    /// </summary>
    internal static class DocumentTypeExport
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
            if (objectType == DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE)
            {
                var settings = e.Settings;
                if (!settings.CopyFiles)
                {
                    return;
                }

                // Get info object
                var infoObj = ModuleManager.GetReadOnlyObject(objectType);
                var data = e.Data;

                var table = ObjectHelper.GetTable(data, infoObj);

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(table))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                string webSitePath = settings.WebsitePath.TrimEnd('\\');
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
                string targetFilesPath = targetPath;
                string sourcePath = webSitePath;

                targetPath = DirectoryHelper.CombinePath(targetFilesPath, safeObjectType);

                // Log process
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                foreach (DataRow dr in table.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Prepare the data
                    string docType = dr["ClassName"].ToString();

                    string imagePath = Path.EnsureBackslashes(AdministrationUrlHelper.GetDocumentTypeIconPath(null, docType).TrimStart(new[] { '~' }));
                    string imageLargePath = Path.EnsureBackslashes(AdministrationUrlHelper.GetDocumentTypeIconPath(null, docType, "48x48").TrimStart(new[] { '~' }));
                    
                    string targetObjectPath = targetPath + imagePath;
                    string sourceObjectPath = sourcePath + imagePath;
                    string targetLargeObjectPath = targetPath + imageLargePath;
                    string sourceLargeObjectPath = sourcePath + imageLargePath;

                    try
                    {
                        ExportProvider.CopyFile(sourceObjectPath, targetObjectPath, settings.WebsitePath);
                        ExportProvider.CopyFile(sourceLargeObjectPath, targetLargeObjectPath, settings.WebsitePath);
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

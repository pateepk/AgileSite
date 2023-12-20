using System.Data;

using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Document type import process.
    /// </summary>
    internal static class DocumentTypeImport
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
            if (objectType == DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Class"]))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + "\\";
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

                var table = data.Tables["CMS_Class"];
                foreach (DataRow dr in table.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    string docType = dr["ClassName"].ToString();

                    // Copy files if object is processed
                    if (settings.IsProcessed(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, docType, siteObjects))
                    {
                        // Prepare the data
                        string imagePath = AdministrationUrlHelper.GetDocumentTypeIconPath(null, docType).TrimStart(new[] { '~', '/' }).Replace('/', '\\');
                        string imageLargePath = AdministrationUrlHelper.GetDocumentTypeIconPath(null, docType, "48x48").TrimStart(new[] { '~', '/' }).Replace('/', '\\');
                        string fileName = docType.Replace(".", "_");

                        string targetObjectPath = DirectoryHelper.CombinePath(targetPath, DirectoryHelper.CombinePath(Path.GetDirectoryName(imagePath), fileName));
                        string sourceObjectPath = DirectoryHelper.CombinePath(sourcePath, DirectoryHelper.CombinePath(Path.GetDirectoryName(imagePath), fileName));
                        string targetLargeObjectPath = DirectoryHelper.CombinePath(targetPath, DirectoryHelper.CombinePath(Path.GetDirectoryName(imageLargePath), fileName));
                        string sourceLargeObjectPath = DirectoryHelper.CombinePath(sourcePath, DirectoryHelper.CombinePath(Path.GetDirectoryName(imageLargePath), fileName));

                        // Document type icon
                        settings.FileOperations.Add(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, sourceObjectPath + ".gif", targetObjectPath + ".gif", FileOperationEnum.CopyFile);
                        settings.FileOperations.Add(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, sourceLargeObjectPath + ".gif", targetLargeObjectPath + ".gif", FileOperationEnum.CopyFile);
                        settings.FileOperations.Add(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, sourceObjectPath + ".png", targetObjectPath + ".png", FileOperationEnum.CopyFile);
                        settings.FileOperations.Add(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, sourceLargeObjectPath + ".png", targetLargeObjectPath + ".png", FileOperationEnum.CopyFile);
                    }
                }
            }
        }

        #endregion
    }
}

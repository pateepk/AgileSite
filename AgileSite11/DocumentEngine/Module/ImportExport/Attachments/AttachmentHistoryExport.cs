using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Attachment history export process.
    /// </summary>
    internal static class AttachmentHistoryExport
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
            if ((objectType == AttachmentHistoryInfo.OBJECT_TYPE) || (objectType == AttachmentHistoryInfo.OBJECT_TYPE_VARIANT))
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
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
                targetPath = DirectoryHelper.CombinePath(targetPath, safeObjectType) + "\\";

                // Log process
                ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                // Process all attachments
                foreach (DataRow dr in dt.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    string guid = ValidationHelper.GetString(dr["AttachmentHistoryGUID"], "");
                    string extension = ValidationHelper.GetString(dr["AttachmentExtension"], "");
                    string fileName = guid + extension;

                    if (!ExportProvider.IsFileExcluded(fileName))
                    {
                        // Get the binary
                        object binary = DataHelper.GetDataRowValue(dr, "AttachmentBinary");
                        byte[] fileBinary = null;
                        if (binary != DBNull.Value)
                        {
                            fileBinary = (byte[])binary;
                        }
                        if (fileBinary == null)
                        {
                            int attachmentHistoryId = ValidationHelper.GetInteger(dr["AttachmentHistoryID"], 0);
                            var attachment = AttachmentHistoryInfoProvider.GetAttachmentHistory(attachmentHistoryId);
                            if (attachment != null)
                            {
                                fileBinary = attachment.AttachmentBinary;
                            }
                        }

                        // Save the file
                        if ((fileBinary != null) && (guid != ""))
                        {
                            try
                            {
                                string filePath = targetPath + DirectoryHelper.CombinePath(guid.Substring(0, 2), fileName);
                                filePath = ImportExportHelper.GetExportFilePath(filePath);

                                // Copy file
                                DirectoryHelper.EnsureDiskPath(filePath, settings.WebsitePath);
                                File.WriteAllBytes(filePath, fileBinary);

                                // Clear the binary
                                DataHelper.SetDataRowValue(dr, "AttachmentBinary", null);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
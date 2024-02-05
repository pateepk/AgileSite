using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Handles special actions during the Avatar export process.
    /// </summary>
    internal static class AvatarExport
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
            if (objectType == AvatarInfo.OBJECT_TYPE)
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
                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);
                    string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";

                    targetPath = DirectoryHelper.CombinePath(targetPath, safeObjectType) + "\\CMSFiles\\";

                    // Log process
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingAvatars", "Copying '{0}' avatars"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                    // Process all avatars
                    foreach (DataRow dr in dt.Rows)
                    {
                        // Export process canceled
                        if (settings.ProcessCanceled)
                        {
                            ExportProvider.ExportCanceled();
                        }

                        string guid = dr["AvatarGUID"].ToString();
                        string extension = ValidationHelper.GetString(dr["AvatarFileExtension"], "");
                        string fileName = guid + extension;

                        if (!ExportProvider.IsFileExcluded(fileName))
                        {
                            // Get the binary
                            object binary = DataHelper.GetDataRowValue(dr, "AvatarBinary");
                            byte[] fileBinary = null;
                            if (binary != DBNull.Value)
                            {
                                fileBinary = (byte[])binary;
                            }
                            if (fileBinary == null)
                            {
                                AvatarInfo avatarInfo = new AvatarInfo(dr);
                                fileBinary = AvatarInfoProvider.GetAvatarFile(avatarInfo);
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
                                    DataHelper.SetDataRowValue(dr, "AvatarBinary", null);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
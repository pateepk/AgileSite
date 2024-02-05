using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Provides methods for updating and getting physical files of <see cref="MediaFileInfo" />. 
    /// Physical file is represented by DataSet with hard coded data structure which is used in SyncManager (Staging, Versioning).
    /// </summary>
    internal class FileBinaryDataSyncManager
    {
        private readonly MediaFileInfo mMediaFileInfo;

        /// <summary>
        /// Initializes a new instance of the FileBinaryDataSyncManager class.
        /// </summary>
        /// <param name="mediaFileInfo">Instance of <see cref="MediaFileInfo" />.</param>
        public FileBinaryDataSyncManager(MediaFileInfo mediaFileInfo)
        {
            mMediaFileInfo = mediaFileInfo;
        }


        /// <summary>
        /// Updates (removes old and saves new) media file physical files.
        /// <param name="filesData">DataSet with physical files data.</param>
        /// </summary>
        public void UpdatePhysicalFiles(DataSet filesData)
        {
            if (DataHelper.DataSourceIsEmpty(filesData))
            {
                return;
            }

            SiteInfo siteInfo = SiteInfoProvider.GetSiteInfo(mMediaFileInfo.FileSiteID);

            // Delete thumbnails, preview and old physical files
            DeleteOldPhysicalFiles(siteInfo);

            // Save data and preview to disk
            ProcessFilesData(siteInfo, filesData);
        }

        /// <summary>
        /// Method returns DataSet with binary data of <see cref="MediaFileInfo" />.
        /// </summary>
        /// <param name="operationType">Supported operations are Synchronization and Versioning.</param>
        /// <param name="binaryData">If true, gets the binary data to the DataSet.</param>
        public DataSet GetPhysicalFiles(OperationTypeEnum operationType, bool binaryData)
        {
            string fileFullPath = MediaFileInfoProvider.GetMediaFilePath(mMediaFileInfo.FileLibraryID, mMediaFileInfo.FilePath);
            string thumbnailFullPath = MediaFileInfoProvider.GetPreviewFilePath(mMediaFileInfo);
            long maxFileSize;
            switch (operationType)
            {
                case OperationTypeEnum.Synchronization:
                    maxFileSize = MediaFileInfoProvider.MaxStagingFileSize;
                    break;

                case OperationTypeEnum.Versioning:
                    maxFileSize = MediaFileInfoProvider.MaxVersioningFileSize;
                    break;

                default:
                    throw new Exception(string.Format("[FileBinaryDataSyncManager.GetPhysicalFiles]: Given operation type '{0}' is not supported.", operationType));
            }

            DataSet binaryDataSet = null;
            try
            {
                binaryDataSet = ObjectHelper.GetBinaryData(mMediaFileInfo, new[,] { { fileFullPath, ObjectHelper.BINARY_DATA_DEFAULT }, { thumbnailFullPath, ObjectHelper.BINARY_DATA_PREVIEW } }, maxFileSize, binaryData);
            }
            catch (Exception ex)
            {
                EventLog.EventLogProvider.LogException("Media file", "LOADBINARYDATA", ex);
            }

            return binaryDataSet;
        }


        private void DeleteOldPhysicalFiles(SiteInfo siteInfo)
        {
            // Delete media file thumbnails
            MediaFileInfoProvider.DeleteMediaFileThumbnails(mMediaFileInfo);

            // Delete preview file with thumbnails
            MediaFileInfoProvider.DeleteMediaFilePreview(siteInfo.SiteName, mMediaFileInfo.FileLibraryID, mMediaFileInfo.FilePath);

            // Remove an old media file and its hidden files if FilePath was changed
            var originalFilepath = mMediaFileInfo.GetOriginalValue("FilePath") as string;
            if (originalFilepath != null && originalFilepath != mMediaFileInfo.FilePath)
            {
                MediaFileInfoProvider.DeleteMediaFile(siteInfo.SiteID, mMediaFileInfo.FileLibraryID, originalFilepath);
            }
        }


        private void ProcessFilesData(SiteInfo siteInfo, DataSet filesData)
        {
            // Get media library
            MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(mMediaFileInfo.FileLibraryID);
            string libraryFolder = library.LibraryFolder;

            foreach (DataRow drFile in filesData.Tables[0].Rows)
            {
                // Get file information
                byte[] binnaryData = GetFileData(drFile);

                string filePath = ValidationHelper.GetString(drFile["FileName"], "");
                string fileType = ValidationHelper.GetString(drFile["FileType"], ObjectHelper.BINARY_DATA_DEFAULT);
                string fileSubPath = Path.GetDirectoryName(mMediaFileInfo.FilePath);
                switch (fileType)
                {
                    case ObjectHelper.BINARY_DATA_DEFAULT:

                        // Save media file
                        MediaFileInfoProvider.SaveFileToDisk(siteInfo.SiteName, libraryFolder, fileSubPath, mMediaFileInfo.FileName, mMediaFileInfo.FileExtension, mMediaFileInfo.FileGUID, binnaryData, false, false);
                        break;

                    case ObjectHelper.BINARY_DATA_PREVIEW:

                        // Save preview file
                        SavePreviewToDisk(siteInfo.SiteName, libraryFolder, fileSubPath, filePath, binnaryData);
                        break;
                }
            }
        }


        private void SavePreviewToDisk(string siteName, string libraryFolder, string fileSubPath, string filePath, byte[] fileData)
        {
            string previewSuffix = MediaLibraryHelper.GetMediaFilePreviewSuffix(siteName);

            if (!string.IsNullOrEmpty(previewSuffix))
            {
                string previewExtension = Path.GetExtension(filePath);
                string previewName = Path.GetFileNameWithoutExtension(MediaLibraryHelper.GetPreviewFileName(mMediaFileInfo.FileName, mMediaFileInfo.FileExtension, previewExtension, siteName, previewSuffix));
                string previewFolder = string.Format("{0}/{1}", 
                    Path.EnsureSlashes(fileSubPath), 
                    MediaLibraryHelper.GetMediaFileHiddenFolder(siteName)
                );

                // Save preview file
                MediaFileInfoProvider.SaveFileToDisk(siteName, libraryFolder, previewFolder, previewName, previewExtension, mMediaFileInfo.FileGUID, fileData, false, false);
            }
        }


        private static byte[] GetFileData(DataRow drFile)
        {
            byte[] fileData = null;
            var data = drFile["FileBinaryData"];
            if (!DataHelper.IsEmpty(data))
            {
                fileData = (byte[])data;
            }
            return fileData;
        }
    }
}

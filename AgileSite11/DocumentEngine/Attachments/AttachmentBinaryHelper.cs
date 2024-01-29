using System;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Helper for getting and managing attachment binary data
    /// </summary>
    public class AttachmentBinaryHelper : AbstractHelper<AttachmentBinaryHelper>
    {
        #region "Variables"

        private static int? mThumbnailQuality;
        private static bool mDeletePhysicalFiles = true;
        private static readonly object ensureFileLock = new object();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Thumbnail quality.
        /// </summary>
        public static bool DeletePhysicalFiles
        {
            get
            {
                return mDeletePhysicalFiles && CMSActionContext.CurrentDeletePhysicalFiles;
            }
            set
            {
                mDeletePhysicalFiles = value;
            }
        }


        /// <summary>
        /// Thumbnail quality.
        /// </summary>
        public static int ThumbnailQuality
        {
            get
            {
                // Use default system quality settings
                if (mThumbnailQuality == null)
                {
                    mThumbnailQuality = ImageHelper.DefaultQuality;
                }

                return mThumbnailQuality.Value;
            }
            set
            {
                mThumbnailQuality = value;
            }
        }

        #endregion


        #region "Public methods - Binary data management"

        /// <summary>
        /// Returns the current settings whether the thumbnails should be generated.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        internal static bool GenerateThumbnails(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSGenerateThumbnails");
        }


        /// <summary>
        /// Resizes image to specified dimensions.
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        /// <param name="width">New width of the attachment.</param>
        /// <param name="height">New height of the attachment.</param>
        /// <param name="maxSideSize">Maximal side size of the attachment.</param>
        internal static void ResizeImageAttachment(IAttachment attachment, int width, int height, int maxSideSize)
        {
            HelperObject.ResizeImageAttachmentInternal(attachment, width, height, maxSideSize);
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        public static byte[] GetAttachmentBinary(DocumentAttachment attachment)
        {
            return HelperObject.GetAttachmentBinaryInternal(attachment);
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="guid">GUID of the file to get.</param>
        /// <param name="siteName">Site name.</param>
        internal static byte[] GetAttachmentBinary(Guid guid, string siteName)
        {
            return HelperObject.GetAttachmentBinaryInternal(guid, siteName);
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="height">Image thumbnail width.</param>
        /// <param name="width">Image thumbnail height.</param>
        internal static byte[] GetImageThumbnailBinary(DocumentAttachment attachment, int width, int height)
        {
            return HelperObject.GetImageThumbnailBinaryFileInternal(attachment, width, height);
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="guid">GUID of the file to get.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="height">Image thumbnail width.</param>
        /// <param name="width">Image thumbnail height.</param>
        internal static byte[] GetImageThumbnailBinary(Guid guid, string siteName, int width, int height)
        {
            var atInfo = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(guid, siteName);

            return GetImageThumbnailBinary((DocumentAttachment)atInfo, width, height);
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesn't exist).
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="maxSideSize">Maximum side size.</param>
        /// <param name="searchThumbnailFile">Indicates if thumbnail should be searched on the disk.</param>
        internal static byte[] GetImageThumbnailBinary(DocumentAttachment attachment, int width, int height, int maxSideSize, bool searchThumbnailFile = true)
        {
            return HelperObject.GetImageThumbnailInternal(attachment, width, height, maxSideSize, searchThumbnailFile);
        }


        /// <summary>
        /// Ensures the file in the file system and returns the path to the file.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        public static string EnsurePhysicalFile(DocumentAttachment attachment)
        {
            return HelperObject.EnsurePhysicalFileInternal(attachment);
        }


        /// <summary>
        /// Ensures the thumbnail file.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="width">File width.</param>
        /// <param name="height">File height.</param>
        /// <param name="maxSideSize">Maximum side size.</param>
        internal static string EnsureThumbnailFile(DocumentAttachment attachment, int width, int height, int maxSideSize)
        {
            return HelperObject.EnsureThumbnailFileInternal(attachment, width, height, maxSideSize);
        }


        /// <summary>
        /// Saves file to the disk.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="fileExtension">File extension.</param>
        /// <param name="fileData">File data (byte[] or Stream).</param>   
        /// <param name="deleteOldFiles">Indicates whether files in destination folder with mask '[guid]*.*' should be deleted.</param>
        /// <param name="synchronization">Indicates if this function is called from "ProcessTask".</param>
        /// <returns>Returns the path to the file on the disk.</returns>
        internal static string SaveFileToDisk(string siteName, string guid, string fileName, string fileExtension, BinaryData fileData, bool deleteOldFiles, bool synchronization = false)
        {
            return HelperObject.SaveFileToDiskInternal(siteName, guid, fileName, fileExtension, fileData, deleteOldFiles, synchronization);
        }


        /// <summary>
        /// Delete all files with the same name ([name].*) in specified directory.
        /// </summary>
        /// <param name="fileGuid">GUID of the file to delete.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not.</param>
        /// <param name="synchronization">Indicates whether the method is called due to synchronization.</param>
        public static void DeleteFile(Guid fileGuid, string siteName, bool deleteDirectory, bool synchronization = false)
        {
            HelperObject.DeleteFileInternal(fileGuid, siteName, deleteDirectory, synchronization);
        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <param name="maxSideSize">Max side size.</param>
        internal static bool CanResizeImage(IAttachment attachment, int width, int height, int maxSideSize)
        {
            return HelperObject.CanResizeImageInternal(attachment, width, height, maxSideSize);
        }

        #endregion


        #region "Public methods - Physical path"

        /// <summary>
        /// Returns files folder physical path according to 'CMSFilesFolder' settings key is set or not.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static string GetFilesFolderPath(string siteName)
        {
            return HelperObject.GetFilesFolderPathInternal(siteName);
        }


        /// <summary>
        /// Returns files folder relative path according to 'CMSFilesFolder' settings key is set or not.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        internal static string GetFilesFolderRelativePath(string siteName)
        {
            return HelperObject.GetFilesFolderRelativePathInternal(siteName);
        }


        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="extension">File extension.</param>
        public static string GetFilePhysicalPath(string siteName, string guid, string extension)
        {
            return HelperObject.GetFilePhysicalPathInternal(siteName, guid, extension);
        }


        /// <summary>
        /// Returns physical path to the thumbnail.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="extension">File extension.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        internal static string GetThumbnailPhysicalPath(string siteName, string guid, string extension, int width, int height)
        {
            return HelperObject.GetThumbnailPhysicalPathInternal(siteName, guid, extension, width, height);
        }

        #endregion


        #region "Internal methods - Binary data management"

        /// <summary>
        /// Resizes image to specified dimensions.
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        /// <param name="width">New width of the attachment.</param>
        /// <param name="height">New height of the attachment.</param>
        /// <param name="maxSideSize">Maximal side size of the attachment.</param>
        protected virtual void ResizeImageAttachmentInternal(IAttachment attachment, int width, int height, int maxSideSize)
        {
            // Resize image only if necessary
            if (!CanResizeImage(attachment, width, height, maxSideSize))
            {
                return;
            }

            var ih = new ImageHelper(attachment.AttachmentBinary, attachment.AttachmentImageWidth, attachment.AttachmentImageHeight);
            int[] newDims = ih.EnsureImageDimensions(width, height, maxSideSize);

            // If new dimensions are different from the original ones, resize the file
            if (((newDims[0] == ih.ImageWidth) && (newDims[1] == ih.ImageHeight)) || (newDims[0] <= 0) || (newDims[1] <= 0))
            {
                return;
            }

            attachment.AttachmentBinary = ih.GetResizedImageData(newDims[0], newDims[1], ThumbnailQuality);
            attachment.AttachmentSize = attachment.AttachmentBinary.Length;
            attachment.AttachmentImageHeight = newDims[1];
            attachment.AttachmentImageWidth = newDims[0];
        }


        /// <summary>
        /// Returns attachment binary and optionally store it in file system.
        /// </summary>
        /// <param name="guid">GUID of the attachment to get.</param>
        /// <param name="id">ID of the attachment to get.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="storeInFileSystem">If true, given attachment is stored in file system.</param>
        /// <returns>Attachment binary.</returns>
        protected virtual byte[] GetAttachmentBinaryInternal(Guid guid, int id, string siteName, bool storeInFileSystem)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            // Get attachment with binary from database
            var attachment =
                (id > 0) ?
                AttachmentInfoProvider.GetAttachmentInfo(id, true) :
                AttachmentInfoProvider.GetAttachmentInfo(guid, siteName);

            if (attachment == null)
            {
                return null;
            }

            // Get file content from attachment
            var binary = attachment.AttachmentBinary;

            // Save file to the disk for next use if exist in DB
            if (!storeInFileSystem || (attachment.AttachmentBinary == null))
            {
                return binary;
            }

            try
            {
                string stringGuid = guid.ToString();
                SaveFileToDisk(siteName, stringGuid, stringGuid, attachment.AttachmentExtension, attachment.AttachmentBinary, true);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Attachment", "GetAttachmentBinary", ex);
            }

            return binary;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="guid">GUID of the file to get.</param>
        /// <param name="siteName">Site name.</param>
        protected virtual byte[] GetAttachmentBinaryInternal(Guid guid, string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            byte[] fileContent = null;
            var locationType = FileHelper.FilesLocationType(siteName);

            if (locationType != FilesLocationTypeEnum.Database)
            {
                // Files are stored in file system - Get from the file system primarily
                var atInfo = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(guid, siteName);
                if (atInfo != null)
                {
                    fileContent = GetAttachmentBinary((DocumentAttachment)atInfo);
                }
            }
            else
            {
                // Get from the database
                fileContent = GetAttachmentBinaryInternal(guid, 0, siteName, false);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        protected virtual byte[] GetAttachmentBinaryInternal(DocumentAttachment attachment)
        {
            if (attachment == null)
            {
                return null;
            }

            byte[] fileContent;

            var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
            if (string.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException("Attachment site is not initialized.");
            }

            var locationType = FileHelper.FilesLocationType(siteName);

            if (locationType != FilesLocationTypeEnum.Database)
            {
                // Get file path                    
                string filePath = GetFilePhysicalPath(siteName, attachment.AttachmentGUID.ToString(), attachment.AttachmentExtension);
                bool fileValid = false;

                // Check if the file on the disk is valid (can use the file from disk)
                if (File.Exists(filePath))
                {
                    // If the size is valid, load from the file system
                    FileInfo fi = FileInfo.New(filePath);
                    if (fi.LastWriteTime >= attachment.AttachmentLastModified)
                    {
                        fileValid = true;
                    }
                }

                if (fileValid)
                {
                    // Get file contents from file system
                    fileContent = File.ReadAllBytes(filePath);
                }
                else
                {
                    // If the file has not been found, seek it in database and save it in file system
                    fileContent = GetAttachmentBinaryInternal(attachment.AttachmentGUID, attachment.AttachmentID, siteName, true);

                    // If no content found but file still exists, return the file
                    if ((fileContent == null) && File.Exists(filePath))
                    {
                        fileContent = File.ReadAllBytes(filePath);
                    }
                }
            }
            else
            {
                // Get from the database
                fileContent = GetAttachmentBinaryInternal(attachment.AttachmentGUID, attachment.AttachmentID, siteName, false);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="height">Image thumbnail width.</param>
        /// <param name="width">Image thumbnail height.</param>
        protected virtual byte[] GetImageThumbnailBinaryFileInternal(DocumentAttachment attachment, int width, int height)
        {
            if (attachment == null)
            {
                return null;
            }

            // Check file location
            var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
            if (string.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException("Attachment site is not initialized.");
            }

            var locationType = FileHelper.FilesLocationType(siteName);
            if (locationType == FilesLocationTypeEnum.Database)
            {
                // Files are stored in the database
                return null;
            }

            // Get thumbnail file name
            string stringGuid = attachment.AttachmentGUID.ToString();
            string filePath = GetThumbnailPhysicalPath(siteName, stringGuid, attachment.AttachmentExtension, width, height);

            if (!File.Exists(filePath))
            {
                return null;
            }

            // Get file content from file system
            return File.ReadAllBytes(filePath);
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesn't exist).
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="maxSideSize">Maximum side size.</param>
        /// <param name="searchThumbnailFile">Indicates if thumbnail should be searched on the disk.</param>
        protected virtual byte[] GetImageThumbnailInternal(DocumentAttachment attachment, int width, int height, int maxSideSize, bool searchThumbnailFile)
        {
            if (attachment == null)
            {
                return null;
            }

            byte[] thumbnail = null;
            byte[] data = attachment.AttachmentBinary;

            if (!ImageHelper.IsImage(attachment.AttachmentExtension))
            {
                return data;
            }

            // Get new dimensions
            int originalWidth = attachment.AttachmentImageWidth;
            int originalHeight = attachment.AttachmentImageHeight;
            int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

            // If new thumbnail dimensions are different from the original ones, resize the file
            bool resize = ((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0);
            if (resize)
            {
                var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
                if (string.IsNullOrEmpty(siteName))
                {
                    throw new InvalidOperationException("Attachment site is not initialized.");
                }

                var locationType = FileHelper.FilesLocationType(siteName);

                // Try to get image thumbnail from the disk
                if ((locationType != FilesLocationTypeEnum.Database) && searchThumbnailFile)
                {
                    thumbnail = GetImageThumbnailBinary(attachment, newDims[0], newDims[1]);
                }
            }

            // Create the thumbnail if not yet present
            if (thumbnail == null)
            {
                // If no data available, ensure the source data
                if (data == null)
                {
                    data = GetAttachmentBinary(attachment);
                }

                // Resize the image
                if ((data != null) && (resize))
                {
                    ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                    thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1], ThumbnailQuality);
                }
            }

            // If no thumbnail created, return original size
            return thumbnail ?? data;
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesn't exist).
        /// </summary>
        /// <param name="guid">File GUID.</param>
        /// <param name="imageData">Image data.</param>
        /// <param name="extension">Image extension.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="maxSideSize">Maximum side size.</param>
        /// <param name="originalWidth">Original width of the image.</param>
        /// <param name="originalHeight">Original height of the image.</param>
        protected virtual byte[] GetImageThumbnailInternal(Guid guid, byte[] imageData, string extension, string siteName, int width, int height, int maxSideSize, int originalWidth, int originalHeight)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            byte[] thumbnail = null;

            // Process image if data available and file is image
            try
            {
                if ((imageData != null) && ImageHelper.IsImage(extension))
                {
                    // Get new dimensions
                    ImageHelper imgHelper = new ImageHelper(imageData, originalWidth, originalHeight);
                    int[] newDims = imgHelper.EnsureImageDimensions(width, height, maxSideSize);

                    // If new thumbnail dimensions are different from the original ones, resize the file
                    if (((newDims[0] != imgHelper.ImageWidth) || (newDims[1] != imgHelper.ImageHeight)) && ((newDims[0] > 0) && (newDims[1] > 0)))
                    {
                        var locationType = FileHelper.FilesLocationType(siteName);
                        var storeFilesInFileSystem = locationType != FilesLocationTypeEnum.Database;

                        // Try to get image thumbnail from the disk
                        if (storeFilesInFileSystem)
                        {
                            thumbnail = GetImageThumbnailBinary(guid, siteName, newDims[0], newDims[1]);
                        }

                        if (thumbnail == null)
                        {
                            thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1], ThumbnailQuality);

                            // Save image thumbnail to disk if necessary
                            if (storeFilesInFileSystem && GenerateThumbnails(siteName))
                            {
                                string thumbnailFileName = ImageHelper.GetImageThumbnailFileName(guid.ToString(), newDims[0], newDims[1]);
                                SaveFileToDisk(siteName, guid.ToString(), thumbnailFileName, extension, thumbnail, false, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Attachment", "CreateThumbnail", ex);
            }

            return thumbnail ?? imageData;
        }


        /// <summary>
        /// Ensures the file in the file system and returns the path to the file.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        protected virtual string EnsurePhysicalFileInternal(DocumentAttachment attachment)
        {
            if (attachment == null)
            {
                return null;
            }

            var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
            if (string.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException("Attachment site is not initialized.");
            }

            var locationType = FileHelper.FilesLocationType(siteName);
            if (locationType == FilesLocationTypeEnum.Database)
            {
                return null;
            }

            // Check if the file exists
            string stringGuid = attachment.AttachmentGUID.ToString();
            string path = GetFilePhysicalPath(siteName, stringGuid, attachment.AttachmentExtension);
            if (File.Exists(path))
            {
                return path;
            }

            lock (ensureFileLock)
            {
                // Check the file existence again (in case other thread created it)
                if (File.Exists(path))
                {
                    return path;
                }

                // Create new file if the file does not exist
                byte[] data = attachment.AttachmentBinary;
                if (data == null)
                {
                    // Load the data from the database
                    var atInfo = AttachmentInfoProvider.GetAttachmentInfo(attachment.AttachmentID, true);
                    if (atInfo != null)
                    {
                        data = atInfo.AttachmentBinary;
                    }
                }

                if (data != null)
                {
                    try
                    {
                        return SaveFileToDisk(siteName, stringGuid, stringGuid, attachment.AttachmentExtension, data, true);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Attachment", "SaveFileToDisk", ex);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Ensures the thumbnail file.
        /// </summary>
        /// <param name="attachment">Document attachment.</param>
        /// <param name="width">File width.</param>
        /// <param name="height">File height.</param>
        /// <param name="maxSideSize">Maximum side size.</param>
        protected virtual string EnsureThumbnailFileInternal(DocumentAttachment attachment, int width, int height, int maxSideSize)
        {
            if (attachment == null)
            {
                return null;
            }

            var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
            if (string.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException("Attachment site is not initialized.");
            }

            var locationType = FileHelper.FilesLocationType(siteName);
            if ((locationType == FilesLocationTypeEnum.Database) || !GenerateThumbnails(siteName))
            {
                return null;
            }

            // Get new dimensions
            int originalWidth = attachment.AttachmentImageWidth;
            int originalHeight = attachment.AttachmentImageHeight;
            int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

            // If new thumbnail dimensions are different from the original ones, get resized file
            bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
            if (!resize)
            {
                return EnsurePhysicalFile(attachment);
            }

            // Check if the file exists
            string stringGuid = attachment.AttachmentGUID.ToString();
            string path = GetThumbnailPhysicalPath(siteName, stringGuid, attachment.AttachmentExtension, newDims[0], newDims[1]);
            if (File.Exists(path))
            {
                return path;
            }

            lock (ensureFileLock)
            {
                // Check the file existence again (in case other thread created it)
                if (File.Exists(path))
                {
                    return path;
                }

                // Create new file if the file does not exist
                byte[] data = GetImageThumbnailBinary(attachment, newDims[0], newDims[1], 0);
                if (data != null)
                {
                    try
                    {
                        // Save the data to the disk
                        string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, newDims[0], newDims[1]);
                        return SaveFileToDisk(siteName, stringGuid, fileName, attachment.AttachmentExtension, data, false, true);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Attachment", "SaveThumbnailFileToDisk", ex);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Saves file to the disk.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="fileExtension">File extension.</param>
        /// <param name="fileData">File data (byte[] or Stream).</param>   
        /// <param name="deleteOldFiles">Indicates whether files in destination folder with mask '[guid]*.*' should be deleted.</param>
        /// <param name="synchronization">Indicates if this function is called from "ProcessTask".</param>
        /// <returns>Returns the path to the file on the disk.</returns>
        protected virtual string SaveFileToDiskInternal(string siteName, string guid, string fileName, string fileExtension, BinaryData fileData, bool deleteOldFiles, bool synchronization)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("GUID of the file is not provided.");
            }

            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            // Get file folder path
            string fileFolderPath = GetFileFolder(siteName, guid);

            // Ensure disk path
            DirectoryHelper.EnsureDiskPath(fileFolderPath, SystemContext.WebApplicationPhysicalPath);

            // Check permission for specified folder
            if (!DirectoryHelper.CheckPermissions(fileFolderPath))
            {
                throw new PermissionException("Access to the path '" + fileFolderPath + "' is denied.");
            }

            // Get file path
            string filePath = GetFilePhysicalPath(siteName, fileName, fileExtension);

            // Ensure disk path
            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);

            if (deleteOldFiles)
            {
                // Delete all file occurrences in destination folder
                DeleteFile(ValidationHelper.GetGuid(guid, Guid.Empty), siteName, false, synchronization);
            }

            // Save specified file
            if (fileData != null)
            {
                StorageHelper.SaveFileToDisk(filePath, fileData, false);

                // If the action is not caused by synchronization, create the web farm task
                if (!synchronization)
                {
                    WebFarmHelper.CreateIOTask(DocumentTaskType.UpdateAttachment, filePath, fileData, "FileUpload", siteName, guid, fileName, fileExtension, deleteOldFiles.ToString());
                }

                fileData.Close();
            }

            return filePath;
        }


        /// <summary>
        /// Delete all files with the same name ([name].*) in specified directory.
        /// </summary>
        /// <param name="fileGuid">GUID of the file to delete.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not.</param>
        /// <param name="synchronization">Indicates whether the method is called due to synchronization.</param>
        protected virtual void DeleteFileInternal(Guid fileGuid, string siteName, bool deleteDirectory, bool synchronization)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            if (!DeletePhysicalFiles)
            {
                return;
            }

            string stringGuid = fileGuid.ToString();
            string folderPath = GetFileFolder(siteName, stringGuid);

            if (!Directory.Exists(folderPath))
            {
                return;
            }

            DirectoryInfo di = DirectoryInfo.New(folderPath);

            // Select all files with the same name ( '<GUID>*.*' )
            FileInfo[] filesInfos = di.GetFiles(stringGuid + "*.*");
            if (filesInfos == null)
            {
                return;
            }

            // Delete all selected files
            foreach (FileInfo file in filesInfos)
            {
                File.Delete(file.FullName);
                if (!synchronization)
                {
                    WebFarmHelper.CreateIOTask(DocumentTaskType.DeleteAttachment, file.FullName, null, "FileDelete", stringGuid, siteName, deleteDirectory.ToString());
                }
            }

            if (deleteDirectory)
            {
                // If directory is empty -> delete it
                filesInfos = di.GetFiles();
                if (filesInfos.Length == 0)
                {
                    DirectoryHelper.DeleteDirectory(folderPath);
                }
            }
        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="attachment">Attachment.</param>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <param name="maxSideSize">Max side size.</param>
        protected virtual bool CanResizeImageInternal(IAttachment attachment, int width, int height, int maxSideSize)
        {
            if (attachment == null)
            {
                return false;
            }

            // Resize only when bigger than required 
            if (maxSideSize > 0)
            {
                if ((maxSideSize < attachment.AttachmentImageWidth) || (maxSideSize < attachment.AttachmentImageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (attachment.AttachmentImageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (attachment.AttachmentImageHeight > height))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region "Internal methods - Physical path"

        /// <summary>
        /// Returns files folder physical path according to 'CMSFilesFolder' settings key is set or not.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        protected virtual string GetFilesFolderPathInternal(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            // Get files folder path from the settings key
            string filesFolderPath = FileHelper.FilesFolder(siteName);

            // If settings key is not specified -> get default files folder path
            if (filesFolderPath == "")
            {
                filesFolderPath = DirectoryHelper.CombinePath(SystemContext.WebApplicationPhysicalPath, siteName, "files") + "\\";
            }
            else
            {
                // Get full physical path
                filesFolderPath = FileHelper.GetFullFolderPhysicalPath(filesFolderPath, SystemContext.WebApplicationPhysicalPath);

                // Check if site specific folder should be used
                if (FileHelper.UseSiteSpecificCustomFolder(siteName))
                {
                    filesFolderPath = DirectoryHelper.CombinePath(filesFolderPath, siteName) + "\\";
                }
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Returns files folder relative path according to 'CMSFilesFolder' settings key is set or not.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        protected virtual string GetFilesFolderRelativePathInternal(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }

            // Get files folder path from the settings key
            string filesFolderPath = FileHelper.FilesFolder(siteName);

            // If settings key is not specified -> get default files folder path
            if (filesFolderPath == "")
            {
                filesFolderPath = siteName + "/files/";
            }
            else if (Path.IsPathRooted(filesFolderPath))
            {
                // Rooted path - cannot create relative path
                filesFolderPath = null;
            }
            else
            {
                filesFolderPath = filesFolderPath.StartsWithCSafe("~/") ? filesFolderPath.Substring(2) : filesFolderPath;
                filesFolderPath = filesFolderPath.Trim('/') + "/";

                // Check if site specific folder should be used
                if (FileHelper.UseSiteSpecificCustomFolder(siteName))
                {
                    filesFolderPath += siteName + "/";
                }
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Returns physical path to the file folder(files folder path + subfolder).
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>        
        private string GetFileFolder(string siteName, string guid)
        {
            // Get files folder physical path
            string filesFolderPath = GetFilesFolderPath(siteName);

            // Subfolder start with first two letters of the names of files that are placed in the folder
            string subfolder = guid.Substring(0, 2);

            return filesFolderPath + subfolder + "\\";
        }


        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="extension">File extension.</param>
        protected virtual string GetFilePhysicalPathInternal(string siteName, string guid, string extension)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name is not provided.");
            }
            
            // Get file folder physical path
            string fileFolder = GetFileFolder(siteName, guid);

            return fileFolder + AttachmentHelper.GetFullFileName(guid, extension);
        }


        /// <summary>
        /// Returns physical path to the thumbnail.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="guid">File GUID.</param>
        /// <param name="extension">File extension.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        protected virtual string GetThumbnailPhysicalPathInternal(string siteName, string guid, string extension, int width, int height)
        {
            // Get thumbnail file name and physical file path
            string fileName = ImageHelper.GetImageThumbnailFileName(guid, width, height);
            return GetFilePhysicalPath(siteName, fileName, extension);
        }

        #endregion
    }
}

using System;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Forums
{
    using TypedDataSet = InfoDataSet<ForumAttachmentInfo>;

    /// <summary>
    /// Class providing ForumAttachment management.
    /// </summary>
    public class ForumAttachmentInfoProvider : AbstractInfoProvider<ForumAttachmentInfo, ForumAttachmentInfoProvider>
    {
        #region "Public properties"

        /// <summary>
        /// Full path to the root of the web.
        /// </summary>
        public static string WebApplicationPhysicalPath
        {
            get
            {
                return SystemContext.WebApplicationPhysicalPath;
            }
        }


        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Parameters</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns a query for all the ForumAttachmentInfo objects.
        /// </summary>
        public static ObjectQuery<ForumAttachmentInfo> GetForumAttachments()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the ForumAttachment structure for the specified forumAttachment.
        /// </summary>
        /// <param name="guid">Forum attachment guid</param>
        /// <param name="siteName">Attachemnt site name</param>
        public static ForumAttachmentInfo GetForumAttachmentInfoByGuid(Guid guid, string siteName)
        {
            return ProviderObject.GetForumAttachmentInfoInternal(guid, siteName);
        }


        /// <summary>
        /// Returns the ForumAttachment structure for the specified forumAttachment.
        /// </summary>
        /// <param name="guid">GUID of the forumAttachment to return</param>   
        /// <param name="siteName">Attachment site name</param>
        public static ForumAttachmentInfo GetForumAttachmentInfoWithoutBinary(Guid guid, string siteName)
        {
            return ProviderObject.GetForumAttachmentInfoWithoutBinaryInternal(guid, siteName);
        }


        /// <summary>
        /// Returns the ForumAttachment structure for the specified forumAttachment.
        /// </summary>
        /// <param name="forumAttachmentId">ForumAttachment id</param>
        public static ForumAttachmentInfo GetForumAttachmentInfo(int forumAttachmentId)
        {
            return ProviderObject.GetInfoById(forumAttachmentId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumAttachment.
        /// </summary>
        /// <param name="forumAttachment">ForumAttachmentInfo to set</param>
        public static void SetForumAttachmentInfo(ForumAttachmentInfo forumAttachment)
        {
            ProviderObject.SetInfo(forumAttachment);
        }


        /// <summary>
        /// Deletes specified forumAttachment.
        /// </summary>
        /// <param name="infoObj">ForumAttachmentInfo object</param>
        public static void DeleteForumAttachmentInfo(ForumAttachmentInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified forumAttachment.
        /// </summary>
        /// <param name="forumAttachmentId">ForumAttachment id</param>
        public static void DeleteForumAttachmentInfo(int forumAttachmentId)
        {
            ForumAttachmentInfo infoObj = GetForumAttachmentInfo(forumAttachmentId);
            DeleteForumAttachmentInfo(infoObj);
        }


        /// <summary>
        /// Delete files in file system, with dependence on where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="siteName">Site name</param>
        public static void DeleteFiles(string where, string siteName)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return;
            }

            DataSet ds = ProviderObject.GetObjectQuery().Where(where).Column("AttachmentGUID");
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DeleteAttachmentFile(siteName, dr["AttachmentGUID"].ToString(), false, false);
            }
        }


        /// <summary>
        /// Returns DataSet with attachment which have relation to specified forum post id.
        /// </summary>
        /// <param name="postId">Forum post id</param>   
        /// <param name="getBinary">If true binary data will be retrieved from DB</param>
        public static TypedDataSet GetForumAttachments(int postId, bool getBinary)
        {
            return ProviderObject.GetForumAttachmentsInternal(postId, getBinary);
        }


        /// <summary>
        /// Returns dataset with attachments with dependence on input parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Columns</param>
        [Obsolete("Use method GetForumAttachments() instead")]
        public static TypedDataSet GetForumAttachments(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetForumAttachmentsInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns true if file extension is allowed.
        /// </summary>
        /// <param name="filename">File name with extension</param>
        /// <param name="siteName">Site name</param>
        public static bool IsExtensionAllowed(string filename, string siteName)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                string ext = String.Empty;
                if (filename.Contains("."))
                {
                    // Get filename extension
                    ext = filename.Substring(filename.LastIndexOfCSafe('.')).ToLowerCSafe();
                    // Remove dot at the start, and add colon at the start and at the end
                    ext = ";" + ext.Replace(".", "") + ";";
                }

                // Get allowed extensions from settings
                string allowedExtensions = ";" + ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName + ".CMSForumAttachmentExtensions"), "").ToLowerCSafe().Replace(" ", "") + ";";

                // If does not exist at least one extension => extension is enabled
                if (allowedExtensions != ";;")
                {
                    if (ext == String.Empty)
                    {
                        return false;
                    }

                    // Check whether extension is in allowed extension
                    return allowedExtensions.Contains(ext);
                }
            }

            return true;
        }


        /// <summary>
        /// Delete forum attachment file stored in the file system.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="attachmentFileName">Name of the file to be deleted</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not</param>
        /// <param name="synchronization">Indicates whether the method is called due to synchronization</param>
        public static void DeleteAttachmentFile(string siteName, string attachmentFileName, bool deleteDirectory, bool synchronization)
        {
            // Get file folde path
            string directoryPath = DirectoryHelper.CombinePath(GetFilesFolderPath(siteName), AttachmentHelper.GetFileSubfolder(attachmentFileName)) + "\\";
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo di = DirectoryInfo.New(directoryPath);

                // Select file with the specified name and extension
                FileInfo[] filesInfos = di.GetFiles(attachmentFileName + "*.*");

                if (filesInfos != null)
                {
                    // Delete selected file
                    foreach (FileInfo file in filesInfos)
                    {
                        File.Delete(file.FullName);
                        if (!synchronization)
                        {
                            WebFarmHelper.CreateIOTask(ForumsTaskType.DeleteForumAttachment, file.FullName, null, "ForumAttachmentDelete", siteName, attachmentFileName, deleteDirectory.ToString());
                        }
                    }

                    if (deleteDirectory)
                    {
                        // If the folder is empty, delete it
                        filesInfos = di.GetFiles();
                        if (filesInfos.Length == 0)
                        {
                            DirectoryHelper.DeleteDirectory(directoryPath);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns the attachment file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="guid">Guid of the attachment file to get</param>        
        /// <param name="siteName">Attachment site name</param>
        public static byte[] GetAttachmentFile(Guid guid, string siteName)
        {
            byte[] fileContent = null;
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            if (filesLocationType != FilesLocationTypeEnum.Database)
            {
                // Files are stored in file system or using both storages - Get from the file system primarily
                ForumAttachmentInfo attachmentInfo = GetForumAttachmentInfoWithoutBinary(guid, siteName);
                if (attachmentInfo != null)
                {
                    fileContent = GetAttachmentFile(attachmentInfo);
                }
            }
            else
            {
                // Get from the database
                fileContent = GetAttachmentFileBinary(guid, siteName);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="attachmentInfo">Forum attachment info object</param>
        public static byte[] GetAttachmentFile(ForumAttachmentInfo attachmentInfo)
        {
            if (attachmentInfo == null)
            {
                return null;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(attachmentInfo.AttachmentSiteID);
            if (si == null)
            {
                return null;
            }

            byte[] fileContent;
            var filesLocationType = FileHelper.FilesLocationType(si.SiteName);

            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                // Get from the database
                fileContent = GetAttachmentFileBinary(attachmentInfo.AttachmentGUID, si.SiteName);
            }
            else
            {
                // Files are stored in file system or using both types of storages - Get from the file system primarily
                bool fileValid = false;

                // Get file path            
                string filePath = EnsureAttachmentPhysicalFile(attachmentInfo, si.SiteName);

                // Check if the file on the disk is valid (can use the file from disk)
                if (File.Exists(filePath))
                {
                    // If the file time is valid, load from the file system
                    FileInfo fi = FileInfo.New(filePath);
                    if (fi.LastWriteTime >= attachmentInfo.AttachmentLastModified)
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
                    fileContent = GetAttachmentFileBinary(attachmentInfo.AttachmentGUID, si.SiteName);

                    // If no content found but file still exists, return the file
                    if ((fileContent == null) && File.Exists(filePath))
                    {
                        fileContent = File.ReadAllBytes(filePath);
                    }
                }
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file binary from disk.
        /// </summary>
        /// <param name="guid">Attachment guid</param>        
        /// <param name="siteName">Attachment sitename</param>
        public static byte[] GetAttachmentFileBinary(Guid guid, string siteName)
        {
            string stringGuid = guid.ToString();
            byte[] binary = null;

            // Get attachment with binary from database
            ForumAttachmentInfo attachmentInfo = GetForumAttachmentInfoByGuid(guid, siteName);
            if (attachmentInfo != null)
            {
                // Get file content from file
                binary = attachmentInfo.AttachmentBinary;
                var filesLocationType = FileHelper.FilesLocationType(siteName);

                // Save file to the disk for next use
                if ((filesLocationType != FilesLocationTypeEnum.FileSystem) || (attachmentInfo.AttachmentBinary == null))
                {
                    return binary;
                }

                try
                {
                    SaveAttachmentFileToDisk(siteName, stringGuid, stringGuid, attachmentInfo.AttachmentFileExtension, attachmentInfo.AttachmentBinary, false);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("ForumAttachmentInfoProvider.GetFileBinary", "E", ex);
                }
            }

            return binary;
        }


        /// <summary>
        /// Returns physical path to the file folder (files folder path + subfolder).
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>        
        private static string GetFileFolder(string siteName, string guid)
        {
            // Subfolder name start with first two letters of the names of files which are placed in the folder
            string subfolder = AttachmentHelper.GetFileSubfolder(guid);

            return DirectoryHelper.CombinePath(GetFilesFolderPath(siteName), subfolder) + "\\";
        }


        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="extension">File extension</param>
        public static string GetFilePhysicalPath(string siteName, string guid, string extension)
        {
            // Get file physical path
            return GetFileFolder(siteName, guid) + AttachmentHelper.GetFullFileName(guid, extension);
        }


        /// <summary>
        /// Ensures the attachment file in the file system and returns the path to the file.
        /// </summary>
        /// <param name="attachmentInfo">Attachment info</param>       
        /// <param name="siteName">Attachment site name</param>
        public static string EnsureAttachmentPhysicalFile(ForumAttachmentInfo attachmentInfo, string siteName)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            if ((attachmentInfo == null) || (filesLocationType == FilesLocationTypeEnum.Database))
            {
                return null;
            }

            // Check if the file exists                
            string path = GetFilePhysicalPath(siteName, attachmentInfo.AttachmentGUID.ToString(), attachmentInfo.AttachmentFileExtension);
            if (File.Exists(path))
            {
                return path;
            }

            // Create new file if the file does not exist
            byte[] data = attachmentInfo.AttachmentBinary;
            if (data == null)
            {
                // Load the data from the database
                ForumAttachmentInfo attachment = GetForumAttachmentInfo(attachmentInfo.AttachmentID);
                if (attachment != null)
                {
                    data = attachment.AttachmentBinary;
                }
            }
            if (data != null)
            {
                try
                {
                    return SaveAttachmentFileToDisk(siteName, attachmentInfo.AttachmentGUID.ToString(), attachmentInfo.AttachmentGUID.ToString(), attachmentInfo.AttachmentFileExtension, data, false);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("ForumAttachmentInfoProvider", "SaveAttachmentFileToDisk", ex);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns physical path to folder with forum attachments files which are associated with the specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFilesFolderPath(string siteName)
        {
            // Get files folder path from the settings
            string filesFolderPath = FileHelper.FilesFolder(siteName);

            // Folder is not specified in settings -> get default files folder path
            if (filesFolderPath == "")
            {
                if ((siteName != null) && (siteName != ""))
                {
                    // Site files
                    filesFolderPath = DirectoryHelper.CombinePath(WebApplicationPhysicalPath, siteName, "forumattachments") + "\\";
                }
                else
                {
                    // Global files
                    filesFolderPath = WebApplicationPhysicalPath + "\\CMSFiles";
                }
            }
            // Folder is specified in settings -> ensure files folder path
            else
            {
                // Get full physical path
                filesFolderPath = FileHelper.GetFullFolderPhysicalPath(filesFolderPath, WebApplicationPhysicalPath);

                // Check if site specific folder should be used
                if (FileHelper.UseSiteSpecificCustomFolder(siteName))
                {
                    filesFolderPath = DirectoryHelper.CombinePath(filesFolderPath, siteName) + "\\";
                }
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Saves attachment file to the disk.
        /// </summary>
        /// <param name="siteName">Attachment site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="fileData">File data (byte[] or Stream)</param>   
        /// <param name="synchronization">Indicates if this function is called from "ProcessTask"</param>
        public static string SaveAttachmentFileToDisk(string siteName, string guid, string fileName, string fileExtension, BinaryData fileData, bool synchronization)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                string filesFolderPath = GetFilesFolderPath(siteName);

                // Check permission for specified folder
                if (!DirectoryHelper.CheckPermissions(filesFolderPath))
                {
                    throw new PermissionException("[ForumAttachmentInfoProvider.SaveFileToDisk]: Access to the path '" + filesFolderPath + "' is denied.");
                }

                // Get file path
                string filePath = DirectoryHelper.CombinePath(filesFolderPath, AttachmentHelper.GetFileSubfolder(fileName), AttachmentHelper.GetFullFileName(fileName, fileExtension));

                // Ensure disk path
                DirectoryHelper.EnsureDiskPath(filePath, WebApplicationPhysicalPath);

                // Save specified file
                if (fileData != null)
                {
                    StorageHelper.SaveFileToDisk(filePath, fileData, false);

                    // If the action is not caused by synchronization, create the web farm task
                    if (!synchronization)
                    {
                        WebFarmHelper.CreateIOTask(ForumsTaskType.UpdateForumAttachment, filePath, fileData, "ForumAttachmentUpdate", siteName, guid, fileName, fileExtension);
                    }

                    fileData.Close();

                    if (synchronization)
                    {
                        SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                        if (si != null)
                        {
                            // Drop the cache dependencies
                            CacheHelper.TouchKey("forumattachment|" + ValidationHelper.GetString(guid, Guid.Empty.ToString()).ToLowerCSafe() + "|" + si.SiteID.ToString(), false, false);
                        }
                    }
                }

                return null;
            }

            throw new Exception("[ForumAttachmentInfoProvider.SaveFileToDisk]: Filename of the file is not specified.");
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesnt exist).
        /// </summary>        
        /// <param name="attachmentInfo">Attachment info</param>        
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static byte[] GetImageThumbnail(ForumAttachmentInfo attachmentInfo, int width, int height, int maxSideSize)
        {
            if (attachmentInfo == null)
            {
                return null;
            }

            byte[] thumbnail = null;
            byte[] data = attachmentInfo.AttachmentBinary;

            if (ImageHelper.IsImage(attachmentInfo.AttachmentFileExtension))
            {
                // Get new dimensions
                int originalWidth = attachmentInfo.AttachmentImageWidth;
                int originalHeight = attachmentInfo.AttachmentImageHeight;
                int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                // If new thumbnail dimensions are different from the original ones, resize the file
                bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));

                // Create the thumbnail if not yet present
                if (thumbnail == null)
                {
                    // If no data available, ensure the source data
                    if (data == null)
                    {
                        data = GetAttachmentFile(attachmentInfo);
                    }

                    // Resize the image
                    if ((data != null) && (resize))
                    {
                        ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                        thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1], ImageHelper.DefaultQuality);
                    }
                }
            }

            // If no thumbnail created, return original size
            return thumbnail ?? data;
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="guid">Guid of the file to get</param>  
        /// <param name="siteName">Attachment sitename</param>
        /// <param name="height">Image thumbnail width</param>
        /// <param name="width">Image thumbnail height</param>
        public static byte[] GetImageThumbnailFile(Guid guid, string siteName, int width, int height)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            // files are stored in database
            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return null;
            }

            byte[] fileContent = null;

            // Get file without binary
            ForumAttachmentInfo attachmentInfo = GetForumAttachmentInfoWithoutBinary(guid, siteName);
            if (attachmentInfo != null)
            {
                string stringGuid = guid.ToString();
                string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, width, height);

                // Get file path            
                string filePath = GetFilePhysicalPath(siteName, fileName, attachmentInfo.AttachmentFileExtension);

                // Get the data
                if (File.Exists(filePath))
                {
                    fileContent = File.ReadAllBytes(filePath);
                }
            }

            return fileContent;
        }


        /// <summary>
        /// Ensures the thumbnail.
        /// </summary>
        /// <param name="attachmentInfo">Attachment info</param>
        /// <param name="width">File width</param>
        /// <param name="height">File height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static string EnsureThumbnailFile(ForumAttachmentInfo attachmentInfo, int width, int height, int maxSideSize)
        {
            if (attachmentInfo == null)
            {
                return null;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(attachmentInfo.AttachmentSiteID);
            if (si == null)
            {
                return null;
            }

            var filesLocationType = FileHelper.FilesLocationType(si.SiteName);
            if ((!GenerateThumbnails(si.SiteName)) || (filesLocationType == FilesLocationTypeEnum.Database))
            {
                return null;
            }

            // Get new dimensions
            int originalWidth = attachmentInfo.AttachmentImageWidth;
            int originalHeight = attachmentInfo.AttachmentImageHeight;
            int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

            // If new thumbnail dimensions are different from the original ones, get resized file
            bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
            if (!resize)
            {
                return EnsureAttachmentPhysicalFile(attachmentInfo, si.SiteName);
            }

            // Check if the file exists
            string stringGuid = attachmentInfo.AttachmentGUID.ToString();

            string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, width, height);

            // Get file path            
            string path = GetFilePhysicalPath(si.SiteName, fileName, attachmentInfo.AttachmentFileExtension);

            if (File.Exists(path))
            {
                return path;
            }

            // Create new file if the file does not exist
            byte[] data = GetImageThumbnail(attachmentInfo, newDims[0], newDims[1], 0);
            if (data == null)
            {
                return null;
            }

            try
            {
                // Save the data to the disk
                fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, newDims[0], newDims[1]);
                return SaveAttachmentFileToDisk(si.SiteName, stringGuid, fileName, attachmentInfo.AttachmentFileExtension, data, false);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ForumAttachmentProvider", "EnsureThumbnailFile", ex);
            }

            return null;
        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="fileInfo">File info to check</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static bool CanResizeImage(ForumAttachmentInfo fileInfo, int width, int height, int maxSideSize)
        {
            if (fileInfo == null)
            {
                return false;
            }

            // Resize only when bigger than required 
            if (maxSideSize > 0)
            {
                if ((maxSideSize < fileInfo.AttachmentImageWidth) || (maxSideSize < fileInfo.AttachmentImageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (fileInfo.AttachmentImageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (fileInfo.AttachmentImageHeight > height))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns the current settings whether the thumbnails should be generated.
        /// </summary>        
        /// <param name="siteName">Site name</param>
        public static bool GenerateThumbnails(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSGenerateThumbnails");
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the ForumAttachment structure for the specified forumAttachment.
        /// </summary>
        /// <param name="guid">Forum attachment guid</param>
        /// <param name="siteName">Attachemnt site name</param>
        protected virtual ForumAttachmentInfo GetForumAttachmentInfoInternal(Guid guid, string siteName)
        {
            // Get the data
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetInfoByGuid(guid, si.SiteID);
            }

            return null;
        }


        /// <summary>
        /// Returns the ForumAttachment structure for the specified forumAttachment.
        /// </summary>
        /// <param name="guid">GUID of the forumAttachment to return</param>   
        /// <param name="siteName">Attachment site name</param>
        protected virtual ForumAttachmentInfo GetForumAttachmentInfoWithoutBinaryInternal(Guid guid, string siteName)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                return null;
            }

            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                string columns = "AttachmentID, AttachmentFileName, AttachmentFileExtension, AttachmentGUID, AttachmentLastModified, AttachmentMimeType, AttachmentFileSize, AttachmentImageHeight, AttachmentImageWidth, AttachmentPostID, AttachmentSiteID";

                return GetForumAttachments().Where("AttachmentGUID = '" + guid + "' AND AttachmentSiteID = " + si.SiteID).Columns(columns).BinaryData(false).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ForumAttachmentInfo info)
        {
            if (info != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(info.AttachmentSiteID);
                if (si != null)
                {
                    string siteName = si.SiteName;

                    byte[] data = info.AttachmentBinary;
                    var filesLocationType = FileHelper.FilesLocationType(siteName);

                    if (filesLocationType == FilesLocationTypeEnum.FileSystem)
                    {
                        info.AttachmentBinary = null;
                    }

                    // Use transaction
                    using (var tr = BeginTransaction())
                    {
                        bool isInsert = (info.AttachmentID <= 0);

                        base.SetInfo(info);

                        if (isInsert)
                        {
                            // Update attachments count
                            ForumPostInfoProvider.UpdatePostAttachmentCount(info.AttachmentPostID);
                        }

                        string guid = Convert.ToString(info.AttachmentGUID);

                        if (filesLocationType != FilesLocationTypeEnum.Database)
                        {
                            if (data != null)
                            {
                                // Save using memory data
                                SaveAttachmentFileToDisk(siteName, guid, guid, info.AttachmentFileExtension, data, false);
                            }
                        }
                        else
                        {
                            // Log web farm task
                            WebFarmHelper.CreateTask(ForumsTaskType.UpdateForumAttachment, "ForumAttachmentUpdate", siteName, guid, guid, info.AttachmentFileExtension);
                        }

                        // Commit transaction
                        tr.Commit();
                    }
                }
                else
                {
                    throw new ArgumentException("Site name is not defined.");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(info));
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ForumAttachmentInfo info)
        {
            if (info != null)
            {
                // Get post ID of attachment
                int postID = info.AttachmentPostID;

                base.DeleteInfo(info);

                // Updates attachments post
                ForumPostInfoProvider.UpdatePostAttachmentCount(postID);
            }
        }


        /// <summary>
        /// Returns DataSet with attachment which have relation to specified forum post id.
        /// </summary>
        /// <param name="postId">Forum post id</param>   
        /// <param name="getBinary">If true binary data will be retrieved from DB</param>
        protected virtual TypedDataSet GetForumAttachmentsInternal(int postId, bool getBinary)
        {
            string columns = null;

            // Get rid of binary columns
            if (!getBinary)
            {
                columns = "[AttachmentID], [AttachmentFileName], [AttachmentFileExtension], [AttachmentGUID], [AttachmentLastModified], [AttachmentMimeType], [AttachmentFileSize], [AttachmentImageHeight], [AttachmentImageWidth], [AttachmentPostID], [AttachmentSiteID]";
            }

            // Get the data
            return GetForumAttachments().WhereEquals("AttachmentPostID", postId).Columns(columns).BinaryData(false).TypedResult;
        }


        /// <summary>
        /// Returns dataset with attachments with dependence on input parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Columns</param>
        [Obsolete("Use method GetForumAttachments() instead")]
        protected virtual TypedDataSet GetForumAttachmentsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetForumAttachments().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }

        #endregion
    }
}
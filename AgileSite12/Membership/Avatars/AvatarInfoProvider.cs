using System;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing AvatarInfo management.
    /// </summary>
    public class AvatarInfoProvider : AbstractInfoProvider<AvatarInfo, AvatarInfoProvider>
    {
        #region "Private fields"

        /// <summary>
        /// Contains default avatars, key is DefaultAvatarTypeEnum. [AvatarTypeEnum -> AvatarInfo]
        /// </summary>
        private static readonly CMSStatic<SafeDictionary<DefaultAvatarTypeEnum, AvatarInfo>> mDefaultAvatars = new CMSStatic<SafeDictionary<DefaultAvatarTypeEnum, AvatarInfo>>(() => new SafeDictionary<DefaultAvatarTypeEnum, AvatarInfo>());

        /// <summary>
        /// Lock object for ensuring of the physical files.
        /// </summary>
        private static readonly object ensureFileLock = new object();

        #endregion


        #region "Public constants"

        /// <summary>
        /// Constant for avatar option
        /// </summary>
        public const string AVATAR = "avatar";

        /// <summary>
        /// Constant for gravatar option
        /// </summary>
        public const string GRAVATAR = "gravatar";

        /// <summary>
        /// Constant for user choice option
        /// </summary>
        public const string USERCHOICE = "userchoice";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Contains default avatars, key is DefaultAvatarTypeEnum. [DefaultAvatarTypeEnum -> AvatarInfo]
        /// </summary>
        private static SafeDictionary<DefaultAvatarTypeEnum, AvatarInfo> DefaultAvatars => mDefaultAvatars;


        /// <summary>
        /// Full path to the root of the web.
        /// </summary>
        public static string WebApplicationPhysicalPath => SystemContext.WebApplicationPhysicalPath;

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the query for all avatars.
        /// </summary>
        public static ObjectQuery<AvatarInfo> GetAvatars()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="guid">Avatar id</param>
        public static AvatarInfo GetAvatarInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarId">Avatar id</param>
        public static AvatarInfo GetAvatarInfo(int avatarId)
        {
            return ProviderObject.GetInfoById(avatarId);
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarName">Avatar name</param>
        public static AvatarInfo GetAvatarInfo(string avatarName)
        {
            return ProviderObject.GetAvatarInfoInternal(avatarName);
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarId">Avatar id</param>
        public static AvatarInfo GetAvatarInfoWithoutBinary(int avatarId)
        {
            return ProviderObject.GetAvatarInfoWithoutBinaryInternal(avatarId);
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="guid">GUID of the avatar to return</param>
        public static AvatarInfo GetAvatarInfoWithoutBinary(Guid guid)
        {
            return ProviderObject.GetAvatarInfoWithoutBinaryInternal(guid);
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarName">Name of the avatar to return</param>
        public static AvatarInfo GetAvatarInfoWithoutBinary(string avatarName)
        {
            return ProviderObject.GetAvatarInfoWithoutBinaryInternal(avatarName);
        }


        /// <summary>
        /// Returns default avatar of specified type or null if such is not set.
        /// </summary>
        /// <param name="type">Default avatar type</param>
        public static AvatarInfo GetDefaultAvatar(DefaultAvatarTypeEnum type)
        {
            return ProviderObject.GetDefaultAvatarInternal(type);
        }


        /// <summary>
        /// Clears specified default avatar type from all avatars.
        /// </summary>
        /// <param name="type">Default avatar type</param>
        public static void ClearDefaultAvatar(DefaultAvatarTypeEnum type)
        {
            ClearDefaultAvatar(type, false);
        }


        /// <summary>
        /// Clears specified default avatar type from all avatars.
        /// </summary>
        /// <param name="type">Default avatar type</param>
        /// <param name="onlyDefaultAvatarChahe">If true clears only default avatar cache</param>
        public static void ClearDefaultAvatar(DefaultAvatarTypeEnum type, bool onlyDefaultAvatarChahe)
        {
            ClearDefaultAvatar(type, onlyDefaultAvatarChahe, true);
        }


        /// <summary>
        /// Clears specified default avatar type from all avatars.
        /// </summary>
        /// <param name="type">Default avatar type</param>
        /// <param name="onlyDefaultAvatarCache">If true clears only default avatar cache</param>
        /// <param name="logWebFarm">Enables or disables webfarm task logging</param>
        public static void ClearDefaultAvatar(DefaultAvatarTypeEnum type, bool onlyDefaultAvatarCache, bool logWebFarm)
        {
            // Clear from cache
            DefaultAvatars[type] = null;

            if (!onlyDefaultAvatarCache)
            {
                //Clear from database
                string query = "";

                // Different query name by type
                switch (type)
                {
                    case DefaultAvatarTypeEnum.Female:
                        query = "ClearDefaultFemaleAvatar";
                        break;

                    case DefaultAvatarTypeEnum.Group:
                        query = "ClearDefaultGroupAvatar";
                        break;

                    case DefaultAvatarTypeEnum.Male:
                        query = "ClearDefaultMaleAvatar";
                        break;

                    case DefaultAvatarTypeEnum.User:
                        query = "ClearDefaultUserAvatar";
                        break;
                }

                // Execute clear
                ConnectionHelper.ExecuteQuery("cms.avatar." + query, null);
            }

            // Create webfarm task if needed
            if (logWebFarm)
            {
                string taskParams = type.ToString();
                ProviderObject.CreateWebFarmTask("cleardefaultavatar", taskParams);
            }
        }


        /// <summary>
        /// Sets (updates or inserts) specified avatar.
        /// </summary>
        /// <param name="avatar">Avatar to set</param>
        public static void SetAvatarInfo(AvatarInfo avatar)
        {
            ProviderObject.SetAvatarInfoInternal(avatar);
        }


        /// <summary>
        /// Deletes specified avatar.
        /// </summary>
        /// <param name="infoObj">Avatar object</param>
        public static void DeleteAvatarInfo(AvatarInfo infoObj)
        {
            ProviderObject.DeleteAvatarInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified avatar.
        /// </summary>
        /// <param name="avatarId">ID of the avatar object</param>
        public static void DeleteAvatarInfo(int avatarId)
        {
            AvatarInfo ai = GetAvatarInfo(avatarId);
            DeleteAvatarInfo(ai);
        }


        /// <summary>
        /// Delete avatar file stored in the file system.
        /// </summary>
        /// <param name="avatarGuid">Guid file to delete</param>
        /// <param name="fileExtension">Extension of the avatar file</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not</param>
        /// <param name="synchronization">Indicates whether the method is called due to synchronization</param>
        public static void DeleteAvatarFile(string avatarGuid, string fileExtension, bool deleteDirectory, bool synchronization)
        {
            // Get file folder path
            string directoryPath = DirectoryHelper.CombinePath(GetFilesFolderPath(), AttachmentHelper.GetFileSubfolder(avatarGuid)) + "\\";
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo di = DirectoryInfo.New(directoryPath);

                // Select file with the specified name and extension
                FileInfo[] filesInfos = di.GetFiles(avatarGuid + "*.*");

                if (filesInfos.Length > 0)
                {
                    // Delete selected file
                    foreach (FileInfo file in filesInfos)
                    {
                        File.Delete(file.FullName);
                        if (!synchronization)
                        {
                            WebFarmHelper.CreateIOTask(new DeleteAvatarWebFarmTask
                            {
                                FileGuid = ValidationHelper.GetGuid(avatarGuid, Guid.Empty),
                                FileExtension = fileExtension,
                                DeleteDirectory = deleteDirectory,
                                TaskFilePath = file.FullName
                            });
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
        /// Returns the avatar file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="guid">Guid of the avatar file to get</param>
        public static byte[] GetAvatarFile(Guid guid)
        {
            byte[] fileContent = null;

            var filesLocationType = FileHelper.FilesLocationType();
            if (filesLocationType != FilesLocationTypeEnum.Database)
            {
                // Files are stored in file system or both storages are used - Get from the file system primarily
                AvatarInfo avatarInfo = GetAvatarInfoWithoutBinary(guid);
                if (avatarInfo != null)
                {
                    fileContent = GetAvatarFile(avatarInfo);
                }
            }
            else
            {
                // Get from the database
                fileContent = GetAvatarFileBinary(guid);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="avatarInfo">Avatar info object</param>
        public static byte[] GetAvatarFile(AvatarInfo avatarInfo)
        {
            if (avatarInfo == null)
            {
                return null;
            }

            byte[] fileContent;

            var filesLocationType = FileHelper.FilesLocationType();
            if (filesLocationType != FilesLocationTypeEnum.Database)
            {
                // Files are stored in file system - Get from the file system primarily

                bool fileValid = false;

                // Get file path
                string filePath = EnsureAvatarPhysicalFile(avatarInfo);

                // Check if the file on the disk is valid (can use the file from disk)
                if (File.Exists(filePath))
                {
                    // If the size is valid, load from the file system
                    FileInfo fi = FileInfo.New(filePath);
                    if (fi.LastWriteTime >= avatarInfo.AvatarLastModified)
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
                    fileContent = GetAvatarFileBinary(avatarInfo.AvatarGUID);

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
                fileContent = GetAvatarFileBinary(avatarInfo.AvatarGUID);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file binary from database and save it in file system
        /// </summary>
        /// <param name="guid">Avatar guid</param>
        public static byte[] GetAvatarFileBinary(Guid guid)
        {
            string stringGuid = guid.ToString();
            byte[] binary = null;

            // Get attachment with binary from database
            AvatarInfo avatarInfo = GetAvatarInfo(guid);
            if (avatarInfo != null)
            {
                // Get file content from avatar file
                binary = avatarInfo.AvatarBinary;

                string filePath = GetFilePhysicalPath(null, avatarInfo.AvatarGUID.ToString(), avatarInfo.AvatarFileExtension);
                var filesLocationType = FileHelper.FilesLocationType();

                // Save file to the disk for next use
                if ((filesLocationType != FilesLocationTypeEnum.Database) && (avatarInfo.AvatarBinary != null) && !File.Exists(filePath))
                {
                    try
                    {
                        SaveAvatarFileToDisk(stringGuid, stringGuid, avatarInfo.AvatarFileExtension, avatarInfo.AvatarBinary, false);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("AvatarInfoProvider.GetFileBinary", "E", ex);
                    }
                }
            }

            return binary;
        }


        /// <summary>
        /// Update file last modified attribute, so it's evaluated us up to date.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="extension">File extension</param>
        internal static void UpdatePhysicalFileLastWriteTime(string fileName, string extension)
        {
            var filesLocationType = FileHelper.FilesLocationType();

            // Update timestamp
            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return;
            }

            // Get file path
            string filePath = GetFilePhysicalPath(null, fileName, extension);

            if (!File.Exists(filePath))
            {
                return;
            }

            // If the size is valid, load from the file system
            FileInfo fi = FileInfo.New(filePath);
            fi.LastWriteTime = DateTime.Now;
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesn't exist).
        /// </summary>
        /// <param name="avatarInfo">Avatar info</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static byte[] GetImageThumbnail(AvatarInfo avatarInfo, int width, int height, int maxSideSize)
        {
            if (avatarInfo == null)
            {
                return null;
            }

            byte[] thumbnail = null;
            byte[] data = avatarInfo.AvatarBinary;

            if (ImageHelper.IsImage(avatarInfo.AvatarFileExtension))
            {
                // Get new dimensions
                int originalWidth = avatarInfo.AvatarImageWidth;
                int originalHeight = avatarInfo.AvatarImageHeight;
                int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                // If new thumbnail dimensions are different from the original ones, resize the file
                bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));

                // If no data available, ensure the source data
                if (data == null)
                {
                    data = GetAvatarFile(avatarInfo);
                }

                // Resize the image, create thumbnail
                if ((data != null) && (resize))
                {
                    // Resize image
                    ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                    thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1]);
                }
            }

            // If no thumbnail created, return original data
            return thumbnail ?? data;
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="guid">Guid of the file to get</param>
        /// <param name="height">Image thumbnail width</param>
        /// <param name="width">Image thumbnail height</param>
        public static byte[] GetImageThumbnailFile(Guid guid, int width, int height)
        {
            var filesLocationType = FileHelper.FilesLocationType();

            // files are stored in file system
            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return null;
            }

            // Get avatar file without binary
            AvatarInfo avatarInfo = GetAvatarInfoWithoutBinary(guid);
            if (avatarInfo == null)
            {
                return null;
            }

            byte[] fileContent = null;
            string stringGuid = guid.ToString();

            string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, width, height);

            // Get file path
            string filePath = GetFilePhysicalPath(null, fileName, avatarInfo.AvatarFileExtension);

            // Get the data
            if (File.Exists(filePath))
            {
                fileContent = File.ReadAllBytes(filePath);
            }

            return fileContent;
        }


        /// <summary>
        /// Ensures the thumbnail avatar.
        /// </summary>
        /// <param name="avatarInfo">Avatar info</param>
        /// <param name="width">File width</param>
        /// <param name="height">File height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static string EnsureThumbnailFile(AvatarInfo avatarInfo, int width, int height, int maxSideSize)
        {
            var filesLocationType = FileHelper.FilesLocationType();

            if ((filesLocationType == FilesLocationTypeEnum.Database) || !GenerateThumbnails() || (avatarInfo == null))
            {
                return null;
            }

            // Get new dimensions
            int originalWidth = avatarInfo.AvatarImageWidth;
            int originalHeight = avatarInfo.AvatarImageHeight;
            int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

            // If new thumbnail dimensions are different from the original ones, get resized file
            bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
            if (!resize)
            {
                return EnsureAvatarPhysicalFile(avatarInfo);
            }

            // Check if the file exists
            string stringGuid = avatarInfo.AvatarGUID.ToString();

            string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, newDims[0], newDims[1]);

            // Get file path
            string path = GetFilePhysicalPath(null, fileName, avatarInfo.AvatarFileExtension);
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
                byte[] data = GetImageThumbnail(avatarInfo, newDims[0], newDims[1], 0);
                if (data == null)
                {
                    return null;
                }

                try
                {
                    // Save the data to the disk
                    fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, newDims[0], newDims[1]);
                    SaveAvatarFileToDisk(stringGuid, fileName, avatarInfo.AvatarFileExtension, data, false);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AvatarInfo", "EnsureThumbnailFile", ex);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the enum representation of avatar type based on specified string.
        /// </summary>
        /// <param name="avatarTypeString">String representation of avatar type</param>
        public static AvatarTypeEnum GetAvatarTypeEnum(string avatarTypeString)
        {
            // Get a enum representation and return it as result
            switch (avatarTypeString.ToLowerInvariant())
            {
                case "user":
                    return AvatarTypeEnum.User;

                case "group":
                    return AvatarTypeEnum.Group;

                default:
                    return AvatarTypeEnum.All;
            }
        }


        /// <summary>
        /// Returns the string representation of avatar type based on specified enum value.
        /// </summary>
        /// <param name="avatarTypeEnum">Enum representation of avatar type</param>
        public static string GetAvatarTypeString(AvatarTypeEnum avatarTypeEnum)
        {
            // Get a enum representation and return it as result
            switch (avatarTypeEnum)
            {
                case AvatarTypeEnum.User:
                    return "user";

                case AvatarTypeEnum.Group:
                    return "group";

                default:
                    return "all";
            }
        }


        /// <summary>
        /// Returns AvatarTypeEnum string code derived from type of enumeration and enum value.
        /// </summary>
        public static string GetAvatarTypeString(Type enumType, int value)
        {
            AvatarTypeEnum avatarType = (AvatarTypeEnum)Enum.ToObject(enumType, value);
            return GetAvatarTypeString(avatarType);
        }


        /// <summary>
        /// Ensures the avatar file in the file system and returns the path to the file.
        /// </summary>
        /// <param name="avatarInfo">Avatar info</param>
        public static string EnsureAvatarPhysicalFile(AvatarInfo avatarInfo)
        {
            var filesLocationType = FileHelper.FilesLocationType();

            if ((filesLocationType == FilesLocationTypeEnum.Database) || (avatarInfo == null))
            {
                return null;
            }

            // Check if the file exists
            string path = GetFilePhysicalPath(null, avatarInfo.AvatarGUID.ToString(), avatarInfo.AvatarFileExtension);
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
                byte[] data = avatarInfo.AvatarBinary;
                if (data == null)
                {
                    // Load the data from the database
                    AvatarInfo avatar = GetAvatarInfo(avatarInfo.AvatarID);
                    if (avatar != null)
                    {
                        data = avatar.AvatarBinary;
                    }
                }

                if (data == null)
                {
                    return null;
                }

                try
                {
                    SaveAvatarFileToDisk(avatarInfo.AvatarGUID.ToString(), avatarInfo.AvatarGUID.ToString(), avatarInfo.AvatarFileExtension, data, false);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Avatar", "SaveAvatarFileToDisk", ex);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns physical path to the avatar file folder (avatars files folder path + subfolder).
        /// </summary>
        /// <param name="guid">Avatar file GUID</param>
        private static string GetFileFolder(string guid)
        {
            // Subfolder name start with first two letters of the names of files which are placed in the folder
            string subfolder = AttachmentHelper.GetFileSubfolder(guid);

            return DirectoryHelper.CombinePath(GetFilesFolderPath(), subfolder) + "\\";
        }


        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="extension">File extension</param>
        public static string GetFilePhysicalPath(string siteName, string guid, string extension)
        {
            // Get avatar file physical path
            return GetFileFolder(guid) + AttachmentHelper.GetFullFileName(guid, extension);
        }


        /// <summary>
        /// Returns physical path to folder with avatar files which are associated with the specified site.
        /// </summary>
        public static string GetFilesFolderPath()
        {
            // Get avatar files folder path from the settings
            string filesFolderPath = FileHelper.FilesFolder(null);

            // Folder is not specified in settings -> get default files folder path
            if (filesFolderPath == "")
            {
                // Global avatar files
                filesFolderPath = WebApplicationPhysicalPath.TrimEnd('\\') + "\\CMSFiles\\";
            }
            // Folder is specified in settings -> ensure files folder path
            else
            {
                // Get full physical path
                filesFolderPath = FileHelper.GetFullFolderPhysicalPath(filesFolderPath, WebApplicationPhysicalPath);
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Saves avatar file to the disk.
        /// </summary>
        /// <param name="guid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="fileData">File data (byte[] or Stream)</param>
        /// <param name="synchronization">Indicates if this function is called from "ProcessTask"</param>
        public static void SaveAvatarFileToDisk(string guid, string fileName, string fileExtension, BinaryData fileData, bool synchronization)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                string filesFolderPath = GetFilesFolderPath();

                // Check permission for specified folder
                if (!DirectoryHelper.CheckPermissions(filesFolderPath))
                {
                    throw new PermissionException("[AvatarInfoProvider.SaveFileToDisk]: Access to the path '" + filesFolderPath + "' is denied.");
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
                        WebFarmHelper.CreateIOTask(new UpdateAvatarWebFarmTask
                        {
                            FileGuid = ValidationHelper.GetGuid(guid, Guid.Empty),
                            FileName = fileName,
                            FileExtension = fileExtension,
                            TaskFilePath = filePath,
                            TaskBinaryData = fileData
                        });
                    }

                    fileData.Close();

                    if (synchronization)
                    {
                        // Drop the cache dependencies
                        CacheHelper.TouchKey("avatarfile|" + ValidationHelper.GetString(guid, Guid.Empty.ToString()).ToLowerInvariant(), false, false);
                    }
                }
            }
            else
            {
                throw new Exception("[AvatarInfoProvider.SaveFileToDisk]: GUID of the file is not specified.");
            }

        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="avatarInfo">Avatar file info to check</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static bool CanResizeImage(AvatarInfo avatarInfo, int width, int height, int maxSideSize)
        {
            if (avatarInfo == null)
            {
                return false;
            }

            // Resize only when bigger than required
            if (maxSideSize > 0)
            {
                if ((maxSideSize < avatarInfo.AvatarImageWidth) || (maxSideSize < avatarInfo.AvatarImageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (avatarInfo.AvatarImageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (avatarInfo.AvatarImageHeight > height))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns the current settings whether the thumbnails should be generated.
        /// </summary>
        public static bool GenerateThumbnails()
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSGenerateThumbnails");
        }


        /// <summary>
        /// Uploads and resizes (if needed) avatar to specified AvatarInfo and sets its properties.
        /// </summary>
        public static void UploadAvatar(AvatarInfo ai, HttpPostedFile postedFile, int maxWidth, int maxHeight, int maxSideSize)
        {
            int fileNameStartIndex = postedFile.FileName.LastIndexOf("\\", StringComparison.Ordinal) + 1;

            if (ImageHelper.IsMimeImage(postedFile.ContentType))
            {
                // Set the avatar
                ai.AvatarFileName = URLHelper.GetSafeFileName(postedFile.FileName.Substring(fileNameStartIndex), null);
                ai.AvatarFileExtension = Path.GetExtension(postedFile.FileName);
                ai.AvatarFileSize = postedFile.ContentLength;
                ai.AvatarMimeType = postedFile.ContentType;

                // Make copy of the posted avatar file data
                if ((postedFile.InputStream.CanRead) && (postedFile.InputStream.Position == 0))
                {
                    ai.AvatarBinary = new byte[ai.AvatarFileSize];
                    postedFile.InputStream.Read(ai.AvatarBinary, 0, ai.AvatarFileSize);
                }
                else
                {
                    throw new Exception("[AvatarInfo.AvatarInfo]: Input stream is not at the beginning position.");
                }

                // Set the image properties
                if (ImageHelper.IsImage(ai.AvatarFileExtension))
                {
                    ImageHelper ih = new ImageHelper(ai.AvatarBinary);
                    ai.AvatarImageHeight = ih.ImageHeight;
                    ai.AvatarImageWidth = ih.ImageWidth;

                    if ((maxHeight <= 0) || (maxWidth <= 0))
                    {
                        maxWidth = ih.ImageWidth;
                        maxHeight = ih.ImageHeight;
                    }

                        // Resize image width and height
                    else
                    {
                        // Do not resize to bigger images
                        if (ih.ImageWidth < maxWidth)
                        {
                            maxWidth = ih.ImageWidth;
                        }

                        if (ih.ImageHeight < maxHeight)
                        {
                            maxHeight = ih.ImageHeight;
                        }
                    }

                    // Resize avatar
                    if (CanResizeImage(ai, maxWidth, maxHeight, maxSideSize))
                    {
                        int[] newDims = ImageHelper.EnsureImageDimensions(maxWidth, maxHeight, maxSideSize, ai.AvatarImageWidth, ai.AvatarImageHeight);

                        ai.AvatarBinary = ih.GetResizedImageData(newDims[0], newDims[1]);
                        ai.AvatarImageHeight = ih.ImageHeight;
                        ai.AvatarImageWidth = ih.ImageWidth;
                        ai.AvatarFileSize = ai.AvatarBinary.Length;
                    }
                }
            }
        }


        /// <summary>
        /// Returns unique name for avatar.
        /// </summary>
        /// <param name="avatarName">Avatar name</param>
        public static string GetUniqueAvatarName(string avatarName)
        {
            // Get all similar avatars
            string where = "AvatarName LIKE N'" + SqlHelper.GetSafeQueryString(avatarName, false) + "%'";
            var avatars = GetAvatars().Where(where);

            // Create string fo searching
            string names = ";";
            foreach (var avatar in avatars)
            {
                names += avatar.AvatarName + ";";
            }

            // Get unique avatar name
            int i = 1;
            string safeName = avatarName;
            while (names.Contains(";" + safeName + ";"))
            {
                safeName = avatarName + "_" + i;
                i++;
            }

            return safeName;
        }


        /// <summary>
        /// Returns parameter for default gravatar image
        /// </summary>
        /// <param name="gender">User gender</param>
        /// <param name="maxSideSize">Maximal side size</param>
        /// <param name="siteName">Site name</param>
        public static string GetGravatarDefaultParameter(UserGenderEnum gender, int maxSideSize, string siteName)
        {
            string defaultImg = SettingsKeyInfoProvider.GetValue(siteName + ".CMSGravatarDefaultImage");
            switch (defaultImg)
            {
                case "local":
                    AvatarInfo defAv = GetDefaultAvatar(gender);
                    if (defAv == null)
                    {
                        return null;
                    }
                    string url = URLHelper.ResolveUrl("~/getavatar/" + defAv.AvatarGUID + "/" + maxSideSize + defAv.AvatarFileExtension);

                    // Return encoded URL for our default Avatar image
                    return HttpUtility.UrlEncode(URLHelper.GetAbsoluteUrl(url));

                default:
                    return defaultImg;
            }
        }


        /// <summary>
        /// Gets default avatar info based on user gender
        /// </summary>
        /// <param name="gender">User gender</param>
        public static AvatarInfo GetDefaultAvatar(UserGenderEnum gender)
        {
            AvatarInfo ai;

            switch (gender)
            {
                case UserGenderEnum.Male:
                    ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Male);
                    break;
                case UserGenderEnum.Female:
                    ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Female);
                    break;
                default:
                    ai = GetDefaultAvatar(DefaultAvatarTypeEnum.User);
                    break;
            }

            // If user gender specified and default avatar for that gender does not exist
            if ((ai == null) && (gender == UserGenderEnum.Male || gender == UserGenderEnum.Female))
            {
                ai = GetDefaultAvatar(DefaultAvatarTypeEnum.User);
            }

            return ai;
        }


        /// <summary>
        /// Creates URL for gravatar image
        /// </summary>
        /// <param name="email">User e-mail</param>
        /// <param name="gender">User gender</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="siteName">Site name</param>
        /// <returns>URL of gravatar image</returns>
        public static string CreateGravatarLink(string email, int gender, int maxSideSize, string siteName)
        {
            // Get correct protocol
            string gravatarUrl = RequestContext.IsSSL ? "https://secure." : "http://www.";

            // Create MD5 hash from email
            string hash = SecurityHelper.GetMD5Hash(email.ToLowerInvariant().Trim()).ToLowerInvariant();

            // Prepare parameters
            string parameters = "s=" + maxSideSize + "&r=" + SettingsKeyInfoProvider.GetValue(siteName + ".CMSGravatarRating");
            string defaultGravatar = GetGravatarDefaultParameter((UserGenderEnum)gender, maxSideSize, siteName);
            if (!String.IsNullOrEmpty(defaultGravatar))
            {
                parameters += "&d=" + defaultGravatar;
            }

            // Complete link
            gravatarUrl += "gravatar.com/avatar/" + hash + "?" + parameters;

            return gravatarUrl;
        }

        #endregion


        #region "Avatar URL methods"

        /// <summary>
        /// Returns avatar or gravatar image url, if it is not defined returns gender dependent avatar or user default avatar.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar or user avatar for specified user if avatar given by avatar id doesn't exist</param>
        /// <param name="userEmail">User e-mail</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public static string GetUserAvatarImageUrl(object avatarID, object userID, object userEmail, int maxSideSize, int width, int height)
        {
            return ProviderObject.GetUserAvatarImageUrlInternal(avatarID, userID, userEmail, maxSideSize, width, height);
        }


        /// <summary>
        /// Returns avatar or gravatar image url, if it is not defined returns gender dependent avatar or user default avatar.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar or user avatar for specified user if avatar given by avatar id doesn't exist</param>
        /// <param name="userEmail">User e-mail</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        protected virtual string GetUserAvatarImageUrlInternal(object avatarID, object userID, object userEmail, int maxSideSize, int width, int height)
        {
            string aType = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSAvatarType");

            int userId = ValidationHelper.GetInteger(userID, 0);

            // Get avatar type from current user
            if (aType == USERCHOICE)
            {
                UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                aType = ui != null ? ui.UserSettings.UserAvatarType : GRAVATAR;
            }

            switch (aType)
            {
                case AVATAR:
                    {
                        AvatarInfo ai = null;
                        int avatId = ValidationHelper.GetInteger(avatarID, 0);
                        // Try to get user defined avatar
                        if (avatId > 0)
                        {
                            ai = RequestStockHelper.GetItem("UserAvatarInfo_" + avatId) as AvatarInfo;
                            if (ai == null)
                            {
                                ai = GetAvatarInfoWithoutBinary(avatId);
                                if (ai != null)
                                {
                                    RequestStockHelper.Add("UserAvatarInfo_" + avatId, ai);
                                }
                            }
                        }

                        // Try to get gender depend avatar
                        if (ai == null)
                        {
                            if (userId > 0)
                            {
                                // Get user settings of selected user
                                UserSettingsInfo uSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userId);
                                if (uSettings != null)
                                {
                                    // Try to load avatar from user settings
                                    if (uSettings.UserAvatarID > 0)
                                    {
                                        ai = RequestStockHelper.GetItem("UserAvatarInfo_" + uSettings.UserAvatarID) as AvatarInfo;
                                        if (ai == null)
                                        {
                                            ai = GetAvatarInfoWithoutBinary(uSettings.UserAvatarID);
                                            if (ai != null)
                                            {
                                                RequestStockHelper.Add("UserAvatarInfo_" + uSettings.UserAvatarID, ai);
                                            }
                                        }
                                    }

                                    // Default gender avatar
                                    if (ai == null)
                                    {
                                        switch (uSettings.UserGender)
                                        {
                                            // Male
                                            case 1:
                                                ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Male);
                                                break;
                                            // Female
                                            case 2:
                                                ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Female);
                                                break;
                                        }
                                    }
                                }

                            }
                        }


                        // Try to get user default avatar
                        if (ai == null)
                        {
                            ai = GetDefaultAvatar(DefaultAvatarTypeEnum.User);
                        }

                        // If exists avatar info, generate img tag
                        if (ai != null)
                        {
                            string url = URLHelper.ResolveUrl("~/CMSPages/GetAvatar.aspx");
                            url = URLHelper.AddParameterToUrl(url, "avatarguid", ai.AvatarGUID.ToString());

                            // Max side size
                            if (maxSideSize > 0)
                            {
                                url = URLHelper.AddParameterToUrl(url, "maxsidesize", maxSideSize.ToString());
                            }

                            // Width
                            if (width > 0)
                            {
                                url = URLHelper.AddParameterToUrl(url, "width", width.ToString());
                            }

                            // Height
                            if (height > 0)
                            {
                                url = URLHelper.AddParameterToUrl(url, "height", height.ToString());
                            }

                            return HTMLHelper.EncodeForHtmlAttribute(url);
                        }
                    }
                    break;

                case GRAVATAR:
                    {
                        string email = ValidationHelper.GetString(userEmail, String.Empty);
                        int gender = 0;

                        UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                        // Get user info for email and gender
                        if (ui != null)
                        {
                            email = ui.Email;
                            gender = ui.UserSettings.UserGender;
                        }

                        // Get correct Gravatar link
                        string imageUrl = CreateGravatarLink(email, gender, maxSideSize, SiteContext.CurrentSiteName);

                        // Return complete img tag with gravatar URL
                        return HTMLHelper.EncodeForHtmlAttribute(imageUrl);
                    }
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public static string GetUserAvatarImageForUser(object userID, int maxSideSize, int width, int height, object alt)
        {
            int id = ValidationHelper.GetInteger(userID, 0);

            if (id > 0)
            {

                string aType = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSAvatarType");

                // Get avatar type from current user
                if (aType == USERCHOICE)
                {
                    UserInfo ui = UserInfoProvider.GetUserInfo(id);
                    aType = ui != null ? ui.UserSettings.UserAvatarType : GRAVATAR;
                }

                switch (aType)
                {
                    case AVATAR:
                        {
                            string key = "UserAvatarID_" + id;
                            int avatarId;

                            // Store avatar to the request to minimize the DB access
                            if (RequestStockHelper.Contains(key))
                            {
                                avatarId = ValidationHelper.GetInteger(RequestStockHelper.GetItem(key), 0);
                            }
                            else
                            {
                                avatarId = UserInfoProvider.GetUsersDataWithSettings().WhereEquals("UserID", id).Column("UserAvatarID").GetScalarResult(0);
                                if (avatarId > 0)
                                {
                                    RequestStockHelper.Add(key, avatarId);
                                }
                            }

                            return GetUserAvatarImage(avatarId, userID, maxSideSize, width, height, alt);
                        }

                    case GRAVATAR:
                        {
                            string email = "";
                            int gender = 0;

                            // Get user info for email and gender
                            UserInfo ui = UserInfoProvider.GetUserInfo(id);
                            if (ui != null)
                            {
                                email = ui.Email;
                                gender = ui.UserSettings.UserGender;
                            }

                            // Get correct Gravatar link
                            string imageUrl = CreateGravatarLink(email, gender, maxSideSize, SiteContext.CurrentSiteName);

                            // Alternate text
                            string altText = DataHelper.GetNotEmpty(alt, "Avatar");

                            // Return complete img tag with gravatar URL
                            return "<img alt=\"" + altText + "\" src=\"" + imageUrl + "\" />";
                        }
                }
            }

            return "";
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public static string GetUserAvatarImage(object avatarID, object userID, int maxSideSize, int width, int height, object alt)
        {
            string url = GetUserAvatarImageUrl(avatarID, userID, null, maxSideSize, width, height);

            if (!String.IsNullOrEmpty(url))
            {
                // Alternate text
                string altText = DataHelper.GetNotEmpty(alt, "Avatar");

                return "<img alt=\"" + altText + "\" src=\"" + url + "\" />";
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns user avatar image.
        /// </summary>
        /// <param name="avatarGuid">Avatar GUID</param>
        /// <param name="userGender">Avatar gender</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public static string GetUserAvatarImageByGUID(object avatarGuid, object userGender, int maxSideSize, int width, int height, object alt)
        {
            Guid guid = ValidationHelper.GetGuid(avatarGuid, Guid.Empty);
            int gender = ValidationHelper.GetInteger(userGender, 0);

            if (guid == Guid.Empty)
            {
                AvatarInfo ai = null;
                switch (gender)
                {
                    // Male
                    case 1:
                        ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Male);
                        break;

                    // Female
                    case 2:
                        ai = GetDefaultAvatar(DefaultAvatarTypeEnum.Female);
                        break;
                }

                if (ai == null)
                {
                    ai = GetDefaultAvatar(DefaultAvatarTypeEnum.User);
                }

                if (ai != null)
                {
                    guid = ai.AvatarGUID;
                }
            }

            if (guid != Guid.Empty)
            {
                string url = URLHelper.ResolveUrl("~/CMSPages/GetAvatar.aspx");
                url += "?avatarguid=" + guid;

                // Max side size
                if (maxSideSize > 0)
                {
                    url += "&amp;maxsidesize=" + maxSideSize;
                }

                // Width
                if (width > 0)
                {
                    url += "&amp;width=" + width;
                }

                // Height
                if (height > 0)
                {
                    url += "&amp;height=" + height;
                }

                return "<img alt=\"" + DataHelper.GetNotEmpty(alt, "Avatar") + "\" src=\"" + url + "\" />";
            }

            return "";
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns default avatar of specified type or null if such is not set.
        /// </summary>
        /// <param name="type">Default avatar type</param>
        public AvatarInfo GetDefaultAvatarInternal(DefaultAvatarTypeEnum type)
        {
            // Try to find default avatar in cache
            if (DefaultAvatars[type] != null)
            {
                return DefaultAvatars[type];
            }

            // Try to find in database
            var where = new WhereCondition();

            // Different where condition by type
            switch (type)
            {
                case DefaultAvatarTypeEnum.Female:
                    where = where.WhereEquals("DefaultFemaleUserAvatar", 1);
                    break;

                case DefaultAvatarTypeEnum.Group:
                    where = where.WhereEquals("DefaultGroupAvatar", 1);
                    break;

                case DefaultAvatarTypeEnum.Male:
                    where = where.WhereEquals("DefaultMaleUserAvatar", 1);
                    break;

                case DefaultAvatarTypeEnum.User:
                    where = where.WhereEquals("DefaultUserAvatar", 1);
                    break;
            }

            var avatar = GetAvatars().Where(where).TopN(1).BinaryData(false).FirstOrDefault();
            if (avatar != null)
            {
                // Store in cache
                DefaultAvatars[type] = avatar;
            }

            return avatar;
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarName">Avatar name</param>
        protected virtual AvatarInfo GetAvatarInfoInternal(string avatarName)
        {
            return GetObjectQuery().WhereEquals("AvatarName", avatarName).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarId">Avatar id</param>
        protected virtual AvatarInfo GetAvatarInfoWithoutBinaryInternal(int avatarId)
        {
            return GetObjectQuery().WhereEquals("AvatarID", avatarId).TopN(1).BinaryData(false).FirstOrDefault();
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="guid">GUID of the avatar to return</param>
        protected virtual AvatarInfo GetAvatarInfoWithoutBinaryInternal(Guid guid)
        {
            return GetObjectQuery().WhereEquals("AvatarGUID", guid).TopN(1).BinaryData(false).FirstOrDefault();
        }


        /// <summary>
        /// Returns the AvatarInfo structure for the specified avatar.
        /// </summary>
        /// <param name="avatarName">Avatar name</param>
        protected virtual AvatarInfo GetAvatarInfoWithoutBinaryInternal(string avatarName)
        {
            return GetObjectQuery().WhereEquals("AvatarName", avatarName).TopN(1).BinaryData(false).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified avatar.
        /// </summary>
        /// <param name="avatar">Avatar to set</param>
        protected virtual void SetAvatarInfoInternal(AvatarInfo avatar)
        {
            if (avatar != null)
            {
                var filesLocationType = FileHelper.FilesLocationType();

                // Indicates whether files should be stored in database
                bool storeInDatabase = filesLocationType != FilesLocationTypeEnum.FileSystem;

                // Indicates whether avatar binary data was empty
                bool avatarBinaryWasEmpty = avatar.AvatarBinary == null;

                // If the avatar image should be stored within database make sure binary data are present
                byte[] data = null;

                if (storeInDatabase || (avatar.AvatarBinary == null) || WebFarmHelper.WebFarmEnabled)
                {
                    data = avatar.Generalized.EnsureBinaryData();
                }

                if (filesLocationType == FilesLocationTypeEnum.FileSystem)
                {
                    // Files are being stored in the FS - look for the binary
                    if ((data == null) && (avatar.AvatarBinary != null))
                    {
                        data = avatar.AvatarBinary;
                    }
                    else if ((data == null) && (avatar.AvatarBinary == null))
                    {
                        // Try to load the file from FS
                        data = GetAvatarFile(avatar);
                    }

                    // Clear the binary column
                    avatar.AvatarBinary = null;
                }

                // Save memory
                if (data == null)
                {
                    data = avatar.AvatarBinary;
                }

                // Try to load file from FS
                if (data == null)
                {
                    data = GetAvatarFile(avatar);
                }

                // Update avatar image dimension info
                if (data != null)
                {
                    ImageHelper ih = new ImageHelper(data);
                    avatar.AvatarImageHeight = ih.ImageHeight;
                    avatar.AvatarImageWidth = ih.ImageWidth;
                }

                BinaryData newBinaryData = data;
                var oldInfo = GetAvatarInfo(avatar.AvatarID);
                var oldChecksum = oldInfo?.BinaryDataChecksum;
                var newChecksum = newBinaryData?.Checksum;
                var dataChanged = !avatarBinaryWasEmpty && (oldChecksum != newChecksum);

                // Use transaction
                using (var tr = BeginTransaction())
                {
                    if (avatar.AvatarID > 0)
                    {
                        if ((avatar.AvatarBinary == null) && (storeInDatabase))
                        {
                            avatar.AvatarBinary = data;
                        }
                    }

                    SetInfo(avatar);

                    var guid = avatar.AvatarGUID.ToString();

                    if (filesLocationType != FilesLocationTypeEnum.Database)
                    {
                        // Save file to disk
                        if (dataChanged)
                        {
                            DeleteAvatarFile(guid, avatar.AvatarFileExtension, false, false);

                            if (data != null)
                            {
                                // Save using memory data
                                SaveAvatarFileToDisk(guid, guid, avatar.AvatarFileExtension, data, false);
                            }
                        }
                        else
                        {
                            UpdatePhysicalFileLastWriteTime(guid, avatar.AvatarFileExtension);
                        }
                    }

                    // Commit transaction
                    tr.Commit();
                }
            }
            else
            {
                throw new Exception("[AvatarInfoProvider.SetAvatarInfo]: No AvatarInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified avatar.
        /// </summary>
        /// <param name="infoObj">Avatar object</param>
        protected virtual void DeleteAvatarInfoInternal(AvatarInfo infoObj)
        {
            if (infoObj != null)
            {
                // Clear default avatars cache
                DefaultAvatars.Clear();

                DeleteInfo(infoObj);
            }
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            // Switch by action name
            switch (actionName)
            {
                // Clear default avatar
                case "cleardefaultavatar":
                    DefaultAvatarTypeEnum avatarType = (DefaultAvatarTypeEnum)Enum.Parse(typeof(DefaultAvatarTypeEnum), data);
                    ClearDefaultAvatar(avatarType, true, false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}
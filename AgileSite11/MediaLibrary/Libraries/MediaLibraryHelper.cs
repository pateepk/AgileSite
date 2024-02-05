using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Synchronization;
using CMS.Membership;

using Directory = CMS.IO.Directory;
using DirectoryInfo = CMS.IO.DirectoryInfo;
using File = CMS.IO.File;
using FileInfo = CMS.IO.FileInfo;
using Path = CMS.IO.Path;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class providing helper methods for media library.
    /// </summary>
    public class MediaLibraryHelper
    {
        #region "Variables"

        /// <summary>
        /// Object type for abstract media folder.
        /// </summary>
        public const string OBJECT_TYPE_FOLDER = "media.folder";


        private static Regex mSuffixRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Suffix regex for files and folders.
        /// </summary>
        public static Regex SuffixRegex
        {
            get
            {
                return mSuffixRegex ?? (mSuffixRegex = RegexHelper.GetRegex("(?:[_](\\d+))$"));
            }
            set
            {
                mSuffixRegex = value;
            }
        }


        /// <summary>
        /// Custom path to media library.
        /// </summary>
        public static string MediaLibraryCustomPath
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Return file path with replaced slash ("/") to back slash ("\").
        /// </summary>
        /// <param name="path">File path</param>
        public static string EnsurePhysicalPath(string path)
        {
            if (path != null)
            {
                return Path.EnsureBackslashes(path);
            }

            return String.Empty;
        }


        /// <summary>
        /// Ensures unique path for the specified directory. If the directory with the specified name already exist in the target location 
        /// new path with the added suffix is returned. The suffix consist of '_' sign and the number.
        /// </summary>
        /// <param name="path">Directory path to ensure</param>
        public static string EnsureUniqueDirectory(string path)
        {
            // Ensure physical path
            path = EnsurePhysicalPath(path);

            if (Directory.Exists(path))
            {
                path = path.TrimEnd('\\');
                string directoryName = Path.GetFileName(path);
                string directoryPath = Path.GetDirectoryName(path);

                int index = directoryName.LastIndexOfCSafe('_');

                // Create search pattern for current file
                string searchPattern = (index != -1) && ValidationHelper.IsInteger(directoryName.Substring(index + 1)) ? SuffixRegex.Replace(directoryName, "_*") : directoryName + "_*";

                List<string> foundDirs = new List<string>();
                foundDirs.AddRange(Directory.GetDirectories(directoryPath, searchPattern));
                if (foundDirs.Count > 0)
                {
                    // Check directory pattern
                    string checkPath = DirectoryHelper.CombinePath(directoryPath, SuffixRegex.Replace(directoryName, String.Empty)) + "_{0}";
                    // Loop trough all founded dirs
                    for (int i = 1; i < foundDirs.Count; i++)
                    {
                        string currentPath = String.Format(checkPath, i);
                        // If directory is not in list return this directory name as unique
                        if (!foundDirs.Contains(currentPath))
                        {
                            return currentPath;
                        }
                    }
                    // Return next directory name as unique
                    return String.Format(checkPath, foundDirs.Count + 1);
                }
                else
                {
                    return DirectoryHelper.CombinePath(directoryPath, directoryName) + "_1";
                }
            }
            return path;
        }


        /// <summary>
        /// Ensures unique path for the specified file. If the directory with the specified name already exist in the target location 
        /// new path with the added suffix is returned. The suffix consist of '_' sign and the number.
        /// </summary>
        /// <param name="path">Complete path to the file to ensure</param>
        public static string EnsureUniqueFileName(string path)
        {
            // Ensure physical path
            path = EnsurePhysicalPath(path);

            if (File.Exists(path))
            {
                string ext = Path.GetExtension(path);
                string directoryPath = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);

                int index = filename.LastIndexOfCSafe('_');

                // Create search pattern for current file
                string searchPattern = (index != -1) && (ValidationHelper.GetInteger(filename.Substring(index + 1), 0) != 0) ? SuffixRegex.Replace(filename, "_*") : filename + "_*";

                // Find matching files and convert its paths to lower case, because comparison will be made case insensitive
                HashSet<string> foundFiles = new HashSet<string>(Directory.GetFiles(directoryPath, searchPattern).Select(s => Path.GetFileName(s.ToLowerCSafe())));

                if (foundFiles.Count > 0)
                {
                    // Check file pattern used to construct final file path
                    string outputPattern = String.Format("{0}_{{0}}{1}", SuffixRegex.Replace(filename, String.Empty), ext);

                    // Checking will be done case insensitive, so convert pattern to lower case
                    string outputPatternLower = outputPattern.ToLowerCSafe();

                    // Loop through all founded files
                    for (int i = 1; i <= foundFiles.Count; i++)
                    {
                        string currentPath = String.Format(outputPatternLower, i);

                        // If file is not in list return this file name as unique
                        if (!foundFiles.Contains(currentPath))
                        {
                            // Return file made with the original pattern (preserves letter case in path)
                            return DirectoryHelper.CombinePath(directoryPath, String.Format(outputPattern, i));
                        }
                    }
                    // Return next file name as unique
                    return DirectoryHelper.CombinePath(directoryPath, String.Format(outputPattern, foundFiles.Count + 1));
                }
                else
                {
                    return String.Format("{0}_1{1}", DirectoryHelper.CombinePath(directoryPath, filename), ext);
                }
            }
            return path;
        }


        /// <summary>
        /// Returns media file preview suffix from settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetMediaFilePreviewSuffix(string siteName)
        {
            string previewSuffix = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMediaFilePreviewSuffix");
            if (String.IsNullOrEmpty(previewSuffix))
            {
                throw new Exception("[MediaLibraryHelper.GetMediaFilePreviewSuffix]: Settings key \"CMSMediaFilePreviewSuffix\" is empty for site \"" + siteName + "\"!");
            }
            return previewSuffix;
        }


        /// <summary>
        /// Returns media file hidden folder name from settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetMediaFileHiddenFolder(string siteName)
        {
            string hiddenFolder = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMediaFileHiddenFolder");
            if (String.IsNullOrEmpty(hiddenFolder))
            {
                throw new Exception("[MediaLibraryHelper.GetMediaFileHiddenFolder]: Settings key \"CMSMediaFileHiddenFolder\" is empty for site \"" + siteName + "\"!");
            }
            return hiddenFolder;
        }


        /// <summary>
        /// Returns root folder from settings where all media libraries are stored.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetMediaLibrariesFolder(string siteName)
        {
            return !String.IsNullOrEmpty(MediaLibraryCustomPath) ? MediaLibraryCustomPath : SettingsKeyInfoProvider.GetValue(siteName + ".CMSMediaLibrariesFolder").TrimEnd('/');
        }


        /// <summary>
        /// Returns true if media library root folder is outside of CMS.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="libraryFolder">Library folder.</param>
        public static bool IsExternalLibrary(string siteName, string libraryFolder)
        {
            // Check whether is path remapped to different location
            string path = Path.EnsureEndBackslash(GetMediaRootFolderPath(siteName)) + libraryFolder;

            AbstractStorageProvider provider = StorageHelper.GetStorageProvider(path);

            // Special URL is always used for external storage so link should be visible all the time
            if (provider.IsExternalStorage)
            {
                return false;
            }

            // In case of System.IO provider always indicate library as external
            if (!String.IsNullOrEmpty(provider.CustomRootPath))
            {
                return true;
            }

            string libraryRoot = GetMediaLibrariesFolder(siteName);
            if (!String.IsNullOrEmpty(libraryRoot))
            {
                if (libraryRoot.StartsWithCSafe("~/"))
                {
                    return false;
                }

                if (Path.IsPathRooted(libraryRoot))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns media file preview file name.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="previewExtension">File preview extension</param>
        /// <param name="siteName">Site name</param>
        /// <param name="previewSuffix">Preview suffix</param>
        public static string GetPreviewFileName(string fileName, string fileExtension, string previewExtension, string siteName, string previewSuffix = null)
        {
            if (previewSuffix == null)
            {
                previewSuffix = GetMediaFilePreviewSuffix(siteName);
            }

            if ((fileName != null) && (fileExtension != null) && (previewExtension != null) && (previewSuffix != null))
            {
                return String.Format("{0}_{1}{2}.{3}", fileName, fileExtension.ToLowerCSafe().TrimStart('.'), previewSuffix, previewExtension.TrimStart('.'));
            }
            return null;
        }


        /// <summary>
        /// Returns media file preview file path.
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        public static string GetPreviewFilePath(MediaFileInfo fileInfo)
        {
            if (fileInfo != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
                if (si != null)
                {
                    string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(fileInfo.FileLibraryID);
                    string filePath = Path.GetDirectoryName(fileInfo.FilePath);
                    string hiddenFolder = GetMediaFileHiddenFolder(si.SiteName);

                    string folderPath = DirectoryHelper.CombinePath(libraryPath, (!String.IsNullOrEmpty(filePath) ? filePath : String.Empty), hiddenFolder);
                    if (Directory.Exists(folderPath))
                    {
                        string previewName = GetPreviewFileName(fileInfo.FileName, fileInfo.FileExtension, "*", si.SiteName);
                        string[] files = Directory.GetFiles(folderPath, previewName);
                        if ((files != null) && (files.Length > 0))
                        {
                            return files[0];
                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Moves preview file into new location according new media file name.
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        /// <param name="newName">New media file path</param>
        public static void MoveMediaFilePreview(MediaFileInfo fileInfo, string newName)
        {
            if (fileInfo != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
                if (si != null)
                {
                    string previewPath = GetPreviewFilePath(fileInfo);
                    if (previewPath != null)
                    {
                        // Get file name and extension from new file name
                        string fileName = Path.GetFileNameWithoutExtension(newName);
                        string fileExt = Path.GetExtension(newName);

                        // Ensure safe file name
                        fileName = URLHelper.GetSafeFileName(fileName, si.SiteName);

                        string newPreviewPath = DirectoryHelper.CombinePath(Path.GetDirectoryName(previewPath), GetPreviewFileName(fileName, fileExt, Path.GetExtension(previewPath), si.SiteName));

                        File.Move(previewPath, newPreviewPath);
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Recursive copy media library directory.
        /// </summary>
        /// <param name="libraryID">Library ID</param>
        /// <param name="dir">Directory Info</param>
        /// <param name="destinationDirectory">Destination path</param>
        /// <param name="sourcePath">Source DB path</param>
        /// <param name="libraryPathIndex">Library path index</param>
        /// <param name="startingPoint">Starting point</param>
        /// <param name="copyDB">Indicate if database entries should copy</param>
        /// <param name="userId">ID of the user performing action</param>
        public static void CopyRecursive(int libraryID, DirectoryInfo dir, string destinationDirectory, string sourcePath, int libraryPathIndex, string startingPoint, bool copyDB, int userId)
        {
            CopyRecursive(libraryID, libraryID, dir, destinationDirectory, sourcePath, libraryPathIndex, startingPoint, copyDB, userId);
        }


        /// <summary>
        /// Recursive copy media library directory.
        /// </summary>
        /// <param name="sourceLibraryID">Source library ID</param>
        /// <param name="destinationLibraryID">Destination library ID</param>
        /// <param name="dir">Directory Info</param>
        /// <param name="destinationDirectory">Destination path</param>
        /// <param name="sourcePath">Source DB path</param>
        /// <param name="libraryPathIndex">Library path index</param>
        /// <param name="startingPoint">Starting point</param>
        /// <param name="copyDB">Indicate if database entries should copy</param>
        /// <param name="userId">ID of the user performing action</param>
        /// <param name="overwrite">Indicates if the destination folder and files can be overwritten</param>
        /// <param name="cloneSettings">MediaLibrary clone settings. Only some parameters are used. If null, cloning is not used and FileMediaInfo is inserted as a directly.</param>
        /// <param name="cloneResult">Results of cloning will be stored to this instance</param>
        public static void CopyRecursive(int sourceLibraryID, int destinationLibraryID, DirectoryInfo dir, string destinationDirectory, string sourcePath, int libraryPathIndex, string startingPoint, bool copyDB, int userId, bool overwrite = false, CloneSettings cloneSettings = null, CloneResult cloneResult = null)
        {
            CopyRecursiveInternal(sourceLibraryID, destinationLibraryID, dir, destinationDirectory, sourcePath, libraryPathIndex, startingPoint, copyDB, userId, overwrite, cloneSettings, cloneResult);
        }


        /// <summary>
        /// Recursive copy media library directory.
        /// </summary>
        /// <param name="sourceLibraryID">Source library ID</param>
        /// <param name="destinationLibraryID">Destination library ID</param>
        /// <param name="dir">Directory Info</param>
        /// <param name="destinationDirectory">Destination path</param>
        /// <param name="sourcePath">Source DB path</param>
        /// <param name="libraryPathIndex">Library path index</param>
        /// <param name="startingPoint">Starting point</param>
        /// <param name="copyDB">Indicate if database entries should copy</param>
        /// <param name="userId">ID of the user performing action</param>
        /// <param name="overwrite">Indicates if the destination folder and files can be overwritten</param>
        /// <param name="cloneSettings">MediaLibrary clone settings. Only some parameters are used. If null, cloning is not used and FileMediaInfo is inserted as a directly.</param>
        /// <param name="cloneResult">Results of cloning will be stored to this instance</param>
        /// <param name="fileGUIDs">List of original file GUIDs and their copied ones when staging is used</param>
        internal static void CopyRecursiveInternal(int sourceLibraryID, int destinationLibraryID, DirectoryInfo dir, string destinationDirectory, string sourcePath, int libraryPathIndex, string startingPoint, bool copyDB, int userId, bool overwrite = false, CloneSettings cloneSettings = null, CloneResult cloneResult = null, Dictionary<Guid, Guid> fileGUIDs = null)
        {
            destinationDirectory = Path.EnsureSlashes(destinationDirectory);
            sourcePath = Path.EnsureSlashes(sourcePath).TrimStart('/');
            startingPoint = Path.EnsureSlashes(startingPoint);

            // Get files
            FileInfo[] fis = dir.GetFiles();

            // Check if destination directory exist
            if (!overwrite && Directory.Exists(destinationDirectory))
            {
                // Get unique directory path
                destinationDirectory = EnsureUniqueDirectory(destinationDirectory);
            }

            LogContext.AppendLine(destinationDirectory, "MediaLibrary");
                            
            // Create the destination directory
            Directory.CreateDirectory(destinationDirectory);

            foreach (FileInfo fi in fis)
            {
                if (DirectoryHelper.CheckPermissions(fi.FullName, true, false, false, false))
                {
                    if (copyDB)
                    {
                        String fileDBPath = String.Format("{0}/{1}", sourcePath.Trim('/'), fi.Name).TrimStart('/');

                        MediaFileInfo fileInfo = MediaFileInfoProvider.GetMediaFileInfo(sourceLibraryID, fileDBPath);

                        if (fileInfo != null)
                        {                            
                            // Prepare new file path
                            var newFilePath = String.Format("{0}/{1}", Path.EnsureSlashes(destinationDirectory.Substring(libraryPathIndex)).Trim('/'), fi.Name).TrimStart('/');

                            LogContext.AppendLine(newFilePath, "MediaLibrary");
                            
                            // Create clone and save it as new file
                            MediaFileInfo fileCopy = fileInfo.Clone(true);

                            if ((fileGUIDs != null) && fileGUIDs.ContainsKey(fileInfo.FileGUID))
                            {
                                // Ensure correct GUID for synchronized files
                                fileCopy.FileGUID = fileGUIDs[fileInfo.FileGUID];
                            }

                            // Do not log synchronization for each file
                            fileCopy.Generalized.LogSynchronization = SynchronizationTypeEnum.None;

                            fileCopy.FilePath = newFilePath;

                            // Correct library
                            fileCopy.FileLibraryID = destinationLibraryID;

                            // Set new media file info
                            if (cloneSettings != null)
                            {
                                // Store previous setting of CodeName and DisplayName and set it to null (so DisplayName and CodeName for new file will be generated automatically)
                                string originalCodeName = cloneSettings.CodeName;
                                cloneSettings.CodeName = null;
                                string originalDisplayName = cloneSettings.DisplayName;
                                cloneSettings.DisplayName = null;

                                fileCopy.Generalized.InsertAsClone(cloneSettings, cloneResult);

                                // Set previous CodeName and DisplayName back to the settings instance
                                cloneSettings.CodeName = originalCodeName;
                                cloneSettings.DisplayName = originalDisplayName;
                            }
                            else
                            {
                                MediaFileInfoProvider.SetMediaFileInfo(fileCopy, true, userId);
                            }
                        }
                    }

                    fi.CopyTo(DirectoryHelper.CombinePath(destinationDirectory, fi.Name), overwrite);
                }
            }

            // Recursive copy children directories
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                // If read permission on directory
                if (DirectoryHelper.CheckPermissions(di.FullName, true, false, false, false))
                {
                    // If destination is different from current directory path
                    if ((di.FullName.ToLowerCSafe() != destinationDirectory.ToLowerCSafe()) && (di.FullName.ToLowerCSafe() != EnsurePhysicalPath(startingPoint.ToLowerCSafe())))
                    {
                        CopyRecursive(sourceLibraryID, destinationLibraryID, di, DirectoryHelper.CombinePath(destinationDirectory, di.Name), String.Format("{0}/{1}", sourcePath, di.Name), libraryPathIndex, startingPoint, copyDB, userId, overwrite, cloneSettings, cloneResult);
                    }
                }
            }
        }


        /// <summary>
        /// Clone media library files and folder to new media library.
        /// </summary>
        /// <param name="sourceLibraryID">Source library ID</param>
        /// <param name="destinationLibraryID">Destination library ID</param>
        /// <param name="cloneSettings">MediaLibrary clone settings. Only some parameters are used</param>
        /// <param name="cloneResult">Results of cloning will be stored to this instance</param>
        public static void CloneLibraryFiles(int sourceLibraryID, int destinationLibraryID, CloneSettings cloneSettings, CloneResult cloneResult)
        {
            MediaLibraryInfo destLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo(destinationLibraryID);
            if (destLibrary != null)
            {
                string srcRootFolder = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(sourceLibraryID);
                string destRootFolder = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(destinationLibraryID);
                DirectoryInfo dirInfo = DirectoryInfo.New(srcRootFolder);

                CopyRecursive(sourceLibraryID, destinationLibraryID, dirInfo, destRootFolder, String.Empty, destRootFolder.Length, destRootFolder, true, MembershipContext.AuthenticatedUser.UserID, true, cloneSettings, cloneResult);
            }
        }


        /// <summary>
        /// Returns true if file has preview file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="filePath">File path within library</param>
        public static bool HasPreview(string siteName, int libraryID, string filePath)
        {
            string path;

            // Get preview directory path
            if (String.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            {
                path = DirectoryHelper.CombinePath(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(libraryID), GetMediaFileHiddenFolder(siteName));
            }
            else
            {
                path = DirectoryHelper.CombinePath(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(libraryID), Path.GetDirectoryName(filePath), GetMediaFileHiddenFolder(siteName));
            }

            // Get search pattern
            string previewPattern = GetPreviewFileName(Path.GetFileNameWithoutExtension(filePath), Path.GetExtension(filePath), String.Empty, siteName);

            if ((!String.IsNullOrEmpty(path)) && (!String.IsNullOrEmpty(previewPattern)) && (Directory.Exists(path)))
            {
                // Ensure preview path length
                string previewPath = String.Format("{0}{1}*", path, previewPattern.TrimEnd('.'));
                if (previewPath.Length < 260)
                {
                    // Get preview file
                    string[] preview = Directory.GetFiles(path, previewPattern.TrimEnd('.') + "*");
                    if ((preview != null) && (preview.Length > 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Returns physical path to the directory where all media libraries are stored.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaRootFolderPath(string siteName, string webFullPath = null)
        {
            // get files folder path from the settings key
            string filesFolderPath = GetMediaLibrariesFolder(siteName);

            if (String.IsNullOrEmpty(webFullPath))
            {
                webFullPath = SystemContext.WebApplicationPhysicalPath;
            }

            // if settings key is not specified -> get default files folder path
            if (filesFolderPath == String.Empty)
            {
                // Use site name folder always to ensure backward compatibility when no custom folder path is specified
                filesFolderPath = DirectoryHelper.CombinePath(webFullPath, siteName, "media") + "\\";
            }
            else
            {
                // Path is relative, for example: '~/filefolder', '/filefolder', 'filefolder'
                if (!Path.IsPathRooted(filesFolderPath) || (filesFolderPath.StartsWithCSafe("/") && !filesFolderPath.StartsWithCSafe("//")))
                {
                    // Keep these methods separated because path '~/~filefolder' is allowed
                    filesFolderPath = filesFolderPath.TrimStart('~');
                    filesFolderPath = filesFolderPath.TrimStart('/');
                    filesFolderPath = DirectoryHelper.CombinePath(webFullPath, EnsurePhysicalPath(filesFolderPath)) + "\\";
                }
                // Path is absolute, for example: 'c:\filefolder', 'c:\filefolder\'
                // or path is for another server, for example //server/media
                else
                {
                    filesFolderPath = Path.EnsureEndBackslash(filesFolderPath);
                }

                // Check if site specific folder should be used
                if (UseMediaLibrariesSiteFolder(siteName))
                {
                    filesFolderPath += siteName + "\\";
                }
            }

            return filesFolderPath;
        }


        /// <summary>
        /// Returns allowed extensions list from settings.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static string GetAllowedExtensions(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSMediaFileAllowedExtensions");
        }


        /// <summary>
        /// Gets the value that indicates if site-specific folder should be used for media files physical files.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseMediaLibrariesSiteFolder(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseMediaLibrariesSiteFolder");
        }


        /// <summary>
        /// Determines whether the file with the specified extension (case insensitive) can be uploaded into library module on site specified by name.
        /// </summary>
        /// <param name="extension">File extension to check</param>
        /// <param name="siteName">File extension to check</param>
        public static bool IsExtensionAllowed(string extension, string siteName = "")
        {
            if (String.IsNullOrEmpty(siteName))
            {
                siteName = SiteContext.CurrentSiteName;
            }

            extension = extension.ToLowerCSafe();

            // Get the allowed extensions from the settings
            string globalExtensions = GetAllowedExtensions(siteName);
            globalExtensions = globalExtensions.ToLowerCSafe();

            // No extensions specified - all extensions allowed by default
            if (globalExtensions.Trim() == String.Empty)
            {
                return true;
            }

            if (extension == String.Empty)
            {
                // Handle empty extension
                return (globalExtensions.Contains(";;") || globalExtensions.StartsWithCSafe(";") || globalExtensions.EndsWithCSafe(";"));
            }

            globalExtensions = String.Format(";{0};", globalExtensions);
            return (globalExtensions.Contains(String.Format(";{0};", extension)) || globalExtensions.Contains(";." + extension + ";"));
        }

        
        /// <summary>
        /// Returns access denied message for specified permission.
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        public static string GetAccessDeniedMessage(string permissionName)
        {
            string output = null;
            switch (permissionName.ToLowerCSafe())
            {
                case "libraryaccess":
                    output = ResHelper.GetString("media.security.noaccess");
                    break;

                case "filecreate":
                    output = ResHelper.GetString("media.security.nofilecreate");
                    break;

                case "filedelete":
                    output = ResHelper.GetString("media.security.nofiledelete");
                    break;

                case "filemodify":
                    output = ResHelper.GetString("media.security.nofilemodify");
                    break;

                case "foldercreate":
                    output = ResHelper.GetString("media.security.nofoldercreate");
                    break;

                case "folderdelete":
                    output = ResHelper.GetString("media.security.nofolderdelete");
                    break;

                case "foldermodify":
                    output = ResHelper.GetString("media.security.nofoldermodify");
                    break;

                case "manage":
                    output = ResHelper.GetString("media.security.nomanage");
                    break;
            }

            return String.Format("{0} {1}", output, String.Format(ResHelper.GetString("general.accessdeniedonpermissionname"), permissionName));
        }

        
        /// <summary>
        /// Returns media file URL according to site settings.
        /// </summary>
        /// <param name="fileGuid">Media file GUID</param>
        /// <param name="siteName">Site name</param>
        public static string GetMediaFileUrl(Guid fileGuid, string siteName)
        {
            string outUrl = null;

            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                MediaFileInfo mfi = MediaFileInfoProvider.GetMediaFileInfo(fileGuid, siteName);
                if (mfi != null)
                {
                    MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(mfi.FileLibraryID);
                    if (mli != null)
                    {
                        bool usePermanentURL = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSMediaUsePermanentURLs");
                        if (SiteContext.CurrentSiteName != siteName)
                        {
                            // If permanent URLs should be generated
                            if (usePermanentURL)
                            {
                                // URL in format 'http://domainame/getmedia/123456-25245-45454-45455-5455555545/testfile.png'
                                outUrl = MediaFileInfoProvider.GetMediaFileAbsoluteUrl(siteName, fileGuid, mfi.FileName);
                            }
                            else
                            {
                                // URL in format 'http://domainame/cms/EcommerceSite/media/testlibrary/folder1/testfile.png'
                                outUrl = MediaFileInfoProvider.GetMediaFileUrl(si.SiteName, mli.LibraryFolder, mfi.FilePath);
                                outUrl = URLHelper.GetAbsoluteUrl(outUrl, si.DomainName, URLHelper.GetApplicationUrl(si.DomainName), RequestContext.URL.AbsolutePath);
                            }
                        }
                        else
                        {
                            if (usePermanentURL)
                            {
                                // URL in format '/cms/getmedia/123456-25245-45454-45455-5455555545/testfile.png'
                                outUrl = MediaFileInfoProvider.GetMediaFileUrl(mfi.FileGUID, mfi.FileName);
                            }
                            else
                            {
                                // URL in format '/cms/EcommerceSite/media/testlibrary/folder1/testfile.png'
                                outUrl = MediaFileInfoProvider.GetMediaFileUrl(SiteContext.CurrentSiteName, mli.LibraryFolder, mfi.FilePath);
                            }
                        }
                    }
                }
            }

            return outUrl;
        }

        #endregion


        #region "Staging methods"

        /// <summary>
        /// Logs the synchronization task for media folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <returns>Returns new synchronization task</returns>
        public static StagingTaskInfo LogSynchronization(string siteName, int libraryId, string sourcePath, string targetPath, TaskTypeEnum taskType, bool runAsync)
        {
            return LogSynchronization(siteName, libraryId, sourcePath, targetPath, taskType, SynchronizationInfoProvider.ENABLED_SERVERS, runAsync);
        }


        /// <summary>
        /// Logs the synchronization task for media folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="serverId">Server ID to synchronize</param>
        /// <returns>Returns new synchronization task</returns>
        public static StagingTaskInfo LogSynchronization(string siteName, int libraryId, string sourcePath, string targetPath, TaskTypeEnum taskType, int serverId)
        {
            return LogSynchronization(siteName, libraryId, sourcePath, targetPath, taskType, serverId, true);
        }


        /// <summary>
        /// Logs the synchronization task for media folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="taskType">Task type</param>
        /// <param name="serverId">Server ID to synchronize</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <returns>Returns new synchronization task</returns>
        public static StagingTaskInfo LogSynchronization(string siteName, int libraryId, string sourcePath, string targetPath, TaskTypeEnum taskType, int serverId, bool runAsync)
        { 
            var settings = new LogMediaLibraryChangeSettings
            {
                SiteName = siteName,
                LibraryID = libraryId,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                TaskType = taskType,
                ServerID = serverId,
                RunAsynchronously = runAsync
            };

            return LogSynchronization(settings);
        }


        /// <summary>
        /// Logs the synchronization task for media folder.
        /// </summary>
        /// <param name="settings">Settings for staging and integration bus task logging</param>
        /// <returns>Returns new synchronization task</returns>
        internal static StagingTaskInfo LogSynchronization(LogMediaLibraryChangeSettings settings)
        {
            // Ensure correct path
            var sourcePath = Path.EnsureSlashes(settings.SourcePath);
            var targetPath = Path.EnsureSlashes(settings.TargetPath);

            // Get site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(settings.SiteName);
            if (si == null)
            {
                throw new Exception("[MediaLibraryHelper.LogSynchronization]: Site not found.");
            }

            // Log only if synchronization enabled
            if ((settings.ServerID != SynchronizationInfoProvider.ENABLED_SERVERS) || (StagingTaskInfoProvider.LogObjectChanges(settings.SiteName) && ServerInfoProvider.IsEnabledServer(si.SiteID)))
            {
                settings.InitUserAndTaskGroups();

                if (settings.RunAsynchronously && CMSActionContext.CurrentAllowAsyncActions && !TaskHelper.IsExcludedAsyncTask(settings.TaskType))
                {
                    // Check media library ID
                    if ((settings.TaskType != TaskTypeEnum.DeleteMediaFolder) && (settings.LibraryID == 0))
                    {
                        throw new Exception("[FolderTaskWorker]: Media library ID is not specified.");
                    }

                    SynchronizationQueueWorker.Current.Enqueue(
                        TextHelper.Merge("|", "FolderTaskWorker", settings.TaskType, sourcePath, targetPath, settings.LibraryID, settings.ServerID, settings.SiteName), 
                        () => 
                            {
                                settings.WorkerCall = true;
                                settings.RunAsynchronously = false;

                                LogSynchronization(settings);
                            }
                        );
                }
                else
                {
                    try
                    {
                        // Get media library
                        MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(settings.LibraryID);

                        // Lock on the object instance to ensure only single running logging for the object
                        object locker = (mli != null) ? mli.Generalized.GetLockObject() : LockHelper.GetLockObject(String.Format("{0}_{1}", MediaLibraryInfo.OBJECT_TYPE, settings.LibraryID));
                        lock (locker)
                        {

                            if ((settings.TaskType != TaskTypeEnum.DeleteMediaRootFolder) && (mli == null))
                            {
                                throw new Exception("[MediaLibraryHelper.LogSynchronization]: Given media library could not be found");
                            }

                            // Create data set with task data
                            DataSet ds = GetTaskData(settings.TaskType, settings.LibraryID, sourcePath, targetPath);

                            if (mli != null)
                            {
                                // Translation table
                                TranslationHelper th = new TranslationHelper();
                                
                                // Register media library
                                th.RegisterRecord(settings.LibraryID, new TranslationParameters(mli.TypeInfo)
                                {
                                    CodeName = mli.LibraryName,
                                    SiteName = settings.SiteName,
                                    GroupId = mli.LibraryGroupID
                                });

                                if (mli.LibraryGroupID > 0)
                                {
                                    // Register group
                                    BaseInfo group = ModuleCommands.CommunityGetGroupInfo(mli.LibraryGroupID);
                                    if (group != null)
                                    {
                                        th.RegisterRecord(group.Generalized.ObjectID, new TranslationParameters(group.TypeInfo)
                                        {
                                            CodeName = group.Generalized.ObjectCodeName,
                                            SiteName = group.Generalized.ObjectSiteName
                                        });
                                    }
                                }
                                // Add translation table
                                ds.Tables.Add(th.TranslationTable);
                            }

                            var taskTitle = GetTaskTitle(settings.TaskType, sourcePath, targetPath, mli);

                            // Create synchronization task
                            var ti = new StagingTaskInfo
                            {
                                TaskData = ds.GetXml(),
                                TaskObjectID = settings.LibraryID,
                                TaskObjectType = OBJECT_TYPE_FOLDER,
                                TaskTime = DateTime.Now,
                                TaskSiteID = si.SiteID,
                                TaskTitle = taskTitle,
                                TaskType = settings.TaskType,
                            };

                            StagingTaskInfoProvider.UpdateTaskServers(ti);

                            using (var h = StagingEvents.LogTask.StartEvent(ti, mli))
                            {
                                if (h.CanContinue())
                                {
                                    var serverIds = StagingTaskInfoProvider.GetServerIdsToLogTaskTo(ti, si.SiteID, settings.ServerID);
                                    if (serverIds.Count > 0)
                                    {
                                        // Log task preparation
                                        var synchronization = String.Format(ResHelper.GetAPIString("synchronization.preparing", "Preparing '{0}' task"), HTMLHelper.HTMLEncode(taskTitle));
                                        LogContext.AppendLine(synchronization, StagingTaskInfoProvider.LOGCONTEXT_SYNCHRONIZATION);

                                        // Save within transaction to keep multithreaded consistency in DB
                                        using (var tr = new CMSTransactionScope())
                                        {
                                            StagingTaskInfoProvider.SetTaskInfo(ti);
                                            SynchronizationInfoProvider.CreateSynchronizationRecords(ti.TaskID, serverIds);
                                            SynchronizationHelper.LogTasksWithUserAndTaskGroups(new[] { ti }, settings.TaskGroups, settings.User);

                                            // Commit the transaction
                                            tr.Commit();
                                        }
                                    }
                                }

                                h.FinishEvent();
                            }

                            return ti;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Staging", "LOGMEDIAFOLDER", ex);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Gets data for the staging task.
        /// </summary>
        /// <param name="taskType">Staging task type</param>
        /// <param name="libraryId">Library identifier</param>
        /// <param name="sourcePath">Library source path</param>
        /// <param name="targetPath">Library target path</param>
        private static DataSet GetTaskData(TaskTypeEnum taskType, int libraryId, string sourcePath, string targetPath)
        {
            // Build source table
            DataSet ds = new DataSet();
            DataTable sourceDt = new DataTable("FolderData");
            ds.Tables.Add(sourceDt);

            // Add columns
            sourceDt.Columns.Add("SourcePath", typeof(string));
            sourceDt.Columns.Add("TargetPath", typeof(string));
            sourceDt.Columns.Add("LibraryID", typeof(int));

            // Add additional data based on the task type
            switch (taskType)
            {
                // Object update - full DataSet
                case TaskTypeEnum.CreateMediaFolder:
                case TaskTypeEnum.MoveMediaFolder:
                case TaskTypeEnum.RenameMediaFolder:
                case TaskTypeEnum.DeleteMediaFolder:
                case TaskTypeEnum.DeleteMediaRootFolder:
                case TaskTypeEnum.CopyMediaFolder:
                    DataRow folderRow = ds.Tables[0].NewRow();
                    folderRow["SourcePath"] = sourcePath;
                    folderRow["TargetPath"] = targetPath;
                    folderRow["LibraryID"] = libraryId;
                    sourceDt.Rows.Add(folderRow);
                    break;

                default:
                    throw new Exception(String.Format("[MediaLibraryHelper.EnsureMediaFolderData]: Unknown task type '{0}'.", taskType));
            }

            if (taskType == TaskTypeEnum.CopyMediaFolder)
            {
                // Add additional media files data for 'Copy media folder' action
                ds.Tables.Add(GetMediaFilesData(sourcePath, targetPath, libraryId).Copy());
            }

            return ds;
        }


        /// <summary>
        /// Retrieves file GUIDs from source path in match with target path.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="libraryId">Library ID</param>
        private static DataTable GetMediaFilesData(string sourcePath, string targetPath, int libraryId)
        {
            // Get data
            var data = new DataQuery()
               .Columns(new QueryColumn("[f1].[FileGuid]").As("SourceGUID"), new QueryColumn("[f2].[FileGuid]").As("TargetGUID"))
               .From(new QuerySource("[Media_File] [f1]").InnerJoin("[Media_File] [f2]",
                       new WhereCondition()
                               .Where(String.Format("[f1].[FilePath] = '{0}' + [f1].[FileName] + [f1].[FileExtension]", SqlHelper.EscapeQuotes(sourcePath) + "/"))
                               .Where(String.Format("[f2].[FilePath] = '{0}' + [f2].[FileName] + [f2].[FileExtension]", SqlHelper.EscapeQuotes(targetPath) + "/"))
                               .Where("[f1].[FileName] = [f2].[FileName]")
                           )
               )
               .WhereEquals("[f1].[FileLibraryID]", libraryId)
               .WhereEquals("[f2].[FileLibraryID]", libraryId);

            // Set table name
            var table = data.Tables[0];
            table.TableName = "FileGUIDs";

            return table;
        }


        /// <summary>
        /// Gets task title.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <param name="library">Media library</param>
        public static string GetTaskTitle(TaskTypeEnum taskType, string sourcePath, string targetPath, MediaLibraryInfo library)
        {
            string cultureCode = CultureHelper.PreferredUICultureCode;

            sourcePath = Path.EnsureSlashes(sourcePath);
            targetPath = Path.EnsureSlashes(targetPath);

            // Correct source path
            sourcePath = String.IsNullOrEmpty(sourcePath) ? "/" : sourcePath;

            // Task title
            string title;
            int parameters = 1;

            switch (taskType)
            {
                case TaskTypeEnum.CreateMediaFolder:
                    // Create
                    parameters = 2;
                    title = ResHelper.GetAPIString("TaskTitle.CreateMediaFolder", cultureCode, "Create folder {0} in library {1}");
                    break;

                case TaskTypeEnum.CopyMediaFolder:
                    // Copy
                    parameters = 3;
                    title = ResHelper.GetAPIString("TaskTitle.CopyMediaFolder", cultureCode, "Copy folder {0} to {1} in library {2}");
                    break;

                case TaskTypeEnum.MoveMediaFolder:
                    // Move
                    parameters = 3;
                    title = ResHelper.GetAPIString("TaskTitle.MoveMediaFolder", cultureCode, "Move folder {0} to {1} in library {2}");
                    break;

                case TaskTypeEnum.RenameMediaFolder:
                    // Rename
                    parameters = 3;
                    title = ResHelper.GetAPIString("TaskTitle.RenameMediaFolder", cultureCode, "Rename folder {0} to {1} in library {2}");
                    break;

                case TaskTypeEnum.DeleteMediaRootFolder:
                    // Delete
                    parameters = 1;
                    title = ResHelper.GetAPIString("TaskTitle.DeleteMediaRootFolder", cultureCode, "Delete root folder {0}");
                    break;

                case TaskTypeEnum.DeleteMediaFolder:
                    // Delete
                    parameters = 2;
                    title = ResHelper.GetAPIString("TaskTitle.DeleteMediaFolder", cultureCode, "Delete folder {0} in library {1}");
                    break;

                default:
                    title = ResHelper.GetAPIString("TaskTitle.Unknown", cultureCode, "[Unknown] {0}");
                    break;
            }

            string libraryDisplayName = (library != null) ? library.LibraryDisplayName : String.Empty;
            object[] values = new object[parameters];

            if (parameters == 2)
            {
                values[0] = sourcePath;
                values[1] = libraryDisplayName;
            }
            else
            {
                if (parameters == 3)
                {
                    values[0] = sourcePath;
                    values[1] = targetPath;
                    values[2] = libraryDisplayName;
                }
                else
                {
                    values[0] = sourcePath;
                }
            }

            return TextHelper.LimitLength(String.Format(title, values), 450);
        }

        #endregion
    }
}
using System;
using System.Web;
using System.Linq;

using CMS.Core;
using CMS.Base;

using SystemIO = System.IO;

namespace CMS.IO
{
    /// <summary>
    /// Class providing helper methods for storage providers management.
    /// </summary>
    public static class StorageHelper
    {
        #region "Variables"

        private static readonly char[] InvalidPathChars = SystemIO.Path.GetInvalidPathChars();

        /// <summary>
        /// Buffer size 64 kB.
        /// </summary>
        [Obsolete("Use custom value instead")]
        public const int BUFFER_SIZE = 65536;


        /// <summary>
        /// Default path to the images folder
        /// </summary>
        public const string DEFAULT_IMAGES_PATH = "~/App_Themes/Default/Images/";

        // If true, the system is allowed to use zipped resources such as SQL installation scripts, web templates, or images
        private static bool? mUseZippedResources;

        #endregion


        #region "Public properties"

        /// <summary>
        /// If true, the system is allowed to use zipped resources such as SQL installation scripts, web templates, or images
        /// </summary>
        public static bool UseZippedResources
        {
            get
            {
                if (mUseZippedResources == null)
                {
                    // Get the settings value
                    mUseZippedResources = CoreServices.AppSettings["CMSUseZippedResources"].ToBoolean(true);
                }

                return mUseZippedResources.Value;
            }
            set
            {
                mUseZippedResources = value;
            }
        }


        /// <summary>
        /// Returns whether current instance running on external storage.
        /// </summary>
        [Obsolete("Use AbstractStorageProvider.DefaultProvider.IsExternalStorage instead.")]
        public static bool IsDefaultStorageExternal
        {
            get
            {
                return AbstractStorageProvider.DefaultProvider.IsExternalStorage;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true, if the given file stream is bound to an external source
        /// </summary>
        /// <param name="stream">Stream to check</param>
        [Obsolete("Use IsExternalStorage with path instead.")]
        public static bool IsExternalFileStream(SystemIO.Stream stream)
        {
            var fs = stream as FileStream;
            if (fs != null)
            {
                return IsExternalStorage(fs.Path);
            }

            return false;
        }


        /// <summary>
        /// Returns whether the path is targeting a shared storage.
        /// </summary>
        /// <param name="path">File path to check (virtual or physical)</param>
        public static bool IsSharedStorage(string path)
        {
            // For empty path, return default provider state
            if (String.IsNullOrEmpty(path))
            {
                return AbstractStorageProvider.DefaultProvider.IsSharedStorage;
            }

            path = GetMappingPath(path);

            var provider = GetStorageProvider(path);

            return provider.IsSharedStorage;
        }


        /// <summary>
        /// Returns whether the path is targeting an external storage.
        /// </summary>
        /// <param name="path">File path to check (virtual or physical)</param>
        public static bool IsExternalStorage(string path)
        {
            // For empty path, return default provider state
            if (String.IsNullOrEmpty(path))
            {
                return AbstractStorageProvider.DefaultProvider.IsExternalStorage;
            }

            path = GetMappingPath(path);

            var provider = GetStorageProvider(path);

            return provider.IsExternalStorage;
        }


        /// <summary>
        /// Removes the mapping to a storage provider
        /// </summary>
        /// <param name="path">Path to unmap</param>
        public static AbstractStorageProvider UnMapStoragePath(string path)
        {
            path = GetMappingPath(path);

            return AbstractStorageProvider.DefaultProvider.UnMapStoragePath(path);
        }


        /// <summary>
        /// Maps the given storage path to a specific provider
        /// </summary>
        /// <param name="path">Path to map, e.g. ~/App_Data</param>
        /// <param name="provider">Provider to use for the given path and sub paths</param>
        public static void MapStoragePath(string path, AbstractStorageProvider provider)
        {
            path = GetMappingPath(path);

            AbstractStorageProvider.DefaultProvider.MapStoragePath(path, provider);
        }


        /// <summary>
        /// Returns full physical path combined from <see cref="SystemContext.WebApplicationPhysicalPath"/> and given path if path is in relative form.
        /// </summary>
        /// <throws>
        /// <see cref="ArgumentNullException"/> when null path is provided.
        /// </throws>
        internal static string GetMappingPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.IndexOfAny(InvalidPathChars) >= 0)
            {
                throw new ArgumentException("Path contains invalid characters.");
            }

            // UNC path should not be normalized because is considered as rooted and starting chararcter can be detected later as not-rooted path 
            if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (path.StartsWith(SystemContext.WebApplicationPhysicalPath, StringComparison.Ordinal))
            {
                return path;
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                path = Path.EnsureBackslashes(path.Substring(2), true);
                path = path.TrimStart('\\');

                return DirectoryHelper.CombinePath(SystemContext.WebApplicationPhysicalPath, path);
            }

            if (!Path.IsPathRooted(path) || path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("\\", StringComparison.Ordinal))
            {
                path = Path.EnsureBackslashes(path, true);
                path = path.TrimStart('\\');

                return DirectoryHelper.CombinePath(SystemContext.WebApplicationPhysicalPath, path);
            }

            return path;
        }


        /// <summary>
        /// Maps the given storage path to a local file system provider
        /// </summary>
        /// <param name="path">Path to map, e.g. ~/App_Data</param>
        /// <param name="customRootPath">Custom root path of the representing file system, e.g. c:\MyFiles</param>
        public static void UseLocalFileSystemForPath(string path, string customRootPath = null)
        {
            var provider = StorageProvider.CreateFileSystemStorageProvider();

            provider.CustomRootPath = customRootPath;

            MapStoragePath(path, provider);
        }


        /// <summary>
        /// Maps the automatic zipped folders
        /// </summary>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static void MapZippedFolders(string zippedFilesFolder)
        {
            // Map all zipped content
            string zippedPath = GetFullFilePhysicalPath(zippedFilesFolder);
            if (Directory.Exists(zippedPath))
            {
                var dir = DirectoryInfo.New(zippedPath);

                // Register zipped files
                RegisterZippedFiles(zippedFilesFolder, dir, null);
            }
        }


        /// <summary>
        /// Registers zipped folders based on the 
        /// </summary>
        /// <param name="zippedFilesFolder">Zipped files folder</param>
        /// <param name="dir">Directory info</param>
        /// <param name="subFolder">Current subfolder</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        private static void RegisterZippedFiles(string zippedFilesFolder, DirectoryInfo dir, string subFolder)
        {
            // Register zipped files
            var files = dir.GetFiles("*.zip");

            foreach (var zippedFile in files)
            {
                // Map the corresponding folder to the zip file
                string fileName = zippedFile.Name;
                string folderName = Path.GetFileNameWithoutExtension(fileName);

                string sourcePath = "~/" + subFolder + folderName;
                string targetPath = zippedFilesFolder + "/" + subFolder;

                if (UseZippedResources)
                {
                    targetPath += ZipStorageProvider.GetZipFileName(fileName);
                }
                else
                {
                    targetPath += folderName;
                }

                Path.RegisterMappedPath(sourcePath, targetPath);
            }

            // Register zipped subfolders
            var subDirs = dir.GetDirectories();

            foreach (var subDir in subDirs)
            {
                subFolder += subDir.Name + "/";

                RegisterZippedFiles(zippedFilesFolder, subDir, subFolder);
            }
        }


        /// <summary>
        /// Returns true, if two given paths use the same storage provider
        /// </summary>
        /// <param name="path1">First file path</param>
        /// <param name="path2">Second file path</param>
        public static bool IsSameStorageProvider(string path1, string path2)
        {
            var provider1 = GetStorageProvider(path1);
            var provider2 = GetStorageProvider(path2);

            return (provider1 == provider2);
        }


        /// <summary>
        /// Copies two files across different storage providers
        /// </summary>
        /// <param name="sourceFileName">Source path</param>
        /// <param name="destFileName">Destination path</param>
        public static void CopyFileAcrossProviders(string sourceFileName, string destFileName)
        {
            // Read the data from source file
            using (var fs = File.OpenRead(sourceFileName))
            {
                // Write the data to destination file
                SaveFileToDisk(destFileName, fs);
            }
        }


        /// <summary>
        /// Copies two files across different storage providers
        /// </summary>
        /// <param name="sourceFileName">Source path</param>
        /// <param name="destFileName">Destination path</param>
        public static void MoveFileAcrossProviders(string sourceFileName, string destFileName)
        {
            bool copied = false;
            bool deleted = false;

            try
            {
                // Copy the file to the target first
                File.Copy(sourceFileName, destFileName);
                copied = true;

                // Delete the file
                File.Delete(sourceFileName);
                deleted = true;
            }
            finally
            {
                if (copied && !deleted)
                {
                    // If delete operation on source didn't success, delete the copied target
                    File.Delete(destFileName);
                }
            }
        }


        /// <summary>
        /// Gets the storage provider based on the given path, updates the path so it matches the provider internal structure
        /// </summary>
        /// <param name="path">Input path, output is path relative to the returned storage provider</param>
        public static AbstractStorageProvider GetStorageProvider(string path)
        {
            return AbstractStorageProvider.DefaultProvider.GetCustomizedStorageProvider(path);
        }


        /// <summary>
        /// Returns new instance of FileInfo object.
        /// </summary>
        /// <param name="filename">File name</param>
        public static FileInfo GetFileInfo(string filename)
        {
            var provider = GetStorageProvider(filename);
            return provider.GetFileInfo(filename);
        }


        /// <summary>
        /// Returns new instance of directory info.
        /// </summary>
        /// <param name="path">Path</param>        
        public static DirectoryInfo GetDirectoryInfo(string path)
        {
            var provider = GetStorageProvider(path);
            return provider.GetDirectoryInfo(path);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>        
        public static FileStream GetFileStream(string path, FileMode mode)
        {
            var provider = GetStorageProvider(path);
            return provider.GetFileStream(path, mode);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        public static FileStream GetFileStream(string path, FileMode mode, FileAccess access)
        {
            var provider = GetStorageProvider(path);
            return provider.GetFileStream(path, mode, access);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>        
        /// <param name="share">Sharing permissions</param>
        public static FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            var provider = GetStorageProvider(path);
            return provider.GetFileStream(path, mode, access, share);
        }


        /// <summary>
        /// Returns new instance of file stream.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="mode">File mode</param>    
        /// <param name="access">File access</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="share">Sharing permissions</param>
        public static FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            var provider = GetStorageProvider(path);
            return provider.GetFileStream(path, mode, access, share, bufferSize);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Saves the given file to the disk file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="data">File data</param>
        /// <param name="closeStream">If true, and source data is stream, the stream gets closed</param>
        public static void SaveFileToDisk(string filePath, BinaryData data, bool closeStream = true)
        {
            if (data.SourceStream != null)
            {
                // Use stream for saving the data
                SaveStreamToDisk(filePath, data.SourceStream, closeStream);
            }
            else
            {
                // Stream to disk
                SaveBinaryDataToDisk(filePath, data.Data);
            }
        }


        /// <summary>
        /// Saves the file from given byte array to the file system.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="fileData">File data</param>
        public static void SaveBinaryDataToDisk(string filePath, byte[] fileData)
        {
            if ((fileData != null) && !String.IsNullOrEmpty(filePath))
            {
                using (FileStream fs = FileStream.New(filePath, FileMode.Create))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }
            }
        }


        /// <summary>
        /// Saves the file from given stream to the file system.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="str">File stream</param>
        /// <param name="closeStream">If true, fileData stream is closed and disposed after save</param>
        public static void SaveStreamToDisk(string filePath, SystemIO.Stream str, bool closeStream = true)
        {
            try
            {
                using (FileStream fs = FileStream.New(filePath, FileMode.Create))
                {
                    str.CopyTo(fs);
                }
            }
            finally
            {
                if ((str != null) && closeStream)
                {
                    str.Dispose();
                }
            }
        }


        /// <summary>
        /// Returns full physical path of a file or folder. Does not change the ending slash
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullFilePhysicalPath(string path, string webFullPath = null)
        {
            // Path is relative, for example: '~/filefolder', '/filefolder', 'filefolder'
            if (!string.IsNullOrEmpty(path) && (!Path.IsPathRooted(path) || path.StartsWith("/", StringComparison.Ordinal)))
            {
                // Ensure webFullPath
                if (webFullPath == null)
                {
                    webFullPath = SystemContext.WebApplicationPhysicalPath;
                }

                path = path.StartsWith("~/", StringComparison.Ordinal) ? path.Substring(2) : path;
                path = path.TrimStart('/');
                path = DirectoryHelper.CombinePath(webFullPath, Path.EnsureBackslashes(path));
            }

            return path;
        }


        /// <summary>
        /// Converts the given physical path to an application relative path
        /// </summary>
        /// <param name="path">Path to convert</param>
        public static string GetWebApplicationRelativePath(string path)
        {
            if (path.StartsWith(SystemContext.WebApplicationPhysicalPath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.EnsureSlashes(path.Substring(SystemContext.WebApplicationPhysicalPath.Length));
            }

            return null;
        }


        /// <summary>
        /// Gets the real file path for the given path (for zip returns the path to the zip file)
        /// </summary>
        /// <param name="path">Path</param>
        public static string GetImageUrl(string path)
        {
            // Do not process empty path
            if (path == null)
            {
                return null;
            }

            // Keep non-zipped files with static URL
            var isZipped = IsZippedFilePath(path);
            if (!isZipped)
            {
                if (!SystemContext.DevelopmentMode && !IsExternalStorage(path))
                {
                    return path;
                }

                // Do not handle paths that are not mapped
                if (!path.StartsWith("~/App_Themes/", StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            // Remove the default images path to save space
            if (path.StartsWith(DEFAULT_IMAGES_PATH, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(DEFAULT_IMAGES_PATH.Length);
            }

            // Merge parameters if contains query string
            path = path.Replace('?', '&');

            path = "~/CMSPages/GetResource.ashx?image=" + HttpUtility.UrlEncode(path);

            return path;
        }


        /// <summary>
        /// Returns true if the given path is a path to the zipped file
        /// </summary>
        /// <param name="path">Path to check</param>
        public static bool IsZippedFilePath(string path)
        {
            bool isZipped = (path.IndexOf(ZipStorageProvider.ZIP_START) >= 0);

            return isZipped;
        }


        /// <summary>
        /// Gets the real file path for the given path (for zip returns the path to the zip file)
        /// </summary>
        /// <param name="path">Path</param>
        public static string GetRealFilePath(string path)
        {
            int zipIndex = path.IndexOf(ZipStorageProvider.ZIP_START);
            if (zipIndex >= 0)
            {
                int endZipIndex = path.IndexOf(ZipStorageProvider.ZIP_END, zipIndex);

                if ((endZipIndex >= 0) && (zipIndex < endZipIndex))
                {
                    // Ensure the zip provider mapped to the zip file
                    string zipPath = path.Substring(0, zipIndex) + path.Substring(zipIndex + 1, endZipIndex - zipIndex - 1);

                    return zipPath;
                }
            }

            return path;
        }


        /// <summary>
        /// Deletes files older than specified time from the file system.
        /// </summary>
        /// <param name="dir">Directory to process</param>
        /// <param name="olderThan">Limit time for deletion of a file</param>
        /// <param name="deleteEmptyFolder">Indicates if the specified folder should be deleted if there are no files left at the end</param>
        public static void DeleteOldFiles(DirectoryInfo dir, DateTime olderThan, bool deleteEmptyFolder = true)
        {
            if (dir.Exists)
            {
                // Go through files
                var files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    if (file.LastWriteTime < olderThan)
                    {
                        file.Delete();
                    }
                }

                // Go through nested directories
                var dirs = dir.GetDirectories();

                foreach (DirectoryInfo childDir in dirs)
                {
                    DeleteOldFiles(childDir, olderThan, deleteEmptyFolder);
                }

                // Delete the folder itself
                if (deleteEmptyFolder &&
                    !dir.GetFiles().Any() &&
                    !dir.GetDirectories().Any())
                {
                    try
                    {
                        dir.Delete();
                    }
                    catch (SystemIO.IOException)
                    {
                        // Do not stop on IO exceptions like "Directory is not empty" or "Directory is being used by another process"
                    }
                }
            }
        }


        /// <summary>
        /// Deletes files older than specified time from the file system.
        /// </summary>
        /// <param name="folder">Folder path</param>
        /// <param name="olderThan">Limit time for deletion of a file</param>
        /// <param name="deleteEmptyFolder">Indicates if the specified folder should be deleted if there are no files left at the end</param>
        public static void DeleteOldFiles(string folder, DateTime olderThan, bool deleteEmptyFolder = true)
        {
            DirectoryInfo dir = DirectoryInfo.New(folder);
            DeleteOldFiles(dir, olderThan, deleteEmptyFolder);
        }

        #endregion


        #region "Web farm methods"

        /// <summary>
        /// Logs the directory delete task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        [Obsolete("Use StorageSynchronization.LogDirectoryDeleteTask instead.")]
        public static void LogDirectoryDeleteTask(string path)
        {
            StorageSynchronization.LogDirectoryDeleteTask(path);
        }


        /// <summary>
        /// Logs the file delete task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        [Obsolete("Use StorageSynchronization.LogDeleteFileTask instead.")]
        public static void LogDeleteFileTask(string path)
        {
            StorageSynchronization.LogDeleteFileTask(path);
        }


        /// <summary>
        /// Logs the file update task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        [Obsolete("Use StorageSynchronization.LogUpdateFileTask instead.")]
        public static void LogUpdateFileTask(string path)
        {
            StorageSynchronization.LogUpdateFileTask(path);
        }

        #endregion
    }
}
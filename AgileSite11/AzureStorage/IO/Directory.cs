using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

using CMS.IO;
using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;

using Microsoft.WindowsAzure.Storage.Blob;
using Path = System.IO.Path;
using SearchOption = CMS.IO.SearchOption;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Implementation of Directory provider for Azure.
    /// </summary>
    public class Directory : AbstractDirectory
    {
        private const int PATH_MAX_LENGTH = 247;

        #region "Variables"

        private static string mCurrentDirectory;
        private readonly IDateTimeNowService mDateTimeNowService;

        #endregion


        #region "Properties"

        /// <summary>       
        /// Returns current directory. Value remains the same so it can be cached. 
        /// </summary>
        private static string CurrentDirectory
        {
            get
            {
                return mCurrentDirectory ?? (mCurrentDirectory = GetCaseValidPath(SystemContext.WebApplicationPhysicalPath));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="Directory"/> class, which provides operations with directories.
        /// </summary>
        public Directory() : this(Service.Resolve<IDateTimeNowService>()) {}


        internal Directory(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }

        #endregion


        #region "Public override methods"

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test.</param>  
        public override bool Exists(string path)
        {
            return ExistsInFileSystem(path) || ExistsInBlobStorage(path);
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create.</param> 
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override IO.DirectoryInfo CreateDirectory(string path)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            // Get valid path
            path = GetValidPath(path);

            // Check if directory already exists
            if (Exists(path))
            {
                return new DirectoryInfo(path);
            }

            // Create blob which represents directory
            IO.File.Create(path + "\\" + BlobInfoProvider.DIRECTORY_BLOB);

            // When blob is created create directory info
            var info = new DirectoryInfo(path);

            // Fill information
            info.CreationTime = mDateTimeNowService.GetDateTimeNow();
            info.Exists = true;
            info.FullName = path;
            info.LastWriteTime = info.CreationTime;
            info.Name = Path.GetFileName(path);

            return info;
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_FILES, -1, null, null, IOProviderName.Azure);

            return EnumerateFilesCore(path, searchPattern);
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        private IEnumerable<string> EnumerateFilesCore(string path, string searchPattern)
        {
            IEnumerable<string> files = Enumerable.Empty<string>();

            // Get files from file system
            if (ExistsInFileSystem(path))
            {
                files = System.IO.Directory.EnumerateFiles(path, searchPattern).Select(f => GetCaseValidPath(f));
            }

            path = GetValidPath(path);

            // Get files from blob storage
            if (!ExistsInBlobStorage(path))
            {
                return files;
            }

            var containerInfo = ContainerInfoProvider.GetRootContainerInfo(path);

            // Prepare search condition
            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);

            // Get all blobs
            IEnumerable<IListBlobItem> blobs = ContainerInfoProvider.GetContent(containerInfo, path).Where(blob => blob is CloudBlockBlob);

            // Get all azure files
            IEnumerable<string> azureFiles = blobs.Select(GetBlobName)
                .Where(blobName => searchCondition(blobName) && (CMSString.Compare(blobName, BlobInfoProvider.DIRECTORY_BLOB, true) != 0))
                .Select(blobName => GetCaseValidPath(path + "\\" + blobName));

            return files.Union(azureFiles, StringComparer.Ordinal);
        }


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param> 
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override string[] GetFiles(string path, string searchPattern)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            var files = EnumerateFilesCore(path, searchPattern).ToArray();
            
            FileDebug.LogFileOperation(path, FileDebugOperation.GET_FILES, -1, files.Length.ToString(), null, IOProviderName.Azure);

            return files;
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES, -1, null, null, IOProviderName.Azure);

            return EnumerateDirectoriesCore(path, searchPattern, searchOption);
        }


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern in the current directory,
        /// and optionally searches subdirectories.
        /// </summary>        
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            var directories = EnumerateDirectoriesCore(path, searchPattern, searchOption).ToArray();

            FileDebug.LogFileOperation(path, FileDebugOperation.GET_DIRECTORIES, -1, directories.Length.ToString(), null, IOProviderName.Azure);

            return directories;
        }


        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public override string GetCurrentDirectory()
        {
            return SystemContext.WebApplicationPhysicalPath;
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">Delete if subdirs exists.</param>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>
        public override void Delete(string path, bool recursive)
        {
            ThrowOnTooLongDirecotoryPath(path);
            ThrowOnInvalidPath(path);

            path = GetValidPath(path);

            if (!ExistsInBlobStorage(path))
            {
                System.IO.Directory.Delete(path, recursive);
                return;
            }

            if (recursive)
            {
                ContainerInfo ci = ContainerInfoProvider.GetRootContainerInfo(path);

                // Get all blobs
                IEnumerable<IListBlobItem> blockBlobs = ContainerInfoProvider.GetContent(ci, path, true)
                                                                             .Where(b => b is CloudBlockBlob);

                foreach (var blob in blockBlobs)
                {
                    BlobInfo file = new BlobInfo(ci, blob.Uri.ToString(), true);
                    try
                    {
                        BlobInfoProvider.DeleteBlob(file);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                // Get files from current directory
                string[] files = GetFiles(path);

                // Get subdirectories from current directory                        
                string[] subdirectories = GetDirectories(path);

                // Check if directory is empty
                if ((subdirectories.Length == 0) && (files.Length == 0))
                {
                    DeleteAzureDirectory(path);
                }
                else
                {
                    throw new IOException($"Directory {path} is not empty.");
                }
            }

            // Delete data from temp and cache directories
            if (path.StartsWith(CurrentDirectory, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(CurrentDirectory.Length);

                try
                {
                    System.IO.Directory.Delete(Path.Combine(PathHelper.TempPath, path));
                    System.IO.Directory.Delete(Path.Combine(PathHelper.CachePath, path));
                }
                catch (IOException)
                {
                    // Operation is not crucial, in case that something went wrong (e.g. concurrent access, etc.) during the cleanup, suppress the exception
                }
            }
        }


        /// <summary>
        /// Moves directory.
        /// </summary>
        /// <param name="sourceDirName">Source directory name.</param>
        /// <param name="destDirName">Destination directory name.</param>        
        /// <exception cref="PathTooLongException"><paramref name="sourceDirName"/> or <paramref name="destDirName"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceDirName"/> or <paramref name="destDirName"/> contains invalid characters</exception>
        /// <exception cref="IOException"><paramref name="sourceDirName"/> does not exist or <paramref name="destDirName"/> already exists</exception>
        public override void Move(string sourceDirName, string destDirName)
        {
            Move(sourceDirName, destDirName, 0);
        }


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <exception cref="PathTooLongException"><paramref name="path"/> is too long</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters</exception>        
        public override void Delete(string path)
        {
            Delete(path, false);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified file.
        /// </summary>
        /// <param name="path">Path to directory.</param>        
        public override DirectorySecurity GetAccessControl(string path)
        {
            // In Microsoft Azure has user full control over file system
            return new DirectorySecurity();
        }


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public override void PrepareFilesForImport(string path)
        {
            path = GetValidPath(path);
            string blobPath = GetBlobPathFromPath(path);

            if (!ExistsInBlobStorage(blobPath))
            {
                return;
            }

            ContainerInfo containerInfo = ContainerInfoProvider.GetRootContainerInfo(path);
            IEnumerable<IListBlobItem> blockBlobFiles = ContainerInfoProvider.GetContent(containerInfo, blobPath, true)
                                                                             .Where(b => b is CloudBlockBlob && !IsBlobDirectory(b));
                                                                         

            foreach (var blob in blockBlobFiles)
            {
                string originalBlobUri = blob.Uri.ToString();
                string loweredBlobUri = originalBlobUri.ToLowerInvariant();

                if (originalBlobUri != loweredBlobUri)
                {
                    // Prepare blobs
                    BlobInfo source = new BlobInfo(containerInfo, originalBlobUri, true);
                    BlobInfo dest = new BlobInfo(containerInfo, loweredBlobUri, true);

                    BlobInfoProvider.CopyBlobs(source, dest);
                    BlobInfoProvider.DeleteBlob(source);
                }
            }
        }


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public override void DeleteDirectoryStructure(string path)
        {
            path = GetValidPath(path);

            if (!ExistsInBlobStorage(path))
            {
                return;
            }

            ContainerInfo containerInfo = ContainerInfoProvider.GetRootContainerInfo(path);

            // Get all blobs
            IEnumerable<IListBlobItem> blockBlobFiles = ContainerInfoProvider.GetContent(containerInfo, path, true)
                                                                             .Where(b => b is CloudBlockBlob && !IsBlobDirectory(b));

            foreach (var blob in blockBlobFiles)
            {
                BlobInfo blobInfo = new BlobInfo(containerInfo, blob.Uri.ToString(), true);
                try
                {
                    BlobInfoProvider.DeleteBlob(blobInfo);
                }
                catch
                {
                }
            }
        }

        #endregion


        #region "Other public methods"

        /// <summary>
        /// Converts path to valid and lower case.
        /// </summary>
        /// <param name="path">Path to modify.</param>
        /// <param name="caseSensitive">Case sensitive.</param>
        public static string GetValidPath(string path, bool? caseSensitive = null)
        {
            path = IO.Path.EnsureBackslashes(path);
            path = GetCaseValidPath(path, caseSensitive);
            path = path.TrimEnd('\\');
            return path;
        }


        /// <summary>
        /// Returns system path from URI and container name.
        /// </summary>
        /// <param name="uri">Uri.</param>
        /// <param name="absolute">If path should be absolute.</param>
        /// <param name="containerName">Container name.</param>
        /// <returns>Absolute path.</returns>
        public static string GetPathFromUri(Uri uri, bool absolute, string containerName = null)
        {
            string path = GetValidPath(uri.LocalPath);
            string contName = containerName ?? ContainerInfoProvider.GetRootContainer(path);
            path = path.Substring(path.IndexOf(contName, StringComparison.Ordinal) + contName.Length + 1);

            if (absolute)
            {
                return CurrentDirectory + "\\" + path;
            }
            return path;
        }


        /// <summary>
        /// Returns blob path from given file system path. Returns relative path with slashes '/' from the root of the blob storage.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="caseSensitive">Case sensitive.</param>
        public static string GetBlobPathFromPath(string path, bool? caseSensitive = null)
        {
            // Get current lowered path
            string currentDirectory = CurrentDirectory;

            path = GetValidPath(path, caseSensitive);

            // Delete beginning of absolute path
            if (path.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(currentDirectory.Length);
            }

            // Replace back slash with slash
            path = IO.Path.EnsureSlashes(path);

            return path.Trim('/');
        }


        /// <summary>
        /// Determines whether the given path refers to an existing directory on azure blob.
        /// </summary>
        /// <param name="path">Path to test.</param>  
        public static bool ExistsInBlobStorage(string path)
        {
            ContainerInfo container = ContainerInfoProvider.GetRootContainerInfo(path);
            string dir = GetBlobPathFromPath(path);

            // Root directory always exists 
            if (string.IsNullOrEmpty(dir))
            {
                return true;
            }

            // Some directory in root
            CloudBlobDirectory directory = container.BlobContainer.GetDirectoryReference(dir);
            return directory.ListBlobs().Any();
        }


        /// <summary>
        /// Determines whether the given path refers to an existing directory on filesystem
        /// </summary>
        /// <param name="path">Path to test.</param>  
        public static bool ExistsInFileSystem(string path)
        {
            return System.IO.Directory.Exists(path);
        }


        /// <summary>
        /// Creates directory structure for given path.
        /// </summary>
        /// <param name="path">Path with temporary file.</param>
        public static void CreateDirectoryStructure(string path)
        {
            string directory = IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }


        /// <summary>
        /// Returns lowered path if given container if not case sensitive. Otherwise method returns original path.
        /// </summary>
        /// <param name="path">Path to modify.</param>
        /// <param name="caseSensitive">Indicates whether path is case sensitive</param>
        public static string GetCaseValidPath(string path, bool? caseSensitive = null)
        {
            ContainerInfo container = ContainerInfoProvider.GetRootContainerInfo(path);

            bool sensitive = caseSensitive ?? container.CaseSensitive;
            if (sensitive)
            {
                return path;
            }

            return path.ToLowerInvariant();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Deletes container and map record in database.
        /// </summary>
        /// <param name="path">Path to directory</param>
        private static void DeleteAzureDirectory(string path)
        {
            IO.File.Delete(path + "/" + BlobInfoProvider.DIRECTORY_BLOB);
        }


        /// <summary>
        /// Moves directory.
        /// </summary>
        /// <param name="sourceDirName">Source directory name.</param>
        /// <param name="destDirName">Destination directory name.</param>
        /// <param name="level">Level.</param>
        private void Move(string sourceDirName, string destDirName, int level)
        {
            if (GetValidPath(destDirName).Equals(GetValidPath(sourceDirName), StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException($"'{nameof(sourceDirName)}' and '{nameof(destDirName)}' parameters refer to the same file or directory.");
            }

            ThrowOnInvalidPath(sourceDirName);
            ThrowOnTooLongDirecotoryPath(sourceDirName);

            ThrowOnInvalidPath(destDirName);
            ThrowOnTooLongDirecotoryPath(destDirName);
            ThrowOnExistentDirectory(destDirName);

            var azureFile = new File();
            sourceDirName = GetValidPath(sourceDirName);
            destDirName = GetValidPath(destDirName);

            if (!Exists(sourceDirName))
            {
                if (azureFile.Exists(sourceDirName))
                {
                    azureFile.Move(sourceDirName, destDirName);
                    return;
                }

                throw new IOException($"Path '{sourceDirName}' does not exist.");
            }

            CreateDirectory(destDirName);

            // Copy files
            string[] files = GetFiles(sourceDirName);
            foreach (string file in files)
            {
                azureFile.Copy(file, destDirName + "\\" + IO.Path.GetFileName(file));
                azureFile.Delete(file);
            }

            // Recursive Copy directories
            string[] dirs = GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                Move(dir, destDirName + "\\" + IO.Path.GetFileName(dir), level + 1);
            }

            // Delete directories
            if (level == 0)
            {
                Delete(sourceDirName, true);
            }
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        private IEnumerable<string> EnumerateDirectoriesCore(string path, string searchPattern, SearchOption searchOption)
        {
            IEnumerable<string> directories = null;

            path = GetValidPath(path);

            // Directories from filesystem
            if (ExistsInFileSystem(path))
            {
                directories = System.IO.Directory.EnumerateDirectories(path, searchPattern, (System.IO.SearchOption)searchOption).Select(d => GetCaseValidPath(d));
            }

            // Directories from blob storage
            if (!ExistsInBlobStorage(path))
            {
                return directories ?? Enumerable.Empty<string>();
            }

            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);

            ContainerInfo container = ContainerInfoProvider.GetRootContainerInfo(path);
            IEnumerable<IListBlobItem> blobs = ContainerInfoProvider.GetContent(container, path, searchOption == SearchOption.AllDirectories);

            IEnumerable<string> azureDirectories;

            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                var filteredBlobs = blobs.Where(b => b is CloudBlobDirectory);

                azureDirectories = filteredBlobs
                    .Select(b => GetCaseValidPath(GetPathFromUri(b.Uri, true, container.ContainerName)))
                    .Where(d => searchCondition(IO.Path.GetFileName(d.TrimEnd('\\'))));
            }
            else
            {
                // Azure lists files, not virtual folders when flat listing is set
                var fullFilePaths = blobs.Select(b => GetCaseValidPath(GetPathFromUri(b.Uri, true, container.ContainerName)));

                // Extract folder paths from file paths, omit the root (if it contained a file) and remove duplicates
                var fullDirectoryPaths = fullFilePaths
                    .Select(IO.Path.GetDirectoryName)
                    .Where(d => d.Length > path.Length).Distinct(StringComparer.OrdinalIgnoreCase);

                azureDirectories = fullDirectoryPaths.Where(d => searchCondition(IO.Path.GetFileName(d)));
            }

            directories = directories == null ? azureDirectories : directories.Union(azureDirectories, StringComparer.Ordinal);

            return directories;
        }
        

        /// <summary>
        /// Throws <see cref="FileNotFoundException"/> if directory referenced by <paramref name="path"/>
        /// does not exist.
        /// </summary>
        private void ThrowOnExistentDirectory(string path)
        {
            if (ExistsInBlobStorage(path))
            {
                throw new IOException($"Path '{path}' already exists.");
            }
        }


        /// <summary>
        /// Throws <see cref="PathTooLongException"/> if the path leading to a directory exceeds maximum length.
        /// </summary>
        private void ThrowOnTooLongDirecotoryPath(string path)
        {
            var normalizedPath = GetValidPath(path);
            if (normalizedPath.Length > PATH_MAX_LENGTH)
            {
                throw new PathTooLongException($"Path '{path}' is too long.");
            }
        }


        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the path leading to a directory contains invalid characters.
        /// </summary>
        private void ThrowOnInvalidPath(string path)
        {
            if (IsPathToDirectoryContainingInvalidChars(path))
            {
                throw new ArgumentException($"Path '{path}' is invalid.");
            }
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="path"/> contains some invalid char.
        /// </summary>
        private bool IsPathToDirectoryContainingInvalidChars(string path)
        {
            var invalidPathChars = new string(Path.GetInvalidPathChars());

            return path.Intersect(invalidPathChars).Count() != 0;
        }


        /// <summary>
        /// Returns true, if blob is placeholder for directory
        /// </summary>
        private bool IsBlobDirectory(IListBlobItem blob)
        {
            var blobName = GetBlobName(blob);
            return CMSString.Compare(blobName, BlobInfoProvider.DIRECTORY_BLOB, true) == 0;
        }


        /// <summary>
        /// Returns name of the blob.
        /// </summary>
        private static string GetBlobName(IListBlobItem blob)
        {
            return HttpUtility.UrlDecode(blob.Uri.Segments[blob.Uri.Segments.Length - 1]);
        }

        #endregion
    }
}
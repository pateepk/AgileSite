using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

using CMS.IO;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// Envelope for System.IO.Directory.
    /// </summary>
    public class Directory : AbstractDirectory
    {
        #region "Overridden methods"

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test</param>
        public override bool Exists(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            return System.IO.Directory.Exists(path);
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create</param>
        public override IO.DirectoryInfo CreateDirectory(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            System.IO.DirectoryInfo ioInfo = System.IO.Directory.CreateDirectory(path);

            DirectoryInfo info = new DirectoryInfo(ioInfo);
            return info;
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            // Get list of files from the target path
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_FILES, -1, null, null, IOProviderName.FileSystem);

            var files = System.IO.Directory.EnumerateFiles(path, searchPattern);

            // Unmap the results to virtual paths
            return files.Select(f => AbstractStorageProvider.GetVirtualPhysicalPath(provider, f));
        }


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
        public override string[] GetFiles(string path, string searchPattern)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            // Get list of files from the target path
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            var result = System.IO.Directory.GetFiles(path, searchPattern);

            // Unmap the results to virtual paths
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = AbstractStorageProvider.GetVirtualPhysicalPath(provider, result[i]);
            }

            FileDebug.LogFileOperation(path, FileDebugOperation.GET_FILES, -1, result.Length.ToString(), null, IOProviderName.FileSystem);

            return result;
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">If delete if subdirs exists</param>
        public override void Delete(string path, bool recursive)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            System.IO.Directory.Delete(path, recursive);

            StorageSynchronization.LogDirectoryDeleteTask(path);
        }


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override void Delete(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            System.IO.Directory.Delete(path);

            StorageSynchronization.LogDirectoryDeleteTask(path);
        }


        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="sourceDirName">Source directory name</param>
        /// <param name="destDirName">Destination directory name</param>
        public override void Move(string sourceDirName, string destDirName)
        {
            sourceDirName = AbstractStorageProvider.GetTargetPhysicalPath(sourceDirName);
            destDirName = AbstractStorageProvider.GetTargetPhysicalPath(destDirName);

            System.IO.Directory.Move(sourceDirName, destDirName);
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            // Get list of files from the target path
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            FileDebug.LogFileOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES, -1, null, null, IOProviderName.FileSystem);

            // Get the directories from file system
            var directories = System.IO.Directory.EnumerateDirectories(path, searchPattern, (System.IO.SearchOption)searchOption);

            // Unmap the results to virtual paths
            return directories.Select(d => AbstractStorageProvider.GetVirtualPhysicalPath(provider, d));
        }


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern in the current directory,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            // Get list of files from the target path
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            // Get the directories from file system
            var result = System.IO.Directory.GetDirectories(path, searchPattern, (System.IO.SearchOption)searchOption);

            // Unmap the results to virtual paths
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = AbstractStorageProvider.GetVirtualPhysicalPath(provider, result[i]);
            }

            FileDebug.LogFileOperation(path, FileDebugOperation.GET_DIRECTORIES, -1, result.Length.ToString(), null, IOProviderName.FileSystem);

            return result;
        }


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override DirectorySecurity GetAccessControl(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

#if NETSTANDARD
            var directoryInfo = new System.IO.DirectoryInfo(path);
            return System.IO.FileSystemAclExtensions.GetAccessControl(directoryInfo);
#elif NETFULLFRAMEWORK
            return System.IO.Directory.GetAccessControl(path);
#endif
        }


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public override void PrepareFilesForImport(string path)
        {
            // There is nothing to do for System.IO
        }


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public override void DeleteDirectoryStructure(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);

            // For each subdirectory recursively
            var dir = new System.IO.DirectoryInfo(path);
            if (dir.Exists)
            {
                foreach (var subdir in dir.GetDirectories())
                {
                    DeleteDirectoryStructure(subdir.FullName);
                }

                // Delete all files in the current directory
                foreach (var file in dir.GetFiles())
                {
                    file.Delete();
                }
            }
        }

#endregion
    }
}
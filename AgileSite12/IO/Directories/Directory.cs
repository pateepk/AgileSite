using System.Collections.Generic;
using System.Data;
using System.Security.AccessControl;

namespace CMS.IO
{
    /// <summary>
    /// Envelope for Directory classes
    /// </summary>
    public static class Directory
    {
        #region "Public methods"

        /// <summary>
        /// Gets the directory provider object for given path
        /// </summary>
        /// <param name="path">Input path, output is path relative to the returned storage provider</param>
        public static AbstractDirectory GetDirectoryObject(ref string path)
        {
            var provider = StorageHelper.GetStorageProvider(path);

            return provider.DirectoryProviderObject;
        }


        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static bool ExistsRelative(string path)
        {
            path = StorageHelper.GetFullFilePhysicalPath(path);

            return Exists(path);
        }


        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test</param>
        public static bool Exists(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.DirExists);

            try
            {
                var dir = GetDirectoryObject(ref path);
                return dir.Exists(path);
            }
            catch (StorageProviderException)
            {
                throw;
            }
            catch
            {
                // Other exceptions are not propagated (similar behavior as native implementation of Directory.Exists method)
            }

            return false;
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create</param>
        public static DirectoryInfo CreateDirectory(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.CREATE_DIR);

            var dir = GetDirectoryObject(ref path);

            return dir.CreateDirectory(path);
        }


        /// <summary>
        /// Returns an enumerable collection of file names in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/>.</returns>
        /// <remarks>
        /// This method is identical to <see cref="EnumerateFiles(string,string)"/> with the asterisk (*) specified as the search pattern.
        /// </remarks>
        public static IEnumerable<string> EnumerateFiles(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.ENUMERATE_FILES);

            var dir = GetDirectoryObject(ref path);

            return dir.EnumerateFiles(path);
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory, or an empty array if no files are found.</returns>
        /// <remarks>
        /// This method is identical to <see cref="GetFiles(string,string)"/> with the asterisk (*) specified as the search pattern.
        /// </remarks>
        public static string[] GetFiles(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_FILES);

            var dir = GetDirectoryObject(ref path);

            return dir.GetFiles(path);
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            LogDirectoryOperation(path, FileDebugOperation.ENUMERATE_FILES);

            var dir = GetDirectoryObject(ref path);

            return dir.EnumerateFiles(path, searchPattern);
        }


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
        public static string[] GetFiles(string path, string searchPattern)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_FILES);

            var dir = GetDirectoryObject(ref path);

            return dir.GetFiles(path, searchPattern);
        }


        /// <summary>
        /// Returns an enumerable collection of directory names in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method is identical to <see cref="EnumerateDirectories(string, string)"/> with the asterisk (*) specified as the search pattern, so it returns all subdirectories.
        /// </para>
        /// <para>
        /// If you need to search subdirectories recursively, use the <see cref="EnumerateDirectories(string,string,SearchOption)"/> method, which enables you to specify a search of all subdirectories.
        /// </para>
        /// </remarks>
        public static IEnumerable<string> EnumerateDirectories(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.EnumerateDirectories(path);
        }


        /// <summary>
        /// Returns the names of subdirectories (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An array of the full names (including paths) of subdirectories in the specified path, or an empty array if no directories are found.</returns>
        /// <remarks>
        /// <para>
        /// This method is identical to <see cref="GetDirectories(string, string)"/> with the asterisk (*) specified as the search pattern, so it returns all subdirectories.
        /// </para>
        /// <para>
        /// If you need to search subdirectories recursively, use the <see cref="GetDirectories(string,string,SearchOption)"/> method, which enables you to specify a search of all subdirectories.
        /// </para>
        /// </remarks>
        public static string[] GetDirectories(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.GetDirectories(path);
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">If delete if subdirs exists</param>
        public static void Delete(string path, bool recursive)
        {
            using (var h = IOEvents.DeleteDirectory.StartEvent(path))
            {
                LogDirectoryOperation(path, FileDebugOperation.DELETE_DIR);

                var dir = GetDirectoryObject(ref path);

                dir.Delete(path, recursive);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public static void Delete(string path)
        {
            using (var h = IOEvents.DeleteDirectory.StartEvent(path))
            {
                LogDirectoryOperation(path, FileDebugOperation.DELETE_DIR);

                var dir = GetDirectoryObject(ref path);

                dir.Delete(path);

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="sourceDirName">Source directory name</param>
        /// <param name="destDirName">Destination directory name</param>
        public static void Move(string sourceDirName, string destDirName)
        {
            LogDirectoryOperation(sourceDirName + "|" + destDirName, FileDebugOperation.MOVE_DIR);

            var dir = GetDirectoryObject(ref sourceDirName);

            dir.Move(sourceDirName, destDirName);
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        /// <remarks>
        /// If you need to search subdirectories recursively, use the <see cref="EnumerateDirectories(string,string,SearchOption)"/> method, which enables you to specify a search of all subdirectories.
        /// </remarks>
        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            LogDirectoryOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.EnumerateDirectories(path, searchPattern);
        }


        /// <summary>
        /// Returns the names of subdirectories (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the search pattern in the specified directory, or an empty array if no directories are found.</returns>
        /// <remarks>
        /// If you need to search subdirectories recursively, use the <see cref="GetDirectories(string,string,SearchOption)"/> method, which enables you to specify a search of all subdirectories.
        /// </remarks>
        public static string[] GetDirectories(string path, string searchPattern)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.GetDirectories(path, searchPattern);
        }



        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            LogDirectoryOperation(path, FileDebugOperation.ENUMERATE_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.EnumerateDirectories(path, searchPattern, searchOption);
        }


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern in the current directory,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
        public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_DIRECTORIES);

            var dir = GetDirectoryObject(ref path);

            return dir.GetDirectories(path, searchPattern, searchOption);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file</param>
        public static DirectorySecurity GetAccessControl(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.GET_ACCESS_CONTROL);

            var dir = GetDirectoryObject(ref path);

            return dir.GetAccessControl(path);
        }


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public static void PrepareFilesForImport(string path)
        {
            LogDirectoryOperation(path, "PrepareFilesForImport");

            var dir = GetDirectoryObject(ref path);

            dir.PrepareFilesForImport(path);
        }


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public static void DeleteDirectoryStructure(string path)
        {
            LogDirectoryOperation(path, FileDebugOperation.DELETE_DIR_STRUCTURE);

            var dir = GetDirectoryObject(ref path);

            dir.DeleteDirectoryStructure(path);
        }

        #endregion


        #region "Debug methods"

        /// <summary>
        /// Logs the directory operation to a current request log for debugging.
        /// </summary>
        /// <param name="path">Path of the directory</param>
        /// <param name="operation">Operation with directory</param>
        public static DataRow LogDirectoryOperation(string path, string operation)
        {
            return FileDebug.LogFileOperation(path, operation);
        }


        /// <summary>
        /// Logs the directory operation to a current request log for debugging.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="operation">Operation with file (open, close, read, write)</param>
        /// <param name="providerName">Provider name</param>
        public static DataRow LogDirectoryOperation(string path, string operation, string providerName)
        {
            return FileDebug.LogFileOperation(path, operation, providerName);
        }

        #endregion
    }
}
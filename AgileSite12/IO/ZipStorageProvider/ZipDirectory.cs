using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace CMS.IO.Zip
{
    /// <summary>
    /// Envelope for System.IO.Directory.
    /// </summary>
    public class ZipDirectory : AbstractDirectory
    {
        #region "Variables"

        /// <summary>
        /// Parent provider
        /// </summary>
        private readonly ZipStorageProvider mProvider;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Parent provider</param>
        public ZipDirectory(ZipStorageProvider provider)
        {
            mProvider = provider;
        }


        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test</param>
        public override bool Exists(string path)
        {
            return (mProvider.GetDirectoryInfo(path) != null);
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create</param>
        public override DirectoryInfo CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns an enumerable collection of file names in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/>.</returns>
        public override IEnumerable<string> EnumerateFiles(string path)
        {
            DirectoryInfo dir = mProvider.GetDirectoryInfo(path);
            var files = dir.EnumerateFiles();

            return files.Select(f => f.FullName);
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">Path to retrieve files from</param>
        public override string[] GetFiles(string path)
        {
            return EnumerateFiles(path).ToArray();
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            DirectoryInfo dir = mProvider.GetDirectoryInfo(path);
            var files = dir.EnumerateFiles(searchPattern);

            return files.Select(f => f.FullName);
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">Path to retrieve files from</param>
        /// <param name="searchPattern">Search pattern</param>
        public override string[] GetFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern).ToArray();
        }


        /// <summary>
        /// Returns an enumerable collection of directory names in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/>.</returns>
        /// <remarks>
        /// This method is identical to <see cref="EnumerateDirectories(string, string)"/> with the asterisk (*) specified as the search pattern, so it returns all subdirectories.
        /// </remarks>
        public override IEnumerable<string> EnumerateDirectories(string path)
        {
            return EnumerateDirectories(path, "*");
        }


        /// <summary>
        /// Gets the names of subdirectories in the specified directory.
        /// </summary>
        /// <param name="path">Path to retrieve directories from</param>
        public override string[] GetDirectories(string path)
        {
            return GetDirectories(path, "*");
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            DirectoryInfo dir = mProvider.GetDirectoryInfo(path);
            var dirs = dir.EnumerateDirectories(searchPattern);

            return dirs.Select(d => d.FullName);
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="searchPattern">Search pattern</param>
        public override string[] GetDirectories(string path, string searchPattern)
        {
            return EnumerateDirectories(path, searchPattern).ToArray();
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
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern
        /// in the current directory, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">If delete if subdirs exists</param>
        public override void Delete(string path, bool recursive)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override void Delete(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="sourceDirName">Source directory name</param>
        /// <param name="destDirName">Destination directory name</param>
        public override void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public override DirectorySecurity GetAccessControl(string path)
        {
            return new DirectorySecurity();
        }


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public override void PrepareFilesForImport(string path)
        {
        }


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public override void DeleteDirectoryStructure(string path)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
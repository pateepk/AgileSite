using System;
using System.Collections.Generic;
using System.Security.AccessControl;

using CMS.IO;

namespace CMS.CustomFileSystemProvider
{
    /// <summary>
    /// Sample of Directory class of CMS.IO provider.
    /// </summary>
    public class Directory : CMS.IO.AbstractDirectory
    {
        #region "Public override methods"

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test.</param>  
        public override bool Exists(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create.</param> 
        public override CMS.IO.DirectoryInfo CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">Path to retrieve files from.</param> 
        /// <param name="searchPattern">Search pattern.</param>
        public override string[] GetFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
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
        /// Gets the names of the subdirectories (including their paths) that match the specified search
        /// pattern in the current directory, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">Path to retrieve directories from.</param>
        /// <param name="searchPattern">Pattern to be searched.</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public override string GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">If delete if sub directories exists.</param>
        public override void Delete(string path, bool recursive)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Moves directory.
        /// </summary>
        /// <param name="sourceDirName">Source directory name.</param>
        /// <param name="destDirName">Destination directory name.</param>
        public override void Move(string sourceDirName, string destDirName)
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
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified file.
        /// </summary>
        /// <param name="path">Path to directory.</param>        
        public override DirectorySecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public override void PrepareFilesForImport(string path)
        {
            throw new NotImplementedException();
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

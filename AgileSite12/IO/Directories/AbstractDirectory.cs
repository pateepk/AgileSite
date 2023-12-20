using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace CMS.IO
{
    /// <summary>
    /// Abstract class for directory providers.
    /// </summary>
    public abstract class AbstractDirectory
    {
        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">Path to test</param>
        public abstract bool Exists(string path);


        /// <summary>
        /// Creates all directories and subdirectories as specified by path.
        /// </summary>
        /// <param name="path">Path to create</param>
        public abstract DirectoryInfo CreateDirectory(string path);


        /// <summary>
        /// Returns an enumerable collection of file names in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/>.</returns>
        /// <remarks>
        /// This method is identical to <see cref="EnumerateFiles(string,string)"/> with the asterisk (*) specified as the search pattern.
        /// </remarks>
        public virtual IEnumerable<string> EnumerateFiles(string path)
        {
            return EnumerateFiles(path, "*");
        }


        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory, or an empty array if no files are found.</returns>
        /// <remarks>
        /// This method is identical to <see cref="GetFiles(string,string)"/> with the asterisk (*) specified as the search pattern.
        /// </remarks>
        public virtual string[] GetFiles(string path)
        {
            return GetFiles(path, "*");
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        public abstract IEnumerable<string> EnumerateFiles(string path, string searchPattern);


        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
        public abstract string[] GetFiles(string path, string searchPattern);


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
        public virtual IEnumerable<string> EnumerateDirectories(string path)
        {
            return EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
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
        public virtual string[] GetDirectories(string path)
        {
            return GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="recursive">If delete if subdirs exists</param>
        public abstract void Delete(string path, bool recursive);


        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public abstract void Delete(string path);


        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="sourceDirName">Source directory name</param>
        /// <param name="destDirName">Destination directory name</param>
        public abstract void Move(string sourceDirName, string destDirName);


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        /// <remarks>
        /// If you need to search subdirectories recursively, use the <see cref="EnumerateDirectories(string,string,SearchOption)"/> method, which enables you to specify a search of all subdirectories.
        /// </remarks>
        public virtual IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
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
        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path"/> and that match the specified search pattern and option.</returns>
        public abstract IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);


        /// <summary>
        /// Gets the names of the subdirectories (including their paths) that match the specified search pattern in the current directory,
        /// and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
        public abstract string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);


        /// <summary>
        /// Gets a DirectorySecurity  object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public abstract DirectorySecurity GetAccessControl(string path);


        /// <summary>
        /// Prepares files for import. Converts them to media library.
        /// </summary>
        /// <param name="path">Path.</param>
        public abstract void PrepareFilesForImport(string path);


        /// <summary>
        /// Deletes all files in the directory structure. It works also in a shared hosting environment.
        /// </summary>
        /// <param name="path">Full path of the directory to delete</param>
        public abstract void DeleteDirectoryStructure(string path);


        /// <summary>
        /// Returns search condition delegate.
        /// </summary>
        /// <param name="searchPattern">Can be a combination of literal and wildcard characters, but doesn't support regular expressions. Supports only <c>*</c> and <c>?</c></param>
        protected Func<string, bool> GetSearchCondition(string searchPattern)
        {
            return DirectoryHelper.GetSearchCondition(searchPattern);
        }
    }
}
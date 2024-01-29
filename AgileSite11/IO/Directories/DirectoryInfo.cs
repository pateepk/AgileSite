using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMS.IO
{
    /// <summary>
    /// Exposes instance methods for creating, moving, and enumerating through directories and subdirectories.
    /// </summary>
    public abstract class DirectoryInfo
    {
        #region "Constructors"

        /// <summary>
        /// Creates new instance.
        /// </summary>
        protected DirectoryInfo()
        {
        }

        #endregion


        #region "Methods for creating new instances"

        /// <summary>
        /// Creates new instance of directory info object.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public static DirectoryInfo New(string path)
        {
            return StorageHelper.GetDirectoryInfo(path);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Full name of directory (whole path).
        /// </summary>
        public abstract string FullName
        {
            get;
            set;
        }


        /// <summary>
        /// Last write time to directory.
        /// </summary>
        public abstract DateTime LastWriteTime
        {
            get;
            set;
        }


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Creation time.
        /// </summary>
        public abstract DateTime CreationTime
        {
            get;
            set;
        }


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public abstract bool Exists
        {
            get;
            set;
        }


        /// <summary>
        /// Parent directory.
        /// </summary>
        public abstract DirectoryInfo Parent
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create</param>
        public IO.DirectoryInfo CreateSubdirectory(string subdir)
        {
            return CreateSubdirectoryInternal(subdir);
        }


        /// <summary>
        /// Deletes directory.
        /// </summary>
        public void Delete()
        {
            DeleteInternal();

            StorageSynchronization.LogDirectoryDeleteTask(FullName);
        }


        /// <summary>
        /// Returns an enumerable collection of file information in the current directory.
        /// </summary>
        /// <returns>An enumerable collection of the files in the current directory.</returns>
        public IEnumerable<FileInfo> EnumerateFiles()
        {
            return EnumerateFiles("*", SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a search pattern.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/>.</returns>
        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            var files = EnumerateFilesInternal(searchPattern, searchOption);

            FileDebug.LogFileOperation(FullName, FileDebugOperation.ENUMERATE_FILES, -1, null, null, IOProviderName.FileSystem);

            return files;
        } 


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>        
        public IO.FileInfo[] GetFiles()
        {
            return GetFiles("*", SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>              
        public IO.FileInfo[] GetFiles(string searchPattern)
        {
            return GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search options</param>      
        public IO.FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            var files = GetFilesInternal(searchPattern, searchOption);

            FileDebug.LogFileOperation(FullName, FileDebugOperation.GET_FILES, -1, files.Length.ToString(), null, IOProviderName.FileSystem);

            return files;
        }


        /// <summary>
        /// Returns an enumerable collection of directory information in the current directory.
        /// </summary>
        /// <returns>An enumerable collection of directories in the current directory.</returns>
        public IEnumerable<DirectoryInfo> EnumerateDirectories()
        {
            return EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/>.</returns>
        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            var dirs = EnumerateDirectoriesInternal(searchPattern, searchOption);

            FileDebug.LogFileOperation(FullName, FileDebugOperation.ENUMERATE_DIRECTORIES, -1, null, null, IOProviderName.FileSystem);

            return dirs;
        } 


        /// <summary>
        /// Returns the subdirectories of the current directory.
        /// </summary>        
        public IO.DirectoryInfo[] GetDirectories()
        {
            return GetDirectories("*", SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns the subdirectories of the current directory.
        /// </summary>        
        /// <param name="searchPattern">Search pattern</param>
        public IO.DirectoryInfo[] GetDirectories(string searchPattern)
        {
            return GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }


        /// <summary>
        /// Returns an array of directories in the current DirectoryInfo matching the given search criteria and using a value
        /// to determine whether to search subdirectories.
        /// </summary>        
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        public IO.DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            var dirs = GetDirectoriesInternal(searchPattern, searchOption);

            FileDebug.LogFileOperation(FullName, FileDebugOperation.GET_DIRECTORIES, -1, dirs.Length.ToString(), null, IOProviderName.FileSystem);

            return dirs.ToArray();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create</param>
        protected abstract DirectoryInfo CreateSubdirectoryInternal(string subdir);


        /// <summary>
        /// Deletes directory.
        /// </summary>
        protected abstract void DeleteInternal();


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected abstract IEnumerable<FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption); 


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search options</param>        
        protected abstract FileInfo[] GetFilesInternal(string searchPattern, SearchOption searchOption);


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected abstract IEnumerable<DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption); 

        
        /// <summary>
        /// Returns an array of directories in the current DirectoryInfo matching the given search criteria and using a value
        /// to determine whether to search subdirectories.
        /// </summary>        
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        protected abstract DirectoryInfo[] GetDirectoriesInternal(string searchPattern, SearchOption searchOption);


        /// <summary>
        /// Returns search condition delegate.
        /// </summary>
        /// <param name="searchPattern">Can be a combination of literal and wildcard characters, but doesn't support regular expressions. Supports only <c>*</c> and <c>?</c></param>
        protected Func<string, bool> GetSearchCondition(string searchPattern)
        {
            return DirectoryHelper.GetSearchCondition(searchPattern);
        }

        #endregion
    }
}
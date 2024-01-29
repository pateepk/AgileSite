using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.IO.Zip
{
    /// <summary>
    /// Directory operation provider for zip files
    /// </summary>
    public class ZipDirectoryInfo : DirectoryInfo
    {
        #region "Variables"

        private readonly string mFullName = string.Empty;
        private readonly string mName = string.Empty;


        /// <summary>
        /// List of inner files
        /// </summary>
        private readonly List<ZipFileInfo> mFiles = new List<ZipFileInfo>();

        /// <summary>
        /// List of inner directories
        /// </summary>
        private readonly List<ZipDirectoryInfo> mDirectories = new List<ZipDirectoryInfo>();

        /// <summary>
        /// Parent provider
        /// </summary>
        private readonly ZipStorageProvider mProvider;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Full name of directory (whole path).
        /// </summary>
        public override string FullName
        {
            get
            {
                return mFullName;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Last write time to directory.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                return mProvider.ZipFile.LastWriteTime;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                return mName;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Creation time.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                return mProvider.ZipFile.CreationTime;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Parent directory.
        /// </summary>
        public override DirectoryInfo Parent
        {
            get
            {
                var path = Path.GetDirectoryName(FullName);

                // Try to get from the provider
                var result = mProvider.GetDirectoryInfo(path);
                if (result == null)
                {
                    // If not found, search in the general file system
                    result = New(path);
                }

                return result;
            }            
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Parent provider</param>
        /// <param name="path">Path to directory</param>
        public ZipDirectoryInfo(ZipStorageProvider provider, string path)
        {
            mFullName = path;
            mName = Path.GetFileName(path);

            mProvider = provider;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the file within the directory
        /// </summary>
        public void RegisterFile(ZipFileInfo file)
        {
            mFiles.Add(file);
        }


        /// <summary>
        /// Registers the subdirectory within the directory
        /// </summary>
        public void RegisterDirectory(ZipDirectoryInfo dir)
        {
            mDirectories.Add(dir);
        }
        

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create</param>
        protected override DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            ZipStorageProvider.ThrowReadOnly();
            return null;
        }


        /// <summary>
        /// Deletes directory.
        /// </summary>
        protected override void DeleteInternal()
        {
            ZipStorageProvider.ThrowReadOnly();
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption)
        {
            FileDebug.LogFileOperation(FullName, FileDebugOperation.ENUMERATE_FILES, -1, null, null, IOProviderName.Zip);

            return EnumerateFilesCore(searchPattern, searchOption);
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        private IEnumerable<FileInfo> EnumerateFilesCore(string searchPattern, SearchOption searchOption)
        {
            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);

            IEnumerable<FileInfo> files = mFiles.Where(f => searchCondition(f.Name));

            if (searchOption == SearchOption.AllDirectories)
            {
                files = files.Concat(mDirectories.SelectMany(d => d.EnumerateFiles(searchPattern, searchOption)));
            }

            return files;
        }


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search options</param>      
        protected override FileInfo[] GetFilesInternal(string searchPattern, SearchOption searchOption)
        {
            var files = EnumerateFilesCore(searchPattern, searchOption).ToArray();

            FileDebug.LogFileOperation(FullName, FileDebugOperation.GET_FILES, -1, files.Length.ToString(), null, IOProviderName.Zip);
                        
            return files;
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            FileDebug.LogFileOperation(FullName, FileDebugOperation.ENUMERATE_DIRECTORIES, -1, null, searchPattern, IOProviderName.Zip);

            return EnumerateDirectoriesCore(searchPattern, searchOption);
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        private IEnumerable<DirectoryInfo> EnumerateDirectoriesCore(string searchPattern, SearchOption searchOption)
        {
            if (searchOption != SearchOption.TopDirectoryOnly)
            {
                throw new NotSupportedException("Zip files support listing only directories in the given level.");
            }

            Func<string, bool> searchCondition = GetSearchCondition(searchPattern);
            return mDirectories.Where(d => searchCondition(d.Name));
        }


        /// <summary>
        /// Returns the subdirectories of the current directory.
        /// </summary>        
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        protected override DirectoryInfo[] GetDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            var directories = EnumerateDirectoriesCore(searchPattern, searchOption).ToArray();

            FileDebug.LogFileOperation(FullName, FileDebugOperation.GET_DIRECTORIES, -1, directories.Length.ToString(), searchPattern, IOProviderName.Zip);

            return directories;
        }

        #endregion
    }
}
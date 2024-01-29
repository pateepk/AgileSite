using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.IO;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Directory info for Amazon storage provider
    /// </summary>
    public class DirectoryInfo : IO.DirectoryInfo
    {
        #region "Variables"

        // Memory variables for properties
        DateTime mLastWriteTime = DateTimeHelper.ZERO_TIME;
        DateTime mCreationTime = DateTimeHelper.ZERO_TIME;

        readonly string currentPath;

        readonly System.IO.DirectoryInfo systemDirectory;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public DirectoryInfo(string path)
        {
            currentPath = PathHelper.GetValidPath(path);

            // Get path without ending \.
            currentPath = currentPath.TrimEnd('\\');

            if (Directory.ExistsInFileSystem(path))
            {
                systemDirectory = new System.IO.DirectoryInfo(currentPath);
            }

            InitCMSValues();
        }

        #endregion


        #region "Properties - overrides"

        /// <summary>
        /// Creation time.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                return mCreationTime;
            }
            set
            {
                mCreationTime = value;
            }
        }


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public override bool Exists
        {
            get;
            set;
        }


        /// <summary>
        /// Full name of directory (whole path).
        /// </summary>
        public override string FullName
        {
            get;
            set;
        }


        /// <summary>
        /// Last write time to directory.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                return mLastWriteTime;
            }
            set
            {
                mLastWriteTime = value;
            }
        }


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public override string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Parent directory
        /// </summary>
        public override IO.DirectoryInfo Parent
        {
            get
            {
                // Directory exists in filesystem
                if (systemDirectory != null)
                {
                    System.IO.DirectoryInfo parent = systemDirectory.Parent;

                    DirectoryInfo parentDirectory = new DirectoryInfo(parent.FullName);
                    parentDirectory.CreationTime = parent.CreationTime;
                    parentDirectory.Exists = parent.Exists;
                    parentDirectory.FullName = parent.FullName;
                    parentDirectory.LastWriteTime = parent.LastWriteTime;
                    parentDirectory.Name = parent.Name;

                    return parentDirectory;
                }
                // Directory exists in Amazon S3 storage
                if (!string.IsNullOrEmpty(currentPath))
                {
                    // Get parent directory name
                    string parentDirectoryPath = Path.GetDirectoryName(currentPath);

                    DirectoryInfo parentDirectory = new DirectoryInfo(parentDirectoryPath);
                    parentDirectory.Exists = true;
                    parentDirectory.FullName = parentDirectoryPath;
                    parentDirectory.Name = Path.GetFileName(parentDirectoryPath);

                    return parentDirectory;
                }
                return this;
            }
        }

        #endregion


        #region "Methods - overrides"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create.</param>
        protected override IO.DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            return IO.Directory.CreateDirectory(FullName + "\\" + subdir);
        }


        /// <summary>
        /// Deletes directory
        /// </summary>
        protected override void DeleteInternal()
        {
            IO.Directory.Delete(currentPath, true);
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<IO.DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            var dirs = IO.Directory.EnumerateDirectories(FullName, searchPattern, searchOption);

            return dirs.Select(d => new DirectoryInfo(d));
        }


        /// <summary>
        /// Returns an array of directories in the current DirectoryInfo matching the given search criteria and using a value
        /// to determine whether to search subdirectories.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        protected override IO.DirectoryInfo[] GetDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectoriesInternal(searchPattern, searchOption).ToArray();
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<IO.FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption)
        {
            IEnumerable<IO.FileInfo> files = IO.Directory.EnumerateFiles(FullName, searchPattern).Select(f => new FileInfo(f));

            if (searchOption == SearchOption.AllDirectories)
            {
                var dirs = IO.Directory.EnumerateDirectories(FullName);
                
                files = files.Concat(dirs.SelectMany(d => new DirectoryInfo(d).EnumerateFiles(searchPattern, searchOption)));
            }

            return files;
        }


        /// <summary>
        /// Returns files of the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param> 
        /// <param name="searchOption">Search option.</param>
        protected override IO.FileInfo[] GetFilesInternal(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFilesInternal(searchPattern, searchOption).ToArray();
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Initializes CMS values by System.IO values
        /// </summary>     
        private void InitCMSValues()
        {
            if (systemDirectory != null)
            {
                InitCMSValuesFromSystemDirectory();
            }
            else
            {
                InitCMSValuesFromS3ObjectInfo();
            }
        }


        /// <summary>
        /// Initializes CMS values by System.IO values
        /// </summary>
        private void InitCMSValuesFromSystemDirectory()
        {
            if (systemDirectory != null)
            {
                CreationTime = systemDirectory.CreationTime;
                Exists = systemDirectory.Exists;
                FullName = systemDirectory.FullName;
                LastWriteTime = systemDirectory.LastWriteTime;
                Name = systemDirectory.Name;
            }
        }


        /// <summary>
        /// Initializes CMS values by S3 object. 
        /// </summary>
        private void InitCMSValuesFromS3ObjectInfo()
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                Exists = Directory.ExistsInS3Storage(currentPath);
                FullName = currentPath;
                Name = Path.GetFileName(FullName);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.IO;

using Microsoft.WindowsAzure.Storage.Blob;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Implementation of DirectoryInfo object for Azure.
    /// </summary>
    public class DirectoryInfo : IO.DirectoryInfo
    {
        #region "Variables"

        // Memory variables for properties

        private readonly ContainerInfo mContainer;
        private string mCurrentPath;

        private readonly System.IO.DirectoryInfo mSystemDirectory;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Full name of directory (whole path).
        /// </summary>
        public override string FullName
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Last write time to directory.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get;
            set;
        } = DateTimeHelper.ZERO_TIME;


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public override string Name
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Creation time.
        /// </summary>
        public override DateTime CreationTime
        {
            get;
            set;
        } = DateTimeHelper.ZERO_TIME;


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public override bool Exists
        {
            get;
            set;
        }


        /// <summary>
        /// Parent directory.
        /// </summary>
        public override IO.DirectoryInfo Parent
        {
            get
            {
                // Directory exists in filesystem
                if (mSystemDirectory != null)
                {
                    if (mSystemDirectory.Parent == null)
                    {
                        return null;
                    }

                    System.IO.DirectoryInfo parent = mSystemDirectory.Parent;

                    DirectoryInfo parentDirectory = new DirectoryInfo(parent.FullName);
                    parentDirectory.CreationTime = parent.CreationTime;
                    parentDirectory.Exists = parent.Exists;
                    parentDirectory.FullName = parent.FullName;
                    parentDirectory.LastWriteTime = parent.LastWriteTime;
                    parentDirectory.Name = parent.Name;

                    return parentDirectory;
                }
                // Directory exists in blob storage
                else
                {
                    // Get parent directory name
                    string parentDirectoryPath = Path.GetDirectoryName(mCurrentPath);

                    DirectoryInfo parentDirectory = new DirectoryInfo(parentDirectoryPath);
                    parentDirectory.Exists = true;
                    parentDirectory.FullName = parentDirectoryPath;
                    parentDirectory.Name = Path.GetFileName(parentDirectoryPath);

                    return parentDirectory;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public DirectoryInfo(string path)
        {
            mCurrentPath = Directory.GetValidPath(path);

            mContainer = ContainerInfoProvider.GetRootContainerInfo(path);
            if (Directory.ExistsInFileSystem(path))
            {
                mSystemDirectory = new System.IO.DirectoryInfo(mCurrentPath);
            }

            InitCMSValues();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create.</param>
        protected override IO.DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            return IO.Directory.CreateDirectory(FullName + "\\" + subdir);
        }


        /// <summary>
        /// Deletes directory.
        /// </summary>
        protected override void DeleteInternal()
        {
            if (Directory.ExistsInBlobStorage(mCurrentPath))
            {
                mCurrentPath = Directory.GetBlobPathFromPath(mCurrentPath);

                // Get all blobs and directories
                foreach (var item in mContainer.BlobContainer.GetDirectoryReference(mCurrentPath).ListBlobs())
                {
                    // Delete blob
                    var blob = item as CloudBlockBlob;
                    if (blob != null)
                    {
                        blob.Delete();
                    }
                    // Delete subdirectory
                    else if (item is CloudBlobDirectory)
                    {
                        IO.Directory.Delete(Directory.GetPathFromUri(item.Uri, true));
                    }
                }
            }

            if (mSystemDirectory != null)
            {
                System.IO.Directory.Delete(mCurrentPath);
            }
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
        /// <param name="searchOption">Whether return files from top directory or also from any subdirectories.</param>
        protected override IO.FileInfo[] GetFilesInternal(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFilesInternal(searchPattern, searchOption).ToArray();
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Initializes CMS values by System.IO values.
        /// </summary>     
        private void InitCMSValues()
        {
            if (mSystemDirectory != null)
            {
                InitCMSValuesFromSystemDirectory();
            }
            else
            {
                InitCMSValuesFromBlob();
            }
        }


        /// <summary>
        /// Initializes CMS values by System.IO values.
        /// </summary>
        private void InitCMSValuesFromSystemDirectory()
        {
            if (mSystemDirectory != null)
            {
                CreationTime = mSystemDirectory.CreationTime;
                Exists = mSystemDirectory.Exists;
                FullName = mSystemDirectory.FullName;
                LastWriteTime = mSystemDirectory.LastWriteTime;
                Name = mSystemDirectory.Name;
            }
        }


        /// <summary>
        /// Initializes CMS values by azure container values.
        /// </summary>
        private void InitCMSValuesFromBlob()
        {
            Exists = Directory.ExistsInBlobStorage(mCurrentPath);
            FullName = mCurrentPath;
            Name = Path.GetFileName(FullName);
        }

        #endregion
    }
}

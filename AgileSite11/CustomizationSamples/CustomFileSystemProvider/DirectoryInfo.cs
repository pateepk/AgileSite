using System;
using System.Collections.Generic;

using CMS.IO;

namespace CMS.CustomFileSystemProvider
{
    /// <summary>
    /// Sample of DirectoryInfo class object of CMS.IO provider.
    /// </summary>
    class DirectoryInfo : CMS.IO.DirectoryInfo
    {

        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public DirectoryInfo(string path)
        {
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Full name of directory (whole path).
        /// </summary>
        public override string FullName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Last write time to directory.
        /// </summary>
        public override DateTime LastWriteTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Creation time.
        /// </summary>
        public override DateTime CreationTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Parent directory.
        /// </summary>
        public override CMS.IO.DirectoryInfo Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create.</param>
        protected override CMS.IO.DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes directory
        /// </summary>
        protected override void DeleteInternal()
        {
            throw new NotImplementedException();
        }
        

        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<CMS.IO.DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns an array of directories in the current DirectoryInfo matching the given search criteria and using a value
        /// to determine whether to search subdirectories.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        protected override CMS.IO.DirectoryInfo[] GetDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<CMS.IO.FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns files of the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">Whether return files from top directory or also from any subdirectories.</param>
        protected override CMS.IO.FileInfo[] GetFilesInternal(string searchPattern, CMS.IO.SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

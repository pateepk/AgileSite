using System;
using System.Collections.Generic;
using System.Linq;

using CMS.IO;

namespace CMS.FileSystemStorage
{
    /// <summary>
    /// Envelope for System.IO.DirectoryInfo.
    /// </summary>
    public class DirectoryInfo : IO.DirectoryInfo
    {
        #region "Variables"

        private readonly System.IO.DirectoryInfo systemDirectory;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Path to directory</param>
        public DirectoryInfo(string path)
        {
            path = AbstractStorageProvider.GetTargetPhysicalPath(path);
            systemDirectory = new System.IO.DirectoryInfo(path);
        }


        /// <summary>
        /// Initializes new instance of directory info.
        /// </summary>
        /// <param name="info">System info</param>
        internal DirectoryInfo(System.IO.DirectoryInfo info)
        {
            systemDirectory = info;
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
                return systemDirectory.FullName;
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
                return systemDirectory.LastWriteTime;
            }
            set
            {
                systemDirectory.LastWriteTime = value;
            }
        }


        /// <summary>
        /// Name of directory (without path).
        /// </summary>
        public override string Name
        {
            get
            {
                return systemDirectory.Name;
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
                return systemDirectory.CreationTime;
            }
            set
            {
                systemDirectory.CreationTime = value;
            }
        }


        /// <summary>
        /// Whether directory exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return systemDirectory.Exists;
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Parent directory.
        /// </summary>
        public override IO.DirectoryInfo Parent
        {
            get
            {
                System.IO.DirectoryInfo systemParent = systemDirectory.Parent;
                DirectoryInfo parent = new DirectoryInfo(systemParent);
                return parent;
            }            
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates subdirectory.
        /// </summary>
        /// <param name="subdir">Subdirectory to create</param>
        protected override IO.DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            System.IO.DirectoryInfo subDirectorySystem = systemDirectory.CreateSubdirectory(subdir);
            DirectoryInfo subDirectoryCMS = new DirectoryInfo(subDirectorySystem);
            return subDirectoryCMS;
        }


        /// <summary>
        /// Deletes directory.
        /// </summary>
        protected override void DeleteInternal()
        {
            systemDirectory.Delete();

            StorageSynchronization.LogDirectoryDeleteTask(FullName);
        }


        /// <summary>
        /// Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<IO.FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption)
        {
            return systemDirectory.EnumerateFiles(searchPattern, (System.IO.SearchOption)searchOption).Select(f => new FileInfo(f));
        }


        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search options</param>      
        protected override IO.FileInfo[] GetFilesInternal(string searchPattern, SearchOption searchOption)
        {
            List<FileInfo> files = new List<FileInfo>();

            foreach (System.IO.FileInfo info in systemDirectory.GetFiles(searchPattern, (System.IO.SearchOption)searchOption))
            {
                FileInfo tmp = new FileInfo(info);
                files.Add(tmp);
            }

            return files.ToArray();
        }


        /// <summary>
        /// Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.
        /// </summary>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern"/> and <paramref name="searchOption"/>.</returns>
        protected override IEnumerable<IO.DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            return systemDirectory.EnumerateDirectories(searchPattern, (System.IO.SearchOption)searchOption).Select(d => new DirectoryInfo(d));
        }


        /// <summary>
        /// Returns an array of directories in the current DirectoryInfo matching the given search criteria and using a value
        /// to determine whether to search subdirectories.
        /// </summary>        
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Specifies whether to search the current directory, or the current directory and all subdirectories.</param>
        protected override IO.DirectoryInfo[] GetDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            List<DirectoryInfo> dirs = new List<DirectoryInfo>();

            foreach (System.IO.DirectoryInfo info in systemDirectory.GetDirectories(searchPattern, (System.IO.SearchOption)searchOption))
            {
                DirectoryInfo tmp = new DirectoryInfo(info);
                dirs.Add(tmp);
            }

            return dirs.ToArray();
        }
        #endregion
    }
}
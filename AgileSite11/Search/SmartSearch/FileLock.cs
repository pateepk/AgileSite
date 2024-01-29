using System;

using CMS.IO;

namespace CMS.Search.Internal
{
    /// <summary>
    /// Provides instance methods locking functionality. Uses file in file system for this purpose.
    /// </summary>
    public class FileLock
    {
        #region "Variables"

        private readonly string mDirectoryPath;
        private readonly string mFullPath;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of FileLock class.
        /// </summary>
        /// <param name="dirPath">Path to directory.</param>
        /// <param name="lockName">Lock file name.</param>
        public FileLock(string dirPath, string lockName)
        {
            mDirectoryPath = dirPath;
            mFullPath = DirectoryHelper.CombinePath(mDirectoryPath, lockName);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns whether current file lock exists.
        /// </summary>
        public bool IsLocked()
        {
            return File.Exists(mFullPath);
        }

        
        /// <summary>
        /// Obtains file lock for web farm instance given by <paramref name="instanceName"/>, which is stored inside lock file.
        /// </summary>
        /// <param name="instanceName">Name of instance for which to obtain lock.</param>
        internal bool ObtainForInstance(string instanceName)
        {
            if (IsLocked())
            {
                return String.Equals(File.ReadAllText(mFullPath), instanceName, StringComparison.OrdinalIgnoreCase);
            }

            try
            {
                Directory.CreateDirectory(mDirectoryPath);
                File.WriteAllText(mFullPath, instanceName);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Obtains file lock.
        /// </summary>
        public bool Obtain()
        {
            // If is already created
            if (IsLocked())
            {
                return false;
            }

            try
            {
                // Create directory
                Directory.CreateDirectory(mDirectoryPath);

                // Create empty file
                File.Create(mFullPath).Close();
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Releases file lock.
        /// </summary>
        public void Release()
        {
            if (File.Exists(mFullPath))
            {
                File.Delete(mFullPath);
            }
        }

        #endregion
    }
}

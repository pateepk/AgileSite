using CMS.Search.Internal;

using Lucene.Net.Store;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Provides instance methods for locking files. Primary used by Smart search module when Index writer writes data into index.
    /// </summary>
    internal class SearchLock : Lock
    {
        #region "Variables"

        private readonly FileLock mLockObj;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of SearchLock class.
        /// </summary>
        /// <param name="dirPath">Path to directory.</param>
        /// <param name="lockName">Lock file name.</param>
        public SearchLock(string dirPath, string lockName)
        {
            mLockObj = new FileLock(dirPath, lockName);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns whether current directory is locked.
        /// </summary>
        public override bool IsLocked()
        {
            return mLockObj.IsLocked();
        }


        /// <summary>
        /// Obtains lock for directory.
        /// </summary>
        public override bool Obtain()
        {
            return mLockObj.Obtain();
        }


        /// <summary>
        /// Obtains lock for directory.
        /// </summary>
        /// <param name="lockWaitTimeout">This parameter is not used in out CMS implementation - because of Azure.</param>
        public override bool Obtain(long lockWaitTimeout)
        {
            return mLockObj.Obtain();
        }


        /// <summary>
        /// Releases lock for directory.
        /// </summary>
        public override void Release()
        {
            mLockObj.Release();
        }

        #endregion
    }
}
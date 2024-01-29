using System;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Locked section, locks only when the object lock is available.
    /// </summary>
    public class LockedSection : IDisposable
    {
        #region "Variables"

        /// <summary>
        /// Object for the synchronization.
        /// </summary>
        protected object mLock = null;

        /// <summary>
        /// Boolean condition for the locking.
        /// </summary>
        protected bool mBoolCondition = true;

        /// <summary>
        /// Flag whether the execution was blocked by another code or not.
        /// </summary>
        protected bool mWasBlocked = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns whether the execution was blocked by another code or not.
        /// </summary>
        public bool WasBlocked
        {
            get
            {
                return mWasBlocked;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, locks the section on the given string key.
        /// </summary>
        /// <param name="condition">Boolean condition for the locking</param>
        /// <param name="lockKey">String key to use for the lock</param>
        public LockedSection(bool condition, string lockKey)
        {
            if (condition && !String.IsNullOrEmpty(lockKey))
            {
                // Get the lock object
                mLock = LockHelper.GetLockObject(lockKey);

                Enter();
            }
        }


        /// <summary>
        /// Constructor, locks the section on specific object.
        /// </summary>
        /// <param name="condition">Boolean condition for the locking</param>
        /// <param name="lockObject">Lock object</param>
        public LockedSection(bool condition, object lockObject)
        {
            // Lock the thread to ensure one load only
            if (condition && (lockObject != null))
            {
                mLock = lockObject;

                Enter();
            }
        }


        /// <summary>
        /// Enters the monitor.
        /// </summary>
        protected void Enter()
        {
            // Try entering the monitor
            if (!Monitor.TryEnter(mLock))
            {
                // Must wait
                mWasBlocked = true;

                Monitor.Enter(mLock);
            }
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            // Release the lock
            if (mLock != null)
            {
                Monitor.Exit(mLock);
            }
        }

        #endregion
    }
}
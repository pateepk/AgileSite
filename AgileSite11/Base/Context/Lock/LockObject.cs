using System;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Locking object
    /// </summary>
    public class LockObject
    {
        #region "Variables"

        /// <summary>
        /// Synchronization lock for potentially colliding operations
        /// </summary>
        private readonly object syncLock = new object();

        /// <summary>
        /// Full access lock
        /// </summary>
        private readonly object fullLock = new object();

        /// <summary>
        /// Read lock
        /// </summary>
        private readonly object readLock = new object();


        /// <summary>
        /// Number of readers waiting for the data
        /// </summary>
        private int mDataReferences;

        #endregion


        #region "Invalid data"

        /// <summary>
        /// Invalid data container
        /// </summary>
        private class InvalidData
        {
            /// <summary>
            /// Invalid data
            /// </summary>
            public static readonly InvalidData Value = new InvalidData();
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Flag indicating that the read failed
        /// </summary>
        public bool ReadFailed
        {
            get;
            protected set;
        }


        /// <summary>
        /// Read data
        /// </summary>
        protected object Data
        {
            get;
            set;
        }


        /// <summary>
        /// Exception that occurred within data reading
        /// </summary>
        public Exception Exception 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Lock object key
        /// </summary>
        public string Key
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public LockObject(string key = null)
        {
            Data = InvalidData.Value;
            Key = key;
        }


        /// <summary>
        /// Attempts to enter read lock. Returns true if the lock was acquired
        /// </summary>
        /// <param name="output">Returns the read data in case other thread is reading. Waits until the other reading finishes before returning the data.</param>
        public bool EnterRead<OutputType>(ref OutputType output)
        {
            // Try to enter the read state
            if (TryEnterRead())
            {
                return true;
            }

            // Wait for output from other threads
            if (WaitForOutput(out output))
            {
                return false;
            }

            // Wasn't able to retrieve output from other thread (typically due to unhandled exception), start reading on its own
            return EnterRead(ref output);
        }


        /// <summary>
        /// Tries to enter the read state, returns true if the read lock was acquired. If the read lock is acquired, resets internal state so that other threads can receive the result.
        /// </summary>
        private bool TryEnterRead()
        {
            bool lockAcquired;
            
            lock (syncLock)
            {
                // Add data reference
                AddDataReference();

                lockAcquired = Monitor.TryEnter(readLock);
            }

            if (lockAcquired)
            {
                // Lock complete access
                Monitor.Enter(fullLock);

                // Resets the lock object state
                ResetState();

                return true;
            }

            return false;
        }


        /// <summary>
        /// Waits for the output from other thread that is reading the data. Returns true, if the output was provided.
        /// </summary>
        /// <param name="output">Returns the read data in case other thread is reading. Waits until the other reading finishes before returning the data.</param>
        private bool WaitForOutput<OutputType>(out OutputType output)
        {
            // Wait for the other thread to finish
            Monitor.Enter(readLock);

            Exception ex;
            bool invalid;

            lock (syncLock)
            {
                // Read the values
                ex = Exception;
                invalid = (Data == InvalidData.Value);

                output = default(OutputType);

                if (!invalid)
                {
                    output = (OutputType)Data;
                }

                // Release data reference
                ReleaseDataReference();
            }

            Monitor.Exit(readLock);

            // Throw the exception in case the master thread failed
            if (ex != null)
            {
                throw ex;
            }

            // Report whether providing data from other thread was successful or not
            return !invalid;
        }


        /// <summary>
        /// Enters the data reading process
        /// </summary>
        private void AddDataReference()
        {
            Interlocked.Increment(ref mDataReferences);
        }


        /// <summary>
        /// Exists the data reading process
        /// </summary>
        private void ReleaseDataReference()
        {
            if (Interlocked.Decrement(ref mDataReferences) <= 0)
            {
                Exception = null;
                Data = InvalidData.Value;
            }
        }
                

        /// <summary>
        /// Resets the lock object state
        /// </summary>
        private void ResetState()
        {
            Exception = null;
            Data = InvalidData.Value;
            ReadFailed = false;
        }


        /// <summary>
        /// Finishes the read operation with failure
        /// </summary>
        public void FinishReadFailed()
        {
            lock (syncLock)
            {
                ReadFailed = true;

                Data = InvalidData.Value;

                // Release data reference
                ReleaseDataReference();

                Monitor.Exit(fullLock);
                Monitor.Exit(readLock);
            }
        }


        /// <summary>
        /// Finishes the read operation
        /// </summary>
        /// <param name="output">Output type</param>
        public void FinishRead<OutputType>(OutputType output)
        {
            lock (syncLock)
            {
                Data = output;

                // Release data reference
                ReleaseDataReference();

                Monitor.Exit(fullLock);
                Monitor.Exit(readLock);
            }
        }

        #endregion
    }
}
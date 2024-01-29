using System;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Class to provide lock objects based on the given key.
    /// </summary>
    public class LockHelper
    {
        #region "Variables"

        // Inner list of locking objects
        private static readonly SafeDictionary<string, LockObject> lockObjects = new SafeDictionary<string, LockObject>();

        #endregion
        

        #region "Methods"
        
        /// <summary>
        /// Gets the object for lock.
        /// </summary>
        /// <param name="key">Lock object key</param>
        public static LockObject GetLockObject(string key)
        {
            // Try to get from hashtable
            var result = lockObjects[key];
            if (result == null)
            {
                // Not found, lock table to create new one
                lock (lockObjects)
                {
                    // Check if other thread didn't create it already
                    result = lockObjects[key];
                    if (result == null)
                    {
                        // Create new one
                        result = new LockObject(key);
                        lockObjects[key] = result;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Ensures the value of the given variable in a locked context to prevent multiple loads.
        /// Uses the default constructor of <typeparamref name="T"/> to initialize the value.
        /// </summary>
        /// <param name="variable">Variable to load</param>
        /// <param name="lockObject">Object to use for locking context to ensure only one load</param>
        /// <exception cref="System.ArgumentNullException">Throws when lock object is not defined</exception>
        public static T Ensure<T>(ref T variable, object lockObject)
            where T : class, new()
        {
            if (lockObject == null)
            {
                throw new ArgumentNullException("lockObject");
            }

            if (variable == null)
            {
                lock (lockObject)
                {
                    if (variable == null)
                    {
                        var tmp = new T();

                        // Ensure release-fence (it actually uses full fence internally)
                        Volatile.Write(ref variable, tmp); 
                    }
                }
            }

            return variable;
        }
       

        /// <summary>
        /// Ensures the value of the given variable in a locked context to prevent multiple loads.
        /// Uses the <paramref name="loadMethod"/> to initialize the value.
        /// </summary>
        /// <param name="variable">Variable to load</param>
        /// <param name="loadMethod">Methods that provides the variable value</param>
        /// <param name="lockObject">Object to use for locking context to ensure only one load</param>
        /// <exception cref="System.ArgumentNullException">Throws when lock object is not defined</exception>
        public static T Ensure<T>(ref T variable, Func<T> loadMethod, object lockObject)
            where T : class
        {
            if (lockObject == null)
            {
                throw new ArgumentNullException("lockObject");
            }

            if (variable == null)
            {
                lock (lockObject)
                {
                    if (variable == null)
                    {
                        var tmp = loadMethod();

                        // Ensure release-fence (it actually uses full fence internally)
                        Volatile.Write(ref variable, tmp); 
                    }
                }
            }

            return variable;
        }


        /// <summary>
        /// Ensures that the given action is executed only once
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="lockObject">Object to use for locking context to ensure only one load</param>
        /// <param name="executedFlag">Flag that indicates if the action was already executed</param>
        /// <exception cref="System.ArgumentNullException">Throws when lock object is not defined</exception>
        public static bool ExecuteOnceInLifetime(Action action, object lockObject, ref bool executedFlag)
        {
            if (!executedFlag)
            {
                if (lockObject == null)
                {
                    throw new ArgumentNullException("lockObject");
                }

                // Try to take lock
                if (Monitor.TryEnter(lockObject))
                {
                    if (!executedFlag)
                    {
                        // Only execute if lock was acquired, otherwise skip
                        try
                        {
                            action();
                        }
                        finally
                        {
                            executedFlag = true;

                            // Release taken lock
                            Monitor.Exit(lockObject);
                        }
                    }
                    else
                    {
                        // Release taken lock
                        Monitor.Exit(lockObject);
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
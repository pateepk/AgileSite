using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Base class for the disposable objects that can carry other depending disposable objects
    /// </summary>
    public class DisposableObject : IDisposable
    {
        #region "Variables"

        /// <summary>
        /// List of objects allocated with the where condition
        /// </summary>
        private List<IDisposable> mUsing;

        #endregion


        #region "Methods"

        /// <summary>
        /// Locks the event on the given lock object. The context stays locked until the handler is disposed.
        /// </summary>
        /// <param name="lockObject">Lock object</param>
        public void Lock(object lockObject)
        {
            Monitor.Enter(lockObject);
            CallOnDispose(() => ExitLock(lockObject));
        }


        /// <summary>
        /// Exists the given lock object
        /// </summary>
        /// <param name="lockObject">Lock object to exit</param>
        private static void ExitLock(object lockObject)
        {
            Monitor.Exit(lockObject);
        }


        /// <summary>
        /// Adds the given action to the list of actions called when the handler object is disposed
        /// </summary>
        /// <param name="method">Method to call</param>
        public void CallOnDispose(Action method)
        {
            Using(new OnDisposedCallback(method));
        }


        /// <summary>
        /// Adds the given object to the list of the allocated objects to dispose
        /// </summary>
        /// <param name="obj">Object to register</param>
        public void Using(IDisposable obj)
        {
            if (mUsing == null)
            {
                mUsing = new List<IDisposable>();
            }

            mUsing.Add(obj);
        }


        /// <summary>
        /// Disposes the objects that were previously registered by the Using method
        /// </summary>
        internal void DisposeUsings()
        {
            var usings = mUsing;
            if (usings != null)
            {
                mUsing = null;
                
                // Dispose all objects in the reverse order than registered
                for (int i = usings.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        usings[i].Dispose();
                    }
                    catch //(Exception ex)
                    {
                        // Suppress exceptions to allow proper disposal of all registered objects
                        //CoreServices.EventLog.LogException("Using", "DISPOSE", ex);
                    }
                }
            }
        }


        /// <summary>
        /// Make sure the objects get disposed
        /// </summary>
        [HideFromDebugContext]
        public virtual void Dispose()
        {
            // Make sure the objects get disposed when this object gets disposed
            DisposeUsings();
        }

        #endregion
    }
}

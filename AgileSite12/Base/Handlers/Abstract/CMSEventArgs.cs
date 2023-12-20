using System;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Base class for the CMS event arguments with one generic parameter
    /// </summary>
    public class CMSEventArgs<ParameterType> : CMSEventArgs
    {
        /// <summary>
        /// Event parameter
        /// </summary>
        public ParameterType Parameter 
        { 
            get; 
            set; 
        }
    }


    /// <summary>
    /// Base class for the CMS event arguments
    /// </summary>
    public class CMSEventArgs : EventArgs, IDisposable
    {
        private readonly DisposableObject mDisposable = new DisposableObject();
        private List<Action> mCallWhenFinished;


        /// <summary>
        /// Original event arguments for the event
        /// </summary>
        public EventArgs OriginalEventArgs 
        { 
            get;
            internal set; 
        }


        /// <summary>
        /// Currently executing handler
        /// </summary>
        public AbstractHandler CurrentHandler
        {
            get;
            internal set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public CMSEventArgs()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Original event arguments for the event</param>
        public CMSEventArgs(EventArgs e)
        {
            OriginalEventArgs = e;
        }


        /// <summary>
        /// Locks the event on the given lock object. The context stays locked until the handler is disposed.
        /// </summary>
        /// <param name="lockObject">Lock object</param>
        public CMSEventArgs Lock(object lockObject)
        {
            mDisposable.Lock(lockObject);
            return this;
        }
        

        /// <summary>
        /// Prevents the recursion of the handler execution using the given unique key. The handler won't execute again with the given key until the current execution is finished. Returns true, if recursion is detected and the code shouldn't continue. Otherwise returns false.
        /// </summary>
        /// <param name="recursionKey">Recursion key</param>
        public bool DetectRecursion(string recursionKey)
        {
            var rc = new RecursionControl(recursionKey);

            Using(rc);

            return !rc.Continue;
        }


        /// <summary>
        /// Adds the given object to the list of object that get disposed when the handler object is disposed
        /// </summary>
        /// <param name="obj">Object to dispose</param>
        public CMSEventArgs Using(IDisposable obj)
        {
            mDisposable.Using(obj);
            return this;
        }

        
        /// <summary>
        /// Make sure the objects get disposed
        /// </summary>
        public void Dispose()
        {
            // Dispose the hosted disposable object
            mDisposable.Dispose();
        }


        /// <summary>
        /// Adds the given action to the list of actions called when the handler object is disposed
        /// </summary>
        /// <param name="method">Method to call</param>
        public CMSEventArgs CallOnDispose(Action method)
        {
            mDisposable.CallOnDispose(method);
            return this;
        }


        /// <summary>
        /// Adds the given action to be called when the handler finishes
        /// </summary>
        /// <param name="method">Action to call</param>
        public CMSEventArgs CallWhenFinished(Action method)
        {
            if (mCallWhenFinished == null)
            {
                mCallWhenFinished = new List<Action>();
            }

            mCallWhenFinished.Add(method);

            return this;
        }


        /// <summary>
        /// Disposes the objects that were previously registered by the Using method
        /// </summary>
        internal void CallFinishActions()
        {
            if (mCallWhenFinished != null)
            {
                // Dispose all objects
                foreach (Action method in mCallWhenFinished)
                {
                    method();
                }

                mCallWhenFinished.Clear();
            }
        }


        /// <summary>
        /// Disposes all objects used by this handler
        /// </summary>
        internal void DisposeUsedObjects()
        {
            mDisposable.DisposeUsings();
        }


        /// <summary>
        /// Cancels the current handler execution
        /// </summary>
        public void Cancel()
        {
            var handler = CurrentHandler as AbstractAdvancedHandler;
            if (handler != null)
            {
                handler.Continue = false;
            }
            else
            {
                throw new NotSupportedException("Only Before/After handler type supports canceling.");
            }
        }
    }
}

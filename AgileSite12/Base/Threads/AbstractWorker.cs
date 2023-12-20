using System;

namespace CMS.Base
{
    /// <summary>
    /// Abstract class for the thread worker.
    /// </summary>
    public abstract class AbstractWorker
    {
        /// <summary>
        /// Raised when worker finishes.
        /// </summary>
        public event EventHandler OnStop;


        /// <summary>
        /// If true, the thread is a part of the sequence and should perform the actions after the previous thread finishes.
        /// </summary>
        public bool RunInSequence
        {
            get;
            set;
        }


        /// <summary>
        /// Runs the worker as a new thread.
        /// </summary>
        public virtual void RunAsync()
        {
            var thread = new CMSThread(Run);
            thread.OnStop += OnStop;
            thread.Start(RunInSequence);
        }


        /// <summary>
        /// Raises OnStop event.
        /// </summary>
        protected void RaiseStop()
        {
            if (OnStop != null)
            {
                OnStop(this, null);
            }
        }


        /// <summary>
        /// Runs the action.
        /// </summary>
        public abstract void Run();
    }
}
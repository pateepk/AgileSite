using System;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Thread debug item used in debug report. This class is intended for internal use only. 
    /// </summary>
    public class ThreadDebugItem
    {
        #region "Properties"

        /// <summary>
        /// Gets the thread GUID
        /// </summary>
        public Guid ThreadGuid
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the method class name
        /// </summary>
        public string MethodClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the method name
        /// </summary>
        public string MethodName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the thread id
        /// </summary>
        public int ThreadId
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the request URL
        /// </summary>
        public string RequestUrl
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the value that indicates whether thread has log
        /// </summary>
        public bool HasLog
        {
            get
            {
                CMSThread thread = CMSThread.GetThread(ThreadGuid);
                return ((thread != null) && (thread.Log != null));
            }
        }


        /// <summary>
        /// Gets the thread finished time
        /// </summary>
        public DateTime ThreadFinished
        {
            get;
            internal set;

        }


        /// <summary>
        /// Gets the thread duration in seconds
        /// </summary>
        public double Duration
        {
            get
            {
                DateTime finished = ThreadFinished;
                if (finished == DateTime.MinValue)
                {
                    finished = DateTime.Now;
                }
                double duration = (finished - ThreadStarted).TotalSeconds;
                return duration;
            }
        }


        /// <summary>
        /// Gets the thread status
        /// </summary>
        public string Status
        {
            get
            {
                if (ThreadFinished != DateTime.MinValue)
                {
                    return ThreadState.Stopped.ToString();
                }
                
                CMSThread thread = CMSThread.GetThread(ThreadGuid);
                if ((thread != null) && thread.Mode == ThreadModeEnum.Async)
                {
                    return thread.InnerThread != null ? thread.InnerThread.ThreadState.ToString() : "";
                }

                return ThreadState.Running.ToString();
            }
        }


        /// <summary>
        /// Gets or sets the thread starting time
        /// </summary>
        public DateTime ThreadStarted
        {
            get;
            private set;
        }


        /// <summary>
        /// Request GUID to identify debugs of the current thread
        /// </summary>
        public Guid RequestGuid
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thread">Source thread object</param>
        public ThreadDebugItem(CMSThread thread)
        {
            ThreadGuid = thread.ThreadGUID;
            MethodClassName = thread.MethodClassName;
            MethodName = thread.MethodName;
            ThreadId = thread.ThreadID;
            RequestUrl = thread.RequestUrl;
            ThreadFinished = thread.ThreadFinished;
            ThreadStarted = thread.ThreadStarted;

            var debug = DebugContext.FromThread(thread);
            if (debug != null)
            {
                RequestGuid = debug.RequestGUID;
            }
        }


        /// <summary>
        /// Returns the string representation of the object
        /// </summary>
        public override string ToString()
        {
            return "[Thread ID " + ThreadId + "] " + MethodClassName + "." + MethodName;
        }

        #endregion
    }
}

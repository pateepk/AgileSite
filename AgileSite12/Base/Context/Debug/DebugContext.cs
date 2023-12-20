using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// Debug context
    /// </summary>
    public class DebugContext : AbstractContext<DebugContext>
    {
        #region "Variables"

        private bool mDebugPresentInResponse;
        private Stack<string> mRequestContext;
        private string mCurrentLogFolder;
        private RequestLogs mCurrentRequestLogs;
        private RequestSettings mCurrentRequestSettings;
        private Guid mRequestGUID;
        private Stack<ThreadDebugItem> mThreadStack;
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Request GUID to identify debugs of the current thread
        /// </summary>
        internal Guid RequestGUID
        {
            get
            {
                if (mRequestGUID == default(Guid))
                {
                    mRequestGUID = Guid.NewGuid();
                }

                return mRequestGUID;
            }
            set
            {
                mRequestGUID = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether debug information is present in response
        /// </summary>
        public static bool DebugPresentInResponse
        {
            get
            {
                return Current.mDebugPresentInResponse;
            }
            set
            {
                Current.mDebugPresentInResponse = value;
            }
        }


        /// <summary>
        /// Request context.
        /// </summary>
        public static Stack<string> RequestContext
        {
            get
            {
                if (CMSHttpContext.Current == null)
                {
                    return null;
                }

                var c = Current;

                // Ensure the context stack
                var result = c.mRequestContext;
                if (result == null)
                {
                    result = new Stack<string>();

                    c.mRequestContext = result;
                }

                return result;
            }
        }


        /// <summary>
        /// Current subfolder for debug log
        /// </summary>
        public static string CurrentLogFolder
        {
            get
            {
                return Current.mCurrentLogFolder;
            }
            set
            {
                Current.mCurrentLogFolder = value;
            }
        }


        /// <summary>
        /// Returns the logs for current request.
        /// </summary>
        public static RequestLogs CurrentRequestLogs
        {
            get
            {
                var c = Current;

                var result = c.mCurrentRequestLogs;
                if (result == null)
                {
                    // Create new log table
                    result = new RequestLogs(c.RequestGUID);

                    c.mCurrentRequestLogs = result;
                }

                return result;
            }
            set
            {
                Current.mCurrentRequestLogs = value;
            }
        }


        /// <summary>
        /// Current context identifier.
        /// </summary>
        public static string CurrentExecutionContext
        {
            get
            {
                if (CMSHttpContext.Current == null)
                {
                    return null;
                }

                // Get top context
                var context = RequestContext;
                if ((context != null) && (context.Count > 0))
                {
                    string result = null;
                    string[] items = context.ToArray();
                    for (int i = items.Length - 1; i >= 0; i--)
                    {
                        result += items[i] + "\r\n";
                    }
                    return result;
                }

                return null;
            }
        }


        /// <summary>
        /// Returns current request settings.
        /// </summary>
        public static RequestSettings CurrentRequestSettings
        {
            get
            {
                // Get from the request
                var c = Current;

                var result = c.mCurrentRequestSettings;
                if (result == null)
                {
                    result = new RequestSettings();

                    c.mCurrentRequestSettings = result;
                }

                return result;
            }
            set
            {
                Current.mCurrentRequestSettings = value;
            }
        }


        /// <summary>
        /// Current thread stack
        /// </summary>
        public static Stack<ThreadDebugItem> CurrentThreadStack
        {
            get
            {
                return Current.mThreadStack;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Notifies the debug that a specific thread started in current context
        /// </summary>
        /// <param name="thread">Thread debug item</param>
        internal static void ThreadStarted(ThreadDebugItem thread)
        {
            var c = Current;
            if (c.mThreadStack == null)
            {
                c.mThreadStack = new Stack<ThreadDebugItem>();
            }

            c.mThreadStack.Push(thread);
        }


        /// <summary>
        /// Notifies the debug that a thread finished in current context
        /// </summary>
        internal static void ThreadFinished()
        {
            var c = Current;

            var stack = c.mThreadStack;
            if ((stack != null) && (stack.Count > 0))
            {
                stack.Pop();
            }
        }


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            var c = new DebugContext
            {
                mCurrentLogFolder = mCurrentLogFolder,
            };

            if (mCurrentRequestSettings != null)
            {
                mCurrentRequestSettings = mCurrentRequestSettings.Clone();
            }

            // Clone the thread stack
            if (mThreadStack != null)
            {
                c.mThreadStack = new Stack<ThreadDebugItem>(mThreadStack.Reverse());
            }

            // Do not copy following items as they are not supposed to be distributed to inner thread:

            //c.mCurrentRequestLogs = mCurrentRequestLogs;
            //c.mDebugPresentInResponse = mDebugPresentInResponse;
            //c.mRequestContext = mRequestContext;

            return c;
        }

        #endregion
    }
}

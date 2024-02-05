using System;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Thread debug helper
    /// </summary>
    public static class ThreadDebug
    {
        #region "Variables"

        // Maximum number of the finished threads to keep.
        private static int mMaxFinishedThreadsLogged = -1;

        private static readonly List<ThreadDebugItem> mLiveThreadItems = new List<ThreadDebugItem>();
        private static readonly List<ThreadDebugItem> mFinishedThreadItems = new List<ThreadDebugItem>();

        private static readonly Object liveLocker = new Object();
        private static readonly Object finishedLocker = new Object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Maximum number of finished threads that should be logged by the system.
        /// </summary>
        public static int MaxFinishedThreadsLogged
        {
            get
            {
                if (mMaxFinishedThreadsLogged < 0)
                {
                    mMaxFinishedThreadsLogged = CoreServices.Conversion.GetInteger(SettingsHelper.AppSettings["CMSMaxFinishedThreadsLogged"], DebugHelper.EverythingLogLength);
                }

                return mMaxFinishedThreadsLogged;
            }
            set
            {
                mMaxFinishedThreadsLogged = value;
            }
        }


        /// <summary>
        /// Gets the List of currently running threads. This property is intended for internal use only. 
        /// </summary>
        public static List<ThreadDebugItem> LiveThreadItems
        {
            get
            {
                return mLiveThreadItems;
            }
        }


        /// <summary>
        /// Gets the List of finished threads. This property is intended for internal use only. 
        /// </summary>
        public static List<ThreadDebugItem> FinishedThreadItems
        {
            get
            {
                return mFinishedThreadItems;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the thread to the thread list
        /// </summary>
        public static void ThreadStarted(CMSThread thread)
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                // Add the thread to the live threads
                lock (liveLocker)
                {
                    var item = new ThreadDebugItem(thread);

                    thread.DebugItem = item;
                    mLiveThreadItems.Add(item);

                    DebugContext.ThreadStarted(item);
                }
            }
        }


        /// <summary>
        /// Proceeds the necessary actions when the thread finished
        /// </summary>
        /// <param name="thread">Thread that finished</param>
        public static void ThreadFinished(CMSThread thread)
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                DebugContext.ThreadFinished();

                ThreadDebugItem item;

                lock (liveLocker)
                {
                    item = thread.DebugItem;
                    mLiveThreadItems.Remove(item);
                    thread.DebugItem = null;
                }

                if (MaxFinishedThreadsLogged > 0)
                {
                    // Register the thread within the finished threads
                    lock (finishedLocker)
                    {
                        if (item != null)
                        {
                            item.ThreadFinished = thread.ThreadFinished;
                            mFinishedThreadItems.Add(item);
                            while (mFinishedThreadItems.Count > MaxFinishedThreadsLogged)
                            {
                                mFinishedThreadItems.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
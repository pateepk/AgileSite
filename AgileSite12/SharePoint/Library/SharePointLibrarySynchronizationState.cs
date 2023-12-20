using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// State of the SharePoint library synchronization.
    /// </summary>
    public class SharePointLibrarySynchronizationState
    {
        /// <summary>
        /// True if the synchronization is currently being performed.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return NextRunTime == null;
            }
        }


        /// <summary>
        /// Last run time of the synchronization.
        /// </summary>
        public DateTime? LastRunTime
        {
            get;
            private set;
        }


        /// <summary>
        /// Last result of the synchronization. Null if the synchronization was performed successfully.
        /// </summary>
        public virtual string LastResult
        {
            get;
            private set;
        }


        /// <summary>
        /// Next run time of the synchronization. If the time is from the past, it means the synchronization
        /// is waiting for the scheduler to execute it.
        /// Gets null if the synchronization is currently running (<see cref="IsRunning"/> is set).
        /// </summary>
        public DateTime? NextRunTime
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates a new state of SharePoint library synchronization.
        /// </summary>
        /// <param name="lastRunTime">Last time the synchronization was run.</param>
        /// <param name="lastResult">Result of the last synchronization (if error condition was met), null otherwise.</param>
        /// <param name="nextRunTime">Next time the synchronization is scheduled for run. Null means the synchronization is currently running.</param>
        public SharePointLibrarySynchronizationState(DateTime? lastRunTime, string lastResult, DateTime? nextRunTime)
        {
            LastRunTime = lastRunTime;
            LastResult = lastResult;
            NextRunTime = nextRunTime;
        }
    }
}

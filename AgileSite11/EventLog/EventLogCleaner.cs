using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Thread for deleting old event logs.
    /// </summary>
    public class EventLogCleaner
    {
        #region "Variables"

        private readonly int mSiteId;
        private readonly int mLogSize;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="logSize">Log size</param>
        public EventLogCleaner(int siteId, int logSize)
        {
            mSiteId = siteId;
            mLogSize = logSize;
        }


        /// <summary>
        /// Delete older event logs.
        /// </summary>
        private void Run()
        {
            try
            {
                EventLogProvider.DeleteOlderItems(mSiteId, mLogSize);
            }
            finally
            {
                EventLogProvider.DeleteOlderThreadRunning = false;
            }
        }


        /// <summary>
        /// Run async thread to delete old logs.
        /// </summary>
        public void RunAsync()
        {
            EventLogProvider.DeleteOlderThreadRunning = true;

            var thr = new CMSThread(Run);

            thr.Start();
        }

        #endregion
    }
}
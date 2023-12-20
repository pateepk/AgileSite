using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Asynchronous thread running in background which logs the events to the event log
    /// </summary>
    internal sealed class EventLogWorker : ThreadQueueWorker<EventLogInfo, EventLogWorker>
    {
        private int mLoggedEvents;
        private HashSet<int> mLoggedSiteIds = new HashSet<int>();

        /// <summary>
        /// Gets the interval in milliseconds for the worker (default 100ms)
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return 100;
            }
        }


        /// <summary>
        /// Gets the maintenance interval in milliseconds (default 60s)
        /// </summary>
        protected override int MaintenanceInterval
        {
            get
            {
                return 60000;
            }
        }


        /// <summary>
        /// Use the log context to report status
        /// </summary>
        protected override bool UseLogContext
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Initializes the worker. Runs in the worker thread before the thread processes the first iteration.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Limit the maximum length of the log
            if (UseLogContext)
            {
                Log.MaxLength = LOG_MAX_LENGTH;
            }
        }


        /// <summary>
        /// Reports the status to the thread log
        /// </summary>
        protected override void DoMaintenance()
        {
            var loggedEvents = mLoggedEvents;
            var log = Log;
            if ((log != null) && (loggedEvents > 0))
            {
                var siteIds = mLoggedSiteIds;

                // Reset internal status
                mLoggedEvents = 0;
                mLoggedSiteIds = new HashSet<int>();

                log.AppendText(String.Format("[{0}]: {1} event(s) logged in last 60 seconds.", DateTime.Now, loggedEvents));

                // Delete older items for sites that were logged
                if (EventLogProvider.DeleteOlderItems(siteIds))
                {
                    log.AppendText(String.Format("[{0}]: Older events were cleared.", DateTime.Now));
                }
            }
        }

        
        /// <summary>
        /// Processes the items in the queue
        /// </summary>
        /// <param name="items">Items to process</param>
        protected override int ProcessItems(IEnumerable<EventLogInfo> items)
        {
            var count = base.ProcessItems(items);

            mLoggedEvents += count;

            return count;
        }


        /// <summary>
        /// Processes the item in the queue
        /// </summary>
        /// <param name="item">Item to process</param>
        protected override void ProcessItem(EventLogInfo item)
        {
            EventLogProvider.LogEventCore(item, true);
            mLoggedSiteIds.Add(item.SiteID);
        }


        /// <summary>
        /// Finishes processing all the items remaining in the worker queue
        /// </summary>
        protected override void Finish()
        {
            // Run the process for the one last time
            RunProcess();
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact actions log worker. Takes information from Contact management queues (activities and contact changes) and processes them. 
    /// </summary>
    internal sealed class ContactActionsLogWorker : ThreadWorker<ContactActionsLogWorker>
    {
        private readonly ContactActionsQueueProcessor mQueueProcessor = new ContactActionsQueueProcessor();
        private int? mInterval;


        /// <summary>
        /// Gets the interval in milliseconds for the worker.
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                if (!mInterval.HasValue)
                {
                    mInterval = SettingsHelper.AppSettings["CMSProcessContactActionsInterval"].ToInteger(10) * 1000;
                }
                return mInterval.Value;
            }
        }


        /// <summary>
        /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
        /// </summary>
        protected override void Finish()
        {
        }


        /// <summary>
        /// Method that is being run every <see cref="DefaultInterval"/> millisecond.
        /// </summary>
        protected override void Process()
        {
            CheckElapsedTime(ProcessActions);
        }


        /// <summary>
        /// Processes items from queue.
        /// </summary>
        private void ProcessActions()
        {
            try
            {
                // At this point the process may took as much time as it needs, because it is running in the separate thread. 
                // The process should not create other child threads, firstly to avoid thread explosion and secondly for correct time measurement. 
                // Other actions should not rely on the process always running in the context of the request (e.g. CMSWorkerQueue is automatically processed at the end of request but this is not happening here). 
                using (new CMSActionContext { AllowAsyncActions = false })
                {
                    mQueueProcessor.ProcessAllContactActions();
                }
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("ContactActionsLogWorker", "Processing contact actions", e, additionalMessage: "Unexpected exception occurred while processing activities.");
            }
        }


        /// <summary>
        /// Logs an event into event log when given actions take too long to complete.
        /// </summary>
        private void CheckElapsedTime(Action processActions)
        {
            var stopwatch = Stopwatch.StartNew();
            processActions();
            stopwatch.Stop();
            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            if (ElapsedTimeExceedsLimit(totalMilliseconds))
            {
                // Log it just once a day, not to spam event log
                LogIntervalTooLowWarning(totalMilliseconds);
            }
        }


        /// <summary>
        /// Logs information about long activity processing into event log.
        /// </summary>
        private void LogIntervalTooLowWarning(double elapsedMilliseconds)
        {
            var elapsedSeconds = Math.Round(elapsedMilliseconds / 1000);
            var intervalSeconds = DefaultInterval / 1000;
            EventLogProvider.LogEvent(
                EventType.WARNING,
                "ContactActionsLogWorker",
                "ACTIVITYPROCESSINTERVAL",
                string.Format("Processing contact activities and changes took {0} seconds, but the interval is set to {1} seconds. Consider raising the activity processing interval in the CMSProcessContactActionsInterval web.config key.", elapsedSeconds, intervalSeconds),
                loggingPolicy: new LoggingPolicy(TimeSpan.FromDays(1)));
        }


        /// <summary>
        /// Checks whether given time exceeds task interval hardcoded limit.
        /// </summary>
        private bool ElapsedTimeExceedsLimit(double elapsedMilliseconds)
        {
            return elapsedMilliseconds > (DefaultInterval * 3);
        }      
    }
}

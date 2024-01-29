using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Context for the process of event logging.
    /// </summary>
    [RegisterAllProperties]
    internal sealed class EventLoggingContext : AbstractActionContext<EventLoggingContext>
    {
        private bool? mEventLoggingInProgress;
        private EventLogWorker mLogWorker;


        /// <summary>
        /// Indicates whether event logging procedure code is being executed.
        /// </summary>
        public bool EventLoggingInProgress
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mEventLoggingInProgress, CurrentEventLoggingInProgress);
                CurrentEventLoggingInProgress = value;
            }
        }


        /// <summary>
        /// Event log queue worker instance.
        /// </summary>
        internal EventLogWorker LogWorker
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mLogWorker, mLogWorker);
                mLogWorker = value;
            }
        }


        /// <summary>
        /// Indicates whether event logging procedure code is being executed.
        /// </summary>
        public static bool CurrentEventLoggingInProgress
        {
            get
            {
                return Current.mEventLoggingInProgress ?? false;
            }
            private set
            {
                Current.mEventLoggingInProgress = value;
            }
        }


        /// <summary>
        /// Current event log queue worker instance.
        /// </summary>
        public static EventLogWorker CurrentLogWorker
        {
            get
            {
                return Current.mLogWorker ?? EventLogWorker.Current;
            }
        }


        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore current data context
            var o = OriginalData;
            if (o.mEventLoggingInProgress.HasValue)
            {
                EventLoggingInProgress = o.mEventLoggingInProgress.Value;
            }

            // Restore to previous value even if it is null
            mLogWorker = o.mLogWorker;

            base.RestoreOriginalValues();
        }
    }
}

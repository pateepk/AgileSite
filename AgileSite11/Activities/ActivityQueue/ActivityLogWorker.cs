using System;

using CMS.Base;
using CMS.Core;

namespace CMS.Activities
{
    /// <summary>
    /// Activities log worker. Takes information from <see cref="IActivityQueue"/> and processes it. 
    /// </summary>
    internal sealed class ActivityLogWorker : ThreadWorker<ActivityLogWorker>
    {
        private readonly IActivityQueueProcessor mActivityQueueProcessor;
        private readonly IEventLogService mEventLogService;
        private int? mInterval;
        
        /// <summary>
        /// Events fired by the worker once process method executes.
        /// </summary>
        internal event EventHandler Processed;
        

        /// <summary>
        /// Instantiates new instance of <see cref="ActivityLogWorker"/>.
        /// </summary>
        public ActivityLogWorker()
        {
            mActivityQueueProcessor = Service.Resolve<IActivityQueueProcessor>();
            mEventLogService = Service.Resolve<IEventLogService>();
        }


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
        /// Finishes the worker process. No extra action has to be taken.
        /// </summary>
        protected override void Finish()
        {
        }


        /// <summary>
        /// Method that is being run every <see cref="DefaultInterval"/> millisecond. Processes items from <see cref="ActivityMemoryQueue"/>.
        /// </summary>
        protected override void Process()
        {
            try
            {
                // The process should not create other child threads, firstly to avoid thread explosion and secondly for correct time measurement. 
                // Other actions should not rely on the process always running in the context of the request (e.g. CMSWorkerQueue is automatically processed at the end of request but this is not happening here). 
                using (new CMSActionContext { AllowAsyncActions = false })
                {
                    mActivityQueueProcessor.InsertActivitiesFromQueueToDB();
                }
            }
            catch (Exception e)
            {
                mEventLogService.LogException("ActivityLogWorker", "ACTIVITIESPROCESSING", e);
            }
            finally
            {
                if (Processed != null)
                {
                    Processed(this, new EventArgs());
                }
            }
        }
    }
}
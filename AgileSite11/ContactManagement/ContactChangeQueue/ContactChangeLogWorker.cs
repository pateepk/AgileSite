using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Logs <see cref="ContactChangeData"/> into the memory queue and periodically bulk inserts it into the DB.
    /// </summary>
    internal sealed class ContactChangeLogWorker : ThreadQueueWorker<ContactChangeData, ContactChangeLogWorker>
    {
        private int? mInterval;
        private readonly IEventLogService mEventLogService;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactChangeLogWorker"/>.
        /// </summary>
        public ContactChangeLogWorker()
        {
            mEventLogService = Service.Resolve<IEventLogService>();
        }


        /// <summary>
        /// Runs the worker process immediately bypassing the <see cref="DefaultInterval"/>. This method should be used only in special cases such as test methods.
        /// </summary>
        internal void ProcessItemsImmediately()
        {
            RunProcess();
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
        /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
        /// </summary>
        protected override void Finish()
        {
            // Run the process for the one last time
            RunProcess();
        }


        /// <summary>
        /// Adds UTM parameters to processing queue.
        /// </summary>
        /// <param name="contactChangeData">Contact change data to be logged</param>
        public static void LogContactChange(ContactChangeData contactChangeData)
        {
            Current.Enqueue(contactChangeData, false);
        }



        /// <summary>
        /// Processing of single item. In this implementation this method is not called at all.
        /// </summary>
        /// <param name="item">Item to be processed.</param>
        protected override void ProcessItem(ContactChangeData item)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Processing of all items.
        /// </summary>
        protected override int ProcessItems(IEnumerable<ContactChangeData> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                return 0;
            }

            try
            {
                ContactChangeRecalculationQueueInfoProvider.BulkInsert(itemList);
            }
            catch (Exception e)
            {
                mEventLogService.LogException("ContactChangeLogWorker", "CONTACTCHANGESPROCESSING", e);
            }

            return itemList.Count;
        }
    }
}

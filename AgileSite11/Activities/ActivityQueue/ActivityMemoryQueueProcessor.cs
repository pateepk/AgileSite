using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine.Internal;

namespace CMS.Activities
{
    /// <summary>
    /// Provides method for processing the activity memory queue.
    /// </summary>
    internal sealed class ActivityMemoryQueueProcessor : IActivityQueueProcessor
    {
        private readonly IActivityQueue mActivitiesQueue;
        
        /// <summary>
        /// Instantiates new instance of <see cref="ActivityMemoryQueueProcessor" />.
        /// </summary>
        /// <param name="activitiesQueue">Provides methods for storing of performed activities to the queue</param>
        /// <exception cref="ArgumentNullException"><paramref name="activitiesQueue" /> is <c>null</c></exception>
        public ActivityMemoryQueueProcessor(IActivityQueue activitiesQueue)
        {
            if (activitiesQueue == null)
            {
                throw new ArgumentNullException("activitiesQueue");
            }

            mActivitiesQueue = activitiesQueue;
        }


        /// <summary>
        /// Performs bulk insert of <see cref="IActivityInfo" /> from <see cref="ActivityMemoryQueue" />. 
        /// Ensures that <see cref="IActivityInfo.ActivityID" /> is correctly filled for every <see cref="IActivityInfo" /> after the bulk insert,
        /// so <see cref="ActivityEvents.ActivityBulkInsertPerformed" /> is fired with correct data.
        /// </summary>
        /// <remarks>
        /// <see cref="ActivityEvents.ActivityBulkInsertPerformed" /> is fired once the bulk is inserted.
        /// </remarks>
        public void InsertActivitiesFromQueueToDB()
        {
            var activities = mActivitiesQueue.GetQueuedActivities().ToList();
            if (!activities.Any())
            {
                return;
            }

            using (var h = ActivityEvents.ActivityBulkInsertPerformed.StartEvent(activities))
            {
                if (h.CanContinue())
                {
                    var lastActivityID = ActivityInfoProvider.BulkInsertAndGetLastID(activities.Cast<IDataTransferObject>());
                    FillActivityIDs(activities, lastActivityID);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Fills the <see cref="IActivityInfo.ActivityID" /> property to every <see cref="IActivityInfo" /> in given
        /// <paramref name="activities" /> collection.
        /// </summary>
        /// <remarks>
        /// <see cref="IActivityInfo.ActivityID" /> is obtained from the DB for the first <see cref="IActivityInfo" /> from
        /// <paramref name="activities" />, other
        /// <see cref="IActivityInfo.ActivityID" /> values are computed by repeated addition of 1. This can be done since bulk
        /// insert guarantees atomicity and order of the inserted items.
        /// </remarks>
        /// <param name="activities">Collection of activities to be filled with <see cref="IActivityInfo.ActivityID" /></param>
        /// <param name="lastActivityID">ID of the last inserted activity</param>
        private void FillActivityIDs(IList<IActivityInfo> activities, int lastActivityID)
        {
            if (lastActivityID == 0)
            {
                return;
            }

            for (var i = 0; i < activities.Count; i++)
            {
                activities[activities.Count - 1 - i].ActivityID = lastActivityID - i;
            }
        }
    }
}
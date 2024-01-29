using System;

using CMS.Activities.Internal;

namespace CMS.Activities
{
    /// <summary>
    /// Represents activity repository for storing the <see cref="IActivityInfo"/> in queue.
    /// </summary>
    internal class ActivityQueueRepository : IActivityRepository
    {
        private readonly IActivityQueue mActivitiesQueue;


        /// <summary>
        /// Instantiates new instance of <see cref="ActivityQueueRepository"/>.
        /// </summary>
        /// <param name="activitiesQueue">Provides methods for storing of performed activities to the queue.</param>
        /// <exception cref="ArgumentNullException"><paramref name="activitiesQueue"/> is <c>null</c></exception>
        public ActivityQueueRepository(IActivityQueue activitiesQueue)
        {
            if (activitiesQueue == null)
            {
                throw new ArgumentNullException("activitiesQueue");
            }

            mActivitiesQueue = activitiesQueue;
        }


        /// <summary>
        /// Saves given <paramref name="activity"/> into the queue.
        /// </summary>
        /// <param name="activity">Activity to be saved</param>
        public void Save(IActivityInfo activity)
        {
            mActivitiesQueue.EnqueueActivity(activity);
        }
    }
}
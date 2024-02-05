using System.Collections.Generic;

using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivityQueue), typeof(ActivityMemoryQueue), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides methods for storing of performed activities to the queue.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public interface IActivityQueue
    {
        /// <summary>
        /// Tries to remove and return <see cref="IActivityInfo"/> at the beginning of the concurrent queue.
        /// </summary>
        /// <param name="activity">
        /// When this method returns, if the operation was successful, <paramref name="activity"/> contains the object removed. If no object was available to be removed, the <paramref name="activity"/> is unspecified.
        /// </param>
        /// <returns>
        /// True if <see cref="IActivityInfo"/> was removed and returned from the beginning of the <see cref="ActivityMemoryQueue"/> successfully; otherwise, false.
        /// </returns>
        bool TryDequeueActivity(out IActivityInfo activity);


        /// <summary>
        /// Adds <see cref="IActivityInfo"/> to the end of the <see cref="ActivityMemoryQueue"/>.
        /// </summary>
        /// <param name="activity">The <see cref="IActivityInfo"/> to be added to the end of the <see cref="ActivityMemoryQueue"/>.</param>
        void EnqueueActivity(IActivityInfo activity);


        /// <summary>
        /// Enumerates all items in the queue of activities.
        /// </summary>
        /// <returns>Collection containing all the <see cref="IActivityInfo"/> from the queue</returns>
        IEnumerable<IActivityInfo> GetQueuedActivities();
    }
}
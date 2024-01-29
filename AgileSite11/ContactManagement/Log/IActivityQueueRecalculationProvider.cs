using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.Activities;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IActivityQueueRecalculationProvider), typeof(SqlActivityQueueRecalculationProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Provides methods for storing activity to queue.
    /// </summary>
    public interface IActivityQueueRecalculationProvider
    {
        /// <summary>
        /// Stores activity for further processing.
        /// </summary>
        /// <param name="activity"><see cref="IActivityInfo"/> of stored activity in database</param>
        /// <remarks>
        /// In default implementation, SQL database will be used for storing.
        /// </remarks>
        void Store(IActivityInfo activity);


        /// <summary>
        /// Stores all the activities from given <paramref name="activities"/> for further processing.
        /// </summary>
        /// <param name="activities">Collection of <see cref="IActivityInfo"/> representing stored activity in database</param>
        /// <remarks>
        /// In default implementation, SQL database will be used for storing.
        /// </remarks>
        void StoreRange(IEnumerable<IActivityInfo> activities);


        /// <summary>
        /// Returns top 10000 items from the queue and at the same time it removes them from the queue.
        /// </summary> 
        IList<IActivityInfo> Dequeue();
    }
}

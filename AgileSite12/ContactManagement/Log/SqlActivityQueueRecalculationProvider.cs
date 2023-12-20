using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineMarketing.Internal;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Provides methods for storing activity to SQL database.
    /// </summary>
    internal class SqlActivityQueueRecalculationProvider : IActivityQueueRecalculationProvider
    {
        /// <summary>
        /// Stores flag (in SQL db) that activity should be further processed.
        /// </summary>
        /// <param name="activity"><see cref="IActivityInfo"/> of stored activity in database</param>
        public void Store(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            var activityQueueInfo = new ActivityRecalculationQueueInfo
            {
                ActivityRecalculationQueueActivityID = activity.ActivityID
            };
            
            ActivityRecalculationQueueInfoProvider.SetActivityRecalculationQueueInfo(activityQueueInfo);
        }


        /// <summary>
        /// Stores flag (in SQL db) that all the activities given in <paramref name="activities"/> should be further processed.
        /// </summary>
        /// <param name="activities">Collection of <see cref="IActivityInfo"/> representing stored activity in database</param>
        /// <exception cref="ArgumentNullException"><paramref name="activities"/> is <c>null</c></exception>
        public void StoreRange(IEnumerable<IActivityInfo> activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }

            var activityQueueInfo = activities.Select(activity => new ActivityRecalculationQueueDto
            {
                ActivityRecalculationQueueActivityID = activity.ActivityID
            });
            
            ActivityRecalculationQueueInfoProvider.BulkInsert(activityQueueInfo);
        }


        /// <summary>
        /// Returns top 10000 items from the queue and at the same time it removes them from the queue.
        /// </summary>
        public IList<IActivityInfo> Dequeue()
        {
            var activitiesDataSet = ConnectionHelper.ExecuteQuery("OM.ActivityRecalculationQueue.FetchActivityIDs", new QueryDataParameters());
            var result = new List<IActivityInfo>();

            if (DataHelper.DataSourceIsEmpty(activitiesDataSet))
            {
                return result;
            }

            foreach (DataRow dataRow in activitiesDataSet.Tables[0].Rows)
            {
                result.Add(new ActivityDto(dataRow));
            }

            return result;
        }
    }
}

using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Core.Internal;

namespace CMS.Activities.Internal
{
    /// <summary>
    /// Extends <see cref="ObjectQuery{ActivityInfo}"/>.
    /// </summary>
    public static class ActivityInfoObjectQueryExtensions
    {
        /// <summary>
        /// Returns <see cref="ObjectQuery"/> with Activities that are older than given <paramref name="time"/>.
        /// </summary>
        public static ObjectQuery<ActivityInfo> NewerThan(this ObjectQuery<ActivityInfo> query, TimeSpan time)
        {
            var dateTimeNow = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            var minCreated = dateTimeNow.Add(-time);
            return query.Where("ActivityCreated", QueryOperator.LargerOrEquals, minCreated);
        }
    }
}
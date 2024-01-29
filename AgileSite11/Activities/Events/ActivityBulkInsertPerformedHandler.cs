using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Activities
{
    /// <summary>
    /// Event handler for the event fired once the activities are inserted in bulk to the database.
    /// </summary>
    public class ActivityBulkInsertPerformedHandler : AdvancedHandler<ActivityBulkInsertPerformedHandler, CMSEventArgs<IList<IActivityInfo>>>
    {
        /// <summary>
        /// Initiates the event.
        /// </summary>
        /// <param name="activities">Collection of processed <see cref="IActivityInfo"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="activities"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">First <see cref="IActivityInfo"/> has to have its <see cref="IActivityInfo.ActivityID"/> property set</exception>
        /// <returns>Event handler</returns>
        public ActivityBulkInsertPerformedHandler StartEvent(IList<IActivityInfo> activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }

            var e = new CMSEventArgs<IList<IActivityInfo>>
            {
                Parameter = activities
            };

            return StartEvent(e);
        }
    }
}
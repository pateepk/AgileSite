using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Activities
{
    /// <summary>
    /// Event handler for the event fired once the activities are filtered out before bulk processing.
    /// </summary>
    public class ActivityFilteredOutHandler : SimpleHandler<ActivityFilteredOutHandler, CMSEventArgs<IList<IActivityInfo>>>
    {
        /// <summary>
        /// Initiates the event.
        /// </summary>
        /// <param name="activities">Collection of filtered out<see cref="IActivityInfo"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="activities"/> is <c>null</c></exception>
        /// <returns>Event handler</returns>
        public CMSEventArgs<IList<IActivityInfo>> StartEvent(IList<IActivityInfo> activities)
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
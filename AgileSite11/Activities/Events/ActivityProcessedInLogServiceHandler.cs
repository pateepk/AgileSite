using System;

using CMS.Base;

namespace CMS.Activities
{
    /// <summary>
    /// Event handler for the event fired once the activity is processed in <see cref="IActivityLogService"/>.
    /// </summary>
    public class ActivityProcessedInLogServiceHandler : SimpleHandler<ActivityProcessedInLogServiceHandler, CMSEventArgs<IActivityInfo>>
    {
        /// <summary>
        /// Initiates the event.
        /// </summary>
        /// <param name="activity">Processed <see cref="IActivityInfo"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c></exception>
        /// <returns>Event handler</returns>
        public CMSEventArgs<IActivityInfo> StartEvent(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            var e = new CMSEventArgs<IActivityInfo>
            {
                Parameter = activity
            };

            return StartEvent(e);
        }
    }
}
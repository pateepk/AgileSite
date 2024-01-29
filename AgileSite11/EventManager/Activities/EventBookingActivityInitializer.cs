using System;

using CMS.Activities;
using CMS.DocumentEngine;

namespace CMS.EventManager
{
    /// <summary>
    /// Represents implementation of <see cref="IActivityInitializer"/> for event booking activity.
    /// </summary>
    public class EventBookingActivityInitializer : IActivityInitializer
    {
        private readonly TreeNode mEventNode;
        private readonly int mAttendeeId;
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Instantiate new instance of <see cref="EventBookingActivityInitializer"/>.
        /// </summary>
        /// <param name="attendeeId">Specifies id of attendee that booked to event</param>
        /// <param name="eventNode">Specifies event node</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventNode"/> is <c>null</c>.</exception>
        public EventBookingActivityInitializer(int attendeeId, TreeNode eventNode)
        {
            if (eventNode == null)
            {
                throw new ArgumentNullException("eventNode");
            }

            mAttendeeId = attendeeId;
            mEventNode = eventNode;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties for event booking activity.
        /// </summary>
        /// <param name="activity">Activity info</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is <c>null</c>.</exception>
        public void Initialize(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            activity.ActivityItemID = mAttendeeId;
            activity.ActivityItemDetailID = mEventNode.DocumentID;
            activity.ActivityTitle = mActivityTitleBuilder.CreateTitle(ActivityType, mEventNode.GetDocumentName());
            activity.ActivityNodeID = mEventNode.NodeID;
            activity.ActivityCulture = mEventNode.DocumentCulture;
        }


        /// <summary>
        /// Gets string type of the activity.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.EVENT_BOOKING;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMEventBooking";
            }
        }
    }
}
using System.ComponentModel;

namespace CMS.Activities.Internal
{
    /// <summary>
    /// Activity events for internal purposes.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ActivityEventsInternal
    {
        /// <summary>
        /// Event fired once the activities are filtered out before bulk processing.
        /// </summary>
        public static ActivityFilteredOutHandler ActivityFilteredOut = new ActivityFilteredOutHandler
        {
            Name = "ActivityEvents.ActivityFilteredOut"
        };


        /// <summary>
        /// Event fired once the activities are bulk inserted to the database and their <see cref="ActivityInfo.ActivityID"/> is assigned.
        /// </summary>
        public static ActivityBulkInsertPerformedHandler ActivityIDMapped = new ActivityBulkInsertPerformedHandler
        {
            Name = "ActivityEvents.ActivityIDMapped"
        };
    }
}

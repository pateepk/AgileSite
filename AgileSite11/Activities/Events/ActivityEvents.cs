namespace CMS.Activities
{
    /// <summary>
    /// Activity events.
    /// </summary>
    public static class ActivityEvents
    {
        /// <summary>
        /// Event fired once all the activities are inserted in bulk to the database.
        /// </summary>
        public static ActivityBulkInsertPerformedHandler ActivityBulkInsertPerformed = new ActivityBulkInsertPerformedHandler
        {
            Name = "ActivityEvents.ActivityBulkInsertPerformed"
        };

        
        /// <summary>
        /// Event handler for the event fired once the activity is processed in log service.
        /// </summary>
        public static ActivityProcessedInLogServiceHandler ActivityProcessedInLogService = new ActivityProcessedInLogServiceHandler
        {
            Name = "ActivityEvents.ActivityProcessedInLogService"
        };
    }
}
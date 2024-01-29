namespace CMS.Synchronization
{
    /// <summary>
    /// Enumeration indicationg processing result. 
    /// Task the value says whether the 3rd party system processed the task well or not.
    /// See comments for each value.
    /// </summary>
    public enum IntegrationProcessResultEnum
    {
        /// <summary>
        /// OUTBOUND TASKS  - Default state of the task. Says that task was processed without any error and should be deleted (or the relation should be deleted).
        /// </summary>
        OK = 0,

        /// <summary>
        /// OUTBOUND TASKS  - The result indicating that the processing of the task failed and should not continue.
        ///                 - SYNC TASKS - Synchronization does not continue for other connectors.
        /// </summary>
        Error = 1,

        /// <summary>
        /// OUTBOUND TASKS  - The result indicating that the processing of the task ended with error but should continue.
        ///                 - SYNC TASKS - Synchronization does continue for other connectors.
        /// </summary>
        ErrorAndSkip = 2,

        /// <summary>
        /// OUTBOUND TASKS  - The result indicating that the task was not processed and should be processed later. The task should remain in database with its current state.
        /// </summary>
        SkipNow = 3
    }
}
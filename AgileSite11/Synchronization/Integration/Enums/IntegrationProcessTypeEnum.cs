namespace CMS.Synchronization
{
    /// <summary>
    /// Says how the CMS should process the task.
    /// See comments for each value.
    /// </summary>
    public enum IntegrationProcessTypeEnum
    {
        /// <summary>
        /// Processes the task immediately. If error occurs, the processing stops and sets the type to Error.
        /// </summary>
        Default = 0,


        /// <summary>
        /// Does not process the task during first processing (just sets the type to Default so it is going to be processed during next processing).
        /// </summary>
        SkipOnce = 1,


        /// <summary>
        /// Processes the task immediately. If error occurs, it skips the task and continues with processing.
        /// </summary>
        SkipOnError = 2,


        /// <summary>
        /// Processes the task immediately. If error occurs, deletes the task and continues with processing.
        /// </summary>
        DeleteOnError = 3,


        /// <summary>
        /// Says that processing should not continue due to critical error.
        /// </summary>
        Error = 4
    }
}
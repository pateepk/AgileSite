namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Process recurrence type enumeration.
    /// </summary>
    public enum ProcessRecurrenceTypeEnum
    {
        /// <summary>
        /// Recurrence type not specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// The process can be started on the contact anytime.
        /// </summary>
        Recurring = 1,

        /// <summary>
        /// The process can only be started if it hasn't been run on the contact before.
        /// </summary>
        NonRecurring = 2,

        /// <summary>
        /// The process can only be started if the same process is currently not running on the contact. It is either already finished or there is no record of the run.
        /// </summary>
        NonConcurrentRecurring = 3
    }
}

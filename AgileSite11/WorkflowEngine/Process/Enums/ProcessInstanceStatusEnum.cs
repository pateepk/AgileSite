namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Process instance status enumeration.
    /// </summary>
    public enum ProcessInstanceStatusEnum
    {
        /// <summary>
        /// There is no process instance
        /// </summary>
        None = 0,

        /// <summary>
        /// There is at least one running instance
        /// </summary>
        Running = 1,

        /// <summary>
        /// All process instances are finished
        /// </summary>
        Finished = 2,
    }
}
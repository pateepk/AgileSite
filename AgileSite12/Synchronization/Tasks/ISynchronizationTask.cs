namespace CMS.Synchronization
{
    /// <summary>
    /// Represents generic synchronization task.
    /// </summary>
    public interface ISynchronizationTask
    {
        /// <summary>
        /// Task identifier.
        /// </summary>
        int TaskID
        {
            get;
            set;
        }

        /// <summary>
        /// Caption of the task.
        /// </summary>
        string TaskTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Data of the task. (Usually XML)
        /// </summary>
        string TaskData
        {
            get;
            set;
        }
    }
}
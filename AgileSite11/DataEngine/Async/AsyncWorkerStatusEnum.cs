namespace CMS.DataEngine
{
    /// <summary>
    /// Async worker status enumeration.
    /// </summary>
    public enum AsyncWorkerStatusEnum
    {
        /// <summary>
        /// Process stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Process running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Process finished.
        /// </summary>
        Finished = 2,

        /// <summary>
        /// Error occurred.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Process waits for being finished.
        /// </summary>
        WaitForFinish = 4,

        /// <summary>
        /// Unknown status
        /// </summary>
        Unknown = 5
    }
}
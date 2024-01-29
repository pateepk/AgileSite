namespace CMS.Base
{
    /// <summary>
    /// Logs status mode enumeration.
    /// </summary>
    public enum LogStatusEnum
    {
        /// <summary>
        /// Information during process.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Finished with error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Warning during the process.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Successfully finished.
        /// </summary>
        Finish = 3,

        /// <summary>
        /// Unexpectedly finished.
        /// </summary>
        UnexpectedFinish = 4,

        /// <summary>
        /// Process starting.
        /// </summary>
        Start = 5
    }
}
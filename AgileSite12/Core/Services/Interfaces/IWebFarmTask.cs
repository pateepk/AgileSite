namespace CMS.Core
{
    /// <summary>
    /// Web farm task representation.
    /// </summary>
    public interface IWebFarmTask
    {
        /// <summary>
        /// Binary data.
        /// </summary>
        byte[] TaskBinaryData
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the task is not assigned to any server.
        /// </summary>
        bool TaskIsAnonymous
        {
            get;
            set;
        }


        /// <summary>
        /// Task type.
        /// </summary>
        string TaskType
        {
            get;
            set;
        }


        /// <summary>
        /// Task target.
        /// </summary>
        string TaskTarget
        {
            get;
            set;
        }


        /// <summary>
        /// Task type.
        /// </summary>
        string TaskTextData
        {
            get;
            set;
        }


        /// <summary>
        /// Target path of file transported by task.
        /// </summary>
        string TaskFilePath
        {
            get;
            set;
        }
    }
}

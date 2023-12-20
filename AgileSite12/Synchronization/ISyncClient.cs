namespace CMS.Synchronization
{
    /// <summary>
    /// Interface for SyncClient
    /// </summary>
    public interface ISyncClient
    {
        #region "Properties"

        /// <summary>
        /// Synchronization server.
        /// </summary>
        ServerInfo Server
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the synchronization task.
        /// </summary>
        /// <param name="taskObj">Task object</param>
        /// <returns>Returns error message</returns>
        string RunTask(StagingTaskInfo taskObj);

        #endregion
    }
}

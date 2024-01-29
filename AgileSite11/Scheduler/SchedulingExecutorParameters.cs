namespace CMS.Scheduler
{
    /// <summary>
    /// Wrapper for SchedulingExecutor.ExecuteScheduledTasks method. 
    /// Used to create CMSThread with proper calling context.
    /// </summary>
    public class SchedulingExecutorParameters
    {

        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Server name
        /// </summary>
        public string ServerName
        {
            get;
            set;
        }


        /// <summary>
        /// Executes scheduled task
        /// </summary>
        public void ExecuteScheduledTasks()
        {
            SchedulingExecutor.ExecuteScheduledTasks(SiteName, ServerName);
        }
    }
}

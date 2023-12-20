using CMS.Scheduler;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Provides methods for managing Azure search task processing.
    /// </summary>
    public static class SearchTaskExecutorUtils
    {
        // Name of the scheduled task for processing Azure search tasks.
        private const string SEARCH_TASKS_PROCESSING_SCHEDULED_TASK_NAME = "Search.Azure.TaskExecutor";


        /// <summary>
        /// Starts <see cref="SearchTaskAzureInfo"/>s processing. 
        /// If processing is already running then nothing happens.
        /// </summary>
        public static void ProcessSearchTasks()
        {
            SchedulingExecutor.ExecuteTask(TaskInfoProvider.GetTaskInfo(SEARCH_TASKS_PROCESSING_SCHEDULED_TASK_NAME, 0));
        }


        /// <summary>
        /// Indicates whether Azure search task processing is running already.
        /// </summary>
        /// <returns>True if <see cref="SearchTaskAzureInfo"/>s processing is running. False otherwise.</returns>
        /// <seealso cref="ProcessSearchTasks"/>
        public static bool IsSearchTaskProcessingRunning()
        {
            return TaskInfoProvider.GetTaskInfo(SEARCH_TASKS_PROCESSING_SCHEDULED_TASK_NAME, 0).TaskIsRunning;
        }
    }
}

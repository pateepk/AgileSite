using System;
using System.Security.Principal;

using CMS.Base;
using CMS.Search;

namespace CMS.Scheduler
{
    /// <summary>
    /// Class used by scheduler to execute the task.
    /// </summary>  
    public class SearchTaskExecutor : ITask
    {
        /// <summary>
        /// Processes search tasks (starts indexer).
        /// </summary>
        /// <param name="task">Task to start</param>
        public string Execute(TaskInfo task)
        {
            // Task processing should not start if running on Azure and tasks are not set to be processed only by scheduler
            if (!SearchTaskInfoProvider.ProcessSearchTasksByScheduler && SystemContext.IsRunningOnAzure)
            {
                return String.Empty;
            }

            // Run task indexer
            try
            {
                SearchTaskInfoProvider.RunAsync(WindowsIdentity.GetCurrent());
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
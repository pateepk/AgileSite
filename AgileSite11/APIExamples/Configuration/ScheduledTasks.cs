using System;

using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds scheduled task API examples.
    /// </summary>
    /// <pageTitle>Scheduled tasks</pageTitle>
    internal class ScheduledTasks
    {
        /// <heading>Creating a scheduled task</heading>
        private void CreateScheduledTask()
        {
            // Creates a new scheduled task object
            TaskInfo newTask = new TaskInfo();

            // Sets the basic task properties
            newTask.TaskDisplayName = "New task";
            newTask.TaskName = "NewTask";
            newTask.TaskAssemblyName = "CMS.WorkflowEngine";
            newTask.TaskClass = "CMS.WorkflowEngine.ContentPublisher";
            newTask.TaskSiteID = SiteContext.CurrentSiteID;
            newTask.TaskEnabled = true;

            // Creates the scheduling interval for the task
            TaskInterval interval = new TaskInterval();

            // Sets the interval properties
            interval.Period = SchedulingHelper.PERIOD_DAY;
            interval.StartTime = DateTime.Now;
            interval.Every = 2;
            interval.Days = new System.Collections.Generic.List<DayOfWeek>()
            {
                DayOfWeek.Monday,
                DayOfWeek.Sunday,
                DayOfWeek.Thursday
            };

            // Assigns the interval settings to the task
            newTask.TaskInterval = SchedulingHelper.EncodeInterval(interval);

            // Calculates the first run time for the new task
            newTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

            // Saves the scheduled task to the database
            TaskInfoProvider.SetTaskInfo(newTask);
        }


        /// <heading>Updating a scheduled task</heading>
        private void GetAndUpdateScheduledTask()
        {
            // Gets the scheduled task
            TaskInfo updateTask = TaskInfoProvider.GetTaskInfo("NewTask", SiteContext.CurrentSiteID);
            if (updateTask != null)
            {
                // Updates the task properties
                updateTask.TaskDisplayName = updateTask.TaskDisplayName.ToLowerCSafe();

                // Saves the updated task to the database
                TaskInfoProvider.SetTaskInfo(updateTask);
            }
        }


        /// <heading>Updating multiple scheduled tasks</heading>
        private void GetAndBulkUpdateScheduledTasks()
        {
            // Gets all enabled scheduled tasks whose code name starts with 'New'
            var tasks = TaskInfoProvider.GetTasks()
                                            .WhereStartsWith("TaskName", "New")
                                            .WhereEquals("TaskEnabled", 1);

            // Loops through individual tasks
            foreach (TaskInfo task in tasks)
            {
                // Updates the task properties
                task.TaskDisplayName = task.TaskDisplayName.ToUpper();

                // Saves the updated task to the database
                TaskInfoProvider.SetTaskInfo(task);
            }
        }


        /// <heading>Executing a scheduled task</heading>
        private void RunTask()
        {
            // Gets the scheduled task
            TaskInfo runTask = TaskInfoProvider.GetTaskInfo("NewTask", SiteContext.CurrentSiteID);

            if (runTask != null)
            {
                // Executes (runs) the task, regardless of its interval settings
                SchedulingExecutor.ExecuteTask(runTask);
            }
        }


        /// <heading>Deleting a scheduled task</heading>
        private void DeleteScheduledTask()
        {
            // Gets the scheduled task
            TaskInfo deleteTask = TaskInfoProvider.GetTaskInfo("NewTask", SiteContext.CurrentSiteID);

            if (deleteTask != null)
            {
                // Deletes the scheduled task
                TaskInfoProvider.DeleteTaskInfo(deleteTask);
            }
        }
    }
}

using System;
using System.Data;

using CMS.DataEngine;
using CMS.HealthMonitoring;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Performance counters for scheduler
    /// </summary>
    internal class SchedulerCounters
    {
        /// <summary>
        /// Registers the performance counters
        /// </summary>
        public static void RegisterPerformanceCounters()
        {
            HealthMonitoringLogHelper.RegisterCounter(CounterName.TASKS_IN_QUEUE, UpdateTasksInQueue);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.RUNNING_TASKS, SchedulingExecutor.RunningTasks);
        }


        /// <summary>
        /// Updates performance counter that contains value of tasks in queue.
        /// </summary>
        private static void UpdateTasksInQueue(Counter counter)
        {
            // Get tasks in the queue
            DataSet taskCount = TaskInfoProvider.GetTasks()
                .WhereLessThan("TaskNextRunTime", DateTime.Now)
                .WhereTrue("TaskEnabled")
                .Column(new CountColumn().As("TaskCount"));

            if (!DataHelper.DataSourceIsEmpty(taskCount))
            {
                long count = ValidationHelper.GetLong(taskCount.Tables[0].Rows[0]["TaskCount"], 0);
                counter.PerformanceCounter.SetValue(count, null);
            }
            else
            {
                counter.PerformanceCounter.Reset(true);
            }
        }
    }
}

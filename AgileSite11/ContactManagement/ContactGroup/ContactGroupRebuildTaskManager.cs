using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Manages scheduled tasks responsible for rebuilding contact groups.
    /// </summary>
    public static class ContactGroupRebuildTaskManager
    {
        private static readonly string TASK_ASSEMBLY_NAME = typeof(ContactGroupEvaluator).Assembly.GetName().Name;
        private static readonly string TASK_CLASS = typeof(ContactGroupEvaluator).FullName;


        /// <summary>
        /// Enables scheduled task connected to the given contact group.
        /// If such task does not exits, creates a new one.
        /// </summary>
        /// <param name="contactGroup">Contact group containing the scheduled task</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactGroup"/> is null</exception>
        public static void EnableScheduledTask(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            var task = GetScheduledTask(contactGroup) ?? CreateScheduledTask(contactGroup);

            task.TaskEnabled =  true;
            task.Generalized.LogSynchronization = SynchronizationTypeEnum.LogSynchronization;
            TaskInfoProvider.SetTaskInfo(task);
        }


        /// <summary>
        /// Disables scheduled task connected to the given contact group.
        /// Does nothing when scheduled task does not exist.
        /// </summary>
        /// <param name="contactGroup">Contact group containing the scheduled task</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactGroup"/> is null</exception>
        public static void DisableScheduledTask(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            var task = GetScheduledTask(contactGroup);
            if (task == null)
            {
                return;
            }

            task.TaskEnabled = false;
            task.Generalized.LogSynchronization = SynchronizationTypeEnum.LogSynchronization;
            TaskInfoProvider.SetTaskInfo(task);
        }


        /// <summary>
        /// Gets the scheduled task responsible for rebuilding the given contact group.
        /// </summary>
        /// <param name="contactGroup">Contact group</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactGroup"/> is null</exception>
        public static TaskInfo GetScheduledTask(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            return TaskInfoProvider.GetTasks()
                                   .WhereEquals("TaskData", contactGroup.ContactGroupGUID.ToString())
                                   .WhereEquals("TaskClass", TASK_CLASS)
                                   .FirstOrDefault();
        }


        /// <summary>
        /// Creates new scheduled task with basic properties set.
        /// </summary>
        /// <remarks>
        /// When creating new scheduled task, execution time is set to off peak time.
        /// </remarks>
        /// <param name="contactGroup">Contact group info</param>
        private static TaskInfo CreateScheduledTask(ContactGroupInfo contactGroup)
        {
            var defaultTaskInterval = GenerateDefaultTaskInterval();

            return new TaskInfo
            {
                TaskAssemblyName = TASK_ASSEMBLY_NAME,
                TaskClass = TASK_CLASS,
                TaskEnabled = true,
                TaskLastResult = string.Empty,
                TaskData = contactGroup.ContactGroupGUID.ToString(),
                TaskDisplayName = "Contact group '" + contactGroup.ContactGroupDisplayName + "' rebuild",
                TaskName = "ContactGroupRebuild_" + contactGroup.ContactGroupName,
                TaskType = ScheduledTaskTypeEnum.System,
                TaskInterval = SchedulingHelper.EncodeInterval(defaultTaskInterval),
                TaskNextRunTime = defaultTaskInterval.StartTime,
                TaskRunInSeparateThread = true,
            };
        }


        /// <summary>
        /// Gets randomly chosen date time from the interval 2 AM – 6 AM. The date part will be set to 01/01/0001.
        /// </summary>
        /// <remarks>
        /// The difference between random times is always 5 minutes k-fold.
        /// </remarks>
        /// <returns>Random DateTime from the off peak interval</returns>
        private static DateTime GetRandomOffPeakDateTime()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            
            // Get current date with the time set to 02:00:00 as the initial one
            var initialDate = DateTime.Now.Date.AddHours(2);

            var randomFeed = random.Next(0, 49);
            var finalDate = initialDate.AddMinutes(randomFeed * 5);

            // If the running time already passed, add another day to avoid immediately execution of the task after first request
            if (DateTime.Now > finalDate)
            {
                finalDate = finalDate.AddDays(1);
            }

            return finalDate;
        }

            
        /// <summary>
        /// Creates default task interval used when creating new task info. 
        /// </summary>
        /// <remarks>
        /// Time will be randomly chosen from interval 2 AM – 6 AM in order to fit execution to off peak time.
        /// Uses interval of one day, every day in week.
        /// </remarks>
        private static TaskInterval GenerateDefaultTaskInterval()
        {
            return new TaskInterval
            {
                Period = "Day",
                StartTime = GetRandomOffPeakDateTime(),
                Every = 1,
                Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList()
            };
        }
    }
}
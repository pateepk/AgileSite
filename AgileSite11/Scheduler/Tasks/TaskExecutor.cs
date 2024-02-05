using System.Security.Principal;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Task executor.
    /// </summary>
    public class TaskExecutor
    {
        #region "Variables"

        private int mTaskId;
        private TaskInfo mTask;
        private object mTaskInstance;

        #endregion


        #region "Properties"

        /// <summary>
        /// Task ID.
        /// </summary>
        public int TaskID
        {
            get
            {
                if (mTask != null)
                {
                    return mTask.TaskID;
                }
                return mTaskId;
            }
            set
            {
                mTaskId = value;
                mTask = null;
                mTaskInstance = null;
            }
        }


        /// <summary>
        /// Task instance object.
        /// </summary>
        private object TaskInstance
        {
            get
            {
                return mTaskInstance ?? (mTaskInstance = SchedulingExecutor.GetTaskInstance(Task));
            }
        }


        /// <summary>
        /// Task object.
        /// </summary>
        public TaskInfo Task
        {
            get
            {
                return mTask ?? (mTask = TaskInfoProvider.GetTaskInfo(TaskID));
            }
            set
            {
                mTask = value;
                if (value == null)
                {
                    mTaskId = 0;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the task.
        /// </summary>
        public void Run()
        {
            var ti = Task;
            if (ti != null)
            {
                SchedulingExecutor.ExecuteTask(ti);
            }
        }


        /// <summary>
        /// Runs the task in an asynchronous thread.
        /// </summary>
        /// <param name="wi">Windows identity</param>
        public void RunAsync(WindowsIdentity wi)
        {
            var thread = new CMSThread(Run, true);

            var taskImplementation = TaskInstance;
            if (taskImplementation != null)
            {
                // Provide more appropriate delegate info for the target action
                var taskCodeDelegate = SchedulingExecutor.GetTaskDelegate(taskImplementation);
                if (taskCodeDelegate != null)
                {
                    thread.LoadTargetDelegateInfo(taskCodeDelegate);
                }
            }

            thread.Start();
        }


        /// <summary>
        /// Runs the specified task in an asynchronous thread.
        /// </summary>
        /// <param name="taskId">Task ID to run</param>
        /// <param name="wi">Windows identity</param>
        public static void RunAsync(int taskId, WindowsIdentity wi)
        {
            var exec = new TaskExecutor();

            exec.TaskID = taskId;
            exec.RunAsync(wi);
        }


        /// <summary>
        /// Runs the specified task in an asynchronous thread.
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="wi">Windows identity</param>
        public static void RunAsync(TaskInfo task, WindowsIdentity wi)
        {
            var exec = new TaskExecutor();

            exec.Task = task;
            exec.RunAsync(wi);
        }

        #endregion
    }
}
using System;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing <see cref="TaskGroupTaskInfo"/> management.
    /// </summary>
    public class TaskGroupTaskInfoProvider : AbstractInfoProvider<TaskGroupTaskInfo, TaskGroupTaskInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all <see cref="TaskGroupTaskInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<TaskGroupTaskInfo> GetTaskGroupTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="TaskGroupTaskInfo"/> binding structure.
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>  
        public static TaskGroupTaskInfo GetTaskGroupTask(int taskGroupId, int taskId)
        {
            return ProviderObject.GetTaskGroupTaskInternal(taskGroupId, taskId);
        }


        /// <summary>
        /// Sets specified <see cref="TaskGroupTaskInfo"/>.
        /// </summary>
        /// <param name="infoObj">TaskGroupTask to set</param>
        public static void SetTaskGroupTask(TaskGroupTaskInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="TaskGroupTaskInfo"/> binding.
        /// </summary>
        /// <param name="infoObj"><see cref="TaskGroupTaskInfo"/> object</param>
        public static void DeleteTaskGroupTask(TaskGroupTaskInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="TaskGroupTaskInfo"/> binding.
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>  
        public static void RemoveTaskGroupFromTask(int taskGroupId, int taskId)
        {
            ProviderObject.RemovetaskGroupFromTaskInternal(taskGroupId, taskId);
        }


        /// <summary>
        /// Creates <see cref="TaskGroupTaskInfo"/> binding. 
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>   
        public static void AddTaskGroupToTask(int taskGroupId, int taskId)
        {
            ProviderObject.AddtaskGroupToTaskInternal(taskGroupId, taskId);
        }

        #endregion


        #region "Internal methods"
        
        /// <summary>
        /// Updates <see cref="TaskGroupTaskInfo"/> bindings for given where condition with values.
        /// </summary>
        /// <param name="where">Staging tasks group bindings to be updated</param>
        /// <param name="values">Values to be updated in columns</param>
        internal static void UpdateTaskGroupTask(IWhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values, false);
        }


        /// <summary>
        /// Returns the <see cref="TaskGroupTaskInfo"/> structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>  
        protected virtual TaskGroupTaskInfo GetTaskGroupTaskInternal(int taskGroupId, int taskId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("TaskGroupID", taskGroupId)
                .WhereEquals("TaskID", taskId).FirstOrDefault();
        }


        /// <summary>
        /// Deletes <see cref="TaskGroupTaskInfo"/> binding.
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>  
        protected virtual void RemovetaskGroupFromTaskInternal(int taskGroupId, int taskId)
        {
            var infoObj = GetTaskGroupTask(taskGroupId, taskId);
            if (infoObj != null)
            {
                DeleteTaskGroupTask(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="TaskGroupTaskInfo"/> binding. 
        /// </summary>
        /// <param name="taskGroupId">Task group ID</param>
        /// <param name="taskId">Staging task ID</param>   
        protected virtual void AddtaskGroupToTaskInternal(int taskGroupId, int taskId)
        {
            // Create new binding
            var infoObj = new TaskGroupTaskInfo
            {
                TaskGroupID = taskGroupId,
                TaskID = taskId
            };

            // Save to the database
            SetTaskGroupTask(infoObj);
        }


        /// <summary>
        /// Deletes all <see cref="TaskGroupTaskInfo"/> bindings that satisfy where condition.
        /// </summary>
        /// <param name="where">Where condition to which <see cref="TaskGroupTaskInfo"/> bindings to delete</param>
        internal static void DeleteTaskGroupTaskInfos(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}

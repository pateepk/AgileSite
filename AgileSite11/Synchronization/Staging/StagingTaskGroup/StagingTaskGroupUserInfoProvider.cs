using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing TaskGroupUserInfo management.
    /// </summary>
    public class TaskGroupUserInfoProvider : AbstractInfoProvider<TaskGroupUserInfo, TaskGroupUserInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all TaskGroupUserInfo bindings.
        /// </summary>
        public static ObjectQuery<TaskGroupUserInfo> GetTaskGroupUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns TaskGroupUserInfo binding structure.
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>  
        public static TaskGroupUserInfo GetTaskGroupUserInfo(int taskgroupId, int userId)
        {
            return ProviderObject.GetTaskGroupUserInfoInternal(taskgroupId, userId);
        }


        /// <summary>
        /// Sets specified TaskGroupUserInfo.
        /// </summary>
        /// <param name="infoObj">TaskGroupUserInfo to set</param>
        public static void SetTaskGroupUserInfo(TaskGroupUserInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified TaskGroupUserInfo binding.
        /// </summary>
        /// <param name="infoObj">TaskGroupUserInfo object</param>
        public static void DeleteTaskGroupUserInfo(TaskGroupUserInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes TaskGroupUserInfo binding.
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>  
        public static void RemoveTaskGroupFromUser(int taskgroupId, int userId)
        {
            ProviderObject.RemoveTaskGroupFromUserInternal(taskgroupId, userId);
        }


        /// <summary>
        /// Creates TaskGroupUserInfo binding. 
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>   
        public static void AddTaskGroupToUser(int taskgroupId, int userId)
        {
            ProviderObject.AddTaskGroupToUserInternal(taskgroupId, userId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the TaskGroupUserInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>  
        protected virtual TaskGroupUserInfo GetTaskGroupUserInfoInternal(int taskgroupId, int userId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("TaskGroupID", taskgroupId)
                .WhereEquals("UserID", userId).FirstOrDefault();
        }


        /// <summary>
        /// Deletes TaskGroupUserInfo binding.
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>  
        protected virtual void RemoveTaskGroupFromUserInternal(int taskgroupId, int userId)
        {
            var infoObj = GetTaskGroupUserInfo(taskgroupId, userId);
            if (infoObj != null)
            {
                DeleteTaskGroupUserInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates TaskGroupUserInfo binding. 
        /// </summary>
        /// <param name="taskgroupId">Task group ID</param>
        /// <param name="userId">User ID</param>   
        protected virtual void AddTaskGroupToUserInternal(int taskgroupId, int userId)
        {
            // Create new binding
            var infoObj = new TaskGroupUserInfo
            {
                TaskGroupID = taskgroupId,
                UserID = userId,
            };

            // Save to the database
            SetTaskGroupUserInfo(infoObj);
        }

        #endregion
    }
}
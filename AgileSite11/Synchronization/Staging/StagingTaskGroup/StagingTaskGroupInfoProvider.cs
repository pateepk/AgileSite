using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing TaskGroupInfo management.
    /// </summary>
    public class TaskGroupInfoProvider : AbstractInfoProvider<TaskGroupInfo, TaskGroupInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskGroupInfoProvider()
            : base(TaskGroupInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the TaskGroupInfo objects.
        /// </summary>
        public static ObjectQuery<TaskGroupInfo> GetTaskGroups()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns TaskGroupInfo with specified ID.
        /// </summary>
        /// <param name="id">TaskGroupInfo ID</param>
        public static TaskGroupInfo GetTaskGroupInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns TaskGroupInfo with specified name.
        /// </summary>
        /// <param name="name">TaskGroupInfo name</param>
        public static TaskGroupInfo GetTaskGroupInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Returns TaskGroupInfo with specified GUID.
        /// </summary>
        /// <param name="guid">TaskGroupInfo GUID</param>                
        public static TaskGroupInfo GetTaskGroupInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified TaskGroupInfo.
        /// </summary>
        /// <param name="infoObj">TaskGroupInfo to be set</param>
        public static void SetTaskGroupInfo(TaskGroupInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified TaskGroupInfo.
        /// </summary>
        /// <param name="infoObj">TaskGroupInfo to be deleted</param>
        public static void DeleteTaskGroupInfo(TaskGroupInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes TaskGroupInfo with specified ID.
        /// </summary>
        /// <param name="id">TaskGroupInfo ID</param>
        public static void DeleteTaskGroupInfo(int id)
        {
            TaskGroupInfo infoObj = GetTaskGroupInfo(id);
            DeleteTaskGroupInfo(infoObj);
        }


        /// <summary>
        /// Get TaskGroupInfo for user or null.
        /// </summary>    
        /// <param name="userID">User for whom to return his current task group ID</param>    
        public static TaskGroupInfo GetUserTaskGroupInfo(int userID)
        {
            return ProviderObject.GetUserTaskGroupInfoInternal(userID);
        }


        /// <summary>
        /// Sets task group info for user.
        /// </summary>
        /// <param name="taskGroupID">
        /// Task group ID to be bound with user. If <paramref name="taskGroupID"/> is 0, user won't be assigned to any task group.
        /// </param>
        /// <param name="userID">User to whom set task group.</param>       
        /// <remarks>
        /// It's not recommend to set the task group for users without their knowledge. 
        /// For example, a suitable way to call the method is in the logic of a custom interface that allows users to set their task group.
        /// </remarks>
        public static void SetTaskGroupForUser(int taskGroupID, int userID)
        {
            ProviderObject.SetTaskGroupForUserInternal(taskGroupID, userID);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Get TaskGroupInfo for user or null.
        /// </summary>       
        /// <param name="userID">User for whom to return his current task group ID</param>
        protected virtual TaskGroupInfo GetUserTaskGroupInfoInternal(int userID)
        {
            return TaskGroupInfoProvider.GetTaskGroups().WhereIn("TaskGroupID", TaskGroupUserInfoProvider.GetTaskGroupUsers().WhereEquals("UserID", userID).Column("TaskGroupID")).FirstObject;
        }


        /// <summary>
        /// Sets task group info for user.
        /// </summary>
        /// <param name="taskGroupID">
        /// Task group ID to be bound with user. If <paramref name="taskGroupID"/> is 0, user won't be assigned to any task group.. 
        /// </param>
        /// <param name="userID">User to whom set task group</param>       
        /// <remarks>
        /// It's not recommend to set the task group for users without their knowledge. 
        /// For example, a suitable way to call the method is in the logic of a custom interface that allows users to set their task group.
        /// </remarks>
        protected virtual void SetTaskGroupForUserInternal(int taskGroupID, int userID)
        {
            // Task group to update or delete
            var taskGroupUser = TaskGroupUserInfoProvider.GetTaskGroupUsers().WhereEquals("UserID", userID).FirstObject;

            if (taskGroupID == 0)
            {
                TaskGroupUserInfoProvider.DeleteTaskGroupUserInfo(taskGroupUser);
            }
            else if (taskGroupID > 0 && taskGroupUser != null)
            {
                taskGroupUser.UserID = userID;
                taskGroupUser.TaskGroupID = taskGroupID;
                TaskGroupUserInfoProvider.SetTaskGroupUserInfo(taskGroupUser);
            }
            else if (taskGroupUser == null)
            {
                var newTaskGroupUser = new TaskGroupUserInfo
                {
                    UserID = userID,
                    TaskGroupID = taskGroupID
                };

                TaskGroupUserInfoProvider.SetTaskGroupUserInfo(newTaskGroupUser);
            }
        }

        #endregion
    }
}
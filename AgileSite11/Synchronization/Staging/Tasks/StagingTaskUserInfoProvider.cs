using System.Collections.Generic;

using CMS.DataEngine;
using System.Linq;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing <see cref="StagingTaskUserInfo"/> management.
    /// </summary>
    public class StagingTaskUserInfoProvider : AbstractInfoProvider<StagingTaskUserInfo, StagingTaskUserInfoProvider>
    {
        #region "Public methods"

		/// <summary>
        /// Returns all <see cref="StagingTaskUserInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<StagingTaskUserInfo> GetTaskUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


		/// <summary>
        /// Returns <see cref="StagingTaskUserInfo"/> binding structure.
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>  
        public static StagingTaskUserInfo GetStagingTaskUserInfo(int taskId, int userId)
        {
            return ProviderObject.GetStagingTaskInfoInternal(taskId, userId);
        }


        /// <summary>
        /// Sets specified <see cref="StagingTaskUserInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="StagingTaskUserInfo"/> to set</param>
        public static void SetStagingTaskUserInfo(StagingTaskUserInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="StagingTaskUserInfo"/> binding.
        /// </summary>
        /// <param name="infoObj"><see cref="StagingTaskUserInfo"/> object</param>
        public static void DeleteStagingTaskUserInfo(StagingTaskUserInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="StagingTaskUserInfo"/> binding.
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>  
        public static void RemoveStagingTaskFromUser(int taskId, int userId)
        {
            ProviderObject.RemoveStagingTaskFromUserInternal(taskId, userId);
        }


        /// <summary>
        /// Creates <see cref="StagingTaskUserInfo"/> binding. 
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>   
        public static void AddStagingTaskToUser(int taskId, int userId)
        {
            ProviderObject.AddStagingTaskToUserInternal(taskId, userId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Updates staging tasks user bindings for given where condition with values.
        /// </summary>
        /// <param name="where">Staging tasks user bindings to be updated</param>
        /// <param name="values">Values to be updated in columns</param>
        internal static void UpdateStagingTaskUsers(IWhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values);
        }


        /// <summary>
        /// Returns the <see cref="StagingTaskUserInfo"/> structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>  
        protected virtual StagingTaskUserInfo GetStagingTaskInfoInternal(int taskId, int userId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("TaskID", taskId)
                .WhereEquals("UserID", userId).FirstOrDefault();
        }


		/// <summary>
        /// Deletes <see cref="StagingTaskUserInfo"/> binding.
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>  
        protected virtual void RemoveStagingTaskFromUserInternal(int taskId, int userId)
        {
            var infoObj = GetStagingTaskUserInfo(taskId, userId);
			if (infoObj != null) 
			{
                DeleteStagingTaskUserInfo(infoObj);
			}
        }


        /// <summary>
        /// Creates <see cref="StagingTaskUserInfo"/> binding. 
        /// </summary>
        /// <param name="taskId">StagingTaskInfo ID</param>
        /// <param name="userId">UserInfo ID</param>   
        protected virtual void AddStagingTaskToUserInternal(int taskId, int userId)
        {
            // Create new binding
            var infoObj = new StagingTaskUserInfo();
            infoObj.TaskID = taskId;
			infoObj.UserID = userId;

            // Save to the database
            SetStagingTaskUserInfo(infoObj);
        }

        #endregion		
    }
}
﻿using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowUserInfo>;

    /// <summary>
    /// Class providing WorkflowUserInfo management.
    /// </summary>
    public class WorkflowUserInfoProvider : AbstractInfoProvider<WorkflowUserInfo, WorkflowUserInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all workflow users.
        /// </summary>
        public static ObjectQuery<WorkflowUserInfo> GetWorkflowUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the WorkflowUserInfo structure for the specified workflowUser.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="userId">User ID</param>
        public static WorkflowUserInfo GetWorkflowUserInfo(int workflowId, int userId)
        {
            return ProviderObject.GetWorkflowUserInfoInternal(workflowId, userId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflowUser.
        /// </summary>
        /// <param name="workflowUser">WorkflowUser to set</param>
        public static void SetWorkflowUserInfo(WorkflowUserInfo workflowUser)
        {
            ProviderObject.SetInfo(workflowUser);
        }


        /// <summary>
        /// Deletes specified workflowUser.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="userId">User ID</param>
        public static void RemoveUserFromWorkflow(int workflowId, int userId)
        {
            WorkflowUserInfo infoObj = GetWorkflowUserInfo(workflowId, userId);
            DeleteWorkflowUserInfo(infoObj);
        }


        /// <summary>
        /// Adds specified user to the workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="userId">User ID</param>
        public static void AddUserToWorkflow(int workflowId, int userId)
        {
            // Create new binding
            WorkflowUserInfo infoObj = new WorkflowUserInfo();
            infoObj.WorkflowID = workflowId;
            infoObj.UserID = userId;

            // Save to the database
            SetWorkflowUserInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified workflowUser.
        /// </summary>
        /// <param name="workflowUser">WorkflowUser object</param>
        public static void DeleteWorkflowUserInfo(WorkflowUserInfo workflowUser)
        {
            ProviderObject.DeleteInfo(workflowUser);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WorkflowUserInfo structure for the specified workflowUser.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="userId">User ID</param>
        protected virtual WorkflowUserInfo GetWorkflowUserInfoInternal(int workflowId, int userId)
        {
            var condition = new WhereCondition()
                .WhereEquals("WorkflowID", workflowId)
                .WhereEquals("UserID", userId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion
    }
}
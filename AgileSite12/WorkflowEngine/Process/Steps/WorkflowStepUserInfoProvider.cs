using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowStepUserInfo>;

    /// <summary>
    /// Class providing WorkflowStepUserInfo management.
    /// </summary>
    public class WorkflowStepUserInfoProvider : AbstractInfoProvider<WorkflowStepUserInfo, WorkflowStepUserInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns workflow step users.
        /// </summary>
        public static ObjectQuery<WorkflowStepUserInfo> GetWorkflowStepUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the WorkflowStepUserInfo structure for the specified workflowStepUser.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        public static WorkflowStepUserInfo GetWorkflowStepUserInfo(int stepId, int userId)
        {
            return GetWorkflowStepUserInfo(stepId, userId, Guid.Empty);
        }


        /// <summary>
        /// Returns the WorkflowStepUserInfo structure for the specified workflow step, user and source point GUID.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static WorkflowStepUserInfo GetWorkflowStepUserInfo(int stepId, int userId, Guid sourcePointGuid)
        {
            return ProviderObject.GetWorkflowStepUserInfoInternal(stepId, userId, sourcePointGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflowStepUser.
        /// </summary>
        /// <param name="workflowStepUser">WorkflowStepUser to set</param>
        public static void SetWorkflowStepUserInfo(WorkflowStepUserInfo workflowStepUser)
        {
            ProviderObject.SetInfo(workflowStepUser);
        }


        /// <summary>
        /// Deletes specified workflowStepUser.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        public static void RemoveUserFromWorkflowStep(int stepId, int userId)
        {
            RemoveUserFromWorkflowStep(stepId, userId, Guid.Empty);
        }


        /// <summary>
        /// Removes user from workflow step source point.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static void RemoveUserFromWorkflowStep(int stepId, int userId, Guid sourcePointGuid)
        {
            WorkflowStepUserInfo infoObj = GetWorkflowStepUserInfo(stepId, userId, sourcePointGuid);
            DeleteWorkflowStepUserInfo(infoObj);
        }


        /// <summary>
        /// Adds specified user to the workflow step.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        public static void AddUserToWorkflowStep(int stepId, int userId)
        {
            AddUserToWorkflowStep(stepId, userId, Guid.Empty);
        }


        /// <summary>
        /// Adds specified user to the workflow step source point.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static void AddUserToWorkflowStep(int stepId, int userId, Guid sourcePointGuid)
        {
            // Create new binding
            WorkflowStepUserInfo infoObj = new WorkflowStepUserInfo();
            infoObj.StepID = stepId;
            infoObj.UserID = userId;
            infoObj.StepSourcePointGUID = sourcePointGuid;

            // Save to the database
            SetWorkflowStepUserInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified workflowStepUser.
        /// </summary>
        /// <param name="workflowStepUser">WorkflowStepUser object</param>
        public static void DeleteWorkflowStepUserInfo(WorkflowStepUserInfo workflowStepUser)
        {
            ProviderObject.DeleteInfo(workflowStepUser);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WorkflowStepUserInfo structure for the specified workflowStepUser.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        protected virtual WorkflowStepUserInfo GetWorkflowStepUserInfoInternal(int stepId, int userId, Guid sourcePointGuid)
        {
            var condition = new WhereCondition()
                .WhereEquals("StepID", stepId)
                .WhereEquals("UserID", userId);

            if (sourcePointGuid == Guid.Empty)
            {
                condition.WhereNull("StepSourcePointGUID");
            }
            else
            {
                condition.WhereEquals("StepSourcePointGUID", sourcePointGuid);
            }

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Deletes the workflow step users in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        internal static void DeleteWorkflowStepUserInfos(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}
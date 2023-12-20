using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowStepRoleInfo>;

    /// <summary>
    /// Class providing WorkflowStepRoleInfo management.
    /// </summary>
    public class WorkflowStepRoleInfoProvider : AbstractInfoProvider<WorkflowStepRoleInfo, WorkflowStepRoleInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns workflow step roles.
        /// </summary>
        public static ObjectQuery<WorkflowStepRoleInfo> GetWorkflowStepRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the WorkflowStepRoleInfo structure for the specified workflowStepRole.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        public static WorkflowStepRoleInfo GetWorkflowStepRoleInfo(int stepId, int roleId)
        {
            return GetWorkflowStepRoleInfo(stepId, roleId, Guid.Empty);
        }


        /// <summary>
        /// Returns the WorkflowStepRoleInfo structure for the specified workflow step, role and source point GUID.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static WorkflowStepRoleInfo GetWorkflowStepRoleInfo(int stepId, int roleId, Guid sourcePointGuid)
        {
            return ProviderObject.GetWorkflowStepRoleInfoInternal(stepId, roleId, sourcePointGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflowStepRole.
        /// </summary>
        /// <param name="workflowStepRole">WorkflowStepRole to set</param>
        public static void SetWorkflowStepRoleInfo(WorkflowStepRoleInfo workflowStepRole)
        {
            ProviderObject.SetInfo(workflowStepRole);
        }


        /// <summary>
        /// Deletes specified workflowStepRole.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        public static void RemoveRoleFromWorkflowStep(int stepId, int roleId)
        {
            RemoveRoleFromWorkflowStep(stepId, roleId, Guid.Empty);
        }


        /// <summary>
        /// Removes role from workflow step source point.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static void RemoveRoleFromWorkflowStep(int stepId, int roleId, Guid sourcePointGuid)
        {
            WorkflowStepRoleInfo infoObj = GetWorkflowStepRoleInfo(stepId, roleId, sourcePointGuid);
            DeleteWorkflowStepRoleInfo(infoObj);
        }


        /// <summary>
        /// Adds specified role to the workflow step.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        public static void AddRoleToWorkflowStep(int stepId, int roleId)
        {
            AddRoleToWorkflowStep(stepId, roleId, Guid.Empty);
        }


        /// <summary>
        /// Adds specified role to the workflow step source point.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public static void AddRoleToWorkflowStep(int stepId, int roleId, Guid sourcePointGuid)
        {
            // Create new binding
            WorkflowStepRoleInfo infoObj = new WorkflowStepRoleInfo();
            infoObj.StepID = stepId;
            infoObj.RoleID = roleId;
            infoObj.StepSourcePointGUID = sourcePointGuid;

            // Save to the database
            SetWorkflowStepRoleInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified workflowStepRole.
        /// </summary>
        /// <param name="workflowStepRole">WorkflowStepRole object</param>
        public static void DeleteWorkflowStepRoleInfo(WorkflowStepRoleInfo workflowStepRole)
        {
            ProviderObject.DeleteInfo(workflowStepRole);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WorkflowStepRoleInfo structure for the specified workflowStepRole.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        protected virtual WorkflowStepRoleInfo GetWorkflowStepRoleInfoInternal(int stepId, int roleId, Guid sourcePointGuid)
        {
            var condition = new WhereCondition()
                .WhereEquals("StepID", stepId)
                .WhereEquals("RoleID", roleId);

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
        /// Deletes the workflow step roles in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        internal static void DeleteWorkflowStepRoleInfos(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        } 

        #endregion
    }
}
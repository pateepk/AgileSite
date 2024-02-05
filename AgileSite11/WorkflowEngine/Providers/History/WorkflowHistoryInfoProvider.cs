using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class providing WorkflowHistoryInfo management.
    /// </summary>
    public class WorkflowHistoryInfoProvider : AbstractInfoProvider<WorkflowHistoryInfo, WorkflowHistoryInfoProvider>
    {
        /// <summary>
        /// Returns all workflow histories.
        /// </summary>
        public static ObjectQuery<WorkflowHistoryInfo> GetWorkflowHistories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the WorkflowHistoryInfo structure for the specified workflow history.
        /// </summary>
        /// <param name="workflowHistoryId">History ID.</param>
        public static WorkflowHistoryInfo GetWorkflowHistoryInfo(int workflowHistoryId)
        {
            return ProviderObject.GetInfoById(workflowHistoryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflow history.
        /// </summary>
        /// <param name="workflowHistory">History to set.</param>
        public static void SetWorkflowHistoryInfo(WorkflowHistoryInfo workflowHistory)
        {
            ProviderObject.SetInfo(workflowHistory);
        }


        /// <summary>
        /// Deletes specified workflow history.
        /// </summary>
        /// <param name="workflowHistory">History to delete.</param>
        public static void DeleteWorkflowHistoryInfo(WorkflowHistoryInfo workflowHistory)
        {
            ProviderObject.DeleteInfo(workflowHistory);
        }


        /// <summary>
        /// Gets last approval action from given step.
        /// </summary>
        /// <param name="stepId">Step ID.</param>
        /// <param name="objectType">Object type.</param>
        /// <param name="objectId">Object ID.</param>
        public static WorkflowHistoryInfo GetLastApprovalActionFromStep(int stepId, string objectType, int objectId)
        {
            var last = GetLastApprovalActionQuery(objectType, objectId)
                .WhereEquals("StepID", stepId)
                .FirstOrDefault();

            if (last != null)
            {
                return last;
            }

            // Backward compatibility for document workflow history
            if (objectType == PredefinedObjectType.DOCUMENT)
            {
                return GetWorkflowHistories()
                    .TopN(1)
                    .WhereIn("VersionHistoryID", new ObjectQuery(PredefinedObjectType.VERSIONHISTORY)
                        .WhereEquals("DocumentID", objectId)
                        .AsIDQuery())
                    .WhereEquals("StepID", stepId)
                    .WhereFalse("WasRejected")
                    .OrderByDescending("WorkflowHistoryID")
                    .FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Gets last approval action to given step for advanced workflow only.
        /// </summary>
        /// <param name="stepId">Step ID.</param>
        /// <param name="objectType">Object type.</param>
        /// <param name="objectId">Object ID.</param>
        public static WorkflowHistoryInfo GetLastApprovalActionToStep(int stepId, string objectType, int objectId)
        {
            return GetLastApprovalActionQuery(objectType, objectId)
                .WhereEquals("TargetStepID", stepId)
                .WhereEqualsOrNull("HistoryRejected", 0)
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets last approval action from first step for advanced workflow only.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <param name="objectId">Object ID.</param>
        public static WorkflowHistoryInfo GetLastApprovalActionFromFirstStep(string objectType, int objectId)
        {
            return GetLastApprovalActionQuery(objectType, objectId)
                .WhereEquals("StepType", (int)WorkflowStepTypeEnum.DocumentEdit)
                .FirstOrDefault();
        }


        /// <summary>
        /// Changes document ID for workflow histories.
        /// </summary>
        /// <param name="originalDocumentId">Original document ID.</param>
        /// <param name="newDocumentId">New document ID.</param>
        public static void ChangeDocument(int originalDocumentId, int newDocumentId)
        {
            ProviderObject.UpdateData(
                new WhereCondition()
                    .WhereEquals("HistoryObjectType", PredefinedObjectType.DOCUMENT)
                    .WhereEquals("HistoryObjectID", originalDocumentId),
                new Dictionary<string, object>
                {
                    { "HistoryObjectID", newDocumentId }
                });
        }


        /// <summary>
        /// Marks workflow histories as used when rejecting to specific step.
        /// </summary>
        /// <param name="startHistoryId">Start history ID.</param>
        /// <param name="endHistoryId">End history ID.</param>
        /// <param name="workflowId">Workflow ID.</param>
        /// <param name="objectType">Object type.</param>
        /// <param name="objectId">Object ID.</param>
        public static void MarkRejected(int startHistoryId, int endHistoryId, int workflowId, string objectType, int objectId)
        {
            ProviderObject.UpdateData(
                new WhereCondition()
                    .WhereLessOrEquals("WorkflowHistoryID", startHistoryId)
                    .WhereGreaterOrEquals("WorkflowHistoryID", endHistoryId)
                    .WhereEqualsOrNull("HistoryTransitionType", (int)WorkflowTransitionTypeEnum.Manual)
                    .WhereEquals("HistoryObjectType", objectType)
                    .WhereEquals("HistoryObjectID", objectId)
                    .WhereEquals("HistoryWorkflowID", workflowId)
                    .WhereFalse("WasRejected")
                    .WhereEqualsOrNull("HistoryRejected", 0),
                new Dictionary<string, object>
                {
                    { "HistoryRejected", 1 }
                });
        }


        private static ObjectQuery<WorkflowHistoryInfo> GetLastApprovalActionQuery(string objectType, int objectId)
        {
            return GetWorkflowHistories()
                .TopN(1)
                .WhereEquals("HistoryObjectType", objectType)
                .WhereEquals("HistoryObjectID", objectId)
                .WhereFalse("WasRejected")
                .OrderByDescending("WorkflowHistoryID");
        }
    }
}
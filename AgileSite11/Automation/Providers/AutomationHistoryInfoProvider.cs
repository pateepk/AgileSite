using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Automation
{
    using TypedDataSet = InfoDataSet<AutomationHistoryInfo>;


    /// <summary>
    /// Class providing AutomationHistoryInfo management.
    /// </summary>
    public class AutomationHistoryInfoProvider : AbstractInfoProvider<AutomationHistoryInfo, AutomationHistoryInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all automation history records.
        /// </summary>
        public static ObjectQuery<AutomationHistoryInfo> GetAutomationHistories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all automation history records matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetAutomationHistories() instead.")]
        public static TypedDataSet GetAutomationHistories(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetAutomationHistoriesInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns automation history with specified ID.
        /// </summary>
        /// <param name="historyId">Automation history ID.</param>        
        public static AutomationHistoryInfo GetAutomationHistoryInfo(int historyId)
        {
            return ProviderObject.GetInfoById(historyId);
        }


        /// <summary>
        /// Returns automation history with specified name.
        /// </summary>
        /// <param name="historyName">Automation history name.</param>                
        public static AutomationHistoryInfo GetAutomationHistoryInfo(string historyName)
        {
            return ProviderObject.GetInfoByCodeName(historyName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified automation history.
        /// </summary>
        /// <param name="historyObj">Automation history to be set.</param>
        public static void SetAutomationHistoryInfo(AutomationHistoryInfo historyObj)
        {
            ProviderObject.SetInfo(historyObj);
        }


        /// <summary>
        /// Deletes specified automation history.
        /// </summary>
        /// <param name="historyObj">Automation history to be deleted.</param>
        public static void DeleteAutomationHistoryInfo(AutomationHistoryInfo historyObj)
        {
            ProviderObject.DeleteInfo(historyObj);
        }


        /// <summary>
        /// Deletes automation history with specified ID.
        /// </summary>
        /// <param name="historyId">Automation history ID.</param>
        public static void DeleteAutomationHistoryInfo(int historyId)
        {
            AutomationHistoryInfo historyObj = GetAutomationHistoryInfo(historyId);
            DeleteAutomationHistoryInfo(historyObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets last approval action from given step
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="stateId">State object ID</param>
        public static AutomationHistoryInfo GetLastApprovalActionFromStep(int stepId, int stateId)
        {
            return ProviderObject.GetLastApprovalActionInternal(stepId, stateId, true);
        }


        /// <summary>
        /// Gets last approval action to given step
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="stateId">State object ID</param>
        public static AutomationHistoryInfo GetLastApprovalActionToStep(int stepId, int stateId)
        {
            return ProviderObject.GetLastApprovalActionInternal(stepId, stateId, false);
        }


        /// <summary>
        /// Gets last approval action from first step
        /// </summary>
        /// <param name="stateId">State object ID</param>
        public static AutomationHistoryInfo GetLastApprovalActionFromFirstStep(int stateId)
        {
            return ProviderObject.GetLastApprovalActionFromFirstStepInternal(stateId);
        }


        /// <summary>
        /// Marks automation histories as used when rejecting to specific step.
        /// </summary>
        /// <param name="startHistoryId">Start history ID</param>
        /// <param name="endHistoryId">End history ID</param>
        /// <param name="stateId">State object ID</param>
        public static void MarkRejected(int startHistoryId, int endHistoryId, int stateId)
        {
            var where = new WhereCondition()
                .WhereLessOrEquals("HistoryID", startHistoryId)
                .And()
                .WhereGreaterOrEquals("HistoryID", endHistoryId)
                .And()
                .Where(new WhereCondition()
                    .WhereNull("HistoryTransitionType")
                    .Or()
                    .WhereEquals("HistoryTransitionType", 0))
                .And()
                .WhereEquals("HistoryStateID", stateId)
                .And()
                .WhereEquals("HistoryWasRejected", 0)
                .And()
                .Where(new WhereCondition()
                    .WhereNull("HistoryRejected")
                    .Or()
                    .WhereEquals("HistoryRejected", 0));

            ProviderObject.UpdateData("[HistoryRejected] = 1", where.Parameters, where.ToString());
        }


        /// <summary>
        /// Removes histories.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteObjectsHistories(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns dataset of all automation history records matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetAutomationHistories() instead.")]
        protected virtual TypedDataSet GetAutomationHistoriesInternal(string where, string orderBy, int topN, string columns)
        {
            return GetAutomationHistories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets last approval action to/from given step
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="stateId">State object ID</param>
        /// <param name="startStep">Indicates if given step is start step</param>
        protected virtual AutomationHistoryInfo GetLastApprovalActionInternal(int stepId, int stateId, bool startStep)
        {
            var columnName = startStep ? "HistoryStepID" : "HistoryTargetStepID";

            var where = new WhereCondition()
                .WhereEquals("HistoryStateID", stateId)
                .And()
                .WhereEquals(columnName, stepId)
                .And()
                .WhereEquals("HistoryWasRejected", 0)
                .And()
                .Where(new WhereCondition()
                    .WhereNull("HistoryRejected")
                    .Or()
                    .WhereEquals("HistoryRejected", 0));

            return GetAutomationHistories().Where(where).OrderByDescending("HistoryID").TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Gets last approval action from first step
        /// </summary>
        /// <param name="stateId">State object ID</param>
        protected virtual AutomationHistoryInfo GetLastApprovalActionFromFirstStepInternal(int stateId)
        {
            var where = new WhereCondition()
                .WhereEquals("HistoryStateID", stateId)
                .And()
                .WhereEquals("HistoryStepType", (int)WorkflowStepTypeEnum.Start)
                .And()
                .WhereEquals("HistoryWasRejected", 0);

            return GetAutomationHistories().Where(where).OrderByDescending("HistoryID").TopN(1).FirstOrDefault();
        }

        #endregion
    }
}

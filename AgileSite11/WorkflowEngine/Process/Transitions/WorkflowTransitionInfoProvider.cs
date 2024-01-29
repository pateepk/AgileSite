using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowTransitionInfo>;

    /// <summary>
    /// Class providing WorkflowTransitionInfo management.
    /// </summary>
    public class WorkflowTransitionInfoProvider : AbstractInfoProvider<WorkflowTransitionInfo, WorkflowTransitionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowTransitionInfoProvider()
            : base(WorkflowTransitionInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    GUID = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all workflow transitions.
        /// </summary>
        public static ObjectQuery<WorkflowTransitionInfo> GetWorkflowTransitions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all workflow transitions matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetWorkflowTransitions() instead.")]
        public static TypedDataSet GetWorkflowTransitions(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetWorkflowTransitionsInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns the Dataset of workflow transitions.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        [Obsolete("Use method GetWorkflowTransitions() instead.")]
        public static TypedDataSet GetWorkflowTransitions(int workflowId)
        {
            return GetWorkflowTransitions(workflowId, null, null, 0, null);
        }


        /// <summary>
        /// Returns dataset of workflow transitions for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetWorkflowTransitions() instead.")]
        public static TypedDataSet GetWorkflowTransitions(int workflowId, string where, string orderBy, int topN, string columns)
        {
            where = SqlHelper.AddWhereCondition(where, "TransitionWorkflowID = " + workflowId);
            return GetWorkflowTransitions(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns dataset of all workflow transitions matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        [Obsolete("Use method GetWorkflowTransitions() instead.")]
        public static TypedDataSet GetWorkflowTransitions(string where, string orderBy)
        {
            return ProviderObject.GetWorkflowTransitionsInternal(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns workflow transition with specified ID.
        /// </summary>
        /// <param name="transitionId">Workflow transition ID.</param>        
        public static WorkflowTransitionInfo GetWorkflowTransitionInfo(int transitionId)
        {
            return ProviderObject.GetInfoById(transitionId);
        }


        /// <summary>
        /// Returns workflow transition with specified GUID.
        /// </summary>
        /// <param name="transitionGuid">Workflow transition GUID.</param>                
        public static WorkflowTransitionInfo GetWorkflowTransitionInfo(Guid transitionGuid)
        {
            return ProviderObject.GetInfoByGuid(transitionGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflow transition.
        /// </summary>
        /// <param name="transitionObj">Workflow transition to be set.</param>
        public static void SetWorkflowTransitionInfo(WorkflowTransitionInfo transitionObj)
        {
            ProviderObject.SetInfo(transitionObj);
        }


        /// <summary>
        /// Deletes specified workflow transition.
        /// </summary>
        /// <param name="transitionObj">Workflow transition to be deleted.</param>
        public static void DeleteWorkflowTransitionInfo(WorkflowTransitionInfo transitionObj)
        {
            ProviderObject.DeleteInfo(transitionObj);
        }


        /// <summary>
        /// Deletes workflow transition with specified ID.
        /// </summary>
        /// <param name="transitionId">Workflow transition ID.</param>
        public static void DeleteWorkflowTransitionInfo(int transitionId)
        {
            WorkflowTransitionInfo transitionObj = GetWorkflowTransitionInfo(transitionId);
            DeleteWorkflowTransitionInfo(transitionObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets workflow step transitions
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="where">Where condition</param>
        public static List<WorkflowTransitionInfo> GetStepTransitions(WorkflowStepInfo step, string where)
        {
            return ProviderObject.GetStepTransitionsInternal(step, where);
        }


        /// <summary>
        /// Sets given end step ID to all transitions matching specified where condition.
        /// </summary>
        /// <param name="where">Where condition for selection all transitions to be updated.</param>
        /// <param name="endStepID">Desired transition end step ID</param>
        public static void UpdateTransitionsEndStep(string where, int endStepID)
        {
            var values = new Dictionary<string, object> { { "TransitionEndStepID", endStepID } };
            ProviderObject.UpdateData(new WhereCondition(where), values);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns dataset of all workflow transitions matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        [Obsolete("Use method GetWorkflowTransitions() instead.")]
        protected virtual TypedDataSet GetWorkflowTransitionsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetWorkflowTransitions().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WorkflowTransitionInfo info)
        {
            // Ensure correct connection type
            if ((info != null) && (info.ItemChanged("TransitionStartStepID") || info.ItemChanged("TransitionSourcePointGUID")))
            {
                var startStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(info.TransitionStartStepID);
                info.TransitionType = startStep.GetStepConnectionType(info.TransitionSourcePointGUID);
            }

            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the workflow transitions in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        internal static void DeleteWorkflowTransitionInfos(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets workflow step transitions
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="where">Where condition</param>
        protected virtual List<WorkflowTransitionInfo> GetStepTransitionsInternal(WorkflowStepInfo step, string where)
        {
            List<WorkflowTransitionInfo> transitions = new List<WorkflowTransitionInfo>();

            // Get step transitions
            where = SqlHelper.AddWhereCondition("TransitionStartStepID = " + step.StepID, where);
            InfoDataSet<WorkflowTransitionInfo> trans = GetWorkflowTransitions().Where(where).TypedResult;
            foreach (var transition in trans.Items)
            {
                transitions.Add(transition);
            }

            return transitions;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.Modules;

namespace CMS.WorkflowEngine.GraphConfig
{
    /// <summary>
    /// Definition of workflow graph
    /// </summary>
    public class WorkflowGraph : Graph
    {
        #region "Variables"

        /// <summary>
        /// Workflow to be converted.
        /// </summary>
        private readonly WorkflowInfo mWorkflow;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates graph configuration object from given Workflow.
        /// </summary>
        /// <returns>Graph configuration object</returns>
        public WorkflowGraph(WorkflowInfo workflow)
        {
            if (workflow == null)
            {
                throw new NullReferenceException("[WorkflowGraph] : Workflow is null.");
            }

            mWorkflow = workflow;
            ID = mWorkflow.WorkflowID.ToString();

            if (workflow.IsBasic)
            {
                SuplementBasicNodesDefinition();
            }
            ExtendGraphResources();
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Returns default list of connections based on given information.
        /// </summary>
        /// <returns>List of connections.</returns>
        protected override List<Connection> GetDefaultConnections()
        {
            List<Connection> result = new List<Connection>();
            InfoDataSet<WorkflowTransitionInfo> connections = WorkflowTransitionInfoProvider.GetWorkflowTransitions().WhereEquals("TransitionWorkflowID", mWorkflow.WorkflowID).TypedResult;
            foreach (WorkflowTransitionInfo connection in connections)
            {
                Connection newConnection = new WorkflowConnection(connection);
                result.Add(newConnection);
            }
            return result;
        }


        /// <summary>
        /// Returns default list of nodes based on given information.
        /// </summary>
        /// <returns>List of nodes</returns>
        protected override List<Node> GetDefaultNodes()
        {
            List<Node> nodes = WorkflowStepInfoProvider.GetWorkflowSteps(mWorkflow.WorkflowID)
                                                       .OrderBy("StepType", "StepOrder")
                                                       .ToList()
                                                       .Select<WorkflowStepInfo, Node>(WorkflowNode.GetInstance)
                                                       .ToList();
            return nodes;
        }


        /// <summary>
        /// Supplements definition of basic nodes to be printable.
        /// </summary>
        protected void SuplementBasicNodesDefinition()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                AddConnection(i - 1, i);
                Nodes[i].Position = new Point(-1,-1);
            }
        }


        /// <summary>
        /// Adds single connection based on given indexes from list of Nodes.
        /// </summary>
        /// <param name="fromIndex">Index from nodes</param>
        /// <param name="toIndex">Index from nodes</param>
        protected void AddConnection(int fromIndex, int toIndex)
        {
            if ((fromIndex >= 0) && (toIndex >= 0) && (fromIndex < Nodes.Count) && (toIndex < Nodes.Count))
            {
                Connection newConnection = new WorkflowConnection(Nodes[fromIndex].ID, Nodes[toIndex].ID);
                Connections.Add(newConnection);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Extends resource dictionaries for graph in JS.
        /// </summary>
        private void ExtendGraphResources()
        {
            Addresses.Add("EditNodeDialog", ApplicationUrlHelper.GetElementDialogUrl("CMS", "Edit.Workflows.EditStepDialog"));
            Addresses.Add("EditCaseDialog", ApplicationUrlHelper.GetElementDialogUrl("CMS", "Workflows.EditCase"));
         
            GraphResourceStrings.Add("ConditionTooltip", ResHelper.GetString("workflow.conditionsteptooltip"));
            GraphResourceStrings.Add("MultichoiceTooltip", ResHelper.GetString("workflow.multichoicesteptooltip"));
            GraphResourceStrings.Add("MultichoiceFirstWinTooltip", ResHelper.GetString("workflow.multichoicefirstwinsteptooltip"));
            GraphResourceStrings.Add("UserchoiceTooltip", ResHelper.GetString("workflow.userchoicesteptooltip"));
            GraphResourceStrings.Add("ConditionCase", ResHelper.GetString("workflow.conditionstepcase"));
            GraphResourceStrings.Add("ConditionDefault", ResHelper.GetString("workflow.conditionstepdefault"));
            GraphResourceStrings.Add("MultichoiceCase", ResHelper.GetString("workflow.multichoicestepcase"));
            GraphResourceStrings.Add("MultichoiceDefault", ResHelper.GetString("workflow.multichoicestepdefault"));
            GraphResourceStrings.Add("MultichoiceFirstWinCase", ResHelper.GetString("workflow.multichoicestepfirstwincase"));
            GraphResourceStrings.Add("MultichoiceFirstWinDefault", ResHelper.GetString("workflow.multichoicestepfirstwindefault"));
            GraphResourceStrings.Add("UserchoiceCase", ResHelper.GetString("workflow.userchoicestepcase"));
        }

        #endregion
    }
}

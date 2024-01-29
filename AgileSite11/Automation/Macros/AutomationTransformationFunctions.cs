using System;
using System.Linq;

using CMS.DataEngine;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Functions for automation macro methods.
    /// </summary>
    public class AutomationTransformationFunctions
    {
        #region "Public methods"

        /// <summary>
        /// Returns true if process had passed through one/all selected automation actions.
        /// </summary>
        /// <param name="state">Process instance to check</param>
        /// <param name="actions">Automation action names separated with a semicolon</param>
        /// <param name="allActions">If true all actions must have been passed.</param>
        public static bool PassedThroughActions(object state, string actions, bool allActions)
        {
            AutomationStateInfo si = state as AutomationStateInfo;
            if (si != null)
            {
                if (!String.IsNullOrEmpty(actions))
                {
                    // Get IDs of action steps this process visited in history
                    string historyWhere = String.Format("HistoryStepType = {0} AND HistoryStateID = {1}", (int) WorkflowStepTypeEnum.Action, si.StateID);
                    var history = AutomationHistoryInfoProvider.GetAutomationHistories().Where(historyWhere).Column("HistoryStepID");

                    // Get action IDs of actions associated to the visited action steps
                    string stepWhere = SqlHelper.GetWhereCondition("StepID", history.Select(h => h.HistoryStepID).AsEnumerable());
                    var actionSteps = WorkflowStepInfoProvider.GetWorkflowSteps()
                        .Where(stepWhere)
                        .Columns("StepActionID");

                    string[] selectedActions = actions.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                    // Get action infos to visited actions that have selected name
                    string actionWhere = SqlHelper.GetWhereCondition("ActionID", actionSteps.Select(s => s.StepActionID).AsEnumerable());
                    actionWhere = SqlHelper.AddWhereCondition(actionWhere, SqlHelper.GetWhereCondition("ActionName", selectedActions));

                    var actionInfos = WorkflowActionInfoProvider.GetWorkflowActions().Where(actionWhere).Column("ActionName").TypedResult.Items;

                    // Return true if all/any actions were visited in history
                    return allActions ? (actionInfos.Count == selectedActions.Length) : (actionInfos.Count > 0);
                }
                else if (allActions)
                {
                    // No actions were selected
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if process had passed through one/all specified automation steps.
        /// </summary>
        /// <param name="state">Process instance to check</param>
        /// <param name="steps">Automation step names separated with a semicolon</param>
        /// <param name="allSteps">If true all specified steps must have been passed.</param>
        public static bool PassedThroughSteps(object state, string steps, bool allSteps)
        {
            AutomationStateInfo si = state as AutomationStateInfo;
            if (si != null)
            {
                if (!String.IsNullOrEmpty(steps))
                {
                    string[] stepArray = steps.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                    // Get only the step names of workflow steps visited in history that are selected by user
                    string where = String.Format("HistoryStateID  = {0} AND {1}", si.StateID, SqlHelper.GetWhereCondition("HistoryStepName", stepArray));

                    var history = AutomationHistoryInfoProvider.GetAutomationHistories().Where(where).Column("HistoryStepName").TypedResult.Items;
                    if (history.Count > 0)
                    {
                        // At least of the selected steps was found in the history
                        if (!allSteps)
                        {
                            return true;
                        }

                        // Check whether all of the selected steps are in the history
                        return stepArray.All(s => history.Any(h => h.HistoryStepName.Equals(s, StringComparison.InvariantCultureIgnoreCase)));
                    }
                }
                else if (allSteps)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
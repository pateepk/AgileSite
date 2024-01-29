using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Functions for workflow macro methods.
    /// </summary>
    public class WorkflowTransformationFunctions
    {
        #region "Public methods"

        /// <summary>
        /// Returns true if document had passed through one/all selected workflow actions.
        /// </summary>
        /// <param name="document">Document to check</param>
        /// <param name="actions">Workflow action names separated with a semicolon</param>
        /// <param name="allActions">If true all actions must have been passed.</param>
        public static bool PassedThroughActions(object document, string actions, bool allActions)
        {
            var doc = document as TreeNode;
            if (doc == null)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(actions))
            {
                // Get IDs of action steps this document visited in history
                var histories = WorkflowHistoryInfoProvider.GetWorkflowHistories()
                    .WhereEquals("StepType", (int)WorkflowStepTypeEnum.Action)
                    .WhereEquals("HistoryObjectType", TreeNode.OBJECT_TYPE)
                    .WhereEquals("HistoryObjectID", doc.DocumentID)
                    .Column("StepID");

                // Get action IDs of actions associated to the visited action steps
                var actionSteps = WorkflowStepInfoProvider.GetWorkflowSteps()
                                                          .WhereIn("StepID", histories)
                                                          .Columns("StepActionID");

                string[] selectedActions = actions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                // Get action infos to visited actions that have selected name
                var actionInfos = WorkflowActionInfoProvider.GetWorkflowActions()
                                                            .Column("ActionName")
                                                            .WhereIn("ActionID", actionSteps.Select(s => s.StepActionID).ToList())
                                                            .WhereIn("ActionName", selectedActions)
                                                            .TypedResult.Items;

                // Return true if all/any actions were visited in history
                return allActions ? actionInfos.Count == selectedActions.Length : actionInfos.Count > 0;
            }

            return allActions;
        }


        /// <summary>
        /// Returns true if document had passed through one/all specified workflow steps.
        /// </summary>
        /// <param name="document">Document to check</param>
        /// <param name="steps">Workflow step names separated with a semicolon</param>
        /// <param name="allSteps">If true all specified steps must have been passed.</param>
        public static bool PassedThroughSteps(object document, string steps, bool allSteps)
        {
            var doc = document as TreeNode;
            if (doc == null)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(steps))
            {
                string[] stepArray = steps.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                // Get only the step names of workflow steps visited in history that are selected by user
                var history = WorkflowHistoryInfoProvider.GetWorkflowHistories()
                                                         .WhereEquals("HistoryObjectID", doc.DocumentID)
                                                         .WhereEquals("HistoryObjectType", TreeNode.OBJECT_TYPE)
                                                         .WhereIn("StepName", stepArray)
                                                         .Column("StepName")
                                                         .TypedResult.Items;

                if (history.Count > 0)
                {
                    // At least of the selected steps was found in the history
                    if (!allSteps)
                    {
                        return true;
                    }

                    // Check whether all of the selected steps are in the history
                    return stepArray.All(s => history.Any<WorkflowHistoryInfo>(h => h.StepName.EqualsCSafe(s, true)));
                }
            }
            else if (allSteps)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}

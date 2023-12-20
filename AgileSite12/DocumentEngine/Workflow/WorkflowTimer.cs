using System;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides an ITask interface for the workflow timing.
    /// </summary>
    public class WorkflowTimer : ITask
    {
        /// <summary>
        /// Executes the workflow timer.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Get document ID from task data
                string[] data = task.TaskData.Split(';');
                Guid documenGuid = ValidationHelper.GetGuid(data[0], Guid.Empty);
                Guid stepGuid = ValidationHelper.GetGuid(data[1], Guid.Empty);
                if ((documenGuid == Guid.Empty) || (stepGuid == Guid.Empty) || (data.Length != 3))
                {
                    throw new Exception("[WorkflowTimer]: Missing task data.");
                }

                // Get connector GUID
                Guid connectorGuid = ValidationHelper.GetGuid(data[2], Guid.Empty);
                // Get document ID
                int documentId = TreePathUtils.GetDocumentIdByDocumentGUID(documenGuid, SiteInfoProvider.GetSiteName(task.TaskSiteID));

                // Get node to move to next step
                TreeProvider tree = new TreeProvider();
                TreeNode node = DocumentHelper.GetDocument(documentId, tree);

                // Integrity check
                WorkflowStepInfo intStep = WorkflowStepInfoProvider.GetWorkflowStepInfoByGUID(stepGuid);
                int stepId = (intStep != null) ? intStep.StepID : 0;

                if ((node != null) && (node.DocumentWorkflowStepID == stepId))
                {
                    // Do not check permissions for timeout
                    using (new WorkflowActionContext { CheckStepPermissions = false })
                    {
                        WorkflowManager wm = WorkflowManager.GetInstance(tree);

                        // Check integrity
                        WorkflowStepInfo currentStep = wm.GetStepInfo(node);
                        if ((currentStep != null) && currentStep.StepHasTimeout)
                        {
                            if (connectorGuid != Guid.Empty)
                            {
                                var where = new WhereCondition()
                                    .WhereEquals("TransitionSourcePointGUID", connectorGuid)
                                    .WhereEquals("TransitionWorkflowID", currentStep.StepWorkflowID);
                                var transitionEndStepId = WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                .TopN(1)
                                                                                .Columns("TransitionEndStepID")
                                                                                .Where(where)
                                                                                .Select(t => t.TransitionEndStepID)
                                                                                .FirstOrDefault();

                                if (transitionEndStepId > 0)
                                {
                                    var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(transitionEndStepId);
                                    wm.MoveToSpecificNextStep(node, step, null, WorkflowTransitionTypeEnum.Automatic);
                                }
                            }
                            else if (currentStep.StepIsArchived)
                            {
                                wm.ArchiveDocument(node, null, WorkflowTransitionTypeEnum.Automatic);
                            }
                            else
                            {
                                // Move to next step
                                wm.MoveToNextStep(node, null, WorkflowTransitionTypeEnum.Automatic);
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("Workflow", "Timeout", ex);

                return ex.Message;
            }
        }
    }
}
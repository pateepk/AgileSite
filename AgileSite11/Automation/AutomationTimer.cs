using System;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Scheduler;
using CMS.Base;
using CMS.Core;
using CMS.LicenseProvider;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Provides an ITask interface for the automation timing.
    /// </summary>
    public class AutomationTimer : ITask
    {
        /// <summary>
        /// Executes the automation timer.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            if (!ObjectFactory<ILicenseService>.StaticSingleton().IsFeatureAvailable(FeatureEnum.MarketingAutomation))
            {
                throw new LicenseException("The feature 'MarketingAutomation' is not supported in this edition. ");
            }

            try
            {
                LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => MoveToNextStep(task));
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("Automation", "Timeout", ex);

                return ex.Message;
            }

            return null;
        }


        [CanDisableLicenseCheck("TYfr7z8iF05Ta0jEGxAA14kNT5Ub58EIQCtSrPvUxYvQs3rOsh1LYk48d3vM+6i3c5TOJpu/FcUJoMY5TxKi0g==")]
        private static void MoveToNextStep(TaskInfo task)
        {
            // Get state ID from task data
            string[] data = task.TaskData.Split(';');

            if ((data.Length != 3))
            {
                throw new Exception("Missing task data.");
            }

            var stateGuid = ValidationHelper.GetGuid(data[0], Guid.Empty);
            var stepGuid = ValidationHelper.GetGuid(data[1], Guid.Empty);
            if ((stateGuid == Guid.Empty) || (stepGuid == Guid.Empty))
            {
                throw new Exception("Missing task data.");
            }

            // Get state object
            var stateObj = AutomationStateInfoProvider.GetAutomationStateInfo(stateGuid);
            if (stateObj == null)
            {
                throw new NullReferenceException("Missing state object.");
            }

            // Get the object to move to next step
            BaseInfo infoObj = ModuleManager.GetObject(stateObj.StateObjectType);
            if (infoObj == null)
            {
                return;
            }

            BaseInfo generalizedInfoObj = infoObj.Generalized.GetObject(stateObj.StateObjectID);

            // Integrity check
            var intStep = WorkflowStepInfoProvider.GetWorkflowStepInfoByGUID(stepGuid);
            int stepId = (intStep != null) ? intStep.StepID : 0;

            if ((generalizedInfoObj == null) || (stateObj.StateStepID != stepId))
            {
                return;
            }

            // Do not check permissions for timeout
            using (new AutomationActionContext { CheckStepPermissions = false })
            {
                var user = UserInfoProvider.GetUserInfo(CMSActionContext.CurrentUser.UserID);
                var autoMan = AutomationManager.GetInstance(user);

                // Check integrity
                var currentStep = autoMan.GetStepInfo(generalizedInfoObj, stateObj);
                if ((currentStep == null) || !currentStep.StepHasTimeout)
                {
                    return;
                }

                // Get connector GUID
                var connectorGuid = ValidationHelper.GetGuid(data[2], Guid.Empty);
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
                        autoMan.MoveToSpecificNextStep(infoObj, stateObj, step, null, WorkflowTransitionTypeEnum.Automatic);
                    }
                }
                else
                {
                    // Move to next step
                    autoMan.MoveToNextStep(generalizedInfoObj, stateObj, null, WorkflowTransitionTypeEnum.Automatic);
                }
            }
        }
    }
}
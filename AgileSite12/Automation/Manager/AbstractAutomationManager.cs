using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.WorkflowEngine;
using CMS.WorkflowEngine.Definitions;

namespace CMS.Automation
{
    /// <summary>
    /// Class for managing the marketing automation.
    /// </summary>
    public abstract class AbstractAutomationManager<InfoType> : AbstractWorkflowManager<InfoType, AutomationStateInfo, AutomationActionEnum>
        where InfoType : BaseInfo
    {
        #region "Variables"

        private UserInfo mUser;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if step permissions should be checked when the step is moved.
        /// </summary>
        public override bool CheckPermissions
        {
            get
            {
                return base.CheckPermissions && AutomationActionContext.CurrentCheckStepPermissions;
            }
            set
            {
                base.CheckPermissions = value;
            }
        }


        /// <summary>
        /// User
        /// </summary>
        public virtual UserInfo User
        {
            get
            {
                if (mUser == null)
                {
                    throw new NullReferenceException("[AutomationManager.User]: User context is not initialized.");
                }

                return mUser;
            }
            set
            {
                mUser = value;
            }
        }

        #endregion


        #region "Methods to get workflow information"

        /// <summary>
        /// Returns start step for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public WorkflowStepInfo GetFirstStep(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetFirstStepInternal(infoObj, stateObj);
        }


        /// <summary>
        /// Returns finished step for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public WorkflowStepInfo GetFinishedStep(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetFinishedStepInternal(infoObj, stateObj);
        }


        /// <summary>
        /// Returns the process for the specified object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public WorkflowInfo GetObjectProcess(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetObjectProcessInternal(infoObj, stateObj);
        }


        /// <summary>
        /// Gets step information for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public WorkflowStepInfo GetStepInfo(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetStepInfoInternal(infoObj, stateObj);
        }


        /// <summary>
        /// Returns previous step information.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public WorkflowStepInfo GetPreviousStepInfo(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetPreviousStepInfo(infoObj, stateObj, false);
        }


        /// <summary>
        /// Returns previous step information.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        private WorkflowStepInfo GetPreviousStepInfo(InfoType infoObj, AutomationStateInfo stateObj, bool markAsUsed)
        {
            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(infoObj, stateObj);

            // Object is not under automation
            if (currentStep == null)
            {
                throw new Exception("[AutomationManager.GetPreviousStepInfo]: The object '" + infoObj.Generalized.ObjectDisplayName + "' does not support automation.");
            }

            // Get previous step
            return GetPreviousStepInfoInternal(infoObj, stateObj, currentStep, markAsUsed);
        }


        /// <summary>
        /// Returns list of previous steps for current process
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public List<WorkflowStepInfo> GetPreviousSteps(InfoType infoObj, AutomationStateInfo stateObj)
        {
            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(infoObj, stateObj);

            // Object is not under workflow
            if (currentStep == null)
            {
                throw new Exception("[AutomationManager.GetPreviousSteps]: The object '" + infoObj.Generalized.ObjectDisplayName + "' does not support automation.");
            }

            // Get previous steps
            return GetPreviousStepsInternal(infoObj, stateObj, currentStep);
        }


        /// <summary>
        /// Returns list of next steps.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public List<WorkflowStepInfo> GetNextSteps(InfoType infoObj, AutomationStateInfo stateObj)
        {
            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(infoObj, stateObj);

            // Object is not under automation process
            if (currentStep == null)
            {
                throw new Exception("[AutomationManager.GetNextSteps]: The object '" + infoObj.Generalized.ObjectDisplayName + "' does not support automation.");
            }

            return GetNextStepInfoInternal(infoObj, stateObj, currentStep, User);
        }


        /// <summary>
        /// Logs specified action in the object workflow history record.
        /// </summary>
        /// <param name="settings">Log settings</param>
        public void LogProcessHistory(AutomationLogSettings settings)
        {
            WorkflowStepInfo sourceStep = settings.SourceStep;
            WorkflowStepInfo targetStep = settings.TargetStep;
            if ((sourceStep == null) || (targetStep == null))
            {
                throw new Exception("[AutomationManager.LogProcessHistory]: The source and target step must be specified!");
            }

            AutomationHistoryInfo history = new AutomationHistoryInfo();

            history.HistoryStateID = settings.StateObjectID;

            // Source step information
            history.HistoryStepID = sourceStep.StepID;
            history.HistoryStepDisplayName = sourceStep.StepDisplayName;
            history.HistoryStepName = sourceStep.StepName;
            history.HistoryStepType = sourceStep.StepType;

            // Target step information
            history.HistoryTargetStepID = targetStep.StepID;
            history.HistoryTargetStepDisplayName = targetStep.StepDisplayName;
            history.HistoryTargetStepName = targetStep.StepName;
            history.HistoryTargetStepType = targetStep.StepType;

            if (settings.User != null)
            {
                history.HistoryApprovedByUserID = settings.User.UserID;
            }

            history.HistoryApprovedWhen = settings.Time;
            history.HistoryComment = settings.Comment;
            history.HistoryWasRejected = settings.Rejected;

            // Always log automatic transition for action step
            history.HistoryTransitionType = sourceStep.StepAllowOnlyAutomaticTransitions ? WorkflowTransitionTypeEnum.Automatic : settings.TransitionType;
            history.HistoryWorkflowID = sourceStep.StepWorkflowID;

            AutomationHistoryInfoProvider.SetAutomationHistoryInfo(history);
        }

        #endregion


        #region "Methods to handle the steps transitions"

        /// <summary>
        /// Starts process on given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="processId">Process ID</param>
        /// <param name="trigger">Trigger which initiated the process</param>
        public WorkflowStepInfo StartProcess(InfoType infoObj, int processId, ObjectWorkflowTriggerInfo trigger = null)
        {
            return StartProcessInternal(infoObj, processId, trigger);
        }


        /// <summary>
        /// Starts process on given objects.
        /// </summary>
        /// <param name="objects">Object instances</param>
        /// <param name="processId">Process ID</param>
        /// <param name="trigger">Trigger which initiated the process</param>
        public void StartProcess(IEnumerable<AutomationProcessItem<InfoType>> objects, int processId, ObjectWorkflowTriggerInfo trigger = null)
        {
            StartProcessInternal(objects, processId, trigger);
        }


        /// <summary>
        /// Removes process from given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public void RemoveProcess(InfoType infoObj, AutomationStateInfo stateObj)
        {
            RemoveProcessInternal(infoObj, stateObj);
        }


        /// <summary>
        /// Moves the specified object to the next step and returns new step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        public WorkflowStepInfo MoveToNextStep(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToNextStepInternal(infoObj, stateObj, comment, transitionType);
            }
        }


        /// <summary>
        /// Moves the specified object's collection to the next step.
        /// </summary>
        /// <param name="stateObjects">Collection of automation states to move to the next step</param>
        public void MoveToNextStep(IEnumerable<AutomationStateInfo<InfoType>> stateObjects)
        {
            foreach (var state in stateObjects)
            {
                MoveToNextStep(state.StateObject, state, null, WorkflowTransitionTypeEnum.Automatic);
            }
        }


        /// <summary>
        /// Moves the specified object to the previous step and returns the step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>    
        public WorkflowStepInfo MoveToPreviousStep(InfoType infoObj, AutomationStateInfo stateObj, string comment)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToPreviousStepInternal(infoObj, stateObj, null, comment);
            }
        }


        /// <summary>
        /// Moves the specified object to the specified previous step and returns the step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>    
        public WorkflowStepInfo MoveToPreviousStep(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, string comment)
        {
            return MoveToPreviousStepInternal(infoObj, stateObj, step, comment);
        }


        /// <summary>
        /// Moves the specified object to the first step and returns the step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        public WorkflowStepInfo MoveToFirstStep(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToFirstStepInternal(infoObj, stateObj, comment, transitionType);
            }
        }


        /// <summary>
        /// Moves object directly to finished step. (Finishes the process without going through all the steps.)
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        public WorkflowStepInfo MoveToFinishedStep(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToFinishedStepInternal(infoObj, stateObj, comment, transitionType);
            }
        }


        /// <summary>
        /// Moves the specified object to the specified step and returns workflow step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        public WorkflowStepInfo MoveToSpecificStep(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToSpecificStepInternal(infoObj, stateObj, step, comment, transitionType, AutomationActionEnum.MoveToSpecificStep);
            }
        }


        /// <summary>
        /// Moves the specified object to the specified next step and returns workflow step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        public WorkflowStepInfo MoveToSpecificNextStep(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the object instance to ensure only single workflow action
            lock (infoObj.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToSpecificStepInternal(infoObj, stateObj, step, comment, transitionType, AutomationActionEnum.MoveToNextStep);
            }
        }


        /// <summary>
        /// Sets action state
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="status">Status string</param>
        public void SetActionStatus(InfoType infoObj, AutomationStateInfo stateObj, string status)
        {
            SetActionStatusInternal(infoObj, stateObj, status);
        }


        /// <summary>
        /// Gets action state
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        public string GetActionStatus(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return GetActionStatusInternal(infoObj, stateObj);
        }

        #endregion


        #region "Methods for workflow security"

        /// <summary>
        /// Returns true if given user can move given object to the previous/next step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="action">Automation action</param>
        public bool CheckStepPermissions(InfoType infoObj, AutomationStateInfo stateObj, AutomationActionEnum action)
        {
            return CheckStepPermissionsInternal(infoObj, stateObj, action);
        }


        /// <summary>
        /// Returns list of users who can move object to the next step. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        public InfoDataSet<UserInfo> GetUsersWhoCanMove(InfoType infoObj, AutomationStateInfo stateObj, SourcePoint sourcePoint, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetUsersWhoCanMoveInternal(infoObj, stateObj, sourcePoint, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns list of all the users who can move object to the next step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="assigned">Indicates if users assigned to the workflow steps should be get. Users in the generic roles are not included</param>
        /// <param name="managers">Indicates if users who have the manage workflow permission should be get</param>
        /// <param name="administrators">Indicates if users who are global administrators should be get</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        /// <returns>Returns defined role users, Global administrators and users who have the Manage processes permission for the object</returns>
        public InfoDataSet<UserInfo> GetUsersWhoCanMove(InfoType infoObj, AutomationStateInfo stateObj, SourcePoint sourcePoint, bool assigned, bool managers, bool administrators, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetUsersWhoCanMoveInternal(infoObj, stateObj, sourcePoint, assigned, managers, administrators, where, orderBy, topN, columns);
        }

        #endregion


        #region "Internal methods to get workflow information"

        /// <summary>
        /// Returns first step for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        protected virtual WorkflowStepInfo GetFirstStepInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            WorkflowInfo wi = GetObjectProcess(infoObj, stateObj);
            if (wi != null)
            {
                return WorkflowStepInfoProvider.GetFirstStep(wi.WorkflowID);
            }

            return null;
        }


        /// <summary>
        /// Returns finished step for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        protected virtual WorkflowStepInfo GetFinishedStepInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            WorkflowInfo wi = GetObjectProcess(infoObj, stateObj);
            if (wi != null)
            {
                return WorkflowStepInfoProvider.GetFinishedStep(wi.WorkflowID);
            }

            return null;
        }


        /// <summary>
        /// Returns the process for the specified object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        protected virtual WorkflowInfo GetObjectProcessInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            // Get the info by the current step ID
            if (stateObj != null)
            {
                return WorkflowInfoProvider.GetWorkflowInfo(stateObj.StateWorkflowID);
            }

            return null;
        }


        /// <summary>
        /// Gets step information for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        protected virtual WorkflowStepInfo GetStepInfoInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            if (stateObj != null)
            {
                return WorkflowStepInfoProvider.GetWorkflowStepInfo(stateObj.StateStepID);
            }

            return null;
        }


        /// <summary>
        /// Returns previous step information for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        protected override WorkflowStepInfo GetPreviousStepInfoInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, bool markAsUsed)
        {
            // If not specified do not process
            if ((infoObj == null) || (stateObj == null))
            {
                return null;
            }

            // Step doesn't allow to move to previous step
            if (!step.StepAllowReject)
            {
                return null;
            }

            // Get action to step
            AutomationHistoryInfo last = AutomationHistoryInfoProvider.GetLastApprovalActionToStep(step.StepID, stateObj.StateID);
            if (last != null)
            {
                // Manual transition, get previous step
                if (last.HistoryTransitionType == WorkflowTransitionTypeEnum.Manual)
                {
                    return GetPreviousStepInfoInternal(last, step.StepWorkflowID, markAsUsed);
                }
                // Get last manual transition
                else
                {
                    // Get approval action from first step
                    AutomationHistoryInfo lastStart = AutomationHistoryInfoProvider.GetLastApprovalActionFromFirstStep(stateObj.StateID);
                    if (lastStart == null)
                    {
                        throw new Exception("[AutomationManager.GetPreviousStepInfo]: Missing history from first step.");
                    }

                    // Last action is from first step
                    if (last.HistoryID == lastStart.HistoryID)
                    {
                        return null;
                    }

                    // Get last manual approval action
                    var histories = GetForwardHistoriesInternal(last.HistoryID, lastStart.HistoryID, stateObj.StateID, 1);
                    if (!DataHelper.DataSourceIsEmpty(histories))
                    {
                        var history = histories.Items[0];

                        return GetPreviousStepInfoInternal(history, step.StepWorkflowID, markAsUsed);
                    }
                }
            }
            // There is no history
            else
            {
                // Try to find previous step
                var trans = GetStepInboundTransitionsInternal(step, "TransitionType=" + (int)WorkflowTransitionTypeEnum.Manual);
                // Only one possible path
                if (trans.Count == 1)
                {
                    // Return step
                    return WorkflowStepInfoProvider.GetWorkflowStepInfo(trans[0].TransitionStartStepID);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns list of previous steps for current cycle
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        protected override List<WorkflowStepInfo> GetPreviousStepsInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step)
        {
            List<WorkflowStepInfo> steps = new List<WorkflowStepInfo>();

            // If not specified do not process
            if ((infoObj == null) || (stateObj == null))
            {
                return steps;
            }

            // Step doesn't allow to move to previous step
            if (!step.StepAllowReject)
            {
                return steps;
            }

            // Get action to step
            AutomationHistoryInfo last = AutomationHistoryInfoProvider.GetLastApprovalActionToStep(step.StepID, stateObj.StateID);
            if (last != null)
            {
                // Get action from first step
                AutomationHistoryInfo lastStart = AutomationHistoryInfoProvider.GetLastApprovalActionFromFirstStep(stateObj.StateID);
                if (lastStart == null)
                {
                    return steps;
                }

                // Last action is from first step
                if (last.HistoryID == lastStart.HistoryID)
                {
                    return steps;
                }

                if (last.HistoryTransitionType == WorkflowTransitionTypeEnum.Manual)
                {
                    steps.Add(GetPreviousStepInfoInternal(last, step.StepWorkflowID, false));
                }

                // Get manual actions
                var histories = GetForwardHistoriesInternal(last.HistoryID, lastStart.HistoryID, stateObj.StateID, 0);
                if (!DataHelper.DataSourceIsEmpty(histories))
                {
                    foreach (var history in histories.Items)
                    {
                        steps.Add(GetPreviousStepInfoInternal(history, step.StepWorkflowID, false));
                    }
                }
            }
            // There is no workflow history
            else
            {
                // Try to find previous step
                var trans = GetStepInboundTransitionsInternal(step, "TransitionType=" + (int)WorkflowTransitionTypeEnum.Manual);
                // Only one possible path
                if (trans.Count == 1)
                {
                    // Return step
                    steps.Add(WorkflowStepInfoProvider.GetWorkflowStepInfo(trans[0].TransitionStartStepID));
                }
            }

            return steps;
        }


        /// <summary>
        /// Gets previous workflow step
        /// </summary>
        /// <param name="history">History</param>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        private WorkflowStepInfo GetPreviousStepInfoInternal(AutomationHistoryInfo history, int workflowId, bool markAsUsed)
        {
            // Get step
            int stepId = history.HistoryStepID;
            int workfId = (history.HistoryWorkflowID > 0) ? history.HistoryWorkflowID : workflowId;
            WorkflowStepInfo step = stepId > 0 ? WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId) : WorkflowStepInfoProvider.GetFirstStep(workfId);

            // Update history
            if (markAsUsed)
            {
                using (CMSActionContext ctx = new CMSActionContext())
                {
                    ctx.DisableAll();

                    history.HistoryRejected = true;
                    history.Update();
                }
            }

            // Keep related history ID
            WorkflowStepInfo clone = step.Clone();
            clone.RelatedHistoryID = history.HistoryID;

            return clone;
        }


        /// <summary>
        /// Gets histories between two given for specified object
        /// </summary>
        /// <param name="startHistoryId">Start history ID</param>
        /// <param name="endHistoryId">End history ID</param>
        /// <param name="stateId">State object ID</param>
        /// <param name="topN">Top N results</param>
        private InfoDataSet<AutomationHistoryInfo> GetForwardHistoriesInternal(int startHistoryId, int endHistoryId, int stateId, int topN)
        {
            // Prepare where condition
            StringBuilder sb = new StringBuilder();
            sb.Append("HistoryID < ", startHistoryId, " AND HistoryID > ", endHistoryId, " AND (HistoryTransitionType IS NULL OR HistoryTransitionType = 0) AND HistoryStateID = ", stateId, " AND HistoryWasRejected = 0 AND (HistoryRejected IS NULL OR HistoryRejected = 0)");

            // Get actions
            return AutomationHistoryInfoProvider.GetAutomationHistories().Where(sb.ToString()).OrderByDescending("HistoryID").TopN(topN).TypedResult;
        }


        /// <summary>
        /// Returns list of next steps for given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected override List<WorkflowStepInfo> GetNextStepInfoInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, UserInfo user)
        {
            // Prepare resolver
            MacroResolver resolver = GetEvalResolverInternal(infoObj, stateObj, WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID), step, user);
            List<WorkflowStepInfo> steps = new List<WorkflowStepInfo>();

            // Evaluate transitions
            List<WorkflowTransitionInfo> winTransitions = EvaluateTransitions(step, user, infoObj.Generalized.ObjectSiteID, resolver);

            // Get next steps
            foreach (var transition in winTransitions)
            {
                WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(transition.TransitionEndStepID);

                // Keep information about related transition
                WorkflowStepInfo clone = s.Clone();
                clone.RelatedTransition = transition.Clone();
                steps.Add(clone);
            }

            return steps;
        }

        #endregion


        #region "Internal methods to handle the steps transitions"

        /// <summary>
        /// Starts process on given object.
        /// </summary>
        /// <param name="infoObjects">Object instances</param>
        /// <param name="processId">Process ID</param>
        /// <param name="trigger">Trigger which initiated the process</param>
        protected virtual void StartProcessInternal(IEnumerable<AutomationProcessItem<InfoType>> infoObjects, int processId, ObjectWorkflowTriggerInfo trigger)
        {
            if (infoObjects == null)
            {
                throw new ArgumentNullException(nameof(infoObjects), "Missing object instances.");
            }

            // Get process info
            var wi = GetProcessWorkflow(processId);

            // Get first step
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetFirstStep(wi.WorkflowID);
            if (step == null)
            {
                return;
            }

            var states = infoObjects.Select(config => GetNewAutomationState(config.InfoObject, step, wi, trigger, config.AdditionalData));

            // Validate recurrence and filter out states which should not be started again
            var filteredStates = states.Where(state =>
            {
                try
                {
                    CheckRecurrenceInternal(state.StateObject, state, wi.WorkflowRecurrenceType);
                }
                catch (ProcessRecurrenceException)
                {
                    return false;
                }

                return true;

            }).ToList();

            AutomationStateInfoProvider.BulkInsertAutomationState(filteredStates);

            EnsurePrimaryKeys(filteredStates);

            MoveToNextStep(filteredStates);
        }


        private AutomationStateInfo<InfoType> GetNewAutomationState(InfoType objectToProcess, WorkflowStepInfo step, WorkflowInfo wi, ObjectWorkflowTriggerInfo trigger, StringSafeDictionary<object> additionalData = null)
        {
            AutomationStateInfo<InfoType> state = new AutomationStateInfo<InfoType>
            {
                StateObject = objectToProcess,
                StateObjectID = objectToProcess.Generalized.ObjectID,
                StateObjectType = objectToProcess.TypeInfo.ObjectType,
                StateStepID = step.StepID,
                StateWorkflowID = wi.WorkflowID,
                StateStatus = ProcessStatusEnum.Processing,
                StateSiteID = objectToProcess.Generalized.ObjectSiteID
            };

            if (trigger == null)
            {
                // Manually started process
                state.StateUserID = User.UserID;
            }
            else
            {
                // Set activity into state to access this activity in automation steps
                if (additionalData != null)
                {
                    TrySetStateCustomData(TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEMID, additionalData, state.StateCustomData);
                    TrySetStateCustomData(TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEM_DETAILID, additionalData, state.StateCustomData);
                    TrySetStateCustomData(TriggerDataConstants.TRIGGER_DATA_ACTIVITY_VALUE, additionalData, state.StateCustomData);
                    TrySetStateCustomData(TriggerDataConstants.TRIGGER_DATA_ACTIVITY_SITEID, additionalData, state.StateCustomData);
                }
            }

            return state;
        }


        private static void EnsurePrimaryKeys(List<AutomationStateInfo<InfoType>> states)
        {
            var guids = states.Select(t => t.StateGUID).ToList();

            // Get inserted primary keys
            var idsTable = AutomationStateInfoProvider.GetAutomationStates()
                                                      .Columns("StateID", "StateGUID")
                                                      .WhereIn("StateGUID", guids)
                                                      .Select(state => new
                                                      {
                                                          state.StateID,
                                                          state.StateGUID
                                                      });

            states.ForEach(state =>
            {
                var id = idsTable.First(item => item.StateGUID == state.StateGUID);
                state.StateID = id.StateID;
            });
        }

        
        /// <summary>
        /// Starts process on given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="processId">Process ID</param>
        /// <param name="trigger">Trigger which initiated the process</param>
        protected virtual WorkflowStepInfo StartProcessInternal(InfoType infoObj, int processId, ObjectWorkflowTriggerInfo trigger)
        {
            if (infoObj == null)
            {
                throw new ArgumentNullException(nameof(infoObj), "Missing object instance.");
            }

            // Get process info
            var wi = GetProcessWorkflow(processId);

            // Get first step
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetFirstStep(wi.WorkflowID);
            if (step == null)
            {
                return null;
            }

            var state = GetNewAutomationState(infoObj, step, wi, trigger);
            
            // Checks recurrence
            CheckRecurrenceInternal(infoObj, state, wi.WorkflowRecurrenceType);

            state.Insert();

            // Initiate the process
            step = MoveToNextStep(infoObj, state, null, WorkflowTransitionTypeEnum.Automatic);

            return step;
        }


        private static WorkflowInfo GetProcessWorkflow(int processId)
        {
            var wi = WorkflowInfoProvider.GetWorkflowInfo(processId);
            if ((wi == null) || (wi.WorkflowType != WorkflowTypeEnum.Automation))
            {
                throw new InvalidOperationException("Missing automation process or wrong type.");
            }

            if (!wi.WorkflowEnabled)
            {
                throw new ProcessDisabledException("Cannot start disabled process '" + wi.WorkflowDisplayName + "'.");
            }

            return wi;
        }


        /// <summary>
        /// Removes process from given object.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        protected virtual void RemoveProcessInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            if (stateObj != null)
            {
                AutomationStateInfoProvider.DeleteAutomationStateInfo(stateObj);
            }
        }


        /// <summary>
        /// Check automation process recurrence settings
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="type">Recurrence type</param>
        protected virtual void CheckRecurrenceInternal(InfoType infoObj, AutomationStateInfo stateObj, ProcessRecurrenceTypeEnum type)
        {
            switch (type)
            {
                // When there is already finished or started process for given object, process is not started
                case ProcessRecurrenceTypeEnum.NonRecurring:
                    {
                        ProcessInstanceStatusEnum status = AutomationStateInfoProvider.GetProcessInstanceStatus(stateObj);
                        if (status != ProcessInstanceStatusEnum.None)
                        {
                            string name = HTMLHelper.HTMLEncode(infoObj.TypeInfo.GetNiceObjectTypeName().ToLowerInvariant());
                            throw new ProcessRecurrenceException(String.Format(ResHelper.GetAPIString("ma.NotRecurringError", "The automation process has been already initiated for '{0}' {1}."), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(infoObj.Generalized.ObjectDisplayName)), name));
                        }
                    }
                    break;

                // When there is an active process started for given object, process is not started
                case ProcessRecurrenceTypeEnum.NonConcurrentRecurring:
                    {
                        ProcessInstanceStatusEnum status = AutomationStateInfoProvider.GetProcessInstanceStatus(stateObj);
                        if (status == ProcessInstanceStatusEnum.Running)
                        {
                            string name = HTMLHelper.HTMLEncode(infoObj.TypeInfo.GetNiceObjectTypeName().ToLowerInvariant());
                            throw new ProcessRecurrenceException(String.Format(ResHelper.GetAPIString("ma.SingletonError", "There is already running automation process for '{0}' {1}."), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(infoObj.Generalized.ObjectDisplayName)), name));
                        }
                    }
                    break;

                // Process can start anytime
                case ProcessRecurrenceTypeEnum.Recurring:
                    break;
            }
        }


        /// <summary>
        /// Moves the specified object to the next step and returns new step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        protected virtual WorkflowStepInfo MoveToNextStepInternal(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            // Get current step info
            WorkflowStepInfo currentStep = null;
            if (stateObj != null)
            {
                currentStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stateObj.StateStepID);
            }

            // Check if there is next step available
            if ((currentStep == null) || currentStep.StepIsFinished)
            {
                return currentStep;
            }

            // Get next steps
            List<WorkflowStepInfo> nextSteps = GetNextSteps(infoObj, stateObj);

            // If there is no next step or multiple available steps, do not move
            if (nextSteps.Count != 1)
            {
                // No next step, return current
                return currentStep;
            }

            // Next step
            WorkflowStepInfo nextStep = nextSteps[0];

            // Check permissions
            if (!CheckStepPermissions(infoObj, stateObj, AutomationActionEnum.MoveToNextStep))
            {
                throw new PermissionException("[AutomationManager.MoveToNextStepInternal]: User '" + User.GetFormattedUserName(true) + "' is not authorized to move object to the next step.");
            }

            // Move to specified step
            WorkflowStepInfo newStep = MoveToStepInternal(infoObj, stateObj, currentStep, nextStep, comment, transitionType, true);

            return MoveOnInternal(infoObj, stateObj, newStep);
        }


        /// <summary>
        /// Moves the specified object to the previous step and returns the new step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>    
        protected virtual WorkflowStepInfo MoveToPreviousStepInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, string comment)
        {
            // Get current step info
            WorkflowStepInfo currentStep = null;
            if (stateObj != null)
            {
                currentStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stateObj.StateStepID);
            }

            // Move to previous step is not allowed for start step
            if ((currentStep == null) || currentStep.StepIsStart)
            {
                return currentStep;
            }

            // Check permissions
            if (!CheckStepPermissions(infoObj, stateObj, AutomationActionEnum.MoveToPreviousStep))
            {
                throw new PermissionException("[AutomationManager.MoveToPreviousStepInternal]: User '" + User.GetFormattedUserName(true) + "' is not authorized to move object to previous step.");
            }

            WorkflowStepInfo previousStep = step;

            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Handle the event
                using (var h = AutomationEvents.MoveToPreviousStep.StartEvent(infoObj, stateObj))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Get previous step if not specified
                        if (previousStep == null)
                        {
                            previousStep = GetPreviousStepInfo(infoObj, stateObj, true);
                        }
                        else
                        {
                            // Mark the previous path as used
                            MarkReverseHistoriesInternal(previousStep, currentStep, stateObj.StateID);
                        }

                        if (previousStep == null)
                        {
                            // If no previous step, return current step
                            return currentStep;
                        }

                        // Update status
                        stateObj.StateStepID = previousStep.StepID;
                        stateObj.StateStatus = ProcessStatusEnum.Pending;
                        stateObj.Update();

                        // Prepare log settings
                        AutomationLogSettings settings = new AutomationLogSettings(infoObj.TypeInfo.ObjectType, infoObj.Generalized.ObjectID, stateObj.StateID)
                        {
                            Comment = comment,
                            User = User,
                            SourceStep = currentStep,
                            TargetStep = previousStep,
                            Rejected = true
                        };

                        // Log the history
                        LogProcessHistory(settings);
                    }

                    // Finalize the event
                    h.EventArguments.PreviousStep = currentStep;
                    h.FinishEvent();
                }

                // Commit the transaction
                tr.Commit();

                // Insert information about this event to event log.
                if (ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogMATransitions"], true))
                {
                    string objectName = HTMLHelper.HTMLEncode(infoObj.Generalized.ObjectDisplayName);
                    string stepName = HTMLHelper.HTMLEncode(previousStep.StepDisplayName);
                    string objectType = ResHelper.GetString(TypeHelper.GetObjectTypeResourceKey(stateObj.StateObjectType)).ToLowerInvariant();
                    string message = String.Format(ResHelper.GetAPIString("ma.movedtopreviousstepevent", "The {0} '{1}' has been moved to previous '{2}' step."), objectType, objectName, stepName);
                    LogContext.LogEventToCurrent(EventType.INFORMATION, EventLogSource, "MOVETOPREVIOUSSTEP", message, RequestContext.RawURL, User.UserID, User.UserName, 0, null, RequestContext.UserHostAddress, infoObj.Generalized.ObjectSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
                }
            }

            // Get workflow info of the current step
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(currentStep.StepWorkflowID);

            // Process step action
            return HandleStepInternal(infoObj, stateObj, User, workflow, currentStep, previousStep, null, false);
        }


        /// <summary>
        /// Marks workflow histories as used when moving to previous specific step.
        /// </summary>
        /// <param name="previousStep">Previous workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="stateId">State object ID</param>
        private void MarkReverseHistoriesInternal(WorkflowStepInfo previousStep, WorkflowStepInfo currentStep, int stateId)
        {
            // Get approval action to step
            AutomationHistoryInfo last = AutomationHistoryInfoProvider.GetLastApprovalActionToStep(currentStep.StepID, stateId);
            if (last != null)
            {
                AutomationHistoryInfoProvider.MarkRejected(last.HistoryID, previousStep.RelatedHistoryID, stateId);
            }
        }


        /// <summary>
        /// Moves the specified object to the first step and returns the step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        protected virtual WorkflowStepInfo MoveToFirstStepInternal(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            // Get workflow
            WorkflowInfo workflow = GetObjectProcess(infoObj, stateObj);

            if (workflow != null)
            {
                // Get first step
                WorkflowStepInfo firstStep = WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID);
                return MoveToSpecificStepInternal(infoObj, stateObj, firstStep, comment, transitionType, AutomationActionEnum.MoveToNextStep);
            }

            return null;
        }


        /// <summary>
        /// Moves the specified object to the specified step and returns workflow step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        /// <param name="action">Action context</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        protected override WorkflowStepInfo MoveToSpecificStepInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, AutomationActionEnum action)
        {
            // Get default action context
            if (action == AutomationActionEnum.Unknown)
            {
                action = AutomationActionEnum.MoveToNextStep;
            }

            // Get current step info
            WorkflowStepInfo currentStep = null;
            if (stateObj != null)
            {
                currentStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stateObj.StateStepID);
            }

            // If no step given, do not move to the step and finish
            if ((step == null) || (currentStep == null))
            {
                return currentStep;
            }

            // Check permissions
            if (!CheckStepPermissions(infoObj, stateObj, action))
            {
                throw new PermissionException("[AutomationManager.MoveToSpecificStepInternal]: User '" + User.GetFormattedUserName(true) + "' is not authorized to move object to the next step.");
            }

            // Move to specified step
            step = MoveToStepInternal(infoObj, stateObj, currentStep, step, comment, transitionType, true);

            return MoveOnInternal(infoObj, stateObj, step);
        }


        /// <summary>
        /// Moves the specified object to the specified step and returns the step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="currentStep">Current workflow step of the object</param>
        /// <param name="step">Target workflow step of the object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        /// <param name="handleActions">Indicates if step actions should be handled</param>
        protected override WorkflowStepInfo MoveToStepInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, bool handleActions)
        {
            // If no step given, return current step
            if (step == null)
            {
                return currentStep;
            }

            // Get workflow info
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.MarketingAutomation);
            }

            string objectType = infoObj.TypeInfo.ObjectType;
            int objectId = infoObj.Generalized.ObjectID;

            // Move to start step
            if (step.StepIsStart)
            {
                #region "Move to start step"

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Update status
                    stateObj.StateStepID = step.StepID;
                    stateObj.StateStatus = ProcessStatusEnum.Processing;
                    stateObj.Update();

                    // Log the history if the current step is not finished
                    if (!currentStep.StepIsFinished)
                    {
                        // Prepare log settings
                        AutomationLogSettings settings = new AutomationLogSettings(objectType, objectId, stateObj.StateID)
                        {
                            Comment = comment,
                            User = User,
                            SourceStep = currentStep,
                            TargetStep = step,
                            TransitionType = transitionType
                        };

                        // Log the history
                        LogProcessHistory(settings);
                    }

                    // Commit the transaction
                    tr.Commit();
                }

                #endregion
            }
            else
            {
                #region "Standard step"

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Handle the event
                    using (var h = AutomationEvents.MoveToNextStep.StartEvent(infoObj, stateObj))
                    {
                        h.DontSupportCancel();

                        if (h.CanContinue())
                        {
                            // Update status
                            stateObj.StateStepID = step.StepID;
                            stateObj.StateStatus = step.StepIsFinished ? ProcessStatusEnum.Finished : ProcessStatusEnum.Processing;
                            stateObj.Update();

                            // Prepare log settings
                            AutomationLogSettings settings = new AutomationLogSettings(objectType, objectId, stateObj.StateID)
                            {
                                Comment = comment,
                                User = User,
                                SourceStep = currentStep,
                                TargetStep = step,
                                TransitionType = transitionType
                            };

                            // Log the history
                            LogProcessHistory(settings);
                        }

                        // Finalize the event
                        h.EventArguments.PreviousStep = currentStep;
                        h.FinishEvent();
                    }

                    // Commit the transaction
                    tr.Commit();

                    // Log event to the EventLog provider
                    if (ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogMATransitions"], true))
                    {
                        string objectName = HTMLHelper.HTMLEncode(infoObj.Generalized.ObjectDisplayName);
                        string stepName = HTMLHelper.HTMLEncode(step.StepDisplayName);
                        string objectTypeName = ResHelper.GetString(TypeHelper.GetObjectTypeResourceKey(stateObj.StateObjectType)).ToLowerInvariant();
                        string eventText = String.Format(ResHelper.GetAPIString("ma.movedtonextstepevent", "The {1} '{0}' has been moved to next '{2}' step."), objectTypeName, objectName, stepName);
                        LogContext.LogEventToCurrent(EventType.INFORMATION, EventLogSource, "MOVETONEXTSTEP", eventText, RequestContext.RawURL, User.UserID, User.UserName, 0, null, RequestContext.UserHostAddress, infoObj.Generalized.ObjectSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
                    }
                }

                #endregion
            }

            // Process step action
            return HandleStepInternal(infoObj, stateObj, User, workflow, currentStep, step, null, handleActions);
        }


        /// <summary>
        /// Moves the specified object to the first step without automatic transition and returns the final step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="currentStep">Current workflow step of the object</param>
        /// <param name="comment">Action comment</param>
        /// <remarks>The return step can be different than the original target step.</remarks>
        protected virtual WorkflowStepInfo MoveStepInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo currentStep, string comment)
        {
            return base.MoveStepInternal(infoObj, stateObj, currentStep, User, null);
        }

        #endregion


        #region "Internal methods for advanced object actions"

        /// <summary>
        /// Moves object directly to finished step. (Finishes the process without going through all the steps.)
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition (Use Manual when the action is performed by the user, not the process.)</param>
        protected virtual WorkflowStepInfo MoveToFinishedStepInternal(InfoType infoObj, AutomationStateInfo stateObj, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            // Get workflow
            WorkflowInfo workflow = GetObjectProcess(infoObj, stateObj);

            if (workflow == null)
            {
                return null;
            }

            // Get finished step and move object to this step
            WorkflowStepInfo finishedStep = WorkflowStepInfoProvider.GetFinishedStep(workflow.WorkflowID);
            return MoveToSpecificStepInternal(infoObj, stateObj, finishedStep, comment, transitionType, AutomationActionEnum.MoveToNextStep);
        }

        #endregion


        #region "Internal methods for workflow security"

        /// <summary>
        /// Returns true if given user can move given object to previous/next step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="action">Automation action</param>
        protected virtual bool CheckStepPermissionsInternal(InfoType infoObj, AutomationStateInfo stateObj, AutomationActionEnum action)
        {
            if (!CheckPermissions)
            {
                // Permissions are not checked
                return true;
            }

            // If object not specified
            if ((infoObj == null) || (stateObj == null))
            {
                return false;
            }

            // Get site name 
            string siteName = infoObj.Generalized.ObjectSiteName;

            // Special permission for 'Move to specific step' action
            if ((action == AutomationActionEnum.MoveToSpecificStep))
            {
                return WorkflowStepInfoProvider.CanUserMoveToSpecificAutomationStep(User, siteName);
            }

            // Test global permissions
            if (WorkflowStepInfoProvider.CanUserManageAutomationProcesses(User, siteName))
            {
                return true;
            }

            int stepId = stateObj.StateStepID;
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId) ?? GetFirstStep(infoObj, stateObj);
            if (step == null)
            {
                throw new Exception("[AutomationManager.CheckStepPermissionsInternal]: Given step doesn't exist!");
            }

            // Special treatment for start step
            if (step.StepIsStart)
            {
                return true;
            }

            // Special treatment for action step
            if (step.StepIsAction)
            {
                return false;
            }

            // The workflow action is not passed, its for customization purpose
            return WorkflowStepInfoProvider.CanUserApprove(User, step, infoObj.Generalized.ObjectSiteID);
        }


        /// <summary>
        /// Returns list of users who can move object to the next step. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        protected virtual InfoDataSet<UserInfo> GetUsersWhoCanMoveInternal(InfoType infoObj, AutomationStateInfo stateObj, SourcePoint sourcePoint, string where, string orderBy, int topN, string columns)
        {
            // If not specified do not process
            if ((infoObj == null) || (stateObj == null))
            {
                return null;
            }

            // Get step
            int stepId = stateObj.StateStepID;
            WorkflowStepInfo step = null;
            if (stepId > 0)
            {
                step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
            }

            return WorkflowStepInfoProvider.GetUsersWhoCanApprove(step, sourcePoint, infoObj.Generalized.ObjectSiteID, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns list of all the users who can move object to the next step.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="assigned">Indicates if users assigned to the workflow steps should be get. Users in the generic roles are not included</param>
        /// <param name="managers">Indicates if users who have the manage permission should be get</param>
        /// <param name="administrators">Indicates if users who are global administrators should be get</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        /// <returns>Returns defined role users, Global administrators and users who have the Manage processes permission for the object</returns>
        protected virtual InfoDataSet<UserInfo> GetUsersWhoCanMoveInternal(InfoType infoObj, AutomationStateInfo stateObj, SourcePoint sourcePoint, bool assigned, bool managers, bool administrators, string where, string orderBy, int topN, string columns)
        {
            // Role users
            InfoDataSet<UserInfo> result = null;

            // Ensure union column
            columns = SqlHelper.MergeColumns(columns, "UserID");

            // Assigned users
            if (assigned)
            {
                result = GetUsersWhoCanMove(infoObj, stateObj, sourcePoint, where, orderBy, topN, columns);
            }

            // Manage permission
            if (managers)
            {
                InfoDataSet<UserInfo> manageDS = UserInfoProvider.GetRequiredResourceUsers(ModuleName.ONLINEMARKETING, "ManageProcesses", infoObj.Generalized.ObjectSiteName, where, orderBy, topN, columns);
                result = (InfoDataSet<UserInfo>)DataHelper.Union(result, manageDS, "UserID");
            }

            // Global administrators
            if (administrators)
            {
                var adminUsersCondition = new WhereCondition()
                    .WhereIn("UserPrivilageLevel", new[] { (int)UserPrivilegeLevelEnum.Admin, (int)UserPrivilegeLevelEnum.GlobalAdmin })
                    .And(new WhereCondition(where));

                var query = UserInfoProvider.GetUsersDataWithSettings().Where(adminUsersCondition).TopN(topN).Columns(columns);

                if (!string.IsNullOrEmpty(orderBy))
                {
                    string direction;
                    var column = SqlHelper.GetOrderByColumnName(orderBy, out direction);

                    query = SqlHelper.ORDERBY_DESC.Equals(direction, StringComparison.OrdinalIgnoreCase)
                        ? query.OrderByDescending(column)
                        : query.OrderByAscending(column);
                }

                InfoDataSet<UserInfo> adminsDS = query.TypedResult;
                result = (InfoDataSet<UserInfo>)DataHelper.Union(result, adminsDS, "UserID");
            }

            return result;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets action state
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="status">Status string</param>
        protected override void SetActionStatusInternal(InfoType infoObj, AutomationStateInfo stateObj, string status)
        {
            if (stateObj.StateActionStatus != status)
            {
                // Set processing status
                var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stateObj.StateStepID);
                var pendingStatus = step.StepIsFinished ? ProcessStatusEnum.Finished : ProcessStatusEnum.Pending;
                stateObj.StateStatus = (status == WorkflowHelper.ACTION_SATUS_RUNNING) ? ProcessStatusEnum.Processing : pendingStatus;

                // Set action status
                stateObj.StateActionStatus = status;
                using (CMSActionContext ctx = new CMSActionContext())
                {
                    ctx.DisableAll();
                    stateObj.Update();
                }
            }
        }


        /// <summary>
        /// Gets action state
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        protected override string GetActionStatusInternal(InfoType infoObj, AutomationStateInfo stateObj)
        {
            return stateObj?.StateActionStatus;
        }


        /// <summary>
        /// Gets resolver for evaluation of transitions and source points
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected override MacroResolver GetEvalResolverInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowInfo workflow, WorkflowStepInfo step, UserInfo user)
        {
            MacroResolver resolver = GetBasicResolverInternal(workflow, step, user);
            if (infoObj != null)
            {
                resolver.SetAnonymousSourceData(infoObj);

                AddDefaultSourceData(resolver, infoObj, stateObj);
            }

            return resolver;
        }


        /// <summary>
        /// Get resolver for e-mail sending.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="user">User</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="action">Automation action string representation</param>
        /// <param name="comment">Action comment</param>
        public override MacroResolver GetEmailResolver(InfoType infoObj, AutomationStateInfo stateObj, UserInfo user, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowInfo workflow, string action, string comment)
        {
            return GetEmailResolverInternal(infoObj, stateObj, user, originalStep, currentStep, workflow, action, comment);
        }


        /// <summary>
        /// Get resolver for e-mail sending.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="action">Workflow action string representation</param>
        /// <param name="comment">Action comment</param>
        protected virtual MacroResolver GetEmailResolverInternal(InfoType infoObj, AutomationStateInfo stateObj, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowInfo workflow, string action, string comment)
        {
            // Prepare the macro resolver
            MacroResolver resolver = GetBasicResolverInternal(workflow, currentStep, userInfo);

            // Add named sources
            if (infoObj != null)
            {
                AddDefaultSourceData(resolver, infoObj, stateObj);
                resolver.SetNamedSourceData("OriginalStep", originalStep);
            }

            return resolver;
        }


        private static void AddDefaultSourceData(MacroResolver resolver, InfoType infoObj, AutomationStateInfo stateObj)
        {
            resolver.SetNamedSourceData("Object", infoObj);
            string objectName = TypeHelper.GetNiceName(infoObj.TypeInfo.ObjectType);
            resolver.SetNamedSourceData(objectName, infoObj);
            resolver.SetNamedSourceData("AutomationState", stateObj);
        }


        /// <summary>
        /// Processes action connected to given step.
        /// </summary>
        /// <param name="arguments">Action arguments</param>
        protected override void ProcessActionInternal(WorkflowActionEventArgs<InfoType, AutomationStateInfo, AutomationActionEnum> arguments)
        {
            // Handle the event
            using (var h = AutomationEvents.Action.StartEvent(arguments as WorkflowActionEventArgs<BaseInfo, AutomationStateInfo, AutomationActionEnum>))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    base.ProcessActionInternal(arguments);
                }

                // Finalize the event
                h.FinishEvent();
            }
        }


        private WorkflowStepInfo MoveOnInternal(InfoType infoObj, AutomationStateInfo stateObj, WorkflowStepInfo step)
        {
            // Move to step without automatic transitions
            var targetStep = MoveStepInternal(infoObj, stateObj, step, null);

            // Update state info
            stateObj.StateStatus = targetStep.StepIsFinished ? ProcessStatusEnum.Finished : ProcessStatusEnum.Pending;
            stateObj.Update();

            return targetStep;
        }


        /// <summary>
        /// Copy data under specific key from triggerData to stateData. Null items or already existing ones will be ignored.
        /// </summary>
        /// <param name="key">Specific key</param>
        /// <param name="triggerData">Trigger data</param>
        /// <param name="stateData">State data</param>
        private void TrySetStateCustomData(string key, StringSafeDictionary<object> triggerData, ContainerCustomData stateData)
        {
            var data = triggerData[key];

            if ((data != null) && (!stateData.ContainsColumn(key)))
            {
                stateData[key] = data;
            }
        }

        #endregion
    }
}
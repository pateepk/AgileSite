using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.WorkflowEngine.Definitions;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Abstract class for managing the workflow procedure.
    /// </summary>
    public abstract class AbstractWorkflowManager<InfoType, StateInfoType, ActionEnumType> : AbstractManager
        where InfoType : BaseInfo
        where StateInfoType : BaseInfo
        where ActionEnumType : struct, IConvertible
    {
        #region "Variables"

        /// <summary>
        /// Event log source name.
        /// </summary>
        private string mEventLogSource = "Workflow";

        /// <summary>
        /// Inner MacroResolver object.
        /// </summary>
        private MacroResolver mMacroResolver;

        /// <summary>
        /// Indicates if notification e-mails should be sent.
        /// </summary>
        private bool mSendEmails = true;

        /// <summary>
        /// Indicates if step permissions should be checked when the step is moved.
        /// </summary>
        private bool mCheckStepPermissions = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if step permissions should be checked when the step is moved.
        /// </summary>
        public virtual bool CheckPermissions
        {
            get
            {
                return mCheckStepPermissions;
            }
            set
            {
                mCheckStepPermissions = value;
            }
        }


        /// <summary>
        /// Event log source name.
        /// </summary>
        public virtual string EventLogSource
        {
            get
            {
                return mEventLogSource;
            }
            set
            {
                mEventLogSource = value;
            }
        }


        /// <summary>
        /// Macro resolver instance.
        /// </summary>
        public virtual MacroResolver MacroResolver
        {
            get
            {
                if (mMacroResolver == null)
                {
                    mMacroResolver = MacroResolver.GetInstance();
                    mMacroResolver.Settings.EncodeResolvedValues = true;
                }
                return mMacroResolver;
            }
            set
            {
                mMacroResolver = value;
            }
        }


        /// <summary>
        /// Indicates if notification e-mails should be sent
        /// </summary>
        public virtual bool SendEmails
        {
            get
            {
                return mSendEmails && CMSActionContext.CurrentSendEmails;
            }
            set
            {
                mSendEmails = value;
            }
        }


        /// <summary>
        /// Returns whether the e-mails should be sent to the current moderator (reflects the CMSWorkflowSendEmailToModerator web.config settings).
        /// </summary>
        public static bool SendEmailToModerator
        {
            get
            {
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWorkflowSendEmailToModerator"], false);
            }
        }


        /// <summary>
        /// Application URL for macro resolver
        /// </summary>
        public string ApplicationUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Number of currently processed hops
        /// </summary>
        protected int CurrentHops
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Register generic type because of application reset.
        /// </summary>
        static AbstractWorkflowManager()
        {
            TypeManager.RegisterGenericType(typeof(AbstractWorkflowManager<InfoType, StateInfoType, ActionEnumType>));
        }

        #endregion


        #region "Public abstract methods"

        /// <summary>
        /// Get resolver for e-mail sending.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="action">Workflow action string representation</param>
        /// <param name="comment">Action comment</param>
        public abstract MacroResolver GetEmailResolver(InfoType infoObj, StateInfoType stateObj, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowInfo workflow, string action, string comment);

        #endregion


        #region "Protected abstract methods"

        /// <summary>
        /// Sets action status
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="status">Status string</param>
        protected abstract void SetActionStatusInternal(InfoType infoObj, StateInfoType stateObj, string status);


        /// <summary>
        /// Gets action status
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        protected abstract string GetActionStatusInternal(InfoType infoObj, StateInfoType stateObj);


        /// <summary>
        /// Gets resolver for evaluation of transitions and source points
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected abstract MacroResolver GetEvalResolverInternal(InfoType infoObj, StateInfoType stateObj, WorkflowInfo workflow, WorkflowStepInfo step, UserInfo user);


        /// <summary>
        /// Moves the specified object to the specified step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified object moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="action">Action context</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        protected abstract WorkflowStepInfo MoveToSpecificStepInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, ActionEnumType action);


        /// <summary>
        /// Moves the specified object to the specified step in the workflow and returns the step.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="currentStep">Current workflow step of the object</param>
        /// <param name="step">Target workflow step of the object</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="handleActions">Indicates if step actions should be handled</param>
        protected abstract WorkflowStepInfo MoveToStepInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, bool handleActions);


        /// <summary>
        /// Returns list of next steps for given object.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        /// <param name="user">User</param>
        protected abstract List<WorkflowStepInfo> GetNextStepInfoInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo step, UserInfo user);


        /// <summary>
        /// Returns previous step information for given node.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        protected abstract WorkflowStepInfo GetPreviousStepInfoInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo step, bool markAsUsed);


        /// <summary>
        /// Returns list of previous steps for current workflow cycle
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        protected abstract List<WorkflowStepInfo> GetPreviousStepsInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo step);

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets all workflow step transitions
        /// </summary>
        /// <param name="step">Workflow step</param>
        public List<WorkflowTransitionInfo> GetStepTransitions(WorkflowStepInfo step)
        {
            return GetStepTransitionsInternal(step, null);
        }


        /// <summary>
        /// Gets workflow step transitions
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="transitionType">Type of transitions to get</param>
        public List<WorkflowTransitionInfo> GetStepTransitions(WorkflowStepInfo step, WorkflowTransitionTypeEnum transitionType)
        {
            return GetStepTransitionsInternal(step, "TransitionType = " + (int)transitionType);
        }


        /// <summary>
        /// Evaluates list of existing transitions for given workflow step and returns either one best match or multiple transitions matched
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="resolver">Macro resolver to evaluate conditions</param>
        public List<WorkflowTransitionInfo> EvaluateTransitions(WorkflowStepInfo step, UserInfo user, int siteId, MacroResolver resolver)
        {
            return EvaluateTransitionsInternal(step, user, siteId, resolver, null);
        }


        /// <summary>
        /// Evaluates list of existing transitions for given workflow step and returns either one best match or multiple transitions matched
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="resolver">Macro resolver to evaluate conditions</param>
        /// <param name="transitionType">Transition type</param>
        public List<WorkflowTransitionInfo> EvaluateTransitions(WorkflowStepInfo step, UserInfo user, int siteId, MacroResolver resolver, WorkflowTransitionTypeEnum transitionType)
        {
            return EvaluateTransitionsInternal(step, user, siteId, resolver, "TransitionType = " + (int)transitionType);
        }


        /// <summary>
        /// Sends the workflow e-mails to given recipients.
        /// </summary>
        /// <param name="settings">E-mail sending settings</param>
        public void SendWorkflowEmails(WorkflowEmailSettings settings)
        {
            // Check if e-mails should be sent
            if (!SendEmails)
            {
                return;
            }

            SendWorkflowEmailsInternal(settings);
        }


        /// <summary>
        /// Processes all actions in scope.
        /// </summary>
        /// <param name="arguments">Initial action arguments</param>
        public WorkflowStepInfo ProcessActions(WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> arguments)
        {
            return ProcessActionsInternal(arguments);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Handles step additional actions
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="user">User info</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="step">Target workflow step</param>
        /// <param name="comment">Action comment</param>
        /// <param name="handleActions">Indicates if step actions should be handled</param>
        protected virtual WorkflowStepInfo HandleStepInternal(InfoType infoObj, StateInfoType stateObj, UserInfo user, WorkflowInfo workflow, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, bool handleActions)
        {
            // Handle step timeout
            HandleStepTimeoutInternal(infoObj, stateObj, user, workflow, currentStep, step);

            if (step != null)
            {
                // Handle step action
                if (step.StepIsAction && handleActions)
                {
                    var arguments = CreateActionArgumentsInternal(infoObj, stateObj, user, currentStep, step, comment);

                    if (CMSActionContext.CurrentAllowAsyncActions && WorkflowActionContext.CurrentProcessActionsAsync)
                    {
                        // Set status to 'RUNNING' synchronously
                        SetActionStatusInternal(infoObj, stateObj, WorkflowHelper.ACTION_SATUS_RUNNING);

                        WorkflowActionQueueWorker.Current.Enqueue(() => arguments.Manager.ProcessActions(arguments));
                    }
                    else
                    {
                        return arguments.Manager.ProcessActions(arguments);
                    }
                }
            }

            return step;
        }


        /// <summary>
        /// Handles step timeout
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        /// <param name="user">User info</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="step">Target workflow step</param>
        protected virtual void HandleStepTimeoutInternal(InfoType infoObj, StateInfoType stateObj, UserInfo user, WorkflowInfo workflow, WorkflowStepInfo currentStep, WorkflowStepInfo step)
        {
            if (stateObj == null)
            {
                throw new Exception("[AbstractWorkflowManager.HandleStepTimeout]: Missing state information.");
            }

            // Get object info
            string objectType = infoObj.TypeInfo.OriginalObjectType;
            int objectSiteId = infoObj.Generalized.ObjectSiteID;
            Guid stateGuid = stateObj.Generalized.ObjectGUID;

            // Delete timer task if exists
            if (currentStep != null)
            {
                if (workflow == null)
                {
                    throw new Exception("[AbstractWorkflowManager.HandleStepTimeout]: Missing workflow information.");
                }
                string taskName = WorkflowHelper.GetScheduledTaskName(stateGuid);
                TaskInfo existingTask = TaskInfoProvider.GetTaskInfo(taskName, objectSiteId);
                if (existingTask != null)
                {
                    TaskInfoProvider.DeleteTaskInfo(existingTask);
                }
            }

            if (step != null)
            {
                // Handle step timeout
                if (step.StepHasTimeout)
                {
                    if (workflow == null)
                    {
                        throw new Exception("[AbstractWorkflowManager.HandleStepTimeout]: Missing workflow information.");
                    }

                    // Prepare interval
                    TaskInterval taskInterval = new TaskInterval();
                    taskInterval.Period = SchedulingHelper.PERIOD_ONCE;

                    TaskInterval originalTaskInterval = SchedulingHelper.DecodeInterval(step.StepDefinition.TimeoutInterval);
                    taskInterval.StartTime = SchedulingHelper.GetFirstRunTime(originalTaskInterval);
                    string interval = SchedulingHelper.EncodeInterval(taskInterval);

                    // Create new task to schedule the wait
                    TaskInfo task = new TaskInfo
                    {
                        TaskObjectType = stateObj.TypeInfo.ObjectType,
                        TaskObjectID = stateObj.Generalized.ObjectID,
                        TaskInterval = interval,
                        TaskLastResult = string.Empty,
                        TaskName = WorkflowHelper.GetScheduledTaskName(stateGuid),
                        TaskSiteID = objectSiteId,
                        TaskNextRunTime = taskInterval.StartTime,
                        TaskDeleteAfterLastRun = true,
                        TaskType = ScheduledTaskTypeEnum.System,
                        TaskEnabled = true,

                        // Set user context
                        TaskUserID = (user != null) ? user.UserID : 0,

                        // Set task for processing in external service
                        TaskAllowExternalService = true,
                        TaskUseExternalService = SchedulingHelper.UseExternalService,
                    };

                    // Get task assembly
                    string taskDisplayName = null;
                    if (workflow.IsAutomation)
                    {
                        task.TaskAssemblyName = "CMS.Automation";
                        task.TaskClass = "CMS.Automation.AutomationTimer";
                        taskDisplayName = "MA.WaitStepTask";
                        task.TaskData = stateGuid + ";" + step.StepGUID + ";" + step.StepDefinition.TimeoutTarget;
                    }
                    else if (objectType == PredefinedObjectType.DOCUMENT)
                    {
                        task.TaskAssemblyName = "CMS.DocumentEngine";
                        task.TaskClass = "CMS.DocumentEngine.WorkflowTimer";
                        taskDisplayName = "Workflow.DocumentWaitStepTask";
                        task.TaskData = stateGuid + ";" + step.StepGUID + ";" + step.StepDefinition.TimeoutTarget;
                    }

                    string objectTypeName = ResHelper.GetString(TypeHelper.GetObjectTypeResourceKey(infoObj.TypeInfo.ObjectType));
                    task.TaskDisplayName = String.Format(
                        ResHelper.GetAPIString(taskDisplayName, "{0} '{1}' is waiting in '{2}' step of '{3}' workflow/process."),
                        objectTypeName,
                        TextHelper.LimitLength(infoObj.Generalized.ObjectDisplayName, 30),
                        TextHelper.LimitLength(step.StepDisplayName, 30),
                        TextHelper.LimitLength(workflow.WorkflowDisplayName, 30));

                    TaskInfoProvider.SetTaskInfo(task);
                }
            }
        }


        /// <summary>
        /// Creates action arguments with prefilled parameters.
        /// </summary>
        /// <param name="infoObj">Current object in workflow</param>
        /// <param name="stateObj">State object</param>
        /// <param name="user">User</param>
        /// <param name="initialStep">Initial workflow step</param>
        /// <param name="actionStep">Initial workflow step</param>
        /// <param name="comment">Action comment</param>
        protected virtual WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> CreateActionArgumentsInternal(InfoType infoObj, StateInfoType stateObj, UserInfo user, WorkflowStepInfo initialStep, WorkflowStepInfo actionStep, string comment)
        {
            // Prepare parameters
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(actionStep.StepWorkflowID);
            return CreateActionArgumentsInternal(infoObj, stateObj, user, initialStep, actionStep, workflow, this, comment);
        }


        /// <summary>
        /// Processes action connected to given step.
        /// </summary>
        /// <param name="arguments">Action arguments</param>
        protected virtual void ProcessActionInternal(WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> arguments)
        {
            // Prepare arguments
            WorkflowActionInfo action = arguments.ActionDefinition;
            InfoType infoObj = arguments.InfoObject;
            UserInfo currentUser = arguments.User;

            // Get executable
            try
            {
                if (action.ActionEnabled)
                {
                    // Get executable
                    var executable = ClassHelper.GetClass<BaseWorkflowAction<InfoType, StateInfoType, ActionEnumType>>(action.ActionAssemblyName, action.ActionClass);

                    if (executable != null)
                    {
                        executable.Process(arguments);
                    }
                    else
                    {
                        string message = String.Format("Action '{0}' cannot be loaded. Make sure that all the settings including assembly name and class name are correct.", HTMLHelper.HTMLEncode(ResHelper.LocalizeString(action.ActionDisplayName)));
                        LogMessageInternal(EventType.ERROR, EventLogSource + " action", "LOAD", message, infoObj, currentUser);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Concat(String.Format("There was an error processing action '{0}'. Original exception: ", HTMLHelper.HTMLEncode(ResHelper.LocalizeString(action.ActionDisplayName))), "\n", EventLogProvider.GetExceptionLogMessage(ex));
                LogMessageInternal(EventType.ERROR, EventLogSource + " action", "PROCESS", message, infoObj, currentUser);
            }
        }


        /// <summary>
        /// Creates action arguments.
        /// </summary>
        /// <param name="infoObj">Current object in workflow</param>
        /// <param name="stateObj">State object</param>
        /// <param name="user">User</param>
        /// <param name="initialStep">Initial workflow step</param>
        /// <param name="actionStep">Initial workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="manager">Manager</param>
        /// <param name="comment">Action comment</param>
        protected virtual WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> CreateActionArgumentsInternal(InfoType infoObj, StateInfoType stateObj, UserInfo user, WorkflowStepInfo initialStep, WorkflowStepInfo actionStep, WorkflowInfo workflow, AbstractWorkflowManager<InfoType, StateInfoType, ActionEnumType> manager, string comment)
        {
            WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> args = new WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>
            {
                InfoObject = infoObj,
                StateObject = stateObj,
                User = user,
                Workflow = workflow,
                ActionStep = actionStep,
                InitialStep = initialStep,
                Manager = manager,
                Comment = comment
            };

            return args;
        }


        /// <summary>
        /// Processes all actions in scope.
        /// </summary>
        /// <param name="arguments">Initial action arguments</param>
        protected virtual WorkflowStepInfo ProcessActionsInternal(WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> arguments)
        {
            // Get arguments
            UserInfo user = arguments.User;
            WorkflowStepInfo lastStep = arguments.InitialStep;
            WorkflowStepInfo currentStep = arguments.ActionStep;
            WorkflowInfo workflow = arguments.Workflow;
            InfoType infoObj = arguments.InfoObject;
            StateInfoType stateObj = arguments.StateObject;
            bool processStep = currentStep.StepIsAction;

            try
            {
                // Set status to 'RUNNING'
                SetActionStatusInternal(infoObj, stateObj, WorkflowHelper.ACTION_SATUS_RUNNING);

                // Go through all actions in sequence or through all next steps with single automatic transition
                while (processStep && (lastStep.StepGUID != currentStep.StepGUID) && (CurrentHops < WorkflowHelper.MaxStepsHopsCount))
                {
                    // Update parameters
                    arguments.OriginalStep = lastStep;
                    arguments.ActionStep = currentStep;

                    // Process action
                    if (currentStep.StepIsAction)
                    {
                        ProcessActionInternal(arguments);
                        // Update instance
                        infoObj = arguments.InfoObject;
                    }

                    // Edited object has been deleted by the action or processing should be stopped
                    if ((infoObj == null) || arguments.StopProcessing)
                    {
                        return currentStep;
                    }

                    // Prepare resolver
                    MacroResolver resolver = GetEvalResolverInternal(infoObj, stateObj, workflow, currentStep, user);

                    // Get wining transition
                    List<WorkflowTransitionInfo> winTransitions = EvaluateTransitions(currentStep, user, infoObj.Generalized.ObjectSiteID, resolver, WorkflowTransitionTypeEnum.Automatic);

                    if (winTransitions.Count != 1)
                    {
                        // Multiple or none automatic transition -> needs user interaction
                        break;
                    }

                    // Update for next move
                    lastStep = currentStep;
                    currentStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(winTransitions[0].TransitionEndStepID);

                    // Move to next step
                    using (new CMSActionContext(user) { LogEvents = !currentStep.StepIsAction })
                    {
                        currentStep = MoveToStepInternal(infoObj, stateObj, lastStep, currentStep, arguments.Comment, WorkflowTransitionTypeEnum.Automatic, false);
                    }

                    // Process next step if not finished and step isn't a wait step
                    processStep = (currentStep != null) && (currentStep.StepType != WorkflowStepTypeEnum.Wait);

                    ++CurrentHops;
                }

                // Log warning
                if (CurrentHops >= WorkflowHelper.MaxStepsHopsCount)
                {
                    LogHopsReachedWarningInternal(infoObj, user);
                }
            }
            finally
            {
                if (infoObj != null)
                {
                    SetActionStatusInternal(infoObj, stateObj, null);
                }
            }

            return currentStep;
        }


        /// <summary>
        /// Moves the specified object to the first step without automatic transition in the workflow and returns the final step.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="stateObj">State object</param>
        /// <param name="currentStep">Current workflow step of the object</param>
        /// <param name="user">User</param>
        /// <param name="comment">Action comment</param>
        /// <remarks>The return step can be different than the original target step.</remarks>
        protected virtual WorkflowStepInfo MoveStepInternal(InfoType infoObj, StateInfoType stateObj, WorkflowStepInfo currentStep, UserInfo user, string comment)
        {
            // Do not move further
            if ((currentStep == null) || currentStep.StepIsAction || (currentStep.StepType == WorkflowStepTypeEnum.Wait))
            {
                return currentStep;
            }

            // Get step workflow
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(currentStep.StepWorkflowID);
            // Unsupported workflow type
            if (workflow.IsBasic)
            {
                return currentStep;
            }

            // Prepare resolver
            MacroResolver resolver = GetEvalResolverInternal(infoObj, stateObj, workflow, currentStep, user);

            // Get wining transition
            List<WorkflowTransitionInfo> winTransitions = EvaluateTransitions(currentStep, user, infoObj.Generalized.ObjectSiteID, resolver, WorkflowTransitionTypeEnum.Automatic);
            bool logWarning = (winTransitions.Count < 1) && !currentStep.StepAllowTimeout && !currentStep.StepIsFinished;

            // Handle transitions
            while ((winTransitions.Count == 1) && (CurrentHops < WorkflowHelper.MaxStepsHopsCount))
            {
                // Get next step
                WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(winTransitions[0].TransitionEndStepID);

                // Do not move further if the next step is current step
                if (currentStep.StepGUID == step.StepGUID)
                {
                    return currentStep;
                }
                else
                {
                    // Move to next step
                    currentStep = MoveToStepInternal(infoObj, stateObj, currentStep, step, comment, WorkflowTransitionTypeEnum.Automatic, true);

                    // Workflow is finished or step is an action or wait step
                    if ((currentStep == null) || currentStep.StepIsAction || (currentStep.StepType == WorkflowStepTypeEnum.Wait))
                    {
                        // Finish
                        return currentStep;
                    }

                    // Get wining transition
                    resolver = GetEvalResolverInternal(infoObj, stateObj, workflow, currentStep, user);
                    winTransitions = EvaluateTransitions(currentStep, user, infoObj.Generalized.ObjectSiteID, resolver, WorkflowTransitionTypeEnum.Automatic);
                    logWarning = (winTransitions.Count < 1) && !currentStep.StepIsDefault && (currentStep.StepType != WorkflowStepTypeEnum.Standard);
                }

                ++CurrentHops;
            }

            // Log warning
            if (CurrentHops >= WorkflowHelper.MaxStepsHopsCount)
            {
                LogHopsReachedWarningInternal(infoObj, user);
            }

            // Log warning, missing transition
            if (logWarning)
            {
                LogMissingConnectionWarningInternal(infoObj, user);
            }

            return currentStep;
        }


        /// <summary>
        /// Gets basic resolver for evaluation of transitions and source points
        /// </summary>
        /// <param name="workflow">Workflow</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected virtual MacroResolver GetBasicResolverInternal(WorkflowInfo workflow, WorkflowStepInfo step, UserInfo user)
        {
            MacroResolver resolver = MacroResolver.CreateChild();

            // Backward compatibility
            resolver.SetNamedSourceData("User", user);

            // Add named sources
            resolver.SetNamedSourceData("CurrentUser", user);
            resolver.SetNamedSourceData("Workflow", workflow);
            if (step != null)
            {
                resolver.SetNamedSourceData("CurrentStep", step);

                if (step.StepActionID > 0)
                {
                    // Action definition
                    resolver.SetNamedSourceData("ActionDefinition", WorkflowActionInfoProvider.GetWorkflowActionInfo(step.StepActionID));
                }
            }

            return resolver;
        }


        /// <summary>
        /// Evaluates source points with given macro resolver
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="points">Source points</param>
        /// <param name="resolver">Macro resolver</param>
        protected virtual List<SourcePoint> EvaluateSourcePointsInternal(WorkflowStepInfo step, List<SourcePoint> points, MacroResolver resolver)
        {
            List<SourcePoint> matchPoints = new List<SourcePoint>();

            bool firstWin = step.StepHasSingleWinTransition;

            // Go through all connect points
            foreach (var point in points)
            {
                // Get possible wining transition
                WorkflowTransitionInfo transition = WorkflowTransitionInfoProvider.GetWorkflowTransitions().WhereEquals("TransitionSourcePointGUID", point.Guid).TopN(1).FirstOrDefault();

                if (transition != null)
                {
                    // Get next step
                    WorkflowStepInfo nextStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(transition.TransitionEndStepID);

                    // Prepare resolver
                    resolver.SetNamedSourceData("NextStep", nextStep);

                    // Condition not specified or matches
                    if (string.IsNullOrEmpty(point.Condition) || ValidationHelper.GetBoolean(resolver.ResolveMacros(point.Condition), false))
                    {
                        matchPoints.Add(point);

                        // Return first matched transition if required
                        if (firstWin)
                        {
                            return matchPoints;
                        }
                    }
                }
            }

            return matchPoints;
        }


        /// <summary>
        /// Sends the workflow e-mails to given recipients.
        /// </summary>
        /// <param name="settings">E-mail sending settings</param>
        protected virtual void SendWorkflowEmailsInternal(WorkflowEmailSettings settings)
        {
            if (settings.Recipients.Count > 0)
            {
                // Get site information
                string siteName = null;
                int siteId = 0;
                if (!string.IsNullOrEmpty(settings.SiteName))
                {
                    siteName = settings.SiteName;
                    siteId = SiteInfoProvider.GetSiteID(siteName);
                }
                string defaultSender = WorkflowHelper.GetWorkflowEmailsSender(siteName);

                EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate(settings.EmailTemplateName, settings.SiteName);
                // Check email template
                if (emailTemplate == null)
                {
                    if (!string.IsNullOrEmpty(settings.EmailTemplateName))
                    {
                        if (settings.LogEvents)
                        {
                            EventLogProvider.LogEvent(EventType.ERROR, EventLogSource + " e-mail", "SEND", "E-mail template '" + settings.EmailTemplateName + "' not found.'", RequestContext.RawURL, settings.User.UserID, settings.User.UserName, 0, null, RequestContext.UserHostAddress, siteId);
                        }
                        // E-mail template not found, do not continue
                        return;
                    }
                    else
                    {
                        emailTemplate = new EmailTemplateInfo();
                    }
                }

                // Prepare the message
                EmailMessage mess = new EmailMessage();
                mess.EmailFormat = EmailFormatEnum.Both;
                mess.From = DataHelper.GetNotEmpty(settings.Sender, EmailHelper.GetSender(emailTemplate, defaultSender));
                mess.CcRecipients = emailTemplate.TemplateCc;
                mess.BccRecipients = emailTemplate.TemplateBcc;
                mess.ReplyTo = emailTemplate.TemplateReplyTo;

                // Resolve macros in message body
                mess.Body = DataHelper.GetNotEmpty(settings.Body, settings.Resolver.ResolveMacros(emailTemplate.TemplateText));

                // Do not encode plain text body and subject
                MacroResolver childResolver = settings.Resolver.CreateChild();
                childResolver.Settings.EncodeResolvedValues = false;
                mess.Subject = childResolver.ResolveMacros(DataHelper.GetNotEmpty(settings.Subject, DataHelper.GetNotEmpty(emailTemplate.TemplateSubject, settings.DefaultSubject)));
                mess.PlainTextBody = childResolver.ResolveMacros(emailTemplate.TemplatePlainText);

                // Resolve e-mail attachments
                EmailHelper.ResolveMetaFileImages(mess, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                // Send e-mail to all recipients
                foreach (string recipient in settings.Recipients)
                {
                    try
                    {
                        // Skip empty recipients
                        if (string.IsNullOrEmpty(recipient))
                        {
                            continue;
                        }

                        mess.Recipients = recipient;
                        EmailSender.SendEmail(siteName, mess);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogEvent(EventType.ERROR, EventLogSource + " e-mail", "SEND", EventLogProvider.GetExceptionLogMessage(ex), RequestContext.RawURL, settings.User.UserID, settings.User.UserName, 0, null, RequestContext.UserHostAddress, siteId);
                    }
                }
            }
        }


        /// <summary>
        /// Gets transitions leading into given step
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="where">Where condition</param>
        protected virtual List<WorkflowTransitionInfo> GetStepInboundTransitionsInternal(WorkflowStepInfo step, string where)
        {
            List<WorkflowTransitionInfo> transitions = new List<WorkflowTransitionInfo>();

            // Get step transitions
            where = SqlHelper.AddWhereCondition("TransitionEndStepID = " + step.StepID, where);
            InfoDataSet<WorkflowTransitionInfo> trans = WorkflowTransitionInfoProvider.GetWorkflowTransitions().Where(where).TypedResult;

            foreach (var transition in trans.Items)
            {
                transitions.Add(transition);
            }

            return transitions;
        }

        #endregion


        #region "Event log methods"

        /// <summary>
        /// Logs custom message to event log.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="eventCode">Event code</param>
        /// <param name="message">Message</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="user">User</param>
        public virtual void LogMessage(string eventType, string eventCode, string message, InfoType infoObj, UserInfo user)
        {
            LogMessageInternal(eventType, EventLogSource, eventCode, message, infoObj, user);
        }


        /// <summary>
        /// Logs warning about missing connection
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="user">User</param>
        protected virtual void LogMissingConnectionWarningInternal(InfoType infoObj, UserInfo user)
        {
            LogMessageInternal(EventType.WARNING, EventLogSource, "PROCESS", ResHelper.GetAPIString("workflow.warning.logmissingconnection", "Process stopped in unexpected step. Make sure that you defined all necessary connections."), infoObj, user);
        }


        /// <summary>
        /// Logs warning about maximum hops reached
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="user">User</param>
        protected virtual void LogHopsReachedWarningInternal(InfoType infoObj, UserInfo user)
        {
            LogMessageInternal(EventType.WARNING, EventLogSource, "PROCESS", ResHelper.GetAPIString("workflow.warning.loghopsreached", "Process has been stopped. The number of maximum automatic transitions in one sequence has been reached. Make sure that you don't have cycles in your workflow process."), infoObj, user);
        }


        /// <summary>
        /// Logs message to the event log
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="source">Source</param>
        /// <param name="eventCode">Event code</param>
        /// <param name="message">Message</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="user">User</param>
        protected virtual void LogMessageInternal(string type, string source, string eventCode, string message, InfoType infoObj, UserInfo user)
        {
            string objectName = infoObj.TypeInfo.GetNiceObjectTypeName().ToLowerInvariant();
            string name = objectName + " '" + ResHelper.LocalizeString(infoObj.Generalized.ObjectDisplayName) + "'";

            string text = String.Format(ResHelper.GetAPIString("TaskTitle.ProcessObject", "Processing {0}."), name);

            EventLogProvider.LogEvent(type, source, eventCode, text + "\r\n\r\n " + message, null, user.UserID, user.UserName, 0, null, null, infoObj.Generalized.ObjectSiteID);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Evaluates list of existing transitions for given workflow step and returns either one best match or multiple transitions matched
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="resolver">Macro resolver to evaluate conditions</param>
        /// <param name="where">Where condition for transitions</param>
        private List<WorkflowTransitionInfo> EvaluateTransitionsInternal(WorkflowStepInfo step, UserInfo user, int siteId, MacroResolver resolver, string where)
        {
            List<WorkflowTransitionInfo> winTransitions = new List<WorkflowTransitionInfo>();

            // Evaluate possible transition
            if (!step.StepAllowBranch)
            {
                // Get step connect point if exists
                SourcePoint point = step.StepDefinition.DefinitionPoint;
                if (point != null)
                {
                    // Get step transition if exists
                    where = SqlHelper.AddWhereCondition(where, string.Format("TransitionSourcePointGUID='{0}'", point.Guid));
                    WorkflowTransitionInfo transition = GetStepTransitionsInternal(step, where).FirstOrDefault();
                    if (transition != null)
                    {
                        // Condition specified
                        if (!string.IsNullOrEmpty(point.Condition))
                        {
                            // Get next step
                            WorkflowStepInfo nextStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(transition.TransitionEndStepID);

                            // Prepare resolver
                            resolver.SetNamedSourceData("NextStep", nextStep);

                            // Evaluate condition
                            bool condition = ValidationHelper.GetBoolean(resolver.ResolveMacros(point.Condition), false);
                            if (condition)
                            {
                                // Transition matched
                                winTransitions.Add(transition);
                            }
                        }
                        else
                        {
                            // Add transition
                            winTransitions.Add(transition);
                        }
                    }
                }
                else
                {
                    // Get step transition if exists
                    WorkflowTransitionInfo transition = GetStepTransitionsInternal(step, where).FirstOrDefault();
                    if (transition != null)
                    {
                        winTransitions.Add(transition);
                    }
                }
            }
            // Evaluate multiple transitions
            else
            {
                // Get step connected points if exists, except timeout source point
                var points = step.StepDefinition.SourcePoints.Where(p => !(p is TimeoutSourcePoint)).ToList();
                var transitions = GetStepTransitionsInternal(step, where);

                // No connect points to evaluate
                if (!points.Any())
                {
                    // Get all existing transitions
                    return transitions;
                }

                // Evaluate transitions (macros, security etc.) - step configuration
                bool firstWin = step.StepHasSingleWinTransition;
                WorkflowTransitionInfo elsePointTransition = null;

                // Go through all connected points
                foreach (var point in points)
                {
                    // Skip timeout source points
                    if (point is TimeoutSourcePoint)
                    {
                        continue;
                    }

                    // Get possible wining transition
                    WorkflowTransitionInfo transition = transitions.SingleOrDefault(t => t.TransitionSourcePointGUID == point.Guid);

                    // Do not evaluate source points without transitions
                    if (transition == null)
                    {
                        continue;
                    }

                    // Check permissions
                    if (WorkflowStepInfoProvider.CanUserApprove(user, step, point, siteId))
                    {
                        // Condition specified
                        if (!string.IsNullOrEmpty(point.Condition))
                        {
                            // Get next step
                            WorkflowStepInfo nextStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(transition.TransitionEndStepID);

                            // Prepare resolver
                            resolver.SetNamedSourceData("NextStep", nextStep);

                            // Evaluate condition
                            bool condition = ValidationHelper.GetBoolean(resolver.ResolveMacros(point.Condition), false);
                            if (condition)
                            {
                                // Possible transition
                                winTransitions.Add(transition);

                                // Else branch evaluated
                                if (point is ElseSourcePoint)
                                {
                                    elsePointTransition = transition;
                                }

                                // Return first matched transition if required
                                if (firstWin)
                                {
                                    return winTransitions;
                                }
                            }
                        }
                        else
                        {
                            // First possible transition
                            winTransitions.Add(transition);

                            // Else branch evaluated
                            if (point is ElseSourcePoint)
                            {
                                elsePointTransition = transition;
                            }

                            // Return first possible transition if required
                            if (firstWin)
                            {
                                return winTransitions;
                            }
                        }
                    }
                }

                // Possible case and else winning transitions
                if ((winTransitions.Count == 2) && (elsePointTransition != null))
                {
                    // Remove else transition which should be used only when none of the case transitions matches
                    winTransitions.Remove(elsePointTransition);
                }
            }

            return winTransitions;
        }


        /// <summary>
        /// Gets workflow step transitions
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="where">Where condition</param>
        private List<WorkflowTransitionInfo> GetStepTransitionsInternal(WorkflowStepInfo step, string where)
        {
            return WorkflowTransitionInfoProvider.GetStepTransitions(step, where);
        }

        #endregion
    }
}
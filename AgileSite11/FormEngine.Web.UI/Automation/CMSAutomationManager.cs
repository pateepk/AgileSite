using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Automation;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.WorkflowEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Automation manager.
    /// </summary>
    [ToolboxData("<{0}:CMSAutomationManager runat=server></{0}:CMSAutomationManager>")]
    public class CMSAutomationManager : CMSAbstractManager<AutomationManagerEventArgs, SimpleObjectManagerEventArgs>, IPostBackEventHandler
    {
        #region "Variables"

        /// <summary>
        /// AutomationManager for managing processes.
        /// </summary>
        private AutomationManager mManager = null;


        /// <summary>
        /// Next steps
        /// </summary>
        private List<WorkflowStepInfo> mNextSteps = null;


        /// <summary>
        /// Process
        /// </summary>
        private WorkflowInfo mProcess = null;


        /// <summary>
        /// Object ID
        /// </summary>
        private int? mObjectID = null;
        private AutomationStateInfo mStateObject = null;


        /// <summary>
        /// Object type
        /// </summary>
        private string mObjectType = null;


        /// <summary>
        /// Indicates if content should be refreshed (action is being processed)
        /// </summary>
        private bool? mRefreshActionContent = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if manager should register for common events (save etc.).
        /// </summary>
        public bool RegisterEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Gets Automation manager instance.
        /// </summary>
        public virtual AutomationManager Manager
        {
            get
            {
                return mManager ?? (mManager = AutomationManager.GetInstance(MembershipContext.AuthenticatedUser));
            }
        }


        /// <summary>
        /// Automation process.
        /// </summary>
        public WorkflowInfo Process
        {
            get
            {
                if (mProcess == null)
                {
                    if (StateObject != null)
                    {
                        // Get process info
                        mProcess = Manager.GetObjectProcess(InfoObject, StateObject);
                    }
                }

                return mProcess;
            }
        }


        /// <summary>
        /// Step.
        /// </summary>
        public WorkflowStepInfo Step
        {
            get
            {
                if (StateObject != null)
                {
                    // Get step info
                    return Manager.GetStepInfo(InfoObject, StateObject);
                }

                return null;
            }
        }


        /// <summary>
        /// Next steps
        /// </summary>
        public List<WorkflowStepInfo> NextSteps
        {
            get
            {
                return mNextSteps ?? (mNextSteps = Manager.GetNextSteps(InfoObject, StateObject));
            }
        }


        /// <summary>
        /// Automation information label.
        /// </summary>
        public Label AutomationInfoLabel
        {
            get
            {
                EnsureChildControls();
                return InfoPanel.Label;
            }
        }


        /// <summary>
        /// Automation information text.
        /// </summary>
        public string AutomationInfo
        {
            get
            {
                return AutomationInfoLabel.Text;
            }
            set
            {
                AutomationInfoLabel.Text = value;
            }
        }


        /// <summary>
        /// State object ID
        /// </summary>
        public int StateObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// State object
        /// </summary>
        public AutomationStateInfo StateObject
        {
            get
            {
                if (mStateObject == null)
                {
                    if (StateObjectID > 0)
                    {
                        mStateObject = AutomationStateInfoProvider.GetAutomationStateInfo(StateObjectID);
                    }

                    if ((mStateObject == null) && (Mode != FormModeEnum.Insert))
                    {
                        // Show standard page
                        URLHelper.Redirect(AdministrationUrlHelper.GetInformationUrl("editedobject.notexists"));
                    }
                }

                return mStateObject;
            }
            set
            {
                mStateObject = value;
            }
        }


        /// <summary>
        /// Returns currently edited object if it is available in the given context.
        /// </summary>
        public BaseInfo InfoObject
        {
            get
            {
                if (UIContext.EditedObject == null)
                {
                    BaseInfo obj = null;
                    switch (Mode)
                    {
                        case FormModeEnum.Update:
                            obj = ProviderHelper.GetInfoById(ObjectType, ObjectID);
                            break;

                        case FormModeEnum.Insert:
                            obj = ModuleManager.GetObject(ObjectType);

                            // Load default data
                            FormHelper.LoadDefaultValues(obj.TypeInfo.ObjectClassName, obj);
                            break;
                    }

                    UIContext.EditedObject = obj;
                }

                return UIContext.EditedObject as BaseInfo;
            }
        }


        /// <summary>
        /// Object ID. Together with ObjectType property identifies edited object.
        /// </summary>
        public int ObjectID
        {
            get
            {
                // Check state object
                if (mObjectID == null)
                {
                    mObjectID = (StateObject != null) ? StateObject.StateObjectID : 0;
                }

                return mObjectID.Value;
            }
            set
            {
                mObjectID = value;
            }
        }


        /// <summary>
        /// Object type. Together with ObjectID property identifies edited object.
        /// </summary>
        public string ObjectType
        {
            get
            {
                // Check state object
                if (mObjectType == null)
                {
                    mObjectType = (StateObject != null) ? StateObject.StateObjectType : null;
                }

                return mObjectType;
            }
            set
            {
                mObjectType = value;
            }
        }


        /// <summary>
        /// Indicates if java script functions for automation management should be rendered.
        /// </summary>
        public bool RenderScript
        {
            get;
            set;
        }


        /// <summary>
        /// Current step ID
        /// </summary>
        protected int CurrentStepID
        {
            get
            {
                return ValidationHelper.GetInteger(CurrentStepArg.Value, 0);
            }
            set
            {
                CurrentStepArg.Value = value.ToString();
            }
        }


        /// <summary>
        /// Indicates if Save action is allowed.
        /// </summary>
        public bool AllowSave
        {
            get
            {
                return IsActionAllowed(ComponentEvents.SAVE);
            }
        }


        /// <summary>
        /// Indicates if action is being processed
        /// </summary>
        public bool ProcessingAction
        {
            get
            {
                return (Manager.GetActionStatus(InfoObject, StateObject) == WorkflowHelper.ACTION_SATUS_RUNNING);
            }
        }


        /// <summary>
        /// Indicates if content should be refreshed (action is being processed)
        /// </summary>
        public bool RefreshActionContent
        {
            get
            {
                if (mRefreshActionContent == null)
                {
                    bool isActionStep = (Step != null) && Step.StepIsAction;
                    if (isActionStep)
                    {
                        // Action is being processed
                        mRefreshActionContent = ProcessingAction;
                    }
                    else
                    {
                        mRefreshActionContent = false;
                    }
                }

                return mRefreshActionContent.Value;
            }
        }


        /// <summary>
        /// Action comment
        /// </summary>
        public string ActionComment
        {
            get
            {
                EnsureChildControls();
                return HTMLHelper.HTMLEncode(HiddenComment.Value);
            }
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        public override MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                return LocalMessagesPlaceHolder;
            }
        }


        /// <summary>
        /// Local messages placeholder
        /// </summary>
        public MessagesPlaceHolder LocalMessagesPlaceHolder
        {
            get
            {
                EnsureChildControls();
                return plcMess;
            }
            set
            {
                plcMess = value;
            }
        }


        /// <summary>
        /// Info panel
        /// </summary>
        public CMSInfoPanel InfoPanel
        {
            get
            {
                return LocalInfoPanel;
            }
        }


        /// <summary>
        /// Local info panel
        /// </summary>
        public CMSInfoPanel LocalInfoPanel
        {
            get
            {
                EnsureChildControls();
                return pnlInfo;
            }
            set
            {
                pnlInfo = value;
            }
        }


        /// <summary>
        /// Hidden argument
        /// </summary>
        protected HiddenField HiddenArguments
        {
            get
            {
                EnsureChildControls();
                return hdnArgs;
            }
        }


        /// <summary>
        /// Hidden current step flag
        /// </summary>
        protected HiddenField CurrentStepArg
        {
            get
            {
                EnsureChildControls();
                return hdnCurrStep;
            }
        }


        /// <summary>
        /// Hidden comment field
        /// </summary>
        protected HiddenField HiddenComment
        {
            get
            {
                EnsureChildControls();
                return hdnComment;
            }
        }

        #endregion


        #region "Controls"

        /// <summary>
        /// Hidden field for post back arguments
        /// </summary>
        protected HiddenField hdnArgs = new HiddenField { ID = "hdnArgs", EnableViewState = false };

        /// <summary>
        /// Hidden field for current step argument
        /// </summary>
        protected HiddenField hdnCurrStep = new HiddenField { ID = "hdnCurrStep", EnableViewState = true };

        /// <summary>
        /// Hidden field for comment argument
        /// </summary>
        protected HiddenField hdnComment = new HiddenField { ID = "hdnComment", EnableViewState = false };

        /// <summary>
        /// Messages placeholder
        /// </summary>
        protected MessagesPlaceHolder plcMess = null;

        /// <summary>
        /// Info panel
        /// </summary>
        protected CMSInfoPanel pnlInfo = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSAutomationManager()
        {
            RenderScript = false;
            CheckPermissions = true;
            Mode = FormModeEnum.Update;
            DataConsistent = true;
            RegisterEvents = true;
        }

        #endregion


        #region "Page processing methods"

        /// <summary>
        /// CreateChildControls event handler.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (StopProcessing)
            {
                return;
            }

            if (plcMess == null)
            {
                ICMSPage page = Page as ICMSPage;
                if (page != null)
                {
                    plcMess = page.MessagesPlaceHolder;
                }
                else
                {
                    plcMess = new MessagesPlaceHolder { ID = "plcMess", ShortID = "pM", IsLiveSite = IsLiveSite };
                    Controls.AddAt(0, plcMess);
                }
            }

            // Add the hidden field for post back arguments
            Controls.Add(hdnArgs);
            Controls.Add(hdnComment);
            Controls.Add(hdnCurrStep);

            // Info panel
            if (pnlInfo == null)
            {
                pnlInfo = new CMSInfoPanel { ID = "pnlInfo", ShortID = "pI" };
                Controls.AddAt(0, pnlInfo);
            }
        }


        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                return;
            }

            if (RegisterEvents)
            {
                if (InfoObject == null)
                {
                    return;
                }

                // Register events
                if (Mode == FormModeEnum.Update)
                {
                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.AUTOMATION_MOVE_NEXT, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_MOVE_NEXT)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_NEXT))
                        {
                            CommandEventArgs com = args as CommandEventArgs;
                            int stepId = ValidationHelper.GetInteger((com != null) ? com.CommandArgument : HiddenArguments.Value, 0);
                            MoveToNextStep(stepId, ActionComment);
                        }
                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.AUTOMATION_MOVE_SPEC, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_MOVE_SPEC)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_SPEC))
                        {
                            CommandEventArgs com = args as CommandEventArgs;
                            int stepId = ValidationHelper.GetInteger((com != null) ? com.CommandArgument : HiddenArguments.Value, 0);
                            MoveToSpecificStep(stepId, ActionComment);
                        }
                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.AUTOMATION_MOVE_PREVIOUS, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_MOVE_PREVIOUS)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_PREVIOUS))
                        {
                            CommandEventArgs com = args as CommandEventArgs;
                            int historyId = ValidationHelper.GetInteger((com != null) ? com.CommandArgument : HiddenArguments.Value, 0);
                            MoveToPreviousStep(historyId, ActionComment);
                        }

                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.AUTOMATION_REMOVE, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_REMOVE)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.AUTOMATION_REMOVE))
                        {
                            RemoveProcess();
                        }

                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.AUTOMATION_START, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_START)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.AUTOMATION_START))
                        {
                            CommandEventArgs com = args as CommandEventArgs;
                            int processId = ValidationHelper.GetInteger((com != null) ? com.CommandArgument : HiddenArguments.Value, 0);
                            StartProcess(processId);
                        }

                        ClearComment();
                    });
                }
            }
        }


        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StopProcessing)
            {
                return;
            }

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CMSAutomationManager Control : " + ClientID + " ]");
                return;
            }

            RenderScripts();
        }


        /// <summary>
        /// Renders scripts
        /// </summary>
        public void RenderScripts()
        {
            StringBuilder sb = new StringBuilder();
            if (RenderScript)
            {
                // Submit script for next, previous and save action
                bool addComment = false;
                bool setStepId = false;

                sb.Append("function CheckConsistency_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(this, null), "; } \n");
                if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_NEXT))
                {
                    sb.Append("function MoveNext_", ClientID, "(stepId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(stepId);", ControlsHelper.GetPostBackEventReference(this, ComponentEvents.AUTOMATION_MOVE_NEXT), "; } \n");
                    addComment = true;
                    setStepId = true;
                }
                if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_SPEC))
                {
                    sb.Append("function MoveSpecific_", ClientID, "(stepId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(stepId);", ControlsHelper.GetPostBackEventReference(this, ComponentEvents.AUTOMATION_MOVE_SPEC), "; } \n");
                    addComment = true;
                    setStepId = true;
                }
                if (IsActionAllowed(ComponentEvents.AUTOMATION_MOVE_PREVIOUS))
                {
                    sb.Append("function MovePrevious_", ClientID, "(historyId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(historyId);", ControlsHelper.GetPostBackEventReference(this, ComponentEvents.AUTOMATION_MOVE_PREVIOUS), "; } \n");
                    addComment = true;
                    setStepId = true;
                }

                if (setStepId)
                {
                    sb.Append("function SetParam_", ClientID, "(param) { document.getElementById('", HiddenArguments.ClientID, "').value = param; } \n");
                }

                if (addComment)
                {
                    // Register the dialog script
                    ScriptHelper.RegisterDialogScript(Page);

                    string url = ApplicationUrlHelper.ResolveDialogUrl("~/CMSModules/Automation/Pages/Comment.aspx");
                    sb.Append("function AddComment_", ClientID, "(name, stateId, menuId) { if((menuId == null) || (menuId == '')){ menuId = '", ClientID, "'; } modalDialog('", url, "?acname=' + name + '&stateId=' + stateId + '&menuId=' + menuId, 'automationComment', 770, 400, null, null, true); }\n");
                    sb.Append("function SetComment_", ClientID, "(comment) { document.getElementById('", HiddenComment.ClientID, "').value = comment; } \n");
                }
            }

            // Register the script
            if (sb.Length > 0)
            {
                ScriptHelper.RegisterStartupScript(Page, typeof(string), "AutomationManagement_" + ClientID, ScriptHelper.GetScript(sb.ToString()));
            }
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="argument">Action argument</param>
        /// <param name="comment">Action comment</param>
        public string GetJSFunction(string action, string argument, string comment)
        {
            return GetJSFunctionInternal(action, argument, comment);
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="arg">Argument (optional)</param>
        /// <param name="comment">Action comment (optional)</param>
        private string GetJSFunctionInternal(string action, string arg, string comment)
        {
            string name = "";

            if (string.IsNullOrEmpty(comment))
            {
                comment = "''";
            }

            switch (action)
            {
                case "CONS":
                    name = "CheckConsistency_{0}()";
                    break;

                case ComponentEvents.AUTOMATION_MOVE_NEXT:
                    name = "MoveNext_{0}({2},{1})";
                    break;

                case ComponentEvents.AUTOMATION_MOVE_PREVIOUS:
                    name = "MovePrevious_{0}({2},{1})";
                    break;

                case ComponentEvents.AUTOMATION_MOVE_SPEC:
                    name = "MoveSpecific_{0}({2},{1})";
                    break;

                // Special case
                case ComponentEvents.COMMENT:
                    // actionName|stateId|menuId
                    string[] args = arg.Split('|');
                    string menuId = (args.Length == 3) ? args[2] : "''";
                    return string.Format("AddComment_{0}({1},{2},{3})", ClientID, args[0], args[1], menuId);
            }

            return string.Format(name, ClientID, comment, arg, action);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (StopProcessing)
            {
                return;
            }

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            // Store step ID
            switch (Mode)
            {
                case FormModeEnum.Insert:
                case FormModeEnum.InsertNewCultureVersion:
                    break;

                default:
                    // Store step ID only for editing
                    CurrentStepID = (Step != null) ? Step.StepID : 0;
                    break;
            }

            ScriptHelper.RegisterCompletePageScript(Page);
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Checks if given action is allowed
        /// </summary>
        public bool IsActionAllowed(string actionName)
        {
            // Object is not initialized
            if (InfoObject == null)
            {
                return true;
            }

            // Initialize security properties
            string storageKey = string.Format("AutomationManager_{0}_{1}_{2}", ObjectType, InfoObject.Generalized.ObjectID, Mode);
            InitializeSecurityProperties(storageKey);

            // Check request stock helper
            string key = GetActionKey(actionName);
            if (RequestStockHelper.Contains(storageKey, key, false))
            {
                return ValidationHelper.GetBoolean(RequestStockHelper.GetItem(storageKey, key, false), false);
            }

            return false;
        }


        /// <summary>
        /// Gets action request key
        /// </summary>
        private string GetActionKey(string actionName)
        {
            return string.Join("_", new string[] { actionName });
        }


        /// <summary>
        /// Clears properties
        /// </summary>
        public override void ClearProperties()
        {
            if (InfoObject != null)
            {
                string storageKey = string.Format("AutomationManager_{0}_{1}_{2}", ObjectType, InfoObject.Generalized.ObjectID, Mode);
                RequestStockHelper.DropStorage(storageKey, false);
            }

            // Clear next step info
            mNextSteps = null;

            // Clear process info
            mProcess = null;
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Gets the default error message for check permission error
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override string GetDefaultCheckPermissionsError(SimpleObjectManagerEventArgs args)
        {
            return ResHelper.GetString("ma.notauthorizedtomanageprocess", ResourceCulture);
        }


        /// <summary>
        /// Checks the default security for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override void CheckDefaultSecurity(SimpleObjectManagerEventArgs args)
        {
            bool authorized = false;
            switch (args.ActionName)
            {
                case ComponentEvents.AUTOMATION_REMOVE:
                    authorized = WorkflowStepInfoProvider.CanUserRemoveAutomationProcess(Manager.User, InfoObject.Generalized.ObjectSiteName);
                    break;

                case ComponentEvents.AUTOMATION_START:
                    authorized = WorkflowStepInfoProvider.CanUserStartAutomationProcess(Manager.User, InfoObject.Generalized.ObjectSiteName);
                    break;

                default:
                    // Other actions don't have special permission
                    authorized = true;
                    break;
            }

            if (!authorized)
            {
                args.IsValid = false;
                args.ErrorMessage = GetDefaultCheckPermissionsError(args);
            }
        }


        /// <summary>
        /// Initializes security properties
        /// </summary>
        /// <param name="storageKey">Storage key for request</param>
        private void InitializeSecurityProperties(string storageKey)
        {
            // Properties already initialized
            if (RequestStockHelper.GetStorage(storageKey, false, false) != null)
            {
                return;
            }

            bool allowSave = false;
            bool allowNext = false;
            bool allowSpec = WorkflowStepInfoProvider.CanUserMoveToSpecificAutomationStep(Manager.User, InfoObject.Generalized.ObjectSiteName);
            bool allowPrevious = false;
            bool allowStart = WorkflowStepInfoProvider.CanUserStartAutomationProcess(Manager.User, InfoObject.Generalized.ObjectSiteName);
            bool allowRemove = WorkflowStepInfoProvider.CanUserRemoveAutomationProcess(Manager.User, InfoObject.Generalized.ObjectSiteName);

            allowSave = IsAuthorized(false);
            if (allowSave)
            {
                WorkflowInfo process = Process;

                if (Mode == FormModeEnum.Update)
                {
                    // If process not null, process the process information
                    if (process != null)
                    {
                        // Get current step info
                        WorkflowStepInfo si = Step;

                        // Allow start only for finished step
                        allowStart &= si.StepIsFinished;

                        bool canMoveToNextStep = Manager.CheckStepPermissions(InfoObject, StateObject, AutomationActionEnum.MoveToNextStep);
                        if (!canMoveToNextStep)
                        {
                            allowSave = false;
                        }

                        // Next action
                        allowNext = canMoveToNextStep;

                        // Previous action
                        bool canMoveToPreviousStep = Manager.CheckStepPermissions(InfoObject, StateObject, AutomationActionEnum.MoveToPreviousStep);
                        // Step allows move to previous step and user has permissions, allow action
                        if (si.StepAllowReject && canMoveToPreviousStep)
                        {
                            allowPrevious = true;
                        }

                        if (si.StepIsAction)
                        {
                            // Action is processing
                            if (ProcessingAction)
                            {
                                allowNext = false;
                                allowPrevious = false;
                                allowRemove = false;
                                allowStart = false;
                                allowSpec = false;
                            }
                            else
                            {
                                // There are no next steps
                                if (NextSteps.Count == 0)
                                {
                                    // Hide next button
                                    allowNext = false;
                                }
                            }

                            // Disable actions if in action step
                            allowSave = false;
                        }
                    }
                }
            }


            // Add results to the request
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.SAVE), allowSave, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.AUTOMATION_MOVE_NEXT), allowNext, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.AUTOMATION_MOVE_SPEC), allowSpec, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.AUTOMATION_MOVE_PREVIOUS), allowPrevious, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.AUTOMATION_REMOVE), allowRemove, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.AUTOMATION_START), allowStart, false);
        }


        /// <summary>
        /// Checks object limitations and permissions
        /// </summary>
        /// <param name="showMessage">Indicates if info message should be shown</param>
        private bool IsAuthorized(bool showMessage)
        {
            bool authorized = true;

            // Check permissions
            SimpleObjectManagerEventArgs args = new SimpleObjectManagerEventArgs(InfoObject, Mode);
            if (!RaiseCheckPermissions(args))
            {
                authorized = false;
                if (showMessage)
                {
                    ShowError(args.ErrorMessage);
                }
            }

            return authorized;
        }

        #endregion


        #region "Consistency methods"

        /// <summary>
        /// Checks the default consistency for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override void CheckDefaultConsistency(SimpleObjectManagerEventArgs args)
        {
            if (InfoObject != null)
            {
                // Store step ID
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                    case FormModeEnum.InsertNewCultureVersion:
                        break;

                    default:
                        {
                            WorkflowStepInfo step = Manager.GetStepInfo(InfoObject, StateObject);
                            int stepId = (step != null) ? step.StepID : 0;

                            // Check integrity
                            if (stepId != CurrentStepID)
                            {
                                args.IsValid = false;
                                ShowWarning(GetString("ContentEdit.ConsistencyCheck"), null, null);
                            }
                        }
                        break;
                }
            }
            else
            {
                // Object is not initialized, do not continue
                args.IsValid = false;
            }
        }

        #endregion


        #region "Action handling"

        /// <summary>
        /// Raises the post back event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            // Check consistency
            if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, eventArgument)))
            {
                return;
            }

            if ((eventArgument == null))
            {
                return;
            }

            if (IsActionAllowed(eventArgument))
            {
                switch (eventArgument)
                {
                    case ComponentEvents.AUTOMATION_MOVE_NEXT:
                        {
                            int stepId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            MoveToNextStep(stepId, ActionComment);
                        }
                        break;

                    case ComponentEvents.AUTOMATION_MOVE_SPEC:
                        {
                            int stepId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            MoveToSpecificStep(stepId, ActionComment);
                        }
                        break;

                    case ComponentEvents.AUTOMATION_MOVE_PREVIOUS:
                        {
                            int historyId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            MoveToPreviousStep(historyId, ActionComment);
                        }
                        break;

                    case ComponentEvents.AUTOMATION_REMOVE:
                        {
                            RemoveProcess();
                        }
                        break;

                    case ComponentEvents.AUTOMATION_START:
                        {
                            int processId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            StartProcess(processId);
                        }
                        break;
                }
            }

            ClearComment();
        }


        /// <summary>
        /// Moves object to next step.
        /// </summary>
        /// <param name="stepId">Step ID (optional)</param>
        /// <param name="comment">Comment</param>
        public void MoveToNextStep(int stepId, string comment)
        {
            MoveToNextStepInternal(stepId, comment, false);
        }


        /// <summary>
        /// Moves object to specific step.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        /// <param name="comment">Comment</param>
        public void MoveToSpecificStep(int stepId, string comment)
        {
            MoveToNextStepInternal(stepId, comment, true);
        }


        /// <summary>
        /// Moves object to next step.
        /// </summary>
        /// <param name="stepId">Step ID (optional)</param>
        /// <param name="comment">Comment</param>
        /// <param name="specific">Indicates if move to specific step</param>
        protected void MoveToNextStepInternal(int stepId, string comment, bool specific)
        {
            if ((InfoObject == null) || (StateObject == null))
            {
                return;
            }

            // Check modify permissions
            string eventName = specific ? ComponentEvents.AUTOMATION_MOVE_SPEC : ComponentEvents.AUTOMATION_MOVE_NEXT;
            AutomationActionEnum action = specific ? AutomationActionEnum.MoveToSpecificStep : AutomationActionEnum.MoveToNextStep;
            
            SimpleObjectManagerEventArgs permArgs = new SimpleObjectManagerEventArgs(InfoObject, Mode, eventName);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !Manager.CheckStepPermissions(InfoObject, StateObject, action)))
            {
                AddError(permArgs.ErrorMessage, null);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = Manager.GetStepInfo(InfoObject, StateObject);
                AutomationManagerEventArgs args = new AutomationManagerEventArgs(InfoObject, StateObject, originalStep, Mode, eventName)
                {
                    Process = Process
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Go to next step
                // Get next step
                WorkflowStepInfo nextStep = null;

                if (specific)
                {
                    WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                    nextStep = Manager.MoveToSpecificStep(InfoObject, StateObject, s, comment, WorkflowTransitionTypeEnum.Manual);
                }
                else if (stepId == 0)
                {
                    nextStep = Manager.MoveToNextStep(InfoObject, StateObject, comment);
                }
                else
                {
                    WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                    nextStep = Manager.MoveToSpecificNextStep(InfoObject, StateObject, s, comment, WorkflowTransitionTypeEnum.Manual);
                }

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                // Ensure correct message is displayed
                if (nextStep != null)
                {
                    if (specific)
                    {
                        AddConfirmation(string.Format(ResHelper.GetString("ma.step.movetospecificstep", ResourceCulture), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(nextStep.StepDisplayName))), null);
                    }
                    else
                    {
                        if (nextStep.StepIsFinished)
                        {
                            AddConfirmation(ResHelper.GetString("ma.step.movetofinishstep", ResourceCulture), null);
                        }
                        else
                        {
                            AddConfirmation(ResHelper.GetString("ma.step.movetonextstep", ResourceCulture), null);
                        }
                    }
                }
                else
                {
                    AddConfirmation(ResHelper.GetString("ma.step.movetofinishstep", ResourceCulture), null);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("ma.errorduringaction", ResourceCulture);
                AddError(message, null);

                // Log error
                EventLogProvider.LogException("Automation", specific ? "MoveToSpecificStep" : "MoveToNextStep", ex, InfoObject.Generalized.ObjectSiteID, message);
            }
        }


        /// <summary>
        /// Moves object to previous step.
        /// </summary>
        /// <param name="comment">Comment</param>
        public void MoveToPreviousStep(string comment)
        {
            MoveToPreviousStep(0, comment);
        }


        /// <summary>
        /// Moves object to previous step.
        /// </summary>
        /// <param name="historyId">History ID (optional)</param>
        /// <param name="comment">Comment</param>
        public void MoveToPreviousStep(int historyId, string comment)
        {
            if ((InfoObject == null) || (StateObject == null))
            {
                return;
            }

            // Check modify permissions
            SimpleObjectManagerEventArgs permArgs = new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_MOVE_PREVIOUS);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !Manager.CheckStepPermissions(InfoObject, StateObject, AutomationActionEnum.MoveToPreviousStep)))
            {
                AddError(permArgs.ErrorMessage, null);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = Manager.GetStepInfo(InfoObject, StateObject);
                AutomationManagerEventArgs args = new AutomationManagerEventArgs(InfoObject, StateObject, originalStep, Mode, ComponentEvents.AUTOMATION_MOVE_PREVIOUS)
                {
                    Process = Process
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Move to previous step
                if (historyId == 0)
                {
                    Manager.MoveToPreviousStep(InfoObject, StateObject, comment);
                }
                else
                {
                    var history = AutomationHistoryInfoProvider.GetAutomationHistoryInfo(historyId);
                    if (history != null)
                    {
                        WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(history.HistoryStepID).Clone();
                        s.RelatedHistoryID = historyId;
                        Manager.MoveToPreviousStep(InfoObject, StateObject, s, comment);
                    }
                    else
                    {
                        throw new NullReferenceException("[CMSAutomationManager.MoveToPreviousStep]: Missing automation history.");
                    }
                }

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                AddConfirmation(ResHelper.GetString("ma.step.movetopreviousstep", ResourceCulture), null);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("ma.errorduringaction", ResourceCulture);
                AddError(message, null);
                EventLogProvider.LogException("Automation", "MoveToPreviousStep", ex, InfoObject.Generalized.ObjectSiteID, message);
            }
        }


        /// <summary>
        /// Removes process.
        /// </summary>
        public void RemoveProcess()
        {
            if ((InfoObject == null) || (StateObject == null))
            {
                return;
            }

            // Check modify permissions
            SimpleObjectManagerEventArgs permArgs = new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_REMOVE);
            if (CheckPermissions && !RaiseCheckPermissions(permArgs))
            {
                AddError(permArgs.ErrorMessage, null);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = Manager.GetStepInfo(InfoObject, StateObject);
                AutomationManagerEventArgs args = new AutomationManagerEventArgs(InfoObject, StateObject, originalStep, Mode, ComponentEvents.AUTOMATION_REMOVE)
                {
                    Process = Process
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Remove process
                Manager.RemoveProcess(InfoObject, StateObject);

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                string objectType = TypeHelper.GetNiceObjectTypeName(ObjectType);
                AddConfirmation(string.Format(ResHelper.GetString("ma.process.removed", ResourceCulture), objectType), null);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("ma.errorduringaction", ResourceCulture);
                AddError(message, null);
                EventLogProvider.LogException("Automation", "RemoveProcess", ex, InfoObject.Generalized.ObjectSiteID, message);
            }
        }


        /// <summary>
        /// Starts process.
        /// </summary>
        /// <param name="processId">Process ID</param>
        public void StartProcess(int processId)
        {
            if ((processId <= 0) && (StateObject == null))
            {
                return;
            }

            // Check modify permissions
            SimpleObjectManagerEventArgs permArgs = new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.AUTOMATION_START);
            if (CheckPermissions && !RaiseCheckPermissions(permArgs))
            {
                AddError(permArgs.ErrorMessage, null);
                return;
            }

            try
            {
                // Prepare arguments
                AutomationManagerEventArgs args = new AutomationManagerEventArgs(InfoObject, StateObject, null, Mode, ComponentEvents.AUTOMATION_START)
                {
                    Process = Process
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Start process
                int pId = (processId > 0) ? processId : StateObject.StateWorkflowID;
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.AllowAsyncActions = false;

                    Manager.StartProcess(InfoObject, pId);
                }

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                AddConfirmation(ResHelper.GetString("ma.process.restarted", ResourceCulture), null);
            }
            catch (ProcessRecurrenceException ex)
            {
                ShowError(ex.Message);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("ma.errorduringaction", ResourceCulture);
                AddError(message, null);
                EventLogProvider.LogException("Automation", "StartProcess", ex, InfoObject.Generalized.ObjectSiteID, message);
            }
        }


        /// <summary>
        /// Clears current object.
        /// </summary>
        public void ClearObject()
        {
            ClearProperties();

            UIContext.EditedObject = null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates if local messages placeholder is used
        /// </summary>
        public bool HasLocalMessagesPlaceHolder()
        {
            return (plcMess != null);
        }


        /// <summary>
        /// Shows default automation information text
        /// </summary>
        public void ShowAutomationInfo()
        {
            ShowAutomationInfo(null);
        }


        /// <summary>
        /// Shows default automation information text
        /// </summary>
        /// <param name="message">Additional message</param>
        public void ShowAutomationInfo(string message)
        {
            // Return explicitly set info message
            if (!string.IsNullOrEmpty(AutomationInfo))
            {
                return;
            }

            // Return info only for update mode
            if ((Mode != FormModeEnum.Update) || (InfoObject == null))
            {
                return;
            }

            string autoInfo = GetAutomationInfo();
            AutomationInfo = AddText(autoInfo, message);
        }


        /// <summary>
        /// Gets default automation information text
        /// </summary>
        public string GetAutomationInfo()
        {
            string automationInfo = null;

            bool authorized = true;

            // Check permissions
            SimpleObjectManagerEventArgs args = new SimpleObjectManagerEventArgs(InfoObject, Mode);
            if (!RaiseCheckPermissions(args))
            {
                authorized = false;
                automationInfo = args.ErrorMessage;
            }

            // If authorized
            if (authorized)
            {
                // If process not null, process the process information to display the items
                if (Process != null)
                {
                    bool isActionStep = (Step != null) && Step.StepIsAction;
                    bool canMoveToNextStep = Manager.CheckStepPermissions(InfoObject, StateObject, AutomationActionEnum.MoveToNextStep);

                    // Not authorized to move to next step
                    if (!canMoveToNextStep)
                    {
                        automationInfo = AddText(automationInfo, ResHelper.GetString("ma.NotAuthorizedToMoveToNextStep", ResourceCulture));
                    }

                    string objectType = TypeHelper.GetNiceObjectTypeName(ObjectType);
                    string workflow = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(Process.WorkflowDisplayName));
                    string step = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(Step.StepDisplayName));
                    automationInfo = AddText(automationInfo, String.Format(ResHelper.GetString("ma.CurrentStepInfo", ResourceCulture), objectType, workflow, step));

                    // Display action step message
                    if (isActionStep)
                    {
                        if (!ProcessingAction && (NextSteps.Count > 0))
                        {
                            // Display warning message
                            automationInfo = AddText(automationInfo, ResHelper.GetString("ma.actionwarning", ResourceCulture));
                        }
                    }
                }
            }

            return automationInfo;
        }


        /// <summary>
        /// Clears current comment
        /// </summary>
        private void ClearComment()
        {
            hdnComment.Value = "";
        }

        #endregion
    }
}
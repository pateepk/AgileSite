using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using CMS.WorkflowEngine.Definitions;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for managing the workflow procedure.
    /// </summary>
    public class WorkflowManager : AbstractWorkflowManager<TreeNode, BaseInfo, WorkflowActionEnum>
    {
        #region "Variables"

        /// <summary>
        /// Inner TreeProvider object for the data access.
        /// </summary>
        private TreeProvider mTreeProvider;

        /// <summary>
        /// Manager for versions
        /// </summary>
        private VersionManager mVersionManager;

        /// <summary>
        /// Default scopes order by expression to handle scope priorities.
        /// </summary>
        private const string mScopesOrderBy = "ScopeClassID DESC,ScopeCultureID DESC, ScopeStartingPath DESC, ScopeExcludeChildren DESC, ScopeExcluded DESC, ScopeMacroCondition DESC";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if step permissions should be checked when the step is moved.
        /// </summary>
        public override bool CheckPermissions
        {
            get
            {
                return base.CheckPermissions && WorkflowActionContext.CurrentCheckStepPermissions;
            }
            set
            {
                base.CheckPermissions = value;
            }
        }


        /// <summary>
        /// TreeProvider instance.
        /// </summary>
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Version manager instance.
        /// </summary>
        protected virtual VersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = VersionManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Default scopes order by expression to handle scope priorities.
        /// </summary>
        protected virtual string ScopesOrderBy
        {
            get
            {
                return mScopesOrderBy;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. For inheritance, use constructor WorkflowManager(tree)")]
        public WorkflowManager()
        {
            Init(null);
        }


        /// <summary>
        /// Constructor - Creates workflow manager.
        /// </summary>
        /// <param name="tree">TreeProvider instance to use</param>
        protected WorkflowManager(TreeProvider tree)
        {
            Init(tree);
        }


        /// <summary>
        /// Initializes the manager
        /// </summary>
        /// <param name="tree">Tree provider</param>
        private void Init(TreeProvider tree)
        {
            mTreeProvider = tree;
            ApplicationUrl = URLHelper.GetAbsoluteUrl("~/");
            ApplicationUrl = ApplicationUrl?.TrimEnd('/');
        }


        /// <summary>
        /// Changes the manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public override void ChangeManagerTypeTo(Type newType)
        {
            ChangeManagerType<WorkflowManager>(newType);
        }


        /// <summary>
        /// Gets the instance of the manager.
        /// </summary>
        /// <param name="tree">TreeProvider instance to use</param>
        public static WorkflowManager GetInstance(TreeProvider tree)
        {
            var wm = LoadManager<WorkflowManager>();
            wm.TreeProvider = tree;

            return wm;
        }

        #endregion


        #region "Methods to get document workflow information"

        /// <summary>
        /// Returns the workflow scope info for the given node, or null when the node does not support workflow.
        /// </summary>
        /// <param name="node">Document node</param>
        public WorkflowScopeInfo GetNodeWorkflowScope(TreeNode node)
        {
            return GetNodeWorkflowScopeInternal(node);
        }


        /// <summary>
        /// Returns first workflow step for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        public WorkflowStepInfo GetFirstWorkflowStep(TreeNode node)
        {
            return GetFirstWorkflowStepInternal(node);
        }


        /// <summary>
        /// Returns published workflow step for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        public WorkflowStepInfo GetPublishedWorkflowStep(TreeNode node)
        {
            return GetPublishedWorkflowStepInternal(node);
        }


        /// <summary>
        /// Returns the workflow for the specified node.
        /// </summary>
        /// <param name="node">Tree node to analyze for workflow</param>
        public WorkflowInfo GetNodeWorkflow(TreeNode node)
        {
            return GetNodeWorkflowInternal(node);
        }


        /// <summary>
        /// Gets step information for given document.
        /// </summary>
        /// <param name="node">Tree node</param>
        public WorkflowStepInfo GetStepInfo(TreeNode node)
        {
            return GetStepInfoInternal(node);
        }


        /// <summary>
        /// Ensures workflow step for given document.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <remarks>In case given node is under workflow but doesn't have the workflow step set yet, updates the node information.</remarks>
        public WorkflowStepInfo EnsureStepInfo(TreeNode node)
        {
            return EnsureStepInfoInternal(node);
        }


        /// <summary>
        /// Returns previous step information for given node.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        private WorkflowStepInfo GetPreviousStepInfo(TreeNode node, bool markAsUsed = false)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(node);

            // Document is not under workflow
            if (currentStep == null)
            {
                throw new Exception($"The node {node.GetDocumentName()} does not support workflow.");
            }

            // Get current step workflow
            int workflowId = currentStep.StepWorkflowID;
            WorkflowInfo workflowInfo = WorkflowInfoProvider.GetWorkflowInfo(workflowId);

            // Custom steps are not allowed or workflow automatically published changes
            if (!WorkflowInfoProvider.IsCustomStepAllowed() || workflowInfo.WorkflowAutoPublishChanges)
            {
                // Limited workflow - only basic steps
                if (currentStep.StepIsArchived)
                {
                    return WorkflowStepInfoProvider.GetPublishedStep(workflowId);
                }

                return WorkflowStepInfoProvider.GetEditStep(workflowId);
            }

            // Full workflow
            // Get previous step
            return GetPreviousStepInfoInternal(node, node, currentStep, markAsUsed);
        }


        /// <summary>
        /// Returns list of previous steps for current workflow cycle
        /// </summary>
        /// <param name="node">Tree node</param>
        public List<WorkflowStepInfo> GetPreviousSteps(TreeNode node)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(node);

            // Document is not under workflow
            if (currentStep == null)
            {
                throw new Exception($"The node {node.GetDocumentName()} does not support workflow.");
            }

            // Get current step workflow
            int workflowId = currentStep.StepWorkflowID;
            WorkflowInfo workflowInfo = WorkflowInfoProvider.GetWorkflowInfo(workflowId);

            // Custom steps are not allowed or workflow automatically published changes
            if (!WorkflowInfoProvider.IsCustomStepAllowed() || workflowInfo.WorkflowAutoPublishChanges)
            {
                List<WorkflowStepInfo> steps = new List<WorkflowStepInfo>();
                // Limited workflow - only basic steps
                steps.Add(currentStep.StepIsArchived ? WorkflowStepInfoProvider.GetPublishedStep(workflowId) : WorkflowStepInfoProvider.GetEditStep(workflowId));

                return steps;
            }

            // Full workflow
            // Get previous steps
            return GetPreviousStepsInternal(node, node, currentStep);
        }


        /// <summary>
        /// Returns list of next steps for given node.
        /// </summary>
        /// <param name="node">Node to process</param>
        public List<WorkflowStepInfo> GetNextStepInfo(TreeNode node)
        {
            // Get current step
            WorkflowStepInfo currentStep = GetStepInfo(node);

            return GetNextSteps(node, currentStep);
        }


        /// <summary>
        /// Returns list of next steps for given node.
        /// </summary>
        /// <param name="node">Node to process</param>
        public List<WorkflowStepInfo> GetNextSteps(TreeNode node)
        {
            return GetNextStepInfo(node);
        }


        /// <summary>
        /// Returns list of next steps for given node.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Workflow step</param>
        public List<WorkflowStepInfo> GetNextSteps(TreeNode node, WorkflowStepInfo step)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Document is not under workflow
            if (step == null)
            {
                throw new Exception($"The node {node.GetDocumentName()} does not support workflow.");
            }

            // Get current step workflow
            int workflowId = step.StepWorkflowID;
            WorkflowInfo workflowInfo = WorkflowInfoProvider.GetWorkflowInfo(workflowId);

            // Custom steps are not allowed or workflow automatically published changes
            if (!WorkflowInfoProvider.IsCustomStepAllowed() || workflowInfo.WorkflowAutoPublishChanges)
            {
                // Limited workflow - only basic steps
                if (step.StepIsPublished)
                {
                    return new List<WorkflowStepInfo> { WorkflowStepInfoProvider.GetArchivedStep(step.StepWorkflowID) };
                }

                return new List<WorkflowStepInfo> { WorkflowStepInfoProvider.GetPublishedStep(step.StepWorkflowID) };
            }

            // Full workflow
            return GetNextStepInfoInternal(node, step);
        }


        /// <summary>
        /// Logs specified action in the workflow history record.
        /// </summary>
        /// <param name="settings">Log settings</param>
        public void LogWorkflowHistory(WorkflowLogSettings settings)
        {
            WorkflowStepInfo sourceStep = settings.SourceStep;
            WorkflowStepInfo targetStep = settings.TargetStep;
            if ((sourceStep == null) || (targetStep == null))
            {
                throw new Exception("The source and target step must be specified!");
            }

            WorkflowHistoryInfo history = new WorkflowHistoryInfo();

            history.VersionHistoryID = settings.VersionHistoryId;

            // Source step information
            history.StepID = sourceStep.StepID;
            history.StepDisplayName = sourceStep.StepDisplayName;
            history.StepName = sourceStep.StepName;
            history.StepType = sourceStep.StepType;

            // Target step information
            history.TargetStepID = targetStep.StepID;
            history.TargetStepDisplayName = targetStep.StepDisplayName;
            history.TargetStepName = targetStep.StepName;
            history.TargetStepType = targetStep.StepType;

            if (settings.User != null)
            {
                history.ApprovedByUserID = settings.User.UserID;
            }

            history.ApprovedWhen = settings.Time;
            history.Comment = settings.Comment;
            history.WasRejected = settings.Rejected;

            history.HistoryObjectType = settings.ObjectType;
            history.HistoryObjectID = settings.ObjectID;
            // Always log automatic transition for action step
            history.HistoryTransitionType = sourceStep.StepAllowOnlyAutomaticTransitions ? WorkflowTransitionTypeEnum.Automatic : settings.TransitionType;
            history.HistoryWorkflowID = sourceStep.StepWorkflowID;

            WorkflowHistoryInfoProvider.SetWorkflowHistoryInfo(history);
        }

        #endregion


        #region "Methods to handle the steps transitions"

        /// <summary>
        /// Handles step timeout
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="user">User info</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="step">Target workflow step</param>
        public void HandleStepTimeout(TreeNode node, UserInfo user, WorkflowInfo workflow, WorkflowStepInfo currentStep, WorkflowStepInfo step)
        {
            HandleStepTimeoutInternal(node, node, user, workflow, currentStep, step);
        }


        /// <summary>
        /// Moves the specified node to the next step in the workflow and returns new step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        public WorkflowStepInfo MoveToNextStep(TreeNode node, string comment = null, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToNextStepInternal(node, comment, transitionType);
            }
        }


        /// <summary>
        /// Moves the specified node to the previous step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>    
        public WorkflowStepInfo MoveToPreviousStep(TreeNode node, string comment = null)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToPreviousStepInternal(node, null, comment);
            }
        }


        /// <summary>
        /// Moves the specified node to the specified previous step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>    
        public WorkflowStepInfo MoveToPreviousSpecificStep(TreeNode node, WorkflowStepInfo step, string comment = null)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToPreviousStepInternal(node, step, comment);
            }
        }


        /// <summary>
        /// Moves the specified node to the first step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        public WorkflowStepInfo MoveToFirstStep(TreeNode node, string comment = null)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToFirstStepInternal(node, comment);
            }
        }


        /// <summary>
        /// Automatically publishes given document. (Moves document to published step.)
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        /// <param name="increasePublishVersionNumber">Indicates if the publish version number should be increased</param>
        public WorkflowStepInfo MoveToPublishedStep(TreeNode node, string comment = null, bool increasePublishVersionNumber = true)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToPublishedStepInternal(node, comment, increasePublishVersionNumber);
            }
        }


        /// <summary>
        /// Moves the specified node to the specified next step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        public WorkflowStepInfo MoveToSpecificNextStep(TreeNode node, WorkflowStepInfo step, string comment = null, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToSpecificStepInternal(node, step, comment, true, transitionType, WorkflowActionEnum.Approve);
            }
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        public WorkflowStepInfo MoveToSpecificStep(TreeNode node, WorkflowStepInfo step, string comment = null, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            return MoveToSpecificStep(node, step, comment, true, transitionType);
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="increasePublishVersionNumber">Indicates if the publish version number should be increased</param>
        /// <param name="transitionType">Type of transition</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        public WorkflowStepInfo MoveToSpecificStep(TreeNode node, WorkflowStepInfo step, string comment, bool increasePublishVersionNumber, WorkflowTransitionTypeEnum transitionType)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveToSpecificStepInternal(node, step, comment, increasePublishVersionNumber, transitionType, WorkflowActionEnum.MoveToSpecificStep);
            }
        }


        /// <summary>
        /// Sets action status
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="status">Status string</param>
        public void SetActionStatus(TreeNode node, string status)
        {
            SetActionStatusInternal(node, node, status);
        }


        /// <summary>
        /// Gets action status
        /// </summary>
        /// <param name="node">Document node</param>
        public string GetActionStatus(TreeNode node)
        {
            return GetActionStatusInternal(node, node);
        }

        #endregion


        #region "Methods for advanced document workflow actions"

        /// <summary>
        /// Archives the document, returns the last step info.
        /// </summary>
        /// <param name="node">Document to archive</param>
        /// <param name="comment">Version comment</param>
        /// <param name="transitionType">Transition type</param>
        public WorkflowStepInfo ArchiveDocument(TreeNode node, string comment = null, WorkflowTransitionTypeEnum transitionType = WorkflowTransitionTypeEnum.Manual)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return ArchiveDocumentInternal(node, comment, transitionType);
            }
        }


        /// <summary>
        /// Publishes the document, returns the last step info.
        /// </summary>
        /// <param name="node">Document to publish</param>
        /// <param name="comment">Version comment</param>
        public WorkflowStepInfo PublishDocument(TreeNode node, string comment = null)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return PublishDocumentInternal(node, comment);
            }
        }


        /// <summary>
        /// Moves the document to the next step until the given condition is satisfied, returns the last step info.
        /// </summary>
        /// <param name="node">Document to move</param>
        /// <param name="comment">Version comment</param>
        /// <param name="condition">Condition to evaluate</param>
        public WorkflowStepInfo MoveDocument(TreeNode node, string comment, Func<WorkflowStepInfo, bool> condition)
        {
            // Lock on the document instance to ensure only single workflow action
            lock (node.Generalized.GetLockObject())
            {
                // Clear hops count
                CurrentHops = 0;

                return MoveDocumentInternal(node, comment, condition);
            }
        }

        #endregion


        #region "Methods for workflow security"

        /// <summary>
        /// Returns true if current user can approve or reject given node in its current workflow step.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="action">Workflow action</param>
        public bool CheckStepPermissions(TreeNode node, WorkflowActionEnum action)
        {
            return CheckStepPermissions(node, TreeProvider.UserInfo, action);
        }


        /// <summary>
        /// Returns true if given user can approve or reject given node in its current workflow step.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="user">Current user info</param>
        /// <param name="action">Workflow action</param>
        public bool CheckStepPermissions(TreeNode node, UserInfo user, WorkflowActionEnum action)
        {
            return CheckStepPermissionsInternal(node, user, action);
        }


        /// <summary>
        /// Indicates if user can manage workflow.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public bool CanUserManageWorkflow(UserInfo user, string siteName)
        {
            return CanUserManageWorkflowInternal(user, siteName);
        }


        /// <summary>
        /// Returns list of users who can approve node in the current workflow step. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        public InfoDataSet<UserInfo> GetUsersWhoCanApprove(TreeNode node, SourcePoint sourcePoint, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetUsersWhoCanApproveInternal(node, sourcePoint, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns list of all the users who can approve node in the current workflow step.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="assigned">Indicates if users assigned to the workflow steps should be get. Users in the generic roles are not included</param>
        /// <param name="managers">Indicates if users who have the manage workflow permission should be get</param>
        /// <param name="administrators">Indicates if users who are global administrators should be get</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        /// <returns>Returns defined role users, Global administrators and users who have the Manage workflow permission for the document</returns>
        public InfoDataSet<UserInfo> GetUsersWhoCanApprove(TreeNode node, SourcePoint sourcePoint, bool assigned, bool managers, bool administrators, string where = null, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetUsersWhoCanApproveInternal(node, sourcePoint, assigned, managers, administrators, where, orderBy, topN, columns);
        }

        #endregion


        #region "Methods for e-mail sending"

        /// <summary>
        /// Sends the workflow email for the specified document node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="action">Workflow action to process</param>
        /// <param name="comment">Action comment</param>
        public void SendWorkflowEmails(TreeNode node, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowActionEnum action, string comment)
        {
            SendWorkflowEmailsInternal(node, userInfo, originalStep, currentStep, action, comment);
        }


        /// <summary>
        /// Gets list of user e-mail addresses to send the workflow e-mail
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="action">Action</param>
        public List<string> GetWorkflowEmailRecipients(TreeNode node, WorkflowStepInfo currentStep, SourcePoint sourcePoint, WorkflowActionEnum action)
        {
            return GetWorkflowEmailRecipientsInternal(node, currentStep, sourcePoint, action);
        }

        #endregion


        #region "Internal methods to get document workflow information"

        /// <summary>
        /// Propagates tree provider
        /// </summary>
        /// <param name="node">Tree node</param>
        protected virtual void PropagateTreeProviderInternal(TreeNode node)
        {
            if (node == null)
            {
                throw new Exception("No node given.");
            }

            // Propagate tree provider
            node.TreeProvider = TreeProvider;
        }


        /// <summary>
        /// Always gets instance of current workflow step. (Also checks document for required settings.)
        /// </summary>
        /// <param name="node">Tree node</param>
        protected virtual WorkflowStepInfo GetCurrentStepInternal(TreeNode node)
        {
            // Check and setup the node
            if (node == null)
            {
                throw new Exception("No node given.");
            }

            // Document which is checked out by a user cannot be sent to the specific step
            if (node.IsCheckedOut)
            {
                throw new Exception("Given node is exclusively checked out by another user.");
            }

            // Ensure step info
            WorkflowStepInfo currentStep = EnsureStepInfo(node);
            if (currentStep == null)
            {
                throw new Exception("Missing current workflow step.");
            }

            return currentStep;
        }


        /// <summary>
        /// Returns the workflow scope info for the given node, or null when the node does not support workflow.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual WorkflowScopeInfo GetNodeWorkflowScopeInternal(TreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get the scope of original document
            node = TreeProvider.GetOriginalNode(node, TreeProvider.ALL_CULTURES);

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@NodeAliasPath", node.NodeAliasPath);
            parameters.Add("@ScopeSiteID", node.NodeSiteID);
            parameters.Add("@ScopeClassID", Convert.ToInt32(node.GetValue("NodeClassID")));

            var culture = CultureInfoProvider.GetCultureInfo(node.DocumentCulture);
            parameters.Add("@ScopeCultureID", culture?.CultureID ?? 0);

            var dataSet = new InfoDataSet<WorkflowScopeInfo>();
            parameters.FillDataSet = dataSet;

            // Get the best scope
            var totalRecords = 0;
            ConnectionHelper.ExecuteQuery("cms.workflowscope.getnodescope", parameters, null, ScopesOrderBy, 0, null, 0, 0, ref totalRecords);

            var scopes = dataSet.Items;
            if (scopes.Count <= 0)
            {
                return null;
            }

            var matchedWorkflowIds = new HashSet<int>();
            foreach (var scope in scopes)
            {
                // Exclude excluded scope
                bool excluded = scope.ScopeExcluded;
                bool match = true;
                string condition = scope.ScopeMacroCondition;
                if (!string.IsNullOrEmpty(condition))
                {
                    // Evaluate macro condition
                    var workflow = WorkflowInfoProvider.GetWorkflowInfo(scope.ScopeWorkflowID);
                    var step = WorkflowStepInfoProvider.GetFirstStep(scope.ScopeWorkflowID);
                    MacroResolver resolver = GetEvalResolverInternal(node, node, workflow, step, node.TreeProvider.UserInfo);
                    match = ValidationHelper.GetBoolean(resolver.ResolveMacros(condition), false);
                }

                if (!match)
                {
                    continue;
                }

                // Return allowed scope
                if (!excluded)
                {
                    // Workflow is not excluded
                    if (!matchedWorkflowIds.Contains(scope.ScopeWorkflowID))
                    {
                        return scope;
                    }
                }
                else
                {
                    // Store workflow ID
                    matchedWorkflowIds.Add(scope.ScopeWorkflowID);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns first workflow step for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual WorkflowStepInfo GetFirstWorkflowStepInternal(TreeNode node)
        {
            var workflow = GetNodeWorkflow(node);
            return workflow != null ? WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID) : null;
        }


        /// <summary>
        /// Returns published workflow step for given node.
        /// </summary>
        /// <param name="node">Document node</param>
        protected virtual WorkflowStepInfo GetPublishedWorkflowStepInternal(TreeNode node)
        {
            var workflow = GetNodeWorkflow(node);
            return workflow != null ? WorkflowStepInfoProvider.GetPublishedStep(workflow.WorkflowID) : null;
        }


        /// <summary>
        /// Returns the workflow for the specified node.
        /// </summary>
        /// <param name="node">Tree node to analyze for workflow</param>
        protected virtual WorkflowInfo GetNodeWorkflowInternal(TreeNode node)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get the info by the current step ID
            if (node.DocumentWorkflowStepID > 0)
            {
                var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(node.DocumentWorkflowStepID);
                if (step != null)
                {
                    return WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);
                }
            }

            // Get node scope
            var scope = GetNodeWorkflowScope(node);
            if (scope != null)
            {
                return WorkflowInfoProvider.GetWorkflowInfo(scope.ScopeWorkflowID);
            }

            return null;
        }


        /// <summary>
        /// Gets step information for given document.
        /// </summary>
        /// <param name="node">Tree node</param>
        protected virtual WorkflowStepInfo GetStepInfoInternal(TreeNode node)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            if (node.IsPublished && (node.DocumentCheckedOutVersionHistoryID <= 0))
            {
                return GetPublishedWorkflowStep(node);
            }

            if (node.DocumentWorkflowStepID > 0)
            {
                return WorkflowStepInfoProvider.GetWorkflowStepInfo(node.DocumentWorkflowStepID);
            }

            return GetFirstWorkflowStep(node);
        }


        /// <summary>
        /// Ensures workflow step for given document.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <remarks>In case given node is under workflow but doesn't have the workflow step set yet, updates the node information.</remarks>
        protected virtual WorkflowStepInfo EnsureStepInfoInternal(TreeNode node)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get workflow step
            var currentStep = GetStepInfo(node);

            // Document is not under workflow
            if (currentStep == null)
            {
                throw new Exception($"The node {node.GetDocumentName()} does not support workflow.");
            }

            if (node.DocumentWorkflowStepID > 0)
            {
                return currentStep;
            }

            // Get the published node record from the database
            TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

            // Disable TreeProvider actions
            bool originalTimeStamps = TreeProvider.UpdateTimeStamps;
            bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
            TreeProvider.UpdateTimeStamps = false;
            TreeProvider.UpdateSKUColumns = false;

            try
            {
                // Update the step information
                node.DocumentWorkflowStepID = currentStep.StepID;
                currentNode.DocumentWorkflowStepID = currentStep.StepID;
                node.DocumentIsArchived = currentStep.StepIsArchived;
                currentNode.DocumentIsArchived = currentStep.StepIsArchived;

                using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                {
                    currentNode.Update();
                }
            }
            finally
            {
                // Restore TreeProvider actions
                TreeProvider.UpdateTimeStamps = originalTimeStamps;
                TreeProvider.UpdateSKUColumns = originalUpdateSKU;
            }

            // Update search index for node
            if (DocumentHelper.IsSearchTaskCreationAllowed(currentNode))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, currentNode.GetSearchID(), currentNode.DocumentID);
            }

            return currentStep;
        }


        /// <summary>
        /// Returns previous step information for given node.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        protected override WorkflowStepInfo GetPreviousStepInfoInternal(TreeNode node, BaseInfo stateObj, WorkflowStepInfo step, bool markAsUsed)
        {
            // Get step workflow
            var workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);

            // Step doesn't allow reject
            if (!step.StepAllowReject)
            {
                return null;
            }

            // Basic workflow, use steps order
            if (workflow.IsBasic)
            {
                // Prepare the parameters
                var parameters = new QueryDataParameters();
                parameters.Add("@StepID", step.StepID);

                // Get the step data
                DataSet ds = ConnectionHelper.ExecuteQuery("cms.workflowstep.selectpreviousstep", parameters);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return new WorkflowStepInfo(ds.Tables[0].Rows[0]);
                }

                if (step.StepIsPublished)
                {
                    // If previous step wasn't found, return first step for published step
                    return WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID);
                }
            }
            // Advanced workflow, use workflow history
            else
            {
                // Get approval action to step
                var last = WorkflowHistoryInfoProvider.GetLastApprovalActionToStep(step.StepID, TreeNode.OBJECT_TYPE, node.DocumentID);
                if (last != null)
                {
                    // Manual transition, get previous step
                    if (last.HistoryTransitionType == WorkflowTransitionTypeEnum.Manual)
                    {
                        return GetPreviousStepInfoInternal(last, step.StepWorkflowID, markAsUsed);
                    }

                    // Get last manual transition
                    // Get approval action from first step
                    WorkflowHistoryInfo lastStart = WorkflowHistoryInfoProvider.GetLastApprovalActionFromFirstStep(TreeNode.OBJECT_TYPE, node.DocumentID);
                    if (lastStart == null)
                    {
                        throw new Exception("Missing workflow history from first step.");
                    }

                    // Last approval action is from first step
                    if (last.WorkflowHistoryID == lastStart.WorkflowHistoryID)
                    {
                        // Return first step
                        return GetPreviousStepInfoInternal(lastStart, step.StepWorkflowID, markAsUsed);
                    }

                    // Get last manual approval action
                    var histories = GetApprovalHistoriesInternal(last.WorkflowHistoryID, lastStart.WorkflowHistoryID, step.StepWorkflowID, TreeNode.OBJECT_TYPE, node.DocumentID, 1);
                    if (!DataHelper.DataSourceIsEmpty(histories))
                    {
                        var history = histories.Items[0];

                        return GetPreviousStepInfoInternal(history, step.StepWorkflowID, markAsUsed);
                    }

                    // Return at least the first step
                    return GetPreviousStepInfoInternal(lastStart, step.StepWorkflowID, markAsUsed);
                }

                // There is no workflow history
                if (step.StepIsPublished)
                {
                    // If previous step wasn't found, return first step for published step
                    return WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID);
                }

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
        /// Returns list of previous steps for current workflow cycle
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Current workflow step</param>
        protected override List<WorkflowStepInfo> GetPreviousStepsInternal(TreeNode node, BaseInfo stateObj, WorkflowStepInfo step)
        {
            var steps = new List<WorkflowStepInfo>();

            // Get step workflow
            var workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);

            // Step doesn't allow reject
            if (!step.StepAllowReject)
            {
                return steps;
            }

            // Basic workflow, use steps order
            if (workflow.IsBasic)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@StepID", step.StepID);

                // Get the step data
                DataSet ds = ConnectionHelper.ExecuteQuery("cms.workflowstep.selectpreviousstep", parameters);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    steps.Add(new WorkflowStepInfo(ds.Tables[0].Rows[0]));
                    return steps;
                }

                if (step.StepIsPublished)
                {
                    // If previous step wasn't found, return first step for published step
                    steps.Add(WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID));
                    return steps;
                }
            }
            // Advanced workflow, use workflow history
            else
            {
                // Get approval action to step
                var last = WorkflowHistoryInfoProvider.GetLastApprovalActionToStep(step.StepID, TreeNode.OBJECT_TYPE, node.DocumentID);
                if (last != null)
                {
                    // Get approval action from first step
                    var lastStart = WorkflowHistoryInfoProvider.GetLastApprovalActionFromFirstStep(TreeNode.OBJECT_TYPE, node.DocumentID);
                    if (lastStart == null)
                    {
                        return steps;
                    }

                    // Last approval action is from first step
                    if (last.WorkflowHistoryID == lastStart.WorkflowHistoryID)
                    {
                        // Return first step
                        steps.Add(GetPreviousStepInfoInternal(lastStart, step.StepWorkflowID, false));
                        return steps;
                    }

                    if (last.HistoryTransitionType == WorkflowTransitionTypeEnum.Manual)
                    {
                        steps.Add(GetPreviousStepInfoInternal(last, step.StepWorkflowID, false));
                    }

                    // Get manual approval actions
                    var histories = GetApprovalHistoriesInternal(last.WorkflowHistoryID, lastStart.WorkflowHistoryID, step.StepWorkflowID, TreeNode.OBJECT_TYPE, node.DocumentID, 0);
                    if (!DataHelper.DataSourceIsEmpty(histories))
                    {
                        foreach (var history in histories.Items)
                        {
                            steps.Add(GetPreviousStepInfoInternal(history, step.StepWorkflowID, false));
                        }
                    }

                    steps.Add(GetPreviousStepInfoInternal(lastStart, step.StepWorkflowID, false));
                }
                // There is no workflow history
                else
                {
                    if (step.StepIsPublished)
                    {
                        // If previous step wasn't found, return first step for published step
                        steps.Add(WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID));
                    }
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
                }
            }

            return steps;
        }


        /// <summary>
        /// Gets previous workflow step
        /// </summary>
        /// <param name="history">Workflow history</param>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="markAsUsed">Indicates if the history should be marked as used.</param>
        private WorkflowStepInfo GetPreviousStepInfoInternal(WorkflowHistoryInfo history, int workflowId, bool markAsUsed)
        {
            int stepId = history.StepID;
            int workfId = (history.HistoryWorkflowID > 0) ? history.HistoryWorkflowID : workflowId;
            var step = stepId > 0 ? WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId) : WorkflowStepInfoProvider.GetFirstStep(workfId);

            // Update history
            if (markAsUsed)
            {
                using (var ctx = new CMSActionContext())
                {
                    ctx.DisableAll();

                    history.HistoryRejected = true;
                    history.Update();
                }
            }

            // Keep related history ID
            var clone = step.Clone();
            clone.RelatedHistoryID = history.WorkflowHistoryID;

            return clone;
        }


        /// <summary>
        /// Gets approval workflow histories between two given for specified object
        /// </summary>
        /// <param name="startHistoryId">Start history ID</param>
        /// <param name="endHistoryId">End history ID</param>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="topN">Top N results</param>
        private InfoDataSet<WorkflowHistoryInfo> GetApprovalHistoriesInternal(int startHistoryId, int endHistoryId, int workflowId, string objectType, int objectId, int topN)
        {
            return WorkflowHistoryInfoProvider.GetWorkflowHistories()
                                              .WhereLessThan("WorkflowHistoryID", startHistoryId)
                                              .WhereGreaterThan("WorkflowHistoryID", endHistoryId)
                                              .WhereEqualsOrNull("HistoryTransitionType", (int)WorkflowTransitionTypeEnum.Manual)
                                              .WhereEquals("HistoryObjectType", objectType)
                                              .WhereEquals("HistoryObjectID", objectId)
                                              .WhereEquals("HistoryWorkflowID", workflowId)
                                              .WhereFalse("WasRejected")
                                              .WhereEqualsOrNull("HistoryRejected", 0)
                                              .TopN(topN)
                                              .OrderByDescending("WorkflowHistoryID")
                                              .TypedResult;
        }


        /// <summary>
        /// Returns list of next steps for given node.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Current step</param>
        protected virtual List<WorkflowStepInfo> GetNextStepInfoInternal(TreeNode node, WorkflowStepInfo step)
        {
            // Current user
            UserInfo currentUser = TreeProvider.UserInfo;
            return GetNextStepInfoInternal(node, node, step, currentUser);
        }


        /// <summary>
        /// Returns list of next steps for given object.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected override List<WorkflowStepInfo> GetNextStepInfoInternal(TreeNode node, BaseInfo stateObj, WorkflowStepInfo step, UserInfo user)
        {
            // Prepare resolver
            MacroResolver resolver = GetEvalResolverInternal(node, stateObj, WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID), step, user);
            List<WorkflowStepInfo> steps = new List<WorkflowStepInfo>();

            // Get step workflow
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);

            // Basic workflow, use steps order
            if (workflow.IsBasic)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@StepID", step.StepID);

                // Get the step data
                DataSet ds = ConnectionHelper.ExecuteQuery("cms.workflowstep.selectnextstep", parameters);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    steps.Add(new WorkflowStepInfo(ds.Tables[0].Rows[0]));
                }
            }
            // Advanced workflow, use transitions
            else
            {
                // Evaluate transitions
                List<WorkflowTransitionInfo> winTransitions = EvaluateTransitions(step, user, node.NodeSiteID, resolver);

                // Get next steps
                foreach (var transition in winTransitions)
                {
                    WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(transition.TransitionEndStepID);

                    // Keep information about related transition
                    WorkflowStepInfo clone = s.Clone();
                    clone.RelatedTransition = transition.Clone();
                    steps.Add(clone);
                }
            }

            return steps;
        }

        #endregion


        #region "Internal methods to handle the steps transitions"

        /// <summary>
        /// Moves the specified node to the next step in the workflow and returns new step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        protected virtual WorkflowStepInfo MoveToNextStepInternal(TreeNode node, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);

            // Get current workflow
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(currentStep.StepWorkflowID);

            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            // Check if there is next step available (document must be archived to move from published step to archived for basic workflow)
            if ((currentStep.StepIsPublished || currentStep.StepIsArchived) && workflow.IsBasic)
            {
                return currentStep;
            }

            // Get next steps
            List<WorkflowStepInfo> nextSteps = GetNextSteps(node, currentStep);

            // If there is no next step or multiple available steps, do not move
            if (nextSteps.Count != 1)
            {
                // No next step, return current
                return currentStep;
            }

            // Next step
            WorkflowStepInfo nextStep = nextSteps[0];

            // Check permissions
            WorkflowActionEnum action = nextStep.StepIsPublished ? WorkflowActionEnum.Publish : WorkflowActionEnum.Approve;
            if (!CheckStepPermissions(node, currentUser, action))
            {
                throw new PermissionException($"User {currentUser.UserName} is not authorized to approve the page in current step.");
            }

            // Move to specified step
            WorkflowStepInfo newStep = MoveToStepInternal(node, currentStep, nextStep, comment, transitionType);

            // Move to step without automatic transitions
            return MoveStepInternal(node, newStep, comment);
        }


        /// <summary>
        /// Moves the specified node to the previous step in the workflow and returns StepID of the new step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>    
        protected virtual WorkflowStepInfo MoveToPreviousStepInternal(TreeNode node, WorkflowStepInfo step, string comment)
        {
            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);

            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            // Reject is not allowed for edit and archived step
            if ((currentStep == null) || currentStep.StepIsEdit || currentStep.StepIsArchived)
            {
                return currentStep;
            }

            // Check permissions
            if (!CheckStepPermissions(node, currentUser, WorkflowActionEnum.Reject))
            {
                throw new PermissionException($"User {currentUser.UserName} is not authorized to reject the page in current step.");
            }

            WorkflowStepInfo previousStep = step;

            // Process within transaction
            using (var tr = new CMSTransactionScope())
            {
                // Handle the event
                using (var h = WorkflowEvents.Reject.StartEvent(node, TreeProvider))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Ensure version
                        int versionHistoryId = VersionManager.EnsureVersion(node, node.IsPublished);

                        // Get previous step if not specified
                        if (previousStep == null)
                        {
                            previousStep = GetPreviousStepInfo(node, true);
                        }
                        else
                        {
                            // Mark the reject path as used
                            MarkRejectHistoriesInternal(previousStep, currentStep, TreeNode.OBJECT_TYPE, node.DocumentID);
                        }

                        if (previousStep == null)
                        {
                            // If no previous step, return current step
                            return currentStep;
                        }

                        // Get node site
                        string siteName = node.NodeSiteName;

                        // Get the published node record from the database
                        TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

                        // Disable tree provider actions
                        bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
                        TreeProvider.UpdateSKUColumns = false;

                        try
                        {
                            // Reset workflow cycle GUID if node was published or archived (including steps behind the publish step)
                            if (((node.IsInPublishStep || node.IsArchived) && previousStep.StepIsEdit) && !TreePathUtils.AllowPermanentPreviewLink(siteName))
                            {
                                Guid newGuid = Guid.NewGuid();
                                currentNode.DocumentWorkflowCycleGUID = newGuid;
                                node.DocumentWorkflowCycleGUID = newGuid;
                            }

                            // Update version workflow information
                            VersionHistoryInfo version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                            version.VersionWorkflowStepID = previousStep.StepID;
                            version.VersionWorkflowID = previousStep.StepWorkflowID;

                            // If published version is current, remove the version from being published
                            int publishedVersionHistoryId = ValidationHelper.GetInteger(currentNode.GetValue("DocumentPublishedVersionHistoryID"), 0);
                            if ((publishedVersionHistoryId == versionHistoryId) && currentStep.StepIsPublished)
                            {
                                // Un-publish the version
                                version.ToBePublished = false;
                                version.WasPublishedFrom = DateTimeHelper.ZERO_TIME;

                                node.DocumentPublishedVersionHistoryID = 0;
                                currentNode.DocumentPublishedVersionHistoryID = 0;

                                // Clear Last published time stamp
                                node.DocumentLastPublished = DateTime.MinValue;
                                currentNode.DocumentLastPublished = DateTime.MinValue;

                                // Log synchronization task for rejecting
                                DocumentHelper.LogDocumentChange(currentNode, TaskTypeEnum.RejectDocument, TreeProvider);

                                // Remove document from search
                                if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(currentNode.NodeClassName))
                                {
                                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, currentNode.GetSearchID(), currentNode.DocumentID);
                                }
                            }

                            // Save version changes
                            VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                            // Set the document version history
                            node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                            currentNode.DocumentCheckedOutVersionHistoryID = versionHistoryId;

                            // Set the document step to the current step
                            node.DocumentWorkflowStepID = previousStep.StepID;
                            currentNode.DocumentWorkflowStepID = previousStep.StepID;

                            // Set the document archived flag
                            if (previousStep.StepIsArchived)
                            {
                                node.DocumentIsArchived = true;
                                currentNode.DocumentIsArchived = true;
                            }
                            else if (previousStep.StepIsEdit || previousStep.StepIsPublished)
                            {
                                node.DocumentIsArchived = false;
                                currentNode.DocumentIsArchived = false;
                            }

                            using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                            {
                                currentNode.Update();
                            }
                        }
                        finally
                        {
                            TreeProvider.UpdateSKUColumns = originalUpdateSKU;
                        }

                        // Prepare log settings
                        WorkflowLogSettings settings = new WorkflowLogSettings(TreeNode.OBJECT_TYPE, node.DocumentID)
                        {
                            VersionHistoryId = node.DocumentCheckedOutVersionHistoryID,
                            Comment = comment,
                            User = currentUser,
                            SourceStep = currentStep,
                            TargetStep = previousStep,
                            Rejected = true
                        };

                        // Log the history
                        LogWorkflowHistory(settings);
                    }

                    // Finalize the event
                    h.EventArguments.PreviousStep = currentStep;
                    h.FinishEvent();
                }

                // Commit the transaction
                tr.Commit();

                new DocumentEventLogger(node, EventLogSource).Log("REJECT", ResHelper.GetString("contentedit.documentwasrejected"), false);
            }

            // Get workflow info of the current step
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(currentStep.StepWorkflowID);

            // Send workflow e-mails
            if (SendEmails)
            {
                // Send e-mails asynchronously
                WorkflowEmailSender sender = new WorkflowEmailSender(node, currentUser, currentStep, previousStep, WorkflowActionEnum.Reject, comment, ApplicationUrl);
                sender.RunAsync();
            }

            // Process step action
            return HandleStepInternal(node, node, currentUser, workflow, currentStep, previousStep, comment, true);
        }


        /// <summary>
        /// Marks workflow histories as used when rejecting to specific step.
        /// </summary>
        /// <param name="previousStep">Previous workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        private void MarkRejectHistoriesInternal(WorkflowStepInfo previousStep, WorkflowStepInfo currentStep, string objectType, int objectId)
        {
            // Get approval action to step
            WorkflowHistoryInfo last = WorkflowHistoryInfoProvider.GetLastApprovalActionToStep(currentStep.StepID, objectType, objectId);
            if (last != null)
            {
                WorkflowHistoryInfoProvider.MarkRejected(last.WorkflowHistoryID, previousStep.RelatedHistoryID, previousStep.StepWorkflowID, objectType, objectId);
            }
        }


        /// <summary>
        /// Moves the specified node to the first step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        protected virtual WorkflowStepInfo MoveToFirstStepInternal(TreeNode node, string comment)
        {
            // Get node workflow
            WorkflowInfo workflow = GetNodeWorkflow(node);

            if (workflow != null)
            {
                // Get first workflow step
                WorkflowStepInfo firstStep = WorkflowStepInfoProvider.GetFirstStep(workflow.WorkflowID);
                return MoveToSpecificStepInternal(node, firstStep, comment, true, WorkflowTransitionTypeEnum.Manual, WorkflowActionEnum.Approve);
            }
            else
            {
                using (new CMSActionContext { LogEvents = false })
                {
                    // Remove remaining workflow information (node does not use workflow any more)
                    TreeProvider.ClearWorkflowInformation(node);
                    node.Update();
                }
            }

            return null;
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="stateObj">State object</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="action">Action context</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        protected override WorkflowStepInfo MoveToSpecificStepInternal(TreeNode node, BaseInfo stateObj, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, WorkflowActionEnum action)
        {
            // State object is not used
            return MoveToSpecificStepInternal(node, step, comment, true, transitionType, action);
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns workflow step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="step">Step to which should be specified document moved</param>
        /// <param name="comment">Action comment</param>
        /// <param name="increasePublishVersionNumber">Indicates if the publish version number should be increased</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="action">Action context</param>
        /// <remarks>The return step can be different than the original given step.</remarks>
        protected virtual WorkflowStepInfo MoveToSpecificStepInternal(TreeNode node, WorkflowStepInfo step, string comment, bool increasePublishVersionNumber, WorkflowTransitionTypeEnum transitionType, WorkflowActionEnum action)
        {
            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);

            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            // If no step given, do not move to the step and finish
            if (step == null)
            {
                return currentStep;
            }

            // If current step is same as the specified one do not move
            if (currentStep.StepGUID == step.StepGUID)
            {
                return currentStep;
            }

            // Prepare action context
            if (action == WorkflowActionEnum.Unknown)
            {
                action = WorkflowActionEnum.Approve;
            }

            switch (step.StepType)
            {
                case WorkflowStepTypeEnum.DocumentArchived:
                    action = WorkflowActionEnum.Archive;
                    break;

                case WorkflowStepTypeEnum.DocumentPublished:
                    action = WorkflowActionEnum.Publish;
                    break;
            }

            // Check permissions
            if (!CheckStepPermissions(node, currentUser, action))
            {
                throw new PermissionException($"User {currentUser.UserName} is not authorized to approve the page in current step.");
            }

            // Move to specified step
            step = MoveToStepInternal(node, currentStep, step, comment, increasePublishVersionNumber, transitionType, true);

            // Move to step without automatic transitions
            return MoveStepInternal(node, step, comment);
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="currentStep">Current workflow step of the document</param>
        /// <param name="step">Target workflow step of the document</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        protected virtual WorkflowStepInfo MoveToStepInternal(TreeNode node, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            return MoveToStepInternal(node, node, currentStep, step, comment, transitionType, true);
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="stateObj">State object</param>
        /// <param name="currentStep">Current workflow step of the document</param>
        /// <param name="step">Target workflow step of the document</param>
        /// <param name="comment">Action comment</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="handleActions">Indicates if step actions should be handled</param>
        protected override WorkflowStepInfo MoveToStepInternal(TreeNode node, BaseInfo stateObj, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, WorkflowTransitionTypeEnum transitionType, bool handleActions)
        {
            // State object is not used
            return MoveToStepInternal(node, currentStep, step, comment, true, transitionType, handleActions);
        }


        /// <summary>
        /// Moves the specified node to the specified step in the workflow and returns the step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="currentStep">Current workflow step of the document</param>
        /// <param name="step">Target workflow step of the document</param>
        /// <param name="comment">Action comment</param>
        /// <param name="increasePublishVersionNumber">Indicates if the publish version number should be increased</param>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="handleActions">Indicates if step actions should be handled</param>
        protected virtual WorkflowStepInfo MoveToStepInternal(TreeNode node, WorkflowStepInfo currentStep, WorkflowStepInfo step, string comment, bool increasePublishVersionNumber, WorkflowTransitionTypeEnum transitionType, bool handleActions)
        {
            // If no step given, return current step
            if (step == null)
            {
                return currentStep;
            }

            // If current step is same as the specified one do not move
            if (currentStep.StepGUID == step.StepGUID)
            {
                return currentStep;
            }

            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            // Get node site and its site name
            string siteName = node.NodeSiteName;
            bool logHistory = true;
            bool sendEmails = SendEmails;

            // Get workflow info
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);

            // Check license for advanced workflow
            if (!workflow.IsBasic)
            {
                if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
                {
                    LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.AdvancedWorkflow);
                }
            }

            // Move to edit step
            if (step.StepIsEdit)
            {
                #region "Move to edit step"

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Ensure version
                    int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;

                    // Update the version history if exists
                    if (versionHistoryId > 0)
                    {
                        VersionHistoryInfo version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                        version.VersionWorkflowStepID = step.StepID;
                        version.VersionWorkflowID = step.StepWorkflowID;
                        VersionHistoryInfoProvider.SetVersionHistoryInfo(version);
                    }

                    // Get the published node record from the database
                    TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

                    // Disable tree provider actions
                    bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
                    TreeProvider.UpdateSKUColumns = false;

                    try
                    {
                        // Reset workflow cycle GUID
                        if (!TreePathUtils.AllowPermanentPreviewLink(siteName))
                        {
                            Guid newGuid = Guid.NewGuid();
                            currentNode.DocumentWorkflowCycleGUID = newGuid;
                            node.DocumentWorkflowCycleGUID = newGuid;
                        }

                        // Set the document version history
                        node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                        currentNode.DocumentCheckedOutVersionHistoryID = versionHistoryId;

                        // Set the document step to the current step
                        node.DocumentWorkflowStepID = step.StepID;
                        currentNode.DocumentWorkflowStepID = step.StepID;

                        // Unset the document archived flag
                        node.DocumentIsArchived = false;
                        currentNode.DocumentIsArchived = false;

                        using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                        {
                            // Update document data
                            currentNode.Update();
                        }
                    }
                    finally
                    {
                        TreeProvider.UpdateSKUColumns = originalUpdateSKU;
                    }

                    // Log the history if the current step is not published or archived
                    if (!currentStep.StepIsPublished && !currentStep.StepIsArchived)
                    {
                        // Prepare log settings
                        WorkflowLogSettings settings = new WorkflowLogSettings(TreeNode.OBJECT_TYPE, node.DocumentID)
                        {
                            VersionHistoryId = node.DocumentCheckedOutVersionHistoryID,
                            Comment = comment,
                            User = currentUser,
                            SourceStep = currentStep,
                            TargetStep = step,
                            TransitionType = transitionType
                        };

                        // Log the history
                        LogWorkflowHistory(settings);
                    }

                    // Commit the transaction
                    tr.Commit();
                }

                #endregion
            }
            else if (step.StepIsArchived)
            {
                #region "Move to archived step"

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Handle the event
                    using (var h = WorkflowEvents.Archive.StartEvent(node, TreeProvider))
                    {
                        h.DontSupportCancel();

                        if (h.CanContinue())
                        {
                            int versionHistoryId = VersionManager.EnsureVersion(node, node.IsPublished);

                            // Update the version history object
                            VersionHistoryInfo version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                            version.WasPublishedTo = DateTime.Now;
                            version.ToBePublished = false;
                            version.VersionWorkflowStepID = step.StepID;
                            version.VersionWorkflowID = step.StepWorkflowID;
                            VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                            // Get the published node record from the database
                            TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

                            // Disable tree provider actions
                            bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
                            TreeProvider.UpdateSKUColumns = false;

                            try
                            {
                                // Set the document version history
                                node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                                currentNode.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                                node.DocumentPublishedVersionHistoryID = 0;
                                currentNode.DocumentPublishedVersionHistoryID = 0;

                                // Clear last published time stamp
                                node.DocumentLastPublished = DateTime.MinValue;
                                currentNode.DocumentLastPublished = DateTime.MinValue;

                                // Set the document step to the current step
                                node.DocumentWorkflowStepID = step.StepID;
                                currentNode.DocumentWorkflowStepID = step.StepID;

                                // Set the document archived flag
                                node.DocumentIsArchived = true;
                                currentNode.DocumentIsArchived = true;

                                using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                                {
                                    currentNode.Update();
                                }
                            }
                            finally
                            {
                                TreeProvider.UpdateSKUColumns = originalUpdateSKU;
                            }

                            // Remove document 
                            if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(currentNode.NodeClassName))
                            {
                                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, currentNode.GetSearchID(), node.DocumentID);
                            }

                            // Log synchronization
                            DocumentHelper.LogDocumentChange(currentNode, TaskTypeEnum.ArchiveDocument, TreeProvider);

                            // Prepare log settings
                            WorkflowLogSettings settings = new WorkflowLogSettings(TreeNode.OBJECT_TYPE, node.DocumentID)
                            {
                                VersionHistoryId = node.DocumentCheckedOutVersionHistoryID,
                                Comment = comment,
                                User = currentUser,
                                SourceStep = currentStep,
                                TargetStep = step,
                                TransitionType = transitionType
                            };

                            // Log the history
                            LogWorkflowHistory(settings);
                        }

                        // Finalize the event
                        h.EventArguments.PreviousStep = currentStep;
                        h.FinishEvent();
                    }

                    // Commit the transaction
                    tr.Commit();

                    new DocumentEventLogger(node, EventLogSource).Log("ARCHIVE", ResHelper.GetString("contentedit.documentwasarchived"), false);
                }

                #endregion
            }
            else
            {
                #region "Standard & Published step"

                // Check whether activity points should be added to the node owner
                bool addActivityPoints = (step.StepIsPublished && (CMSString.Compare(node.NodeClassName, "cms.blogpost", true) == 0) && !node.PublishedVersionExists);

                // Process within transaction
                using (var tr = new CMSTransactionScope())
                {
                    // Handle the event
                    using (var h = WorkflowEvents.Approve.StartEvent(node, TreeProvider))
                    {
                        h.DontSupportCancel();

                        if (h.CanContinue())
                        {
                            // Ensure correct version number if version will be ensured
                            bool published = !step.StepIsPublished && node.IsPublished;

                            // Ensure version
                            int versionHistoryId = VersionManager.EnsureVersion(node, published);

                            // Get document version
                            VersionHistoryInfo version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                            version.VersionWorkflowStepID = step.StepID;
                            version.VersionWorkflowID = step.StepWorkflowID;

                            bool unpublishedVersion = (version.WasPublishedTo != DateTimeHelper.ZERO_TIME);

                            // Increase version number
                            if (step.StepIsPublished)
                            {
                                // Set the version number to X.0
                                if (increasePublishVersionNumber)
                                {
                                    // Check if document uses check-in/check-out functionality
                                    bool useCheckInCheckOut = workflow.UseCheckInCheckOut(siteName);

                                    // Set version number
                                    version.VersionNumber = VersionManager.GetNewVersion(ValidationHelper.GetString(version.VersionNumber, "0.0"), true, siteName, useCheckInCheckOut);
                                }

                                // Clear value from previous actions
                                version.WasPublishedTo = DateTimeHelper.ZERO_TIME;
                            }

                            // Update version
                            VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                            // Get the published node record from the database
                            TreeNode currentNode = TreeProvider.SelectSingleNode(node.NodeID, node.DocumentCulture, false);

                            // Disable tree provider actions
                            bool originalUpdateSKU = TreeProvider.UpdateSKUColumns;
                            TreeProvider.UpdateSKUColumns = false;

                            try
                            {
                                // Set the document version history
                                node.DocumentCheckedOutVersionHistoryID = versionHistoryId;
                                currentNode.DocumentCheckedOutVersionHistoryID = versionHistoryId;

                                // Set the document step to the current step
                                node.DocumentWorkflowStepID = step.StepID;
                                currentNode.DocumentWorkflowStepID = step.StepID;

                                // Set version number
                                node.DocumentLastVersionNumber = version.VersionNumber;
                                currentNode.DocumentLastVersionNumber = version.VersionNumber;

                                if (step.StepIsPublished)
                                {
                                    // Save date when the document moved to the published step
                                    currentNode.DocumentLastPublished = DateTime.Now;

                                    // Unset the document archived flag
                                    node.DocumentIsArchived = false;
                                    currentNode.DocumentIsArchived = false;
                                }

                                using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                                {
                                    currentNode.Update();
                                }
                            }
                            finally
                            {
                                TreeProvider.UpdateSKUColumns = originalUpdateSKU;
                            }

                            // Publish document
                            if (step.StepIsPublished)
                            {
                                // Get the version to be published and publish node if scheduled
                                var nodeToBePublished = VersionManager.GetVersion(version, currentNode);
                                if (nodeToBePublished != null)
                                {
                                    // Set the version to be published in future
                                    version.ToBePublished = true;
                                    VersionHistoryInfoProvider.SetVersionHistoryInfo(version);

                                    // Get document scope
                                    var scope = GetNodeWorkflowScope(nodeToBePublished);

                                    // Publish version only if hasn't been published yet
                                    // Compare to node value, in case the 'Publish from' field is filled
                                    if ((version.WasPublishedFrom <= node.DocumentPublishFrom) || unpublishedVersion)
                                    {
                                        // If the document is scheduled for now, publish it immediately
                                        if (nodeToBePublished.DocumentPublishFrom <= DateTime.Now)
                                        {
                                            using (var context = new CMSActionContext())
                                            {
                                                // Process synchronously to include version history into the data when the workflow will be removed
                                                context.AllowAsyncActions = scope != null;

                                                VersionManager.PublishVersion(versionHistoryId, false);
                                                PropagateNodeAliasChange(node);
                                            }

                                            // Update node instances
                                            node.DocumentPublishedVersionHistoryID = versionHistoryId;
                                            nodeToBePublished.DocumentPublishedVersionHistoryID = versionHistoryId;
                                        }
                                        else
                                        {
                                            // Log synchronization
                                            DocumentHelper.LogDocumentChange(nodeToBePublished, TaskTypeEnum.PublishDocument, TreeProvider);
                                        }
                                    }
                                    else
                                    {
                                        // Do not log activity and send e-mails, if document already published
                                        sendEmails = false;
                                        addActivityPoints = false;
                                    }

                                    // If workflow scope not defined anymore, convert the document to not versioned
                                    if (scope == null)
                                    {
                                        // Remove workflow
                                        VersionManager.RemoveWorkflow(node);
                                        logHistory = false;
                                    }
                                }
                            }

                            // Log workflow history
                            if (logHistory)
                            {
                                // Prepare log settings
                                var settings = new WorkflowLogSettings(TreeNode.OBJECT_TYPE, node.DocumentID)
                                {
                                    VersionHistoryId = versionHistoryId,
                                    Comment = comment,
                                    User = currentUser,
                                    SourceStep = currentStep,
                                    TargetStep = step,
                                    TransitionType = transitionType
                                };

                                // Log the history
                                LogWorkflowHistory(settings);
                            }
                        }

                        // Finalize the event
                        h.EventArguments.PreviousStep = currentStep;
                        h.FinishEvent();
                    }

                    // Commit the transaction
                    tr.Commit();

                    if (!step.StepIsPublished)
                    {
                        new DocumentEventLogger(node, EventLogSource).Log("APPROVE", ResHelper.GetString("contentedit.documentwasapproved"), false);
                    }
                }

                // Add activity points
                if (addActivityPoints)
                {
                    // Update activity points
                    BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogPosts, node.NodeOwner, siteName, true);
                }

                #endregion
            }

            #region "Send notification e-mails"

            WorkflowActionEnum action = WorkflowActionEnum.Unknown;
            switch (step.StepType)
            {
                case WorkflowStepTypeEnum.DocumentEdit:
                    // Do not send e-mails for edit step
                    break;

                case WorkflowStepTypeEnum.DocumentPublished:
                    action = WorkflowActionEnum.Publish;
                    break;

                case WorkflowStepTypeEnum.DocumentArchived:
                    action = WorkflowActionEnum.Archive;
                    break;

                default:
                    action = WorkflowActionEnum.Approve;
                    break;
            }

            // Send workflow e-mails
            if (sendEmails && (action != WorkflowActionEnum.Unknown))
            {
                // Send e-mails asynchronously
                WorkflowEmailSender sender = new WorkflowEmailSender(node, currentUser, currentStep, step, action, comment, ApplicationUrl);
                sender.RunAsync();
            }

            #endregion

            // Process step action
            step = HandleStepInternal(node, node, currentUser, workflow, currentStep, step, comment, handleActions);

            // Return the next step
            return (logHistory ? step : null);
        }


        private static void PropagateNodeAliasChange(TreeNode node)
        {
            if (!node.ClassNodeAliasSourceDefined())
            {
                return;
            }

            var nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(node.NodeID);
            node.NodeAlias = nodeData.NodeAlias;
            node.NodeAliasPath = nodeData.NodeAliasPath;
        }


        /// <summary>
        /// Moves the specified node to the first step without automatic transition in the workflow and returns the final step.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="currentStep">Current workflow step of the document</param>
        /// <param name="comment">Action comment</param>
        /// <remarks>The return step can be different than the original target step.</remarks>
        protected virtual WorkflowStepInfo MoveStepInternal(TreeNode node, WorkflowStepInfo currentStep, string comment)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            return base.MoveStepInternal(node, null, currentStep, currentUser, comment);
        }

        #endregion


        #region "Internal methods for advanced document workflow actions"

        /// <summary>
        /// Moves document directly to published step. (Publishes the document without going through all the steps.)
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="comment">Action comment</param>
        /// <param name="increasePublishVersionNumber">Indicates if the publish version number should be increased</param>
        protected virtual WorkflowStepInfo MoveToPublishedStepInternal(TreeNode node, string comment, bool increasePublishVersionNumber)
        {
            // Get document workflow
            WorkflowInfo workflow = GetNodeWorkflow(node);

            if (workflow == null)
            {
                return null;
            }

            // Get published step and move document to this step
            WorkflowStepInfo publishStep = WorkflowStepInfoProvider.GetPublishedStep(workflow.WorkflowID);
            return MoveToSpecificStepInternal(node, publishStep, comment, increasePublishVersionNumber, WorkflowTransitionTypeEnum.Automatic, WorkflowActionEnum.Publish);
        }


        /// <summary>
        /// Archives the document, returns the last step info.
        /// </summary>
        /// <param name="node">Document to archive</param>
        /// <param name="comment">Version comment</param>
        /// <param name="transitionType">Transition type</param>
        protected virtual WorkflowStepInfo ArchiveDocumentInternal(TreeNode node, string comment, WorkflowTransitionTypeEnum transitionType)
        {
            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);
            WorkflowStepInfo archivedStep;

            // Check if next step is archived
            var steps = GetNextSteps(node, currentStep);
            if ((steps.Count > 0) && steps[0].StepIsArchived)
            {
                archivedStep = steps[0];
            }
            else if (currentStep.StepIsArchived)
            {
                return currentStep;
            }
            else
            {
                // Get default archived step
                archivedStep = WorkflowStepInfoProvider.GetArchivedStep(currentStep.StepWorkflowID);
            }

            if (archivedStep == null)
            {
                throw new Exception("Missing archive workflow step.");
            }

            // Propagate tree provider
            PropagateTreeProviderInternal(node);
            UserInfo currentUser = TreeProvider.UserInfo;

            // Check permissions
            if (!CheckStepPermissions(node, currentUser, WorkflowActionEnum.Archive))
            {
                throw new PermissionException($"User {currentUser.UserName} is not authorized to archive the page in current step.");
            }

            // Move to archive step
            WorkflowStepInfo newStep = MoveToStepInternal(node, currentStep, archivedStep, comment, transitionType);

            // Move to step without automatic transitions
            return MoveStepInternal(node, newStep, comment);
        }


        /// <summary>
        /// Publishes the document, returns the last step info.
        /// </summary>
        /// <param name="node">Document to publish</param>
        /// <param name="comment">Version comment</param>
        protected virtual WorkflowStepInfo PublishDocumentInternal(TreeNode node, string comment)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);

            // Check current step
            if (currentStep.StepIsPublished)
            {
                return currentStep;
            }

            using (new CMSActionContext { LogEvents = false })
            {
                // For archive step start new version
                if (currentStep.StepIsArchived)
                {
                    currentStep = VersionManager.CheckOut(node, node.IsPublished, true, true);
                    // Workflow hasn't been finished
                    if (currentStep != null)
                    {
                        VersionManager.CheckIn(node, null);
                    }
                }
            }

            // Make sure action steps are processed synchronously
            using (new WorkflowActionContext { ProcessActionsAsync = false })
            {
                // Approve until the step is published step
                while ((currentStep != null) && !currentStep.StepIsPublished && (CurrentHops < WorkflowHelper.MaxStepsHopsCount))
                {
                    // Get next step
                    WorkflowStepInfo nextStep = MoveToNextStepInternal(node, comment, WorkflowTransitionTypeEnum.Manual);
                    // Next step is the same
                    if ((nextStep != null) && (currentStep.StepGUID == nextStep.StepGUID))
                    {
                        return currentStep;
                    }
                    currentStep = nextStep;
                    ++CurrentHops;
                }
            }

            // Log warning
            if (CurrentHops >= WorkflowHelper.MaxStepsHopsCount)
            {
                LogHopsReachedWarningInternal(node, TreeProvider.UserInfo);
                return null;
            }

            return currentStep;
        }


        /// <summary>
        /// Moves the document to the next step until the given condition is satisfied, returns the last step info.
        /// </summary>
        /// <param name="node">Document to move</param>
        /// <param name="comment">Version comment</param>
        /// <param name="condition">Condition to evaluate</param>
        protected virtual WorkflowStepInfo MoveDocumentInternal(TreeNode node, string comment, Func<WorkflowStepInfo, bool> condition)
        {
            // Propagate tree provider
            PropagateTreeProviderInternal(node);

            // Get current step info
            WorkflowStepInfo currentStep = GetCurrentStepInternal(node);

            // Check condition
            if ((condition == null) || condition(currentStep))
            {
                return currentStep;
            }

            // Make sure action steps are processed synchronously
            using (new WorkflowActionContext { ProcessActionsAsync = false })
            {
                // Approve until the step is published step
                while ((currentStep != null) && !condition(currentStep) && (CurrentHops < WorkflowHelper.MaxStepsHopsCount))
                {
                    // Get next step
                    WorkflowStepInfo nextStep = MoveToNextStepInternal(node, comment, WorkflowTransitionTypeEnum.Manual);
                    // Next step is the same
                    if ((nextStep != null) && (currentStep.StepGUID == nextStep.StepGUID))
                    {
                        return currentStep;
                    }
                    currentStep = nextStep;
                    ++CurrentHops;
                }
            }

            // Log warning
            if (CurrentHops >= WorkflowHelper.MaxStepsHopsCount)
            {
                LogHopsReachedWarningInternal(node, TreeProvider.UserInfo);
                return null;
            }

            return currentStep;
        }

        #endregion


        #region "Internal methods for workflow security"

        /// <summary>
        /// Indicates if user can manage workflow.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserManageWorkflowInternal(UserInfo user, string siteName)
        {
            return WorkflowStepInfoProvider.CanUserManageWorkflow(user, siteName);
        }


        /// <summary>
        /// Returns true if given user can approve or reject given node in its current workflow step.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="user">User</param>
        /// <param name="action">Workflow action</param>
        protected virtual bool CheckStepPermissionsInternal(TreeNode node, UserInfo user, WorkflowActionEnum action)
        {
            if (!CheckPermissions)
            {
                // Permissions are not checked
                return true;
            }

            // If node not specified or user not specified cannot approve
            if ((node == null) || (user == null))
            {
                return false;
            }

            // Get node site name 
            string siteName = node.NodeSiteName;

            // Check global permissions
            if (CanUserManageWorkflowInternal(user, siteName))
            {
                return true;
            }

            // Only global administrators or users with manage workflow permission can archive document
            if (action == WorkflowActionEnum.Archive)
            {
                return false;
            }

            int stepId = node.DocumentWorkflowStepID;
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId) ?? GetFirstWorkflowStep(node);
            if (step == null)
            {
                throw new Exception("Given step doesn't exist!");
            }

            // Only global administrators or users with manage workflow can reject document from published step
            if (step.StepIsPublished && (action == WorkflowActionEnum.Reject))
            {
                return false;
            }

            // Other default steps can be managed by anyone
            if (step.StepIsDefault)
            {
                return true;
            }

            // Action step cannot be managed
            if (step.StepIsAction)
            {
                return false;
            }

            // The workflow action is not passed, its for customization purpose
            return WorkflowStepInfoProvider.CanUserApprove(user, step, node.NodeSiteID);
        }


        /// <summary>
        /// Returns list of users who can approve node in the current workflow step. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        protected virtual InfoDataSet<UserInfo> GetUsersWhoCanApproveInternal(TreeNode node, SourcePoint sourcePoint, string where, string orderBy, int topN, string columns)
        {
            // If node not specified do not process
            if (node == null)
            {
                return null;
            }

            // Get node workflow step
            int stepId = node.DocumentWorkflowStepID;
            WorkflowStepInfo step = null;
            if (stepId > 0)
            {
                step = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
            }

            return WorkflowStepInfoProvider.GetUsersWhoCanApprove(step, sourcePoint, node.NodeSiteID, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns list of all the users who can approve node in the current workflow step.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="assigned">Indicates if users assigned to the workflow steps should be get. Users in the generic roles are not included</param>
        /// <param name="managers">Indicates if users who have the manage workflow permission should be get</param>
        /// <param name="administrators">Indicates if users who are global administrators should be get</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        /// <returns>Returns defined role users, Global administrators and users who have the Manage workflow permission for the document</returns>
        protected virtual InfoDataSet<UserInfo> GetUsersWhoCanApproveInternal(TreeNode node, SourcePoint sourcePoint, bool assigned, bool managers, bool administrators, string where, string orderBy, int topN, string columns)
        {
            // Role users
            InfoDataSet<UserInfo> result = null;

            // Ensure union column
            columns = SqlHelper.MergeColumns(columns, "UserID");

            // Include condition for allowed cultures
            var usersWhere = new WhereCondition(where).Where(GetUsersAllowedToEditCultureWhereCondition(node.DocumentCulture)).ToString(true);

            // Assigned users
            if (assigned)
            {
                result = GetUsersWhoCanApprove(node, sourcePoint, usersWhere, orderBy, topN, columns);
            }

            // Manage workflow permission
            if (managers)
            {
                InfoDataSet<UserInfo> manageDS = UserInfoProvider.GetRequiredResourceUsers("CMS.Content", "manageworkflow", node.NodeSiteName, usersWhere, orderBy, topN, columns);
                result = (InfoDataSet<UserInfo>)DataHelper.Union(result, manageDS, "UserID");
            }

            // Global administrators
            if (administrators)
            {
                var adminUsersCondition = new WhereCondition()
                   .WhereGreaterOrEquals("UserPrivilegeLevel", UserPrivilegeLevelEnum.Admin)
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


        /// <summary>
        /// Returns the where condition to get only users who are allowed to edit given culture. Doesn't take into account global administrators.
        /// </summary>
        /// <param name="culture">Culture to edit</param>
        private WhereCondition GetUsersAllowedToEditCultureWhereCondition(string culture)
        {
            return new WhereCondition()
                    .Where("(NULLIF(UserHasAllowedCultures, 0) IS NULL)")
                    .Or()
                    .WhereIn("UserID", new IDQuery<UserCultureInfo>("UserID")
                                                .WhereIn("CultureID", new IDQuery<CultureInfo>()
                                                                            .WhereEquals("CultureCode", culture)));
        }

        #endregion


        #region "Internal methods for e-mail sending"

        /// <summary>
        /// Sends the workflow email for the specified document node.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="action">Workflow action to process</param>
        /// <param name="comment">Action comment</param>
        protected virtual void SendWorkflowEmailsInternal(TreeNode node, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowActionEnum action, string comment)
        {
            // Check if e-mails should be sent
            if (!SendEmails)
            {
                return;
            }

            // Do not send e-mails for unknown action
            if (action == WorkflowActionEnum.Unknown)
            {
                return;
            }

            // Check the User
            if (userInfo == null)
            {
                throw new Exception("No user specified.");
            }

            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (si == null)
            {
                throw new Exception("Node site not found.");
            }

            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(originalStep.StepWorkflowID);
            MacroResolver resolver = GetEmailResolverInternal(node, userInfo, originalStep, currentStep, workflow, WorkflowHelper.GetWorkflowActionString(action), comment);

            string editorEmail = GetEditorEmailInternal(node, originalStep);
            string moderatorEmail = userInfo.Email.ToLowerInvariant();
            bool editorEmailSent = false;
            List<string> recipients = new List<string>();
            KeyValuePair<string, string> emailTemplate = GetEmailTemplateInternal(workflow, currentStep, action, false, false, resolver);

            // Prepare e-mail settings
            WorkflowEmailSettings settings = new WorkflowEmailSettings(userInfo)
            {
                Resolver = resolver,
                EmailTemplateName = emailTemplate.Key,
                LogEvents = TreeProvider.LogEvents,
                SiteName = si.SiteName,
                DefaultSubject = emailTemplate.Value,
            };

            // Get e-mail type
            WorkflowEmailTypeEnum emailType;
            switch (action)
            {
                case WorkflowActionEnum.Publish:
                    emailType = WorkflowEmailTypeEnum.Published;
                    break;

                case WorkflowActionEnum.Archive:
                    emailType = WorkflowEmailTypeEnum.Archived;
                    break;

                case WorkflowActionEnum.Reject:
                    emailType = WorkflowEmailTypeEnum.Rejected;
                    break;

                default:
                    emailType = WorkflowEmailTypeEnum.ReadyForApproval;
                    break;
            }

            if (currentStep.SendEmails(si.SiteName, emailType))
            {
                // Send e-mail to all recipients due to step settings
                List<SourcePoint> points;
                if (workflow.IsBasic || (currentStep.StepDefinition.SourcePoints.Count == 0))
                {
                    // Get dummy point if no source points defined
                    points = new List<SourcePoint> { null };
                }
                else
                {
                    // Prepare resolver
                    MacroResolver pointsResolver = GetEvalResolverInternal(node, node, workflow, currentStep, userInfo);

                    // Get possible source points and next steps due to conditions
                    points = EvaluateSourcePointsInternal(currentStep, currentStep.StepDefinition.SourcePoints, pointsResolver);
                }

                // Send e-mails for each source point with transition (next step)
                foreach (var point in points)
                {
                    // Get recipients due to security settings
                    List<string> pointRecipients = GetWorkflowEmailRecipients(node, currentStep, point, action);
                    // Get only new e-mails
                    pointRecipients = pointRecipients.Except(recipients).ToList();
                    // Store new e-mails for next iteration
                    recipients = recipients.Union(pointRecipients).ToList();

                    // If set, the moderator e-mail is excluded
                    if (!SendEmailToModerator)
                    {
                        pointRecipients.Remove(moderatorEmail);
                    }

                    // Check if editor e-mail was sent
                    if (pointRecipients.Contains(editorEmail))
                    {
                        editorEmailSent = true;
                    }

                    // Send notification e-mails
                    settings.Recipients = pointRecipients;
                    SendWorkflowEmails(settings);
                }
            }

            // Send e-mail to the editor if not already sent and for approve action the approved notification e-mail should be sent
            WorkflowEmailTypeEnum editorEmailType = (action == WorkflowActionEnum.Approve) ? WorkflowEmailTypeEnum.Approved : emailType;
            bool sendEditorEmail = currentStep.SendEmails(si.SiteName, editorEmailType);
            if (sendEditorEmail && !string.IsNullOrEmpty(editorEmail) && !editorEmailSent && (editorEmail != moderatorEmail))
            {
                KeyValuePair<string, string> approvedTemplate = GetEmailTemplateInternal(workflow, currentStep, action, true, false, resolver);
                settings.EmailTemplateName = approvedTemplate.Key;
                settings.DefaultSubject = approvedTemplate.Value;

                // Send notification e-mails
                settings.Recipients = new List<string> { editorEmail };
                recipients.Add(editorEmail);
                SendWorkflowEmails(settings);
            }

            // Send notification e-mail to explicitly selected users
            if (currentStep.SendEmails(si.SiteName, WorkflowEmailTypeEnum.Notification))
            {
                // Get only new e-mails
                recipients = GetNotificationEmailRecipientsInternal(node, currentStep, action).Except(recipients).ToList();

                // If set, the moderator e-mail is excluded
                if (!SendEmailToModerator)
                {
                    recipients.Remove(moderatorEmail);
                }

                KeyValuePair<string, string> notificationTemplate = GetEmailTemplateInternal(workflow, currentStep, action, true, true, resolver);
                settings.EmailTemplateName = notificationTemplate.Key;
                settings.DefaultSubject = notificationTemplate.Value;

                // Send notification e-mails
                settings.Recipients = recipients;
                SendWorkflowEmails(settings);
            }
        }


        /// <summary>
        /// Gets e-mail template name and default subject to be used for given action
        /// </summary>
        /// <param name="workflow">Workflow</param>
        /// <param name="step">Target workflow step</param>
        /// <param name="action">Workflow action</param>
        /// <param name="toEditor">Indicates if e-mail template should be used to send e-mail to editor</param>
        /// <param name="notification">Indicates if general notification template should be returned</param>
        /// <param name="resolver">Macro resolver</param>
        /// <returns>E-mail template name and default subject as a pair. Key represents the template name, value represents default subject.</returns>
        protected virtual KeyValuePair<string, string> GetEmailTemplateInternal(WorkflowInfo workflow, WorkflowStepInfo step, WorkflowActionEnum action, bool toEditor, bool notification, MacroResolver resolver)
        {
            string defaultTemplateName;
            string subject;

            if (notification)
            {
                // General notification template
                subject = ResHelper.GetAPIString("Workflow.SubjectNotification", "Page '{%DocumentName%}' status changed");
                defaultTemplateName = workflow.GetEmailTemplateName(WorkflowEmailTypeEnum.Notification);
            }
            else
            {
                // Get e-mail subject
                switch (action)
                {
                    // The document was archived
                    case WorkflowActionEnum.Archive:
                        subject = ResHelper.GetAPIString("Workflow.SubjectArchived", "Page '{%DocumentName%}' archived");
                        defaultTemplateName = step.GetEmailTemplateName(WorkflowEmailTypeEnum.Archived);
                        break;

                    // The document was approved
                    case WorkflowActionEnum.Approve:
                        if (!toEditor)
                        {
                            subject = ResHelper.GetAPIString("Workflow.SubjectReadyForApproval", "Page '{%DocumentName%}' is waiting for approval");
                            defaultTemplateName = step.GetEmailTemplateName(WorkflowEmailTypeEnum.ReadyForApproval);
                        }
                        else
                        {
                            subject = ResHelper.GetAPIString("Workflow.SubjectApproved", "Page '{%DocumentName%}' approved");
                            defaultTemplateName = step.GetEmailTemplateName(WorkflowEmailTypeEnum.Approved);
                        }
                        break;

                    // The document was published
                    case WorkflowActionEnum.Publish:
                        subject = ResHelper.GetAPIString("Workflow.SubjectPublished", "Page '{%DocumentName%}' published");
                        defaultTemplateName = step.GetEmailTemplateName(WorkflowEmailTypeEnum.Published);
                        break;

                    // The document was rejected
                    case WorkflowActionEnum.Reject:
                        subject = ResHelper.GetAPIString("Workflow.SubjectRejected", "Page '{%DocumentName%}' rejected");
                        defaultTemplateName = step.GetEmailTemplateName(WorkflowEmailTypeEnum.Rejected);
                        break;

                    default:
                        throw new Exception("Unknown workflow action.");
                }
            }

            // Resolve macros
            defaultTemplateName = resolver.ResolveMacros(defaultTemplateName);
            return new KeyValuePair<string, string>(defaultTemplateName, subject);
        }


        /// <summary>
        /// Gets the document editor e-mail
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="step">Workflow step</param>
        /// <remarks>The document editor is a user who approved the document from the Edit step for the last time.</remarks>
        protected virtual string GetEditorEmailInternal(TreeNode node, WorkflowStepInfo step)
        {
            var editStep = WorkflowStepInfoProvider.GetEditStep(step.StepWorkflowID);
            if (editStep == null)
            {
                return string.Empty;
            }

            // Get last approval action from edit step
            var editorEmail = string.Empty;
            var lastApproval = WorkflowHistoryInfoProvider.GetLastApprovalActionFromStep(editStep.StepID, TreeNode.OBJECT_TYPE, node.DocumentID);
            if (lastApproval != null && lastApproval.ApprovedByUserID > 0)
            {
                var where = "UserID = " + lastApproval.ApprovedByUserID + " AND " + UserInfoProvider.USER_ENABLED_WHERE_CONDITION + " AND Email IS NOT NULL AND Email <>''";
                editorEmail = UserInfoProvider.GetUsersDataWithSettings().Where(new WhereCondition(where)).TopN(1).Column("Email").GetScalarResult(string.Empty).Trim().ToLowerInvariant();
            }

            return editorEmail;
        }


        /// <summary>
        /// Gets list of user e-mail addresses to send the workflow e-mail
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="action">Action</param>
        protected virtual List<string> GetWorkflowEmailRecipientsInternal(TreeNode node, WorkflowStepInfo currentStep, SourcePoint sourcePoint, WorkflowActionEnum action)
        {
            // Columns to get
            const string columns = "UserID, Email";

            var condition = new WhereCondition(UserInfoProvider.USER_ENABLED_WHERE_CONDITION).Where("NULLIF(Email, '') IS NOT NULL");
            string where = condition.ToString(true);

            InfoDataSet<UserInfo> recipientsDataSet = null;

            // Prepare the data
            switch (action)
            {
                // The document was archived
                case WorkflowActionEnum.Archive:
                    break;

                // The document was approved
                case WorkflowActionEnum.Approve:
                    // Add all users that can approve the document within current step
                    recipientsDataSet = GetUsersWhoCanApprove(node, sourcePoint, true, true, false, where, null, 0, columns);
                    break;

                // The document was published
                case WorkflowActionEnum.Publish:
                    break;

                // The document was rejected
                case WorkflowActionEnum.Reject:
                    // Get last approval action from current step
                    if (currentStep != null)
                    {
                        var lastApproval = currentStep.RelatedHistoryID > 0 ?
                            WorkflowHistoryInfoProvider.GetWorkflowHistoryInfo(currentStep.RelatedHistoryID) :
                            WorkflowHistoryInfoProvider.GetLastApprovalActionFromStep(currentStep.StepID, TreeNode.OBJECT_TYPE, node.DocumentID);
                        if (lastApproval != null && lastApproval.ApprovedByUserID > 0)
                        {
                            // Add user that approved the document last
                            var whereCondition = SqlHelper.AddWhereCondition("UserID = " + lastApproval.ApprovedByUserID, where);
                            recipientsDataSet = UserInfoProvider.GetUsersDataWithSettings().Where(new WhereCondition(whereCondition)).Columns(columns).TypedResult;
                        }
                    }
                    break;

                default:
                    throw new Exception("Unknown workflow action.");
            }

            // Get list of e-mails
            return GetDistinctEmailList(recipientsDataSet)?.ToList();
        }


        /// <summary>
        /// Gets list of user e-mail addresses to send the general notification e-mail
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="action">Action</param>
        protected virtual List<string> GetNotificationEmailRecipientsInternal(TreeNode node, WorkflowStepInfo currentStep, WorkflowActionEnum action)
        {
            // Columns to get
            string where = UserInfoProvider.USER_ENABLED_WHERE_CONDITION + " AND Email IS NOT NULL AND Email <>''";

            // Join users from workflow settings
            string workflowWhere = SqlHelper.AddWhereCondition(where, $"UserID IN (SELECT UserID FROM CMS_WorkflowUser WHERE WorkflowID = {currentStep.StepWorkflowID})");
            var assigned = UserInfoProvider.GetUsersDataWithSettings().Where(new WhereCondition(workflowWhere)).Columns("UserID", "Email");

            // Get list of e-mails
            return GetDistinctEmailList(assigned)?.ToList();
        }


        private IEnumerable<string> GetDistinctEmailList(IEnumerable<UserInfo> users)
        {
            return users?.Select(i => i.Email.Trim().ToLowerInvariant()).Distinct() ?? new List<string>();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets action status
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="stateObj">State object</param>
        /// <param name="status">Workflow status</param>
        protected override void SetActionStatusInternal(TreeNode node, BaseInfo stateObj, string status)
        {
            if (node.DocumentWorkflowActionStatus == status)
            {
                return;
            }

            node.DocumentWorkflowActionStatus = status;

            using (CMSActionContext ctx = new CMSActionContext())
            {
                ctx.DisableAll();

                DocumentCultureDataInfoProvider.BulkUpdateData(
                    new WhereCondition().WhereEquals("DocumentID", node.DocumentID),
                    new Dictionary<string, object>
                    {
                        { "DocumentWorkflowActionStatus", status }
                    }
                );
            }
        }


        /// <summary>
        /// Gets action status
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="stateObj">State object</param>
        protected override string GetActionStatusInternal(TreeNode node, BaseInfo stateObj)
        {
            return node?.DocumentWorkflowActionStatus;
        }


        /// <summary>
        /// Gets resolver for evaluation of transitions and source points
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="stateObj">State object</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="step">Workflow step</param>
        /// <param name="user">User</param>
        protected override MacroResolver GetEvalResolverInternal(TreeNode node, BaseInfo stateObj, WorkflowInfo workflow, WorkflowStepInfo step, UserInfo user)
        {
            MacroResolver resolver = GetBasicResolverInternal(workflow, step, user);
            resolver.SetAnonymousSourceData(node);

            // Add named sources
            resolver.SetNamedSourceData("Document", node);

            return resolver;
        }


        /// <summary>
        /// Get resolver for e-mail sending.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="stateObj">State object</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="action">Workflow action string representation</param>
        /// <param name="comment">Action comment</param>
        public override MacroResolver GetEmailResolver(TreeNode node, BaseInfo stateObj, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowInfo workflow, string action, string comment)
        {
            return GetEmailResolverInternal(node, userInfo, originalStep, currentStep, workflow, action, comment);
        }


        /// <summary>
        /// Get resolver for e-mail sending.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="userInfo">User info that performed the action</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="workflow">Workflow</param>
        /// <param name="action">Workflow action string representation</param>
        /// <param name="comment">Action comment</param>
        protected virtual MacroResolver GetEmailResolverInternal(TreeNode node, UserInfo userInfo, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowInfo workflow, string action, string comment)
        {
            // Prepare the macro resolver
            MacroResolver resolver = GetBasicResolverInternal(workflow, currentStep, userInfo);

            // Add named sources
            resolver.SetNamedSourceData("Document", node);
            resolver.SetNamedSourceData("OriginalStep", originalStep);

            // Prepare the e-mail replacements - backwards compatibility
            currentStep = currentStep ?? GetPublishedStep(originalStep.StepWorkflowID);

            resolver.SetNamedSourceData(new Dictionary<string, object>
            {
                { "applicationurl", ApplicationUrl },
                { "approvedby", userInfo.Email },
                { "approvedwhen", DateTime.Now.ToString() },
                { "originalstepname", originalStep.StepDisplayName },
                { "currentstepname", currentStep.StepDisplayName },
                { "comment", comment },
                { "firstname", userInfo.FirstName },
                { "lastname", userInfo.LastName },
                { "username", userInfo.UserName },
                { "email", userInfo.Email },
                { "fullname", userInfo.FullName },
                { "documentpreviewurl", userInfo.IsPublic() ? null : URLHelper.GetAbsoluteUrl(node.GetPreviewLink(userInfo.UserName, node.IsFile(), embededInAdministration: false), null, ApplicationUrl, null) },
                { "documentediturl", URLHelper.GetAbsoluteUrl(String.Format("~/Admin/cmsadministration.aspx?action=edit&nodeid={0}&culture={1}" + ApplicationUrlHelper.GetApplicationHash("cms.content", "content"), node.NodeID, node.DocumentCulture), null, ApplicationUrl, null) },
                { "documentactionname", action }
            }, isPrioritized: false);

            // Data
            object[] data = { node, userInfo, currentStep };
            resolver.SetAnonymousSourceData(data);

            return resolver;
        }


        /// <summary>
        /// Returns published step of the specified workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <exception cref="Exception">Specified workflow does not have a 'published' step.</exception>
        private static WorkflowStepInfo GetPublishedStep(int workflowId)
        {
            // Get workflow published step
            WorkflowStepInfo publishedStep = WorkflowStepInfoProvider.GetPublishedStep(workflowId);
            if (publishedStep == null)
            {
                throw new Exception("Missing workflow published step.");
            }

            return publishedStep;
        }


        /// <summary>
        /// Processes action connected to given step.
        /// </summary>
        /// <param name="arguments">Action arguments</param>
        protected override void ProcessActionInternal(WorkflowActionEventArgs<TreeNode, BaseInfo, WorkflowActionEnum> arguments)
        {
            // Handle the event
            using (var h = WorkflowEvents.Action.StartEvent(arguments))
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

        #endregion
    }
}
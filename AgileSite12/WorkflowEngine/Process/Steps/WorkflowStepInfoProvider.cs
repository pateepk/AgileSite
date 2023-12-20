using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine.Definitions;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow step info provider.
    /// </summary>
    public class WorkflowStepInfoProvider : AbstractInfoProvider<WorkflowStepInfo, WorkflowStepInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowStepInfoProvider()
            : base(WorkflowStepInfo.TYPEINFO, new HashtableSettings
            {
                ID = true
            })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WorkflowStepInfo GetWorkflowStepInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets the specified workflow step data.
        /// </summary>
        /// <param name="infoObj">Workflow step data object</param>
        public static void SetWorkflowStepInfo(WorkflowStepInfo infoObj)
        {
            if ((infoObj != null) && (infoObj.StepID <= 0))
            {
                // Check license
                if (!LicenseVersionCheck(infoObj))
                {
                    throw new LicenseException(ResHelper.GetStringFormat("licenselimitation.infopagemessage", FeatureEnum.WorkflowVersioning));
                }
            }

            ProviderObject.SetWorkflowStepInfoInternal(infoObj);
        }


        /// <summary>
        /// Returns the WorkflowStepInfo structure for the specified workflow step ID.
        /// </summary>
        /// <param name="stepId">ID of the workflow step to retrieve</param>
        public static WorkflowStepInfo GetWorkflowStepInfo(int stepId)
        {
            return ProviderObject.GetInfoById(stepId);
        }


        /// <summary>
        /// Returns the WorkflowStepInfo structure for the specified workflow step name and workflow.
        /// </summary>
        /// <param name="stepName">Step name to retrieve</param>
        /// <param name="workflowId">Workflow id of the step</param>
        public static WorkflowStepInfo GetWorkflowStepInfo(string stepName, int workflowId)
        {
            return ProviderObject.GetWorkflowStepInfoInternal(stepName, workflowId);
        }


        /// <summary>
        /// Deletes the specified workflow step.
        /// </summary>
        /// <param name="wsi">Workflow step object to delete</param>
        public static void DeleteWorkflowStepInfo(WorkflowStepInfo wsi)
        {
            ProviderObject.DeleteInfo(wsi);
        }


        /// <summary>
        /// Deletes the specified workflow step.
        /// </summary>
        /// <param name="stepId">Workflow step ID to delete</param>
        public static void DeleteWorkflowStepInfo(int stepId)
        {
            WorkflowStepInfo wsi = GetWorkflowStepInfo(stepId);
            DeleteWorkflowStepInfo(wsi);
        }


        /// <summary>
        /// Initializes the workflow step orders.
        /// </summary>
        /// <param name="workflow">Workflow</param>
        public static void InitStepOrders(WorkflowInfo workflow)
        {
            ProviderObject.InitStepOrdersInternal(workflow);
        }


        /// <summary>
        /// Moves step up (decreases its order).
        /// </summary>
        /// <param name="stepInfo">Workflow step data object</param>
        public static void MoveStepUp(WorkflowStepInfo stepInfo)
        {
            ProviderObject.MoveStepUpInternal(stepInfo);
        }


        /// <summary>
        /// Moves step down (increases its order).
        /// </summary>
        /// <param name="stepInfo">Workflow step data object</param>
        public static void MoveStepDown(WorkflowStepInfo stepInfo)
        {
            ProviderObject.MoveStepDownInternal(stepInfo);
        }


        /// <summary>
        /// Returns the DataSet of roles assigned to given step.
        /// </summary>
        /// <param name="stepId">Step ID</param>
        public static InfoDataSet<RoleInfo> GetStepRoles(int stepId)
        {
            return RoleInfoProvider.GetRoles().Where("RoleID IN (SELECT RoleID FROM CMS_WorkflowStepRoles WHERE StepID = " + stepId + ")").TypedResult;
        }


        /// <summary>
        /// Returns the edit step for the given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        public static WorkflowStepInfo GetEditStep(int workflowId)
        {
            return ProviderObject.GetEditStepInternal(workflowId);
        }


        /// <summary>
        /// Returns the published step for the given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        public static WorkflowStepInfo GetPublishedStep(int workflowId)
        {
            return ProviderObject.GetPublishedStepInternal(workflowId);
        }


        /// <summary>
        /// Returns the archived step for the given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        public static WorkflowStepInfo GetArchivedStep(int workflowId)
        {
            return ProviderObject.GetArchivedStepInternal(workflowId);
        }


        /// <summary>
        /// Returns first workflow step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        public static WorkflowStepInfo GetFirstStep(int workflowId)
        {
            return ProviderObject.GetFirstStepInternal(workflowId);
        }


        /// <summary>
        /// Returns the finished step for the given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        public static WorkflowStepInfo GetFinishedStep(int workflowId)
        {
            return ProviderObject.GetFinishedStepInternal(workflowId);
        }


        /// <summary>
        /// Returns the Dataset of workflow steps.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        public static ObjectQuery<WorkflowStepInfo> GetWorkflowSteps(int workflowId)
        {
            return GetWorkflowSteps().Where("StepWorkflowID", QueryOperator.Equals, workflowId);
        }


        /// <summary>
        /// Returns the query for all workflow steps.
        /// </summary>
        public static ObjectQuery<WorkflowStepInfo> GetWorkflowSteps()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Creates default workflow steps for the given workflow.
        /// </summary>
        /// <param name="workflow">Workflow</param>
        public static void CreateDefaultWorkflowSteps(WorkflowInfo workflow)
        {
            ProviderObject.CreateDefaultWorkflowStepsInternal(workflow);
        }


        /// <summary>
        /// Deletes all the steps of specified workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        public static void DeleteWorkflowSteps(int workflowId)
        {
            // Get the steps
            DataSet stepsDS = GetWorkflowSteps(workflowId);
            if (!DataHelper.DataSourceIsEmpty(stepsDS))
            {
                foreach (DataRow dr in stepsDS.Tables[0].Rows)
                {
                    WorkflowStepInfo wsi = new WorkflowStepInfo(dr);
                    DeleteWorkflowStepInfo(wsi);
                }
            }
        }


        /// <summary>
        /// Checks if WorkFlowStep fulfill license limitations.
        /// </summary>
        /// <param name="wfsi">Step info to check</param>
        public static bool LicenseVersionCheck(WorkflowStepInfo wfsi)
        {
            if (wfsi == null)
            {
                return true;
            }

            // Get the scopes from the DB
            DataSet ds = WorkflowScopeInfoProvider.GetWorkflowScopes(wfsi.StepWorkflowID);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Check if workflow is not assigned to site not supporting full workflow
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    WorkflowScopeInfo scope = new WorkflowScopeInfo(dr);
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(scope.ScopeSiteID);
                    if (si != null)
                    {
                        // Get limitations
                        int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(si.DomainName, FeatureEnum.WorkflowVersioning, false);

                        // No other version than unlimited supports creating steps
                        if (versionLimitations != LicenseHelper.LIMITATIONS_UNLIMITED)
                        {
                            // License limit reached, log event
                            string message = ResHelper.GetString("licenselimitation.workflow");
                            EventLogProvider.LogEvent(EventType.WARNING, "Workflow", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, RequestContext.CurrentURL);

                            return false;
                        }
                    }
                }
            }

            // All scopes ok
            return true;
        }


        /// <summary>
        /// Returns list of users who can approve given step source point. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        public static InfoDataSet<UserInfo> GetUsersWhoCanApprove(WorkflowStepInfo step, SourcePoint sourcePoint, int siteId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetUsersWhoCanApproveInternal(step, sourcePoint, siteId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns true if given user can approve or reject given step.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="step">Workflow step</param>
        /// <param name="siteId">Site ID</param>
        public static bool CanUserApprove(UserInfo user, WorkflowStepInfo step, int siteId)
        {
            return ProviderObject.CanUserApproveInternal(user, step, null, siteId);
        }


        /// <summary>
        /// Returns true if given user can approve or reject given step.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="step">Workflow step</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="siteId">Site ID</param>
        public static bool CanUserApprove(UserInfo user, WorkflowStepInfo step, SourcePoint sourcePoint, int siteId)
        {
            return ProviderObject.CanUserApproveInternal(user, step, sourcePoint, siteId);
        }


        /// <summary>
        /// Indicates if user can manage workflow.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public static bool CanUserManageWorkflow(UserInfo user, string siteName)
        {
            return ProviderObject.CanUserManageWorkflowInternal(user, siteName);
        }


        /// <summary>
        /// Indicates if user can manage automation.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public static bool CanUserManageAutomationProcesses(UserInfo user, string siteName)
        {
            return ProviderObject.CanUserManageAutomationProcessesInternal(user, siteName);
        }


        /// <summary>
        /// Indicates if user can start automation process.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public static bool CanUserStartAutomationProcess(UserInfo user, string siteName)
        {
            return ProviderObject.CanUserStartAutomationProcessInternal(user, siteName);
        }


        /// <summary>
        /// Indicates if user can start automation process.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public static bool CanUserRemoveAutomationProcess(UserInfo user, string siteName)
        {
            return ProviderObject.CanUserRemoveAutomationProcessInternal(user, siteName);
        }


        /// <summary>
        /// Indicates if user can move automation process to specific step.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        public static bool CanUserMoveToSpecificAutomationStep(UserInfo user, string siteName)
        {
            return ProviderObject.CanUserMoveToSpecificAutomationStepInternal(user, siteName);
        }


        /// <summary>
        /// Check dependencies, return true if is depend.
        /// </summary>
        /// <param name="workflowStepId">Workflow step ID</param>
        /// <param name="documentNames">List of document names which use given workflow step</param>
        public static bool CheckDependencies(int workflowStepId, ref List<string> documentNames)
        {
            var infoObj = ProviderObject.GetInfoById(workflowStepId);
            if (infoObj != null)
            {
                documentNames = infoObj.Generalized.GetDependenciesNames();
                return (documentNames != null) && (documentNames.Count > 0);
            }

            return false;
        }


        /// <summary>
        /// Connects two steps.
        /// </summary>
        /// <param name="startStep">Start step</param>
        /// <param name="sourcePoint">Start source point for branching steps. Use Guid.Empty if there is no branching and standard source point should be used.</param>
        /// <param name="endStep">End step</param>
        public static WorkflowTransitionInfo ConnectSteps(WorkflowStepInfo startStep, Guid sourcePoint, WorkflowStepInfo endStep)
        {
            return ProviderObject.ConnectStepsInternal(startStep, sourcePoint, endStep);
        }


        /// <summary>
        /// Checks whether selected start step and possibly source point already has a connection.
        /// </summary>
        /// <param name="startStep">Start step</param>
        /// <param name="sourcePointGuid">Start step source point GUID</param>
        public static void ValidateStepIntegrity(WorkflowStepInfo startStep, Guid sourcePointGuid)
        {
            ProviderObject.ValidateStepIntegrityInternal(startStep, sourcePointGuid);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Sets the specified workflow step data.
        /// </summary>
        /// <param name="infoObj">Workflow step data object</param>
        protected virtual void SetWorkflowStepInfoInternal(WorkflowStepInfo infoObj)
        {
            // Set definition
            infoObj.SetValue("StepDefinition", infoObj.StepDefinition.GetData());

            // Set action parameters
            infoObj.SetValue("StepActionParameters", infoObj.StepActionParameters.GetData());

            // Ensure step workflow type
            if (infoObj.StepWorkflowID > 0)
            {
                var workflow = WorkflowInfoProvider.GetWorkflowInfo(infoObj.StepWorkflowID);
                if (workflow != null)
                {
                    infoObj.StepWorkflowType = workflow.WorkflowType;
                }
            }

            SetInfo(infoObj);
        }


        /// <summary>
        /// Returns the WorkflowStepInfo structure for the specified workflow step name and workflow.
        /// </summary>
        /// <param name="stepName">Step name to retrieve</param>
        /// <param name="workflowId">Workflow id of the step</param>
        protected virtual WorkflowStepInfo GetWorkflowStepInfoInternal(string stepName, int workflowId)
        {
            return GetWorkflowSteps()
                .WhereEquals("StepWorkflowID", workflowId)
                .WhereEquals("StepName", stepName)
                .TopN(1)
                .FirstOrDefault();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WorkflowStepInfo info)
        {
            base.DeleteInfo(info);

            // Transitions were deleted by query - remove them from cache
            ProviderHelper.ClearHashtables(WorkflowTransitionInfo.OBJECT_TYPE, info.Generalized.LogWebFarmTasks);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Initializes the workflow step orders.
        /// </summary>
        /// <param name="workflow">Workflow</param>
        protected virtual void InitStepOrdersInternal(WorkflowInfo workflow)
        {
            if (workflow == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.InitStepOrdersInternal]: Missing workflow object.");
            }

            if (workflow.IsBasic)
            {
                WorkflowStepInfo step = new WorkflowStepInfo();
                step.StepWorkflowID = workflow.WorkflowID;
                step.Generalized.InitObjectsOrder();
            }
        }


        /// <summary>
        /// Moves step up (decreases its order).
        /// </summary>
        /// <param name="stepInfo">Workflow step data object</param>
        protected virtual void MoveStepUpInternal(WorkflowStepInfo stepInfo)
        {
            if (stepInfo != null)
            {
                // Get step workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(stepInfo.StepWorkflowID);

                if (workflow == null)
                {
                    throw new Exception("[WorkflowStepInfoProvider.MoveStepUpInternal]: Missing workflow object.");
                }

                if (workflow.IsBasic)
                {
                    stepInfo.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves step down (increases its order).
        /// </summary>
        /// <param name="stepInfo">Workflow step data object</param>
        protected virtual void MoveStepDownInternal(WorkflowStepInfo stepInfo)
        {
            if (stepInfo != null)
            {
                // Get step workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(stepInfo.StepWorkflowID);

                if (workflow == null)
                {
                    throw new Exception("[WorkflowStepInfoProvider.MoveStepDownInternal]: Missing workflow object.");
                }

                if (workflow.IsBasic)
                {
                    stepInfo.Generalized.MoveObjectDown();
                }
            }
        }


        /// <summary>
        /// Returns the edit step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        protected virtual WorkflowStepInfo GetEditStepInternal(int workflowId)
        {
            var where = new WhereCondition().WhereEquals("StepType", (int)WorkflowStepTypeEnum.DocumentEdit).WhereEquals("StepWorkflowID", workflowId);

            return GetWorkflowSteps().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Returns the published step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        protected virtual WorkflowStepInfo GetPublishedStepInternal(int workflowId)
        {
            var where = new WhereCondition().WhereEquals("StepType", (int)WorkflowStepTypeEnum.DocumentPublished).WhereEquals("StepWorkflowID", workflowId);

            return GetWorkflowSteps().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Returns the finished step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        protected virtual WorkflowStepInfo GetFinishedStepInternal(int workflowId)
        {
            var where = new WhereCondition().WhereEquals("StepType", (int)WorkflowStepTypeEnum.Finished).WhereEquals("StepWorkflowID", workflowId);

            return GetWorkflowSteps().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Returns the archived step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        protected virtual WorkflowStepInfo GetArchivedStepInternal(int workflowId)
        {
            var where = new WhereCondition().WhereEquals("StepType", (int)WorkflowStepTypeEnum.DocumentArchived).WhereEquals("StepWorkflowID", workflowId);

            return GetWorkflowSteps().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Returns the first workflow step for given workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID of the step required</param>
        protected virtual WorkflowStepInfo GetFirstStepInternal(int workflowId)
        {
            var where = new WhereCondition()
                .WhereIn("StepType", new[]
                {
                    (int)WorkflowStepTypeEnum.DocumentEdit,
                    (int)WorkflowStepTypeEnum.Start
                })
                .WhereEquals("StepWorkflowID", workflowId);

            var step = GetWorkflowSteps().Where(where).OrderByAscending("StepOrder").TopN(1).BinaryData(true).FirstOrDefault();
            if (step == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.GetFirstStepInternal]: First step for given workflow is missing!");
            }

            return step;
        }


        /// <summary>
        /// Creates default workflow steps for the given workflow.
        /// </summary>
        /// <param name="workflow">Workflow</param>
        protected virtual void CreateDefaultWorkflowStepsInternal(WorkflowInfo workflow)
        {
            if (workflow == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.CreateDefaultWorkflowStepsInternal]: Missing workflow object.");
            }

            if (workflow.IsDocumentWorkflow)
            {
                CreateDefaultApprovalSteps(workflow);
            }
            else if (workflow.IsAutomation)
            {
                CreateDefaultAutomationSteps(workflow);
            }
        }


        /// <summary>
        /// Creates default automation steps for the given workflow.
        /// </summary>
        /// <param name="workflow">Workflow to be added steps to</param>
        private void CreateDefaultAutomationSteps(WorkflowInfo workflow)
        {
            // Start step
            WorkflowStepInfo start = new WorkflowStepInfo()
            {
                StepName = "start",
                StepDisplayName = "Start",
                StepType = WorkflowStepTypeEnum.Start,
                StepWorkflowID = workflow.WorkflowID,
                StepAllowReject = false
            };
            SetWorkflowStepInfo(start);

            // Finished step
            WorkflowStepInfo finished = new WorkflowStepInfo()
            {
                StepName = "finished",
                StepDisplayName = "Finished",
                StepType = WorkflowStepTypeEnum.Finished,
                StepWorkflowID = workflow.WorkflowID,
                StepAllowReject = false
            };
            SetWorkflowStepInfo(finished);

            // Connect start and finished steps
            WorkflowTransitionInfo transition1 = new WorkflowTransitionInfo();
            transition1.TransitionSourcePointGUID = start.StepDefinition.SourcePoints[0].Guid;
            transition1.TransitionStartStepID = start.StepID;
            transition1.TransitionEndStepID = finished.StepID;
            transition1.TransitionWorkflowID = workflow.WorkflowID;
            transition1.TransitionType = WorkflowTransitionTypeEnum.Automatic;
            WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(transition1);
        }


        /// <summary>
        /// Creates default approval steps for the given workflow.
        /// </summary>
        /// <param name="workflow">Workflow to be added steps to</param>
        protected virtual void CreateDefaultApprovalSteps(WorkflowInfo workflow)
        {
            // Edit step
            WorkflowStepInfo edit = new WorkflowStepInfo()
            {
                StepName = "edit",
                StepDisplayName = "Edit",
                StepType = WorkflowStepTypeEnum.DocumentEdit,
                StepOrder = !workflow.IsBasic ? 0 : 1,
                StepWorkflowID = workflow.WorkflowID,
                StepAllowReject = false
            };
            SetWorkflowStepInfo(edit);

            // Published step
            WorkflowStepInfo published = new WorkflowStepInfo()
            {
                StepName = "published",
                StepDisplayName = "Published",
                StepType = WorkflowStepTypeEnum.DocumentPublished,
                StepOrder = !workflow.IsBasic ? 0 : 2,
                StepWorkflowID = workflow.WorkflowID
            };
            SetWorkflowStepInfo(published);

            // Archived step
            WorkflowStepInfo archived = new WorkflowStepInfo()
            {
                StepName = "archived",
                StepDisplayName = "Archived",
                StepType = WorkflowStepTypeEnum.DocumentArchived,
                StepOrder = !workflow.IsBasic ? 0 : 3,
                StepWorkflowID = workflow.WorkflowID,
                StepAllowReject = false
            };
            SetWorkflowStepInfo(archived);

            // Create transitions for advanced workflow
            if (!workflow.IsBasic)
            {
                // Connect published and archived steps
                WorkflowTransitionInfo transition2 = new WorkflowTransitionInfo();
                transition2.TransitionSourcePointGUID = published.StepDefinition.SourcePoints[0].Guid;
                transition2.TransitionStartStepID = published.StepID;
                transition2.TransitionEndStepID = archived.StepID;
                transition2.TransitionWorkflowID = workflow.WorkflowID;
                transition2.TransitionType = WorkflowTransitionTypeEnum.Manual;
                WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(transition2);

                // Connect edit and published steps
                WorkflowTransitionInfo transition1 = new WorkflowTransitionInfo();
                transition1.TransitionSourcePointGUID = edit.StepDefinition.SourcePoints[0].Guid;
                transition1.TransitionStartStepID = edit.StepID;
                transition1.TransitionEndStepID = published.StepID;
                transition1.TransitionWorkflowID = workflow.WorkflowID;
                transition1.TransitionType = WorkflowTransitionTypeEnum.Manual;
                WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(transition1);
            }
        }


        /// <summary>
        /// Returns list of users who can approve given step and source point. Users who are approved due to generic roles are not included to the result.
        /// </summary>
        /// <param name="step">Workflow step</param>
        /// <param name="sourcePoint">Step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        protected virtual InfoDataSet<UserInfo> GetUsersWhoCanApproveInternal(WorkflowStepInfo step, SourcePoint sourcePoint, int siteId, string where, string orderBy, int topN, string columns)
        {
            if (step == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.GetUsersWhoCanApproveInternal]: Given step doesn't exist!");
            }

            if (!step.StepIsAllowed)
            {
                // None of the users have permissions for step or source point permissions are inherited from step settings
                if ((sourcePoint == null) || sourcePoint.InheritsStepSettings)
                {
                    return null;
                }
            }

            // Get initial settings
            SecuritySettings sett = new SecuritySettings(step, sourcePoint);

            // Get users condition
            string usersWhere = null;
            if (sett.UsersSettings != WorkflowStepSecurityEnum.Default)
            {
                // Get operator
                string op = (sett.UsersSettings == WorkflowStepSecurityEnum.OnlyAssigned) ? "IN" : "NOT IN";
                // Get source point condition
                string sourcePointWhere = (sett.UsersPointGuid != Guid.Empty) ? "StepSourcePointGUID = @UsersPointGUID" : "StepSourcePointGUID IS NULL";
                // Get site condition
                string siteWhere = (sett.UsersSettings == WorkflowStepSecurityEnum.OnlyAssigned) ? "" : "AND UserID IN (SELECT UserID FROM [CMS_UserSite] WHERE SiteID = @SiteID)";

                // Get complete users condition
                usersWhere = string.Format("UserID {0} (SELECT UserID FROM [CMS_WorkflowStepUser] WHERE (StepID = @StepID) AND {1}) {2}", op, sourcePointWhere, siteWhere);
            }

            // Get roles condition
            string rolesWhere = null;
            if (sett.RolesSettings != WorkflowStepSecurityEnum.Default)
            {
                // Get operator
                string op = (sett.RolesSettings == WorkflowStepSecurityEnum.OnlyAssigned) ? "IN" : "NOT IN";
                // Get source point condition
                string sourcePointWhere = (sett.RolesPointGuid != Guid.Empty) ? "StepSourcePointGUID = @RolesPointGUID" : "StepSourcePointGUID IS NULL";
                // Get first part of condition
                string workflowStepRolesWhere = string.Format("RoleID {0} (SELECT RoleID FROM CMS_WorkflowStepRoles WHERE (StepID = @StepID) AND ({1}))", op, sourcePointWhere);
                // Get second part of condition
                string siteRolesWhere = (siteId == 0) ? "" : "AND RoleID IN (SELECT RoleID FROM [CMS_Role] WHERE (SiteID = @SiteID OR SiteID IS NULL))";

                // Get complete condition
                rolesWhere = string.Format("UserID IN (SELECT UserID FROM [View_CMS_UserRoleMembershipRole] WHERE (ValidTo IS NULL OR ValidTo > @Now) AND {0} {1})", workflowStepRolesWhere, siteRolesWhere);
            }

            // Get complete condition
            string oper = sett.EvaluateRoles ? "AND" : "OR";
            string completeWhere = SqlHelper.AddWhereCondition(usersWhere, rolesWhere, oper);

            // Add additional where condition
            completeWhere = SqlHelper.AddWhereCondition(completeWhere, where);

            var whereCondition = new WhereCondition(completeWhere);
            whereCondition.Parameters = new QueryDataParameters
            {
                { "@Now", DateTime.Now },
                { "@StepID", step.StepID },
                { "@SiteID", siteId },
                { "@RolesPointGUID", sett.RolesPointGuid },
                { "@UsersPointGUID", sett.UsersPointGuid }
            };

            // Get the users
            var query = UserInfoProvider.GetUsersDataWithSettings().Where(whereCondition).TopN(topN).Columns(columns);

            if (!string.IsNullOrEmpty(orderBy))
            {
                string direction;
                var column = SqlHelper.GetOrderByColumnName(orderBy, out direction);

                query = direction != null && direction.Equals(SqlHelper.ORDERBY_DESC, StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(column)
                    : query.OrderByAscending(column);
            }

            return query.TypedResult;
        }


        /// <summary>
        /// Creates complete where condition to filter only pending steps for given user and site.
        /// </summary>
        /// <param name="columnName">Column name which indicates step identifier in table</param>
        /// <param name="userId">User ID for the security check</param>
        /// <param name="siteId">Site ID for the security check</param>
        /// <param name="typeWhereCondition">Partial where condition for filter specific workflow type</param>
        private static WhereCondition GetPendingStepsWhereCondition(string columnName, int userId, int siteId, string typeWhereCondition)
        {
            StringBuilder sb = new StringBuilder();

            // Get all steps where
            sb.Append("(", columnName, " IN (SELECT StepID FROM CMS_WorkflowStep s WHERE ");

            sb.Append("(", typeWhereCondition, ")");

            // User's roles are assigned
            sb.Append(" AND ");
            sb.Append("((StepRolesSecurity = ", (int)WorkflowStepSecurityEnum.OnlyAssigned, @" OR StepRolesSecurity IS NULL) AND ");
            sb.Append("EXISTS (SELECT * FROM CMS_WorkflowStepRoles WHERE StepID = s.StepID AND RoleID IN (SELECT RoleID FROM [View_CMS_UserRoleMembershipRole] WHERE (ValidTo IS NULL OR ValidTo > @Now) AND (UserID = @UserID) AND ((SiteID = @SiteID) OR SiteID IS NULL))))");

            // And user is not excluded
            sb.Append(" AND NOT ");
            sb.Append("(StepUsersSecurity = ", (int)WorkflowStepSecurityEnum.AllExceptAssigned, @" AND ");
            sb.Append("EXISTS (SELECT * FROM CMS_WorkflowStepUser WHERE StepID = s.StepID AND UserID = @UserID))");

            // Or user is not in excluded roles
            sb.Append(" OR ");
            sb.Append("(StepRolesSecurity = ", (int)WorkflowStepSecurityEnum.AllExceptAssigned, @" AND NOT ");
            sb.Append("EXISTS (SELECT * FROM CMS_WorkflowStepRoles WHERE StepID = s.StepID AND RoleID IN (SELECT RoleID FROM [View_CMS_UserRoleMembershipRole] WHERE (ValidTo IS NULL OR ValidTo > @Now) AND (UserID = @UserID) AND ((SiteID = @SiteID) OR SiteID IS NULL))))");

            // And user is not excluded
            sb.Append(" AND NOT ");
            sb.Append("(StepUsersSecurity = ", (int)WorkflowStepSecurityEnum.AllExceptAssigned, @" AND ");
            sb.Append("EXISTS (SELECT * FROM CMS_WorkflowStepUser WHERE StepID = s.StepID AND UserID = @UserID))");

            // Or user is included
            sb.Append(" OR ");
            sb.Append("((StepUsersSecurity = ", (int)WorkflowStepSecurityEnum.OnlyAssigned, @" OR StepUsersSecurity IS NULL) AND ");
            sb.Append("EXISTS (SELECT * FROM CMS_WorkflowStepUser WHERE StepID = s.StepID AND UserID = @UserID))");

            sb.Append("))");

            var where = new WhereCondition(sb.ToString());

            // Prepare parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@Now", DateTime.Now);
            parameters.Add("@SiteID", siteId);
            parameters.Add("@UserID", userId);
            where.Parameters = parameters;

            return where;
        }


        /// <summary>
        /// Gets complete where condition for pending steps from automation processes.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="site">Site identifier</param>
        public static WhereCondition GetAutomationPendingStepsWhereCondition(UserInfo user, SiteInfoIdentifier site)
        {
            // No restriction for authorized users
            if (CanUserManageAutomationProcesses(user, site))
            {
                return new WhereCondition();
            }

            string typeWhereCondition = "StepWorkflowType = " + (int)WorkflowTypeEnum.Automation;
            return GetPendingStepsWhereCondition("StateStepID", user.UserID, site, typeWhereCondition);
        }


        /// <summary>
        /// Gets complete where condition for pending steps from workflow.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="site">Site identifier</param>
        public static WhereCondition GetWorkflowPendingStepsWhereCondition(UserInfo user, SiteInfoIdentifier site)
        {
            // Include excluded steps
            string excludedStepTypes = String.Format("StepType NOT IN ({0}, {1}, {2})", (int)WorkflowStepTypeEnum.DocumentEdit, (int)WorkflowStepTypeEnum.DocumentPublished, (int)WorkflowStepTypeEnum.DocumentArchived);
            var where = new WhereCondition(excludedStepTypes);

            // No need for security where condition
            if (CanUserManageWorkflow(user, site))
            {
                return where;
            }

            // User doesn't have manage workflow permission so filter pending steps
            string typeWhereCondition = String.Format("StepWorkflowType IN ({0}, {1}) OR StepWorkflowType IS NULL", (int)WorkflowTypeEnum.Basic, (int)WorkflowTypeEnum.Approval);
            where.And(GetPendingStepsWhereCondition("DocumentWorkflowStepID", user.UserID, site, typeWhereCondition));

            return where;
        }


        /// <summary>
        /// Indicates if user can manage workflow.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserManageWorkflowInternal(UserInfo user, string siteName)
        {
            // If user not specified cannot manage
            if (user == null)
            {
                return false;
            }

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource("cms.content", "manageworkflow", siteName, user);
        }


        /// <summary>
        /// Indicates if user can manage automation processes.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserManageAutomationProcessesInternal(UserInfo user, string siteName)
        {
            // If user not specified cannot manage
            if (user == null)
            {
                return false;
            }

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ManageProcesses", siteName, user);
        }


        /// <summary>
        /// Indicates if user can start automation process.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserStartAutomationProcessInternal(UserInfo user, string siteName)
        {
            // If user not specified cannot start
            if (user == null)
            {
                return false;
            }

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "StartProcess", siteName, user);
        }


        /// <summary>
        /// Indicates if user can remove automation process.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserRemoveAutomationProcessInternal(UserInfo user, string siteName)
        {
            // If user not specified cannot remove
            if (user == null)
            {
                return false;
            }

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "RemoveProcess", siteName, user);
        }


        /// <summary>
        /// Indicates if user can move automation process to any step.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        protected virtual bool CanUserMoveToSpecificAutomationStepInternal(UserInfo user, string siteName)
        {
            // If user not specified cannot move
            if (user == null)
            {
                return false;
            }

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || UserInfoProvider.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "MoveToSpecificStep", siteName, user);
        }


        /// <summary>
        /// Returns true if given user can approve or reject given step.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="step">Workflow step</param>
        /// <param name="sourcePoint">Workflow step source point (for steps with multiple outgoing transitions)</param>
        /// <param name="siteId">Site ID</param>
        protected virtual bool CanUserApproveInternal(UserInfo user, WorkflowStepInfo step, SourcePoint sourcePoint, int siteId)
        {
            if (step == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.CanUserApproveInternal]: Given step doesn't exist!");
            }

            // Get step workflow
            WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID);
            if (workflow == null)
            {
                throw new Exception("[WorkflowStepInfoProvider.CanUserApproveInternal]: Step workflow doesn't exist!");
            }

            // Get initial settings
            SecuritySettings sett = new SecuritySettings(step, sourcePoint);

            // Get result from request cache
            string key = GetRequestKeyInternal(user, step, sett.PointGuid, siteId);
            if (RequestStockHelper.Contains(key))
            {
                return (bool)RequestStockHelper.GetItem(key);
            }

            // Get site name
            string siteName = SiteInfoProvider.GetSiteName(siteId);

            // User with manage workflow permission can always approve
            bool canManage = workflow.IsAutomation ? CanUserManageAutomationProcessesInternal(user, siteName) : CanUserManageWorkflowInternal(user, siteName);
            if (canManage)
            {
                // Add to request cache
                RequestStockHelper.Add(key, true);
                return true;
            }

            if (!step.StepIsAllowed)
            {
                // User don't have permissions for step or source point permissions are inherited from step settings
                if ((sourcePoint == null) || sourcePoint.InheritsStepSettings)
                {
                    // Add to request cache
                    RequestStockHelper.Add(key, false);
                    return false;
                }
            }

            // Users security
            bool usersResult = false;
            if (sett.UsersSettings != WorkflowStepSecurityEnum.Default)
            {
                usersResult = (sett.UsersSettings == WorkflowStepSecurityEnum.AllExceptAssigned);
                if (WorkflowStepUserInfoProvider.GetWorkflowStepUserInfo(step.StepID, user.UserID, sett.UsersPointGuid) != null)
                {
                    usersResult = (sett.UsersSettings == WorkflowStepSecurityEnum.OnlyAssigned);
                }
            }

            // Evaluate roles permissions
            bool rolesResult = false;
            // Do not evaluate roles if not necessary
            if ((!usersResult && !sett.EvaluateRoles) || (usersResult && sett.EvaluateRoles))
            {
                // Roles security
                rolesResult = (sett.RolesSettings == WorkflowStepSecurityEnum.AllExceptAssigned);

                // Get source point condition
                string sourcePointWhere = (sett.RolesPointGuid != Guid.Empty) ? "StepSourcePointGUID = @RolesPointGUID" : "StepSourcePointGUID IS NULL";
                // Get complete condition
                string where = string.Format("RoleID IN (SELECT RoleID FROM CMS_WorkflowStepRoles WHERE StepID = @StepID AND {0}) AND (SiteID = @SiteID OR SiteID IS NULL)", sourcePointWhere);

                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@StepID", step.StepID);
                parameters.Add("@SiteID", siteId);
                parameters.Add("@RolesPointGUID", sett.RolesPointGuid);
                parameters.EnsureDataSet<RoleInfo>();

                // Get the roles
                var roles = ConnectionHelper.ExecuteQuery("cms.role.selectall", parameters, where, null, 0, "RoleName, SiteID").As<RoleInfo>().Items;

                // Check roles
                foreach (var role in roles)
                {
                    if (user.IsInRole(role.RoleName, siteName, (role.SiteID <= 0), true))
                    {
                        rolesResult = (sett.RolesSettings == WorkflowStepSecurityEnum.OnlyAssigned);
                        break;
                    }
                }
            }

            bool result;
            if (sett.EvaluateRoles)
            {
                result = usersResult && rolesResult;
            }
            else
            {
                result = usersResult || rolesResult;
            }

            // Add to request cache
            RequestStockHelper.Add(key, result);

            return result;
        }


        /// <summary>
        /// Gets key for request to store the permissions check result
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="step">Workflow step</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual string GetRequestKeyInternal(UserInfo user, WorkflowStepInfo step, Guid sourcePointGuid, int siteId)
        {
            return "canUserApproveStep_" + step.StepID + "_" + sourcePointGuid + "_" + user.UserID + "_" + siteId;
        }


        /// <summary>
        /// Connects two steps.
        /// </summary>
        /// <param name="startStep">Start step</param>
        /// <param name="sourcePoint">Start source point for branching steps. Use Guid.Empty if there is no branching and standard source point should be used.</param>
        /// <param name="endStep">End step</param>
        protected virtual WorkflowTransitionInfo ConnectStepsInternal(WorkflowStepInfo startStep, Guid sourcePoint, WorkflowStepInfo endStep)
        {
            if ((startStep == null) || (endStep == null))
            {
                throw new NullReferenceException("[WorkflowStepInfoProvider.ConnectStepsInternal]: Missing start or end step.");
            }

            // Get default point GUID
            if (sourcePoint == Guid.Empty)
            {
                var point = startStep.StepDefinition.SourcePoints.Where(p => !(p is TimeoutSourcePoint)).FirstOrDefault();
                if (point != null)
                {
                    sourcePoint = point.Guid;
                }
                else
                {
                    throw new NullReferenceException("[WorkflowStepInfoProvider.ConnectStepsInternal]: Missing start step source point information.");
                }
            }

            // From each step/source point can start at most one connection
            ValidateStepIntegrityInternal(startStep, sourcePoint);

            // Create new connection
            WorkflowTransitionInfo connection = new WorkflowTransitionInfo()
            {
                TransitionStartStepID = startStep.StepID,
                TransitionEndStepID = endStep.StepID,
                TransitionWorkflowID = startStep.StepWorkflowID,
                TransitionSourcePointGUID = sourcePoint
            };

            // Save new connection to database
            WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(connection);

            return connection;
        }


        /// <summary>
        /// Checks whether selected start step and possibly source point already has a connection.
        /// </summary>
        /// <param name="startStep">Start step</param>
        /// <param name="sourcePointGuid">Start step source point GUID</param>
        protected virtual void ValidateStepIntegrityInternal(WorkflowStepInfo startStep, Guid sourcePointGuid)
        {
            // Regular node can start only 1 connection
            if (sourcePointGuid == Guid.Empty)
            {
                if (!DataHelper.DataSourceIsEmpty(WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                .WhereEquals("TransitionWorkflowID", startStep.StepWorkflowID)
                                                                                .WhereEquals("TransitionStartStepID", startStep.StepID)
                                                                                .TopN(1)
                                                                                .Column("TransitionID")))
                {
                    throw new Exception(string.Format("[WorkflowStepInfoProvider.ValidateStepIntegrityInternal]: Some connection already starts in the '{0}' step.", HTMLHelper.HTMLEncode(startStep.StepDisplayName)));
                }
            }
            else
            {
                if (!startStep.StepDefinition.SourcePoints.Exists(p => p.Guid == sourcePointGuid))
                {
                    throw new Exception("[WorkflowStepInfoProvider.ValidateStepIntegrityInternal]: Case doesn't exist.");
                }

                // Multi-choice node can start only 1 connection in each source point
                if (!DataHelper.DataSourceIsEmpty(WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                .WhereEquals("TransitionWorkflowID", startStep.StepWorkflowID)
                                                                                .WhereEquals("TransitionStartStepID", startStep.StepID)
                                                                                .WhereEquals("TransitionSourcePointGuid", sourcePointGuid)
                                                                                .TopN(1)
                                                                                .Column("TransitionID")))
                {
                    throw new Exception(string.Format("[WorkflowStepInfoProvider.ValidateStepIntegrityInternal]: Some connection already starts in source point with '{0}' GUID.", sourcePointGuid));
                }
            }
        }

        #endregion


        /// <summary>
        /// Class for evaluation security settings
        /// </summary>
        private class SecuritySettings
        {
            #region "Properties"

            /// <summary>
            /// Workflow step
            /// </summary>
            private WorkflowStepInfo Step { get; set; }


            /// <summary>
            /// Source point
            /// </summary>
            private SourcePoint SourcePoint { get; set; }


            /// <summary>
            /// Source point GUID to use
            /// </summary>
            public Guid PointGuid { get; set; }


            /// <summary>
            /// Users source point GUID to use
            /// </summary>
            public Guid UsersPointGuid { get; set; }


            /// <summary>
            /// Roles source point GUID to use
            /// </summary>
            public Guid RolesPointGuid { get; set; }


            /// <summary>
            /// Users security settings
            /// </summary>
            public WorkflowStepSecurityEnum UsersSettings { get; set; }


            /// <summary>
            /// Roles security settings
            /// </summary>
            public WorkflowStepSecurityEnum RolesSettings { get; set; }


            /// <summary>
            /// Indicates if roles should be evaluated if users match (AND operator)
            /// </summary>
            public bool EvaluateRoles { get; set; }

            #endregion


            #region "Constructors"

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="step">Workflow step</param>
            /// <param name="sourcePoint">Source point</param>
            public SecuritySettings(WorkflowStepInfo step, SourcePoint sourcePoint)
            {
                Step = step;
                SourcePoint = sourcePoint;

                // Get source point settings for further evaluation
                UsersSettings = step.StepUsersSecurity;
                RolesSettings = step.StepRolesSecurity;
                if (SourcePoint != null)
                {
                    PointGuid = SourcePoint.InheritsStepSettings ? Guid.Empty : sourcePoint.Guid;

                    // User settings are not inherited, get source point settings
                    if (!SourcePoint.InheritsUsersSettings)
                    {
                        UsersPointGuid = PointGuid;
                        UsersSettings = sourcePoint.StepUsersSecurity;
                    }

                    // Role settings are not inherited, get source point settings
                    if (!SourcePoint.InheritsRolesSettings)
                    {
                        RolesPointGuid = PointGuid;
                        RolesSettings = sourcePoint.StepRolesSecurity;
                    }
                }

                // Indicates if roles should be evaluated if users match (AND operator)
                EvaluateRoles = (UsersSettings == WorkflowStepSecurityEnum.AllExceptAssigned);
            }

            #endregion
        }
    }
}
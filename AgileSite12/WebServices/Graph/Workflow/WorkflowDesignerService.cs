using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using CMS.WorkflowEngine.GraphConfig;

using Definitions = CMS.WorkflowEngine.Definitions;

namespace CMS.WebServices
{
    /// <summary>
    /// Workflow graph service implementation.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class WorkflowDesignerService : IWorkflowDesignerService
    {
        #region "Private methods"

        /// <summary>
        /// Resolves given conditions.
        /// </summary>
        /// <param name="condition">Condition macro</param>
        /// <returns>Resolved condition</returns>
        private string ResolveCondition(string condition)
        {
            return MacroRuleTree.GetRuleText(ValidationHelper.GetString(condition, String.Empty));
        }

        #endregion


        #region "Service methods"

        /// <summary>
        /// Get node specification for refresh purpose.
        /// </summary>
        /// <param name="id">Node ID</param>
        public ServiceResponse<Node> GetNode(string id)
        {
            if (!IsAuthorized())
            {
                return new ServiceResponse<Node>(GetUnauthorizedResponse());
            }

            SecurityHelper.LogScreenLockAction();

            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(id, 0));
            if (step == null)
            {
                return GetNotFoundResponse<Node>();
            }

            WorkflowNode node = WorkflowNode.GetInstance(step);

            // Prepare condition tooltips
            string condition = ResHelper.GetString("graph.conditionmacrotooltip");
            node.SourcePoints.ForEach
            (
                sourcepoint =>
                {
                    var cond = ResolveCondition(sourcepoint.Tooltip);
                    if (!String.IsNullOrEmpty(cond))
                    {
                        sourcepoint.Tooltip = condition + cond;
                    }
                }
            );

            return new ServiceResponse<Node>(node);
        }


        /// <summary>
        /// Sets new position to specified node.
        /// </summary>
        /// <param name="id">Node identifier</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        public ServiceResponse SetNodePosition(string id, int x, int y)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(id, 0));
            if (step == null)
            {
                return GetNotFoundResponse();
            }

            // Make requested changes
            step.StepDefinition.Position = new GraphPoint(x, y);

            // Save changes to database
            try
            {
                WorkflowStepInfoProvider.SetWorkflowStepInfo(step);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Sets node name.
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="name">New name</param>
        public ServiceResponse SetNodeName(string id, string name)
        {
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(id, 0));
            if (step == null)
            {
                return GetNotFoundResponse();
            }

            if (String.IsNullOrEmpty(name.Trim()))
            {
                return GetBadRequestResponse(ResHelper.GetString("graphservice.errornameempty"));
            }

            if (MacroProcessor.IsLocalizationMacro(step.StepDisplayName))
            {
                UpdateResourceStringTranslation(step.StepDisplayName.Substring(2, step.StepDisplayName.Length - 4), name);
            }
            else
            {
                step.StepDisplayName = name;
            }

            try
            {
                WorkflowStepInfoProvider.SetWorkflowStepInfo(step);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Sets switch case name.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="caseId">Switch case ID</param>
        /// <param name="name">New name</param>
        public ServiceResponse SetSwitchCaseName(string nodeId, string caseId, string name)
        {
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(nodeId, 0));
            if (step == null)
            {
                return GetNotFoundResponse();
            }

            Guid sourcePointGuid = ValidationHelper.GetGuid(caseId, Guid.Empty);
            if (sourcePointGuid == Guid.Empty)
            {
                return GetBadRequestResponse(ResHelper.GetString("graphservice.invalidcaseid"));
            }

            Definitions.SourcePoint sourcePoint = step.StepDefinition.SourcePoints.FirstOrDefault(i => i.Guid == sourcePointGuid);
            if (sourcePoint == null)
            {
                return GetNotFoundResponse();
            }

            if (String.IsNullOrEmpty(name.Trim()))
            {
                return GetBadRequestResponse(ResHelper.GetString("graphservice.errornameempty"));
            }

            if (MacroProcessor.IsLocalizationMacro(sourcePoint.Label))
            {
                UpdateResourceStringTranslation(sourcePoint.Label.Substring(2, sourcePoint.Label.Length - 4), name);
            }
            else
            {
                sourcePoint.Label = name;
            }

            try
            {
                WorkflowStepInfoProvider.SetWorkflowStepInfo(step);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTOIN", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Creates a new connection between selected nodes.
        /// </summary>
        /// <param name="startNodeId">Start node ID</param>
        /// <param name="endNodeId">End node ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public ServiceResponse<int> CreateConnection(string startNodeId, string endNodeId, string sourcePointGuid)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return new ServiceResponse<int>(GetUnauthorizedResponse());
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            int startId = ValidationHelper.GetInteger(startNodeId, 0);
            int endId = ValidationHelper.GetInteger(endNodeId, 0);

            WorkflowStepInfo startStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(startId);
            if (startStep == null)
            {
                return GetNotFoundResponse<int>();
            }

            WorkflowStepInfo endStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(endId);
            if (endStep == null)
            {
                return GetNotFoundResponse<int>();
            }

            Guid pointGuid = ValidationHelper.GetGuid(sourcePointGuid, Guid.Empty);
            WorkflowTransitionInfo connection;

            // Save new connection to database
            try
            {
                connection = startStep.ConnectTo(pointGuid, endStep);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse<int>(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.connectioncreationfailed"));
            }

            return new ServiceResponse<int>(connection.TransitionID);
        }


        /// <summary>
        /// Deletes selected connection.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        public ServiceResponse RemoveConnection(string connectionId)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            int connId = ValidationHelper.GetInteger(connectionId, 0);
            if (connId == 0)
            {
                return GetBadRequestResponse(ResHelper.GetString("graphservice.invalidconnectionid"));
            }

            WorkflowTransitionInfo connection = WorkflowTransitionInfoProvider.GetWorkflowTransitionInfo(connId);
            if (connection == null)
            {
                return new ServiceResponse(ResponseStatusEnum.OK);
            }

            try
            {
                WorkflowTransitionInfoProvider.DeleteWorkflowTransitionInfo(connId);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.connectiondeletionfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Moves connection start to specified node.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="startNodeId">New start node ID</param>
        /// <param name="sourcePointGuid">New source point GUID</param>
        public ServiceResponse EditConnectionStart(string connectionId, string startNodeId, string sourcePointGuid)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            WorkflowTransitionInfo connection = WorkflowTransitionInfoProvider.GetWorkflowTransitionInfo(ValidationHelper.GetInteger(connectionId, 0));
            if (connection == null)
            {
                return GetNotFoundResponse();
            }

            Guid pointGuid = ValidationHelper.GetGuid(sourcePointGuid, Guid.Empty);
            int stepId = ValidationHelper.GetInteger(startNodeId, 0);

            if ((connection.TransitionStartStepID == stepId) && (connection.TransitionSourcePointGUID == pointGuid))
            {
                // Nothing changed
                return new ServiceResponse(ResponseStatusEnum.OK);
            }

            if (stepId == connection.TransitionEndStepID)
            {
                // Start and end step can't be the same
                return GetBadRequestResponse(ResHelper.GetString("graphservice.consistencyerror"));
            }

            WorkflowStepInfo startStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
            if (startStep == null)
            {
                return GetBadRequestResponse(ResHelper.GetString("workflowservice.missingstartstep"));
            }

            // Save changes to database
            try
            {
                // The new start step/source point must not start some connection already
                WorkflowStepInfoProvider.ValidateStepIntegrity(startStep, pointGuid);

                // Set new values
                connection.TransitionStartStepID = stepId;
                connection.TransitionSourcePointGUID = pointGuid;

                WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(connection);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Moves connection end to selected node.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="endNodeId">End node ID</param>
        public ServiceResponse EditConnectionEnd(string connectionId, string endNodeId)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            WorkflowTransitionInfo connection = WorkflowTransitionInfoProvider.GetWorkflowTransitionInfo(ValidationHelper.GetInteger(connectionId, 0));
            if (connection == null)
            {
                return GetNotFoundResponse();
            }

            int nodeId = ValidationHelper.GetInteger(endNodeId, 0);
            if (nodeId == connection.TransitionEndStepID)
            {
                // Nothing changed
                return new ServiceResponse(ResponseStatusEnum.OK, null);
            }

            if (nodeId == connection.TransitionStartStepID)
            {
                // Start and end step can't be the same
                return GetBadRequestResponse(ResHelper.GetString("graphservice.consistencyerror"));
            }

            // Save changes to database
            try
            {
                // Set new values
                connection.TransitionEndStepID = nodeId;

                WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(connection);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Adds switch case to multi-choice node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public ServiceResponse<Node> AddSwitchCase(string nodeId)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return new ServiceResponse<Node>(GetUnauthorizedResponse());
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(nodeId, 0));
            if (step == null)
            {
                return GetNotFoundResponse<Node>();
            }

            // Save changes to database
            string result;

            try
            {
                result = step.AddSourcePoint();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse<Node>(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            if (!String.IsNullOrEmpty(result))
            {
                return GetBadRequestResponse<Node>(result, WorkflowNode.GetInstance(step));
            }

            return new ServiceResponse<Node>(WorkflowNode.GetInstance(step));
        }


        /// <summary>
        /// Removes switch case from multi-choice node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="caseId">Case ID</param>
        public ServiceResponse<Node> RemoveSwitchCase(string nodeId, string caseId)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return new ServiceResponse<Node>(GetUnauthorizedResponse());
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameters
            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(ValidationHelper.GetInteger(nodeId, 0));
            if (step == null)
            {
                return GetNotFoundResponse<Node>();
            }

            Guid caseGuid = ValidationHelper.GetGuid(caseId, Guid.Empty);
            if (caseGuid == Guid.Empty)
            {
                return GetBadRequestResponse<Node>(ResHelper.GetString("graphservice.invalidcaseid"));
            }

            string result;

            try
            {
                result = step.RemoveSourcePoint(caseGuid);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse<Node>(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingchangesfailed"));
            }

            if (!String.IsNullOrEmpty(result))
            {
                return GetBadRequestResponse<Node>(result, WorkflowNode.GetInstance(step));
            }

            return new ServiceResponse<Node>(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Creates new node of specified type.
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="actionId">Action type</param>
        /// <param name="parentId">Parent ID</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="splitConnectionsIDs">Connections new node should be attached on</param>
        public GraphPartialRefresh CreateNodeOnConnections(string type, string actionId, string parentId, int x, int y, List<string> splitConnectionsIDs)
        {
            // Create step
            ServiceResponse<Node> serviceNode = CreateNode(type, actionId, parentId, x, y);
            try
            {
                // Get transitions to split
                string where = SqlHelper.GetWhereCondition<int>("TransitionID", splitConnectionsIDs, false);
                var transitions = WorkflowTransitionInfoProvider.GetWorkflowTransitions().Where(where).Column("TransitionEndStepID").TypedResult.Items.ToList();

                // Check that all of them has the same end step
                int targetID = transitions[0].TransitionEndStepID;
                if (transitions.All(c => c.TransitionEndStepID == targetID))
                {
                    // Update transitions to lead to new step
                    Node node = serviceNode.Data;
                    int nodeID = ValidationHelper.GetInteger(node.ID, 0);

                    WorkflowTransitionInfoProvider.UpdateTransitionsEndStep(where, nodeID);

                    transitions = WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                .Where(where)
                                                                .Columns("TransitionEndStepID, TransitionStartStepID, TransitionSourcePointGUID, TransitionID")
                                                                .TypedResult.Items.ToList();

                    // Connect new step to target step
                    WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(nodeID);
                    if (node.SourcePoints.Count > 0)
                    {
                        WorkflowStepInfo targetStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(targetID);
                        WorkflowTransitionInfo newTransition = step.ConnectTo(step.StepDefinition.SourcePoints[0].Guid, targetStep);
                        // Add new transition to list of transitions to be updated
                        transitions.Add(newTransition);
                    }
                    // Return data to refresh relevant par of graph
                    return new GraphPartialRefresh(new [] { step }, transitions);
                }
            }
            catch
            {
                return new GraphPartialRefresh(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingnodeonconnectionfailed"));
            }
            return new GraphPartialRefresh(new [] { serviceNode.Data }, null);
        }


        /// <summary>
        /// Creates new node of specified type.
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="actionId">Action type</param>
        /// <param name="parentId">Workflow ID</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        public ServiceResponse<Node> CreateNode(string type, string actionId, string parentId, int x, int y)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return new ServiceResponse<Node>(GetUnauthorizedResponse());
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameter
            WorkflowStepTypeEnum nodeType;

            try
            {
                nodeType = (WorkflowStepTypeEnum)Enum.Parse(typeof(WorkflowStepTypeEnum), type);
            }
            catch (Exception)
            {
                return GetBadRequestResponse<Node>("Wrong node type.");
            }

            int workflowId = ValidationHelper.GetInteger(parentId, 0);
            int wActionId = ValidationHelper.GetInteger(actionId, 0);
            WorkflowActionInfo action = null;
            if ((nodeType == WorkflowStepTypeEnum.Action) && (wActionId > 0))
            {
                action = WorkflowActionInfoProvider.GetWorkflowActionInfo(wActionId);
            }

            WorkflowNode node = WorkflowNode.GetInstance(nodeType);
            if (action != null)
            {
                node.Name = ResHelper.LocalizeString(action.ActionDisplayName);
            }

            // Create new step
            WorkflowStepInfo step = new WorkflowStepInfo
            {
                StepType = nodeType,
                StepWorkflowID = workflowId,
                StepDisplayName = node.Name,
                StepName = InfoHelper.CODENAME_AUTOMATIC,
                StepActionID = wActionId
            };

            // Do not send notification e-mails for actions and automatic steps by default
            bool disable = (step.StepType == WorkflowStepTypeEnum.Condition) || (step.StepType == WorkflowStepTypeEnum.MultichoiceFirstWin) || step.StepIsAction || (step.StepType == WorkflowStepTypeEnum.Wait);
            if (disable)
            {
                step.StepSendEmails = false;
            }

            step.StepDefinition.Position = new GraphPoint(x, y);

            // Save new node to database
            try
            {
                WorkflowStepInfoProvider.SetWorkflowStepInfo(step);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse<Node>(ResponseStatusEnum.InternalError, ResHelper.GetString("graphservice.savingnewnodefailed"));
            }

            return new ServiceResponse<Node>(WorkflowNode.GetInstance(step));
        }


        /// <summary>
        /// Removes specified node and all its connections.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public ServiceResponse RemoveNode(string nodeId)
        {
            // Check permissions
            if (!IsAuthorized())
            {
                return GetUnauthorizedResponse();
            }

            SecurityHelper.LogScreenLockAction();

            // Validate parameter
            int id = ValidationHelper.GetInteger(nodeId, 0);
            if (id <= 0)
            {
                return GetBadRequestResponse(ResHelper.GetString("workflowservice.invalidstepid"));
            }

            WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(id);
            if (step == null)
            {
                return new ServiceResponse(ResponseStatusEnum.OK);
            }

            if (!CanDeleteStep(step))
            {
                return GetBadRequestResponse(ResHelper.GetString("workflowservice.cantdeletenodetype"));
            }

            return DeleteNode(id);
        }

        #endregion


        #region "Helper methods"

        private static ServiceResponse GetUnauthorizedResponse()
        {
            return new ServiceResponse(ResponseStatusEnum.Unauthorized, ResHelper.GetString("workflowservice.unauthorized"));
        }


        /// <summary>
        /// Gets event log source
        /// </summary>
        protected virtual string GetEventLogSource()
        {
            return "WORKFLOWSERVICE";
        }


        /// <summary>
        /// Check user permissions.
        /// </summary>
        protected virtual bool IsAuthorized()
        {
            // Validate session token
            if (!SecurityHelper.ValidateSessionToken(CookieHelper.GetValue(CookieName.SessionToken), CookieName.SessionToken))
            {
                return false;
            }

            // Check permissions
            return WorkflowStepInfoProvider.CanUserManageWorkflow(MembershipContext.AuthenticatedUser, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Deletes node with dependencies check
        /// </summary>
        /// <param name="id">Node ID</param>
        protected virtual ServiceResponse DeleteNode(int id)
        {
            try
            {
                List<string> documentNames = new List<string>();
                if (WorkflowStepInfoProvider.CheckDependencies(id, ref documentNames))
                {
                    // Encode and localize names
                    StringBuilder sb = new StringBuilder();
                    documentNames.ForEach(item => sb.Append("\r\n", ResHelper.LocalizeString(item)));
                    return GetBadRequestResponse(ResHelper.GetString("Workflow.CannotDeleteStepUsed") + "\r\n" + ResHelper.GetString("workflow.documentlist") + sb);
                }
                else
                {
                    // Delete the workflow step
                    WorkflowStepInfoProvider.DeleteWorkflowStepInfo(id);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("workflowservice.removingstepfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }


        /// <summary>
        /// Returns ServiceResponse with BadRequest type
        /// </summary>
        /// <param name="message">Error message</param>
        protected ServiceResponse GetBadRequestResponse(string message)
        {
            return new ServiceResponse(ResponseStatusEnum.BadRequest, message);
        }


        /// <summary>
        /// Returns generic ServiceResponse with BadRequest type
        /// </summary>
        /// <typeparam name="TData">Response generic type</typeparam>
        /// <param name="message">Error message</param>
        protected ServiceResponse<TData> GetBadRequestResponse<TData>(string message)
        {
            return new ServiceResponse<TData>(ResponseStatusEnum.BadRequest, message);
        }


        /// <summary>
        /// Returns generic ServiceResponse with BadRequest type
        /// </summary>
        /// <typeparam name="TData">Response generic type</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="data">Response data</param>
        protected ServiceResponse<TData> GetBadRequestResponse<TData>(string message, TData data)
        {
            return new ServiceResponse<TData>(ResponseStatusEnum.BadRequest, message, data);
        }


        /// <summary>
        /// Returns ServiceResponse with NotFound type
        /// </summary>
        protected ServiceResponse GetNotFoundResponse()
        {
            return new ServiceResponse(ResponseStatusEnum.NotFound, ResHelper.GetString("editedobject.notexists"));
        }


        /// <summary>
        /// Returns generic ServiceResponse with NotFound type
        /// </summary>
        /// <typeparam name="TData">Response generic type</typeparam>
        protected ServiceResponse<TData> GetNotFoundResponse<TData>()
        {
            return new ServiceResponse<TData>(ResponseStatusEnum.NotFound, ResHelper.GetString("editedobject.notexists"));
        }


        /// <summary>
        /// Returns true if given step can be deleted.
        /// </summary>
        /// <param name="step">Workflow step</param>
        private bool CanDeleteStep(WorkflowStepInfo step)
        {
            if (step != null)
            {
                if (!step.StepIsDeletable)
                {
                    return false;
                }

                if (step.StepIsDefault)
                {
                    var sameTypeSteps = WorkflowStepInfoProvider.GetWorkflowSteps()
                        .Where(String.Format("[StepType] = {0} AND [StepWorkflowID] = {1}", (int)step.StepType, step.StepWorkflowID))
                        .Columns("StepID")
                        .TopN(2);

                    return (sameTypeSteps.Count > 1);
                }
            }

            return true;
        }


        /// <summary>
        /// Update resource string with given translation
        /// </summary>
        /// <param name="key">resource string key</param>
        /// <param name="translation">resource translation</param>
        private void UpdateResourceStringTranslation(string key, string translation)
        {
            ResourceStringInfo rsi = ResourceStringInfoProvider.GetResourceStringInfo(key) ?? new ResourceStringInfo { StringKey = key };

            rsi.TranslationText = translation;
            if (CultureInfoProvider.GetCultureID(CultureHelper.PreferredUICultureCode) != 0)
            {
                rsi.CultureCode = CultureHelper.PreferredUICultureCode;
            }
            else
            {
                rsi.CultureCode = CultureHelper.DefaultUICultureCode;
            }

            try
            {
                ResourceStringInfoProvider.SetResourceStringInfo(rsi);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
            }
        }

        #endregion
    }
}

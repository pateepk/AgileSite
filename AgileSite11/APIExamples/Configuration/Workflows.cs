using System;

using CMS.WorkflowEngine;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Localization;
using CMS.DocumentEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to workflow definitions.
    /// </summary>
    /// <pageTitle>Workflows</pageTitle>
    internal class WorkflowsMain
    {
        /// <summary>
        /// Holds workflow definition API examples.
        /// </summary>
        /// <groupHeading>Workflows</groupHeading>
        private class Workflows
        {
            /// <heading>Creating a new workflow</heading>
            private void CreateWorkflow()
            {
                // Creates a new workflow object
                WorkflowInfo newWorkflow = new WorkflowInfo();

                // Sets the workflow properties
                newWorkflow.WorkflowDisplayName = "New workflow";
                newWorkflow.WorkflowName = "NewWorkflow";

                // Saves the workflow to the database
                WorkflowInfoProvider.SetWorkflowInfo(newWorkflow);

                // Creates the three default workflow steps
                WorkflowStepInfoProvider.CreateDefaultWorkflowSteps(newWorkflow);
            }


            /// <heading>Updating a workflow</heading>
            private void GetAndUpdateWorkflow()
            {
                // Gets the workflow
                WorkflowInfo updateWorkflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");
                if (updateWorkflow != null)
                {
                    // Updates the workflow properties
                    updateWorkflow.WorkflowDisplayName = updateWorkflow.WorkflowDisplayName.ToLowerCSafe();

                    // Saves the modified workflow to the database
                    WorkflowInfoProvider.SetWorkflowInfo(updateWorkflow);
                }
            }


            /// <heading>Updating multiple workflows</heading>
            private void GetAndBulkUpdateWorkflows()
            {
                // Gets all workflows whose code name starts with 'New'
                var workflows = WorkflowInfoProvider.GetWorkflows().WhereStartsWith("WorkflowName", "New");
                
                // Loops through individual workflows
                foreach (WorkflowInfo modifyWorkflow in workflows)
                {
                    // Updates the workflow properties
                    modifyWorkflow.WorkflowDisplayName = modifyWorkflow.WorkflowDisplayName.ToUpper();

                    // Saves the modified workflow to the database
                    WorkflowInfoProvider.SetWorkflowInfo(modifyWorkflow);
                }
            }


            /// <heading>Deleting a workflow</heading>
            private void DeleteWorkflow()
            {
                // Gets the workflow
                WorkflowInfo deleteWorkflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (deleteWorkflow != null)
                {
                    // Deletes the workflow
                    WorkflowInfoProvider.DeleteWorkflowInfo(deleteWorkflow);
                }
            }
        }


        /// <summary>
        /// Holds workflow step API examples.
        /// </summary>
        /// <groupHeading>Workflow steps</groupHeading>
        private class WorkflowSteps
        {
            /// <heading>Adding steps to a workflow</heading>
            private void CreateWorkflowStep()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");
                if (workflow != null)
                {
                    // Creates a new custom workflow step object
                    WorkflowStepInfo newStep = new WorkflowStepInfo();

                    // Sets the workflow step properties
                    newStep.StepWorkflowID = workflow.WorkflowID;
                    newStep.StepName = "NewWorkflowStep";
                    newStep.StepDisplayName = "New workflow step";
                    newStep.StepOrder = 1;
                    newStep.StepType = WorkflowStepTypeEnum.Standard;

                    // Saves the step to the database
                    WorkflowStepInfoProvider.SetWorkflowStepInfo(newStep);

                    // Ensures correct step order for the workflow
                    WorkflowStepInfoProvider.InitStepOrders(workflow);
                }
            }


            /// <heading>Allowing roles to make workflow step transitions</heading>
            private void AddRoleToStep()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");
                if (workflow != null)
                {
                    // Gets the custom workflow step
                    WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewWorkflowStep", workflow.WorkflowID);
                    if (step != null)
                    {
                        // Gets the role
                        RoleInfo role = RoleInfoProvider.GetRoleInfo("Editor", SiteContext.CurrentSiteID);
                        if (role != null)
                        {
                            // Configures the workflow step to be managed only by assigned roles
                            step.StepRolesSecurity = WorkflowStepSecurityEnum.OnlyAssigned;

                            // Assigns the role to the workflow step
                            WorkflowStepRoleInfoProvider.AddRoleToWorkflowStep(step.StepID, role.RoleID);
                        }
                    }
                }
            }


            /// <heading>Removing roles from workflow steps</heading>
            private void RemoveRoleFromStep()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");
                if (workflow != null)
                {
                    // Gets the custom workflow step
                    WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewWorkflowStep", workflow.WorkflowID);
                    if (step != null)
                    {
                        // Gets the role
                        RoleInfo role = RoleInfoProvider.GetRoleInfo("Editor", SiteContext.CurrentSiteID);
                        if (role != null)
                        {
                            // Gets relationship between the role and the workflow step
                            WorkflowStepRoleInfo stepRoleInfo = WorkflowStepRoleInfoProvider.GetWorkflowStepRoleInfo(step.StepID, role.RoleID);

                            if (stepRoleInfo != null)
                            {
                                // Removes the role from the workflow step
                                WorkflowStepRoleInfoProvider.RemoveRoleFromWorkflowStep(step.StepID, role.RoleID);
                            }                            
                        }                        
                    }                    
                }
            }


            /// <heading>Deleting a workflow step</heading>
            private void DeleteWorkflowStep()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Gets the custom workflow step
                    WorkflowStepInfo deleteStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewWorkflowStep", workflow.WorkflowID);

                    if (deleteStep != null)
                    {
                        // Removes the step from the workflow
                        WorkflowStepInfoProvider.DeleteWorkflowStepInfo(deleteStep);                        
                    }
                }
            }
        }


        /// <summary>
        /// Holds workflow scope API examples.
        /// </summary>
        /// <groupHeading>Workflow scopes</groupHeading>
        private class WorkflowScopes
        {
            /// <heading>Creating a scope for a workflow</heading>
            private void CreateWorkflowScope()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Creates a new workflow scope object
                    WorkflowScopeInfo newScope = new WorkflowScopeInfo();

                    // Reads the code of the current site's default culture from the settings
                    string cultureCode = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSDefaultCultureCode");

                    // Gets the culture object based on the culture code
                    CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureCode);

                    // Gets the class ID of the website root page type
                    int classID = DataClassInfoProvider.GetDataClassInfo(SystemDocumentTypes.Root).ClassID;

                    // Sets the properties of the workflow scope
                    newScope.ScopeStartingPath = "/";
                    newScope.ScopeCultureID = culture.CultureID;
                    newScope.ScopeClassID = classID;
                    newScope.ScopeExcluded = false;

                    // Assigns the scope to the workflow for the current site
                    newScope.ScopeWorkflowID = workflow.WorkflowID;
                    newScope.ScopeSiteID = SiteContext.CurrentSiteID;

                    // Saves the workflow scope to the database
                    WorkflowScopeInfoProvider.SetWorkflowScopeInfo(newScope);
                }
            }


            /// <heading>Updating workflow scopes</heading>
            private void GetAndUpdateWorkflowScope()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Gets all scopes assigned to the specified workflow
                    var scopes = WorkflowScopeInfoProvider.GetWorkflowScopes().WhereEquals("ScopeWorkflowID", workflow.WorkflowID);
                    
                    // Loops through the workflow's scopes
                    foreach (WorkflowScopeInfo updateScope in scopes)
                    {
                        // Updates the scope to include all cultures and page types
                        updateScope.ScopeCultureID = 0;
                        updateScope.ScopeClassID = 0;

                        // Saves the updated workflow scope to the database
                        WorkflowScopeInfoProvider.SetWorkflowScopeInfo(updateScope);
                    }
                }
            }


            /// <heading>Deleting workflow scopes</heading>
            private void DeleteWorkflowScope()
            {
                // Gets the workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Gets all scopes assigned to the specified workflow
                    var scopes = WorkflowScopeInfoProvider.GetWorkflowScopes().WhereEquals("ScopeWorkflowID", workflow.WorkflowID);
                    
                    // Loops through the workflow's scopes
                    foreach (WorkflowScopeInfo deleteScope in scopes)
                    {                        
                        // Deletes the workflow scope
                        WorkflowScopeInfoProvider.DeleteWorkflowScopeInfo(deleteScope);
                    }                    
                }
            }
        }


        /// <summary>
        /// Holds advanced workflow API examples.
        /// </summary>
        /// <groupHeading>Advanced workflow</groupHeading>
        private class AdvancedWorkflow
        {
            /// <heading>Converting a basic workflow to advanced</heading>
            private void ConvertToAdvancedWorkflow()
            {
                // Gets the workflow
                WorkflowInfo convertWorkflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (convertWorkflow != null)
                {
                    // Converts the workflow to an advanced workflow
                    WorkflowInfoProvider.ConvertToAdvancedWorkflow(convertWorkflow.WorkflowID);
                }
            }


            /// <heading>Creating advanced workflow actions</heading>
            private void CreateAction()
            {
                // Creates a new advanced workflow action
                WorkflowActionInfo newAction = new WorkflowActionInfo();

                // Sets the action properties
                newAction.ActionDisplayName = "New action";
                newAction.ActionName = "NewAction";
                newAction.ActionAssemblyName = "NewActionAssembly";
                newAction.ActionClass = "Namespace.NewActionClass";
                newAction.ActionEnabled = true;
                newAction.ActionWorkflowType = WorkflowTypeEnum.Approval;

                // Saves the advanced workflow action to the database
                WorkflowActionInfoProvider.SetWorkflowActionInfo(newAction);
            }


            /// <heading>Updating advanced workflow actions</heading>
            private void GetAndUpdateAction()
            {
                // Gets the advanced workflow action
                WorkflowActionInfo updateAction = WorkflowActionInfoProvider.GetWorkflowActionInfo("NewAction", WorkflowTypeEnum.Approval);
                if (updateAction != null)
                {
                    // Updates the workflow action properties
                    updateAction.ActionDisplayName = updateAction.ActionDisplayName.ToLowerCSafe();

                    // Saves the updated workflow action to the database
                    WorkflowActionInfoProvider.SetWorkflowActionInfo(updateAction);
                }
            }


            /// <heading>Updating multiple advanced workflow actions</heading>
            private void GetAndBulkUpdateActions()
            {
                // Gets all advanced workflow actions whose name starts with 'New'
                var actions = WorkflowActionInfoProvider.GetWorkflowActions().WhereStartsWith("ActionName", "New");
                
                // Loops through individual workflow actions
                foreach (WorkflowActionInfo modifyAction in actions)
                {
                    // Updates the workflow actions properties
                    modifyAction.ActionDisplayName = modifyAction.ActionDisplayName.ToUpper();

                    // Saves the updated workflow action to the database
                    WorkflowActionInfoProvider.SetWorkflowActionInfo(modifyAction);
                }
            }


            /// <heading>Deleting advanced workflow actions</heading>
            private void DeleteAction()
            {
                // Gets the advanced workflow action
                WorkflowActionInfo deleteAction = WorkflowActionInfoProvider.GetWorkflowActionInfo("NewAction", WorkflowTypeEnum.Approval);                

                if (deleteAction != null)
                {
                    // Deletes the workflow action
                    WorkflowActionInfoProvider.DeleteWorkflowActionInfo(deleteAction);
                }
            }


            /// <heading>Adding steps to an advanced workflow</heading>
            private void CreateStepForTransition()
            {
                // Gets the advanced workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Creates a new step for the workflow
                    WorkflowStepInfo newStep = new WorkflowStepInfo()
                    {
                        StepWorkflowID = workflow.WorkflowID,
                        StepName = "NewStep",
                        StepDisplayName = "New step",
                        StepType = WorkflowStepTypeEnum.Standard
                    };

                    // Saves the workflow step to the database
                    WorkflowStepInfoProvider.SetWorkflowStepInfo(newStep);
                }
            }


            /// <heading>Creating transitions between advanced workflow steps</heading>
            private void CreateTransition()
            {
                // Gets the advanced workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Gets the workflow's 'NewStep' and 'Published' steps
                    WorkflowStepInfo newStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewStep", workflow.WorkflowID);
                    WorkflowStepInfo publishedStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("Published", workflow.WorkflowID);

                    if ((newStep != null) && (publishedStep != null))
                    {
                        // Gets the workflow's existing transition leading to the 'Published' step
                        WorkflowTransitionInfo existingTransition = WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                                    .WhereEquals("StepWorkflowID", workflow.WorkflowID)
                                                                                                    .WhereEquals("TransitionEndStepID", publishedStep.StepID)
                                                                                                    .FirstObject;

                        // Modifies the existing transition to lead to the 'NewStep' step instead of the 'Published' step
                        existingTransition.TransitionEndStepID = newStep.StepID;

                        // Saves the updated transition to the database
                        WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(existingTransition);

                        // Creates a new transition from the 'NewStep' step to the 'Published' step
                        newStep.ConnectTo(newStep.StepDefinition.SourcePoints[0].Guid, publishedStep);
                    }
                }
            }


            /// <heading>Deleting transitions between advanced workflow steps</heading>
            private void DeleteTransition()
            {
                // Gets the advanced workflow
                WorkflowInfo workflow = WorkflowInfoProvider.GetWorkflowInfo("NewWorkflow");

                if (workflow != null)
                {
                    // Gets the workflow's 'NewStep' step
                    WorkflowStepInfo startStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewStep", workflow.WorkflowID);

                    if (startStep != null)
                    {
                        // Gets all transitions leading from the workflow's 'NewStep' step
                        var existingTransitions = WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                                    .WhereEquals("StepWorkflowID", workflow.WorkflowID)
                                                                                                    .WhereEquals("TransitionStartStepID", startStep.StepID);

                        // Loops through the step transitions
                        foreach (WorkflowTransitionInfo deleteTransition in existingTransitions)
                        {
                            // Deletes the step transition
                            WorkflowTransitionInfoProvider.DeleteWorkflowTransitionInfo(deleteTransition);
                        }
                    }
                }
            }
        }
    }
}

using System;

using CMS.WorkflowEngine;
using CMS.SiteProvider;
using CMS.Automation;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds marketing automation API examples.
    /// </summary>
    /// <pageTitle>Marketing automation</pageTitle>
    internal class MarketingAutomation
    {
        /// <summary>
        /// Holds automation process API examples.
        /// </summary>
        /// <groupHeading>Processes</groupHeading>
        private class Processes
        {
            /// <heading>Creating an automation process</heading>
            private void CreateProcess()
            {
                // Creates a new marketing automation process object
                WorkflowInfo newProcess = new WorkflowInfo()
                {
                    // Sets the process properties
                    WorkflowDisplayName = "New process",
                    WorkflowName = "NewProcess",
                    WorkflowType = WorkflowTypeEnum.Automation,
                    WorkflowRecurrenceType = ProcessRecurrenceTypeEnum.Recurring
                };

                // Saves the new process to the database
                WorkflowInfoProvider.SetWorkflowInfo(newProcess);

                // Creates default steps for the process
                WorkflowStepInfoProvider.CreateDefaultWorkflowSteps(newProcess);

                // Gets the step with codename 'Finished' and allows moving to the previous step
                WorkflowStepInfo finishedStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("Finished", newProcess.WorkflowID);
                finishedStep.StepAllowReject = true;

                // Saves the modified 'Finished' step to the database
                WorkflowStepInfoProvider.SetWorkflowStepInfo(finishedStep);
            }


            /// <heading>Updating a process</heading>
            private void GetAndUpdateProcess()
            {
                // Gets the marketing automation process
                WorkflowInfo modifyProcess = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (modifyProcess != null)
                {
                    // Updates the process properties
                    modifyProcess.WorkflowDisplayName = modifyProcess.WorkflowDisplayName.ToLower();

                    // Saves the modified process to the database
                    WorkflowInfoProvider.SetWorkflowInfo(modifyProcess);
                }
            }


            /// <heading>Updating multiple processes</heading>
            private void GetAndBulkUpdateProcesses()
            {
                // Gets all marketing automation processes whose code name starts with 'New'
                var processes = WorkflowInfoProvider.GetWorkflows()
                                                        .WhereEquals("WorkflowType", WorkflowTypeEnum.Automation)
                                                        .WhereStartsWith("WorkflowName", "New");                

                // Loops through individual processes
                foreach (WorkflowInfo modifyProcess in processes)
                {
                    // Updates the process properties
                    modifyProcess.WorkflowDisplayName = modifyProcess.WorkflowDisplayName.ToUpper();

                    // Saves the modified process to the database
                    WorkflowInfoProvider.SetWorkflowInfo(modifyProcess);
                }
            }


            /// <heading>Deleting processes</heading>
            private void DeleteProcess()
            {
                // Gets the marketing automation process
                WorkflowInfo deleteProcess = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (deleteProcess != null)
                {
                    // Deletes the process
                    WorkflowInfoProvider.DeleteWorkflowInfo(deleteProcess);
                }
            }
        }


        /// <summary>
        /// Holds automation process step API examples.
        /// </summary>
        /// <groupHeading>Process steps</groupHeading>
        private class ProcessSteps
        {
            /// <heading>Adding steps to a process</heading>
            private void CreateProcessStep()
            {
                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (process != null)
                {
                    // Creates a new process step object
                    WorkflowStepInfo newStep = new WorkflowStepInfo()
                    {
                        // Sets the step properties
                        StepWorkflowID = process.WorkflowID,
                        StepName = "NewProcessStep",
                        StepDisplayName = "New step",
                        StepType = WorkflowStepTypeEnum.Standard
                    };

                    // Saves the process step to the database
                    WorkflowStepInfoProvider.SetWorkflowStepInfo(newStep);
                }
            }


            /// <heading>Updating a step in a process</heading>
            private void GetAndUpdateProcessStep()
            {
                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (process != null)
                {
                    // Gets the process step
                    WorkflowStepInfo modifyStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewProcessStep", process.WorkflowID);

                    if (modifyStep != null)
                    {
                        // Updates the step properties
                        modifyStep.StepDisplayName = modifyStep.StepDisplayName.ToLower();

                        // Saves the updated step to the database
                        WorkflowStepInfoProvider.SetWorkflowStepInfo(modifyStep);
                    }
                }
            }


            /// <heading>Updating multiple steps in a process</heading>
            private void GetAndBulkUpdateProcessSteps()
            {
                // Gets a marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                // Gets all steps defined for the specified process whose name starts with 'New'
                var steps = WorkflowStepInfoProvider.GetWorkflowSteps()
                                                            .WhereEquals("StepWorkflowID", process.WorkflowID)
                                                            .WhereStartsWith("StepName", "New");

                // Loops through individual steps
                foreach (WorkflowStepInfo modifyStep in steps)
                {
                    // Updates the step properties
                    modifyStep.StepDisplayName = modifyStep.StepDisplayName.ToUpper();

                    // Saves the updated step to the database
                    WorkflowStepInfoProvider.SetWorkflowStepInfo(modifyStep);
                }
            }


            /// <heading>Creating transitions between process steps</heading>
            private void CreateProcessTransitions()
            {
                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (process != null)
                {
                    // Gets the 'NewProcessStep' and 'Finished' steps created process step
                    WorkflowStepInfo newStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("NewProcessStep", process.WorkflowID);
                    WorkflowStepInfo finishedStep = WorkflowStepInfoProvider.GetWorkflowStepInfo("Finished", process.WorkflowID);

                    if ((newStep != null) && (finishedStep != null))
                    {
                        // Gets the existing transition leading to the 'Published' step for the process
                        WorkflowTransitionInfo existingTransition = WorkflowTransitionInfoProvider.GetWorkflowTransitions()
                                                                                                    .WhereEquals("TransitionWorkflowID", process.WorkflowID)
                                                                                                    .WhereEquals("TransitionEndStepID", finishedStep.StepID)
                                                                                                    .FirstObject;

                        // Modifies the existing transition to lead to the 'NewProcessStep' instead of the 'Finished' step
                        existingTransition.TransitionEndStepID = newStep.StepID;

                        // Saves the updated transition ot the database
                        WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(existingTransition);
                        
                        // Creates a new transition from the 'NewProcessStep' step to the 'Finished' step
                        newStep.ConnectTo(newStep.StepDefinition.SourcePoints[0].Guid, finishedStep);
                    }
                }
            }
        }


        /// <summary>
        /// Holds automation process trigger API examples.
        /// </summary>
        /// <groupHeading>Process triggers</groupHeading>
        private class ProcessTriggers
        {
            /// <heading>Creating a process trigger</heading>
            private void CreateProcessTrigger()
            {
                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if (process != null)
                {
                    // Creates a new process trigger object
                    ObjectWorkflowTriggerInfo newTrigger = new ObjectWorkflowTriggerInfo()
                    {
                        // Sets the trigger properties
                        TriggerDisplayName = "New trigger",
                        TriggerType = WorkflowTriggerTypeEnum.Change,
                        TriggerWorkflowID = process.WorkflowID,
                        TriggerObjectType = "om.contact"
                    };

                    // Saves the process trigger to the database
                    ObjectWorkflowTriggerInfoProvider.SetObjectWorkflowTriggerInfo(newTrigger);
                }
            }


            /// <heading>Updating process triggers</heading>
            private void GetAndBulkUpdateProcessTriggers()
            {
                // Gets a marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);                

                // Gets all triggers defined for the given process
                var triggers = ObjectWorkflowTriggerInfoProvider.GetObjectWorkflowTriggers()
                                                                        .WhereEquals("TriggerWorkflowID", process.WorkflowID);

                // Loops through individual triggers
                foreach (ObjectWorkflowTriggerInfo modifyTrigger in triggers)
                {
                    // Updates the trigger properties
                    modifyTrigger.TriggerDisplayName = modifyTrigger.TriggerDisplayName.ToUpper();

                    // Saves the modified trigger to the database
                    ObjectWorkflowTriggerInfoProvider.SetObjectWorkflowTriggerInfo(modifyTrigger);
                }
            }


            /// <heading>Deleting process triggers</heading>
            private void DeleteProcessTrigger()
            {
                // Gets a marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                // Gets all triggers defined for the given process whose code name starts with 'New"
                var triggers = ObjectWorkflowTriggerInfoProvider.GetObjectWorkflowTriggers()
                                                                        .WhereEquals("TriggerWorkflowID", process.WorkflowID)
                                                                        .WhereStartsWith("TriggerDisplayName", "New");

                // Loops through individual triggers
                foreach (ObjectWorkflowTriggerInfo deleteTrigger in triggers)
                {
                    // Deletes the trigger
                    ObjectWorkflowTriggerInfoProvider.DeleteObjectWorkflowTriggerInfo(deleteTrigger);
                }
            }
        }


        /// <summary>
        /// Holds automation process management API examples.
        /// </summary>
        /// <groupHeading>Process management</groupHeading>
        private class ProcessManagement
        {
            /// <heading>Starting an automation process for a contact</heading>
            private void StartAutomationProcess()
            {
                // Gets the first contact in the system whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                            .WhereEquals("ContactLastName", "Smith")
                                                            .FirstObject;

                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if ((contact != null) && (process != null))
                {
                    // Creates an automation manager instance
                    AutomationManager manager = AutomationManager.GetInstance(MembershipContext.AuthenticatedUser);

                    // Starts the process for the contact
                    manager.StartProcess(contact, process.WorkflowID);
                }
            }


            /// <heading>Moving a contact between steps in a process</heading>
            private void MoveContactToStep()
            {
                // Gets the first contact in the system whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                            .WhereEquals("ContactLastName", "Smith")
                                                            .FirstObject;

                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if ((contact != null) && (process != null))
                {
                    // Creates an automation manager instance
                    AutomationManager manager = AutomationManager.GetInstance(MembershipContext.AuthenticatedUser);

                    // Gets the contact's current state in the given process
                    // Note: This example only gets the first state object
                    // You may need to handle multiple states if the process is allowed to run multiple process instances concurrently for the same contact
                    AutomationStateInfo processState = AutomationStateInfoProvider.GetAutomationStates()
                                                                                        .WhereEquals("StateObjectType", ContactInfo.OBJECT_TYPE)
                                                                                        .WhereEquals("StateObjectID", contact.ContactID)
                                                                                        .WhereEquals("StateWorkflowID", process.WorkflowID)
                                                                                        .FirstObject;

                    if (processState != null)
                    {
                        // Decides where to move the contact within the process
                        string moveOperation = "next"; // "previous", "finished"

                        switch (moveOperation)
                        {
                            case "next":
                                // Moves the contact to the next step in the process
                                manager.MoveToNextStep(contact, processState, "Moved to the next step");                                
                                break;

                            case "previous":
                                // Moves the contact to the previous step in the process
                                manager.MoveToPreviousStep(contact, processState, "Moved to the previous step");                                
                                break;

                            case "finished":
                                // Gets the finished step based on the process state
                                WorkflowStepInfo finishedStep = manager.GetFinishedStep(contact, processState);

                                // Moves the contact to the finished state
                                manager.MoveToSpecificStep(contact, processState, finishedStep, "Moved to the finished step");                                
                                break;
                        }
                    }
                }
            }


            /// <heading>Removing a contact from an automation process</heading>
            private void RemoveContactFromProcess()
            {
                // Gets the first contact in the system whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                            .WhereEquals("ContactLastName", "Smith")
                                                            .FirstObject;

                // Gets the marketing automation process
                WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo("NewProcess", WorkflowTypeEnum.Automation);

                if ((contact != null) && (process != null))
                {
                    // Creates an automation manager instance
                    AutomationManager manager = AutomationManager.GetInstance(MembershipContext.AuthenticatedUser);

                    // Gets the states of all instances of the process that are running for the contact
                    var states = AutomationStateInfoProvider.GetAutomationStates()
                                                            .WhereEquals("StateObjectType", ContactInfo.OBJECT_TYPE)
                                                            .WhereEquals("StateWorkflowID", process.WorkflowID)
                                                            .WhereEquals("StateObjectID", contact.ContactID);                                                            

                    if (states.Count > 0)
                    {
                        // Loops through the contact's states in the given process
                        // There will typically be only one state unless several instances of the process are running concurrently for the same contact
                        foreach (AutomationStateInfo state in states)
                        {
                            // Removes the contact from the process
                            manager.RemoveProcess(contact, state);
                        }
                    }
                }
            }
        }
    }
}

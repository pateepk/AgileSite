using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.Membership;
using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;

namespace CMS.Automation
{
    /// <summary>
    /// Implementation of IAutomationTrigger based on TriggerInfo.
    /// </summary>
    public class Trigger : ITrigger
    {
        #region "Variables

        private readonly ObjectWorkflowTriggerInfo triggerInfo;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public Trigger(ObjectWorkflowTriggerInfo triggerInfo)
        {
            if (triggerInfo == null)
            {
                throw new ArgumentNullException("triggerInfo");
            }
            this.triggerInfo = triggerInfo;
        }


        /// <summary>
        /// Processes the given collection of trigger options and resolves trigger macro on given object and creates workflow process if successful.
        /// </summary>
        /// <param name="options">Collection of <see cref="TriggerOptions"/> objects to process</param>
        public void Process(IEnumerable<TriggerOptions> options)
        {
            options = options.ToHashSetCollection();

            // Handle the event
            using (var h = AutomationEvents.ProcessTrigger.StartEvent(triggerInfo, options))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    var filteredOptions = options
                        // Check if trigger should ignore process
                        .Where(o => !o.IgnoreProcesses.Contains(triggerInfo.TriggerWorkflowID))
                        // Check if trigger can process given types
                        .Where(o => (o.Types == null) || o.Types.Contains(triggerInfo.TriggerType))
                        // Check if trigger passes thru validation function
                        .Where(o => (o.PassFunction == null) || o.PassFunction(triggerInfo))
                        // Check if trigger macro condition passes for options
                        .Where(o =>
                        {
                            if (String.IsNullOrEmpty(triggerInfo.TriggerMacroCondition))
                            {
                                return true;
                            }

                            var resolver = o.Resolver;

                            // Must have some context to process macros on
                            if (resolver == null)
                            {
                                throw new InvalidOperationException("Field Resolver in options argument is null.");
                            }

                            return ValidationHelper.GetBoolean(resolver.ResolveMacros(triggerInfo.TriggerMacroCondition), false);
                        }).ToList();

                    if (filteredOptions.Count > 0)
                    {
                        // All conditions passed - try to add BaseInfo to process
                        using (new CMSActionContext { UseGlobalAdminContext = true })
                        {
                            var automationManager = AutomationManager.GetInstance(UserInfoProvider.AdministratorUser);

                            // Create list of objects to process with its additional data
                            var objectsToProcess = filteredOptions.Select(o => new AutomationProcessItem<BaseInfo>
                            {
                                InfoObject = o.Info,
                                AdditionalData = o.AdditionalData
                            });

                            automationManager.StartProcess(objectsToProcess, triggerInfo.TriggerWorkflowID, triggerInfo);
                        }
                        
                        // Successful or not, ignore it next time
                        filteredOptions.ForEach(o => o.IgnoreProcesses.Add(triggerInfo.TriggerWorkflowID));
                    }
                }

                // Finalize the event
                h.FinishEvent();
            }
        }

        #endregion
    }
}

using System.Collections.Generic;

using CMS.Base;

namespace CMS.Automation
{
    /// <summary>
    /// Automation process trigger handler
    /// </summary>
    public class AutomationProcessTriggerHandler : AdvancedHandler<AutomationProcessTriggerHandler, AutomationProcessTriggerEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="triggerInfo">Trigger info object</param>
        /// <param name="options">Trigger options</param>
        public AutomationProcessTriggerHandler StartEvent(ObjectWorkflowTriggerInfo triggerInfo, IEnumerable<TriggerOptions> options)
        {
            AutomationProcessTriggerEventArgs e = new AutomationProcessTriggerEventArgs
            {
                Options = options,
                TriggerInfo = triggerInfo
            };

            return StartEvent(e);
        }
    }
}
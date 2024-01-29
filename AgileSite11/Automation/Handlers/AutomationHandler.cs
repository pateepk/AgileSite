using CMS.DataEngine;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Automation handler
    /// </summary>
    public class AutomationHandler : AdvancedHandler<AutomationHandler, AutomationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        /// <param name="stateObj">State object</param>
        public AutomationHandler StartEvent(BaseInfo infoObj, AutomationStateInfo stateObj)
        {
            AutomationEventArgs e = new AutomationEventArgs
            {
                InfoObject = infoObj,
                StateObject = stateObj
            };

            return StartEvent(e, true);
        }
    }
}
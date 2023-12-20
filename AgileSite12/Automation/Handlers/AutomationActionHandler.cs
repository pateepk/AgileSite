using CMS.DataEngine;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Automation action handler
    /// </summary>
    public class AutomationActionHandler : AdvancedHandler<AutomationActionHandler, WorkflowActionEventArgs<BaseInfo, AutomationStateInfo, AutomationActionEnum>>
    {
    }
}
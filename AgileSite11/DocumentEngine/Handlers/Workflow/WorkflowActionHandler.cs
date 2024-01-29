using CMS.DataEngine;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Workflow action handler
    /// </summary>
    public class WorkflowActionHandler : AdvancedHandler<WorkflowActionHandler, WorkflowActionEventArgs<TreeNode, BaseInfo, WorkflowActionEnum>>
    {
    }
}
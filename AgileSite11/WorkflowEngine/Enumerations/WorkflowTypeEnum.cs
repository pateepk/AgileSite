using CMS.Helpers;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow type enumeration.
    /// </summary>
    public enum WorkflowTypeEnum
    {
        /// <summary>
        /// Basic approval workflow for documents.
        /// </summary>
        [EnumStringRepresentation("workflow.type.basic")]
        Basic = 0,

        /// <summary>
        /// Advanced approval workflow.
        /// </summary>
        [EnumStringRepresentation("workflow.type.approval")]
        Approval = 1,

        /// <summary>
        /// General advanced workflow.
        /// </summary>
        [EnumStringRepresentation("workflow.type.advanced")]
        Advanced = 2,

        /// <summary>
        /// Automation workflow.
        /// </summary>
        [EnumStringRepresentation("workflow.type.automation")]
        Automation = 3
    }
}
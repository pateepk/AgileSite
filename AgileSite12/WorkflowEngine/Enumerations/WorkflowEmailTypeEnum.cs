namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow email type enumeration.
    /// </summary>
    public enum WorkflowEmailTypeEnum : int
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Template for e-mails sent to editor/moderator when document is approved.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Template for e-mails sent when document is approved.
        /// </summary>
        ReadyForApproval = 2,

        /// <summary>
        /// Template for e-mails sent when document is rejected.
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Template for e-mails sent when document is published.
        /// </summary>
        Published = 4,

        /// <summary>
        /// Template for e-mails sent when document is archived.
        /// </summary>
        Archived = 5,

        /// <summary>
        /// Template for general notification e-mails.
        /// </summary>
        Notification = 6
    }
}
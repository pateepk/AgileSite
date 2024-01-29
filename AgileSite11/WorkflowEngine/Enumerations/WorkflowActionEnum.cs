namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow action enumeration.
    /// </summary>
    public enum WorkflowActionEnum
    {
        /// <summary>
        /// Unknown action.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The document is approved - send e-mail to the users that are approve within next step and to the document editor.
        /// </summary>
        Approve = 1,

        /// <summary>
        /// Document is rejected - send e-mail to the user that reject the document and to the document editor.
        /// </summary>
        Reject = 2,

        /// <summary>
        /// The document is published - send e-mail to the document editor.
        /// </summary>
        Publish = 3,

        /// <summary>
        /// The document is archived - send e-mail to the document editor.
        /// </summary>
        Archive = 4,

        /// <summary>
        /// The document is moved to specific step.
        /// </summary>
        MoveToSpecificStep = 5
    }
}
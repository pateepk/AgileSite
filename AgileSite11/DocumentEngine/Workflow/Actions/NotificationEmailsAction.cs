namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class representing action to send workflow notification e-mails.
    /// </summary>
    public class NotificationEmailsAction: DocumentWorkflowAction
    {
        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            // Does not do anything, because notification e-mails sending works automatically.
        }
    }
}

using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include recipient and attachments for a draft message.
    /// </summary>
    internal sealed class DraftMessageModifier : IEmailMessageModifier
    {
        private readonly string recipient;
        private readonly IssueInfo issue;


        /// <summary>
        /// Creates an instance of <see cref="DraftMessageModifier"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="recipient">Email recipient.</param>
        public DraftMessageModifier(IssueInfo issue, string recipient)
        {
            this.issue = issue;
            this.recipient = recipient;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            AddRecipients(message);
            AttachFiles(message);
        }


        private void AddRecipients(EmailMessage message)
        {
            message.Recipients = recipient;
        }


        private void AttachFiles(EmailMessage message)
        {
            new AttachmentsMessageModifier(issue).Apply(message);
        }
    }
}

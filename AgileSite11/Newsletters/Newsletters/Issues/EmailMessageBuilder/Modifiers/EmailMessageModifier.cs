using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include recipient and tracking fields.
    /// </summary>
    internal sealed class EmailMessageModifier : IEmailMessageModifier
    {
        private readonly SubscriberInfo subscriber;
        private readonly IssueInfo issue;


        /// <summary>
        /// Creates an instance of <see cref="EmailMessageModifier"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>
        public EmailMessageModifier(IssueInfo issue, SubscriberInfo subscriber)
        {
            this.issue = issue;
            this.subscriber = subscriber;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            AddRecipients(message);
            AddTrackingFieldsToMessageHeader(message);
            AttachFiles(message);
        }


        private void AddRecipients(EmailMessage message)
        {
            new RecipientsMessageModifier(subscriber).Apply(message);
        }


        private void AttachFiles(EmailMessage message)
        {
            new AttachmentsMessageModifier(issue).Apply(message);
        }


        private void AddTrackingFieldsToMessageHeader(EmailMessage message)
        {
            new MessageHeaderTrackingFieldModifier(issue, subscriber).Apply(message);
        }
    }
}

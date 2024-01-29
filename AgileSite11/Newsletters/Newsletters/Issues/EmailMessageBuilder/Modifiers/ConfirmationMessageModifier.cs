using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include recipient and attachments for a confirmation message.
    /// </summary>
    internal sealed class ConfirmationMessageModifier : IEmailMessageModifier
    {
        private readonly SubscriberInfo subscriber;
        private readonly EmailTemplateInfo template;


        /// <summary>
        /// Creates an instance of <see cref="ConfirmationMessageModifier"/> class.
        /// </summary>
        /// <param name="template">Email template.</param>
        /// <param name="subscriber">Subscriber.</param>
        public ConfirmationMessageModifier(EmailTemplateInfo template, SubscriberInfo subscriber)
        {
            this.template = template;
            this.subscriber = subscriber;
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
            new RecipientsMessageModifier(subscriber).Apply(message);
        }


        private void AttachFiles(EmailMessage message)
        {
            new ConfirmationAttachmentsMessageModifier(template).Apply(message);
        }
    }
}

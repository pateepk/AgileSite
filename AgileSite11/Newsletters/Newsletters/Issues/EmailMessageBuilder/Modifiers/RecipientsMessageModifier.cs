using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include correct recipient.
    /// </summary>
    internal sealed class RecipientsMessageModifier : IEmailMessageModifier
    {
        private readonly SubscriberInfo subscriber;


        /// <summary>
        /// Creates an instance of <see cref="RecipientsMessageModifier"/> class.
        /// </summary>
        /// <param name="subscriber">Subscriber.</param>
        public RecipientsMessageModifier(SubscriberInfo subscriber)
        {
            this.subscriber = subscriber;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            message.Recipients = subscriber.SubscriberEmail;
        }
    }
}

using System;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handler for <see cref="NewsletterEvents.GenerateQueueItems"/> event.
    /// </summary>
    public class GenerateQueueItemsHandler : AdvancedHandler<GenerateQueueItemsHandler, GenerateQueueItemsEventArgs>
    {
        /// <summary>
        /// Starts event.
        /// </summary>
        /// <param name="issue">Issue which is being sent</param>
        /// <param name="subscriber">Subscriber to which an issue is being sent. Can be null to sent issue to all subscribers</param>
        /// <param name="emailAddressBlocker">Every email should be checked using this email address blocker before it is added to the email queue</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        public GenerateQueueItemsHandler Start(IssueInfo issue, SubscriberInfo subscriber, IEmailAddressBlocker emailAddressBlocker)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            var args = new GenerateQueueItemsEventArgs
            {
                Issue = issue,
                Subscriber = subscriber,
                EmailAddressBlocker = emailAddressBlocker,
            };
            return StartEvent(args);
        }
    }
}
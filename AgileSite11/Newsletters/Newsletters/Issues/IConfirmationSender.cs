using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IConfirmationSender), typeof(ConfirmationSender), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for sending newsletter issues confirmation.
    /// </summary>
    public interface IConfirmationSender
    {
        /// <summary>
        /// Sends e-mail to confirm subscription/unsubscription of subscriber to newsletter.
        /// </summary>
        /// <param name="isSubscriptionEmail">True if the message is subscription confirmation, false if unsubscription confirmation</param>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        void SendConfirmationEmail(bool isSubscriptionEmail, int subscriberId, int newsletterId);


        /// <summary>
        /// Sends double opt-in e-mail to confirm subscription.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        void SendDoubleOptInEmail(int subscriberId, int newsletterId);


        /// <summary>
        /// Sends unsubscription confirmation e-mail.
        /// </summary>
        /// <param name="subscriber">Subscriber to send email to</param>
        /// <param name="newsletterId">ID of newsletter to be unsubscribed from</param>
        void SendUnsubscriptionConfirmation(SubscriberInfo subscriber, int newsletterId);
    }
}

namespace CMS.Newsletters
{
    /// <summary>
    /// Settings defining how exactly will ISubscriptionService.Subscribe method behave.
    /// </summary>
    public class SubscribeSettings
    {
        /// <summary>
        /// If true, confirmation email will be send after successful subscription.
        /// Default is false.
        /// </summary>
        public bool SendConfirmationEmail
        {
            get;
            set;
        }


        /// <summary>
        /// If true and double opt-in is enabled for given newsletter, subscription will be created as unapproved and double opt-in email will be sent.
        /// Default is false.
        /// </summary>
        public bool AllowOptIn
        {
            get;
            set;
        }


        /// <summary>
        /// If true, email is also removed from "unsubscribe all" list so it receives newsletters again.
        /// Default is false.
        /// </summary>
        public bool RemoveAlsoUnsubscriptionFromAllNewsletters
        {
            get;
            set;
        }


        /// <summary>
        /// <see cref="SubscriberInfo"/> object with initial data used for subscriber creation.  
        /// </summary>
        public SubscriberInfo SourceSubscriber
        {
            get;
            set;
        }
    }
}
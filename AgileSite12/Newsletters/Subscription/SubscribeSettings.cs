using CMS.ContactManagement;

namespace CMS.Newsletters
{
    /// <summary>
    /// Settings defining how exactly will ISubscriptionService.Subscribe method behave.
    /// </summary>
    /// <seealso cref="ISubscriptionService.Subscribe(ContactInfo, NewsletterInfo, SubscribeSettings)"/>
    /// <seealso cref="ISubscriptionService.Subscribe(int, int, SubscribeSettings)"/>
    public class SubscribeSettings
    {
        /// <summary>
        /// If <c>true</c>, confirmation email will be send after successful subscription.
        /// Default is <c>false</c>.
        /// </summary>
        public bool SendConfirmationEmail
        {
            get;
            set;
        }


        /// <summary>
        /// If <c>true</c> and double opt-in is enabled for given newsletter, subscription will be created as unapproved and double opt-in email will be sent.
        /// Default is <c>false</c>.
        /// </summary>
        public bool AllowOptIn
        {
            get;
            set;
        }


        /// <summary>
        /// If <c>true</c>, email is also removed from "unsubscribe all" list so it receives newsletters again.
        /// Default is <c>false</c>.
        /// </summary>
        /// <remarks>When set to <c>true</c>, the option <see cref="RemoveUnsubscriptionFromNewsletter"/> is ignored.</remarks>
        public bool RemoveAlsoUnsubscriptionFromAllNewsletters
        {
            get;
            set;
        }


        /// <summary>
        /// If <c>true</c>, unsubscription is removed for specified email and newsletter.
        /// Default is <c>false</c>.
        /// </summary>
        /// <remarks>If the option <see cref="RemoveAlsoUnsubscriptionFromAllNewsletters"/> is set to <c>true</c>, both unsubscription types 
        /// are deleted for specified email address regardless the option <see cref="RemoveUnsubscriptionFromNewsletter"/>.
        /// </remarks>
        public bool RemoveUnsubscriptionFromNewsletter
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
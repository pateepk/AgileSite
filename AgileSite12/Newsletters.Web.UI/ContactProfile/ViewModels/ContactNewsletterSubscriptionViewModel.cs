namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Represents view model for the contact newsletter subscriptions component.
    /// </summary>
    public class ContactNewsletterSubscriptionViewModel
    {
        /// <summary>
        /// Gets or sets name of the newsletter.
        /// </summary>
        public string NewsletterName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets URL for the newsletter.
        /// </summary>
        public string NewsletterUrl
        {
            get;
            set;
        }
    }
}
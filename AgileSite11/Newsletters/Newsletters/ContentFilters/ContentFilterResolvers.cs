namespace CMS.Newsletters
{
    /// <summary>
    /// Email content macro resolvers for content filters.
    /// </summary>
    internal static class ContentFilterResolvers
    {
        /// <summary>
        /// Gets email content resolver for context of email enqueueing in the email queue.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <remarks>This resolver is used for the first wave of macro resolving in the email content and keeps the unresolved macros in the content for second wave.</remarks>
        public static IEmailContentMacroResolver GetQueueResolver(IssueInfo issue, NewsletterInfo newsletter)
        {
            var settings = new EmailContentMacroResolverSettings
            {
                Name = NewsletterConstants.NEWSLETTERISSUERESOLVERNAME,
                Issue = issue,
                Newsletter = newsletter,
                Site = issue.IssueSiteID,
                IsPreview = false,

                // Set macro resolver to keep unresolved macros in original form
                // and disable context macros to preserve basic subscriber macros which are resolved in second round ({%FirstName%}, {%LastName%}, {%Email%})
                KeepUnresolvedMacros = true,
                DisableContextMacros = true
            };

            return new EmailContentMacroResolver(settings);
        }


        /// <summary>
        /// Gets email content resolver for context of email sending.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="preview">Indicates if the resolver is used in context of preview.</param>
        public static IEmailContentMacroResolver GetSendResolver(IssueInfo issue, NewsletterInfo newsletter, SubscriberInfo subscriber, bool preview = false)
        {
            var settings = new EmailContentMacroResolverSettings
            {
                Name = NewsletterConstants.SUBSCRIBERRESOLVERNAME,
                Issue = issue,
                Newsletter = newsletter,
                Site = issue.IssueSiteID,
                IsPreview = preview,
                Subscriber = subscriber
            };

            return new EmailContentMacroResolver(settings);
        }


        /// <summary>
        ///  Gets email content resolver for confirmation email sending.
        /// </summary>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="subscription">Subscription.</param>
        public static EmailContentMacroResolver GetConfirmationResolver(NewsletterInfo newsletter, SubscriberInfo subscriber, SubscriberNewsletterInfo subscription)
        {
            var settings = new EmailContentMacroResolverSettings
            {
                Subscriber = subscriber,
                Newsletter = newsletter,
                Subscription = subscription,
                Site = newsletter.NewsletterSiteID
            };

            return new EmailContentMacroResolver(settings);
        }
    }
}

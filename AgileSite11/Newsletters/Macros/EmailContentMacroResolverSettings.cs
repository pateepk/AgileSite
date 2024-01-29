using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Configuration settings for email content macro resolver.
    /// </summary>
    public class EmailContentMacroResolverSettings
    {
        /// <summary>
        /// Data context of newsletter subscriber.
        /// </summary>
        public SubscriberInfo Subscriber { get; set; }


        /// <summary>
        /// Data context of newsletter.
        /// </summary>
        public NewsletterInfo Newsletter { get; set; }


        /// <summary>
        /// Data context of subscription.
        /// </summary>
        public SubscriberNewsletterInfo Subscription { get; set; }


        /// <summary>
        /// Data context of newsletter issue.
        /// </summary>
        public IssueInfo Issue { get; set; }


        /// <summary>
        /// Data context of newsletter site.
        /// </summary>
        public SiteInfoIdentifier Site { get; set; }


        /// <summary>
        /// Name of the resolver.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Indicates if the macros should be resolved in context of an email preview.
        /// </summary>
        public bool IsPreview { get; set; }


        /// <summary>
        /// If true, unresolved macros are kept in their original form.
        /// </summary>
        public bool KeepUnresolvedMacros { get; set; }


        /// <summary>
        /// If true, all the context macros are disabled (only base MacroResolver sources are checked).
        /// </summary>
        public bool DisableContextMacros { get; set; }

        
        /// <summary>
        /// Indicates if resolver uses fake (dummy, not real data) objects.
        /// </summary>
        internal bool UseFakeData { get; set; }
    }
}
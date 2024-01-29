using CMS.SiteProvider;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for injecting tracking code to URLs within email content.
    /// </summary>
    internal sealed class UrlTrackingCodeInjectorContentFilter : IEmailContentFilter
    {
        private readonly SiteInfo site;
        private readonly IssueInfo issue;
        private readonly NewsletterInfo newsletter;
        private readonly string emailAddress;


        /// <summary>
        /// Creates instance of <see cref="UrlTrackingCodeInjectorContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="site">Issue site.</param>
        /// <param name="emailAddress">Email address to be used for the links.</param>
        public UrlTrackingCodeInjectorContentFilter(IssueInfo issue, NewsletterInfo newsletter, SiteInfo site, string emailAddress)
        {
            this.issue = issue;
            this.newsletter = newsletter;
            this.site = site;
            this.emailAddress = emailAddress;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            return LinkConverter.ConvertToTracking(text, issue, newsletter, site, emailAddress);
        }
    }
}

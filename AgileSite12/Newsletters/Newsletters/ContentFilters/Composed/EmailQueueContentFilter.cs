using System;

using CMS.Core;
using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.SiteProvider;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Composed filter for transforming the email parts when enqueued to the message queue.
    /// </summary>
    internal sealed class EmailQueueContentFilter : IWidgetContentFilter
    {
        private readonly IWidgetContentFilter macroFilter;
        private readonly IEmailContentFilter trackingImageFilter;


        /// <summary>
        /// Creates instance of <see cref="EmailQueueContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="resolver">Macro resolver to use.</param>
        /// <param name="preview">Indicates if the filter is used in context of preview.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when newsletter site not found.</exception>
        public EmailQueueContentFilter(IssueInfo issue, NewsletterInfo newsletter, IEmailContentMacroResolver resolver, bool preview)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
            if (site == null)
            {
                throw new InvalidOperationException("Newsletter site not found.");
            }

            macroFilter = new MacroResolverContentFilter(resolver);

            if (NewsletterHelper.IsTrackingAvailable()  && !preview 
                && newsletter.NewsletterTrackOpenEmails)
            {
                var baseUrl = Service.Resolve<IIssueUrlService>().GetBaseUrl(newsletter);
                trackingImageFilter = new TrackingImageInjectorContentFilter(issue, site, baseUrl);
            }
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            text = macroFilter.Apply(text);
            text = trackingImageFilter?.Apply(text) ?? text;

            return text;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="code"/>.
        /// </summary>
        /// <param name="code">Widget code to transform.</param>
        /// <param name="widget">Widget configuration.</param>
        /// <returns>Code with applied filter transformation.</returns>
        public string Apply(string code, Widget widget)
        {
            return macroFilter.Apply(code, widget);      
        }
    }
}
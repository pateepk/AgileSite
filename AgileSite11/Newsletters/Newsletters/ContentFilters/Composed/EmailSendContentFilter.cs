using System;

using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.SiteProvider;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Composed filter for transforming the email content right before the sending.
    /// </summary>
    internal sealed class EmailSendContentFilter : IWidgetContentFilter
    {
        private readonly IWidgetContentFilter macroFilter;
        private readonly IEmailContentFilter urlResolverFilter;
        private readonly IEmailContentFilter replacerFilter;
        private readonly IEmailContentFilter utmFilter;
        private readonly IEmailContentFilter trackingCodeFilter;


        /// <summary>
        /// Creates instance of <see cref="EmailSendContentFilter"/> class.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="resolver">Macro resover to use.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="issue">Newsletter issue.</param>
        /// <param name="baseUrl">Base URL to use for resolving.</param>
        /// <param name="preview">Indicates if the filter is used in context of preview.</param>
        public EmailSendContentFilter(IssueInfo issue, NewsletterInfo newsletter, IEmailContentMacroResolver resolver, SubscriberInfo subscriber, string baseUrl, bool preview)
        {
            macroFilter = new MacroResolverContentFilter(resolver);
            urlResolverFilter = new UrlResolverContentFilter(baseUrl);

            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
            if (site == null)
            {
                throw new InvalidOperationException("Newsletter site not found.");
            }

            if (issue.IssueUseUTM)
            {
                utmFilter = new UrlUtmParametersInjectorContentFilter(issue, site, baseUrl);
            }

            if (subscriber != null)
            {
                replacerFilter = new MacroReplacerContentFilter(subscriber.SubscriberEmail);

                if (NewsletterHelper.IsTrackingAvailable() && !preview
                    && newsletter.NewsletterTrackClickedLinks)
                {
                    trackingCodeFilter = new UrlTrackingCodeInjectorContentFilter(issue, newsletter, site, subscriber.SubscriberEmail);
                }
            }
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            text = replacerFilter?.Apply(text) ?? text;
            text = macroFilter.Apply(text);
            text = urlResolverFilter.Apply(text);
            text = utmFilter?.Apply(text) ?? text;
            text = trackingCodeFilter?.Apply(text) ?? text;

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
            code = macroFilter.Apply(code, widget);
            code = urlResolverFilter.Apply(code);
            code = utmFilter?.Apply(code) ?? code;
            code = trackingCodeFilter?.Apply(code) ?? code;

            return code;
        }
    }
}
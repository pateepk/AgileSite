using System;

using CMS.Core;
using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Composed filter for transforming the email parts to be viewed.
    /// </summary>
    internal sealed class EmailViewContentFilter : IWidgetContentFilter
    {
        private readonly IWidgetContentFilter queueFilter;
        private readonly IWidgetContentFilter sendFilter;


        /// <summary>
        /// Creates instance of <see cref="EmailViewContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="preview">Indicates if the filter is used in context of preview.</param>
        public EmailViewContentFilter(IssueInfo issue, NewsletterInfo newsletter, SubscriberInfo subscriber, bool preview)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            var queueResolver = ContentFilterResolvers.GetQueueResolver(issue, newsletter);
            queueFilter = new EmailQueueContentFilter(issue, newsletter, queueResolver, preview);

            var sendResolver = ContentFilterResolvers.GetSendResolver(issue, newsletter, subscriber, preview);

            var baseUrl = Service.Resolve<IIssueUrlService>().GetBaseUrl(newsletter);
            sendFilter = new EmailSendContentFilter(issue, newsletter, sendResolver, subscriber, baseUrl, preview);
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            text = queueFilter.Apply(text);
            text = sendFilter.Apply(text);

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
            code = queueFilter.Apply(code, widget);
            code = sendFilter.Apply(code, widget);

            return code;
        }
    }
}
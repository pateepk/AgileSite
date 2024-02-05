using System;

using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Composed filter for transforming the email parts to be displayed within email builder.
    /// </summary>
    public sealed class EmailBuilderContentFilter : IWidgetContentFilter
    {
        private readonly IWidgetContentFilter viewFilter;


        /// <summary>
        /// Creates instance of <see cref="EmailBuilderContentFilter"/> class.
        /// </summary>
        /// <param name="issueIdentifier">Issue identifier.</param>
        /// <exception cref="InvalidOperationException">Thrown when issue is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when newsletter is null.</exception>
        public EmailBuilderContentFilter(int issueIdentifier)
        {
            var issue = IssueInfoProvider.GetIssueInfo(issueIdentifier);
            if (issue == null)
            {
                throw new InvalidOperationException("Issue not found.");
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
            if (newsletter == null)
            {
                throw new InvalidOperationException("Newsletter issue not found.");
            }

            viewFilter = new EmailViewContentFilter(issue, newsletter, null, true);
        }


        /// <summary>
        /// Creates instance of <see cref="EmailBuilderContentFilter"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is null.</exception>
        public EmailBuilderContentFilter(IssueInfo issue, NewsletterInfo newsletter)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            viewFilter = new EmailViewContentFilter(issue, newsletter, null, true);
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            text = viewFilter.Apply(text);
            text = new LinksPacifierContentFilter(false).Apply(text);

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
            code = viewFilter.Apply(code, widget);
            code = new LinksPacifierContentFilter(true).Apply(code);

            return code;
        }
    }
}
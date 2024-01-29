using System;
using System.Linq;

using CMS.EmailEngine;
using CMS.Newsletters.Filters;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides email properties with resolved macros.
    /// </summary>
    public class EmailViewer
    {
        private readonly IssueInfo issue;
        private readonly NewsletterInfo newsletter;
        private readonly EmailViewContentFilter filter;
        private readonly Lazy<EmailMessage> message;


        /// <summary>
        /// Create an instance of <see cref="EmailViewer"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue Newsletter.</param>
        /// <param name="subscriber">Subscriber member - marketable recipient.</param>
        /// <param name="preview">Indicates if the filter is used in context of preview.</param>
        public EmailViewer(IssueInfo issue, NewsletterInfo newsletter, SubscriberInfo subscriber, bool preview)
        {
            this.issue = issue;
            this.newsletter = newsletter;

            filter = new EmailViewContentFilter(issue, newsletter, subscriber, preview);
            message = InitializeMessage();
        }


        /// <summary>
        /// Gets resolved subject of an email.
        /// </summary>
        public string GetSubject()
        {
            return message.Value.Subject;
        }


        /// <summary>
        /// Gets resolved body of an email.
        /// </summary>
        public string GetBody()
        {
            return message.Value.Body;
        }


        /// <summary>
        /// Gets resolved preheader of an email.
        /// </summary>
        public string GetPreheader()
        {
            return filter.Apply(issue.IssuePreheader);
        }


        /// <summary>
        /// Gets email sender address.
        /// </summary>
        public string GetFrom()
        {
            return message.Value.From;
        }


        private Lazy<EmailMessage> InitializeMessage()
        {
            return new Lazy<EmailMessage>(() =>
            {
                var parts = new EmailParts(issue, newsletter);
                parts.ApplyFilters(filter, filter, filter);

                return new EmailMessageBuilder(parts).Build();
            });
        }
    }
}

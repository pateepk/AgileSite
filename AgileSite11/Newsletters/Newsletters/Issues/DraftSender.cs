using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Newsletters.Filters;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for sending drafts of newsletter issues (<see cref="IssueInfo"/>).
    /// </summary>
    internal class DraftSender : IDraftSender
    {
        private readonly IEmailMessageBuilder customEmailMessageBuilder;


        /// <summary>
        /// Creates a new instance of the <see cref="DraftSender"/> class.
        /// </summary>
        public DraftSender()
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="DraftSender"/> class.
        /// </summary>
        /// <param name="customEmailMessageBuilder">Provides method for building new instance of <see cref="EmailMessage"/> from given <see cref="IssueInfo"/></param>
        public DraftSender(IEmailMessageBuilder customEmailMessageBuilder)
        {
            this.customEmailMessageBuilder = customEmailMessageBuilder;
        }


        /// <summary>
        /// Sends the <paramref name="issue"/> as draft to given e-mail addresses (<paramref name="recipients"/>).
        /// </summary>
        /// <param name="issue">Issue to be sent as draft.</param>
        /// <param name="recipients">Recipients delimited by semicolon.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="recipients"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when newsletter the <paramref name="issue"/> is assigned to was not found.</exception>
        public void Send(IssueInfo issue, string recipients)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (string.IsNullOrEmpty(recipients))
            {
                throw new ArgumentException("Recipients cannot be empty", nameof(recipients));
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
            if (newsletter == null)
            {
                throw new InvalidOperationException("Newsletter not found.");
            }

            var validRecipients = GetValidRecipientsCollection(recipients);

            SendToAllRecipients(issue, newsletter, validRecipients);
        }


        private IEmailMessageBuilder GetBuilder(IssueInfo issue, NewsletterInfo newsletter, string recipient)
        {
            if (customEmailMessageBuilder != null)
            {
                return customEmailMessageBuilder;
            }

            var filter = new EmailViewContentFilter(issue, newsletter, null, true);
            var parts = new EmailParts(issue, newsletter);
            parts.ApplyFilters(filter, filter, filter);

            return new EmailMessageBuilder(parts, new DraftMessageModifier(issue, recipient));
        }


        /// <summary>
        /// Asynchronously sends the <paramref name="issue"/> as draft to given e-mail addresses (<paramref name="recipients"/>).
        /// </summary>
        /// <param name="issue">Issue to be sent as draft.</param>
        /// <param name="recipients">Recipients delimited by semicolon.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="recipients"/> is null or empty.</exception>
        public void SendAsync(IssueInfo issue, string recipients)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (string.IsNullOrEmpty(recipients))
            {
                throw new ArgumentNullException(nameof(recipients));
            }

            var thread = new CMSThread(() => Send(issue, recipients));
            thread.RunAsync();
        }


        private IEnumerable<string> GetValidRecipientsCollection(string recipients)
        {
            return recipients.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(recipient => ValidationHelper.IsEmail(recipient));
        }


        private void SendToAllRecipients(IssueInfo issue, NewsletterInfo newsletter, IEnumerable<string> recipients)
        {
            string siteName = SiteInfoProvider.GetSiteName(newsletter.NewsletterSiteID);

            foreach (string recipient in recipients)
            {
                var message = GetBuilder(issue, newsletter, recipient).Build();

                SendToSingleRecipient(message, siteName);
            }
        }


        private static void SendToSingleRecipient(EmailMessage message, string siteName)
        {
            try
            {
                EmailSender.SendEmail(siteName, message, true);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Newsletter", "ThreadIssueEmailSender", ex);
            }
        }
    }
}

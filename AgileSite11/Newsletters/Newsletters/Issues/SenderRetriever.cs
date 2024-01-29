using System;

using CMS.SiteProvider;

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Retrieves sender email and name.
    /// </summary>
    public class SenderRetriever : ISenderRetriever
    {
        private readonly IssueInfo issue;
        private readonly NewsletterInfo newsletter;
        private readonly string siteName;


        /// <summary>
        /// Creates an instance of the <see cref="SenderRetriever"/> class.
        /// </summary>
        /// <param name="issue">Issue to be used for retrieving sender.</param>
        /// <param name="newsletter">Newsletter to be used for retrieving sender.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is null.</exception>
        public SenderRetriever(IssueInfo issue, NewsletterInfo newsletter)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            this.issue = issue;

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }
            this.newsletter = newsletter;

            siteName = SiteInfoProvider.GetSiteName(issue.IssueSiteID);
        }


        /// <summary>
        /// Gets sender for message From value.
        /// </summary>
        public string GetFrom()
        {
            if (NewsletterHelper.MonitorBouncedEmails(siteName))
            {
                string senderEmail = NewsletterHelper.BouncedEmailAddress(siteName);
                if (!string.IsNullOrEmpty(senderEmail))
                {
                    return SenderFormatter.GetSender(GetSenderName(), senderEmail);
                }
            }

            return GetSender();
        }


        /// <summary>
        /// Gets sender for message Reply-To value.
        /// </summary>
        public string GetReplyTo()
        {
            if (NewsletterHelper.MonitorBouncedEmails(siteName))
            {
                string senderEmail = NewsletterHelper.BouncedEmailAddress(siteName);
                if (!string.IsNullOrEmpty(senderEmail))
                {
                    return GetSender();
                }
            }

            return null;
        }


        private string GetSender()
        {
            string senderName = GetSenderName();
            string senderEmail = GetSenderEmail();

            return SenderFormatter.GetSender(senderName, senderEmail);
        }


        /// <summary>
        /// Gets sender name for an email.
        /// </summary>
        public string GetSenderName()
        {
            return string.IsNullOrEmpty(issue.IssueSenderName) ? newsletter.NewsletterSenderName : issue.IssueSenderName;
        }


        /// <summary>
        /// Gets sender email address for an email.
        /// </summary>
        public string GetSenderEmail()
        {
            return string.IsNullOrEmpty(issue.IssueSenderEmail) ? newsletter.NewsletterSenderEmail : issue.IssueSenderEmail;
        }

    }
}

using System;

using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for sending newsletter issues (<see cref="IssueInfo"/>).
    /// </summary>
    internal class IssueSender : IIssueSender
    {
        /// <summary>
        /// Sends e-mail based on the issue to the specified subscriber.
        /// </summary>
        /// <param name="issue">Issue to be sent</param>
        /// <param name="subscriber">Subscriber</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null -or- <paramref name="subscriber"/> is null</exception>
        public void Send(IssueInfo issue, SubscriberInfo subscriber)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            // Generate e-mail(s) to newsletter queue for the subscriber
            EmailQueueManager.GenerateEmails(issue, subscriber);

            // Send the e-mail(s)
            EmailQueueManager.SendAllEmails(false, issue.IssueID);
        }
    }
}
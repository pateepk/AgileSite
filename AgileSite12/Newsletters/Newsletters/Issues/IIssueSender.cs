using System;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IIssueSender), typeof(IssueSender), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for sending newsletter issues (<see cref="IssueInfo"/>).
    /// </summary>
    public interface IIssueSender
    {
        /// <summary>
        /// Sends e-mail based on the issue to the specified subscriber.
        /// </summary>
        /// <param name="issue">Issue to be sent.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null -or- <paramref name="subscriber"/> is null</exception>
        void Send(IssueInfo issue, SubscriberInfo subscriber);
    }
}
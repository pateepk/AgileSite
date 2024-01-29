using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Event arguments for <see cref="NewsletterEvents.GenerateQueueItems"/> event.
    /// </summary>
    public class GenerateQueueItemsEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Backing field for <see cref="GeneratedEmails"/> property.
        /// </summary>
        private readonly HashSet<string> mGeneratedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Subscriber to which the issue is being sent. Can be null to indicate that issue is being sent to all subscribers.
        /// </summary>
        public SubscriberInfo Subscriber
        {
            get;
            set;
        }


        /// <summary>
        /// Issue that's being sent.
        /// </summary>
        public IssueInfo Issue
        {
            get;
            set;
        }


        /// <summary>
        /// Every email should be checked using this email address blocker before it is added to the email queue.
        /// </summary>
        public IEmailAddressBlocker EmailAddressBlocker
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail addresses that have been already added to the queue. Used to remove duplicates throughout different subscriber types.
        /// </summary>
        public HashSet<string> GeneratedEmails
        {
            get
            {
                return mGeneratedEmails;
            }
        }
    }
}
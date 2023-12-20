using System;

using CMS.Activities;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides possibility to log newsletters activities.
    /// </summary>
    public interface INewslettersActivityLogger
    {
        /// <summary>
        /// Logs unsubscription from single newsletter activity.
        /// </summary>
        /// <param name="newsletter">Newsletter contact has unsubscribed from</param>
        /// <param name="issueID">ID of issue used for unsubscribe</param>
        /// <param name="contactId">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is <c>null</c>.</exception>
        void LogUnsubscribeFromSingleNewsletterActivity(NewsletterInfo newsletter, int? issueID = null, int? contactId = null);


        /// <summary>
        /// Logs subscribtion to newsletter activity.
        /// </summary>
        /// <param name="newsletter">Newsletter subscriber subscribed to</param>
        /// <param name="subscriberID">Specifies ID of the subscriber the activity is logged for</param>
        /// <param name="contactId">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is <c>null</c>.</exception>
        void LogNewsletterSubscribingActivity(NewsletterInfo newsletter, int? subscriberID = null, int? contactId = null);


        /// <summary>
        /// Logs activity for opened newsletter email.
        /// </summary>
        /// <param name="issue">Opened issue</param>
        /// <param name="newsletter">Newsletter to which given issue belongs</param>
        /// <param name="contactId">ID of contact who performed activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is <c>null</c>.</exception>
        void LogNewsletterOpenedEmailActivity(IssueInfo issue, NewsletterInfo newsletter, int contactId);


        /// <summary>
        /// Logs unsubscription from all newsletters activity.
        /// </summary>
        /// <param name="issue">Issue used to unsubscribe from all</param>
        /// <param name="contactId">Unsubscribed contact ID</param>
        void LogUnsubscribeFromAllNewslettersActivity(IssueInfo issue, int contactId);


        /// <summary>
        /// Logs newsletter click-through activity.
        /// </summary>
        /// <param name="originalUrl">URL of the click</param>
        /// <param name="issue">Issue where click happened</param>
        /// <param name="newsletter">Newsletter to which given issue belongs</param>
        /// <param name="contactId">ID of contact that performed activity</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="issue"/> or <paramref name="newsletter"/> is null</exception>
        void LogNewsletterClickThroughActivity(string originalUrl, IssueInfo issue, NewsletterInfo newsletter, int contactId);
    }
}
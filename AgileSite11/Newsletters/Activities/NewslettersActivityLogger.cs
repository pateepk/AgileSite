using System;
using System.Web;

using CMS.Activities;
using CMS.Core;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods for logging newsletters activities.
    /// </summary>
    public class NewslettersActivityLogger
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Logs unsubscription from single newsletter activity.
        /// </summary>
        /// <param name="newsletter">Newsletter contact has unsubscribed from</param>
        /// <param name="issueID">ID of issue used for unsubscribe</param>
        /// <param name="contactId">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is <c>null</c>.</exception>
        public void LogUnsubscribeFromSingleNewsletterActivity(NewsletterInfo newsletter, int? issueID = null, int? contactId = null)
        {
			if (newsletter == null)
			{
				throw new ArgumentNullException("newsletter");
			}

			var activityInitializer = new NewsletterUnsubscribingActivityInitializer(newsletter, issueID);
            AddContactToActivityAndLogIfEnabled(activityInitializer, newsletter.NewsletterLogActivity, contactId);
        }


        /// <summary>
        /// Logs subscribtion to newsletter activity.
        /// </summary>
        /// <param name="newsletter">Newsletter subscriber subscribed to</param>
        /// <param name="subscriberID">Specifies ID of the subscriber the activity is logged for</param>
        /// <param name="contactId">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is <c>null</c>.</exception>
        public void LogNewsletterSubscribingActivity(NewsletterInfo newsletter, int? subscriberID = null, int? contactId = null)
        {
            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }
             
            var activityInitializer = new NewsletterSubscribingActivityInitializer(newsletter, subscriberID);
            AddContactToActivityAndLogIfEnabled(activityInitializer, newsletter.NewsletterLogActivity, contactId);
        }


        /// <summary>
        /// Logs activity for opened newsletter email.
        /// </summary>
        /// <param name="issue">Opened issue</param>
        /// <param name="newsletter">Newsletter to which given issue belongs</param>
        /// <param name="contactId">ID of contact who performed activity</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is <c>null</c>.</exception>
        public void LogNewsletterOpenedEmailActivity(IssueInfo issue, NewsletterInfo newsletter, int contactId)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            var activityInitializer = new NewsletterOpenActivityInitializer(issue);
            AddContactToActivityAndLogIfEnabled(activityInitializer, newsletter.NewsletterLogActivity, contactId, false);
        }


        /// <summary>
        /// Logs unsubscription from all newsletters activity.
        /// </summary>
        /// <param name="issue">Issue used to unsubscribe from all</param>
        /// <param name="contactId">Unsubscribed contact ID</param>
        public void LogUnsubscribeFromAllNewslettersActivity(IssueInfo issue, int contactId)
        {
            var activityInitializer = new NewsletterUnsubscribingFromAllActivityInitializer(issue);
            // logging of this activity is always true, not depending on newsletter
            AddContactToActivityAndLogIfEnabled(activityInitializer, true, contactId);
        }


        /// <summary>
        /// Logs newsletter click-through activity.
        /// </summary>
        /// <param name="originalUrl">URL of the click</param>
        /// <param name="issue">Issue where click happened</param>
        /// <param name="newsletter">Newsletter to which given issue belongs</param>
        /// <param name="contactId">ID of contact that performed activity</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="issue"/> or <paramref name="newsletter"/> is null</exception>
        public void LogNewsletterClickThroughActivity(string originalUrl, IssueInfo issue, NewsletterInfo newsletter, int contactId)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            var activityInitializer = new NewsletterClickThroughActivityInitializer(originalUrl, issue, newsletter);
            AddContactToActivityAndLogIfEnabled(activityInitializer, newsletter.NewsletterLogActivity, contactId, false);
        }


        /// <summary>
        /// Returns current request.
        /// </summary>
        /// <returns>Current request.</returns>
        protected virtual HttpRequestBase GetCurrentRequest()
        {
            return CMSHttpContext.Current.Request;
        }


        /// <summary>
        /// Adds <paramref name="contactId"/> to the <paramref name="activityInitializer"/> if the value is not <c>null</c>. Then calls <see cref="IActivityLogService"/> and logs the activity using the given <paramref name="activityInitializer"/>.
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity</param>
        /// <param name="logActivity">Flag from <see cref="NewsletterInfo"/> specifying whether the logging is enabled</param>
        /// <param name="contactId">If set overrides <see cref="ActivityInfo.ActivityContactID" /> of <paramref name="activityInitializer"/></param>
        /// <param name="loggingDisabledInAdministration"><c>True</c> if activities should not be logged in administration</param>
        private void AddContactToActivityAndLogIfEnabled(IActivityInitializer activityInitializer, bool logActivity, int? contactId, bool loggingDisabledInAdministration = true)
        {
            if (logActivity)
            {
                activityInitializer = contactId.HasValue ? activityInitializer.WithContactId(contactId.Value) : activityInitializer;
                mActivityLogService.Log(activityInitializer, GetCurrentRequest(), loggingDisabledInAdministration);
            }
        }
    }
}

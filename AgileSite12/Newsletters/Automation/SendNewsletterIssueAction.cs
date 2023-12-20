using System;

using CMS.Automation;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for Send newsletter issue action
    /// </summary>
    public class SendNewsletterIssueAction : AutomationAction
    {
        #region "Constants"

        /// <summary>
        /// Key for element containing GUID of last sent newsletter.
        /// </summary>
        public const string LAST_SENT_NEWSLETTER_ISSUE_GUID_KEY = "lastsentnewsletterissue";


        /// <summary>
        /// Key for element containing Site ID of last sent newsletter.
        /// </summary>
        public const string LAST_SENT_NEWSLETTER_ISSUE_SITEID_KEY = "lastsentnewsletterissuesiteid";

        #endregion


        #region "Parameters"

        /// <summary>
        /// Newsletter issue identifier.
        /// </summary>
        public virtual Guid NewsletterIssue
        {
            get
            {
                return GetResolvedParameter("NewsletterIssue", Guid.Empty);
            }
        }

        /// <summary>
        /// Site name.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return GetResolvedParameter("Site", String.Empty);
            }
        }

        #endregion


        /// <summary>
        /// Executes current action
        /// </summary>
        public override void Execute()
        {
            if (InfoObject == null)
            {
                return;
            }

            int siteId = SiteInfoProvider.GetSiteID(SiteName);
            if (siteId <= 0)
            {
                LogMessage(EventType.WARNING, "SENDNEWSLETTERISSUE", ResHelper.GetAPIString("ma.action.missingSite", "The selected site was not found."), InfoObject);
                return;
            }

            var contact = (ContactInfo)InfoObject;
            if (String.IsNullOrEmpty(contact.ContactEmail))
            {
                LogMessage(EventType.WARNING, "SENDNEWSLETTERISSUE", ResHelper.GetAPIString("ma.action.sendnewsletterissue.noemail", "Contact e-mail address is missing."), InfoObject);
                return;
            }

            IssueInfo issue = IssueInfoProvider.GetIssueInfo(NewsletterIssue, siteId);
            if (issue == null)
            {
                LogMessage(EventType.ERROR, "SENDNEWSLETTERISSUE", ResHelper.GetAPIString("ma.action.sendnewsletterissue.noissue", "Issue was not found."), InfoObject);
                return;
            }

            // Handle unsubscription
            var subscriptionService = Service.Resolve<ISubscriptionService>();
            if (subscriptionService.IsUnsubscribed(contact.ContactEmail, issue.IssueNewsletterID))
            {
                return;
            }

            // Try get existing subscriber
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, siteId);
            if (subscriber == null)
            {
                var subscriptionProvider = Service.Resolve<ISubscriptionHelper>();
                subscriber = subscriptionProvider.CreateSubscriber(contact, siteId);
            }

            CreateSubscriptionIfNewsletter(issue, subscriptionService, subscriber);

            SendIssue(issue, subscriber);

            // Log newsletter GUID and SiteID to state
            StateObject.StateCustomData[LAST_SENT_NEWSLETTER_ISSUE_GUID_KEY] = issue.IssueGUID;
            StateObject.StateCustomData[LAST_SENT_NEWSLETTER_ISSUE_SITEID_KEY] = issue.IssueSiteID;
        }


        private static void CreateSubscriptionIfNewsletter(IssueInfo issue, ISubscriptionService subscriptionService, SubscriberInfo subscriber)
        {
            if (IsNewsletter(issue))
            {
                subscriptionService.Subscribe(subscriber.SubscriberID, issue.IssueNewsletterID, new SubscribeSettings()
                {
                    SendConfirmationEmail = false,
                    RemoveUnsubscriptionFromNewsletter = false,
                    RemoveAlsoUnsubscriptionFromAllNewsletters = false,
                    AllowOptIn = false,
                });
            }
        }


        private void SendIssue(IssueInfo issue, SubscriberInfo subscriber)
        {
            var originalSiteName = SiteContext.CurrentSiteName;

            try
            {
                // Propagate issue site to use correct site context for macro resolving
                SiteContext.CurrentSiteName = SiteName;
                Service.Resolve<IIssueSender>().Send(issue, subscriber);
            }
            finally
            {
                SiteContext.CurrentSiteName = originalSiteName;
            }
        }


        private static bool IsNewsletter(IssueInfo issue)
        {
            return NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID).NewsletterType == EmailCommunicationTypeEnum.Newsletter;
        }
    }
}

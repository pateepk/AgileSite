using System;
using System.Linq;

using CMS;
using CMS.Activities;
using CMS.Activities.Internal;
using CMS.ContactManagement;
using CMS.Core.Internal;
using CMS.MacroEngine;
using CMS.Newsletters;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ContactManagement.Internal;

[assembly: RegisterExtension(typeof(ContactNewsletterMacroMethods), typeof(ContactInfo))]
namespace CMS.Newsletters
{
    internal class ContactNewsletterMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Checks whether contact is subscribed to the given newsletter. Check is made based on the contact's email address. If
        /// there is a subscriber with the contact's email subscribed to the newsletter, it means contact is subscribed.
        /// Performs check against the Unsubscription table to determine, whether the contact is unsubscripted from the newsletter or not.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Checks whether contact is subscribed to the given newsletter. Check is made based on the contact's email address. If there is a subscriber with the contact's email subscribed to the newsletter, it means contact is subscribed. If subscriber with contact's email is not subscribed, contact's activities are checked. If contact has subscription activity, he is subscribed.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "newsletterGuid", typeof(Guid), "Newsletter GUID.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object SubscribedToNewsletter(EvaluationContext context, params object[] parameters)
        {
            Guid guid = Guid.Empty;
            if (parameters.Length > 1)
            {
                string newsletterGuid = ValidationHelper.GetString(parameters[1], "");
                if (!Guid.TryParse(newsletterGuid, out guid))
                {
                    MacroValidationHelper.LogInvalidGuidParameter("SubscribedToNewsletter", newsletterGuid);
                    return false;
                }
            }

            switch (parameters.Length)
            {
                case 2:
                    return SubscribedToNewsletter(parameters[0], guid, 0);

                case 3:
                    return SubscribedToNewsletter(parameters[0], guid, ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact opened the specified newsletter issue
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact opened specified newsletter issue.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "issueGuid", typeof(Guid), "Newsletter issue GUID.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object OpenedNewsletterIssue(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return InteractedWithNewsletterIssue(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), false, 0);

                case 3:
                    return InteractedWithNewsletterIssue(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), false, ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact clicked link in the specified newsletter issue.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact clicked link in specified newsletter issue.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "issueGuid", typeof(Guid), "Newsletter issue GUID.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object ClickedLinkInNewsletterIssue(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return InteractedWithNewsletterIssue(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), true, 0);

                case 3:
                    return InteractedWithNewsletterIssue(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty), true, ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact unsubscribed from all emails in the last X days.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact unsubscribed from all emails in the last X days.", 1)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object UnsubscribedFromAllEmails(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return UnsubscribedFromAllEmails(parameters[0], ValidationHelper.GetInteger(parameters[1], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact clicked link in specified newsletter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact clicked link in specified newsletter.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "newsletterName", typeof(string), "Name of the newsletter.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object ClickedLinkInNewsletter(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return ClickedLinkInNewsletter(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0);

                case 3:
                    return ClickedLinkInNewsletter(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact clicked link in specified newsletter.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="newsletterName">Newsletter name</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool ClickedLinkInNewsletter(object contact, string newsletterName, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo != null)
            {
                var newsletterIds = NewsletterInfoProvider.GetNewsletters()
                                                          .Column("NewsletterID")
                                                          .WhereEquals("NewsletterName", newsletterName);

                if (newsletterIds.Count > 0)
                {
                    var where = ActivityInfo.TYPEINFO.CreateWhereCondition().WhereIn("ActivityItemID", newsletterIds).ToString(true);
                    return ActivityInfoProvider.ContactDidActivity(contactInfo.ContactID, PredefinedActivityType.NEWSLETTER_CLICKTHROUGH, null, lastXDays, where);
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the contact opened or clicked link in the specified newsletter issue or its variant.
        /// </summary>
        /// <param name="contact">Contact that should be checked.</param>
        /// <param name="issueGuid">Newsletter issue GUID.</param>
        /// <param name="click">Indicates whether contact should have clicked the newsletter issue or just open it.</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied).</param>
        private static bool InteractedWithNewsletterIssue(object contact, Guid issueGuid, bool click, int lastXDays)
        {
            if (contact is ContactInfo contactInfo)
            {
                var activityType = click ? PredefinedActivityType.NEWSLETTER_CLICKTHROUGH : PredefinedActivityType.NEWSLETTER_OPEN;

                var activities = IssueActivitiesRetriever.GetActivitiesQuery(issueGuid, activityType)
                    .TopN(1)
                    .Column("ActivityID")
                    .WhereEquals("ActivityContactID", contactInfo.ContactID);
                     

                if (lastXDays > 0)
                {
                    activities.NewerThan(TimeSpan.FromDays(lastXDays));
                }

                return (activities.FirstOrDefault() != null);
            }

            return false;
        }


        /// <summary>
        /// Checks whether contact is subscribed to the given newsletter. Check is made based on the contact's email address. If
        /// there is a subscriber with the contact's email subscribed to the newsletter, it means contact is subscribed.
        /// Performs check against the Unsubscription table to determine, whether the contact is unsubscripted from the newsletter or not.
        /// </summary>
        /// <param name="contact">Contact whose email should be checked</param>
        /// <param name="newsletterGuid">Newsletter GUID</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool SubscribedToNewsletter(object contact, Guid newsletterGuid, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null || string.IsNullOrEmpty(contactInfo.ContactEmail))
            {
                return false;
            }

            var newsletters = NewsletterInfoProvider.GetNewsletters().WhereEquals("NewsletterGUID", newsletterGuid).TypedResult;
            var subscriptionService = Service.Resolve<ISubscriptionService>();

            return newsletters.Any(newsletter => ContactSubscribedToNewsletter(contactInfo, newsletter, lastXDays, subscriptionService));
        }


        /// <summary>
        /// Returns true if the contact unsubscribed from all emails in the last X days.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        internal static bool UnsubscribedFromAllEmails(object contact, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(contactInfo.ContactEmail))
            {
                return false;
            }

            var query = Service.Resolve<IUnsubscriptionProvider>().GetUnsubscriptionsFromAllNewsletters()
                                                                .TopN(1)
                                                                .Column("UnsubscriptionID")
                                                                .WhereEquals("UnsubscriptionEmail", contactInfo.ContactEmail);

            if (lastXDays > 0)
            {
                var currentTime = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                query.Where("UnsubscriptionCreated", QueryOperator.LargerOrEquals, currentTime.Subtract(TimeSpan.FromDays(lastXDays)));
            }

            return query.Any();
        }


        private static bool ContactSubscribedToNewsletter(ContactInfo contact, NewsletterInfo newsletter, int lastXDays, ISubscriptionService subscritionService)
        {
            if (contact == null)
            {
                return false;
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberByEmail(contact.ContactEmail, newsletter.NewsletterSiteID);
            if (subscriber == null)
            {
                return false;
            }

            if (subscritionService.IsUnsubscribed(contact.ContactEmail, newsletter.NewsletterID))
            {
                return false;
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletters()
                                                               .WhereEquals("NewsletterID", newsletter.NewsletterID)
                                                               .WhereEquals("SubscriberID", subscriber.SubscriberID)
                                                               .WhereTrue("SubscriptionApproved")
                                                               .TopN(1)
                                                               .FirstOrDefault();

            if (subscription == null)
            {
                return false;
            }

            if (lastXDays > 0)
            {
                var currentTime = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                return subscription.SubscribedWhen > currentTime.AddDays(-lastXDays);
            }

            return true;
        }
    }
}

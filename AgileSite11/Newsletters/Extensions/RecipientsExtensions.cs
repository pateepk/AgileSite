using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;

using Microsoft.SqlServer.Server;

namespace CMS.Newsletters
{
    /// <summary>
    /// Extension methods for <see cref="IssueInfo"/> which helps to retrieve an issue recipients.
    /// </summary>
    public static class RecipientsExtensions
    {
        /// <summary>
        /// Returns recipients provider for given <paramref name="newsletter"/>. More information about the provider can be found here <see cref="IRecipientsProvider"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when newsletter is not type of <see cref="EmailCommunicationTypeEnum.Newsletter"/></exception>
        public static IRecipientsProvider GetRecipientsProvider(this NewsletterInfo newsletter)
        {
            if (newsletter.NewsletterType != EmailCommunicationTypeEnum.Newsletter)
            {
                throw new ArgumentException("No provider found for newsletter type of " + newsletter.NewsletterType + ". In case of email campaign, please use other overload with issue info");
            }

            return new NewsletterRecipientsProvider(newsletter);
        }


        /// <summary>
        /// Returns recipients provider for given <paramref name="issue"/>. More information about the provider can be found here <see cref="IRecipientsProvider"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when no matching provider has been found for given newsletter type of (<see cref="EmailCommunicationTypeEnum"/>)</exception>
        public static IRecipientsProvider GetRecipientsProvider(this IssueInfo issue)
        {
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
            if (newsletter.NewsletterType == EmailCommunicationTypeEnum.Newsletter)
            {
                return new NewsletterRecipientsProvider(newsletter);
            }

            if (newsletter.NewsletterType == EmailCommunicationTypeEnum.EmailCampaign)
            {
                return new CampaignEmailRecipientsProvider(issue);
            }

            throw new ArgumentException("No provider found for newsletter type of " + newsletter.NewsletterType);
        }


        /// <summary>
        /// Restricts contact query (<paramref name="contacts"/>) to return only subscribed contacts, all opted out recipients are removed.
        /// </summary>
        public static ObjectQuery<ContactInfo> WithoutUnsubscribed(this ObjectQuery<ContactInfo> contacts, int newsletterId)
        {
            var unsubscribed = Service.Resolve<IUnsubscriptionProvider>()
                                                               .GetUnsubscriptionsFromSingleNewsletter(newsletterId)
                                                               .Column("UnsubscriptionEmail")
                                                               .Distinct();
                                                               

            if (!SqlInstallationHelper.DatabaseIsSeparated())
            {
                return contacts.WhereNotIn("ContactEmail", unsubscribed);
            }

            var unsubscribedEmails = unsubscribed.GetListResult<string>();
            if (unsubscribedEmails.Count <= 0)
            {
                return contacts;
            }

            var unsubscribedEmailsTable = BuildEmailsTable(unsubscribedEmails);
            contacts.EnsureParameters();
            contacts.Parameters.Add("@Emails", unsubscribedEmailsTable, typeof(IEnumerable<ContactEmailRecord>));
            contacts.Where(new WhereCondition("ContactEmail NOT IN (SELECT [Email] FROM @Emails)"));

            return contacts;
        }


        /// <summary>
        /// Restricts contact query (<paramref name="contacts"/>) to return only not bounced contacts, the limit is given in <paramref name="bounceLimit" />.
        /// </summary>
        public static ObjectQuery<ContactInfo> WithoutBounces(this ObjectQuery<ContactInfo> contacts, int bounceLimit)
        {
            if (bounceLimit <= 0)
            {
                return contacts;
            }

            contacts.Where(GetBounceWithinLimitWhere("ContactBounces", bounceLimit));
            var contactIds = GetBouncedSubscriberContactIDs(bounceLimit);

            if (!SqlInstallationHelper.DatabaseIsSeparated())
            {
                contacts.WhereNotIn("ContactID", contactIds);
                return contacts;
            }

            var bouncedContactIds = contactIds.GetListResult<int>();
            if (bouncedContactIds.Count == 0)
            {
                return contacts;
            }

            var bouncedContactIDsIntTable = SqlHelper.BuildOrderedIntTable(bouncedContactIds);
            contacts.Parameters.Add("@ContactIDs", bouncedContactIDsIntTable, typeof(IOrderedEnumerable<int>));
            contacts.Where(new WhereCondition("ContactID NOT IN (SELECT [Value] FROM @ContactIDs)"));

            return contacts;
        }


        private static IEnumerable<SqlDataRecord> BuildEmailsTable(IList<string> emails)
        {
            var metaData = new SqlMetaData[1];
            metaData[0] = new SqlMetaData("Email", SqlDbType.NVarChar, 254);
            var record = new SqlDataRecord(metaData);

            foreach (var email in emails)
            {
                record.SetString(0, email);

                yield return record;
            }
        }


        private static ObjectQuery<SubscriberInfo> GetBouncedSubscriberContactIDs(int bounceLimit)
        {
            var subscribersWithinBounceLimit = GetBounceWithinLimitWhere("SubscriberBounces", bounceLimit);

            return GetContactSubscribers().WhereNot(subscribersWithinBounceLimit)
                                          .Column("SubscriberRelatedID")
                                          .Distinct();
        }


        private static ObjectQuery<SubscriberInfo> GetContactSubscribers()
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .WhereEquals("SubscriberType", PredefinedObjectType.CONTACT);
        }


        private static WhereCondition GetBounceWithinLimitWhere(string bouncesColumnName, int bounceLimit)
        {
            return new WhereCondition().WhereNull(bouncesColumnName)
                                       .Or()
                                       .WhereLessThan(bouncesColumnName, bounceLimit);
        }
    }
}
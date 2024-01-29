using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Core;
using CMS.SiteProvider;
using CMS.ContactManagement;

namespace CMS.Newsletters
{
    using IntDictionary = SafeDictionary<string, int?>;

    /// <summary>
    /// Class providing Subscriber management.
    /// </summary>
    public class SubscriberInfoProvider : AbstractInfoProvider<SubscriberInfo, SubscriberInfoProvider>
    {

        #region "Variables"

        /// <summary>
        /// License limitation subscriber table
        /// </summary>
        private static readonly IntDictionary mLicSubscribers = new IntDictionary();


        /// <summary>
        /// License limitation subscriber table
        /// </summary>
        internal static IntDictionary LicSubscribers
        {
            get
            {
                return mLicSubscribers;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns the Subscriber object for the specified subscriber.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <returns>Subscriber specified by his ID</returns>
        public static SubscriberInfo GetSubscriberInfo(int subscriberId)
        {
            return ProviderObject.GetInfoById(subscriberId);
        }


        /// <summary>
        /// Returns the Subscriber object for the specified subscriber.
        /// </summary>
        /// <param name="subscriberGuid">Subscriber GUID</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Subscriber specified by his GUID and site</returns>
        public static SubscriberInfo GetSubscriberInfo(Guid subscriberGuid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(subscriberGuid, siteId);
        }


        /// <summary>
        /// Returns the Subscriber object for the specified subscriber.
        /// </summary>
        /// <param name="subscriberType">Subscriber type</param>
        /// <param name="subscriberRelatedId">Subscriber's related ID</param>
        /// <param name="siteId">ID of the site this subscriber belongs to</param>
        /// <returns>Subscriber specified by his type, related ID and site</returns>
        public static SubscriberInfo GetSubscriberInfo(string subscriberType, int subscriberRelatedId, int siteId)
        {
            return ProviderObject.GetSubscriberInfoInternal(subscriberType, subscriberRelatedId, siteId);
        }


        /// <summary>
        /// Returns the Subscriber object for the specified subscriber.
        /// </summary>
        /// <param name="email">Subscriber e-mail</param>
        /// <param name="siteId">Site ID</param>        
        /// <returns>Subscriber specified by his email and site ID</returns>
        public static SubscriberInfo GetSubscriberByEmail(string email, int siteId)
        {
            if ((email == null) || (email.Trim() == string.Empty) || (!ValidationHelper.IsEmail(email)) || (siteId <= 0))
            {
                return null;
            }

            return GetSubscribers()
                        .WhereEquals("SubscriberSiteID", siteId)
                        .WhereEquals("SubscriberEmail", email)
                        .FirstObject;
        }


        /// <summary>
        /// Sets (updates or inserts) specified subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber to set</param>
        public static void SetSubscriberInfo(SubscriberInfo subscriber)
        {
            ProviderObject.SetSubscriberInfoInternal(subscriber);
        }


        /// <summary>
        /// Deletes specified subscriber.
        /// </summary>
        /// <param name="subscriberObj">Subscriber object</param>
        public static void DeleteSubscriberInfo(SubscriberInfo subscriberObj)
        {
            ProviderObject.DeleteSubscriberInfoInternal(subscriberObj);
        }


        /// <summary>
        /// Deletes subscription from the newsletter.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        /// <param name="sendConfirmationEmail">Indicates if unsubscription email should be send</param>
        public static void DeleteSubscription(int subscriberId, int newsletterId, bool sendConfirmationEmail)
        {
            if ((subscriberId <= 0) || (newsletterId <= 0))
            {
                return;
            }

            SubscriberNewsletterInfoProvider.DeleteSubscriberNewsletterInfo(subscriberId, newsletterId);
            if (sendConfirmationEmail)
            {
                Service.Resolve<IConfirmationSender>().SendConfirmationEmail(false, subscriberId, newsletterId);
            }
        }


        /// <summary>
        /// Gets the list of subscriber's members.
        /// Subscriber member has SubscriberID=0.
        /// </summary>
        /// <param name="subscriber">Subscriber definition</param>
        /// <param name="topN">Number of members that should be returned (all are returned if 0)</param>
        public static IEnumerable<SubscriberInfo> GetSubscribers(SubscriberInfo subscriber, int topN = 0)
        {
            if (subscriber == null)
            {
                return Enumerable.Empty<SubscriberInfo>();
            }

            switch (subscriber.SubscriberType)
            {
                case PredefinedObjectType.CONTACT:
                    {
                        // Create subscriber from related contact
                        var subscriberMember = CreateSubscriberFromContact(subscriber.SubscriberRelatedID, subscriber, "ContactFirstName", "ContactLastName", "ContactEmail");
                        return new[] { subscriberMember };
                    }

                case PredefinedObjectType.CONTACTGROUP:
                    {
                        string siteName = SiteInfoProvider.GetSiteName(subscriber.SubscriberSiteID);
                        bool monitoringEnabled = NewsletterHelper.MonitorBouncedEmails(siteName);
                        int bounceLimit = NewsletterHelper.BouncedEmailsLimit(siteName);

                        var contacts = GetContactGroupMembers(subscriber.SubscriberRelatedID, monitoringEnabled, bounceLimit, topN, "ContactID, ContactFirstName, ContactLastName, ContactEmail");
                        if (DataHelper.DataSourceIsEmpty(contacts))
                        {
                            return Enumerable.Empty<SubscriberInfo>();
                        }

                        return contacts.Tables[0]
                                       .AsEnumerable()
                                       .Where(FilterEmptyEmails)
                                       .Select(row =>
                                       {
                                           var contactId = DataHelper.GetIntValue(row, "ContactID");
                                           return GetSubscriberClone(subscriber, row, contactId);
                                       });
                    }
            }

            return Enumerable.Empty<SubscriberInfo>();
        }


        /// <summary>
        /// Returns a query for all the SubscriberInfo objects.
        /// </summary>
        public static ObjectQuery<SubscriberInfo> GetSubscribers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Renames subscriber specified by subscriberRealtedId on all sites.
        /// Optional parameters are saved to subscriber only when not set to null.
        /// </summary>
        /// <param name="subscriberType">Type of subscriber</param>
        /// <param name="subscriberRelatedId">ID of related subscriber</param>
        /// <param name="fullName">Subscriber's full name</param>
        /// <param name="firstName">(Optional) Subscriber's first name</param>
        /// <param name="lastName">(Optional) Subscriber's last name</param>
        /// <param name="email">(Optional) Subscriber's e-mail address when needed</param>
        public static void SynchronizeSubscriberInfomation(string subscriberType, int subscriberRelatedId, string fullName, string firstName = null, string lastName = null, string email = null)
        {
            var subscribers = GetSubscribers().WhereEquals("SubscriberType", subscriberType)
                                              .WhereEquals("SubscriberRelatedID", subscriberRelatedId);

            foreach (var subscriber in subscribers)
            {
                subscriber.SubscriberFullName = fullName;
                if (firstName != null)
                {
                    subscriber.SubscriberFirstName = firstName;
                }
                if (lastName != null)
                {
                    subscriber.SubscriberLastName = lastName;
                }
                if (email != null)
                {
                    subscriber.SubscriberEmail = email;
                }

                SetSubscriberInfo(subscriber);
            }
        }


        /// <summary>
        /// Gets first subscriber member with matching email address of given newsletter.
        /// </summary>
        /// <param name="newsletterId">ID of newsletter to get subscribers from</param>
        /// <param name="email">Email to find</param>
        public static SubscriberInfo GetFirstSubscriberWithSpecifiedEmail(int newsletterId, string email)
        {
            var newsletterSubscribersIds = SubscriberNewsletterInfoProvider.GetNewsletterSubscribersIds(newsletterId);

            return GetSubscriberForNewsletterByEmail(newsletterSubscribersIds, email) ?? GetSubscriberForNewsletterFromContactGroupByEmail(newsletterSubscribersIds, email);
        }
        

        #endregion


        #region "Private methods"
        

        private static SubscriberInfo GetSubscriberForNewsletterByEmail(ICollection<int> newsletterSubscribersIds, string email)
        {
            return ProviderObject.GetObjectQuery()
                                 .TopN(1)
                                 .WhereIn("SubscriberID", newsletterSubscribersIds)
                                 .WhereEquals("SubscriberEmail", email)
                                 .FirstObject;
        }


        private static SubscriberInfo GetSubscriberForNewsletterFromContactGroupByEmail(ICollection<int> newsletterSubscribersIds, string email)
        {
            var contact = ContactInfoProvider.GetContacts()
                                                .Columns("ContactID")
                                                .WhereEquals("ContactEmail", email)
                                                .TopN(1)
                                                .FirstObject;

            if (contact == null)
            {
                return null;
            }

            var contactGroupIds = ContactGroupMemberInfoProvider.GetRelationships()
                                                                .Columns("ContactGroupMemberContactGroupID")
                                                                .WhereEquals("ContactGroupMemberRelatedID", contact.ContactID);

            var subscriber = ProviderObject.GetObjectQuery()
                                       .WhereIn("SubscriberID", newsletterSubscribersIds)
                                       .WhereIn("SubscriberRelatedID", contactGroupIds)
                                       .WhereEquals("SubscriberType", PredefinedObjectType.CONTACTGROUP)
                                       .TopN(1)
                                       .FirstObject;

            return subscriber != null ? CreateSubscriberFromContact(contact.ContactID, subscriber, "ContactFirstName", "ContactLastName", "ContactEmail") : null;
        }


        /// <summary>
        /// Gets contacts in a given contact group.
        /// </summary>
        /// <param name="contactGroupId">Contact group ID</param>
        /// <param name="monitoringEnabled">Indicates if bounced e-mail monitoring is enabled</param>
        /// <param name="bounceLimit">Bounce e-mail limit</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Allows to specify columns to be returned</param>
        /// <remarks>If bounced e-mail monitoring is enabled, only contacts that have less bounces then bounce limit are returned.</remarks>
        internal static DataSet GetContactGroupMembers(int contactGroupId, bool monitoringEnabled, int bounceLimit, int topN, string columns)
        {
            var where = new WhereCondition()
                .WhereEquals("ContactGroupMemberContactGroupID", contactGroupId)
                .WhereNotEmpty("ContactEmail")
                .NotBounced("ContactBounces", monitoringEnabled, bounceLimit);

            return ConnectionHelper.ExecuteQuery("om.contactgroupmember.selectfromcontactview", where.Parameters, where.ToString(), "ContactID", topN, columns);
        }


        /// <summary>
        /// Creates subscriber object using contact info and source subscriber data.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="original">Source subscriber</param>
        /// <param name="columns">List of columns to retrieve from contact</param>
        internal static SubscriberInfo CreateSubscriberFromContact(int contactId, SubscriberInfo original, params string[] columns)
        {
            var data = ContactInfoProvider.GetContacts()
                                          .TopN(1)
                                          .Columns(columns)
                                          .WhereEquals("ContactID", contactId)
                                          .WhereNotEmpty("ContactEmail")
                                          .TypedResult;

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            var row = data.Tables[0].Rows[0];

            return GetSubscriberClone(original, row, contactId);
        }


        private static SubscriberInfo GetSubscriberClone(SubscriberInfo original, DataRow data, int contactId)
        {
            return new SubscriberInfo(original, false)
            {
                SubscriberID = string.Equals(original.SubscriberType, PredefinedObjectType.CONTACT, StringComparison.InvariantCultureIgnoreCase) ? original.SubscriberID : 0,
                SubscriberEmail = DataHelper.GetStringValue(data, "ContactEmail", original.SubscriberEmail),
                SubscriberFirstName = DataHelper.GetStringValue(data, "ContactFirstName"),
                SubscriberLastName = DataHelper.GetStringValue(data, "ContactLastName"),
                SubscriberType = PredefinedObjectType.CONTACT,
                SubscriberRelatedID = contactId
            };
        }


        private static bool FilterEmptyEmails(DataRow row)
        {
            var email = DataHelper.GetStringValue(row, "ContactEmail");
            return !string.IsNullOrEmpty(email);
        }


        /// <summary>
        /// Clear hashtable.
        /// </summary>
        private static void ClearLicSubscribers()
        {
            LicSubscribers.Clear();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the Subscriber object for the specified subscriber.
        /// </summary>
        /// <param name="subscriberType">Subscriber type</param>
        /// <param name="subscriberRelatedId">Subscriber's related ID</param>
        /// <param name="siteId">ID of the site this subscriber belongs to</param>
        /// <returns>Subscriber specified by his type, related ID and site</returns>
        protected virtual SubscriberInfo GetSubscriberInfoInternal(string subscriberType, int subscriberRelatedId, int siteId)
        {
            if ((string.IsNullOrEmpty(subscriberType)) || (subscriberRelatedId <= 0) || (siteId <= 0))
            {
                return null;
            }

            return GetObjectQuery().WhereEquals("SubscriberType", subscriberType)
                                   .WhereEquals("SubscriberRelatedID", subscriberRelatedId)
                                   .WhereEquals("SubscriberSiteID", siteId)
                                   .FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber to set</param>
        protected virtual void SetSubscriberInfoInternal(SubscriberInfo subscriber)
        {
            if (subscriber != null)
            {
                // Reset email bounces count if e-mail address changed so that contact can receive newsletters even if he had invalid e-mail address before
                if (subscriber.ItemChanged("SubscriberEmail"))
                {
                    subscriber.SubscriberBounces = 0;
                }

                SetInfo(subscriber);

                ClearLicSubscribers();
            }
            else
            {
                throw new Exception("[SubscriberInfoProvider.SetSubscriber]: No Subscriber object set.");
            }
        }


        /// <summary>
        /// Deletes specified subscriber.
        /// </summary>
        /// <param name="subscriberObj">Subscriber object</param>
        protected virtual void DeleteSubscriberInfoInternal(SubscriberInfo subscriberObj)
        {
            // Delete subscriber
            DeleteInfo(subscriberObj);
            ClearLicSubscribers();
        }


        /// <summary>
        /// Updates the data in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value].</param>
        internal static void UpdateDataInternal(WhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values);
        }


        /// <summary>
        /// Bulk deletes info objects based on the given condition.
        /// </summary>
        /// <param name="where">Where condition for the objects which should be deleted.</param>
        internal static void BulkDeleteInternal(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where, new BulkDeleteSettings { RemoveDependencies = true });
        }

        #endregion
    }
}
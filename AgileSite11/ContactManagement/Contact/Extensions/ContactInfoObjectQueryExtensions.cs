using System;
using System.Collections.Generic;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Core.Internal;
using CMS.DataEngine.Query;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Extensions of <see cref="ObjectQuery{ContactInfo}"/>.
    /// </summary>
    public static class ContactInfoObjectQueryExtensions
    {
        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are exactly age of <paramref name="yearsOld"/> years old.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> WithAge(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            // select Contact where Contact.Age >= yearsOld && Contact.Age <= yearsOld; thus select Contact where Contact.Age == yearsOld
            return contactsQuery.YoungerThanOrWithAge(yearsOld)
                                .OlderThanOrWithAge(yearsOld);
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are not exactly <paramref name="yearsOld"/> years old.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> NotWithAge(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }
            
            // select Contact where Contact.Age < yearsOld || Contact.Age > yearsOld; thus select Contact where Contact.Age != yearsOld
            return contactsQuery.YoungerThan(yearsOld)
                                .Or()
                                .OlderThan(yearsOld)
                                .Or()
                                .WhereNull("ContactBirthday");
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are older than <paramref name="yearsOld"/> (contacts with age <paramref name="yearsOld"/> are not included).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> OlderThan(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.OlderThanOrWithAge(yearsOld + 1);
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are older than <paramref name="yearsOld"/> (contacts with age <paramref name="yearsOld"/> are included).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        internal static ObjectQuery<ContactInfo> OlderThanOrWithAge(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            // There is no method WithBirthdayToOrEquals, we need to add one day to the date to simulate it
            return contactsQuery.WithBirthdayBefore(Service.Resolve<IDateTimeNowService>().GetDateTimeNow().AddYears(-yearsOld).AddDays(1));
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are younger than <paramref name="yearsOld"/> (contacts with age <paramref name="yearsOld"/> are not included).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> YoungerThan(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }
            return contactsQuery.WithBirthdayAfter(Service.Resolve<IDateTimeNowService>().GetDateTimeNow().AddYears(-yearsOld));
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that are younger than <paramref name="yearsOld"/> (contacts with age <paramref name="yearsOld"/> are included).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        internal static ObjectQuery<ContactInfo> YoungerThanOrWithAge(this ObjectQuery<ContactInfo> contactsQuery, int yearsOld)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.WithBirthdayAfter(Service.Resolve<IDateTimeNowService>().GetDateTimeNow().AddYears(-(yearsOld + 1)));
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that have date of birth later than <paramref name="fromExclusive"/>. The method takes only Date part.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> WithBirthdayAfter(this ObjectQuery<ContactInfo> contactsQuery, DateTime fromExclusive)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.Where("ContactBirthday", QueryOperator.LargerThan, fromExclusive);
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that have date of birth sooner than <paramref name="toExclusive"/>. The method takes only Date part.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> WithBirthdayBefore(this ObjectQuery<ContactInfo> contactsQuery, DateTime toExclusive)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.Where("ContactBirthday", QueryOperator.LessThan, toExclusive);
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that have been created later than <paramref name="from"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> CreatedAfter(this ObjectQuery<ContactInfo> contactsQuery, DateTime from)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.Where("ContactCreated", QueryOperator.LargerThan, from);
        }


        /// <summary>
        /// Returns <see cref="ObjectQuery{ContactInfo}"/> with contacts that have been created before <paramref name="to"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> CreatedBefore(this ObjectQuery<ContactInfo> contactsQuery, DateTime to)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            return contactsQuery.Where("ContactCreated", QueryOperator.LessThan, to);
        }


        /// <summary>
        /// Filters out contacts whose email address is unreachable (it has bounced more than <paramref name="bouncedEmailsLimit"/> times).
        /// Contacts blocked manually by administrator are filtered out as well.
        /// </summary>
        /// <param name="contactsQuery">Query which will be changed to include where clause filtering out bounced contacts</param>
        /// <param name="monitorBouncedEmails">If true, bounces will be checked against the <paramref name="bouncedEmailsLimit"/> parameter. Otherwise, only contacts blocked in the UI will be filtered out.</param>
        /// <param name="bouncedEmailsLimit">Number of bounces which are allowed</param>
        /// <returns>Modified query</returns>
        public static ObjectQuery<ContactInfo> NotBounced(this ObjectQuery<ContactInfo> contactsQuery, bool monitorBouncedEmails, int bouncedEmailsLimit)
        {
            return contactsQuery.NotBounced("ContactBounces", monitorBouncedEmails, bouncedEmailsLimit);
        }


        /// <summary>
        /// Filters out contacts whose email address does not match given <paramref name="email"/>. 
        /// </summary>
        /// <remarks>
        /// Since email address is not unique per contact on single site, query can contain multiple contacts.
        /// </remarks>
        /// <param name="contactsQuery">Query which will be changed to omit contacts not satisfying the condition</param>
        /// <param name="email">Email address the contacts will be filtered for. Can be null, in such case filters contacts with email address set to null</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        /// <returns>Modified query</returns>
        public static ObjectQuery<ContactInfo> WithEmail(this ObjectQuery<ContactInfo> contactsQuery, string email)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            if (email == null)
            {
                return contactsQuery.WhereNull("ContactEmail");
            }

            return contactsQuery.WhereEquals("ContactEmail", email);
        }


        /// <summary>
        /// Filters out contacts who have searched for all of the specified keywords in last x days.
        /// </summary>
        /// <param name="contactsQuery">Object query for contact info</param>
        /// <param name="keywords">Array of keywords</param>
        /// <param name="lastXDays">In last x days</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> SearchedForAll(this ObjectQuery<ContactInfo> contactsQuery, IEnumerable<string> keywords, int lastXDays = 0)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            var contactIDFromActivitiesQuery = SearchActivities(lastXDays);

            var existsConditionForSearches = new WhereCondition();

            foreach (var keyword in keywords)
            {
                existsConditionForSearches.WhereExists(ActivityInfoProvider.GetActivities()
                                                                           .TopN(1)
                                                                           .WhereContains("ActivityValue", keyword)
                                                                           .WhereEquals("ActivityContactID", "OM_Contact.ContactID".AsExpression()));
            }

            return contactsQuery.WhereIn("ContactID", contactIDFromActivitiesQuery.Where(existsConditionForSearches));
        }


        /// <summary>
        /// Filters out contacts who have searched for any of the specified keywords in last x days.
        /// </summary>
        /// <param name="contactsQuery">Object query for contact info</param>
        /// <param name="keywords">Array of keywords</param>
        /// <param name="lastXDays">In last x days</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactsQuery"/> is null</exception>
        public static ObjectQuery<ContactInfo> SearchedForAny(this ObjectQuery<ContactInfo> contactsQuery, IEnumerable<string> keywords, int lastXDays = 0)
        {
            if (contactsQuery == null)
            {
                throw new ArgumentNullException("contactsQuery");
            }

            var contactIDFromActivitiesQuery = SearchActivities(lastXDays);

            var keywordsCondition = new WhereCondition();
            foreach (var keyword in keywords)
            {
                keywordsCondition.WhereContains("ActivityValue", keyword).Or();
            }

            var existsConditionForSearches = new WhereCondition().WhereExists(ActivityInfoProvider.GetActivities()
                                                                                                  .TopN(1)
                                                                                                  .Where(keywordsCondition)
                                                                                                  .WhereEquals("ActivityContactID", "OM_Contact.ContactID".AsExpression()));

            return contactsQuery.WhereIn("ContactID", contactIDFromActivitiesQuery.Where(existsConditionForSearches));
        }


        /// <summary>
        /// Filters out contacts whose email address is unreachable (it has bounced more than <paramref name="bouncedEmailsLimit"/> times).
        /// Subscribers blocked manually by administrator are filtered out as well. The column which stores number of bounces has to be 
        /// specified in <paramref name="columnName"/> parameter, because each object which stores bounces count has differently named column.
        /// </summary>
        /// <param name="whereCondition">Condition which will be changed to include where clause filtering out bounced subscribers</param>
        /// <param name="columnName">Name of the column which stores number of times the email of the subscriber bounced (e.g. SubscriberBounces, ContactBounces)</param>
        /// <param name="monitorBouncedEmails">If true, bounces will be checked against the <paramref name="bouncedEmailsLimit"/> parameter. Otherwise, only subscribers blocked via UI will be filtered out (as if <paramref name="bouncedEmailsLimit"/> was 0).</param>
        /// <param name="bouncedEmailsLimit">Limit of bounces which are allowed. If 0, only subscribers blocked via UI will be filtered out.</param>
        /// <typeparam name="TQuery">Type of the query</typeparam>
        /// <returns>IWhereCondition with additional filter</returns>
        public static TQuery NotBounced<TQuery>(this TQuery whereCondition, string columnName, bool monitorBouncedEmails, int bouncedEmailsLimit) where TQuery : IWhereCondition<TQuery>
        {
            if (monitorBouncedEmails && (bouncedEmailsLimit > 0))
            {
                whereCondition = whereCondition.Where(w => w.WhereNull(columnName)
                                                            .Or()
                                                            .WhereLessThan(columnName, bouncedEmailsLimit));
            }
            else
            {
                // If bounce limit is 0 or less, subscribers are never blocked automatically, but their bounced e-mail count is still tracked and 
                // they can be blocked manually via UI by administrator. If monitorBouncedEmails is false, previously blocked subscribers should still be filtered out.
                // If administrator blocked subscription in UI, int.MaxValue was inserted to the bounces column
                // and in that case those subscribers should be blocked even if bounce limit is <= 0
                whereCondition = whereCondition.Where(w => w.WhereNull(columnName)
                                                            .Or()
                                                            .WhereNotEquals(columnName, int.MaxValue));
            }

            return whereCondition;
        }


        /// <summary>
        /// Returns activities with only ActivityContactID column from all search activities.
        /// </summary>
        /// <param name="lastXDays">In last x days</param>
        private static ObjectQuery<ActivityInfo> SearchActivities(int lastXDays = 0)
        {
            var contactIDsFromActivitiesQuery = ActivityInfoProvider.GetActivities()
                                                                    .From("OM_Activity as Activities")
                                                                    .Distinct()
                                                                    .Column("ActivityContactID")
                                                                    .WhereIn("ActivityType", new[]
                                                                    {
                                                                        PredefinedActivityType.INTERNAL_SEARCH,
                                                                        PredefinedActivityType.EXTERNAL_SEARCH,
                                                                    });
            if (lastXDays > 0)
            {
                contactIDsFromActivitiesQuery.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            return contactIDsFromActivitiesQuery;
        }
    }
}
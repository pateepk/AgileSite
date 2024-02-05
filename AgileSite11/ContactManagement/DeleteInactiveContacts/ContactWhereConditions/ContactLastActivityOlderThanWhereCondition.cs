using CMS.Activities;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents object with additional where condition for <see cref="ContactInfo"/> query.
    /// </summary>
    internal sealed class ContactLastActivityOlderThanWhereCondition : IContactsWhereCondition
    {
        private readonly int mDays;


        /// <summary>
        /// Selects all contact with last activity older than x <paramref name="days"/> or contact with no activities but older than x <paramref name="days"/>.
        /// </summary>
        public ContactLastActivityOlderThanWhereCondition(int days)
        {
            mDays = days;
        }


        /// <summary>
        /// Selects all contact with last activity older than x days or contact with no activities but older than x days.
        /// </summary>
        public WhereCondition GetWhere()
        {
            var finalWhere = new WhereCondition();
            if (mDays <= 0)
            {
                return finalWhere;
            }

            var dateTime = Service.Resolve<IDateTimeNowService>().GetDateTimeNow().AddDays(-mDays);

            var contactLatestActivityBeforeXDays = ActivityInfoProvider.GetActivities()
                                                                       .TopN(1)
                                                                       .Column("ActivityContactID")
                                                                       .WhereEquals("ActivityContactID", "ContactID".AsColumn())
                                                                       .GroupBy("ActivityContactID")
                                                                       .Having(new WhereCondition().WhereLessOrEquals(
                                                                           new AggregatedColumn(AggregationType.Max, "ActivityCreated"), dateTime));

            var contactIsOldAndWithoutActivity = new WhereCondition().WhereLessThan("ContactCreated", dateTime)
                                                                     .And()
                                                                     .WhereNotExists(ActivityInfoProvider.GetActivities()
                                                                                                         .TopN(1)
                                                                                                         .Column("ActivityContactID")
                                                                                                         .WhereEquals("ActivityContactID", "ContactID".AsColumn()));
            return finalWhere.WhereExists(contactLatestActivityBeforeXDays)
                             .Or(contactIsOldAndWithoutActivity);
        }
    }
}
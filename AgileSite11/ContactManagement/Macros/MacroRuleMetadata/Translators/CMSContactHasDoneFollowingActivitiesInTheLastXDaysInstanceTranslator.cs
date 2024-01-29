using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class 
        CMSContactHasDoneFollowingActivitiesInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasDoneFollowingActivitiesInTheLastXDays
        /// Contact {_perfectum} done {_any} of the following activities in the last {days} day(s): {activities}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string paramActivityNames = ruleParameters["activities"].Value;
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);
            bool allActivities = ruleParameters["_any"].Value.ToBoolean(false);

            string[] activityNames = paramActivityNames.Split(new[]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);

            var performedActivitiesQuery = ActivityInfoProvider.GetActivities()
                                                               .Distinct()
                                                               .WhereIn("ActivityType", activityNames)
                                                               .Columns("ActivityContactID", "ActivityType");

            if (lastXDays > 0)
            {
                performedActivitiesQuery.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            var activitiesQuery = performedActivitiesQuery.AsNested();

            activitiesQuery.Column("ActivityContactID");

            if (allActivities)
            {
                activitiesQuery.GroupBy("ActivityContactID")
                               .Having(w =>
                                   w.WhereEquals(new CountColumn("*"), activityNames.Length)
                                );
            }

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activitiesQuery);
            }
            else
            {
                contacts.WhereIn("ContactID", activitiesQuery);
            }

            return contacts;
        }
    }
}
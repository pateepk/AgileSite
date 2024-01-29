using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasLoggedInInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasLoggedInInTheLastXDays
        /// Contact {_perfectum} logged in in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);

            var activitiesQuery = ActivityInfoProvider.GetActivities()
                                                      .WhereEquals("ActivityType", PredefinedActivityType.USER_LOGIN)
                                                      .Column("ActivityContactID");

            if (lastXDays > 0)
            {
                activitiesQuery.NewerThan(TimeSpan.FromDays(lastXDays));
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
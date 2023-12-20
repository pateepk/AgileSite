using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasDoneAnyActivityInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasDoneAnyActivityInTheLastXDays
        /// Contact {_perfectum} done any activity in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            int days = ruleParameters["days"].Value.ToInteger(0);

            var activities = ActivityInfoProvider.GetActivities().Column("ActivityContactID");

            if (days > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(days));
            }

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activities);
            }
            else
            {
                contacts.WhereIn("ContactID", activities);
            }

            return contacts;
        }
    }
}
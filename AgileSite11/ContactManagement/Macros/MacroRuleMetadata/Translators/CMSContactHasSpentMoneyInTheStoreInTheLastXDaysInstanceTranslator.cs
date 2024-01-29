using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasSpentMoneyInTheStoreInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasSpentMoneyInTheStoreInTheLastXDays Macro rule.
        /// Contact {_perfectum} spent between {money1} and {money2} in the store in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            decimal from = ValidationHelper.GetDecimal(ruleParameters["money1"].Value, 0m);
            decimal to = ValidationHelper.GetDecimal(ruleParameters["money2"].Value, 0m);
            int days = ValidationHelper.GetInteger(ruleParameters["days"].Value, 0);

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASE)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID")
                                                 .Having(w =>
                                                     w.Where(new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCast("ActivityValue", "DECIMAL")), QueryOperator.LargerOrEquals, from)
                                                 )
                                                 .Having(w =>
                                                     w.Where(new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCast("ActivityValue", "DECIMAL")), QueryOperator.LessOrEquals, to)
                                                 );
            

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
using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasMadeAtLeastXOrdersInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasMadeAtLeastXOrders
        /// Contact {_perfectum} made at least {num} order(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            int minimumNumberOfOrders = ruleParameters["num"].Value.ToInteger(0);

            var contacts = ContactInfoProvider.GetContacts();

            // All contacts have at least zero orders
            if (perfectum != "!" && (minimumNumberOfOrders <= 0))
            {
                return contacts;
            }
            if ((perfectum == "!") && (minimumNumberOfOrders <= 0))
            {
                return contacts.NoResults();
            }

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASE)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID")
                                                 .Having(w =>
                                                     w.Where(new CountColumn("*"), QueryOperator.LargerOrEquals, minimumNumberOfOrders)
                                                 );

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
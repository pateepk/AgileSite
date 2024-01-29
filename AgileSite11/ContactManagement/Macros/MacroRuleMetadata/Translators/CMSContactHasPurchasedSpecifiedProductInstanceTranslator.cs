using System;
using System.Linq;
using System.Text;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasPurchasedSpecifiedProductInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasPurchasedSpecifiedProduct Macro rule.
        /// Contact {_perfectum} purchased product {product}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string productGuid = ruleParameters["product"].Value;

            var product = new DataQuery().From("COM_SKU")
                                         .WhereEquals("SKUGUID", productGuid)
                                         .Column("SKUID");

            var productVariants = new DataQuery().From("COM_SKU")
                                                 .WhereIn("SKUParentSKUID", product)
                                                 .Column("SKUID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASEDPRODUCT)
                                                 .Where(w => w.WhereIn("ActivityItemID", product)
                                                              .Or()
                                                              .WhereIn("ActivityItemID", productVariants))
                                                 .Column("ActivityContactID");

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
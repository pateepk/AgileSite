using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasProductInWishlistInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            Guid paramProductGuid = ValidationHelper.GetGuid(ruleParameters["product"].Value, Guid.Empty);
            string paramHas = ruleParameters["_has"].Value; // "", "!"

            var skuIDsQuery = new DataQuery().From("COM_SKU")
                .WhereEquals("SKUGUID", paramProductGuid)
                .Column("SKUID");

            var userIDsQuery = new DataQuery().From("COM_Wishlist")
                .WhereIn("SKUID", skuIDsQuery)
                .Column("UserID");

            var contactIDsQuery = ContactMembershipInfoProvider.GetRelationships()
                .WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser)
                .WhereIn("RelatedID", userIDsQuery)
                .Column("ContactID");

            var contactsQuery = ContactInfoProvider.GetContacts();
            if (paramHas == "!")
            {
                contactsQuery.WhereNotIn("ContactID", contactIDsQuery);
            }
            else
            {
                contactsQuery.WhereIn("ContactID", contactIDsQuery);
            }

            return contactsQuery;
        }
    }
}
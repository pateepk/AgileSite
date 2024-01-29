using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasAtLeastXProductsInWishlistInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            int numberOfProductsInWishlist = ValidationHelper.GetInteger(ruleParameters["num"].Value, 0);
            string ruleOperator = ruleParameters["has"].Value;
            QueryOperator? op = null;

            switch (ruleOperator)
            {
                case ">=":
                    op = QueryOperator.LargerOrEquals;
                    break;
                case "<=":
                    op = QueryOperator.LessOrEquals;
                    break;

            }

            if (!op.HasValue)
            {
                throw new Exception("[CMSContactHasAtLeastXProductsInWishlistInstanceTranslator.Translate]: Unknown operator: " + ruleOperator);
            }

            // All contacts have at least zero products in wishlist 
            if ((op == QueryOperator.LargerOrEquals) && (numberOfProductsInWishlist <= 0)) 
            {
                return ContactInfoProvider.GetContacts();
            }

            // No contact has negative number of products in wishlist
            if ((op == QueryOperator.LessOrEquals) && (numberOfProductsInWishlist < 0))
            {
                return ContactInfoProvider.GetContacts().NoResults();
            }

            // Simulate <= (at most) by negation, therefore use > and intersection to include contacts without any products in wishlist
            if (op == QueryOperator.LessOrEquals)
            {
                op = QueryOperator.LargerThan;
            }

            // In order to make query simpler, rule must be fulfilled by a single user related to contact. If contact is related to more users
            // and rule is fulfilled by more combined users which are related to contact, but no single user fulfills the 
            // rule, Rule is considered as not fulfilled. To achieve absolutely correct behavior, query would be very complex due to the database
            // separation and the fact that JOIN clause is not supported between tables which are located in two different databases

            var wishlistQuery = new DataQuery().From("COM_Wishlist")
                                               .GroupBy("UserID")
                                                // op has only these values: >, >=
                                               .Having(w => 
                                                   w.Where(new CountColumn("SKUID"), op.Value, numberOfProductsInWishlist)
                                                )
                                               .Column("UserID");

            var contactIDsQuery = ContactMembershipInfoProvider.GetRelationships()
                                                               .WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser)
                                                               .WhereIn("RelatedID", wishlistQuery)
                                                               .Column("ContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (op == QueryOperator.LargerThan)
            {
                contacts.WhereNotIn("ContactID", contactIDsQuery);
            }
            else
            {
                contacts.WhereIn("ContactID", contactIDsQuery);
            }

            return contacts;
        }
    }
}
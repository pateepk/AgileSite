using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.ContactManagement
{
    internal class CMSContactIsInRoleInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactIsInRole Macro rule.
        /// Contact {_is} in {_any} of the roles: {roles}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string paramRoleGuids = ruleParameters["roles"].Value;

            // Careful: parameter "any" is false for any role and true for all roles
            bool isInAllRoles = ValidationHelper.GetBoolean(ruleParameters["_any"].Value, false);

            ICollection<Guid> roleGuids;
            if (!MacroValidationHelper.TryParseGuids(paramRoleGuids, out roleGuids))
            {
                MacroValidationHelper.LogInvalidGuidParameter("IsInRoles", paramRoleGuids);
                return new ObjectQuery<ContactInfo>().NoResults();
            }


            // In order to make query simpler, rule must be fulfilled by a single user related to contact. If contact is related to more users
            // and rule is fulfilled by more combined users which are related to contact, but no single user fulfills the 
            // rule, Rule is considered as not fulfilled. To achieve absolutely correct behavior, query would be very complex due to the database
            // separation and the fact that JOIN clause is not supported between tables which are located in two different databases

            var roleIDs = RoleInfoProvider.GetRoles()
                                          .WhereIn("RoleGuid", roleGuids)
                                          .Column("RoleID");

            var roleMembers = UserRoleInfoProvider.GetUserRoles()
                                                  .WhereIn("RoleID", roleIDs)
                                                  .Column("UserID");

            if (isInAllRoles)
            {
                roleMembers.GroupBy("UserID")
                           .Having(w => 
                               w.WhereEquals(new CountColumn("RoleID"), roleGuids.Count)
                           );
            }

            var contactIDs = ContactMembershipInfoProvider.GetRelationships()
                                                          .WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser)
                                                          .WhereIn("RelatedID", roleMembers)
                                                          .Column("ContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.WhereNotIn("ContactID", contactIDs);
            }
            else
            {
                contacts.WhereIn("ContactID", contactIDs);
            }

            return contacts;
        }
    }
}
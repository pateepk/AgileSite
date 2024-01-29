using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactIsInCommunityGroupInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactIsInCommunityGroup
        /// Contact {_is} in {_any} of the community groups: {groups}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            bool allGroups = ruleParameters["_any"].Value.ToBoolean(false);
            string paramIs = ruleParameters["_is"].Value;
            string paramGroupGuids = ruleParameters["groups"].Value;

            ICollection<Guid> groupGuids;
            if (!MacroValidationHelper.TryParseGuids(paramGroupGuids, out groupGuids))
            {
                MacroValidationHelper.LogInvalidGuidParameter("IsInCommunityGroup", paramGroupGuids);
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            // In order to make query simpler, rule must be fulfilled by a single user related to contact. If contact is related to more users
            // and rule is fulfilled by more combined users which are related to contact, but no single user fulfills the 
            // rule, Rule is considered as not fulfilled. To achieve absolutely correct behavior, query would be very complex due to the database
            // separation and the fact that JOIN clause is not supported between tables which are located in two different databases

            var communityGroupIDs = new DataQuery().From("Community_Group")
                                                   .WhereIn("GroupGuid", groupGuids)
                                                   .Column("GroupID");

            var communityGroupMembers = new DataQuery().From("Community_GroupMember")
                                                       .WhereIn("MemberGroupID", communityGroupIDs)
                                                       .Column("MemberUserID");

            if (allGroups)
            {
                communityGroupMembers.GroupBy("MemberUserID")
                                     .Having(w =>
                                         w.WhereEquals(new CountColumn("MemberGroupID"), groupGuids.Count)
                                     );
            }

            var membershipQuery = ContactMembershipInfoProvider.GetRelationships()
                                                               .WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser)
                                                               .WhereIn("RelatedID", communityGroupMembers)
                                                               .Column("ContactID");
            
            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.WhereNotIn("ContactID", membershipQuery);
            }
            else
            {
                contacts.WhereIn("ContactID", membershipQuery);
            }
            return contacts;
        }
    }
}
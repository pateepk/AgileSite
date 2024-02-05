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
    internal class CMSContactIsInContactGroupInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactIsInContactGroup Macro rule.
        /// Contact {_is} in {_any} of the contact groups: {groups}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string groups = ruleParameters["groups"].Value;

            // Parameter "any" is false for any group and true for all groups
            bool isInAllGroups = ValidationHelper.GetBoolean(ruleParameters["_any"].Value, false);

            // Get groups
            var groupNames = groups.Split(new[]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            var contactGroups = ContactGroupInfoProvider.GetContactGroups()
                                                        .WhereIn("ContactGroupName", groupNames)
                                                        .Column("ContactGroupID");

            // Get group members
            var groupMembers = ContactGroupMemberInfoProvider.GetRelationships()
                                                             .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                                                             .WhereIn("ContactGroupMemberContactGroupID", contactGroups)
                                                             .Column("ContactGroupMemberRelatedID");

            if (isInAllGroups)
            {
                groupMembers.GroupBy("ContactGroupMemberRelatedID")
                            .Having(w => 
                                w.WhereEquals(new CountColumn("*"), groupNames.Length)
                            );
            }

            // Get contacts for the member IDs
            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.WhereNotIn("ContactID", groupMembers);
            }
            else
            {
                contacts.WhereIn("ContactID", groupMembers);
            }

            return contacts;
        }
    }
}
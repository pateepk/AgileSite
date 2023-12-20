using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactIsRegisteredAsAUserInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactIsRegisteredAsAUser Macro rule.
        /// Contact {_is} registered as a user
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;

            var contacts = ContactInfoProvider.GetContacts();
            var membership = ContactMembershipInfoProvider.GetRelationships()
                                                          .WhereEquals("MemberType", (int)MemberTypeEnum.CmsUser)
                                                          .Column("ContactID");
            
            if (paramIs == "!")
            {
                // Not registered - anonymous                
                contacts.WhereNotIn("ContactID", membership);
            }
            else
            {
                // Registered - not anonymous                
                contacts.WhereIn("ContactID", membership);
            }

            return contacts;
        }
    }
}
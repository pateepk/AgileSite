using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactBelongsToAccountInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            int accountID = ruleParameters["account"].Value.ToInteger(0);

            var relationships = AccountContactInfoProvider.GetRelationships()
                                                          .WhereEquals("AccountID", accountID)
                                                          .Column("ContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.WhereNotIn("ContactID", relationships);
            }
            else
            {
                contacts.WhereIn("ContactID", relationships);
            }

            return contacts;
        }
    }
}
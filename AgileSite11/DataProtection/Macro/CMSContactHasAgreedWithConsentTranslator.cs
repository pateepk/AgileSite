using System;

using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.DataProtection
{
    internal class CMSContactHasAgreedWithConsentTranslator : IMacroRuleInstanceTranslator
    {
        public ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance)
        {
            if (macroRuleInstance == null)
            {
                throw new ArgumentNullException(nameof(macroRuleInstance));
            }

            if (macroRuleInstance.MacroRuleName != "CMSContactHasAgreedWithConsent")
            {
                throw new ArgumentException("[CMSContactHasAgreedWithConsent.Translate]: Only macro rule instances of type CMSContactHasAgreedWithConsent can be translated");
            }

            return TranslateInternal(macroRuleInstance.Parameters);
        }


        private ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string consentName = ruleParameters["consent"].Value;

            var consent = ConsentInfoProvider.GetConsentInfo(consentName);
            if (consent == null)
            {
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            var contactIdsWhoAgreed = ConsentAgreementService.GetContactIDsWhoAgreed(consent);

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", contactIdsWhoAgreed);
            }
            else
            {
                contacts.WhereIn("ContactID", contactIdsWhoAgreed);
            }

            return contacts;
        }
    }
}

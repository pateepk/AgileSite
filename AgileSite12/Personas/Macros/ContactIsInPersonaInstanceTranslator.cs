using System;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Personas
{
    /// <summary>
    /// Translator for Contact is in persona macro rule.
    /// </summary>
    internal class ContactIsInPersonaInstanceTranslator : IMacroRuleInstanceTranslator
    {
        /// <summary>
        /// Translates ContactIsInPersona Macro rule. Macro is in format 'Contact {_is} in persona {personaGuid}'.
        /// </summary>
        public ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance)
        {
            if (macroRuleInstance == null)
            {
                throw new ArgumentNullException("macroRuleInstance");
            }
            if (macroRuleInstance.MacroRuleName != "ContactIsInPersona")
            {
                throw new ArgumentException("[ContactIsInPersonaTranslator.Translate]: Only macro rule instances of type ContactIsInPersona can be translated");
            }

            var ruleParameters = macroRuleInstance.Parameters;
            var personaGuid = new Guid(ruleParameters["personaGuid"].Value);
            string paramIs = ruleParameters["_is"].Value;

            var personaIDQuery = PersonaInfoProvider.GetPersonas().WhereEquals("PersonaGuid", personaGuid).Column("PersonaID");

            var contacts = ContactInfoProvider.GetContacts().Column("ContactID");
            if (paramIs == "!")
            {
                contacts.Where(new WhereCondition().WhereNotIn("ContactPersonaID", personaIDQuery)
                                                   .Or()
                                                   .WhereNull("ContactPersonaID"));
            }
            else
            {
                contacts.WhereIn("ContactPersonaID", personaIDQuery);
            }

            return contacts;
        }
    }
}
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactAgeIsBetweenInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactAgeIsBetween
        /// Contact age {_is} between {age1} and {age2}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            int ageInYearsFrom = ruleParameters["age1"].Value.ToInteger(0);
            int ageInYearsTo = ruleParameters["age2"].Value.ToInteger(0);
            string paramIs = ruleParameters["_is"].Value;

            var query = ContactInfoProvider.GetContacts()
                                           .OlderThanOrWithAge(ageInYearsFrom)
                                           .YoungerThanOrWithAge(ageInYearsTo);

            if (paramIs == "!")
            {
                return ContactInfoProvider.GetContacts().WhereNot(query);
            }

            return query;
        }
    }
}
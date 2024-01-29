using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactAgeIsGreaterThanInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactAgeIsGreaterThan macro rule.
        /// Contact age {op} {age}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            int ageInYears = ValidationHelper.GetInteger(ruleParameters["age"].Value, 0);
            string paramOperator = ruleParameters["op"].Value;

            var contactsQuery = ContactInfoProvider.GetContacts();

            switch (paramOperator)
            {
                case "==":
                    contactsQuery.WithAge(ageInYears);
                    break;
                case "!=":
                    contactsQuery.NotWithAge(ageInYears);
                    break;
                case ">":
                    contactsQuery.OlderThan(ageInYears);
                    break;
                case ">=":
                    contactsQuery.OlderThanOrWithAge(ageInYears);
                    break;
                case "<":
                    contactsQuery.YoungerThan(ageInYears);
                    break;
                case "<=":
                    contactsQuery.YoungerThanOrWithAge(ageInYears);
                    break;
            }

            return contactsQuery;
        }
    }
}
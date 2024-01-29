using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactStatusIsInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactStatusIs Macro rule.
        /// Contact status {_is} {status}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string statusName = ruleParameters["status"].Value;

            var statuses = ContactStatusInfoProvider.GetContactStatuses()
                                                    .WhereEquals("ContactStatusName", statusName)
                                                    .Column("ContactStatusID");

            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.Where(new WhereCondition().WhereNotIn("ContactStatusID", statuses)
                                                   .Or()
                                                   .WhereNull("ContactStatusID"));
            }
            else
            {
                contacts.WhereIn("ContactStatusID", statuses);
            }
            return contacts;
        }
    }
}
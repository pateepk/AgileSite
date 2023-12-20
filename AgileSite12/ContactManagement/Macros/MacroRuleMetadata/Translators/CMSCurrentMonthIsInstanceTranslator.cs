using System;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSCurrentMonthIsInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSCurrentMonthIs Macro rule.
        /// Current month {_is} one of the following: {months}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            bool isNot = (ruleParameters["_is"].Value == "!");
            var now = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            string paramMonths = ruleParameters["months"].Value;

            // Get months
            var months = paramMonths.Split(new[]
            {
                '|'
            }, StringSplitOptions.RemoveEmptyEntries);

            bool currentMonthInMonths = months.Contains(now.Month.ToString());

            var contacts = ContactInfoProvider.GetContacts();
            if ((isNot && currentMonthInMonths) || (!isNot && !currentMonthInMonths))
            {
                contacts.NoResults();
            }

            return contacts;
        }
    }
}
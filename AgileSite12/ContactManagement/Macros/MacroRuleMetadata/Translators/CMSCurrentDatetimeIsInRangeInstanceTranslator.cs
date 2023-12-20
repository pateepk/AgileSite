using System;

using CMS.Base;
using CMS.Core;
using CMS.ContactManagement.Internal;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSCurrentDatetimeIsInRangeInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSCurrentDatetimeIsInRange Macro rule.
        /// Current date/time {_is} between {date1} and {date2}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            bool isNot = (ruleParameters["_is"].Value == "!");
            var now = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            DateTime from = ValidationHelper.GetDateTimeSystem(ruleParameters["date1"].Value, DateTimeHelper.ZERO_TIME);
            DateTime to = ValidationHelper.GetDateTimeSystem(ruleParameters["date2"].Value, now + TimeSpan.FromDays(1));
            var nowInRange = (now > from) && (now < to);

            var contacts = ContactInfoProvider.GetContacts();
            if ((isNot && nowInRange) || (!isNot && !nowInRange))
            {
                contacts.NoResults();
            }

            return contacts;
        }
    }
}
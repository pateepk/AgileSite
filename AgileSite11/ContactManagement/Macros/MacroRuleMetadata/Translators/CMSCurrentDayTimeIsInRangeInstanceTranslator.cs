using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSCurrentDayTimeIsInRangeInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSCurrentDayTimeIsInRange Macro rule.
        /// Current day time {_is} between {time1} and {time2}
        /// </summary>
        /// <returns>Returns <c>true</c> when current is between time1 and time2 range.</returns>>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            bool isNot = (ruleParameters["_is"].Value == "!");
            var timeNow = Service.Resolve<IDateTimeNowService>().GetDateTimeNow().TimeOfDay;
            var from = TimeSpan.Parse(ruleParameters["time1"].Value);
            var to = TimeSpan.Parse(ruleParameters["time2"].Value);
            var nowInRange = (timeNow > from) && (timeNow < to);

            if ((!isNot && nowInRange) || (isNot && !nowInRange))
            {
                return ContactInfoProvider.GetContacts();
            }

            return ContactInfoProvider.GetContacts().NoResults();
        }
    }
}
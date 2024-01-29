using System.Linq;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSCurrentDayOfTheWeekIsOneOfSpecifiedDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSCurrentDayOfTheWeekIsOneOfSpecifiedDays Macro rule.
        /// Current day of the week {_is} one of following: {days}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            bool isNot = (ruleParameters["_is"].Value == "!");
            var now = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            var selectedDays = ruleParameters["days"].Value.Split('|').Select(x => x.ToInteger(-1));
            var currentDayFits = selectedDays.Contains((int)now.DayOfWeek);


            if ((!isNot && currentDayFits) || (isNot && !currentDayFits))
            {
                return ContactInfoProvider.GetContacts();                
            }

            return ContactInfoProvider.GetContacts().NoResults();
        }
    }
}
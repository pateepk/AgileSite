using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactWasCreatedInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramOp = ruleParameters["op"].Value;
            DateTime paramDate = ValidationHelper.GetDateTimeSystem(ruleParameters["date"].Value, DateTime.MinValue);

            DateTime startOfTheDay = paramDate.Date;
            DateTime endOfTheDay = startOfTheDay.AddDays(1);

            var query = ContactInfoProvider.GetContacts();

            switch (paramOp)
            {
                case "==":
                    query.CreatedAfter(startOfTheDay)
                         .CreatedBefore(endOfTheDay);
                    break;
                case "<":
                    query.CreatedBefore(startOfTheDay);
                    break;
                case ">":
                    query.CreatedAfter(endOfTheDay);
                    break;
                default:
                    throw new Exception("[CMSContactWasCreatedInstanceTranslator.Translate]: Unknown operator: " + paramOp);
            }

            return query;
        }
    }
}
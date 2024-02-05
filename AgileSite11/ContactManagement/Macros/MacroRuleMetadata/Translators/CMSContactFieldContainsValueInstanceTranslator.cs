using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactFieldContainsValueInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string columnName = ruleParameters["field"].Value;
            string value = ruleParameters["value"].Value;
            string op = ruleParameters["op"].Value;

            var query = ContactInfoProvider.GetContacts();

            switch (op.ToLowerCSafe())
            {
                case "contains":
                    query.WhereContains(columnName, value);
                    break;
                case "notcontains":
                    query.Where(w => w.WhereNotContains(columnName, value).Or().WhereNull(columnName));
                    break;
                case "startswith":
                    query.WhereStartsWith(columnName, value);
                    break;
                case "endswith":
                    query.WhereEndsWith(columnName, value);
                    break;
                case "equals":
                    query.WhereEquals(columnName, value);
                    break;
                case "notequals":
                    query.Where(w => w.WhereNotEquals(columnName, value).Or().WhereNull(columnName));
                    break;
                default:
                    throw new Exception("[CMSContactFieldContainsValueInstanceTranslator.TranslateInternal]: Unknown operator");
            }

            return query;
        }
    }
}
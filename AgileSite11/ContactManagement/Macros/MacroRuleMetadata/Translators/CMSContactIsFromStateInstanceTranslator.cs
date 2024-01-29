using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactIsFromStateInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string states = ruleParameters["states"].Value;

            var stateNames = states.Split(new[]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);

            var stateIDsQuery = StateInfoProvider.GetStates().WhereIn("StateName", stateNames).Column("StateID");

            var query = ContactInfoProvider.GetContacts();

            if (paramIs == "!")
            {
                query.Where(w => w
                    .WhereNotIn("ContactStateID", stateIDsQuery)
                    .Or()
                    .WhereNull("ContactStateID")
                );
            }
            else
            {
                query.WhereIn("ContactStateID", stateIDsQuery);
            }

            return query;
        }
    }
}
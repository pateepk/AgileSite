using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.ContactManagement
{
    internal class CMSContactIsMaleInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;

            if (paramIs == "!")
            {
                return ContactInfoProvider.GetContacts().Where(w => w.WhereNotEquals("ContactGender", (int)UserGenderEnum.Male).Or().WhereNull("ContactGender"));
            }
            
            return ContactInfoProvider.GetContacts().WhereEquals("ContactGender", (int)UserGenderEnum.Male);
        }
    }
}
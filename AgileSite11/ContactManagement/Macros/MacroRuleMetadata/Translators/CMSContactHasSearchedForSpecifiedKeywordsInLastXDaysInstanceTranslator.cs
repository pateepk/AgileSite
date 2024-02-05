using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasSearchedForSpecifiedKeywordsInLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasSearchedForSpecifiedKeywordsInLastXDays
        /// Contact {_perfectum} searched for {_any} of the following keywords in last {days} day(s): {keywords}
        /// {_perfectum}Contact.SearchedForKeywords("{keywords}", ToInt({days}), {_any})
        /// keywords: Enter keywords separated with semicolon.
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            bool allKeywords = ruleParameters["_any"].Value.ToBoolean(false);
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);
            string paramKeywords = ruleParameters["keywords"].Value;

            string[] keywords = paramKeywords.Split(new[]
            {
                ';',
                ',',
            }, StringSplitOptions.RemoveEmptyEntries);

            var contacts = ContactInfoProvider.GetContacts();
            if (allKeywords)
            {
                contacts.SearchedForAll(keywords, lastXDays);
            }
            else
            {
                contacts.SearchedForAny(keywords, lastXDays);
            }
                    
            if (perfectum == "!")
            {
                return ContactInfoProvider.GetContacts().WhereNotIn("ContactID", contacts.Column("ContactID"));
            }

            return contacts;
        }
    }
}
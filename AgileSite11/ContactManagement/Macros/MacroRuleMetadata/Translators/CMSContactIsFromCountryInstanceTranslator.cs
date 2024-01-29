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
    internal class CMSContactIsFromCountryInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string countries = ruleParameters["countries"].Value;

            var countryNames = countries.Split(new[]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);

            var countryIDsQuery = CountryInfoProvider.GetCountries().WhereIn("CountryName", countryNames).Column("CountryID");

            var query = ContactInfoProvider.GetContacts();

            if (paramIs == "!")
            {
                query.Where(w => w
                    .WhereNotIn("ContactCountryID", countryIDsQuery)
                    .Or()
                    .WhereNull("ContactCountryID")
                );
            }
            else
            {
                query.WhereIn("ContactCountryID", countryIDsQuery);
            }

            return query;
        }
    }
}
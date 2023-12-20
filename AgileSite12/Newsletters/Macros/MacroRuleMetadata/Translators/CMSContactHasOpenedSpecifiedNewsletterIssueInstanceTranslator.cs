using System;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    internal class CMSContactHasOpenedSpecifiedNewsletterIssueInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Contact {_perfectum} opened newsletter issue {issue}.
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            var perfectum = ruleParameters["_perfectum"].Value;
            var issueGuid = ValidationHelper.GetGuid(ruleParameters["issue"].Value, Guid.Empty);

            var activities = IssueActivitiesRetriever.GetActivitiesQuery(issueGuid, PredefinedActivityType.NEWSLETTER_OPEN)
                .Column("ActivityContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activities);
            }
            else
            {
                contacts.WhereIn("ContactID", activities);
            }

            return contacts;
        }
    }
}
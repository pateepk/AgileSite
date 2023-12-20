using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    internal class CMSContactHasClickedALinkInNewsletterIssueInLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasClickedALinkInNewsletterIssueInLastXDays Macro rule.
        /// Contact {_perfectum} clicked a link in newsletter issue {issue} in the last {days} days.
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            var perfectum = ruleParameters["_perfectum"].Value;
            var issueGuid = ValidationHelper.GetGuid(ruleParameters["issue"].Value, Guid.Empty);
            var days = ruleParameters["days"].Value.ToInteger(0);

            var activities = IssueActivitiesRetriever.GetActivitiesQuery(issueGuid, PredefinedActivityType.NEWSLETTER_CLICKTHROUGH)
                .Column("ActivityContactID"); 

            if (days > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(days));
            }

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
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
    /// <summary>
    /// Translates CMSContactHasClickedALinkInNewsletterInTheLastXDays Macro rule.
    /// Contact {_perfectum} clicked a link in newsletter {item} in the last {days} day(s)
    /// </summary>
    internal class CMSContactHasClickedALinkInNewsletterInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string newsletterName = ruleParameters["item"].Value;
            int days = ValidationHelper.GetInteger(ruleParameters["days"].Value, 0);

            var newsletter = NewsletterInfoProvider.GetNewsletters()
                                                   .WhereEquals("NewsletterName", newsletterName)
                                                   .Column("NewsletterID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.NEWSLETTER_CLICKTHROUGH)
                                                 .WhereIn("ActivityItemID", newsletter)
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
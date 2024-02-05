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
    internal class CMSContactHasOpenedSpecifiedNewsletterInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasOpenedSpecifiedNewsletterInTheLastXDays Macro rule.
        /// Contact {_perfectum} opened newsletter {item} in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            var stringGuid = ruleParameters["item"].Value;
            string perfectum = ruleParameters["_perfectum"].Value;
            Guid newsletterGuid = ValidationHelper.GetGuid(stringGuid, Guid.Empty);
            int days = ValidationHelper.GetInteger(ruleParameters["days"].Value, 0);

            if (newsletterGuid == Guid.Empty)
            {
                MacroValidationHelper.LogInvalidGuidParameter("OpenedNewsletter", stringGuid);
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            var newsletter = NewsletterInfoProvider.GetNewsletters()
                                                   .WhereEquals("NewsletterGuid", newsletterGuid)
                                                   .Column("NewsletterID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.NEWSLETTER_OPEN)
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
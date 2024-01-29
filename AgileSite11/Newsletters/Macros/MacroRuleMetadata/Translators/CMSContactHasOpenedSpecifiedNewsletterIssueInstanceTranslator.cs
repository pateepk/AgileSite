﻿using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
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
            string perfectum = ruleParameters["_perfectum"].Value;
            string issueGuid = ruleParameters["issue"].Value;

            var issues = IssueInfoProvider.GetIssues()
                                                   .WhereEquals("IssueGuid", issueGuid)
                                                   .Column("IssueID");

            var issueVariants = IssueInfoProvider.GetIssues()
                                                 .WhereIn("IssueVariantOfIssueID", issues)
                                                 .Column("IssueID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.NEWSLETTER_OPEN)
                                                 .Where(where => where
                                                    .WhereIn("ActivityItemDetailID", issues)
                                                    .Or().WhereIn("ActivityItemDetailID", issueVariants)
                                                 )
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
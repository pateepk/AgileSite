using System;
using System.Collections.Generic;

using CMS.Activities;
using CMS.ContactManagement;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for registering <see cref="MacroRuleMetadata"/> to the system.
    /// </summary>
    internal static class MacroRuleMetadataContainer
    {
        /// <summary>
        /// Registers default metadata for existing macro rules.
        /// </summary>
        internal static void RegisterNewsletterMetadata()
        {
            var noNeedToRecalculate = new List<string>(0);

            Func<string, List<string>> affecting = s => new List<string>(1)
            {
                s
            };

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactIsSubscribedToSpecifiedNewsletter",
                new CMSContactIsSubscribedToSpecifiedNewsletterInstanceTranslator(), new List<string>
                {
                    PredefinedActivityType.NEWSLETTER_SUBSCRIBING,
                    PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING,
                    PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING_FROM_ALL
                }, noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasClickedALinkInNewsletterIssue",
                new CMSContactHasClickedALinkInNewsletterIssueInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_CLICKTHROUGH), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasOpenedSpecifiedNewsletterIssueInTheLastXDays",
                new CMSContactHasOpenedSpecifiedNewsletterIssueInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_OPEN), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasClickedALinkInNewsletterInTheLastXDays",
                new CMSContactHasClickedALinkInNewsletterInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_CLICKTHROUGH), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasClickedALinkInNewsletterIssueInLastXDays",
                new CMSContactHasClickedALinkInNewsletterIssueInLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_CLICKTHROUGH), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasOpenedSpecifiedNewsletterInTheLastXDays",
                new CMSContactHasOpenedSpecifiedNewsletterInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_OPEN), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasOpenedSpecifiedNewsletterIssue",
                new CMSContactHasOpenedSpecifiedNewsletterIssueInstanceTranslator(), affecting(PredefinedActivityType.NEWSLETTER_OPEN), noNeedToRecalculate)
                );

            ContactManagement.MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata(
                "CMSContactHasUnsubscribedFromAllEmailsInTheLastXDays",
                new CMSContactHasUnsubscribedFromAllEmailsInTheLastXDaysInstanceTranslator(), new[]
                    {
                        PredefinedActivityType.NEWSLETTER_SUBSCRIBING,
                        PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING_FROM_ALL
                    }, affecting("contactemail"))
                );
        }
    }
}

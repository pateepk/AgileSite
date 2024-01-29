using System;
using System.Collections.Generic;

using CMS.Activities;
using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for registering <see cref="MacroRuleMetadata"/> to the system. Use <see cref="RegisterMetadata(MacroRuleMetadata)"/> or <see cref="RegisterMetadata(Dictionary{string, MacroRuleMetadata}, MacroRuleMetadata)"/> to speed up recalculation of contact groups 
    /// when registered macro rule is used in group condition.
    /// </summary>
    /// <remarks>
    /// Registering <see cref="MacroRuleMetadata"/> can have two profound effects on speed of recalculation. 
    /// 
    /// First is recalculating contacts in the database - normally, when a contact group is being rebuilt, system takes all contacts from database and runs the macro 
    /// condition on each of those. By registering a translator to a rule, the computation can be inverted, that means at first, a database query will be constructed 
    /// to select only those contacts that fit given macro condition.
    /// 
    /// All of the macro rules must be able to translate themselves to database query to use this functionality.
    /// 
    /// 
    /// Second is recalculating the rule only when specified type of activity performs, for example when a <see cref="PredefinedActivityType.PAGE_VISIT"/> performs,
    /// there is no need to recalculate <see cref="PredefinedActivityType.NEWSLETTER_OPEN"/> macro rules. This can significantly reduce the count of contact groups
    /// that need rebuild on each request.
    /// </remarks>
    public static class MacroRuleMetadataContainer
    {
        private static Dictionary<string, MacroRuleMetadata> mMetadata;
        private static readonly object lockObject = new object();


        /// <summary>
        /// Translators container.
        /// </summary>
        private static Dictionary<string, MacroRuleMetadata> Metadata
        {
            get
            {
                return LockHelper.Ensure(ref mMetadata, GetDefaultMetadata, lockObject);
            }
        }

        
        /// <summary>
        /// Gets a translator for given macro rule codename.
        /// </summary>
        internal static MacroRuleMetadata GetMetadata(string ruleName)
        {
            if (ruleName == null)
            {
                throw new ArgumentNullException("ruleName");
            }

            return Metadata[ruleName];
        }


        /// <summary>
        /// Checks whether <see cref="MacroRuleMetadata"/> is registered for given macro rule codename.
        /// </summary>
        /// <returns>True if <see cref="MacroRuleMetadata"/> is registered</returns>
        internal static bool IsMetadataAvailable(string ruleName)
        {
            if (ruleName == null)
            {
                throw new ArgumentNullException("ruleName");
            }

            return Metadata.ContainsKey(ruleName);
        }


        /// <summary>
        /// Checks whether a translator is registered for given macro rule codename.
        /// </summary>
        /// <returns>True if translator is registered</returns>
        public static bool IsTranslatorAvailable(string ruleName)
        {
            if (ruleName == null)
            {
                throw new ArgumentNullException("ruleName");
            }

            return Metadata.ContainsKey(ruleName) && (Metadata[ruleName].Translator != null);
        }


        /// <summary>
        /// Registers metadata for a macro rule. Overrides already registered translators if the <see cref="MacroRuleMetadata.MacroRuleName"/> collides.
        /// </summary>
        /// <param name="dict">Metadata dictionary</param>
        /// <param name="metadata">Metadata to register</param>
        private static void RegisterMetadata(Dictionary<string, MacroRuleMetadata> dict, MacroRuleMetadata metadata)
        {
            dict[metadata.MacroRuleName] = metadata;
        }


        /// <summary>
        /// Registers metadata for a macro rule. Overrides already registered translators if the <see cref="MacroRuleMetadata.MacroRuleName"/> collides.
        /// </summary>
        /// <param name="metadata">Metadata to register</param>
        public static void RegisterMetadata(MacroRuleMetadata metadata)
        {
            RegisterMetadata(Metadata, metadata);
        }


        /// <summary>
        /// Registers default metadata for existing macro rules.
        /// </summary>
        private static Dictionary<string, MacroRuleMetadata> GetDefaultMetadata()
        {
            var dict = new Dictionary<string, MacroRuleMetadata>();

            var noNeedToRecalculate = new List<string>(0);
            var allAttributes = new List<string>(1)
            {
                MacroRuleMetadata.ALL_ATTRIBUTES
            };

            var allActivities = new List<string>(1)
            {
                MacroRuleMetadata.ALL_ACTIVITIES
            };

            Func<string, List<String>> affecting = s => new List<string>(1) { s };

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactFieldContainsValue",
                new CMSContactFieldContainsValueInstanceTranslator(), noNeedToRecalculate, allAttributes)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactAgeIsBetween",
                new CMSContactAgeIsBetweenInstanceTranslator(), noNeedToRecalculate, affecting("contactbirthday"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactAgeIsGreaterThan",
                new CMSContactAgeIsGreaterThanInstanceTranslator(), noNeedToRecalculate, affecting("contactbirthday"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsFemale",
                new CMSContactIsFemaleInstanceTranslator(), noNeedToRecalculate, affecting("contactgender"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsMale",
                new CMSContactIsMaleInstanceTranslator(), noNeedToRecalculate, affecting("contactgender"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsFromCountry",
                new CMSContactIsFromCountryInstanceTranslator(), noNeedToRecalculate, affecting("contactcountryid"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsFromState",
                new CMSContactIsFromStateInstanceTranslator(), noNeedToRecalculate, affecting("contactstateid"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactBelongsToAccount",
                new CMSContactBelongsToAccountInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasDownloadedSpecifiedFileInLastXDays",
                new CMSContactHasDownloadedSpecifiedFileInLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.PAGE_VISIT), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactWasCreated",
                new CMSContactWasCreatedInstanceTranslator(), noNeedToRecalculate, affecting("contactcreated"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasAtLeastXProductsInWishlist",
                new CMSContactHasAtLeastXProductsInWishlistInstanceTranslator(), allActivities, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasProductInWishlist",
                new CMSContactHasProductInWishlistInstanceTranslator(), allActivities, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsInContactGroup",
                new CMSContactIsInContactGroupInstanceTranslator(), allActivities, allAttributes)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasComeToSpecifiedLandingPage",
                new CMSContactHasComeToSpecifiedLandingPageInstanceTranslator(), affecting(PredefinedActivityType.LANDING_PAGE), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasComeToLandingPageWithSpecifiedURL",
                new CMSContactHasComeToLandingPageWithSpecifiedURLTranslator(), affecting(PredefinedActivityType.LANDING_PAGE), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsRegisteredAsAUser",
                new CMSContactIsRegisteredAsAUserInstanceTranslator(), new List<string>
                {
                    PredefinedActivityType.USER_LOGIN,
                    PredefinedActivityType.REGISTRATION
                }, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsRegisteredForSpecifiedEvent",
                new CMSContactIsRegisteredForSpecifiedEventInstanceTranslator(), affecting(PredefinedActivityType.EVENT_BOOKING), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasVotedInSpecifiedPoll",
                new CMSContactHasVotedInSpecifiedPollInstanceTranslator(), affecting(PredefinedActivityType.POLL_VOTING), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasPurchasedSpecifiedProduct",
                new CMSContactHasPurchasedSpecifiedProductInstanceTranslator(), affecting(PredefinedActivityType.PURCHASEDPRODUCT), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasDoneFollowingActivitiesInTheLastXDays",
                new CMSContactHasDoneFollowingActivitiesInTheLastXDaysInstanceTranslator(), allActivities, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasLoggedInInTheLastXDays",
                new CMSContactHasLoggedInInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.USER_LOGIN), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasSearchedForSpecifiedKeywordsInLastXDays",
                new CMSContactHasSearchedForSpecifiedKeywordsInLastXDaysInstanceTranslator(), new List<string>
                {
                    PredefinedActivityType.INTERNAL_SEARCH,
                    PredefinedActivityType.EXTERNAL_SEARCH
                }, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasSubmittedSpecifiedFormInLastXDays",
                new CMSContactHasSubmittedSpecifiedFormInLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.BIZFORM_SUBMIT), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasVisitedSpecifiedPageInLastXDays",
                new CMSContactHasVisitedSpecifiedPageInLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.PAGE_VISIT), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasDoneAnyActivityInTheLastXDays",
                new CMSContactHasDoneAnyActivityInTheLastXDaysInstanceTranslator(), allActivities, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasPurchasedNumberOfProductsInTheLastXDays",
                new CMSContactHasPurchasedNumberOfProductsInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.PURCHASEDPRODUCT), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasSpentMoneyInTheStoreInTheLastXDays",
                new CMSContactHasSpentMoneyInTheStoreInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.PURCHASE), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasMadeAtLeastXOrders",
                new CMSContactHasMadeAtLeastXOrdersInstanceTranslator(), affecting(PredefinedActivityType.PURCHASE), noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasDoneFollowingActivitiesInTheLastXDays",
                new CMSContactHasDoneFollowingActivitiesInTheLastXDaysInstanceTranslator(), allActivities, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsInCommunityGroup",
                new CMSContactIsInCommunityGroupInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSCurrentDatetimeIsInRange",
                new CMSCurrentDatetimeIsInRangeInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSCurrentMonthIs",
                new CMSCurrentMonthIsInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSCurrentDayOfTheWeekIsOneOfSpecifiedDays",
                new CMSCurrentDayOfTheWeekIsOneOfSpecifiedDaysInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSCurrentDayTimeIsInRange",
                new CMSCurrentDayTimeIsInRangeInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactIsInRole",
                new CMSContactIsInRoleInstanceTranslator(), noNeedToRecalculate, noNeedToRecalculate)
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactStatusIs",
                new CMSContactStatusIsInstanceTranslator(), noNeedToRecalculate, affecting("contactstatusid"))
                );

            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasVisitedPageWithURLInLastXDays",
                new CMSContactHasVisitedPageWithURLInLastXDaysTranslator(), affecting(PredefinedActivityType.PAGE_VISIT), noNeedToRecalculate)
                );
            
            RegisterMetadata(dict, new MacroRuleMetadata(
                "CMSContactHasVisitedSpecifiedSiteInTheLastXDays",
                new CMSContactHasVisitedSpecifiedSiteInTheLastXDaysInstanceTranslator(), affecting(PredefinedActivityType.PAGE_VISIT), noNeedToRecalculate)
                );

            return dict;
        }
    }
}
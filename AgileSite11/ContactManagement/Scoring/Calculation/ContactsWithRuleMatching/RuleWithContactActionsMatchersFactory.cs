using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.Activities;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IRuleWithContactActionsMatchersFactory), typeof(RuleWithContactActionsMatchersFactory), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides implementations of <see cref="IContactsAffectedByRuleMatcher"/> for each type of scoring rule. Created implementations
    /// will have a filled state (performed activities or contact changes) and produced contact IDs will be taken from this state.
    /// </summary>
    internal class RuleWithContactActionsMatchersFactory : IRuleWithContactActionsMatchersFactory
    {
        /// <summary>
        /// Creates rules matchers that selects contacts which can be affected by the scoring rule. Contacts are selected from the list of contact actions represented
        /// by performed activities (<paramref name="activities"/>) and changes made to contact properties (<paramref name="contactChanges"/>).
        /// </summary>
        /// <param name="activities">Activities performed by contacts</param>
        /// <param name="contactChanges">Changes made to contact properties</param>
        /// <returns>Matchers index by rule type that are able to match scoring rules against contact actions</returns>
        public Dictionary<RuleTypeEnum, IContactsAffectedByRuleMatcher> CreateRulesMatchers(IList<ActivityInfo> activities, IList<ContactChangeData> contactChanges)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            if (contactChanges == null)
            {
                throw new ArgumentNullException("contactChanges");
            }

            return new Dictionary<RuleTypeEnum, IContactsAffectedByRuleMatcher>
            {
                { RuleTypeEnum.Activity, new ContactsAffectedByActivityRulesMatcher(activities, contactChanges) },
                { RuleTypeEnum.Attribute, new ContactsAffectedByAttributeRulesMatcher(contactChanges) },
                { RuleTypeEnum.Macro, new ContactsAffectedByMacroRulesMatcher(activities, contactChanges) },
            };
        }
    }
}
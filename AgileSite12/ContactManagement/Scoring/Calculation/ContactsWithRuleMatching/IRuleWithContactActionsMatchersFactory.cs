using System.Collections.Generic;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides implementations of <see cref="IContactsAffectedByRuleMatcher"/> for each type of scoring rule. Created implementations
    /// will have a filled state (performed activities or contact changes) and produced contact IDs will be taken from this state.
    /// </summary>
    internal interface IRuleWithContactActionsMatchersFactory
    {
        /// <summary>
        /// Creates rules matchers that selects contacts which can be affected by the scoring rule. Contacts are selected from the list of contact actions represented
        /// by performed activities (<paramref name="activities"/>) and changes made to contact properties (<paramref name="contactChanges"/>).
        /// </summary>
        /// <param name="activities">Activities performed by contacts</param>
        /// <param name="contactChanges">Changes made to contact properties</param>
        /// <returns>Matchers index by rule type that are able to match scoring rules against contact actions</returns>
        Dictionary<RuleTypeEnum, IContactsAffectedByRuleMatcher> CreateRulesMatchers(IList<ActivityInfo> activities, IList<ContactChangeData> contactChanges);
    }
}
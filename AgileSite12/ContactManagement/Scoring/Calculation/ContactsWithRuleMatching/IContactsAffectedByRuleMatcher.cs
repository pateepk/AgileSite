using System.Collections.Generic;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Computes contacts which could be affected by a rule. In other words, provides information which contacts should be recalculated for a certain rule.
    /// </summary>
    internal interface IContactsAffectedByRuleMatcher
    {
        /// <summary>
        /// Checks which contacts were affected by a rule.
        /// </summary>
        /// <param name="rule">Rule which is matched with contact actions</param>
        /// <returns>IDs of contacts whose points for <paramref name="rule"/> should be recalculated</returns>
        ISet<int> GetAffectedContacts(RuleInfo rule);
    }
}
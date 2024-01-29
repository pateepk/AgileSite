using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks which contacts were affected by activity rule. In other words, provides information which contacts should be recalculated for a certain rule.
    /// </summary>
    /// <remarks>
    /// Contact is affected by activity rule if one of this conditions is met:
    /// - contact performed activity which matches rule in its type and other properties (NodeID, ItemID, Value, etc.)
    /// </remarks>
    internal class ContactsAffectedByActivityRulesMatcher : IContactsAffectedByRuleMatcher
    {
        private readonly IEnumerable<ActivityInfo> activities;
        private readonly IEnumerable<ContactChangeData> contactChanges;


        public ContactsAffectedByActivityRulesMatcher(IEnumerable<ActivityInfo> activities, IEnumerable<ContactChangeData> contactChanges)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            if (contactChanges == null)
            {
                throw new ArgumentNullException("contactChanges");
            }

            this.activities = activities;
            this.contactChanges = contactChanges;
        }


        /// <summary>
        /// Checks which contacts were affected by activity rule. Matching algorithm is described in class's comments.
        /// </summary>
        /// <param name="rule">Rule which is matched with contact actions</param>
        /// <returns>IDs of contacts whose points for <paramref name="rule"/> should be recalculated</returns>
        public ISet<int> GetAffectedContacts(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (rule.RuleType != RuleTypeEnum.Activity)
            {
                throw new Exception("[ContactsAffectedByActivityRulesMatcher.GetAffectedContacts]: Only rules of Activity type can be checked for affected contacts");
            }

            return new ContactsAffectingRulesFilter().FilterContacts(rule, activities.ToList(), contactChanges.ToList());
        }
    }
}
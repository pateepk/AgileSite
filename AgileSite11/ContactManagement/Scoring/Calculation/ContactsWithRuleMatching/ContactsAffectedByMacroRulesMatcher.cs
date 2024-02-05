using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks which contacts were affected by macro rule. In other words, provides information which contacts should be recalculated for a certain rule.
    /// </summary>
    /// <remarks>
    /// Contact is affected by macro rule if one of this conditions is met:
    /// - contact's property has changed (contact is present in the list of contact changes represented by <see cref="ContactChangeData"/>).
    /// - contact has performed activity which is affected by the rule's macro. Activities affected by various macro rules are defined in <see cref="MacroRuleMetadataContainer"/>.
    /// </remarks>
    internal class ContactsAffectedByMacroRulesMatcher : IContactsAffectedByRuleMatcher
    {
        private readonly IEnumerable<ActivityInfo> activities;
        private readonly IEnumerable<ContactChangeData> contactChanges;


        public ContactsAffectedByMacroRulesMatcher(IEnumerable<ActivityInfo> activities, IEnumerable<ContactChangeData> contactChanges)
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
        

        public ISet<int> GetAffectedContacts(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (rule.RuleType != RuleTypeEnum.Macro)
            {
                throw new Exception("[ContactsAffectedByMacroRulesMatcher.GetAffectedContacts]: Only rules of Macro type can be checked for affected contacts");
            }

            return new ContactsAffectingRulesFilter().FilterContacts(rule, activities.ToList(), contactChanges.ToList());
        }
    }
}
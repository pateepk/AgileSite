using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks which contacts were affected by attribute rule. In other words, provides information which contacts should be recalculated for a certain rule.
    /// </summary>
    /// <remarks>
    /// Contact is affected by attribute rule if one of this conditions is met:
    /// - contact was just created (see <see cref="ContactChangeData.ContactIsNew"/> is true)
    /// - rule is based on contact's property which was changed (<see cref="RuleInfo.RuleParameter"/> is present in <see cref="ContactChangeData.ChangedColumns"/>)
    /// 
    /// Performing activities don't affect attribute rules.
    /// </remarks>
    internal class ContactsAffectedByAttributeRulesMatcher : IContactsAffectedByRuleMatcher
    {
        private readonly IEnumerable<ContactChangeData> contactChanges;


        public ContactsAffectedByAttributeRulesMatcher(IEnumerable<ContactChangeData> contactChanges)
        {
            if (contactChanges == null)
            {
                throw new ArgumentNullException("contactChanges");
            }

            this.contactChanges = contactChanges;
        }


        /// <summary>
        /// Checks which contacts were affected by attribute rule. Matching algorithm is described in class's comments.
        /// </summary>
        /// <param name="rule">Rule which is matched with contact actions</param>
        /// <returns>IDs of contacts whose points for <paramref name="rule"/> should be recalculated</returns>
        public ISet<int> GetAffectedContacts(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (rule.RuleType != RuleTypeEnum.Attribute)
            {
                throw new Exception("[ContactsAffectedByAttributeRulesMatcher.GetAffectedContacts]: Only rules of Attribute type can be checked for affected contacts");
            }

            var affectedContactIDs = new HashSet<int>();

            foreach (var contactChange in contactChanges)
            {
                // If contact was just created or if contact mas merged into another contact, all rules have to be recalculated for this contact
                if (contactChange.ContactIsNew || contactChange.ContactWasMerged)
                {
                    affectedContactIDs.Add(contactChange.ContactID);
                }
                else
                {
                    if (contactChange.ChangedColumns.Any(rule.IsAffectedByAttributeChange))
                    {
                        affectedContactIDs.Add(contactChange.ContactID);
                    }
                }
            }

            return affectedContactIDs;
        }
    }
}
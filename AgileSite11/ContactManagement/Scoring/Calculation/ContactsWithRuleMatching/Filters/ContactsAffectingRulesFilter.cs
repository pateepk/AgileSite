using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Filter used for removing contacts for which should the rule not be recalculated after queuing period.
    /// </summary>
    internal class ContactsAffectingRulesFilter
    {
        /// <summary>
        /// Gets set of contacts affecting the given rule.
        /// </summary>
        /// <param name="rule">Rule to be checked against</param>
        /// <param name="activities">List of logged activities</param>
        /// <param name="contactChanges">List of logged contact changes</param>
        /// <returns>Set of filtered contact IDs</returns>
        public ISet<int> FilterContacts(RuleInfo rule, IList<ActivityInfo> activities, IList<ContactChangeData> contactChanges)
        {
            var resultContacts = new HashSet<int>();
            
            foreach (var contactChange in contactChanges)
            {
                // Do not check if activity affects rule if some previous attribute affected this rule for the same contact - performance optimization
                if (resultContacts.Contains(contactChange.ContactID))
                {
                    continue;
                }

                if (contactChange.ContactIsNew || contactChange.ContactWasMerged || contactChange.ChangedColumns.Any(rule.IsAffectedByAttributeChange))
                {
                    resultContacts.Add(contactChange.ContactID);
                }
            }


            foreach (var activity in activities)
            {
                // Do not check if activity affects rule if some previous attribute affected this rule for the same contact - performance optimization
                if (resultContacts.Contains(activity.ActivityContactID))
                {
                    continue;
                }

                if (rule.IsAffectedByActivity(activity))
                {
                    resultContacts.Add(activity.ActivityContactID);
                }
            }

            return resultContacts;
        }
    }
}

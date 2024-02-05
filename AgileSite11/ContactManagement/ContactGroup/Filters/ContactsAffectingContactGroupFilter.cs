using System.Collections.Generic;
using System.Linq;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Filter used for removing contacts for which should contact group not be recalculated after queuing period.
    /// </summary>
    internal class ContactsAffectingContactGroupFilter
    {
        /// <summary>
        /// Gets set of contacts affecting the given contact group.
        /// </summary>
        /// <param name="contactGroup">Contact group to be checked against</param>
        /// <param name="activities">List of logged activities</param>
        /// <param name="contactChanges">List of logged contact changes</param>
        /// <returns>Set of filtered contact IDs</returns>
        public ISet<int> FilterContacts(ContactGroupInfo contactGroup, IList<IActivityInfo> activities, IList<ContactChangeData> contactChanges)
        {
            var resultContacts = new HashSet<int>();

            foreach (var contactChange in contactChanges)
            {
                // Do not check if activity affects rule if some previous attribute affected this rule for the same contact - performance optimization
                if (resultContacts.Contains(contactChange.ContactID))
                {
                    continue;
                }

                if (contactChange.ContactIsNew || contactChange.ContactWasMerged || contactChange.ChangedColumns.Any(contactGroup.IsAffectedByAttributeChange))
                {
                    resultContacts.Add(contactChange.ContactID);
                }
            }


            foreach (var activity in activities)
            {
                // Do not check if activity affects rule if some previous attribute/activity affected this rule for the same contact - performance optimization
                if (resultContacts.Contains(activity.ActivityContactID))
                {
                    continue;
                }

                if (contactGroup.IsAffectedByActivityType(activity.ActivityType))
                {
                    resultContacts.Add(activity.ActivityContactID);
                }
            }

            return resultContacts;
        }
    }
}

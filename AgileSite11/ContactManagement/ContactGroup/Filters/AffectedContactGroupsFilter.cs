using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Filter used for removing contact groups which should not be recalculated after queuing period.
    /// </summary>
    internal class AffectedContactGroupsFilter
    {
        /// <summary>
        /// Filter contact groups to match only the ones that are affected either by activity or attribute change.
        /// </summary>
        /// <param name="contactGroups">Contact groups to be filtered</param>
        /// <param name="activityTypes">Collection of logged activity types</param>
        /// <param name="changedColumns">Collection of logged contact column changes</param>
        /// <returns>Filtered collection of contact groups with removed duplicities</returns>
        public IEnumerable<ContactGroupInfo> FilterContactGroups(IList<ContactGroupInfo> contactGroups, ISet<string> activityTypes, ISet<string> changedColumns)
        {          
            return contactGroups
                .Where(contactGroup => 
                    (activityTypes.Any(contactGroup.IsAffectedByActivityType))
                    || 
                    (changedColumns.Any(contactGroup.IsAffectedByAttributeChange))
                );
        }
    }
}

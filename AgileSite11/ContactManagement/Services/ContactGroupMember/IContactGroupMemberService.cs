using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactGroupMemberService), typeof(ContactGroupMemberService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for members in contact groups. 
    /// </summary>
    public interface IContactGroupMemberService
    {
        /// <summary>
        /// Gets count of contacts in contact groups.
        /// </summary>
        /// <param name="contactGroupsId">Contact groups to get count of contacts for</param>
        /// <returns>Count of contacts matched with contact groups.</returns>
        IEnumerable<ContactGroupMembersCount> GetCountOfContactsInContactGroup(IEnumerable<int> contactGroupsId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for members in contact groups. 
    /// </summary>
    internal class ContactGroupMemberService : IContactGroupMemberService
    {
        /// <summary>
        /// Gets count of contacts in contact groups.
        /// </summary>
        /// <param name="contactGroupsId">Contact groups to get count of contacts for</param>
        /// <returns>Count of contacts matched with contact groups.</returns>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="contactGroupsId"/> is null</exception>
        public IEnumerable<ContactGroupMembersCount> GetCountOfContactsInContactGroup(IEnumerable<int> contactGroupsId)
        {
            if (contactGroupsId == null)
            {
                throw new ArgumentNullException("contactGroupsId");
            }

            var countsQuery =
                ContactGroupMemberInfoProvider.GetRelationships()
                    .Columns("ContactGroupMemberContactGroupID")
                    .AddColumn(
                        new CountColumn().As("ContactsCount")
                    )
                    .WhereIn(
                        "ContactGroupMemberContactGroupID",
                        contactGroupsId.ToList()
                    )
                    .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                    .WhereIn(
                        "ContactGroupMemberRelatedID",
                        ContactInfoProvider.GetContacts()
                            .Column("ContactID")
                            .WhereNotEmpty("ContactEmail")
                    )
                    .GroupBy("ContactGroupMemberContactGroupID");

            return countsQuery.Select(
                        item => new ContactGroupMembersCount
                        {
                            ContactGroupID = (int)item["ContactGroupMemberContactGroupID"],
                            MembersCount = (int)item["ContactsCount"]
                        }
                    );
        }
    }
}
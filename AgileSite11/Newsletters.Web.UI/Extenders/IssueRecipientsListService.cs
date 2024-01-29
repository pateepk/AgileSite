using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Service for retrieving bounced and unsubscribed contacts 
    /// </summary>
    public class IssueRecipientsListService
    {
        /// <summary>
        /// Filters contact ids which exceed <paramhref name="bounceLimit">bounce limit</paramhref>.
        /// </summary>
        /// <param name="contactIds">Contact ids</param>
        /// <param name="bounceLimit">Bounce limit</param>
        /// <returns>Contact ids</returns>
        public IList<int> GetBouncedContactsIds(IList<int> contactIds, int bounceLimit)
        {
            var contactsWithinBounceLimit = GetContactIdQuery(contactIds).WithoutBounces(bounceLimit);

            return GetContactIdQuery(contactIds).WhereNotIn("ContactID", contactsWithinBounceLimit)
                                                .GetListResult<int>();
        }


        /// <summary>
        /// Filters unsubscribed contact ids for given <paramhref name="newsletterId">newsletter id</paramhref>.
        /// </summary>
        /// <param name="contactIds">Contact ids</param>
        /// <param name="newsletterId">Newsletter id</param>
        /// <returns>Contact ids</returns>
        public IList<int> GetUnsubscribedContactsIds(IList<int> contactIds, int newsletterId)
        {
            var subscribedContacts = GetContactIdQuery(contactIds).WithoutUnsubscribed(newsletterId);

            return GetContactIdQuery(contactIds).WhereNotIn("ContactID", subscribedContacts)
                                                .GetListResult<int>();
        }
        
        private static ObjectQuery<ContactInfo> GetContactIdQuery(IList<int> contactIds)
        {
            return ContactInfoProvider.GetContacts()
                                      .Columns("ContactID")
                                      .WhereIn("ContactID", contactIds);
        }


        /// <summary>
        /// Returns contact group for given <paramhref name="issueId">issue id</paramhref> and <paramhref name="contactIds">contact ids</paramhref>.
        /// </summary>
        /// <param name="contactIds">Contact ids</param>
        /// <param name="issueId">Issue id</param>
        /// <returns>Tuple collection - contactId, ContactGroupDisplayName</returns>
        public IEnumerable<Tuple<int, string>> GetContactsAndContactGroups(IEnumerable<int> contactIds, int issueId)
        {
            return IssueContactGroupInfoProvider.GetIssueContactGroups()
                                                .Columns("ContactGroupDisplayName", "ContactGroupMemberRelatedID")
                                                .Source(s => s
                                                        .Join("OM_ContactGroupMember", "ContactGroupID", "ContactGroupMemberContactGroupID")
                                                        .Join("OM_ContactGroup", "ContactGroupMemberContactGroupID", "ContactGroupID")
                                                )
                                                .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                                                .WhereIn(
                                                    "ContactGroupMemberRelatedID",
                                                    contactIds.ToList()
                                                )
                                                .WhereEquals("IssueID", issueId)
                                                .OrderBy("ContactGroupDisplayName")
                                                .Select(dr => 
                                                        new Tuple<int, string>(
                                                            ValidationHelper.GetInteger(dr["ContactGroupMemberRelatedID"], 0), 
                                                            ValidationHelper.GetString(dr["ContactGroupDisplayName"], "")
                                                        )
                                                );
        }


        /// <summary>
        /// Returns contact full name for given <paramref name="contactIds"/>
        /// </summary>
        /// <param name="contactIds">Contact ids</param>
        /// <returns>Tuple collection - contact Id, contact full name</returns>
        public IEnumerable<KeyValuePair<int, string>> GetContactsWithFullName(IEnumerable<int> contactIds)
        {
            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactID", contactIds.ToList())
                                      .Select(dr => new KeyValuePair<int, string>(
                                                        ValidationHelper.GetInteger(dr["ContactID"], 0), 
                                                        String.Join(" ", 
                                                                    ValidationHelper.GetString(dr["ContactFirstName"], String.Empty), 
                                                                    ValidationHelper.GetString(dr["ContactLastName"], String.Empty))
                                                              .Trim()
                                              )
                                      );
        }
    }
}
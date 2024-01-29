using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactGroupsMembershipController"/>.
    /// </summary>
    internal class ContactGroupsMembershipControllerService : IContactGroupsMembershipService
    {
        private readonly IUILinkProvider mUILinkProvider;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactGroupsMembershipControllerService"/>.
        /// </summary>
        /// <param name="uiLinkProvider">Provides link for an object</param>
        public ContactGroupsMembershipControllerService(IUILinkProvider uiLinkProvider)
        {
            mUILinkProvider = uiLinkProvider;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/>. Returns empty list if no membership is found for given <paramref name="contactID"/>.
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactGroupsMembershipViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/>, or empty list if no membership is found</returns>
        public IEnumerable<ContactGroupsMembershipViewModel> GetContactGroupMembershipViewModel(int contactID)
        {
            var contactGroupsId = ContactGroupMemberInfoProvider.GetRelationships()
                                          .WhereEquals("ContactGroupMemberRelatedID", contactID)
                                          .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                                          .Column("ContactGroupMemberContactGroupID");

            var contactGroupsMembership = ContactGroupInfoProvider.GetContactGroups()
                                        .WhereIn("ContactGroupID", contactGroupsId)
                                        .Columns("ContactGroupID", "ContactGroupDisplayName")
                                        .OrderBy(contactGroup => contactGroup.ContactGroupDisplayName)
                                        .ToList()
                                        .Select(contactGroup => new ContactGroupsMembershipViewModel
                                        {
                                            ContactGroupDisplayName = contactGroup.ContactGroupDisplayName,
                                            ContactGroupUrl = URLHelper.GetAbsoluteUrl(mUILinkProvider.GetSingleObjectLink(ModuleName.CONTACTMANAGEMENT, "EditContactGroup", 
                                            new ObjectDetailLinkParameters
                                            {
                                                AllowNavigationToListing = true,
                                                ObjectIdentifier = contactGroup.ContactGroupID
                                            }))
                                        }); 

            return contactGroupsMembership;
        }
    }
}

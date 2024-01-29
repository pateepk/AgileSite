using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactGroupsMembershipController))]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact groups membership component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactGroupsMembershipController : CMSApiController
    {
        private readonly IContactGroupsMembershipService mContactGroupsMembershipService;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactGroupsMembershipController"/>.
        /// </summary>
        public ContactGroupsMembershipController()
            :this(Service.Resolve<IContactGroupsMembershipService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactGroupsMembershipController"/>.
        /// </summary>
        /// <param name="contactGroupsMembershipService">Provides service methods used in <see cref="ContactGroupsMembershipController"/></param>
        internal ContactGroupsMembershipController(IContactGroupsMembershipService contactGroupsMembershipService)
        {
            mContactGroupsMembershipService = contactGroupsMembershipService;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactGroupsMembershipViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactGroupsMembershipViewModel> Get(int contactID)
        {
            return mContactGroupsMembershipService.GetContactGroupMembershipViewModel(contactID);
        }
    }
}

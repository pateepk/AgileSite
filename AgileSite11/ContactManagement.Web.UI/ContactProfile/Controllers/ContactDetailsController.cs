using System.Collections.Generic;

using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactDetailsController))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact details component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactDetailsController : CMSApiController
    {
        private readonly IContactDetailsControllerService mContactDetailsControllerService;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactDetailsController"/>.
        /// </summary>
        public ContactDetailsController()
            :this(Service.Resolve<IContactDetailsControllerService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactDetailsController"/>.
        /// </summary>
        /// <param name="contactDetailsControllerService">Provides service methods used in <see cref="ContactDetailsController"/></param>
        internal ContactDetailsController(IContactDetailsControllerService contactDetailsControllerService)
        {
            mContactDetailsControllerService = contactDetailsControllerService;
        }


        /// <summary>
        /// Gets collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection of <see cref="ContactDetailsViewModel"/> is obtained for</param>
        /// <returns>Collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactDetailsViewModel> Get(int contactID)
        {
            return mContactDetailsControllerService.GetContactDetailsViewModel(contactID);
        }
    }
}

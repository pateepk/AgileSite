using System.Net;
using System.Web.Http;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactProfileController))]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact card component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactProfileController : CMSApiController
    {
        private readonly IContactProfileControllerService mContactProfileControllerService;
        

        /// <summary>
        /// Instantiates new instance of <see cref="ContactProfileController"/>.
        /// </summary>
        public ContactProfileController()
            :this(Service.Resolve<IContactProfileControllerService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactProfileController"/>.
        /// </summary>
        /// <param name="contactProfileControllerService">Provides service methods used in <see cref="ContactProfileController"/></param>
        internal ContactProfileController(IContactProfileControllerService contactProfileControllerService)
        {
            mContactProfileControllerService = contactProfileControllerService;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactProfileViewModel"/> is obtained for</param>
        /// <exception cref="HttpResponseException"><see cref="ContactInfo"/> with given <paramref name="contactID"/> not found</exception>
        /// <returns>Instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/></returns>
        public ContactProfileViewModel Get(int contactID)
        {
            var contactCardViewModel = mContactProfileControllerService.GetContactViewModel(contactID);
            if (contactCardViewModel == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            
            return contactCardViewModel;
        }
    }
}
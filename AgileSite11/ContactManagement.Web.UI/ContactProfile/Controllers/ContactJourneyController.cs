using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactJourneyController))]

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact journey component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactJourneyController : CMSApiController
    {
        private readonly IContactJourneyControllerService mContactJourneyControllerService;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactJourneyController"/>.
        /// </summary>
        public ContactJourneyController()
            :this(Service.Resolve<IContactJourneyControllerService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactJourneyController"/>.
        /// </summary>
        /// <param name="contactJourneyControllerService">Provides service methods used in <see cref="ContactJourneyController"/></param>
        internal ContactJourneyController(IContactJourneyControllerService contactJourneyControllerService)
        {
            mContactJourneyControllerService = contactJourneyControllerService;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactJourneyViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactJourneyViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactJourneyViewModel"/> for the given <paramref name="contactID"/></returns>
        public ContactJourneyViewModel Get(int contactID)
        {
            return mContactJourneyControllerService.GetContactJourneyForContact(contactID);
        }
    }
}

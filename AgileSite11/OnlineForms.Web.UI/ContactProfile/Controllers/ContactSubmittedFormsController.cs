using System.Collections.Generic;

using CMS.Core;
using CMS.OnlineForms.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactSubmittedFormsController))]

namespace CMS.OnlineForms.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact submitted forms component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactSubmittedFormsController : CMSApiController
    {
        private readonly IContactSubmittedFormsControllerService mContactSubmittedFormsControllerService;

        /// <summary>Form
        /// Instantiates new instance of <see cref="ContactSubmittedFormsController"/>.
        /// </summary>
        public ContactSubmittedFormsController()
            :this(Service.Resolve<IContactSubmittedFormsControllerService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactSubmittedFormsController"/>.
        /// </summary>
        /// <param name="contactNewsletterSubscriptionsControllerService">Provides service methods used in <see cref="ContactSubmittedFormsControllerService"/></param>
        internal ContactSubmittedFormsController(IContactSubmittedFormsControllerService contactNewsletterSubscriptionsControllerService)
        {
            mContactSubmittedFormsControllerService = contactNewsletterSubscriptionsControllerService;
        }


        /// <summary>
        /// Gets collection of <see cref="ContactSubmittedFormsViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection is obtained for</param>
        /// <returns>Collection of <see cref="ContactSubmittedFormsViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactSubmittedFormsViewModel> Get(int contactID)
        {
            return mContactSubmittedFormsControllerService.GetSubmittedForms(contactID);
        }
    }
}

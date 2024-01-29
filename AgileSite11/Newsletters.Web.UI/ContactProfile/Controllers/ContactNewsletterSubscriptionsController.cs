using System.Collections.Generic;

using CMS.Core;
using CMS.Newsletters.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactNewsletterSubscriptionsController))]

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contact newsletter subscriptions component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactNewsletterSubscriptionsController : CMSApiController
    {
        private readonly IContactNewsletterSubscriptionsControllerService mContactNewsletterSubscriptionsControllerService;
        
        /// <summary>
        /// Instantiates new instance of <see cref="ContactNewsletterSubscriptionsController"/>.
        /// </summary>
        public ContactNewsletterSubscriptionsController()
            :this(Service.Resolve<IContactNewsletterSubscriptionsControllerService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactNewsletterSubscriptionsController"/>.
        /// </summary>
        /// <param name="contactNewsletterSubscriptionsControllerService">Provides service methods used in <see cref="ContactNewsletterSubscriptionsController"/></param>
        internal ContactNewsletterSubscriptionsController(IContactNewsletterSubscriptionsControllerService contactNewsletterSubscriptionsControllerService)
        {
            mContactNewsletterSubscriptionsControllerService = contactNewsletterSubscriptionsControllerService;
        }


        /// <summary>
        /// Gets collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection is obtained for</param>
        /// <returns>Collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactNewsletterSubscriptionViewModel> Get(int contactID)
        {
            return mContactNewsletterSubscriptionsControllerService.GetContactNewsletterSubscriptionViewModels(contactID);
        }
    }
}
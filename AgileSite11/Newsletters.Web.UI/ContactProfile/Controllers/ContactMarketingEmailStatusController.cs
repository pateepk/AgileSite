using CMS.Core;
using CMS.Newsletters.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactMarketingEmailStatusController))]

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the email marketing status for component.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class ContactMarketingEmailStatusController : CMSApiController
    {
        private readonly IContactMarketingEmailStatusService mContactMarketingEmailStatusService;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactMarketingEmailStatusController"/>.
        /// </summary>
        public ContactMarketingEmailStatusController()
            :this(Service.Resolve<IContactMarketingEmailStatusService>())
        {
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ContactMarketingEmailStatusController"/>.
        /// </summary>
        /// <param name="contactNewsletterSubscriptionsControllerService">Provides service methods used in <see cref="ContactMarketingEmailStatusController"/></param>
        internal ContactMarketingEmailStatusController(IContactMarketingEmailStatusService contactNewsletterSubscriptionsControllerService)
        {
            mContactMarketingEmailStatusService = contactNewsletterSubscriptionsControllerService;
        }


        /// <summary>
        /// Gets <see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the view model is obtained for</param>
        /// <returns><see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/></returns>
        public ContactMarketingEmailStatusViewModel Get(int contactID)
        {
            return mContactMarketingEmailStatusService.GetContactMarketingEmailStatus(contactID);
        }
    }
}
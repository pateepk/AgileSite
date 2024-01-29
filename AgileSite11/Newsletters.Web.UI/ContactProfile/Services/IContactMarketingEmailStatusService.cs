using CMS;
using CMS.Newsletters.Web.UI;

[assembly: RegisterImplementation(typeof(IContactMarketingEmailStatusService), typeof(ContactMarketingEmailStatusControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactMarketingEmailStatusControllerService"/>.
    /// </summary>
    internal interface IContactMarketingEmailStatusService
    {
        /// <summary>
        /// Gets <see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact <see cref="ContactMarketingEmailStatusViewModel"/> is obtained for</param>
        /// <returns><see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/></returns>
        ContactMarketingEmailStatusViewModel GetContactMarketingEmailStatus(int contactID);
    }
}

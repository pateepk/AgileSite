using System.Collections.Generic;

using CMS;
using CMS.Newsletters.Web.UI;
using CMS.Newsletters.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactNewsletterSubscriptionsControllerService), typeof(ContactNewsletterSubscriptionsControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactNewsletterSubscriptionsController"/>.
    /// </summary>
    internal interface IContactNewsletterSubscriptionsControllerService
    {
        /// <summary>
        /// Gets collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection is obtained for</param>
        /// <returns>Collection of <see cref="ContactNewsletterSubscriptionViewModel"/> for the given <paramref name="contactID"/></returns>
        IEnumerable<ContactNewsletterSubscriptionViewModel> GetContactNewsletterSubscriptionViewModels(int contactID);
    }
}
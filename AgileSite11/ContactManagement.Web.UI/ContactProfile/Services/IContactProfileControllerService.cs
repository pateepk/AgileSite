using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactProfileControllerService), typeof(ContactProfileControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactProfileController"/>.
    /// </summary>
    internal interface IContactProfileControllerService
    {
        /// <summary>
        /// Gets instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/>. Returns <c>null</c> if no <see cref="ContactInfo"/> is found for given <paramref name="contactID"/>.
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactProfileViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/>, or <c>null</c> is no <see cref="ContactInfo"/> is found</returns>
        ContactProfileViewModel GetContactViewModel(int contactID);
    }
}
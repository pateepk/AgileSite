using System.Collections.Generic;

using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactGroupsMembershipService), typeof(ContactGroupsMembershipControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactGroupsMembershipController"/>.
    /// </summary>
    internal interface IContactGroupsMembershipService
    {
        /// <summary>
        /// Gets instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/>. Returns empty list if no membership is found for given <paramref name="contactID"/>.
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactGroupsMembershipViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactGroupsMembershipViewModel"/> for the given <paramref name="contactID"/>, or empty list if no membership is found</returns>
        IEnumerable<ContactGroupsMembershipViewModel> GetContactGroupMembershipViewModel(int contactID);
    }
}
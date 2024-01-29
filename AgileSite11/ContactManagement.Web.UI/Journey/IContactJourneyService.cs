using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactJourneyService), typeof(ContactJourneyService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides method for obtaining the view model suitable for contact journey component.
    /// </summary>
    public interface IContactJourneyService
    {
        /// <summary>
        /// Gets view model suitable for contact journey component for the given <paramref name="contactID"/>.
        /// </summary>
        ContactJourneyViewModel GetContactJourneyForContact(int contactID);
    }
}
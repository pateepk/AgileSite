using CMS;
using CMS.ContactManagement.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactJourneyControllerService), typeof(ContactJourneyControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI.Internal
{
    internal interface IContactJourneyControllerService
    {
        ContactJourneyViewModel GetContactJourneyForContact(int contactID);
    }
}
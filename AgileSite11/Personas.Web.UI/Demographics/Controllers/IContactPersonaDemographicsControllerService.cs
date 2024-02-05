using System.Collections.Generic;

using CMS;
using CMS.Personas.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactPersonaDemographicsControllerService), typeof(ContactPersonaDemographicsControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Web.UI.Internal
{
    internal interface IContactPersonaDemographicsControllerService
    {
        IEnumerable<ContactsGroupedByPersonaViewModel> GetGroupedByPersona(string retrieverIdentifier);
    }
}
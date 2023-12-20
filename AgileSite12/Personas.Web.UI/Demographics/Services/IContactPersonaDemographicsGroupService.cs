using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Personas.Web.UI;
using CMS.Personas.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IContactPersonaDemographicsGroupService), typeof(ContactPersonaDemographicsGroupService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Web.UI
{
    internal interface IContactPersonaDemographicsGroupService
    {
        IEnumerable<ContactsGroupedByPersonaViewModel> GroupContactsByPersona(ObjectQuery<ContactInfo> contacts);
    }
}

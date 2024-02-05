using System.Collections.Generic;

using CMS;
using CMS.Personas.Web.UI;
using CMS.Personas.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IPersonaContactHistoryControllerService), typeof(PersonaContactHistoryControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Web.UI
{
    internal interface IPersonaContactHistoryControllerService
    {
        IEnumerable<PersonaContactHistoryViewModel> GetPersonaContactHistoryData();
    }
}
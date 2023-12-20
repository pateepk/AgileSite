using System.Collections.Generic;

using CMS;
using CMS.Personas.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IPersonaReportExtenderService), typeof(PersonaReportExtenderService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Web.UI.Internal
{
    internal interface IPersonaReportExtenderService
    {
        IEnumerable<PersonaReportConfigurationViewModel> GetPersonaConfiguration();
    }
}
using System.Collections.Generic;

using CMS.Core;
using CMS.Personas.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(PersonaContactHistoryController))]

namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the persona/contact distribution over time chart.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    public sealed class PersonaContactHistoryController : CMSApiController
    {
        private readonly IPersonaContactHistoryControllerService mPersonaContactHistoryControllerService;


        /// <summary>
        /// Instantiates new instance of <see cref="PersonaContactHistoryController"/>.
        /// </summary>
        public PersonaContactHistoryController() : this(Service.Resolve<IPersonaContactHistoryControllerService>())
        {
        }


        internal PersonaContactHistoryController(IPersonaContactHistoryControllerService personaContactHistoryControllerService)
        {
            mPersonaContactHistoryControllerService = personaContactHistoryControllerService;
        }


        /// <summary>
        /// Get collection containing the persona/contact distribution over time view model.
        /// </summary>
        public IEnumerable<PersonaContactHistoryViewModel> Get()
        {
            return mPersonaContactHistoryControllerService.GetPersonaContactHistoryData();
        }
    }
}
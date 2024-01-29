using CMS.Core;
using CMS.Personas.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactPersonaController))]

namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the data required for the contacts persona.
    /// </summary>
    [AllowOnlyEditor]
    public sealed class ContactPersonaController : CMSApiController
    {
        private readonly IPersonaViewModelService mPersonaViewModelService;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactPersonaController"/>.
        /// </summary>
        public ContactPersonaController()
            :this(Service.Resolve<IPersonaViewModelService>())
        {
            
        }


        internal ContactPersonaController(IPersonaViewModelService personaViewModelService)
        {
            mPersonaViewModelService = personaViewModelService;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactPersonaViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactPersonaViewModel"/> is obtained for.</param>
        /// <returns>Instance of <see cref="ContactPersonaViewModel"/> for the given <paramref name="contactID"/>.</returns>
        public ContactPersonaViewModel Get(int contactID)
        {
            return mPersonaViewModelService.GetPersonaViewModel(contactID);
        }
    }
}
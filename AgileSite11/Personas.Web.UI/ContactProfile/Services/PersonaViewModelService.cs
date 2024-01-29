using CMS.ContactManagement;
using CMS.Core;

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Provides contacts persona view model.
    /// </summary>
    internal class PersonaViewModelService : IPersonaViewModelService
    {
        internal const string NOT_PROFILLED_PERSONA_NAME_RESOURCE_KEY = "persona.notprofilledpersona.name";
        internal const string NOT_PROFILLED_PERSONA_DESCRIPTION_RESOURCE_KEY = "persona.notprofilledpersona.description";

        // default size is multiplied by 2 because of the HDPI
        private const int PERSONA_IMAGE_SIZE = 96 * 2;
        private readonly IPersonaService mPersonaService;
        private readonly IPersonaPictureUrlCreator mPersonaPictureUrlCreator;
        private readonly ILocalizationService mLocalizationService;


        /// <summary>
        /// Initializes new instance of <see cref="PersonaViewModelService"/>.
        /// </summary>
        /// <param name="personaService">Service used to obtain persona data.</param>
        /// <param name="personaPictureUrlCreator">Service used to create persona image URL.</param>
        /// <param name="localizationService">Service used to localize resource strings.</param>
        /// 
        public PersonaViewModelService(IPersonaService personaService, IPersonaPictureUrlCreator personaPictureUrlCreator, ILocalizationService localizationService)
        {
            mPersonaService = personaService;
            mPersonaPictureUrlCreator = personaPictureUrlCreator;
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Provides contacts persona view model.
        /// </summary>
        /// <param name="contactId">Contact ID.</param>
        /// <returns>View model of given contact persona.</returns>
        public ContactPersonaViewModel GetPersonaViewModel(int contactId)
        {
            var contact = ContactInfoProvider.GetContactInfo(contactId);
            if (contact != null)
            {
                var personaInfo = mPersonaService.GetPersonaForContact(contact);
                if (personaInfo != null)
                {
                    return CreatePersonaViewModel(personaInfo);
                }
            }
            return CreateNotProfilledPersona();
        }


        private ContactPersonaViewModel CreatePersonaViewModel(PersonaInfo personaInfo)
        {
            return new ContactPersonaViewModel()
            {
                Name = personaInfo.PersonaDisplayName,
                Description = personaInfo.PersonaDescription,
                ImageUrl = mPersonaPictureUrlCreator.CreatePersonaPictureUrl(personaInfo, PERSONA_IMAGE_SIZE)
            };
        }


        private ContactPersonaViewModel CreateNotProfilledPersona()
        {
            return new ContactPersonaViewModel()
            {
                Name = mLocalizationService.GetString(NOT_PROFILLED_PERSONA_NAME_RESOURCE_KEY),
                Description = mLocalizationService.GetString(NOT_PROFILLED_PERSONA_DESCRIPTION_RESOURCE_KEY),
                ImageUrl = mPersonaPictureUrlCreator.CreateDefaultPersonaPictureUrl(PERSONA_IMAGE_SIZE)
            };
        }
    }
}
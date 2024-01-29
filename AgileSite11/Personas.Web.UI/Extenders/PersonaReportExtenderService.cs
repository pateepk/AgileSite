using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Personas.Web.UI.Internal
{
    internal class PersonaReportExtenderService : IPersonaReportExtenderService
    {
        internal const int PERSONA_IMAGE_SIZE = 64;
        internal const string WITOUT_PERSONA_LABEL = "personas.personareport.withoutpersonalabel";

        private readonly IPersonaPictureImgTagGenerator mPersonaPictureImgTagGenerator;
        private readonly ILocalizationService mLocalizationService;


        public PersonaReportExtenderService(IPersonaPictureImgTagGenerator personaPictureImgTagGenerator, ILocalizationService localizationService)
        {
            mPersonaPictureImgTagGenerator = personaPictureImgTagGenerator;
            mLocalizationService = localizationService;
        }


        public IEnumerable<PersonaReportConfigurationViewModel> GetPersonaConfiguration()
        {
            var personas = PersonaInfoProvider.GetPersonas().Select(GetPersonaViewModel).ToList();
            personas.Add(GetWithoutPersonaViewModel());

            return personas;
        }


        private PersonaReportConfigurationViewModel GetPersonaViewModel(PersonaInfo persona)
        {
            return new PersonaReportConfigurationViewModel
            {
                PersonaID = persona.PersonaID,
                PersonaImage = mPersonaPictureImgTagGenerator.GenerateImgTag(persona, PERSONA_IMAGE_SIZE),
                PersonaName = persona.PersonaDisplayName
            };
        }


        private PersonaReportConfigurationViewModel GetWithoutPersonaViewModel()
        {
            return new PersonaReportConfigurationViewModel
            {
                PersonaID = null,
                PersonaName = mLocalizationService.GetString(WITOUT_PERSONA_LABEL),
                PersonaImage = null
            };
        }
    }
}
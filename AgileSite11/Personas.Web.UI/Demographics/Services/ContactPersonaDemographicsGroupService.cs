using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Personas.Internal;
using CMS.Personas.Web.UI.Internal;

namespace CMS.Personas.Web.UI
{
    internal class ContactPersonaDemographicsGroupService : IContactPersonaDemographicsGroupService
    {
        private readonly IPersonaContactCounter mPersonaContactCounter;
        private readonly ILocalizationService mLocalizationService;
        private const string NULL_PERSONA_NAME = "contactdemographics.personagraph.nullpersona";


        public ContactPersonaDemographicsGroupService(IPersonaContactCounter personaContactCounter, ILocalizationService localizationService)
        {
            mPersonaContactCounter = personaContactCounter;
            mLocalizationService = localizationService;
        }


        public IEnumerable<ContactsGroupedByPersonaViewModel> GroupContactsByPersona(ObjectQuery<ContactInfo> contacts)
        {
            var personaContactCounts = mPersonaContactCounter.GetContactCountForPersonas(contacts)
                                                            .ToList();

            var personaIds = personaContactCounts.Where(p => p.PersonaID.HasValue).Select(p => p.PersonaID.Value).ToList();
            var personaDisplayNames = PersonaInfoProvider.GetPersonas()
                                                         .WhereIn("PersonaID", personaIds)
                                                         .ToDictionary(p => p.PersonaID, p => p.PersonaDisplayName);

            return personaContactCounts.Select(personaContactCount => MapToContactsGroupedByPersonaViewModel(personaDisplayNames, personaContactCount));
        }


        private string GetPersonaName(Dictionary<int, string> personaDisplayNames, int? personaId)
        {
            return personaId.HasValue ? personaDisplayNames[personaId.Value] : mLocalizationService.GetString(NULL_PERSONA_NAME);
        }


        private ContactsGroupedByPersonaViewModel MapToContactsGroupedByPersonaViewModel(Dictionary<int, string> personaDisplayNames, ContactCountForPersona contactCountForPersona)
        {
            string personaName = GetPersonaName(personaDisplayNames, contactCountForPersona.PersonaID);
            return new ContactsGroupedByPersonaViewModel()
            {
                PersonaName = personaName,
                NumberOfContacts = contactCountForPersona.ContactsCount
            };
        }
    }
}

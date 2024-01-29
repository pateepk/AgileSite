using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.Core.Internal;
using CMS.Personas.Internal;

namespace CMS.Personas
{
    internal class PersonaSnapshooter : IPersonaSnapshooter
    {
        private readonly IDateTimeNowService mDateTimeNowService;
        private readonly IPersonaContactCounter mPersonaContactCounter;


        public PersonaSnapshooter(IDateTimeNowService dateTimeNowService, IPersonaContactCounter personaContactCounter)
        {
            mDateTimeNowService = dateTimeNowService;
            mPersonaContactCounter = personaContactCounter;
        }


        public IEnumerable<PersonaContactHistoryInfo> GetSnapshotOfCurrentState()
        {
            var contactCountForPersonas = mPersonaContactCounter.GetContactCountForPersonas(ContactInfoProvider.GetContacts());

            var currentDate = mDateTimeNowService.GetDateTimeNow().Date;

            return contactCountForPersonas.Select(persona => GetSnapshot(persona.PersonaID, persona.ContactsCount, currentDate));
        }


        private PersonaContactHistoryInfo GetSnapshot(int? personaID, int totalContacts, DateTime currentDate)
        {
            return new PersonaContactHistoryInfo
            {
                PersonaContactHistoryPersonaID = personaID,
                PersonaContactHistoryContacts = totalContacts,
                PersonaContactHistoryDate = currentDate
            };
        }
    }
}

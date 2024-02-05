using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Personas.Internal;

namespace CMS.Personas
{
    internal class PersonaContactCounter : IPersonaContactCounter
    {
        public IEnumerable<ContactCountForPersona> GetContactCountForPersonas(ObjectQuery<ContactInfo> contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            int allContactCount = contacts.Count;

            var contactCountForPersonas = contacts.WhereNotNull("ContactPersonaID")
                                                  .Columns("ContactPersonaID")
                                                  .AddColumn(new CountColumn().As("ContactCount"))
                                                  .GroupBy("ContactPersonaID")
                                                  .Select(GetContactCountForPersona)
                                                  .ToList();

            contactCountForPersonas.AddRange(GetContactCountForPersonasWithoutContacts(contactCountForPersonas));

            int contactsCountWithoutPersona = allContactCount - contactCountForPersonas.Sum(persona => persona.ContactsCount);
            contactCountForPersonas.Add(GetContactCountForPersona(null, contactsCountWithoutPersona));

            return contactCountForPersonas;
        }


        private IEnumerable<ContactCountForPersona> GetContactCountForPersonasWithoutContacts(IList<ContactCountForPersona> contactCountForPersonas)
        {
            var idsOfAlreadyUsedPersonas = contactCountForPersonas.Where(persona => persona.PersonaID.HasValue)
                                                                  .Select(persona => persona.PersonaID.Value)
                                                                  .ToList();

            return PersonaInfoProvider.GetPersonas()
                                      .WhereNotIn("PersonaID", idsOfAlreadyUsedPersonas)
                                      .Column("PersonaID")
                                      .GetListResult<int>()
                                      .Select(personaId => GetContactCountForPersona(personaId, 0));
        }


        private ContactCountForPersona GetContactCountForPersona(DataRow dataRow)
        {
            return GetContactCountForPersona(dataRow["ContactPersonaID"].ToInteger(0), dataRow["ContactCount"].ToInteger(0));
        }


        private ContactCountForPersona GetContactCountForPersona(int? personaId, int contactCount)
        {
            return new ContactCountForPersona
            {
                PersonaID = personaId,
                ContactsCount = contactCount
            };
        }
    }
}

using System;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Personas
{
    /// <summary>
    /// Provides methods to determine membership of contacts in personas.
    /// </summary>
    /// <remarks>
    /// This is the default implementation of the <see cref="IPersonaService"/> interface. Please obtain its instance via <see cref="PersonasFactory"/>.
    /// Contact can be in one persona only - contact belongs to persona if score quotient (contact score points divided by persona score limit) is the highest one of all personas.
    /// </remarks>
    public sealed class PersonaService : IPersonaService
    {
        /// <summary>
        /// Returns true when contact belongs to specified persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <param name="persona">Persona</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="persona"/> is null</exception>
        /// <returns>True when contact fulfills persona definition</returns>
        public bool IsContactInPersona(ContactInfo contact, PersonaInfo persona)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            if (!persona.PersonaEnabled)
            {
                return false;
            }

            return contact.ContactPersonaID == persona.PersonaID;
        }


        /// <summary>
        /// Gets persona the specified contact is assigned to or null of contact does not belong to any persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is null</exception>
        /// <returns>Persona that specified contact belongs to or null</returns>
        public PersonaInfo GetPersonaForContact(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (!contact.ContactPersonaID.HasValue)
            {
                return null;
            }

            return PersonaInfoProvider.GetPersonaInfoById(contact.ContactPersonaID.Value);
        }


        /// <summary>
        /// Gets all contacts that fulfills persona definition.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="persona">Persona</param>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        /// <returns>All contacts that fulfills persona definition</returns>
        public ObjectQuery<ContactInfo> GetContactsForPersona(PersonaInfo persona)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            var contacts = ContactInfoProvider.GetContacts();

            return persona.PersonaEnabled ? contacts.WhereEquals("ContactPersonaID", persona.PersonaID) : contacts.NoResults();
        }
    }
}
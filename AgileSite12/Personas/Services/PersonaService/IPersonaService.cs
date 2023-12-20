using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IPersonaService), typeof(PersonaService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{
    /// <summary>
    /// Provides methods to determine membership of contacts in personas.
    /// </summary>
    /// <remarks>
    /// Use <see cref="PersonasFactory"/> to obtain implementation of this interface.
    /// Contact can be in one persona only - contact belongs to persona if score quotient (contact score points divided by persona score limit) is the highest one of all personas.
    /// </remarks>
    public interface IPersonaService
    {
        /// <summary>
        /// Returns true when contact belongs to specified persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <param name="persona">Persona</param>
        /// <returns>True when contact fulfills persona definition</returns>
        bool IsContactInPersona(ContactInfo contact, PersonaInfo persona);


        /// <summary>
        /// Gets persona the specified contact is assigned to or null of contact does not belong to any persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <returns>Persona that specified contact belongs to or null</returns>
        PersonaInfo GetPersonaForContact(ContactInfo contact);


        /// <summary>
        /// Gets all contacts that fulfills persona definition.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="persona">Persona</param>
        /// <returns>All contacts that fulfills persona definition</returns>
        ObjectQuery<ContactInfo> GetContactsForPersona(PersonaInfo persona);
    }
}

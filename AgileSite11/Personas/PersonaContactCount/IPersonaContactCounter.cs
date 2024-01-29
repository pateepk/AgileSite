using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Personas;
using CMS.Personas.Internal;

[assembly: RegisterImplementation(typeof(IPersonaContactCounter), typeof(PersonaContactCounter), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Internal
{
    /// <summary>
    /// Provides method for obtaining current distribution of contacts in persona. 
    /// I.e. how many contacts belong to which persona and how many don't belong to any persona.
    /// </summary>
    public interface IPersonaContactCounter
    {
        /// <summary>
        /// Gets collection of <see cref="ContactCountForPersona"/> containing the distribution of contacts based on personas. 
        /// I.e. how many contacts belong to which persona and how many don't belong to any persona.
        /// </summary>
        /// <param name="contacts">Set of contacts for which is the persona-based distribution calculated.</param>
        /// <returns>Collection of <see cref="ContactCountForPersona"/> containing the distribution of contacts based on personas.</returns>
        IEnumerable<ContactCountForPersona> GetContactCountForPersonas(ObjectQuery<ContactInfo> contacts);
    }
}

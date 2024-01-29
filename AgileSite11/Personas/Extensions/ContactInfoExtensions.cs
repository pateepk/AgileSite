using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.ContactManagement;

namespace CMS.Personas
{
    /// <summary>
    /// Provides extension methods working with personas for <see cref="ContactInfo"/>.
    /// </summary>
    public static class ContactInfoExtensions
    {
        /// <summary>
        /// Returns <see cref="PersonaInfo"/> of the contact.
        /// </summary>
        /// <param name="contact">Contact of which we want to get persona.</param>
        /// <returns>Persona of current contact.</returns>
        public static PersonaInfo GetPersona(this ContactInfo contact)
        {
            if (contact == null || !contact.ContactPersonaID.HasValue)
            {
                return null;
            }

            return PersonaInfoProvider.GetPersonaInfoById(contact.ContactPersonaID.Value);
        }
    }
}

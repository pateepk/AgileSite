using System;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Personas
{
    /// <summary>
    /// Provides methods to determine membership of contacts in personas. Takes into account persona which is enforced in the page preview mode when user wants 
    /// to see how page looks like for certain persona. This persona is taken into account only if caller is asking for personas for current contact.
    /// </summary>
    /// <remarks>
    /// This is the implementation of the <see cref="IPersonaService"/> interface which is used in the page preview mode. Please obtain its instance via <see cref="PersonasFactory"/>.
    /// </remarks>
    internal class PagePreviewPersonaService : IPersonaService
    {
        #region "Fields"

        private readonly IPersonaService mOriginalPersonaService;
        private readonly ContactInfo mCurrentContact;
        private readonly IPreviewPersonaStorage mPreviewPersonaStorage;

        #endregion


        #region "Properties"

        /// <summary>
        /// Persona enforced by the user. If null, user wants to see page as someone who is not assigned to any persona.
        /// </summary>
        private PersonaInfo EnforcedPersona
        {
            get
            {
                return mPreviewPersonaStorage.GetPreviewPersona();
            }
        }


        /// <summary>
        /// If true, user wants to see page as someone who is not assigned to any persona.
        /// </summary>
        private bool ActAsContactWithoutPersona
        {
            get
            {
                return EnforcedPersona != null;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalPersonaService">Service which defines relationships between personas and contacts by default</param>
        /// <param name="currentContact">Current contact</param>
        /// <param name="previewPersonaStorage">Storage for saving and retrieving personas which should be displayed in the preview mode</param>
        /// <exception cref="ArgumentNullException"><paramref name="originalPersonaService"/>, <paramref name="currentContact"/> or <paramref name="previewPersonaStorage"/> is null</exception>
        public PagePreviewPersonaService(IPersonaService originalPersonaService, ContactInfo currentContact, IPreviewPersonaStorage previewPersonaStorage)
        {
            if (originalPersonaService == null)
            {
                throw new ArgumentNullException("originalPersonaService");
            }
            if (currentContact == null)
            {
                throw new ArgumentNullException("currentContact");
            }
            if (previewPersonaStorage == null)
            {
                throw new ArgumentNullException("previewPersonaStorage");
            }

            mOriginalPersonaService = originalPersonaService;
            mCurrentContact = currentContact;
            mPreviewPersonaStorage = previewPersonaStorage;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true when contact belongs to specified persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <param name="persona">Persona</param>
        /// <returns>True when contact fulfills persona definition</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="persona"/> is null</exception>
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

            if (contact.ContactID == mCurrentContact.ContactID)
            {
                return ActAsContactWithoutPersona && (EnforcedPersona.PersonaID == persona.PersonaID);
            }

            return mOriginalPersonaService.IsContactInPersona(contact, persona);
        }


        /// <summary>
        /// Gets persona the specified contact is assigned to or null of contact does not belong to any persona.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="contact">Contact</param>
        /// <returns>Persona that specified contact belongs to or null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is null</exception>
        public PersonaInfo GetPersonaForContact(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (contact.ContactID == mCurrentContact.ContactID)
            {
                return ActAsContactWithoutPersona ? EnforcedPersona : null;
            }

            return mOriginalPersonaService.GetPersonaForContact(contact);
        }


        /// <summary>
        /// Gets all contacts that fulfills persona definition.
        /// </summary>
        /// <remarks>
        /// The contact can be only in one persona at the same time. Belonging to persona is determined by the highest quotient of total score the contact obtained divided by persona score limit.
        /// </remarks>
        /// <param name="persona">Persona</param>
        /// <returns>All contacts that fulfills persona definition</returns>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        public ObjectQuery<ContactInfo> GetContactsForPersona(PersonaInfo persona)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            var allContacts = mOriginalPersonaService.GetContactsForPersona(persona);
            if (ActAsContactWithoutPersona && (EnforcedPersona.PersonaID == persona.PersonaID))
            {
                return allContacts.Or(new WhereCondition().WhereEquals("ContactID", mCurrentContact.ContactID));
            }

            return allContacts;
        }

        #endregion
    }
}
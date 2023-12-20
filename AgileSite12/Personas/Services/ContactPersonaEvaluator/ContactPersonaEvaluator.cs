using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.ContactManagement;

namespace CMS.Personas
{
    /// <summary>
    /// Contains methods for reevaluation of contact's persona. 
    /// </summary>
    /// <remarks>
    /// Contact can be in one persona only - contact belongs to persona if score quotient (contact score points divided by persona score limit) is the highest one of all personas.
    /// </remarks>
    public sealed class ContactPersonaEvaluator : IContactPersonaEvaluator
    {
        #region "Public methods"

        /// <summary>
        /// Reevaluates all contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when persona properties were changed, or rules were recalculated.
        /// </remarks>
        public void ReevaluateAllContacts()
        {
            PersonaInfoProvider.ReevaluateAllContacts();
        }


        /// <summary>
        /// Reevaluates all contacts and assigns them to proper persona asynchronously.
        /// </summary>
        /// <remarks>
        /// This method should be used when persona properties were changed, or rules were recalculated.
        /// </remarks>
        /// <returns>Asynchronous execution task</returns>
        public Task ReevaluateAllContactsAsync()
        {
            return StartNewLongRunningTask(() => ReevaluateAllContacts());
        }


        /// <summary>
        /// Reevaluates given contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contactsIds">Contacts which should be assigned to persona</param>
        public void ReevaluateContacts(IEnumerable<int> contactsIds)
        {
            PersonaInfoProvider.ReevaluateContacts(contactsIds);
        }


        /// <summary>
        /// Reevaluates given contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contactsIds">Contacts which should be assigned to persona</param>
        /// <returns>Asynchronous execution task</returns>
        public Task ReevaluateContactsAsync(IEnumerable<int> contactsIds)
        {
            return StartNewLongRunningTask(() => ReevaluateContacts(contactsIds));
        }


        /// <summary>
        /// Reevaluates given contact and finds proper persona the contact should be assigned to.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contact">Contact which should be assigned to persona</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is null</exception>
        public void ReevaluateContact(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            int? personaId = PersonaInfoProvider.ReevaluateContact(contact);
            if (contact.ContactPersonaID != personaId)
            {
                contact.ContactPersonaID = personaId;
                ContactInfoProvider.SetContactInfo(contact);
            }
        }


        /// <summary>
        /// Reevaluates given contact and finds proper persona the contact should be assigned to asynchronously.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contact">Contact which should be assigned to persona</param>
        /// <returns>Asynchronous execution task</returns>
        public Task ReevaluateContactAsync(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            return StartNewLongRunningTask(() => ReevaluateContact(contact));
        }

        #endregion


        /// <summary>
        /// Copies context into the action and creates new Task which executes it.
        /// </summary>
        /// <param name="action">Action to execute asynchronously</param>
        /// <returns>Newly created task</returns>
        private Task StartNewLongRunningTask(Action action)
        {
            return Task.Factory.StartNew(CMSThread.Wrap(action), TaskCreationOptions.LongRunning);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CMS;
using CMS.ContactManagement;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IContactPersonaEvaluator), typeof(ContactPersonaEvaluator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{
    /// <summary>
    /// Contains methods for reevaluation of contact's persona.
    /// </summary>
    /// <remarks>
    /// Use <see cref="PersonasFactory"/> to obtain implementation of this interface.
    /// Contact can be in one persona only - contact belongs to persona if score quotient (contact score points divided by persona score limit) is the highest one of all personas.
    /// </remarks>
    public interface IContactPersonaEvaluator
    {
        /// <summary>
        /// Reevaluates all contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when persona properties were changed, or rules were recalculated.
        /// </remarks>
        void ReevaluateAllContacts();


        /// <summary>
        /// Reevaluates all contacts and assigns them to proper persona asynchronously.
        /// </summary>
        /// <remarks>
        /// This method should be used when persona properties were changed, or rules were recalculated.
        /// </remarks>
        /// <returns>Asynchronous execution task</returns>
        Task ReevaluateAllContactsAsync();


        /// <summary>
        /// Reevaluates given contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contactsIds">Contacts which should be assigned to persona</param>
        void ReevaluateContacts(IEnumerable<int> contactsIds);


        /// <summary>
        /// Reevaluates given contacts and assigns them to proper persona.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contactsIds">Contacts which should be assigned to persona</param>
        /// <returns>Asynchronous execution task</returns>
        Task ReevaluateContactsAsync(IEnumerable<int> contactsIds);

        
        /// <summary>
        /// Reevaluates given contact and finds proper persona the contact should be assigned to.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contact">Contact which should be assigned to persona</param>
        void ReevaluateContact(ContactInfo contact);


        /// <summary>
        /// Reevaluates given contact and finds proper persona the contact should be assigned to asynchronously.
        /// </summary>
        /// <remarks>
        /// This method should be used when new activity is logged for contact or its attribute was changed. These 
        /// actions can change contact points and thus it is possible the contact can be assigned to more correct persona.
        /// </remarks>
        /// <param name="contact">Contact which should be assigned to persona</param>
        /// <returns>Asynchronous execution task</returns>
        Task ReevaluateContactAsync(ContactInfo contact);
    }
}
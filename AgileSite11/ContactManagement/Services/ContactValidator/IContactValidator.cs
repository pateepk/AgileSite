using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactValidator), typeof(DefaultContactValidator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for validating contact against the database.
    /// </summary>
    public interface IContactValidator
    {
        /// <summary>
        /// Checks whether contact passed in <paramref name="contact"/> is valid and can be used as a current contact.
        /// Updates contact from hashtables/DB. If contact is merged, returns parent contact if available.
        /// </summary>
        /// <param name="contact">Contact to be validated</param>
        /// <returns>Returns updated and parent contact or null if passed contact is not valid</returns>
        ContactInfo ValidateContact(ContactInfo contact);
    }
}
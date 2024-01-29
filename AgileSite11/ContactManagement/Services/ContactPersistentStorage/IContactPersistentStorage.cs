using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactPersistentStorage), typeof(DefaultContactPersistentStorage), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for storing and retrieving contact from/to persistent storage.
    /// Persistent storage is a place where contact can be stored and after the same contact
    /// makes another request, it will be returned.
    /// </summary>
    public interface IContactPersistentStorage
    {
        /// <summary>
        /// Gets contact from the persistent storage.
        /// </summary>
        /// <example>
        /// <para>Following example shows how to use GetPersistentContact method</para>
        /// <code>
        /// ...
        /// IContactPersistentStorage contactPersistentStorage = someImplementation;
        /// // Returns contact from the persistent storage
        /// contactPersistentStorage.GetPersistentContact();
        /// ...
        /// </code>
        /// </example>
        /// <remarks>
        /// In default implementation, cookie will be used as persistent storage for the contacts.
        /// </remarks>
        /// <returns>Contact retrieved from the persistent storage</returns>
        ContactInfo GetPersistentContact();


        /// <summary>
        /// Sets given <paramref name="contact"/> to the persistent storage.
        /// </summary>
        /// <example>
        /// <para>Following example shows how to use SetPersistentContact method</para>
        /// <code>
        /// ...
        /// IContactPersistentStorage contactPersistentStorage = someImplementation;
        /// // Stores given 'contact'  to the persistent storage
        /// contactPersistentStorage.SetPersistentContact(contact);
        /// ...
        /// 
        /// ...
        /// // In another request
        /// 
        /// // Returns the same contact that was set earlier with SetPersistentContact method
        /// contactPersistentStorage.GetPersistentContact();
        /// </code>
        /// </example>
        /// <remarks>
        /// In default implementation, cookie will be used as persistent storage for the contacts.
        /// </remarks>
        /// <param name="contact">Contact to be set</param>
        void SetPersistentContact(ContactInfo contact);
    }
}
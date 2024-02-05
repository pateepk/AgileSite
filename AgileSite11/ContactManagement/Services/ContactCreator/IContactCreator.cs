using System;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactCreator), typeof(DefaultContactCreator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for creating new contacts.
    /// </summary>
    public interface IContactCreator
    {
        /// <summary>
        /// Creates new anonymous contact. Created instance of <see cref="ContactInfo"/> is saved to the database.
        /// <see cref="ContactInfo.ContactLastName"/> starts with prefix <see cref="ContactHelper.ANONYMOUS"/>.
        /// </summary>
        /// <example>
        /// <para>Following example shows how to use the method <see cref="CreateAnonymousContact"/></para>
        /// <code>
        /// ...
        /// IContactCreator contactCreator = someImplementation;
        /// 
        /// // Assume it is now midnight on 2016/01/01
        /// 
        /// // Will create new contact with ContactLastName 'Anonymous - 2016-01-01 00:00:00.000'
        /// var contactWithoutPrefix = contactCreator.CreateAnonymousContact();
        /// ...
        /// </code>
        /// </example>
        /// <returns>Created contact</returns>
        ContactInfo CreateAnonymousContact();
        

        /// <summary>
        /// Creates and returns new contact with given <paramref name="namePrefix"/> in <see cref="ContactInfo.ContactLastName"/>.
        /// Created instance of <see cref="ContactInfo"/> is saved to the database.
        /// </summary>
        /// <remarks>
        /// Current date time will be used as default value of <see cref="ContactInfo.ContactLastName"/>.
        /// </remarks>
        /// <param name="namePrefix">Prefix that will be prepend to the created <see cref="ContactInfo.ContactLastName"/>. If null is passed, no prefix will be used</param>
        /// <example>
        /// <para>Following example shows how to use the method <see cref="CreateContact"/></para>
        /// <code>
        /// ...
        /// IContactCreator contactCreator = someImplementation;
        /// 
        /// // Assume it is now midnight on 2016/01/01
        /// 
        /// // Will create new contact with ContactLastName '2016-01-01 00:00:00.000'
        /// var contactWithoutPrefix = contactCreator.CreateContact();
        /// 
        /// // Will create new contact with ContactLastName 'prefix_2016-01-01 00:00:00.000'
        /// var contactWithoutPrefix = contactCreator.CreateContact("prefix");
        /// ...
        /// </code>
        /// </example>
        /// <returns>Created contact</returns>
        ContactInfo CreateContact(string namePrefix);
    }
}
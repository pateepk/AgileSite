using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ISubscriberFullNameFormater), typeof(SubscriberFullNameFormater), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class builds full name of subscribers of all existing types.
    /// </summary>
    public class SubscriberFullNameFormater : ISubscriberFullNameFormater
    {
        /// <summary>
        /// Returns name for user subscriber.
        /// </summary>
        /// <param name="fullName">Full name of user subscriber</param>
        /// <returns>Name of user subscriber</returns>
        public string GetUserSubscriberName(string fullName)
        {
            return string.Concat("User '", fullName, "'");
        }


        /// <summary>
        /// Returns name for role subscriber.
        /// </summary>
        /// <param name="roleName">Name of role</param>
        /// <returns>Name of role subscriber</returns>
        public string GetRoleSubscriberName(string roleName)
        {
            return string.Concat("Role '", roleName, "'");
        }


        /// <summary>
        /// Returns name of contact subscriber. Adds spaces where necessary.
        /// </summary>
        /// <param name="firstName">First name of contact</param>
        /// <param name="middleName">Middle name of contact</param>
        /// <param name="lastName">Last name of contact</param>
        /// <returns>Name of contact subscriber</returns>
        public string GetContactSubscriberName(string firstName, string middleName, string lastName)
        {
            return string.Format("Contact '{0}{1}{2}'", string.IsNullOrEmpty(firstName) ? string.Empty : firstName + " ", string.IsNullOrEmpty(middleName) ? string.Empty : middleName + " ", lastName);
        }


        /// <summary>
        /// Returns name for contact group subscriber.
        /// </summary>
        /// <param name="contactGroupName">Name of contact group</param>
        /// <returns>Name of contact group subscriber</returns>
        public string GetContactGroupSubscriberName(string contactGroupName)
        {
            return string.Concat("Contact group '", contactGroupName, "'");
        }


        /// <summary>
        /// Returns name for persona subscriber.
        /// </summary>
        /// <param name="personaName">Name of persona</param>
        /// <returns>Name of persona subscriber</returns>
        public string GetPersonaSubscriberName(string personaName)
        {
            return string.Concat("Persona '", personaName, "'");
        }
    }
}
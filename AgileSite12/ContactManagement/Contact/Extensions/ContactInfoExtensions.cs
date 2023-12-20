using System;
using System.Linq;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides extension methods for <see cref="ContactInfo"/>.
    /// </summary>
    public static class ContactInfoExtensions
    {
        /// <summary>
        /// Indicates whether contact is present in contact group on contact's site.
        /// </summary>
        /// <param name="contact">Contact whose presence in contact group is checked.</param>
        /// <param name="contactGroupName">Name of contact group.</param>
        /// <returns>True if contact is present in the specified group. Otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException ">Thrown when <paramref name="contact"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="contactGroupName"/> is null or empty string.</exception>
        public static bool IsInContactGroup(this ContactInfo contact, string contactGroupName)
        {
            if (String.IsNullOrEmpty(contactGroupName))
            {
                throw new ArgumentException("contactGroupName");
            }

            return IsInContactGroupsInternal(contact, false, contactGroupName);
        }


        /// <summary>
        /// Indicates whether contact is present in any of the specified contact groups on contact's site.
        /// </summary>
        /// <param name="contact">Contact whose presence in contact groups is checked.</param>
        /// <param name="contactGroupNames">Names of contact groups.</param>
        /// <returns>True if contact is present in any of the specified groups. Otherwise returns false.</returns>
        /// <remarks>
        /// The method returns false when no contact group name is provided.
        /// </remarks>
        /// <exception cref="ArgumentNullException ">Thrown when <paramref name="contact"/> is null.</exception>
        public static bool IsInAnyContactGroup(this ContactInfo contact, params string[] contactGroupNames)
        {
            return IsInContactGroupsInternal(contact, false, contactGroupNames);
        }


        /// <summary>
        /// Indicates whether contact is present in all of the specified contact groups on contact's site. 
        /// </summary>
        /// <param name="contact">Contact whose presence in contact groups is checked.</param>
        /// <param name="contactGroupNames">Names of contact groups.</param>
        /// <returns>True if contact is present in all of the specified groups. Otherwise returns false.</returns>
        /// <remarks>
        /// The method returns false when no contact group name is provided.
        /// </remarks>
        /// <exception cref="ArgumentNullException ">Thrown when <paramref name="contact"/> is null.</exception>
        public static bool IsInAllContactGroups(this ContactInfo contact, params string[] contactGroupNames)
        {
            return IsInContactGroupsInternal(contact, true, contactGroupNames);
        }


        /// <summary>
        /// Indicates whether contact is present in any of the specified contact groups on contact's site. When <paramref name="allGroups"/> is set to true,
        /// contact must be present in all of the specified contact groups on contact's site.
        /// </summary>
        /// <param name="contact">Contact whose presence in contact groups is checked.</param>
        /// <param name="allGroups">Flag indicating whether contact's presence is to be checked for any or all of the specified contact groups.</param>
        /// <param name="contactGroupNames">Names of contact groups.</param>
        /// <returns>
        /// True if contact is present in any of the specified groups while <paramref name="allGroups"/> is false or contact is present in all of the specified groups while <paramref name="allGroups"/> is true.
        /// Otherwise returns false.
        /// </returns>
        /// <remarks>
        /// The method returns false when no contact group name is provided.
        /// </remarks>
        /// <exception cref="ArgumentNullException ">Thrown when <paramref name="contact"/> is null.</exception>
        internal static bool IsInContactGroupsInternal(ContactInfo contact, bool allGroups, params string[] contactGroupNames)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (contactGroupNames.Length == 0)
            {
                return false;
            }

            // Count groups contact belongs to matching with groups in method parameters
            var groupsMatchCount = contact.ContactGroups.Count(group => contactGroupNames.Any(name => name.Equals(group.ContactGroupName, StringComparison.InvariantCultureIgnoreCase)));

            if (allGroups)
            {
                return groupsMatchCount == contactGroupNames.Length;
            }
            return groupsMatchCount > 0;
        }
    }
}

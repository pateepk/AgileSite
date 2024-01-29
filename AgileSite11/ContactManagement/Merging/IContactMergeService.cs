using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactMergeService), typeof(ContactMergeService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for merging of a <see cref="ContactInfo"/> into another one.
    /// </summary>
    public interface IContactMergeService
    {
        /// <summary>
        /// Tries to merge given <paramref name="contact"/> by <see cref="ContactInfo.ContactEmail"/>. Moves subscriptions and copies contact's data. 
        /// In the case the EMS license is available, moves activities, memberships, relations and copies contact's data. 
        /// Given <paramref name="contact"/> will be used as merge target, i.e. this particular <see cref="ContactInfo"/> will remain, while others with the same <see cref="ContactInfo.ContactEmail"/>
        /// will be merged to the <paramref name="contact"/>.
        /// </summary>
        /// <remarks>
        /// Does not set the result of merge to the current context or store the GUID in response cookie. If the result should be somehow
        /// persistent during requests, caller of the method has to call <see cref="ICurrentContactProvider.SetCurrentContact"/> with the merged contact.
        /// </remarks>
        /// <param name="contact">Contact to be merged with existing one by their emails</param>
        void MergeContactByEmail(ContactInfo contact);


        /// <summary>
        /// Merges given <paramref name="source"/> contact to the <paramref name="target"/>. Moves subscriptions and copies contact's data. 
        /// In the case the EMS license is available, moves activities, memberships and relations.
        /// </summary>
        /// <remarks>
        /// Does not set the result of merge to the current context or store the GUID in response cookie. If the result should be somehow
        /// persistent during requests, caller of the method has to call <see cref="ICurrentContactProvider.SetCurrentContact"/> with the merged contact.
        /// </remarks>
        /// <param name="source">Source contact to be merged to the <paramref name="target"/></param>
        /// <param name="target">Target contact the <paramref name="source"/> is to be merged to</param>
        void MergeContacts(ContactInfo source, ContactInfo target);
    }
}
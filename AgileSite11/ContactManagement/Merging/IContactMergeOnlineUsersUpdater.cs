using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactMergeOnlineUsersUpdater), typeof(ContactMergeOnlineUsersUpdater), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    internal interface IContactMergeOnlineUsersUpdater
    {
        void Update(ContactInfo sourceContact, ContactInfo targetContact);
    }
}
using System;

using CMS.Activities;
using CMS.Core;
using CMS.Membership;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Modifies activity with contact data.
    /// </summary>
    internal class ContactActivityModifier : IActivityModifier
    {
        private readonly ICurrentContactProvider mCurrentContactProvider;
        
        public ContactActivityModifier()
            :this(Service.Resolve<ICurrentContactProvider>())
        {
        }


        public ContactActivityModifier(ICurrentContactProvider currentContactProvider)
        {
            mCurrentContactProvider = currentContactProvider;
        }


        /// <summary>
        /// Updates activity contact id if not already set.
        /// </summary>
        public void Modify(IActivityInfo activity)
        {
            if (ContactIdIsSet(activity))
            {
                activity.ActivityContactGUID = GetContactGuid(activity.ActivityContactID);
                return;
            }

            var currentContact = mCurrentContactProvider.GetCurrentContact(MembershipContext.AuthenticatedUser, false);
            activity.ActivityContactGUID = currentContact.ContactGUID;
            activity.ActivityContactID = currentContact.ContactID;
        }


        private bool ContactIdIsSet(IActivityInfo activity)
        {
            return activity.ActivityContactID != 0;
        }


        private Guid GetContactGuid(int contactID)
        {
            return ContactInfoProvider.GetContactInfo(contactID).ContactGUID;
        }
    }
}
using System;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method to determining whether the <see cref="ContactInfo"/> assigned to the given <see cref="IActivityInfo"/> exists and is monitored or not.
    /// </summary>
    internal class ContactActivityLogValidator : IActivityLogValidator
    {
        /// <summary>
        /// Determines whether the <see cref="ContactInfo"/> assigned to given <paramref name="activity"/> exists and is monitored, i.e. 
        /// <see cref="ContactInfo.ContactMonitored"/> property is set to <c>true</c>.
        /// </summary>
        /// <param name="activity">Activity to be validated</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="IActivityInfo.ActivityContactID"/> of given <paramref name="activity"/> has to be greater than <c>0</c></exception>
        /// <returns>True if assigned <see cref="ContactInfo"/> exists and is monitored; otherwise, false</returns>
        public bool IsValid(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (activity.ActivityContactID <= 0)
            {
                throw new ArgumentException("[ContactActivityLogValidator.ValidateActivity]: Contact ID has to be specified", "activity");
            }
            
            var contact = ContactInfoProvider.GetContactInfo(activity.ActivityContactID);
            if (contact == null)
            {
                return false;
            }

            return contact.ContactMonitored;
        }
    }
}
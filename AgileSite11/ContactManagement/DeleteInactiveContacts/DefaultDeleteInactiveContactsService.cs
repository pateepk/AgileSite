using System;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterImplementation(typeof(IDeleteContactsService), typeof(DefaultDeleteContactsService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Service responsible for deleting all inactive contacts.
    /// Method for deleting is determined by settings.
    /// </summary>
    internal sealed class DefaultDeleteContactsService : IDeleteContactsService
    {
        /// <summary>
        /// Min value that you can insert in the UI (CMSDeleteInactiveContactsLastXDays setting).
        /// </summary>
        private const int LAST_X_DAYS_MIN_VALUE = 10;


        /// <summary>
        /// Deletes batch of inactive contacts.
        /// </summary>
        /// <param name="batchSize">Number of contacts to delete in one call</param>
        /// <returns>Returns true when there are more contacts to delete</returns>
        public bool Delete(int batchSize)
        {
            int remainingContacts = ExecuteDelete(batchSize);
            if (remainingContacts > batchSize)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Deletes inactive contacts.
        /// </summary>
        /// <param name="batchSize">Number of contacts to delete. You can then check returned number of contacts left to delete and call this method until it is 0.</param>
        /// <returns>Number of contacts left to delete</returns>
        private int ExecuteDelete(int batchSize)
        {
            string implementationName = SettingsKeyInfoProvider.GetValue("CMSDeleteInactiveContactsMethod");
            if (string.IsNullOrEmpty(implementationName))
            {
                // No method selected in settings
                return 0;
            }

            int days = SettingsKeyInfoProvider.GetIntValue("CMSDeleteInactiveContactsLastXDays");
            if (days < LAST_X_DAYS_MIN_VALUE)
            {
                throw new InvalidOperationException("Last X days setting ('CMSDeleteInactiveContactsLastXDays') has invalid value. Its value is " + days + ".");
            }

            IDeleteContacts deleteInactiveContacts = DeleteContactsSettingsContainer.GetImplementation(implementationName);
            if (deleteInactiveContacts != null)
            {
                return deleteInactiveContacts.Delete(days, batchSize);
            }
            else
            {
                Service.Resolve<IEventLogService>().LogEvent(
                    "W", 
                    "Delete inactive contacts service", 
                    "NoImplementationFound", 
                    "There is specified implementation '" + implementationName + "' in settings, but no such implementation found. No contacts were deleted.");

                throw new NotImplementedException("There is specified implementation '" + implementationName + "' in settings, but no such implementation found. No contacts were deleted.");
            }
        }
    }
}
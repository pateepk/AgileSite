using System;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.EventLog;

[assembly: RegisterImplementation(typeof(IContactDataInjector), typeof(ContactDataInjector), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Injects data from other objects to <see cref="ContactInfo"/> object.
    /// </summary>
    internal class ContactDataInjector : IContactDataInjector
    {
        /// <summary>
        /// Injects provided <paramref name="data"/> to a <see cref="ContactInfo"/> identified by <paramref name="contactId"/>.
        /// </summary>
        /// <param name="data">Data to update contact with.</param>
        /// <param name="contactId">Contact ID.</param>
        /// <param name="mapper">Maps values from data to contact.</param>
        /// <param name="checker">Optionally checks whether the injection is allowed. If not provided, data is updated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
        public void Inject(ISimpleDataContainer data, int contactId, IContactDataMapper mapper, IContactDataPropagationChecker checker = null)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (checker !=null && !checker.IsAllowed())
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            if (contactId <= 0)
            {
                return;
            }

            var contact = ContactInfoProvider.GetContactInfo(contactId);
            if (contact == null)
            {
                return;
            }

            try
            {
                if (mapper.Map(data, contact))
                {
                    ContactInfoProvider.SetContactInfo(contact);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContactDataInjector", "INJECTDATA", ex);
            }
        }
    }
}
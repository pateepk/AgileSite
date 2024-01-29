using System;

using CMS.Core;
using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents contact changes repository for storing the <see cref="ContactChangeData"/> into the contact change queue.
    /// </summary>
    internal class ContactChangeQueueRepository : IContactChangeRepository
    {
        private readonly ILicenseService mLicenseService;


        public ContactChangeQueueRepository()
        {
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
        }


        /// <summary>
        /// Saves given <paramref name="contactChangeData"/> into the contact change queue.
        /// </summary>
        /// <param name="contactChangeData">Contact change to be saved</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactChangeData"/> is <c>null</c></exception>
        public void Save(ContactChangeData contactChangeData)
        {
            if (contactChangeData == null)
            {
                throw new ArgumentNullException("contactChangeData");
            }

            if (mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement))
            {
                ContactChangeLogWorker.LogContactChange(contactChangeData);
            }
        }
    }
}

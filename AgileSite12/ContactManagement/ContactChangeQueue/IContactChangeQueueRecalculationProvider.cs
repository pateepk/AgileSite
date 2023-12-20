using System.Collections.Generic;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactChangeQueueRecalculationProvider), typeof(SqlContactChangeQueueRecalculationProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for reading the contact changes from the SQL database.
    /// </summary>
    internal interface IContactChangeQueueRecalculationProvider
    {
        /// <summary>
        /// Returns top 10000 items from the queue and at the same time it removes them from the queue.
        /// </summary>
        IEnumerable<ContactChangeData> Dequeue();
    }
}
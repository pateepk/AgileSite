using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IContactChangeRepository), typeof(ContactChangeQueueRepository), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents contact change repository.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public interface IContactChangeRepository
    {
        /// <summary>
        ///  Saves given <paramref name="contactChangeData"/> info to repository.
        /// </summary>
        /// <param name="contactChangeData">Contact change to be saved</param>
        void Save(ContactChangeData contactChangeData);
    }
}
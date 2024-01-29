using CMS;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;

[assembly: RegisterImplementation(typeof(IContactProcessingChecker), typeof(ContactProcessingChecker), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Internal
{
    /// <summary>
    /// Provides method for checking whether the contact processing can continue.
    /// </summary>
    public interface IContactProcessingChecker
    {
        /// <summary>
        /// Checks whether the contact processing can continue in the context of current HTTP request.
        /// </summary>
        /// <returns><c>True</c> if the contact processing can continue; otherwise, <c>false</c></returns>
        bool CanProcessContactInCurrentContext();
    }
}
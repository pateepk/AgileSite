using CMS;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(ICultureService), typeof(DefaultCultureService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Helpers
{
    /// <summary>
    /// Defines culture-related methods
    /// </summary>
    public interface ICultureService
    {
        /// <summary>
        /// Returns true when culture with given code is defined as UICulture.
        /// </summary>
        bool IsUICulture(string cultureCode);
    }
}

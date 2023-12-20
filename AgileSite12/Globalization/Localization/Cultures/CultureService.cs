using CMS.Helpers;

namespace CMS.Localization
{
    /// <summary>
    /// Service providing culture related support
    /// </summary>
    internal sealed class CultureService : ICultureService
    {
        /// <summary>
        /// Returns true when culture with given code is defined as UICulture.
        /// </summary>
        public bool IsUICulture(string cultureCode)
        {
            var culture = CultureInfoProvider.GetCultureInfo(cultureCode);
            return culture != null && culture.CultureIsUICulture;
        }
    }
}
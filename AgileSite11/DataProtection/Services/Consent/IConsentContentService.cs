using System;

using CMS;
using CMS.DataProtection;

[assembly: RegisterImplementation(typeof(IConsentContentService), typeof(ConsentContentService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Interface for service managing language versions of consent content.
    /// </summary>
    internal interface IConsentContentService
    {
        /// <summary>
        /// Adds new or updates existing consent language version.
        /// </summary>
        /// <param name="consentInfo">Consent info.</param>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <param name="shortText">Short text of consent.</param>
        /// <param name="fullText">Full text of consent.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consentInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cultureCode"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shortText"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullText"/> is <c>null</c>.</exception>
        void AddOrUpdateText(ConsentInfo consentInfo, string cultureCode, string shortText, string fullText);
    }
}

using System;

using CMS.Core;

namespace CMS.DataProtection
{
    /// <summary>
    /// Extension methods for <see cref="ConsentInfo"/>.
    /// </summary>
    public static class ConsentInfoExtensions
    {
        /// <summary>
        /// Gets consent's text for specific <paramref name="cultureCode"/>.
        /// </summary>
        /// <param name="consent">Consent for which the text gets retrieved.</param>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <returns>Language specific consent's text.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is null or empty.</exception>
        public static ConsentText GetConsentText(this ConsentInfo consent, string cultureCode)
        {
            if (consent == null)
            {
                throw new ArgumentNullException(nameof(consent));
            }

            var manager = new ConsentContentManager(consent.ConsentContent);

            return new ConsentText
            {
                ShortText = manager.GetShortText(cultureCode),
                FullText = manager.GetFullText(cultureCode)
            };
        }


        /// <summary>
        /// Adds new or updates existing consent language version.
        /// </summary>
        /// <param name="consent">Consent info.</param>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <param name="shortText">Short text of consent.</param>
        /// <param name="fullText">Full text of consent.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consent"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shortText"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullText"/> is <c>null</c>.</exception>
        public static void UpsertConsentText(this ConsentInfo consent, string cultureCode, string shortText, string fullText)
        {
            var consentService = Service.Resolve<IConsentContentService>();

            consentService.AddOrUpdateText(consent, cultureCode, shortText, fullText);
        }
    }
}

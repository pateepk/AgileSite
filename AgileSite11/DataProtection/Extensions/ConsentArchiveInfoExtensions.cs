using System;

namespace CMS.DataProtection
{
    /// <summary>
    /// Extension methods for <see cref="ConsentArchiveInfo"/>.
    /// </summary>
    public static class ConsentArchiveInfoExtensions
    {
        /// <summary>
        /// Gets consent's text for specific <paramref name="cultureCode"/>.
        /// </summary>
        /// <param name="consentArchive">Consent archive for which the text gets retrieved.</param>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <returns>Language specific consent's text.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="consentArchive"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cultureCode"/> is null.</exception>
        public static ConsentText GetConsentText(this ConsentArchiveInfo consentArchive, string cultureCode)
        {
            if (consentArchive == null)
            {
                throw new ArgumentNullException(nameof(consentArchive));
            }

            var manager = new ConsentContentManager(consentArchive.ConsentArchiveContent);

            return new ConsentText
            {
                ShortText = manager.GetShortText(cultureCode),
                FullText = manager.GetFullText(cultureCode)
            };
        }
    }
}

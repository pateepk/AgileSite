using System;

using CMS.Core;
using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Manages consent content language versions.
    /// </summary>
    internal sealed class ConsentContentManager
    {
        private readonly IDataContractSerializerService<ConsentContent> dataContractSerializerService;
        private readonly ConsentContent content;


        /// <summary>
        /// Creates an instance of <see cref="ConsentContentManager"/>.
        /// </summary>
        /// <param name="contentText">Consent content text.</param>
        public ConsentContentManager(string contentText)
        {
            dataContractSerializerService = Service.Resolve<IDataContractSerializerService<ConsentContent>>();

            content = string.IsNullOrEmpty(contentText)
                ? new ConsentContent()
                : dataContractSerializerService.Deserialize(contentText);
        }


        /// <summary>
        /// Adds new or updates existing consent language version.
        /// </summary>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <param name="shortText">Short text of consent.</param>
        /// <param name="fullText">Full text of consent.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shortText"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullText"/> is <c>null</c>.</exception>
        public void Upsert(string cultureCode, string shortText, string fullText)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(cultureCode));
            }

            if (shortText == null)
            {
                throw new ArgumentNullException(nameof(shortText));
            }

            if (fullText == null)
            {
                throw new ArgumentNullException(nameof(fullText));
            }

            var existingConsent = content.ConsentLanguageVersions.Find(c => c.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));
            if (existingConsent != null)
            {
                existingConsent.ShortText = shortText;
                existingConsent.FullText = fullText;
            }
            else
            {
                content.ConsentLanguageVersions.Add(
                    new ConsentLanguageVersion
                    {
                        CultureCode = cultureCode,
                        ShortText = shortText,
                        FullText = fullText
                    }
                );
            }
        }


        /// <summary>
        /// Gets consent content text.
        /// </summary>
        public string GetContentText()
        {
            return dataContractSerializerService.Serialize(content);
        }


        /// <summary>
        /// Gets consent's short text for specific <paramref name="cultureCode"/>.
        /// </summary>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <returns>Language specific consent's short text.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is <c>null</c> or empty.</exception>
        public string GetShortText(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(cultureCode));
            }

            var consentVersion = content.ConsentLanguageVersions.Find(c => c.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));

            return consentVersion?.ShortText;
        }


        /// <summary>
        /// Gets consent's full text for specific <paramref name="cultureCode"/>.
        /// </summary>
        /// <param name="cultureCode">Culture code of consent.</param>
        /// <returns>Language specific consent's full text.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="cultureCode"/> is <c>null</c> or empty.</exception>
        public string GetFullText(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(cultureCode));
            }

            var consentVersion = content.ConsentLanguageVersions.Find(c => c.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));

            return consentVersion?.FullText;
        }
    }
}
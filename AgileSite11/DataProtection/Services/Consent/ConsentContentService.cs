using System;

using CMS.Core;

namespace CMS.DataProtection
{
    /// <summary>
    /// Service for managing language versions of consent content.
    /// </summary>
    internal sealed class ConsentContentService : IConsentContentService
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
        public void AddOrUpdateText(ConsentInfo consentInfo, string cultureCode, string shortText, string fullText)
        {
            if (consentInfo == null)
            {
                throw new ArgumentNullException(nameof(consentInfo));
            }

            var contentManager = new ConsentContentManager(consentInfo.ConsentContent);
            contentManager.Upsert(cultureCode, shortText, fullText);
            var updatedContent = contentManager.GetContentText();

            if (!consentInfo.ConsentContent.Equals(updatedContent, StringComparison.InvariantCulture))
            {
                Service.Resolve<IConsentArchiveService>().Archive(consentInfo);
            }

            consentInfo.ConsentContent = updatedContent;
            ConsentInfoProvider.SetConsentInfo(consentInfo);
        }
    }
}

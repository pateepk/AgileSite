using CMS;
using CMS.DataProtection;

[assembly: RegisterImplementation(typeof(IConsentArchiveService), typeof(ConsentArchiveService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Service for archiving consent versions.
    /// </summary>
    internal sealed class ConsentArchiveService : IConsentArchiveService
    {
        /// <summary>
        /// Archives specified consent if it was already agreed or revoked by some contact.
        /// </summary>
        /// <param name="consent">Consent to archive.</param>
        public void Archive(ConsentInfo consent)
        {
            if (ConsentIsBoundToContact(consent))
            {
                ArchiveConsent(consent);
            }
        }


        private static bool ConsentIsBoundToContact(ConsentInfo consent)
        {
            return ConsentAgreementInfoProvider.GetConsentAgreements()
                .WhereEquals("ConsentAgreementConsentID", consent.ConsentID)
                .WhereEquals("ConsentAgreementConsentHash", consent.ConsentHash)
                .Count > 0;
        }


        private static void ArchiveConsent(ConsentInfo consent)
        {
            var archivedConsent = new ConsentArchiveInfo
            {
                ConsentArchiveHash = consent.ConsentHash,
                ConsentArchiveConsentID = consent.ConsentID,
                ConsentArchiveContent = consent.ConsentContent,
                ConsentArchiveLastModified = consent.ConsentLastModified
            };

            ConsentArchiveInfoProvider.SetConsentArchiveInfo(archivedConsent);
        }
    }
}

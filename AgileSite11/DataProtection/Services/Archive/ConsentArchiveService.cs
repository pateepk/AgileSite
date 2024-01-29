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
        /// Archives specified consent if it was already agreed by some contact.
        /// </summary>
        /// <param name="consent">Consent to archive.</param>
        public void Archive(ConsentInfo consent)
        {
            if (ConsentIsAgreed(consent))
            {
                ArchiveAgreedConsent(consent);
            }
        }


        private static bool ConsentIsAgreed(ConsentInfo consent)
        {
            return ConsentAgreementInfoProvider.GetConsentAgreements()
                .WhereEquals("ConsentAgreementConsentID", consent.ConsentID)
                .WhereEquals("ConsentAgreementConsentHash", consent.ConsentHash)
                .Count > 0;
        }


        private static void ArchiveAgreedConsent(ConsentInfo consent)
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

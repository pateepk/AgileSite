using CMS;
using CMS.DataProtection;

[assembly: RegisterImplementation(typeof(IConsentAgreementMergeService), typeof(ConsentAgreementMergeService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Service for merging consent agreements of contacts.
    /// </summary>
    internal interface IConsentAgreementMergeService
    {
        /// <summary>
        /// Moves all consent agreements from source contact to target contact.
        /// </summary>
        /// <param name="sourceContactId">Source Contact ID.</param>
        /// <param name="targetContactId">Target Contact ID.</param>
        void Merge(int sourceContactId, int targetContactId);
    }
}

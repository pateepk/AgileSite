namespace CMS.DataProtection
{
    /// <summary>
    /// Service for merging consent agreements of contacts.
    /// </summary>
    internal sealed class ConsentAgreementMergeService : IConsentAgreementMergeService
    {
        /// <summary>
        /// Moves all consent agreements from source contact to target contact.
        /// </summary>
        /// <param name="sourceContactId">Source Contact ID</param>
        /// <param name="targetContactId">Target Contact ID</param>
        public void Merge(int sourceContactId, int targetContactId)
        {
            ConsentAgreementInfoProvider.BulkMoveConsentAgreements(sourceContactId, targetContactId);
        }
    }
}

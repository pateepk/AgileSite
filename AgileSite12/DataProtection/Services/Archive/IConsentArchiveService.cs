namespace CMS.DataProtection
{
    /// <summary>
    /// Interface for service archiving consent versions.
    /// </summary>
    internal interface IConsentArchiveService
    {
        /// <summary>
        /// Puts specified consent into archive.
        /// </summary>
        /// <param name="consent">Consent to archive.</param>
        void Archive(ConsentInfo consent);
    }
}

namespace CMS.DataProtection
{
    /// <summary>
    /// Data protection events.
    /// </summary>
    public static class DataProtectionEvents
    {
        /// <summary>
        /// Fires after a consent agreement is revoked.
        /// </summary>
        public static RevokeConsentAgreementHandler RevokeConsentAgreement = new RevokeConsentAgreementHandler { Name = "DataProtectionEvents.RevokeConsentAgreement" };
    }
}

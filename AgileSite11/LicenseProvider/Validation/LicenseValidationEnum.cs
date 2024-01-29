namespace CMS.LicenseProvider
{
    /// <summary>
    /// Type of license validity.
    /// </summary>
    public enum LicenseValidationEnum
    {
        /// <summary>
        /// Valid.
        /// </summary>
        Valid,

        /// <summary>
        /// Invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Expired.
        /// </summary>
        Expired,

        /// <summary>
        /// WrongFormat.
        /// </summary>
        WrongFormat,

        /// <summary>
        /// NotAvailable.
        /// </summary>
        NotAvailable,

        /// <summary>
        /// Unknown license status.
        /// </summary>
        Unknown
    }
}
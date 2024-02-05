
namespace CMS.LicenseProvider
{
    /// <summary>
    /// Provides IP address functions for licensing.
    /// </summary>
    internal interface ILicenseAddressService
    {
        /// <summary>
        /// Returns true when given domain is local loopback, otherwise false.
        /// </summary>
        bool IsLocal(string domain);


        /// <summary>
        /// Returns given domain name transformed to non-shortened format.
        /// </summary>
        string ToFullFormat(string domain);
    }
}

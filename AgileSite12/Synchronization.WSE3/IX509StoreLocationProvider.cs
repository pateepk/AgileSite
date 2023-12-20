using System.Security.Cryptography.X509Certificates;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Describes provider for retrieving certificate store location.
    /// </summary>
    internal interface IX509StoreLocationProvider
    {
        /// <summary>
        /// Returns store location used for X.509 certificate store.
        /// </summary>
        StoreLocation GetStoreLocation();
    }
}
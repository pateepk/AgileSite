using System;
using System.Security.Cryptography;

namespace CMS.Membership
{
    /// <summary>
    /// Provides keyed hash algorithms as per RFC 6238.
    /// </summary>
    internal static class HmacFactories
    {
        /// <summary>
        /// Creates an instance of System.Security.Cryptography.HMACSHA1.
        /// </summary>
        public static readonly Func<HMAC> HMACSHA1 = () => HMAC.Create("System.Security.Cryptography.HMACSHA1");

        /// <summary>
        /// Creates an instance of System.Security.Cryptography.HMACSHA256.
        /// </summary>
        public static readonly Func<HMAC> HMACSHA256 = () => HMAC.Create("System.Security.Cryptography.HMACSHA256");

        /// <summary>
        /// Creates an instance of System.Security.Cryptography.HMACSHA512.
        /// </summary>
        public static readonly Func<HMAC> HMACSHA512 = () => HMAC.Create("System.Security.Cryptography.HMACSHA512");
    }
}

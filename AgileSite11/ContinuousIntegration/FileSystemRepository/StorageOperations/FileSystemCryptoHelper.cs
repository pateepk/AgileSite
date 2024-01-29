using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// File system repository cryptography helper methods.
    /// </summary>
    internal class FileSystemCryptoHelper
    {
        /// <summary>
        /// Gets default hash algorithm.
        /// </summary>
        /// <returns>Instance of default hash algorithm.</returns>
        /// <remarks>
        /// A new instance of hash algorithm is created upon each method call (i.e. you should dispose the instance
        /// when no longer needed).
        /// </remarks>
        public static HashAlgorithm GetHashAlgorithm()
        {
            // Create new instance to ensure thread safety
            return new MD5CryptoServiceProvider();
        }


        /// <summary>
        /// Converts byte array to string of hexadecimal digits (each byte is represented by 2 digits).
        /// The length of the string is truncated to <paramref name="truncateToLength"/>, if specified.
        /// No boundary check is performed.
        /// </summary>
        /// <param name="data">Byte array to be converted to hexadecimal digits string.</param>
        /// <param name="truncateToLength">If specified, the resulting hexadecimal digits string is truncated to given length. Full length is taken by default.</param>
        /// <returns>Bytes of <paramref name="data"/> array converted to hexadecimal string, optionally truncated to specified length.</returns>
        public static string BytesToHexa(byte[] data, int? truncateToLength = null)
        {
            var bytesTaken = truncateToLength.HasValue ? (truncateToLength.Value + 1) / 2 : data.Length;

            var seed = new StringBuilder(bytesTaken * 2);
            var hash = data.Take(bytesTaken).Aggregate(seed, (sb, b) => sb.AppendFormat("{0:x2}", b));

            return truncateToLength.HasValue ? hash.ToString(0, truncateToLength.Value) : hash.ToString();
        }
    }
}

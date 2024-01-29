using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Provides IP address functions for licensing.
    /// </summary>
    internal sealed class LicenseAddressService : ILicenseAddressService
    {
        private const string LOCALHOST = "localhost";


        private static ILicenseAddressService mInstance;


        public static ILicenseAddressService Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new LicenseAddressService(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }



        /// <summary>
        /// Returns true when given domain is local loopback, otherwise false.
        /// </summary>
        public bool IsLocal(string domain)
        {
            IPAddress ip;
            var hostName = Uri.CheckHostName(domain);
            if ((hostName == UriHostNameType.IPv6 || hostName == UriHostNameType.IPv4) && IPAddress.TryParse(domain, out ip))
            {
                return IPAddress.IsLoopback(ip);
            }

            return domain.ToLowerInvariant() == LOCALHOST;
        }


        /// <summary>
        /// Returns given domain name transformed to non-shortened format.
        /// </summary>
        public string ToFullFormat(string domain)
        {
            IPAddress ip;
            if (Uri.CheckHostName(domain) == UriHostNameType.IPv6 && IPAddress.TryParse(domain, out ip))
            {
                var strings = Enumerable.Range(0, 8) // create index
                                        .Select(i => ip.GetAddressBytes().ToList().GetRange(i * 2, 2)) // get 8 chunks of bytes
                                        .Select(i =>
                                        {
                                            i.Reverse();
                                            return i;
                                        }) // reverse bytes for endianness
                                        .Select(bytes => BitConverter.ToInt16(bytes.ToArray(), 0)) // convert bytes to 16 bit int
                                        .Select(int16 => string.Format("{0:X4}", int16).ToUpperInvariant()); // format int as a 4 digit hex 

                var joined = string.Join(":", strings); // join hex ints with ':'
                return string.Format("[{0}]", joined); // brackets are required
            }

            return domain;
        }
    }
}
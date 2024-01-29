using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Validator method to be choosed.
    /// </summary>
    public enum CertificateValidatorEnum : int
    {
        /// <summary>
        /// Chain trust.
        /// </summary>
        ChainTrust = 0,

        /// <summary>
        /// Peer or chain trust.
        /// </summary>
        PeerOrChainTrust = 1,

        /// <summary>
        /// Peer trust.
        /// </summary>
        PeerTrust = 2,

        /// <summary>
        /// No validation applied.
        /// </summary>
        None = 3,
    }
}

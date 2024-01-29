using System.Data;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents an SMTP server container designed to be used in e-mail tokens.
    /// </summary>
    internal sealed class SMTPServerTokenData : SMTPServerInfo
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets a status of the SMTP server.
        /// </summary>
        internal SMTPServerStatusEnum ServerStatus
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the count of send attempts.
        /// </summary>
        internal int SendAttempts
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SMTPServerTokenData"/> class.
        /// </summary>
        internal SMTPServerTokenData()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SMTPServerTokenData"/> class.
        /// </summary>
        /// <param name="dr">The dr</param>
        internal SMTPServerTokenData(DataRow dr)
            : base(dr)
        {
        }

        #endregion
    }
}
using System;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Encapsulates a result of a acquire operation in an SMTP server lookup table.
    /// </summary>
    internal class SMTPServerLookupResult
    {
        #region "Properties"

        /// <summary>
        /// Gets the SMTP server availability.
        /// </summary>        
        public SMTPServerAvailabilityEnum Availability
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the SMTP server.
        /// </summary>        
        public SMTPServerTokenData SMTPServer
        {
            get;
            private set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SMTPServerLookupResult"/> structure.
        /// </summary>
        /// <param name="availability">The SMTP server availability</param>
        /// <param name="smtpServer">The SMTP server</param>
        internal SMTPServerLookupResult(SMTPServerAvailabilityEnum availability, SMTPServerTokenData smtpServer)
        {
            Availability = availability;
            SMTPServer = smtpServer;
        }

        #endregion
    }
}
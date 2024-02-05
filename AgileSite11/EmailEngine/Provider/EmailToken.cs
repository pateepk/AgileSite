using System;
using System.Net.Mail;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents a token that encapsulates the information necessary for callbacks from 
    /// the EmailProvider's asynchronous Send methods.
    /// </summary>
    /// <remarks>
    /// The token contains the information about site, timestamp of the last send attempt, 
    /// the message itself and related object in the DB as well as the used ID (for mass messages). 
    /// This is required in order to persist the message in case of delivery failure or when
    /// attempting to resend the message. Additionally, an SMTP server is present in the token as
    /// well for the correct function and signaling in the e-mail queue (idle/busy notifications 
    /// on callback).
    /// A unique identifier was added to the token to distinguish the threads in the callback so
    /// that only threads that own the queue item will process it.
    /// </remarks>
    public sealed class EmailToken
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the <see cref="EmailInfo" /> object that represents the e-mail message in the DB.
        /// </summary>
        /// <value>The e-mail message identifier</value>
        internal EmailInfo Email
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the User ID (for mass messages).
        /// </summary>
        /// <value>The user ID</value>
        internal int UserId
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        /// <value>The name of the site.</value>
        internal string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the SMTP server.
        /// </summary>
        /// <value>The SMTP server.</value>
        internal SMTPServerTokenData SMTPServer
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the e-mail message.
        /// </summary>
        /// <value>The message.</value>
        internal MailMessage Message
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the timestamp of last send attempt.
        /// </summary>
        /// <value>The last send attempt.</value>
        internal DateTime LastSendAttempt
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
        /// Initializes a new instance of the <see cref="EmailToken"/> class.
        /// </summary>
        internal EmailToken()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmailToken"/> class using the provided token data.
        /// </summary>
        /// <param name="email">E-mail info DB container</param>
        /// <param name="userId">User ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="message">E-mail message that will be sent along with this token</param>        
        internal EmailToken(EmailInfo email, int userId, string siteName, MailMessage message)
        {
            Email = email;
            UserId = userId;
            SiteName = siteName;
            Message = message;
        }

        #endregion
    }
}
using System;

using CMS.EventLog;

using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;

namespace CMS.Newsletters.Extensions
{
    /// <summary>
    /// Contains extension methods for POP3 client to simplify message processing.
    /// </summary>
    internal static class Pop3ClientExtensions
    {
        /// <summary>
        /// Fetches a message from the server and parses it, consuming and logging any exceptions in the process.
        /// </summary>
        /// <param name="pop3Client">POP3 client</param>
        /// <param name="messageNumber">Message number on the server</param>
        /// <returns>Message object</returns>
        public static Message GetSafeMessage(this Pop3Client pop3Client, int messageNumber)
        {
            try
            {
                return pop3Client.GetMessage(messageNumber);
            }
            catch (PopClientException pce)
            {
                EventLogProvider.LogException("Newsletter", "BouncedEmails", pce);
                return null;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Newsletter", "BouncedEmails", ex);
                return null;
            }
        }
    }
}
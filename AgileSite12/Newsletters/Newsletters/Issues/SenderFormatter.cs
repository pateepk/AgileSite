namespace CMS.Newsletters
{
    /// <summary>
    /// Formates the sender name and email address.
    /// </summary>
    internal static class SenderFormatter
    {
        /// <summary>
        /// Limit for the length of sender email in format 'name &lt;email_address&gt;'.
        /// </summary>
        public const int SENDER_LENGTH_LIMIT = 250;


        /// <summary>
        /// Gets the email sender string from given sender name and sender email in format 'name &lt;email_address&gt;'.
        /// </summary>
        /// <param name="senderName">Sender name.</param>
        /// <param name="senderEmail">Sender email.</param>
        public static string GetSender(string senderName, string senderEmail)
        {
            string sender = $"{senderName} <{senderEmail}>";

            return sender.Length > SENDER_LENGTH_LIMIT ? senderEmail : sender;
        }
    }
}

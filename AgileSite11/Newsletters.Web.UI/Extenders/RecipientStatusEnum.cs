namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Enumeration of the email recipient status
    /// </summary>
    public enum EmailRecipientStatusEnum
    {
        /// <summary>
        /// Everything is set, the recipient is marketable
        /// </summary>
        Marketable = 0,

        /// <summary>
        /// The recipient is anonymous without email
        /// </summary>
        MissingEmail = 1,

        /// <summary>
        /// The recipient opted out from receiving emails
        /// </summary>
        OptedOut = 2,

        /// <summary>
        /// The recipient email is unreachable
        /// </summary>
        Bounced = 3
    }
}
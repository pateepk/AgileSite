namespace CMS.Newsletters
{
    /// <summary>
    /// Class containing all newsletters macro constants that can be used in templates or directly in issues.
    /// </summary>
    internal sealed class NewsletterMacroConstants
    {
        /// <summary>
        /// Macro that evaluates <see cref="CMS.Newsletters.Email"/>.
        /// </summary>
        public const string Email = "Email";

        /// <summary>
        /// Macro that evaluates <see cref="CMS.Newsletters.EmailFeed"/>.
        /// </summary>
        public const string EmailFeed = "EmailFeed";

        /// <summary>
        /// Macro that evaluates <see cref="CMS.Newsletters.Recipient"/>.
        /// </summary>
        public const string Recipient = "Recipient";


        /// <summary>
        /// Macro that evaluates <see cref="CMS.Newsletters.Advanced"/>.
        /// </summary>
        public const string Advanced = "Advanced";
    }
}
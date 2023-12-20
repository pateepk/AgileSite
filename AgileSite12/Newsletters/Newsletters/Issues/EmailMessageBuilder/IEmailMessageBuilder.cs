using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for building the <see cref="EmailMessage"/>.
    /// </summary>
    internal interface IEmailMessageBuilder
    {
        /// <summary>
        /// Builds the email message.
        /// </summary>
        EmailMessage Build();
    }
}

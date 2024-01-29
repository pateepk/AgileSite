using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for modifying the <see cref="EmailMessage"/> when built using the <see cref="IEmailMessageBuilder"/>.
    /// </summary>
    internal interface IEmailMessageModifier
    {
        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to be modified.</param>
        void Apply(EmailMessage message);
    }
}

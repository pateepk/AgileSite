using CMS.DataEngine;
using CMS.EmailEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include attachments for confirmation.
    /// </summary>
    internal sealed class ConfirmationAttachmentsMessageModifier : IEmailMessageModifier
    {
        private readonly EmailTemplateInfo template;


        /// <summary>
        /// Creates an instance of <see cref="ConfirmationAttachmentsMessageModifier"/> class.
        /// </summary>
        /// <param name="template">Email template.</param>
        public ConfirmationAttachmentsMessageModifier(EmailTemplateInfo template)
        {
            this.template = template;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            // Attach metafiles from template to email (adds them to email instead of linking)
            EmailHelper.ResolveMetaFileImages(message, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
        }
    }
}

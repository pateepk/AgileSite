using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Converts most important values from <see cref="EmailTemplateInfo"/> into <see cref="EmailMessage"/>
    /// </summary>
    internal sealed class EmailTemplateToEmailMessageConverter
    {
        /// <summary>
        /// Transfer template values from given template to given email message
        /// </summary>
        /// <param name="message">target object where the values are copied</param>
        /// <param name="template">input object from which the values are copied</param>
        /// <param name="resolver">macro resolver used for macro processing, if none is passed resolver is retrieved vie <see cref="MacroResolver.GetInstance"/></param>
        /// <remarks>
        /// Method temporarily changes the <see cref="MacroSettings.EncodeResolvedValues"/> when processing message body
        /// </remarks>
        /// <remarks>
        /// Method resolves possible meta file images defined in message body
        /// </remarks>
        internal void TransferTemplateValues(EmailMessage message, EmailTemplateInfo template, MacroResolver resolver = null)
        {
            resolver = resolver ?? MacroResolver.GetInstance();

            // Enable macro encoding for body
            var originalValue = resolver.Settings.EncodeResolvedValues;
            resolver.Settings.EncodeResolvedValues = true;

            // The e-mail body and subject may contain substrings in format {%fieldname%}
            if (!string.IsNullOrEmpty(message.Body))
            {
                // Use preset body, it could have resolved meta file images
                message.Body = resolver.ResolveMacros(message.Body);
            }
            else
            {
                // Use template body
                message.Body = resolver.ResolveMacros(template.TemplateText);
            }

            // Disable macro encoding for plaintext body and subject
            resolver.Settings.EncodeResolvedValues = originalValue;

            // Attach template meta-files to e-mail
            EmailHelper.ResolveMetaFileImages(message, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

            if (!string.IsNullOrEmpty(message.PlainTextBody))
            {
                // Use preset plain text body
                message.PlainTextBody = resolver.ResolveMacros(message.PlainTextBody);
            }
            else
            {
                // Use template plain text
                message.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);
            }

            // Use template data if not empty instead of e-mail values
            // Subject
            message.Subject = !string.IsNullOrEmpty(template.TemplateSubject)
                                    ? resolver.ResolveMacros(template.TemplateSubject)
                                    : resolver.ResolveMacros(message.Subject);

            if (!string.IsNullOrEmpty(template.TemplateReplyTo))
            {
                message.ReplyTo = template.TemplateReplyTo;
            }

            // Sender address
            if (!string.IsNullOrEmpty(template.TemplateFrom))
            {
                message.From = template.TemplateFrom;
            }

            // Cc
            if (!string.IsNullOrEmpty(template.TemplateCc))
            {
                message.CcRecipients = template.TemplateCc;
            }

            // Bcc
            if (!string.IsNullOrEmpty(template.TemplateBcc))
            {
                message.BccRecipients = template.TemplateBcc;
            }
        }
    }
}

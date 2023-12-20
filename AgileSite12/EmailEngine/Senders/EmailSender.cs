
using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Provides interface to general e-mail sending capabilities.
    /// </summary>
    public static class EmailSender
    {
        #region "Public methods"

        /// <summary>
        /// Send an e-mail with global settings.
        /// </summary>
        /// <param name="message">EmailMessage object with body</param>
        public static void SendEmail(EmailMessage message)
        {
            SendEmail(string.Empty, message);
        }


        /// <summary>
        /// Sends an e-mail based on the template and dynamically merged values.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">EmailMessage object without body</param>
        /// <param name="templateName">Name of the template that will be used for e-mail body text</param>
        /// <param name="resolver">Macro resolver which will be used to resolve the e-mail body (plain) text and subject</param>
        /// <param name="sendImmediately">Send e-mail directly</param>
        public static void SendEmail(string siteName, EmailMessage message, string templateName, MacroResolver resolver, bool sendImmediately)
        {
            // Generate email message body using the template and set it to the message object.
            EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate(templateName, siteName);

            if (emailTemplate != null)
            {
                // Send message using SendEmailWithTemplateText method.
                SendEmailWithTemplateText(siteName, message, emailTemplate, resolver, sendImmediately);
            }
            else
            {
                // Send message using SendEmail method.
                SendEmail(siteName, message, sendImmediately);
            }
        }


        /// <summary>
        /// Send an e-mail with the site settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">EmailMessage object with body</param>
        /// <param name="sendImmediately">Send e-mail directly; optional; default value is FALSE</param>
        public static void SendEmail(string siteName, EmailMessage message, bool sendImmediately = false)
        {
            // Check if sending are allowed in current action context
            if (CMSActionContext.CurrentSendEmails)
            {
                bool sendNow = (EmailHelper.Settings.EmailsEnabled(siteName) && (sendImmediately || !EmailHelper.Settings.EmailQueueEnabled(siteName)));
                if (sendNow)
                {
                    // Send it to the application e-mail queue
                    ThreadSender.Instance.SendToQueue(message, siteName);
                }
                else
                {
                    // Send it to the database e-mail queue
                    EmailInfoProvider.SetEmailInfo(message, EmailHelper.GetSiteId(siteName));
                }
            }
        }


        /// <summary>
        /// Sends an e-mail based on the template text and dynamically merged values.
        /// </summary>
        /// <remarks>
        /// The <paramref name="template"/> body or subject is not used if the body or subject of the <paramref name="message"/> is not empty.
        /// </remarks>
        /// <param name="siteName">Site name</param>
        /// <param name="message">EmailMessage object without body</param>
        /// <param name="template">Template that will be used for e-mail body (plain) text and/or subject</param>
        /// <param name="resolver">Macro resolver which will be used to resolve the e-mail body (plain) text and subject</param>
        /// <param name="sendImmediately">Send e-mail directly</param>
        public static void SendEmailWithTemplateText(string siteName, EmailMessage message, EmailTemplateInfo template, MacroResolver resolver, bool sendImmediately)
        {
            if (template != null)
            {
                var converter = new EmailTemplateToEmailMessageConverter();
                converter.TransferTemplateValues(message, template, resolver);
            }

            // Send message using SendEmail method
            SendEmail(siteName, message, sendImmediately);
        }


        /// <summary>
        /// Creates mass e-mails in the e-mail queue for specified members.
        /// </summary>
        /// <param name="message">E-mail message</param>
        /// <param name="userIds">User IDs separated by semicolon (;), could be 'null'</param>
        /// <param name="roleIds">Role IDs separated by semicolon (;), could be 'null'</param>
        /// <param name="groupIds">Group IDs separated by semicolon (;), could be 'null'</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="sendToEveryone">Indicates if the e-mail should be send to generic role 'Everyone'; default value is FALSE</param>
        public static void SendMassEmails(EmailMessage message, string userIds, string roleIds, string groupIds, int siteId, bool sendToEveryone = false)
        {
            if ((message == null) || !CMSActionContext.CurrentSendEmails || (string.IsNullOrEmpty(userIds) && string.IsNullOrEmpty(roleIds) && string.IsNullOrEmpty(groupIds) && !sendToEveryone))
            {
                return;
            }

            // Create e-mail in the queue which will be used as a pattern
            EmailInfo sampleMsg = EmailInfoProvider.SetEmailInfo(message, siteId, true, true);
            if (sampleMsg == null)
            {
                return;
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@EmailID", sampleMsg.EmailID);
            parameters.Add("@users", string.Concat(";", userIds, ";"));
            parameters.Add("@roles", string.Concat(";", roleIds, ";"));
            parameters.Add("@groups", string.Concat(";", groupIds, ";"));
            parameters.Add("@SiteID", siteId);
            parameters.Add("@Everyone", sendToEveryone);

            // Create records in binding table (CMS_EmailUser)
            // and update status of pattern e-mail to 'waiting so it could be sent
            ConnectionHelper.ExecuteQuery("cms.email.createmassemails", parameters);
        }


        /// <summary>
        /// Sends the test e-mail to verify that the SMTP server receives e-mails.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="message">The message</param>
        /// <param name="smtpServer">The SMTP server</param>
        /// <remarks>
        /// This method sends e-mail messages synchronously and should be used for testing only
        /// as it may disrupt the e-mail queue sending thread if the SMTP server is in use.
        /// </remarks>
        public static void SendTestEmail(string siteName, EmailMessage message, SMTPServerInfo smtpServer)
        {
            EmailProvider.SendEmail(siteName, message.ToMailMessage(siteName), smtpServer);
        }

        #endregion
    }
}
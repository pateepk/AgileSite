using System;
using System.Net.Mail;

using CMS.EventLog;
using CMS.Helpers;
using CMS.EmailEngine;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for support requests when no support is online.
    /// </summary>
    public static class SupportOfflineMessageHelper
    {
        /// <summary>
        /// Sends support request to email.
        /// 
        /// Does nothing, if sending to email is not enabled.
        /// </summary>
        /// <param name="createdWhen">Sending time of this message</param>
        /// <param name="senderEmailAddress">Email address of sender (the one who requests support)</param>
        /// <param name="subject">Subject</param>
        /// <param name="messageText">Body of the message</param>
        /// <returns>True if message was sent</returns>
        public static bool SendSupportMessageAsMail(DateTime createdWhen, string senderEmailAddress, string subject, string messageText)
        {
            if (ChatSettingsProvider.IsSupportMailEnabledAndValid)
            {
                try
                {
                    MailMessage mailMessage = new MailMessage(senderEmailAddress, ChatSettingsProvider.SupportMessageSendToSetting);

                    mailMessage.Subject = string.Format("{0} {1}", ResHelper.GetString("chat.support.newsupportnotification"), subject);

                    // Build body. Format is taken from resource string.
                    string body = string.Format(ResHelper.GetString("chat.supportmail.format"),
                        createdWhen,
                        senderEmailAddress,
                        subject,
                        messageText);


                    mailMessage.Body = body;

                    EmailHelper.SetReplyToEmailAddress(mailMessage, senderEmailAddress);

                    EmailSender.SendEmail(new EmailMessage(mailMessage));

                    return true;
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Chat.SupportOfflineMessageHelper", "Sending mail", ex);

                    return false;
                }
            }

            return false;
        }
    }
}

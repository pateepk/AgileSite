using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// This class sends notifications about contact exceeding score limit.
    /// </summary>
    internal class ScoreNotificationsSender
    {
        private EmailTemplateInfo mScoreNotificationEmailTemplate;


        /// <summary>
        /// Template for scoring email notifications.
        /// </summary>
        private EmailTemplateInfo ScoreNotificationEmailTemplate
        {
            get
            {
                // Gets the global template
                return mScoreNotificationEmailTemplate ?? (mScoreNotificationEmailTemplate = EmailTemplateProvider.GetEmailTemplate("scoring.notification", null));
            }
        }


        /// <summary>
        /// Sends notification about <paramref name="contact"/> reaching score limit in <paramref name="score"/>. Email template with code name "scoring.notification" is used.
        /// </summary>
        /// <remarks>
        /// Notification isn't send if email template with code name "scoring.notification" does not exist or <paramref name="score"/> does not have notification email set.
        /// </remarks>
        /// <param name="contact">Contact who reached score limit</param>
        /// <param name="score">ScoreInfo notification is about</param>
        /// <param name="scoreValue">Number of points <paramref name="contact"/> reached in <paramref name="score"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="score"/> is null</exception>
        public void SendNotification(ContactInfo contact, ScoreInfo score, int scoreValue)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }
            if (score == null)
            {
                throw new ArgumentNullException("score");
            }

            // Do not send emails if notification template does not exist
            if (ScoreNotificationEmailTemplate == null)
            {
                return;
            }

            // Do not send emails if score does not have notification email set or if it is a wrong format
            if (!ValidationHelper.AreEmails(score.ScoreNotificationEmail))
            {
                return;
            }

            EmailMessage email = new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Default,
                From = EmailHelper.Settings.NotificationsSenderAddress()
            };

            // Add template metafiles as attachments to the email
            EmailHelper.ResolveMetaFileImages(email, ScoreNotificationEmailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

            MacroResolver resolver = MacroResolver.GetInstance();

            resolver.SetNamedSourceData("Contact", contact);
            resolver.SetNamedSourceData("Score", score);
            resolver.SetNamedSourceData("ScoreValue", scoreValue);

            email.Body = null;
            email.PlainTextBody = null;
            email.Recipients = score.ScoreNotificationEmail;

            // Send global email
            EmailSender.SendEmailWithTemplateText(String.Empty, email, ScoreNotificationEmailTemplate, resolver, false);
        }
    }
}
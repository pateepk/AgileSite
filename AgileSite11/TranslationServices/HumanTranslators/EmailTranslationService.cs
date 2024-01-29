using System;
using System.Net.Mail;

using CMS.IO;
using CMS.Helpers;
using CMS.EmailEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.MacroEngine;
using CMS.DataEngine;

using SystemIO = System.IO;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Email translations provider.
    /// </summary>
    public class EmailTranslationService : AbstractHumanTranslationService
    {
        #region "Properties"

        /// <summary>
        /// List of recipients of the submission.
        /// </summary>
        public string Recipients
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteName + ".CMSEmailTranslationRecipients");
            }
        }


        /// <summary>
        /// E-mail address to send the e-mails from.
        /// </summary>
        public string SendFromEmail
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteName + ".CMSEmailTranslationFrom");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public override bool IsAvailable()
        {
            // This service is available when recipients and sender e-mail are ready
            return !(string.IsNullOrEmpty(Recipients) && string.IsNullOrEmpty(ValidationHelper.GetString(CustomParameter, ""))) && !string.IsNullOrEmpty(SendFromEmail);
        }


        /// <summary>
        /// Checks if target language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsTargetLanguageSupported(string langCode)
        {
            // All languages are supported
            return true;
        }


        /// <summary>
        /// Checks if source language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsSourceLanguageSupported(string langCode)
        {
            // All languages are supported
            return true;
        }


        /// <summary>
        /// Creates new submission (or resubmits existing if submission ticket is present).
        /// </summary>
        /// <param name="submission">Submission object</param>
        /// <exception cref="Exception">Thrown when missing license for translation feature</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="submission"/> is <c>null</c></exception>
        public override string CreateSubmission(TranslationSubmissionInfo submission)
        {
            TranslationServiceHelper.CheckLicense();

            if (submission == null)
            {
                throw new ArgumentNullException("submission");
            }

            try
            {
                // Get template
                const string templateName = "translationservice.submittranslation";
                var template = EmailTemplateProvider.GetEmailTemplate(templateName, submission.SubmissionSiteID);
                if (template == null)
                {
                    return ResHelper.GetString("translationservice.templatedoesnotexist");
                }

                // Get the zip file
                var outputStream = new SystemIO.MemoryStream();
                TranslationServiceHelper.WriteSubmissionInZIP(submission, outputStream);
                outputStream.Position = 0;

                // Get the e-mail sender
                string emailFrom = SendFromEmail;
                var user = UserInfoProvider.GetUserInfo(submission.SubmissionSubmittedByUserID);
                if ((user != null) && !string.IsNullOrEmpty(user.Email))
                {
                    emailFrom = user.Email;
                }

                // Get the e-mail recipient
                string emailTo = Recipients;
                var service = TranslationServiceInfoProvider.GetTranslationServiceInfo(submission.SubmissionServiceID);
                if ((service != null) && !String.IsNullOrEmpty(service.TranslationServiceParameter))
                {
                    emailTo = service.TranslationServiceParameter;
                }

                // Initialize message with attached zip file
                var email = new EmailMessage();
                email.From = emailFrom;
                email.Recipients = emailTo;
                email.Attachments.Add(new Attachment(outputStream, TranslationServiceHelper.GetSubmissionFileName(submission)));

                // Init resolver with submission data
                var resolver = MacroResolver.GetInstance().CreateChild();
                resolver.SetNamedSourceData("Submission", submission);
                resolver.SetNamedSourceData("SubmissionLink", TranslationServiceHelper.GetSubmissionLinkURL(submission));

                EmailSender.SendEmail(SiteInfoProvider.GetSiteName(submission.SubmissionSiteID), email, templateName, resolver, true);
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }
            return null;
        }


        /// <summary>
        /// Cancels given submission.
        /// </summary>
        /// <param name="submission">Submission to cancel</param>
        public override string CancelSubmission(TranslationSubmissionInfo submission)
        {
            // This provider does not support automatic cancel.
            return null;
        }


        /// <summary>
        /// Retrieves completed XLIFF files from the service and processes them (imports them into the system). Returns true if everything went well.
        /// </summary>
        /// <param name="siteName">Name of site for which this method downloads completed XLIFF files.</param>
        public override string DownloadCompletedTranslations(string siteName)
        {
            // This provider does not support automatic download.
            return null;
        }

        #endregion
    }
}
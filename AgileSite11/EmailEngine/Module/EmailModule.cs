using CMS;
using CMS.DataEngine;
using CMS.EmailEngine;

[assembly: RegisterModule(typeof(EmailModule))]

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents the Email module.
    /// </summary>
    public class EmailModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for general purposes.
        /// </summary>
        public const string GENERAL_EMAIL_TEMPLATE_TYPE_NAME = "general";

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailModule()
            : base(new EmailModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            EmailHandlers.Init();
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("SendEmail", SendEmail);
        }


        /// <summary>
        /// Send the e-mail
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object SendEmail(object[] parameters)
        {
            string recipients = (string)parameters[0];
            string emailSubject = (string)parameters[1];
            string emailBody = (string)parameters[2];
            string siteName = (string)parameters[3];
            string fromEmailAddress = (string)parameters[4];
            string plainTextBody = (string)parameters[5];

            string from = fromEmailAddress;
            if (string.IsNullOrEmpty(from))
            {
                from = EmailHelper.Settings.NoReplyAddress(siteName);
            }

            EmailMessage message = new EmailMessage
            {
                From = from,
                Recipients = recipients,
                Subject = emailSubject,
                Body = emailBody,
                PlainTextBody = plainTextBody
            };

            // Send the e-mail
            EmailSender.SendEmail(siteName, message);

            return null;
        }
        
        #endregion
    }
}
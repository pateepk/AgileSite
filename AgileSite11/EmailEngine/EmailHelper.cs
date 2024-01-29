using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.EmailEngine
{
    /// <summary>
    /// E-mail utility methods.
    /// </summary>
    public static class EmailHelper
    {
        private static bool? mDebugEmails;
        private static bool? mSimulateFailedEmails;
        private static string mLogFile;
        private static bool? mLogEmails;
        private static TransferEncoding? mTransferEncoding;
        private static int? mSendLimit;


        #region "Nested classes"

        /// <summary>
        /// Contains simple services for queue management actions callable from UI.
        /// </summary>
        public static class Queue
        {
            #region "Properties"

            /// <summary>
            /// Gets a value indicating whether there are active sending threads.
            /// </summary>
            /// <value><c>true</c> if sending is in progress, otherwise, <c>false</c>.</value>
            public static bool SendingInProgess
            {
                get
                {
                    return ThreadSender.Instance.Sending;
                }
            }

            #endregion


            #region "Methods"

            /// <summary>
            /// Deletes an e-mail with the specified ID.
            /// </summary>
            /// <param name="emailId">ID of e-mail messages to delete</param>
            public static void Delete(int emailId)
            {
                EmailInfoProvider.DeleteEmailInfo(emailId);
            }


            /// <summary>
            /// Deletes e-mails with the specified IDs.
            /// </summary>
            /// <param name="emailIds">IDs of e-mail messages to delete</param>
            public static void Delete(int[] emailIds)
            {
                if (emailIds == null || emailIds.Length == 0)
                {
                    return;
                }

                foreach (int emailId in emailIds)
                {
                    EmailInfoProvider.DeleteEmailInfo(emailId);
                }
            }


            /// <summary>
            /// Deletes all e-mails whose delivery failed.
            /// </summary>
            /// <param name="siteId">Site ID</param>
            public static void DeleteAllFailed(int siteId)
            {
                EmailInfoProvider.DeleteAllFailed(siteId);
            }


            /// <summary>
            /// Deletes all e-mails waiting in the queue.
            /// </summary>
            /// <param name="siteId">Site ID</param>
            public static void DeleteAll(int siteId)
            {
                EmailInfoProvider.DeleteAll(siteId, EmailStatusEnum.Waiting, false);
            }


            /// <summary>
            /// Deletes all archived e-mails.
            /// </summary>
            /// <param name="siteId">Site ID</param>
            public static void DeleteArchived(int siteId)
            {
                EmailInfoProvider.DeleteAll(siteId, EmailStatusEnum.Archived, false);
            }


            /// <summary>
            /// Sends an e-mail with the specified ID.
            /// </summary>
            /// <param name="emailId">ID of the e-mail message to send</param>
            public static void Send(int emailId)
            {
                Send(new [] { emailId });
            }


            /// <summary>
            /// Sends e-mail messages with the specified IDs.
            /// </summary>
            /// <param name="emailIds">IDs of e-mail messages to send</param>
            public static void Send(int[] emailIds)
            {
                if (emailIds == null || emailIds.Length == 0)
                {
                    return;
                }

                ThreadSender.Instance.Send(emailIds);
            }


            /// <summary>
            /// Sends all e-mails whose delivery failed for given <paramref name="siteId"/>.
            /// </summary>            
            public static void SendAllFailed(int siteId)
            {
                ThreadSender.Instance.SendAllFailed(siteId);
            }


            /// <summary>
            /// Sends all e-mails whose delivery failed.
            /// </summary>            
            public static void SendAllFailed()
            {
                ThreadSender.Instance.Send(EmailMailoutEnum.Failed);
            }


            /// <summary>
            /// Sends all e-mails for given <paramref name="siteId"/>.
            /// </summary>
            public static void SendAll(int siteId)
            {
                ThreadSender.Instance.SendAll(siteId);
            }


            /// <summary>
            /// Sends all e-mails waiting in the queue.
            /// </summary>            
            public static void SendAll()
            {
                ThreadSender.Instance.Send(EmailMailoutEnum.All);
            }


            /// <summary>
            /// Sends all e-mails waiting in the queue. Also sends all failed emails, which are newer than <see cref="DateTime"/>.
            /// </summary>            
            public static void SendScheduledAndFailed(DateTime failedEmailsNewerThan)
            {
                ThreadSender.Instance.Send(EmailMailoutEnum.All, failedEmailsNewerThan);
            }


            /// <summary>
            /// Runs the e-mail queue mailout in a separate thread.
            /// </summary>
            /// <returns>An information message that describes the problem during sending if there was one</returns>
            public static string SendScheduled()
            {
                ThreadSender.Instance.Send(EmailMailoutEnum.New);

                return string.Empty;
            }


            /// <summary>
            /// Cancel currently running mailout.
            /// </summary>
            public static void CancelSending()
            {
                ThreadSender.Instance.StopSend();
            }

            #endregion
        }


        /// <summary>
        /// Contains settings retrieval methods.
        /// </summary>
        public static class Settings
        {
            #region "Methods"

            /// <summary>
            /// Gets whether e-mail sending is enabled for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>true if e-mail sending is enabled, otherwise false</returns>
            public static bool EmailsEnabled(string siteName)
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSEmailsEnabled", siteName);
            }


            /// <summary>
            /// Gets whether e-mail queue is enabled for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>true if e-mail queue is enabled, otherwise false</returns>
            public static bool EmailQueueEnabled(string siteName)
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSEmailQueueEnabled", siteName);
            }


            /// <summary>
            /// Gets the number of days the e-mails should be kept in archive for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>Number of days the e-mails should stay in the archive</returns>
            public static int ArchiveEmails(string siteName)
            {
                return SettingsKeyInfoProvider.GetIntValue("CMSArchiveEmails", siteName);
            }


            /// <summary>
            /// Gets the e-mail format for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>E-mail format</returns>
            public static string EmailFormat(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSEmailFormat", siteName);
            }


            /// <summary>
            /// Gets the e-mail encoding to use for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>E-mail encoding</returns>
            public static string EmailEncoding(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSEmailEncoding", siteName);
            }


            /// <summary>
            /// Gets the e-mail address for automated e-mail messages that should be user as a sender.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>An e-mail address to use as a sender for automated messages</returns>
            public static string NoReplyAddress(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSNoreplyEmailAddress", siteName);
            }


            /// <summary>
            /// Gets the e-mail address where the error notifications are sent.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>An e-mail address to use error notifications</returns>
            public static string ErrorNotificationAddress(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSSendErrorNotificationTo", siteName);
            }


            /// <summary>
            /// Gets the e-mail address for error notifications that should be used as a sender for given site.
            /// If <paramref name="siteName"/> is null global e-mail address is used.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>An e-mail address to use as a sender of error notifications</returns>
            public static string NotificationsSenderAddress(string siteName = null)
            {
                return SettingsKeyInfoProvider.GetValue("CMSSendEmailNotificationsFrom", siteName);
            }


            /// <summary>
            /// Gets the name of the default SMTP server for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>Name of the default SMTP server (in the settings)</returns>
            public static string ServerName(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSSMTPServer", siteName);
            }


            /// <summary>
            /// Gets the user name for the default SMTP server for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>User name for the default SMTP server (in the settings)</returns>
            public static string ServerUserName(string siteName)
            {
                return SettingsKeyInfoProvider.GetValue("CMSSMTPServerUser", siteName);
            }


            /// <summary>
            /// Gets the password for the default SMTP server for the specified site in either plain text or encrypted form.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <param name="encrypted">Whether to return the password in plain text or encrypted form.</param>
            /// <returns>Password for the default SMTP server (in the settings).</returns>
            public static string ServerPassword(string siteName, bool encrypted = false)
            {
                var encryptedPassword = SettingsKeyInfoProvider.GetValue("CMSSMTPServerPassword", siteName);

                return (encrypted) ? encryptedPassword : EncryptionHelper.DecryptData(encryptedPassword);
            }


            /// <summary>
            /// Gets whether connection to the default SMTP server should use SSL for the specified site.
            /// </summary>
            /// <param name="siteName">Name of the site</param>
            /// <returns>Whether to use SSL or not when communication with the default SMTP server (in the settings)</returns>
            public static bool ServerUseSSL(string siteName)
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseSSL", siteName);
            }


            /// <summary>
            /// Gets the number of e-mails to fetch in one batch.
            /// </summary>
            /// <returns>Batch size</returns>
            public static int BatchSize()
            {
                return SettingsKeyInfoProvider.GetIntValue("CMSEmailBatchSize");
            }

            #endregion
        }

        #endregion


        #region "E-mail constants"

        /// <summary>
        /// E-mail used as default sender
        /// </summary>
        public const string DEFAULT_EMAIL_SENDER = "no-reply@localhost.local";


        /// <summary>
        /// Template which is used to create regular expression to match image metafile source in HTML
        /// </summary>
        internal const string IMAGE_SRC_REGEXP_STRING = "src=\"([^\"]+(?:fileguid={0}|/getmetafile/{0})[^\"]*)\"";

        
        /// <summary>
        /// Template which is used to replace URL in image metafile source with content identifier
        /// </summary>
        internal const string INLINE_SRC_REPLACE_STRING = "src=\"cid:{0}\"";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets if the debug mode is active. Default value is false.
        /// If the debug mode is on, the e-mail sending thread sleeps for 500-1000ms instead of actually sending the e-mail.
        /// The setting may be changed via web.config app-key "CMSDebugEmails".
        /// </summary>
        public static bool DebugEmails
        {
            get
            {
                if (mDebugEmails == null)
                {
                    mDebugEmails = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugEmails"], false);
                }

                return mDebugEmails.Value;
            }
        }


        /// <summary>
        /// Gets if the simulation of send failures is active during debug mode (see <see cref="DebugEmails"/> property). Default value is false.
        /// If true, and exception is thrown when trying to send an e-mail.
        /// The setting may be changed via web.config app-key "CMSSimulateFailedEmails".
        /// </summary>
        public static bool SimulateFailedEmails
        {
            get
            {
                if (mSimulateFailedEmails == null)
                {
                    mSimulateFailedEmails = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSimulateFailedEmails"], false);
                }

                return mSimulateFailedEmails.Value;
            }
        }


        /// <summary>
        /// Path to the log file if logging is enabled, see <see cref="LogEmails"/> property.
        /// </summary>
        public static string LogFile
        {
            get
            {
                if (mLogFile == null)
                {
                    var file = SystemContext.WebApplicationPhysicalPath + "\\App_Data\\logemails.log";
                    DirectoryHelper.EnsureDiskPath(file, SystemContext.WebApplicationPhysicalPath);

                    mLogFile = file;
                }

                return mLogFile;
            }
        }


        /// <summary>
        /// If true, sent e-mails are logged to the file. Default value is false.
        /// The setting may be changed via web.config app-key "CMSLogEmails".
        /// </summary>
        public static bool LogEmails
        {
            get
            {
                if (mLogEmails == null)
                {
                    mLogEmails = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogEmails"], DebugHelper.LogEverythingToFile);
                }

                return mLogEmails.Value;
            }
        }


        /// <summary>
        /// E-mail transfer encoding. Default value is TransferEncoding.Base64.
        /// The setting may be changed via web.config app-key "CMSEmailTransferEncoding".
        /// </summary>
        public static TransferEncoding TransferEncoding
        {
            get
            {
                if (mTransferEncoding == null)
                {
                    mTransferEncoding = GetTransferEncoding(SettingsHelper.AppSettings["CMSEmailTransferEncoding"], TransferEncoding.Base64);
                }

                return mTransferEncoding.Value;
            }
        }


        /// <summary>
        /// Indicates the number of mail-outs after which the connection to a SMTP server is closed. Default value is 50.
        /// The setting may be changed via web.config app-key "CMSEmailSendLimit".
        /// </summary>
        internal static int SendLimit
        {
            get
            {
                if (mSendLimit == null)
                {
                    mSendLimit = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSEmailSendLimit"], 50);
                }

                return mSendLimit.Value;
            }
        }

        #endregion
        

        #region "Public methods"

        /// <summary>
        /// Returns if e-mail sending is enabled in either global or any site settings.
        /// </summary>
        public static bool IsAnySendingEnabled()
        {
            // Get global setting
            bool result = Settings.EmailsEnabled(string.Empty);

            if (!result)
            {
                // Check settings of the sites
                var enabledSettings = SettingsKeyInfoProvider.GetSettingsKeys()
                    .Columns("KeyID")
                    .WhereNotNull("SiteID")
                    .WhereEquals("KeyName", "CMSEmailsEnabled")
                    .WhereEquals("KeyValue", "True");

                result = !DataHelper.DataSourceIsEmpty(enabledSettings);
            }

            return result;
        }


        /// <summary>
        /// Returns e-mail sender from template, if defined or specified sender.
        /// </summary>
        /// <param name="template">E-mail template</param>
        /// <param name="sender">Sender e-mail address</param>
        public static string GetSender(EmailTemplateInfo template, string sender)
        {
            if ((template != null) && !string.IsNullOrEmpty(template.TemplateFrom))
            {
                return template.TemplateFrom;
            }

            return sender;
        }


        /// <summary>
        /// Returns e-mail subject from template, if defined or default subject.
        /// </summary>
        /// <param name="template">E-mail template</param>
        /// <param name="subject">E-mail subject</param>
        public static string GetSubject(EmailTemplateInfo template, string subject)
        {
            if ((template != null) && !string.IsNullOrEmpty(template.TemplateSubject))
            {
                return template.TemplateSubject;
            }
            
            return subject;
        }


        /// <summary>
        /// Gets the transfer encoding enumeration.
        /// </summary>
        /// <param name="encoding">Encoding as a string (Base64, SevenBit, QuotedPrintable)</param>
        /// <param name="defaultValue">Default value</param>
        public static TransferEncoding GetTransferEncoding(string encoding, TransferEncoding defaultValue)
        {
            if (encoding == null)
            {
                return defaultValue;
            }

            switch (encoding.ToLowerInvariant())
            {
                case "base64":
                    return TransferEncoding.Base64;

                case "sevenbit":
                    return TransferEncoding.SevenBit;

                case "quotedprintable":
                    return TransferEncoding.QuotedPrintable;

                default:
                    return defaultValue;
            }
        }


        /// <summary>
        /// Gets the e-mail format for the site given its site ID.
        /// </summary>
        /// <param name="siteId">The site ID</param>
        /// <returns>E-mail format selected for the site in the settings</returns>
        public static EmailFormatEnum GetEmailFormat(int siteId)
        {
            return GetEmailFormat(GetSiteName(siteId));
        }


        /// <summary>
        /// Gets the e-mail format for the site given its site name.
        /// </summary>
        /// <param name="siteName">The site name</param>
        /// <returns>E-mail format selected for the site in the settings</returns>
        public static EmailFormatEnum GetEmailFormat(string siteName)
        {
            string format = Settings.EmailFormat(siteName);

            if (string.IsNullOrEmpty(format))
            {
                return EmailFormatEnum.Html;
            }

            switch (format.ToLowerInvariant())
            {
                case "html":
                    return EmailFormatEnum.Html;

                case "plaintext":
                    return EmailFormatEnum.PlainText;

                case "both":
                    return EmailFormatEnum.Both;

                default:
                    return EmailFormatEnum.Html;
            }
        }


        /// <summary>
        /// Converts a <see cref="EmailPriorityEnum" /> value to <see cref="MailPriority" /> value.
        /// </summary>
        /// <param name="priority">The priority</param>
        /// <returns>A <see cref="MailPriority" /> object</returns>
        public static MailPriority ToMailPriority(EmailPriorityEnum priority)
        {
            switch (priority)
            {
                case EmailPriorityEnum.Low:
                    return MailPriority.Low;

                case EmailPriorityEnum.Normal:
                    return MailPriority.Normal;

                case EmailPriorityEnum.High:
                    return MailPriority.High;

                default:
                    return MailPriority.Normal;
            }
        }


        /// <summary>
        /// Converts a <see cref="MailPriority" /> value to <see cref="EmailPriorityEnum" /> value.
        /// </summary>
        /// <param name="priority">The priority</param>
        /// <returns>A <see cref="MailPriority" /> object</returns>
        public static EmailPriorityEnum ToEmailPriority(MailPriority priority)
        {
            switch (priority)
            {
                case MailPriority.Low:
                    return EmailPriorityEnum.Low;

                case MailPriority.Normal:
                    return EmailPriorityEnum.Normal;

                case MailPriority.High:
                    return EmailPriorityEnum.High;

                default:
                    return EmailPriorityEnum.Normal;
            }
        }


        /// <summary>
        /// Returns ReplyTo e-mail address from message (internally handles .NET 4.0 backward compatibility).
        /// </summary>
        /// <param name="message">E-mail message</param>
        public static string GetReplyToEmailAddress(MailMessage message)
        {
            if ((message == null) || (message.ReplyToList.Count == 0))
            {
                return String.Empty;
            }

            return message.ReplyToList[0].ToString();
        }


        /// <summary>
        /// Sets ReplyTo e-mail address of the given message (internally handles .NET 4.0 backward compatibility).
        /// </summary>
        /// <param name="message">E-mail message</param>
        /// <param name="replyTo">Reply to add</param>
        public static void SetReplyToEmailAddress(MailMessage message, string replyTo)
        {
            if (message == null)
            {
                return;
            }

            if (message.ReplyToList.Count > 0)
            {
                message.ReplyToList.Clear();
            }

            Fill(message.ReplyToList, replyTo);
        }


        /// <summary>
        /// Appends all attached files to given email.
        /// Replaces link 'src="...GetMetaFile.aspx?...fileguid=..."' or 'src=".../GetMetaFile/..." with link to e-mail in-line attachments (only for images).
        /// </summary>
        /// <param name="message">E-mail message</param>
        /// <param name="objectId">ID of attached meta-file</param>
        /// <param name="objectType">Type of meta-file</param>
        /// <param name="category">Meta-file category</param>
        public static void ResolveMetaFileImages(EmailMessage message, int objectId, string objectType, string category)
        {
            if (objectId > 0)
            {
                // Get all meta-files associated to object
                var metafiles = MetaFileInfoProvider.GetMetaFiles(objectId, objectType, category, null, null);
                
                // Append files from the list to the email
                ResolveMetaFileImages(message, metafiles);
            }
        }


        /// <summary>
        /// Appends all attached files to given email.
        /// Replaces link 'src="...GetMetaFile.aspx?...fileguid=..."' or 'src=".../GetMetaFile/..." with link to e-mail in-line attachments (only for images).
        /// </summary>
        /// <param name="message">E-mail message</param>
        /// <param name="metafiles">List with MetaFileInfo objects</param>
        public static void ResolveMetaFileImages(EmailMessage message, IEnumerable<MetaFileInfo> metafiles)
        {
            if ((message == null) || (metafiles == null))
            {
                return;
            }

            foreach (var mfi in metafiles)
            {
                if (mfi == null)
                {
                    continue;
                }

                // Ensure file binary data
                var binaryData = mfi.Generalized.EnsureBinaryData();
                if (binaryData != null)
                {
                    // Prepare email attachment
                    var ms = new SystemIO.MemoryStream(binaryData);
                    EmailAttachment att = new EmailAttachment(ms, mfi.MetaFileName, mfi.MetaFileGUID, mfi.MetaFileLastModified, mfi.MetaFileSiteID);

                    // Is it a image? => replace link
                    if ((mfi.MetaFileImageHeight > 0) && (mfi.MetaFileImageWidth > 0) && (!string.IsNullOrEmpty(message.Body)))
                    {
                        // Regular expression to search the metafile source in HTML code
                        Regex mfRegExp = RegexHelper.GetRegex(string.Format(IMAGE_SRC_REGEXP_STRING, mfi.MetaFileGUID), true);

                        if (mfRegExp.IsMatch(message.Body))
                        {
                            att.ContentDisposition.Inline = true;
                            att.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                            att.ContentId = mfi.MetaFileGUID.ToString().Replace("-", String.Empty);
                            att.ContentType.MediaType = mfi.MetaFileMimeType;
                            att.ContentType.Name = Path.GetFileName(mfi.MetaFileName);
                            att.TransferEncoding = TransferEncoding;

                            message.Body = mfRegExp.Replace(message.Body, string.Format(INLINE_SRC_REPLACE_STRING, att.ContentId));
                        }
                    }

                    message.Attachments.Add(att);
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Logs the e-mail message.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void LogEmail(MailMessage message)
        {
            if (string.IsNullOrEmpty(LogFile))
            {
                return;
            }

            // Build the log text
            string logText = string.Format("Timestamp: {0}{1}", DateTime.Now.ToString(), Environment.NewLine);

            logText += string.Format("Rcpt: {0}{1}", Merge(message.To), Environment.NewLine);

            if (message.CC.Count != 0)
            {
                logText += string.Format("Cc: {0}{1}", Merge(message.CC), Environment.NewLine);
            }

            if (message.Bcc.Count != 0)
            {
                logText += string.Format("Bcc: {0}{1}", Merge(message.Bcc), Environment.NewLine);
            }
            
            logText += string.Format("From: {0}{1}", message.From, Environment.NewLine);
            logText += string.Format("Subject: {0}{1}", message.Subject, Environment.NewLine);
            
            if (message.Headers.Count != 0)
            {
                logText += string.Format("Headers:{0}", Environment.NewLine);
                
                foreach (string headerName in message.Headers)
                {
                    logText += string.Format("    {0}: {1}{2}", headerName, message.Headers[headerName], Environment.NewLine);
                }
            }

            // If HeadersEncoding is null then UTF-8 is default (Kentico documentation)
            var messageEncoding = message.HeadersEncoding == null ? Encoding.UTF8.HeaderName : message.HeadersEncoding.HeaderName;
            logText += string.Format("HeadersEncoding: {0}{1}", messageEncoding, Environment.NewLine);

            logText += Environment.NewLine;

            try
            {
                // Log directly to the file
                File.AppendAllText(LogFile, logText);
            }
            catch { }
        }


        /// <summary>
        /// Logs e-mails and simulates failures if enabled.
        /// </summary>        
        /// <param name="message">E-mail message to process</param>
        /// <returns>true, if debugging was performed</returns>
        internal static bool DebugEmail(MailMessage message)
        {
            // Log the e-mail
            if (LogEmails)
            {
                LogEmail(message);
            }

            // Testing code - writes record into EventLog and waits for some time
            if (DebugEmails)
            {
                EventLogProvider.LogEvent(EventType.INFORMATION, "Mail", Merge(message.To));

                // Wait for 500 to 1000 milliseconds
                Random rnd = new Random();
                Thread.Sleep(500 + rnd.Next(500));

                // Throw exception occasionally
                if (rnd.Next(100) == 0)
                {
                    throw new Exception("Sending failed for " + Merge(message.To));
                }
                return true;
            }

            // Simulate failed e-mail
            if (SimulateFailedEmails)
            {
                throw new Exception(string.Format("Sending failed for {0} [Simulated error].", Merge(message.To)));
            }

            return false;
        }


        /// <summary>
        /// Sets the mail message body.
        /// </summary>
        /// <param name="message">The mail message whose body should be set</param>
        /// <param name="emailFormat">The e-mail format of the message</param>
        /// <param name="htmlBody">HTML body</param>
        /// <param name="plainTextBody">Plain text body</param>
        /// <param name="siteName">Site name to determine content encoding from Settings</param>
        /// <param name="attachments">Collection of attachments that should be included</param>
        internal static void SetEmailBody(MailMessage message, EmailFormatEnum emailFormat, string htmlBody, string plainTextBody, string siteName, IEnumerable<Attachment> attachments)
        {
            // Get content encoding
            Encoding encoding = GetEncoding(siteName);

            // Get e-mail content in specific format
            if (emailFormat == EmailFormatEnum.PlainText || (emailFormat == EmailFormatEnum.Both && !String.IsNullOrEmpty(plainTextBody)))
            {
                // Discussion macro helper initialization - used to resolve plain text
                var dmHelper = new DiscussionMacroResolver { ResolveToPlainText = true };

                // Add plain alternate view to the message
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(dmHelper.ResolveMacros(plainTextBody), encoding, MediaTypeNames.Text.Plain);
                plainView.TransferEncoding = TransferEncoding;
                message.AlternateViews.Add(plainView);
            }

            if (emailFormat == EmailFormatEnum.Html || emailFormat == EmailFormatEnum.Both)
            {
                // Add html alternate view to the message
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, encoding, MediaTypeNames.Text.Html);
                htmlView.TransferEncoding = TransferEncoding;

                // Add attachments as standard attachments or linked resources
                foreach (var attachment in attachments)
                {
                    if (attachment.ContentDisposition.Inline)
                    {
                        // Add linked resource
                        htmlView.LinkedResources.Add(GetLinkedResource(attachment));
                    }
                    else
                    {
                        // Add standard attachment
                        message.Attachments.Add(attachment);
                    }
                }

                message.AlternateViews.Add(htmlView);
            }
            else
            {
                // Add standard attachments
                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }
            }
        }


        /// <summary>
        /// Returns <see cref="LinkedResource"/> from given <paramref name="attachment"/>.
        /// </summary>
        /// <param name="attachment">Email attachment</param>
        private static LinkedResource GetLinkedResource(AttachmentBase attachment)
        {
            var linkedResource = new LinkedResource(attachment.ContentStream, attachment.ContentType)
            {
                ContentId = attachment.ContentId,
                TransferEncoding = attachment.TransferEncoding
            };

            return linkedResource;
        }


        /// <summary>
        /// Gets the e-mail format and if the format is default, substitutes its value with the one in settings.
        /// </summary>
        /// <param name="emailFormat">The e-mail format</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Resolved e-mail format</returns>
        internal static EmailFormatEnum ResolveEmailFormat(EmailFormatEnum emailFormat, int siteId)
        {
            return emailFormat != EmailFormatEnum.Default ? emailFormat : GetEmailFormat(siteId);
        }


        /// <summary>
        /// Gets the e-mail format and if the format is default, substitutes its value with the one in settings.
        /// </summary>
        /// <param name="emailFormat">The e-mail format</param>
        /// <param name="siteName">Name of the site</param>
        /// <returns>Resolved e-mail format</returns>
        internal static EmailFormatEnum ResolveEmailFormat(EmailFormatEnum emailFormat, string siteName)
        {
            return emailFormat != EmailFormatEnum.Default ? emailFormat : GetEmailFormat(siteName);
        }


        /// <summary>
        /// Gets the encoding for e-mail message. If encoding is not specified explicitly, then either default or UTF-8 is used.
        /// </summary>
        /// <param name="siteName">Site name</param>        
        /// <returns>E-mail message encoding</returns>
        internal static Encoding GetEncoding(string siteName)
        {
            string encodingName = Settings.EmailEncoding(siteName);

            if (!string.IsNullOrEmpty(encodingName))
            {
                try
                {
                    return Encoding.GetEncoding(encodingName);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("EmailEngine", "EmailHelper", ex);
                }
            }

            return Encoding.UTF8;
        }


        /// <summary>
        /// Fills a collection of mail addresses with a string of addresses.
        /// </summary>
        /// <param name="mailAddresses">Collection of mail addresses</param>
        /// <param name="addresses">Email addresses separated by semicolon</param>
        internal static void Fill(MailAddressCollection mailAddresses, string addresses)
        {
            if ((mailAddresses == null) || String.IsNullOrWhiteSpace(addresses))
            {
                return;
            }

            string[] choppedAddresses = addresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Do not change to mailAddresses.Add(string addresses) as MailAddress constructor accepts even mail addresses with special characters
            Array.ForEach(choppedAddresses, address => mailAddresses.Add(new MailAddress(address)));
        }


        /// <summary>
        /// Merges the specified mail addresses to a semi-colon separated string of addresses.
        /// </summary>
        /// <param name="mailAddresses">The mail addresses collection</param>
        /// <returns>string with e-mail addresses</returns>
        internal static string Merge(MailAddressCollection mailAddresses)
        {
            if (mailAddresses == null)
            {
                return string.Empty;
            }

            return String.Join(";", mailAddresses);
        }

        #endregion


        #region "Site name/ID conversion methods"

        /// <summary>
        /// Converts site ID to site name.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>Name of the site with the specified ID or empty string if not found</returns>
        internal static string GetSiteName(int siteId)
        {
            if (siteId <= 0)
            {
                return string.Empty;
            }

            // Get site name
            return ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);
        }


        /// <summary>
        /// Converts site name to site ID.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>ID of the site with the specified name or 0 if not found</returns>
        internal static int GetSiteId(string siteName)
        {
            // Get site ID
            return ProviderHelper.GetId(PredefinedObjectType.SITE, siteName);
        }

        #endregion
    }
}
using System;
using System.Data;
using System.Security.Principal;

using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.MacroEngine;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Handles sending message board e-mails in separated thread.
    /// </summary>
    public class ThreadEmailSender
    {
        #region "Private variables"

        private string mUnsubscriptionUrl = string.Empty;


        private string mSiteName = string.Empty;


        private static WindowsIdentity mWindowsIdentity;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets or sets the e-mail message to board moderators.
        /// </summary>
        private EmailMessage EmailToModerators
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets e-mail message to board subscribers.
        /// </summary>
        private EmailMessage EmailToSubscribers
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the dataset that holds the information on subscribers e-mail is being sent to.
        /// </summary>
        private DataSet Subscribers
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the site the message board is placed on.
        /// </summary>
        private string SiteName
        {
            get
            {
                if ((mSiteName == string.Empty) && (BoardObj != null))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardObj.BoardSiteID);
                    if (si != null)
                    {
                        mSiteName = si.SiteName;
                    }
                }
                return mSiteName;
            }
        }


        /// <summary>
        /// Gets or sets the macro resolver to use.
        /// </summary>
        private MacroResolver Resolver
        {
            get;
            set;
        }


        /// <summary>
        /// Resolver to use for subscribers notifications.c
        /// </summary>
        private MacroResolver SubscribersResolver
        {
            get;
            set;
        }


        /// <summary>
        /// Message board info.
        /// </summary>
        private BoardMessageInfo MessageObj
        {
            get;
            set;
        }


        /// <summary>
        /// Current board info.
        /// </summary>
        private BoardInfo BoardObj
        {
            get;
            set;
        }


        /// <summary>
        /// Resolved unsubscription URL.
        /// </summary>
        private string UnsubscriptionURL
        {
            get
            {
                return mUnsubscriptionUrl;
            }
            set
            {
                mUnsubscriptionUrl = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - creates and initialize email sender.
        /// </summary>
        /// <param name="message">Board message info</param>
        public ThreadEmailSender(BoardMessageInfo message)
        {
            // Initialize required data
            InitSender(message);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Init sender private data and resolver.
        /// </summary>
        private void InitSender(BoardMessageInfo message)
        {
            if (message == null)
            {
                return;
            }

            MessageObj = message;
            BoardObj = BoardInfoProvider.GetBoardInfo(MessageObj.MessageBoardID);

            if (BoardObj == null)
            {
                return;
            }

            // Create macro resolver
            Resolver = CreateMacroResolver();
            // Create separate resolver for subscribers notification
            SubscribersResolver = CreateMacroResolver();

            // Get unsubscription URL
            UnsubscriptionURL = BoardSubscriptionInfoProvider.GetUnsubscriptionUrl(null, BoardObj, null);
        }


        /// <summary>
        /// Creates and initializes macro resolver.
        /// </summary>
        private MacroResolver CreateMacroResolver()
        {
            // Init resolver
            MacroResolver resolver = MacroContext.CurrentResolver;
            resolver.SetNamedSourceData("Board", BoardObj);
            resolver.SetNamedSourceData("Message", MessageObj);

            UserInfo userInfo = UserInfoProvider.GetUserInfo(MessageObj.MessageUserID);
            if (userInfo != null)
            {
                resolver.SetNamedSourceData("MessageUser", userInfo);
                resolver.SetNamedSourceData("MessageUserSettings", userInfo.UserSettings);
            }
            else
            {
                resolver.SetNamedSourceData("MessageUser", new UserInfo());
                resolver.SetNamedSourceData("MessageUserSettings", new UserSettingsInfo());
            }

            // Add macros for document and unsubscription link
            resolver.SetNamedSourceData("DocumentLink", BoardInfoProvider.GetMessageBoardUrl(BoardObj, SiteName));

            return resolver;
        }


        /// <summary>
        /// Returns initialized e-mail message without recipients and with unresolved macros.
        /// </summary>
        /// <param name="toSubscribers">True - email message for subscribers is returned, False - email message for moderators is returned</param>        
        private EmailMessage GetEmailMessage(bool toSubscribers)
        {
            string templateName;
            string subject;

            // For subscribers
            if (toSubscribers)
            {
                templateName = "Boards.NotificationToSubscribers";
                subject = ResHelper.GetAPIString("boards.newmessagenotification", "New message notification");
            }
            // For board moderators
            else
            {
                templateName = "Boards.NotificationToModerators";
                subject = ResHelper.GetAPIString("boards.newmessageapproval", "New message approval");
            }

            // Get e-mail template
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate(templateName, SiteName);
            if (template != null)
            {
                // Get sender address for given template
                string from = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSSendBoardEmailsFrom");
                from = EmailHelper.GetSender(template, from);

                if (!string.IsNullOrEmpty(from))
                {
                    // Initialize e-mail message
                    EmailMessage email = new EmailMessage
                    {
                        EmailFormat = EmailFormatEnum.Default,
                        From = from,
                        Subject = EmailHelper.GetSubject(template, subject),
                        Recipients = string.Empty,
                        CcRecipients = template.TemplateCc,
                        BccRecipients = template.TemplateBcc,
                        ReplyTo = template.TemplateReplyTo,
                        Body = template.TemplateText,
                        PlainTextBody = template.TemplatePlainText
                    };

                    // Add attachments
                    EmailHelper.ResolveMetaFileImages(email, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                    return email;
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "MessageBoards", "EmailSenderNotSpecified");
                }
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, "MessageBoards", "EmailTemplateNotFound");
            }

            return null;
        }


        /// <summary>
        /// Sends a notification e-mail to all board subscribers.
        /// </summary>
        private void SendNotificationToSubscribers()
        {
            if (MessageObj == null)
            {
                return;
            }

            // Get initialized email 
            EmailToSubscribers = GetEmailMessage(true);
            if (EmailToSubscribers != null)
            {
                // Get subscribers
                string where = "((SubscriptionApproved = 1) OR (SubscriptionApproved IS NULL)) AND (SubscriptionBoardID =" + MessageObj.MessageBoardID + ")";
                Subscribers = BoardSubscriptionInfoProvider.GetSubscriptions(where, null, "SubscriptionEmail, SubscriptionGUID, (SELECT PreferredCultureCode FROM CMS_User WHERE UserID = SubscriptionUserID) AS PreferredCultureCode");

                // Send emails
                CMSThread thread = new CMSThread(SendToSubscribers);

                thread.Start();
            }
        }


        /// <summary>
        /// Sends a notification e-mail to all board moderators.
        /// </summary>
        private void SendNotificationToModerators()
        {
            EmailToModerators = GetEmailMessage(false);
            if (EmailToModerators == null)
            {
                return;
            }

            // Get moderators
            var userModerators = UserInfoProvider.GetUsers()
                    .Column("Email")
                    .WhereIn("UserID", new IDQuery<BoardModeratorInfo>("UserID")
                                                        .WhereEquals("BoardID", BoardObj.BoardID));

            foreach (var user in userModerators)
            {
                string email = user.Email;
                if (ValidationHelper.IsEmail(email))
                {
                    EmailToModerators.Recipients += email + ";";
                }
            }

            // Send email
            CMSThread thread = new CMSThread(SendToModerators);

            thread.Start();
        }


        /// <summary>
        /// Sends e-mails to moderators.
        /// </summary>
        private void SendToModerators()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                // To moderators
                if ((EmailToModerators != null) && (EmailToModerators.Recipients.Trim(';') != string.Empty))
                {
                    // Resolve email macros
                    ResolveEmailMacros(EmailToModerators);

                    EmailSender.SendEmail(SiteName, EmailToModerators);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MessageBoards", "EmailToModerators", ex);
            }
            finally
            {
                // Undo impersonation
                if (ctx != null)
                {
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Sends e-mails to subscribers.
        /// </summary>
        private void SendToSubscribers()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                // To subscribers
                if ((EmailToSubscribers != null) && !DataHelper.DataSourceIsEmpty(Subscribers))
                {
                    // Store original template texts (with macros)
                    string templateSubject = EmailToSubscribers.Subject;
                    string templatePlainText = EmailToSubscribers.PlainTextBody;
                    string templateBody = EmailToSubscribers.Body;

                    // Store original resolver culture
                    string originalCulture = SubscribersResolver.Culture;

                    // Go through the subscribers and send e-mail
                    foreach (DataRow dr in Subscribers.Tables[0].Rows)
                    {
                        string subscriberEmail = ValidationHelper.GetString(dr["SubscriptionEmail"], string.Empty);
                        if (ValidationHelper.IsEmail(subscriberEmail))
                        {
                            // Add unsubscription link to resolver
                            string subscriberGuid = ValidationHelper.GetString(dr["SubscriptionGUID"], string.Empty);
                            string subscriberCulture = ValidationHelper.GetString(dr["PreferredCultureCode"], null);
                            BoardSubscriptionInfo bsi = BoardSubscriptionInfoProvider.GetBoardSubscriptionInfo(new Guid(subscriberGuid));

                            SubscribersResolver.Culture = subscriberCulture;
                            SubscribersResolver.SetNamedSourceData("UnsubscriptionLink", BoardSubscriptionInfoProvider.GetUnsubscriptionUrl(bsi, SubscribersResolver.GetNamedSourceData("Board") as BoardInfo, UnsubscriptionURL));

                            // Resolve macros uniquely for each subscriber
                            SubscribersResolver.Settings.EncodeResolvedValues = false;
                            EmailToSubscribers.Subject = SubscribersResolver.ResolveMacros(templateSubject);
                            EmailToSubscribers.PlainTextBody = SubscribersResolver.ResolveMacros(templatePlainText);

                            SubscribersResolver.Settings.EncodeResolvedValues = true;
                            EmailToSubscribers.Body = SubscribersResolver.ResolveMacros(templateBody);

                            // Set recipient
                            EmailToSubscribers.Recipients = subscriberEmail;

                            EmailSender.SendEmail(SiteName, EmailToSubscribers);
                        }
                    }

                    // Restore original resolver culture
                    SubscribersResolver.Culture = originalCulture;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MessageBoards", "EmailToSubscribers", ex);
            }
            finally
            {
                if (ctx != null)
                {
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Resolves e-mail subject, body and plain text body.
        /// </summary>
        /// <param name="email">Email to be resolved</param>
        private void ResolveEmailMacros(EmailMessage email)
        {
            // Disable macro encoding for plaintext body and subject
            Resolver.Settings.EncodeResolvedValues = false;
            email.Subject = Resolver.ResolveMacros(email.Subject);
            email.PlainTextBody = Resolver.ResolveMacros(email.PlainTextBody);

            // Enable macro encoding for body
            Resolver.Settings.EncodeResolvedValues = true;
            email.Body = Resolver.ResolveMacros(email.Body);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sends a notification e-mail to all board subscribers and to all board moderators.
        /// </summary>       
        /// <param name="wi">Windows identity</param>
        /// <param name="toSubscribers">Indicates if notification email should be sent to board subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to board moderators</param>        
        public void SendNewMessageNotification(WindowsIdentity wi, bool toSubscribers, bool toModerators)
        {
            if (BoardMessageInfoProvider.EnableEmails)
            {
                // Remember windows identity
                mWindowsIdentity = wi;

                if (toSubscribers)
                {
                    SendNotificationToSubscribers();
                }
                if (toModerators)
                {
                    SendNotificationToModerators();
                }
            }
        }

        #endregion
    }
}
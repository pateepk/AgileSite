using System;
using System.Data;
using System.Security.Principal;
using System.Text;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Blogs
{
    /// <summary>
    /// Handles sending message blog e-mails in separated thread.
    /// </summary>
    public class ThreadEmailSender
    {
        #region "Variables"

        private static WindowsIdentity mWindowsIdentity;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Blog comment information.
        /// </summary>
        private BlogCommentInfo CommentObj
        {
            get;
            set;
        }


        /// <summary>
        /// List of users' names. These users represents blog moderators.
        /// </summary>
        private string[] Moderators
        {
            get;
            set;
        }


        /// <summary>
        /// Blog owner e-mail address where the new comment notification should be sent.
        /// </summary>
        private string BlogOwnerEmail
        {
            get;
            set;
        }


        /// <summary>
        /// Holds the information on subscribers e-mail is being sent to.
        /// </summary>
        private DataSet Subscribers
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail message to blog moderators and blog owner.
        /// </summary>
        private EmailMessage EmailToModerators
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail message to blog subscribers.
        /// </summary>
        private EmailMessage EmailToSubscribers
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail message to blog owner.
        /// </summary>
        private EmailMessage EmailToOwner
        {
            get;
            set;
        }


        /// <summary>
        /// Resolver to use.
        /// </summary>
        private MacroResolver OwnerResolver
        {
            get;
            set;
        }


        /// <summary>
        /// Resolver to use for subscribers notifications.
        /// </summary>
        private MacroResolver SubscribersResolver
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the site the blog post is placed on.
        /// </summary>
        private string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Resolved unsubscription URL.
        /// </summary>
        private string UnsubscriptionURL
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - creates and initialize email sender.
        /// </summary>
        /// <param name="blogComment">Blog comment information</param>        
        public ThreadEmailSender(BlogCommentInfo blogComment)
        {
            BlogOwnerEmail = string.Empty;
            UnsubscriptionURL = string.Empty;
            SiteName = string.Empty;

            // Initialize required data
            InitSender(blogComment);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Init sender private data and resolver.
        /// </summary>
        private void InitSender(BlogCommentInfo blogComment)
        {
            if (blogComment == null)
            {
                return;
            }

            CommentObj = blogComment;

            // Get blog post info
            TreeProvider tp = new TreeProvider();
            TreeNode blogPost = tp.SelectSingleDocument(CommentObj.CommentPostDocumentID);
            TreeNode blog = BlogHelper.GetParentBlog(CommentObj.CommentPostDocumentID, true);

            if (blog == null || blogPost == null)
            {
                return;
            }

            UserInfo userInfo = UserInfoProvider.GetUserInfo(CommentObj.CommentUserID);

            // Create resolver for owner and moderators notification
            OwnerResolver = CreateMacroResolver(userInfo, blog, blogPost);

            // Create separate resolver for subscribers notification
            SubscribersResolver = CreateMacroResolver(userInfo, blog, blogPost);

            // Get site name
            SiteInfo si = SiteInfoProvider.GetSiteInfo(blog.NodeSiteID);
            if (si != null)
            {
                SiteName = si.SiteName;
            }

            // Use site settings if unsubscription URL is not defined in blog settings
            UnsubscriptionURL = BlogPostSubscriptionInfoProvider.GetUnsubscriptionUrl(null, blog, null, null);

            // Get blog moderators and owner email address
            Moderators = ValidationHelper.GetString(blog.GetValue("BlogModerators"), string.Empty).Split(';');
            BlogOwnerEmail = ValidationHelper.GetString(blog.GetValue("BlogSendCommentsToEmail"), string.Empty);
        }


        /// <summary>
        /// Creates and initializes macro resolver.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="blog">Blog node info object</param>
        /// <param name="blogPost">Blog post info object</param>
        private MacroResolver CreateMacroResolver(UserInfo user, TreeNode blog, TreeNode blogPost)
        {
            var resolver = BlogHelper.CreateMacroResolver(blog, blogPost);

            // Add data macros
            resolver.SetNamedSourceData("Comment", CommentObj);

            if (user != null)
            {
                resolver.SetNamedSourceData("CommentUser", user);
                resolver.SetNamedSourceData("CommentUserSettings", user.UserSettings);
            }
            else
            {
                resolver.SetNamedSourceData("CommentUser", new UserInfo());
                resolver.SetNamedSourceData("CommentUserSettings", new UserSettingsInfo());
            }

            // Add some macros due to back compatibility
            resolver.SetNamedSourceData("UserFullName", CommentObj.CommentUserName);
            resolver.SetNamedSourceData("CommentUrl", CommentObj.CommentUrl);
            resolver.SetNamedSourceData("Comments", CommentObj.CommentText);
            resolver.SetNamedSourceData("CommentDate", CommentObj.CommentDate.ToString());

            return resolver;
        }


        /// <summary>
        /// Returns initialized e-mail message without recipients and with unresolved macros.
        /// </summary>
        /// <param name="toSubscribers">Indicates if notification email should be sent to blog post subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to blog moderators</param>
        /// <param name="toOwner">Indicates if notification email should be sent to blog owner</param>
        private EmailMessage GetEmailMessage(bool toSubscribers, bool toModerators, bool toOwner)
        {
            string templateName = string.Empty;
            string subject = string.Empty;

            // For blog post subscribers
            if (toSubscribers)
            {
                templateName = "Blog.NotificationToSubcribers";
                subject = ResHelper.GetAPIString("blogs.emailsubject.tosubscribers", "New blog comment notification");
            }
            // For blog moderators
            else if (toModerators)
            {
                templateName = "Blog.NotificationToModerators";
                subject = ResHelper.GetAPIString("blogs.emailsubject.tomoderators", "New comment is waiting for your approval");
            }
            // For blog owner
            else if (toOwner)
            {
                templateName = "Blog.NewCommentNotification";
                subject = ResHelper.GetAPIString("blogs.emailsubject.toowner", "New comment was added to your blog post");
            }

            // Get e-mail template
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate(templateName, SiteName);
            if (template != null)
            {
                // Get sender address for given template
                var from = EmailHelper.GetSender(template, SettingsKeyInfoProvider.GetValue(SiteName + ".CMSSendBlogEmailsFrom"));
                if (!string.IsNullOrEmpty(from))
                {
                    // Initialize e-mail message
                    EmailMessage email = new EmailMessage
                    {
                        EmailFormat = EmailFormatEnum.Default,
                        From = from,
                        Subject = EmailHelper.GetSubject(template, subject),
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
                    EventLogProvider.LogEvent(EventType.ERROR, "Blogs", "EmailSenderNotSpecified");
                }
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, "Blogs", "EmailTemplateNotFound");
            }

            return null;
        }


        /// <summary>
        /// Sends e-mail notification to all blog post subscribers.
        /// </summary>
        private void SendNotificationToSubscribers()
        {
            // Get initialized email message
            EmailToSubscribers = GetEmailMessage(true, false, false);
            if (EmailToSubscribers == null)
            {
                return;
            }

            // Get subscribers
            string where = "((SubscriptionApproved = 1) OR (SubscriptionApproved IS NULL)) AND (SubscriptionPostDocumentID = " + CommentObj.CommentPostDocumentID + ")";
            Subscribers = BlogPostSubscriptionInfoProvider.GetBlogPostSubscriptions(where, null, "SubscriptionEmail, SubscriptionGUID, (SELECT PreferredCultureCode FROM CMS_User WHERE UserID = SubscriptionUserID) AS PreferredCultureCode");

            // Send email
            CMSThread thread = new CMSThread(SendToSubscribers);

            thread.Start();
        }


        /// <summary>
        /// Sends e-mail notification to blog owner.
        /// </summary>
        private void SendNotificationToOwner()
        {
            if (!ValidationHelper.IsEmail(BlogOwnerEmail))
            {
                return;
            }

            // Get initialized email message without recipients
            EmailToOwner = GetEmailMessage(false, false, true);
            if (EmailToOwner == null)
            {
                return;
            }

            EmailToOwner.Recipients += BlogOwnerEmail;

            // Send email
            CMSThread thread = new CMSThread(SendToOwner);

            thread.Start();
        }


        /// <summary>
        /// Sends e-mail notification to blog moderators.
        /// </summary>
        private void SendNotificationToModerators()
        {
            if (Moderators == null)
            {
                return;
            }

            // Get initialized email message without recipients
            EmailToModerators = GetEmailMessage(false, true, false);
            if (EmailToModerators == null)
            {
                return;
            }

            // Build where condition to get moderators and blog owner
            StringBuilder whereBuilder = new StringBuilder();

            foreach (string user in Moderators)
            {
                whereBuilder.AppendFormat("(UserName = N'{0}') OR ", SqlHelper.GetSafeQueryString(user, false));
            }

            string where = whereBuilder.ToString();

            if (where != string.Empty)
            {
                // Remove ending " OR "
                where = where.Substring(0, where.Length - 4);

                // Initialize email recipients
                var data = UserInfoProvider.GetUsers().Where(where).Columns("Email");
                foreach (var moderator in data)
                {
                    if (ValidationHelper.IsEmail(moderator.Email))
                    {
                        EmailToModerators.Recipients += moderator.Email + ";";
                    }
                }

                // Send email
                CMSThread thread = new CMSThread(SendToModerators);

                thread.Start();
            }
        }


        /// <summary>
        /// Sends e-mail to blog owner.
        /// </summary>
        private void SendToOwner()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                if ((EmailToOwner == null) || String.IsNullOrEmpty(EmailToOwner.Recipients))
                {
                    return;
                }

                ResolveEmailMacros(EmailToOwner);
                EmailSender.SendEmail(SiteName, EmailToOwner);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Blogs", "EmailToBlogOwner", ex);
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
        /// Sends e-mails to blog moderators.
        /// </summary>
        private void SendToModerators()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                if ((EmailToModerators != null) && (EmailToModerators.Recipients != null) && (EmailToModerators.Recipients.Trim(';') != ""))
                {
                    ResolveEmailMacros(EmailToModerators);
                    EmailSender.SendEmail(SiteName, EmailToModerators);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Blogs", "EmailToBlogModerators", ex);
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
        /// Sends e-mails to blog post subscribers.
        /// </summary>
        private void SendToSubscribers()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                // Send emails to subscribers
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
                        // Add unsubscription link ro resolver
                        string subscriberEmail = ValidationHelper.GetString(dr["SubscriptionEmail"], "");
                        string subscriberGuid = ValidationHelper.GetString(dr["SubscriptionGUID"], "");
                        string subscriberCulture = ValidationHelper.GetString(dr["PreferredCultureCode"], null);
                        BlogPostSubscriptionInfo bpsi = BlogPostSubscriptionInfoProvider.GetBlogPostSubscriptionInfo(new Guid(subscriberGuid));

                        SubscribersResolver.Culture = subscriberCulture;
                        SubscribersResolver.SetNamedSourceData("UnsubscriptionLink", UnsubscriptionURL + URLHelper.GetQuery(BlogPostSubscriptionInfoProvider.GetUnsubscriptionUrl(bpsi, SubscribersResolver.GetNamedSourceData("Blog") as TreeNode, SubscribersResolver.GetNamedSourceData("BlogPost") as TreeNode, UnsubscriptionURL)));

                        // Resolve macros uniquely for each subscriber
                        SubscribersResolver.Settings.EncodeResolvedValues = false;
                        EmailToSubscribers.Subject = SubscribersResolver.ResolveMacros(templateSubject);
                        EmailToSubscribers.PlainTextBody = SubscribersResolver.ResolveMacros(templatePlainText);

                        SubscribersResolver.Settings.EncodeResolvedValues = true;
                        EmailToSubscribers.Body = SubscribersResolver.ResolveMacros(templateBody);
                        EmailToSubscribers.Recipients = subscriberEmail;

                        EmailSender.SendEmail(SiteName, EmailToSubscribers);
                    }

                    // Restore original resolver culture
                    SubscribersResolver.Culture = originalCulture;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Blogs", "EmailToBlogPostSubscribers", ex);
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
            OwnerResolver.Settings.EncodeResolvedValues = false;
            email.Subject = OwnerResolver.ResolveMacros(email.Subject);
            email.PlainTextBody = OwnerResolver.ResolveMacros(email.PlainTextBody);

            // Enable macro encoding for body
            OwnerResolver.Settings.EncodeResolvedValues = true;
            email.Body = OwnerResolver.ResolveMacros(email.Body);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sends a notification e-mail to blog post subscribers, to blog moderators and to blog owner.
        /// </summary>        
        /// <param name="wi">Windows identity</param>
        /// <param name="toSubscribers">Indicates if notification email should be sent to blog post subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to blog moderators</param>
        /// <param name="toBlogOwner">Indicates if notification email should be sent to blog owner</param>
        public void SendNewCommentNotification(WindowsIdentity wi, bool toSubscribers, bool toModerators, bool toBlogOwner)
        {
            if (!BlogCommentInfoProvider.EnableEmails)
            {
                return;
            }

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
            if (toBlogOwner)
            {
                SendNotificationToOwner();
            }
        }

        #endregion
    }
}
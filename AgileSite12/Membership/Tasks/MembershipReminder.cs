using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Provides an ITask interface to send notifications to users about their expiring memberships.
    /// </summary>
    public class MembershipReminder : ITask
    {
        #region "Variables"

        private int siteId = 0;
        private string siteName = String.Empty;
        private EmailTemplateInfo template = null;

        // Settings
        private int reminderDays = 0;
        private string emailFrom = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Get site information
                SiteInfo si = SiteInfoProvider.GetSiteInfo(task.TaskSiteID);

                if (si != null)
                {
                    siteId = si.SiteID;
                    siteName = si.SiteName;
                }

                // Get email template information
                template = EmailTemplateProvider.GetEmailTemplate("Membership.ExpirationNotification", siteId);

                // If unable to retrieve email template
                if (template == null)
                {
                    throw new Exception("Unable to retrieve 'Membership.ExpirationNotification' email template.");
                }

                // Get settings
                reminderDays = SettingsKeyInfoProvider.GetIntValue("CMSMembershipReminder", siteName);
                emailFrom = SettingsKeyInfoProvider.GetValue("CMSAdminEmailAddress", siteName);

                // If sender email address not specified
                if (String.IsNullOrEmpty(emailFrom))
                {
                    throw new Exception("Unable to send emails. Membership setting 'Administrator's e-mail' not defined.");
                }

                // Get only for users with email address specified
                string where = "ISNULL(CMS_User.Email, '') <> ''";

                // Get expiring memberships
                DataSet dsMemberships = MembershipUserInfoProvider.GetExpiringMemberships(reminderDays, siteId, where, true);
                if (!DataHelper.DataSourceIsEmpty(dsMemberships))
                {
                    // Notifications data
                    List<NotificationData> notifications = new List<NotificationData>();
                    NotificationData currentNotification = null;

                    // Order expiring memberships
                    DataRow[] drOrderedMemberships = dsMemberships.Tables[0].Select(null, "UserID, MembershipID ASC");

                    foreach (DataRow dr in drOrderedMemberships)
                    {
                        int userId = ValidationHelper.GetInteger(dr["UserID"], 0);

                        // If not the same user
                        if ((currentNotification == null) || (currentNotification.UserID != userId))
                        {
                            currentNotification = new NotificationData();

                            // Set general info
                            currentNotification.UserID = userId;
                            currentNotification.UserEmail = ValidationHelper.GetString(dr["Email"], "");
                            ;

                            // Set table schema
                            currentNotification.MembershipsTable = dsMemberships.Tables[0].Clone();

                            notifications.Add(currentNotification);
                        }

                        // Add expiring membership to current notification
                        currentNotification.MembershipsTable.ImportRow(dr);
                    }

                    // Set up notifications
                    List<EmailMessage> notificationEmails = new List<EmailMessage>();

                    foreach (var notificationData in notifications)
                    {
                        notificationEmails.Add(GetNotificationEmail(notificationData));
                    }

                    // Send notifications
                    foreach (EmailMessage email in notificationEmails)
                    {
                        EmailSender.SendEmail(siteName, email);
                    }

                    // Clear send notification flag on user memberships
                    foreach (DataRow item in dsMemberships.Tables[0].Rows)
                    {
                        MembershipUserInfo mui = new MembershipUserInfo(item);

                        // Clear notification flag
                        mui.SetValue("SendNotification", null);

                        // Save changes
                        MembershipUserInfoProvider.SetMembershipUserInfo(mui);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }


        /// <summary>
        /// Returns notification email message according to given notification data.
        /// </summary>
        /// <param name="notification">Notification data</param>
        private EmailMessage GetNotificationEmail(NotificationData notification)
        {
            MacroResolver resolver = MembershipResolvers.GetMembershipExpirationResolver(UserInfoProvider.GetUserInfo(notification.UserID), notification.MembershipsTable);

            // Set up notification email
            EmailMessage email = new EmailMessage();

            resolver.Settings.EncodeResolvedValues = false;
            email.From = EmailHelper.GetSender(template, emailFrom);
            email.Recipients = notification.UserEmail;
            email.CcRecipients = template.TemplateCc;
            email.BccRecipients = template.TemplateBcc;
            email.Subject = resolver.ResolveMacros(EmailHelper.GetSubject(template, ResHelper.GetString("membership.membershipreminder.emailsubject")));
            email.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);
            resolver.Settings.EncodeResolvedValues = true;
            email.Body = resolver.ResolveMacros(template.TemplateText);

            return email;
        }

        #endregion


        #region "Types"

        /// <summary>
        /// Represents membership expiration notification data.
        /// </summary>
        private class NotificationData
        {
            /// <summary>
            /// User ID
            /// </summary>
            public int UserID
            {
                get;
                set;
            }

            /// <summary>
            /// User e-mail
            /// </summary>
            public string UserEmail
            {
                get;
                set;
            }

            /// <summary>
            /// Table with user expiring memberhips.
            /// </summary>
            public DataTable MembershipsTable
            {
                get;
                set;
            }
        }

        #endregion
    }
}
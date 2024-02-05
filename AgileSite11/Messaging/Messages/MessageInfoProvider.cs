using System;
using System.Data;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.MacroEngine;

namespace CMS.Messaging
{
    /// <summary>
    /// Class providing MessageInfo management.
    /// </summary>
    public class MessageInfoProvider : AbstractInfoProvider<MessageInfo, MessageInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static MessageInfo GetMessageInfoByGUID(Guid guid)
        {
            string where = "MessageGUID = '" + guid + "'";

            DataSet ds = ConnectionHelper.ExecuteQuery("Messaging.Message.selectall", null, where);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new MessageInfo(ds.Tables[0].Rows[0]);
            }
            return null;
        }


        /// <summary>
        /// Returns the MessageInfo structure for the specified message.
        /// </summary>
        /// <param name="messageId">Message id</param>
        public static MessageInfo GetMessageInfo(int messageId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();

            parameters.Add("@Id", messageId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Messaging.Message.select", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new MessageInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified message.
        /// </summary>
        /// <param name="message">Message to set</param>
        public static void SetMessageInfo(MessageInfo message)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            if (message != null)
            {
                // Update the database
                if (message.MessageID > 0)
                {
                    message.Generalized.UpdateData();
                }
                else
                {
                    message.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[MessageInfoProvider.SetMessageInfo]: No MessageInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified message.
        /// </summary>
        /// <param name="messageObj">Message object</param>
        public static void DeleteMessageInfo(MessageInfo messageObj)
        {
            if (messageObj != null)
            {
                messageObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified message.
        /// </summary>
        /// <param name="messageId">Message id</param>
        public static void DeleteMessageInfo(int messageId)
        {
            MessageInfo messageObj = GetMessageInfo(messageId);
            DeleteMessageInfo(messageObj);
        }


        /// <summary>
        /// Delete sent message - internally uses DeleteMessageInfo.
        /// </summary>
        /// <param name="messageId">Message ID</param>
        public static void DeleteSentMessage(int messageId)
        {
            DeleteSentMessages(0, "MessageID = " + messageId);
        }


        /// <summary>
        /// Delete received message - internally uses DeleteMessageInfo.
        /// </summary>
        /// <param name="messageId">Message ID</param>
        public static void DeleteReceivedMessage(int messageId)
        {
            DeleteReceivedMessages(0, "MessageID = " + messageId);
        }


        /// <summary>
        ///  Gets messages - general method.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by</param>
        public static DataSet GetMessages(string whereCondition, string orderBy)
        {
            return GetMessages(whereCondition, orderBy, -1, null);
        }


        /// <summary>
        ///  Gets messages - general method.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet GetMessages(string whereCondition, string orderBy, int topN, string columns)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery("Messaging.Message.selectall", null, whereCondition, orderBy, topN, columns);

            return ds;
        }


        /// <summary>
        ///  Gets user's messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        public static DataSet GetMessages(int userId, string orderBy)
        {
            return GetMessages(userId, orderBy, -1, null);
        }


        /// <summary>
        ///  Gets user's messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet GetMessages(int userId, string orderBy, int topN, string columns)
        {
            return GetMessages("MessageRecipientUserID=" + userId + " AND (MessageRecipientDeleted=0 OR MessageRecipientDeleted IS NULL)", orderBy, topN, columns);
        }


        /// <summary>
        ///  Gets user's unread messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        public static DataSet GetUnreadMessages(int userId, string orderBy)
        {
            return GetUnreadMessages(userId, orderBy, -1, null);
        }


        /// <summary>
        ///  Gets user's unread messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet GetUnreadMessages(int userId, string orderBy, int topN, string columns)
        {
            return GetMessages("MessageRecipientUserID=" + userId + " AND (MessageRecipientDeleted=0 OR MessageRecipientDeleted IS NULL) AND (MessageRead IS NULL OR MessageIsRead=0)", orderBy, topN, columns);
        }


        /// <summary>
        ///  Gets user's sent messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        public static DataSet GetSentMessages(int userId, string orderBy)
        {
            return GetSentMessages(userId, orderBy, -1, null);
        }


        /// <summary>
        ///  Gets user's sent messages - uses general method.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet GetSentMessages(int userId, string orderBy, int topN, string columns)
        {
            return GetMessages("MessageSenderUserID=" + userId + " AND (MessageSenderDeleted=0 OR MessageSenderDeleted IS NULL)", orderBy, topN, columns);
        }


        /// <summary>
        /// Gets messages count.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        public static int GetMessagesCount(string whereCondition)
        {
            // Get messages count
            DataSet ds = ConnectionHelper.ExecuteQuery("Messaging.Message.selectCount", null, whereCondition, null);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return 0;
            }
            return (int)ds.Tables[0].Rows[0][0];
        }


        /// <summary>
        /// Gets messages count.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static int GetMessagesCount(int userId)
        {
            return GetMessagesCount("MessageRecipientUserID=" + userId + " AND (MessageRecipientDeleted=0 OR MessageRecipientDeleted IS NULL)");
        }


        /// <summary>
        /// Gets sent messages count.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static int GetSentMessagesCount(int userId)
        {
            return GetMessagesCount("MessageSenderUserID=" + userId + " AND (MessageSenderDeleted=0 OR MessageSenderDeleted IS NULL)");
        }


        /// <summary>
        /// Gets messages count.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static int GetUnreadMessagesCount(int userId)
        {
            return GetMessagesCount("MessageRecipientUserID=" + userId + " AND (MessageRecipientDeleted=0 OR MessageRecipientDeleted IS NULL) AND (MessageRead IS NULL OR MessageIsRead=0)");
        }


        /// <summary>
        /// Delete all user's sent messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static void DeleteSentMessages(int userId)
        {
            DeleteSentMessages(userId, null);
        }


        /// <summary>
        /// Delete all user's sent messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        public static void DeleteSentMessages(int userId, string where)
        {
            ProviderObject.DeleteSentMessagesInternal(userId, where);
        }


        /// <summary>
        /// Delete all user's received messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static void DeleteReceivedMessages(int userId)
        {
            DeleteReceivedMessages(userId, null);
        }


        /// <summary>
        /// Delete all user's received messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        public static void DeleteReceivedMessages(int userId, string where)
        {
            ProviderObject.DeleteReceivedMessagesInternal(userId, where);
        }


        /// <summary>
        /// Send notification email.
        /// </summary>
        /// <param name="messageInfo">Message info</param>
        /// <param name="recipient">Recipient of the message</param>
        /// <param name="sender">Sender of the message</param>
        /// <param name="siteName">Site name</param>
        public static void SendNotificationEmail(MessageInfo messageInfo, UserInfo recipient, UserInfo sender, string siteName)
        {
            if ((messageInfo != null) && (recipient != null))
            {
                if (recipient.UserMessagingNotificationEmail != "")
                {
                    try
                    {
                        // Get email template
                        EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("messaging.messagenotification", siteName);
                        if (template == null)
                        {
                            throw new Exception("[MessageInfoProvider.SendNotificationEmail]: Notification email template is missing.");
                        }
                        // Get settings values
                        string from = ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName + ".CMSMessagingEmailFrom"), null);
                        string subject = ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName + ".CMSMessagingEmailSubject"), null);

                        // Settings keys don't exist
                        if ((string.IsNullOrEmpty(from)) || (subject == null))
                        {
                            throw new Exception("[MessageInfoProvider.SendNotificationEmail]: Settings keys are missing.");
                        }

                        MacroResolver resolver = MacroResolver.GetInstance();
                        resolver.Settings.EncodeResolvedValues = true;

                        // Prepare data for resolver, encode message body
                        resolver.SetAnonymousSourceData(sender, recipient, messageInfo);
                        resolver.SetNamedSourceData("Sender", sender);
                        resolver.SetNamedSourceData("Recipient", recipient);
                        resolver.SetNamedSourceData("Message", messageInfo);

                        // Prepare macros
                        resolver.SetNamedSourceData("LogonUrl", URLHelper.GetAbsoluteUrl(AuthenticationHelper.GetSecuredAreasLogonPage(siteName)), false);

                        // Prepare message
                        EmailMessage message = new EmailMessage();
                        message.From = EmailHelper.GetSender(template, from);
                        message.Recipients = recipient.UserMessagingNotificationEmail;
                        message.EmailFormat = EmailFormatEnum.Default;
                        message.CcRecipients = template.TemplateCc;
                        message.BccRecipients = template.TemplateBcc;

                        // Resolve body macros
                        message.Body = resolver.ResolveMacros(template.TemplateText);

                        // Do not encode plain text body and subject
                        resolver.Settings.EncodeResolvedValues = false;
                        message.Subject = resolver.ResolveMacros(EmailHelper.GetSubject(template, subject));
                        message.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);

                        // Attach template metafiles to e-mail
                        EmailHelper.ResolveMetaFileImages(message, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                        // Send email
                        EmailSender.SendEmail(siteName, message);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Messaging notification email", "MESSAGING", ex);
                    }
                }
            }
        }


        /// <summary>
        /// Mark user's received messages as read.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="date">Date time of reading</param>
        public static void MarkReadReceivedMessages(int userId, string where, DateTime date)
        {
            if (userId > 0)
            {
                where = SqlHelper.AddWhereCondition(where, "MessageRecipientUserID = " + userId);
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();

            parameters.Add("@MessageRead", date);

            // Process the action
            ConnectionHelper.ExecuteQuery("Messaging.Message.markread", parameters, where);
        }


        /// <summary>
        /// Mark user's received messages as unread.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        public static void MarkUnreadReceivedMessages(int userId, string where)
        {
            if (userId > 0)
            {
                where = SqlHelper.AddWhereCondition(where, "MessageRecipientUserID = " + userId);
            }

            // Process the action
            ConnectionHelper.ExecuteQuery("Messaging.Message.markunread", null, where);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Delete all user's received messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteReceivedMessagesInternal(int userId, string where)
        {
            if (userId > 0)
            {
                where = SqlHelper.AddWhereCondition(where, "MessageRecipientUserID = " + userId);
            }

            // Get the connection
            using (var tr = BeginTransaction())
            {
                // Update message information - MessageRecipientDeleted = 1
                ConnectionHelper.ExecuteQuery("Messaging.Message.markrecipientdelete", null, where);

                // Delete all records if MessageRecipientDeleted = 1 AND MessageSenderDeleted = 1
                ConnectionHelper.ExecuteQuery("Messaging.Message.deleteunreferenced", null, where);

                tr.Commit();
            }
        }


        /// <summary>
        /// Delete all user's sent messages.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteSentMessagesInternal(int userId, string where)
        {
            if (userId > 0)
            {
                where = SqlHelper.AddWhereCondition(where, "MessageSenderUserID = " + userId);
            }

            // Get the connection
            using (var tr = BeginTransaction())
            {
                // Update message information - MessageSenderDeleted = 1
                ConnectionHelper.ExecuteQuery("Messaging.Message.marksenderdelete", null, where);

                // Delete all records if MessageRecipientDeleted = 1 AND MessageSenderDeleted = 1
                ConnectionHelper.ExecuteQuery("Messaging.Message.deleteunreferenced", null, where);

                tr.Commit();
            }
        }

        #endregion
    }
}
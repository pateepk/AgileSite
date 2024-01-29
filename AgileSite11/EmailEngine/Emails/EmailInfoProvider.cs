using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Net.Mime;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

using Path = CMS.IO.Path;

using SystemIO = System.IO;

namespace CMS.EmailEngine
{
    using TypedDataSet = InfoDataSet<EmailInfo>;

    /// <summary>
    /// Class providing EmailInfo management.
    /// </summary>
    public class EmailInfoProvider : AbstractInfoProvider<EmailInfo, EmailInfoProvider>
    {
        #region "Properties"

        private static int mBatchSize;


        /// <summary>
        /// Gets the number of e-mails to fetch in one batch.
        /// </summary>
        private static int BatchSize
        {
            get
            {
                if (mBatchSize < 1)
                {
                    int batchSize = EmailHelper.Settings.BatchSize();
                    mBatchSize = batchSize < 1 ? 50 : batchSize;
                }
                return mBatchSize;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the EmailInfo structure for the specified email ID.
        /// </summary>
        /// <param name="emailId">Email ID</param>
        public static EmailInfo GetEmailInfo(int emailId)
        {
            return ProviderObject.GetInfoById(emailId);
        }


        /// <summary>
        /// Returns all EmailInfo objects specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        [Obsolete("Use method GetEmails() instead")]
        public static TypedDataSet GetEmailInfos(string where, string orderBy)
        {
            return ProviderObject.GetEmailsInternal(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns all EmailInfo objects specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Specifies number of records to return</param>
        /// <param name="columns">Data columns to return</param>
        [Obsolete("Use method GetEmails() instead")]
        public static TypedDataSet GetEmailInfos(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetEmailsInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified email.
        /// </summary>
        /// <param name="email">Email to set</param>
        public static void SetEmailInfo(EmailInfo email)
        {
            ProviderObject.SetEmailInfoInternal(email);
        }


        /// <summary>
        /// Deletes specified email.
        /// </summary>
        /// <param name="emailId">Email id</param>
        public static void DeleteEmailInfo(int emailId)
        {
            EmailInfo infoObj = GetEmailInfo(emailId);
            ProviderObject.DeleteEmailInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified email.
        /// </summary>
        /// <param name="infoObj">Email object</param>
        public static void DeleteEmailInfo(EmailInfo infoObj)
        {
            ProviderObject.DeleteEmailInfoInternal(infoObj);
        }


        /// <summary>
        /// Returns a query for all the EmailInfo objects.
        /// </summary>
        public static ObjectQuery<EmailInfo> GetEmails()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Add attachment to specified e-mail and return its ID.
        /// </summary>
        /// <param name="emailId">E-mail</param>
        /// <param name="name">Attachment name</param>
        /// <param name="extension">Attachment extension</param>
        /// <param name="mimetype">Attachment type</param>
        /// <param name="size">Size of the attachment</param>
        /// <param name="binary">Binary data</param>
        /// <param name="contentId">Attachment content ID</param>
        /// <param name="guid">Attachment GUID - e.g. metafile GUID</param>
        /// <param name="lastModified">Last modification date of the attachment - e.g. metafile last modification</param>
        /// <param name="siteId">Site ID - if the attachment is of specified site</param>
        public static int BindEmailAttachment(int emailId, string name, string extension, string mimetype, int size, byte[] binary, string contentId, Guid guid, DateTime lastModified, int siteId)
        {
            // Prepare parameters
            QueryDataParameters parameters = new QueryDataParameters
            {
                { "@EmailID", emailId },
                { "@Name", name },
                { "@Ext", extension },
                { "@Size", size },
                { "@MimeType", mimetype },
                { "@Bin", binary },
                { "@ContentID", contentId ?? (object)DBNull.Value },
                { "@GUID", guid != Guid.Empty ? guid : Guid.NewGuid() },
                { "@LastModified", lastModified != DateTimeHelper.ZERO_TIME ? lastModified : DateTime.Now },
                { "@SiteID", siteId > 0 ? siteId : (object)DBNull.Value }
            };

            var attachmentId = ConnectionHelper.ExecuteScalar("cms.email.bindattachment", parameters);

            return ValidationHelper.GetInteger(attachmentId, 0);
        }


        /// <summary>
        /// Creates new EmailInfo from EmailMessage.
        /// </summary>
        /// <param name="message">Email message</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="dontSend">If true, e-mail status remains 'being created' so it could not be sent</param>
        /// <param name="setName">Allows to flag e-mail as a part of specific e-mail set, could be null</param>
        /// <param name="setRelatedId">ID for indication of the e-mail in specific set</param>
        /// <returns>EmailInfo object</returns>
        [Obsolete("Use SetEmailInfo(EmailMessage, int, bool, bool) method instead.")]
        public static EmailInfo SetEmailInfo(EmailMessage message, int siteId, bool dontSend, string setName, int setRelatedId)
        {
            return SetEmailInfo(message, siteId, dontSend);
        }


        /// <summary>
        /// Creates new EmailInfo from EmailMessage.
        /// </summary>
        /// <param name="message">Email message</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="dontSend">If true, e-mail status remains 'being created' so it could not be sent</param>
        /// <param name="isMassEmail">Indicates if the e-mail is mass</param>
        /// <returns>EmailInfo object</returns>
        public static EmailInfo SetEmailInfo(EmailMessage message, int siteId, bool dontSend = false, bool isMassEmail = false)
        {
            if (message == null)
            {
                return null;
            }

            // Create new EmailInfo object and set its properties to that of EmailMessage
            var emailInfo = new EmailInfo(message)
            {
                EmailSiteID = siteId,
                EmailIsMass = isMassEmail
            };

            // Write this object to DB
            SetEmailInfo(emailInfo);

            // Bind attachments
            BindEmailAttachments(message.Attachments, emailInfo.EmailID, siteId);

            if (!dontSend)
            {
                // Set e-mail status to 'waiting' so it can be send
                emailInfo.EmailStatus = EmailStatusEnum.Waiting;
                SetEmailInfo(emailInfo);
            }

            return emailInfo;
        }


        private static void BindEmailAttachments(IEnumerable<Attachment> attachments, int emailId, int siteId)
        {
            // Loop through all attachments of EmailMessage and assign these attachments to EmailInfo
            string contentId = null;
            string extension;
            Guid attachGuid = Guid.Empty;
            DateTime lastModification = DateTimeHelper.ZERO_TIME;
            int mSiteId = siteId;
            foreach (var at in attachments)
            {
                var eat = at as EmailAttachment;

                // Set to this object all properties from Attachment
                var binary = new byte[at.ContentStream.Length];
                at.ContentStream.Seek(0, System.IO.SeekOrigin.Begin);
                at.ContentStream.Read(binary, 0, (int)at.ContentStream.Length);
                at.ContentStream.Seek(0, System.IO.SeekOrigin.Begin);

                // Store attachment content ID if its disposition type is 'inline'
                if (at.ContentDisposition.Inline && (at.ContentDisposition.DispositionType == DispositionTypeNames.Inline))
                {
                    contentId = at.ContentId;
                }

                // Save attachment to DB and connect it with the e-mail
                if (eat != null)
                {
                    attachGuid = eat.AttachmentGUID;
                    lastModification = eat.LastModified;
                    mSiteId = eat.SiteID;
                }

                // Try to get file extension
                try
                {
                    extension = Path.GetExtension(at.ContentType.Name);
                }
                catch
                {
                    extension = Path.GetExtension(at.Name);
                }

                // Set attachment to DB
                BindEmailAttachment(emailId, at.Name, extension, at.ContentType.MediaType, (int)at.ContentStream.Length, binary, contentId, attachGuid, lastModification, mSiteId);

                // Reset variables
                contentId = null;
                attachGuid = Guid.Empty;
                lastModification = DateTimeHelper.ZERO_TIME;
                mSiteId = siteId;
            }
        }


        /// <summary>
        ///  Creates new <see cref="MailMessage"/> from given <see cref="EmailInfo"/>.
        /// </summary>
        /// <param name="emailInfo">EmailInfo object</param>
        /// <param name="siteName">Site name; default value is null</param>
        /// <returns>New <see cref="MailMessage"/> object</returns>
        public static MailMessage GetMailMessage(EmailInfo emailInfo, string siteName = null)
        {
            if (emailInfo == null)
            {
                return null;
            }

            EmailFormatEnum emailFormat = EmailHelper.ResolveEmailFormat(emailInfo.EmailFormat, emailInfo.EmailSiteID);

            MailMessage message = new MailMessage
            {
                Subject = emailInfo.EmailSubject.Trim(),
                From = new MailAddress(emailInfo.EmailFrom),
                IsBodyHtml = (emailFormat == EmailFormatEnum.Html || emailFormat == EmailFormatEnum.Both)
            };

            EmailHelper.Fill(message.To, emailInfo.EmailTo);
            EmailHelper.Fill(message.CC, emailInfo.EmailCc);
            EmailHelper.Fill(message.Bcc, emailInfo.EmailBcc);

            // Set Reply-to address if specified
            if (!string.IsNullOrEmpty(emailInfo.EmailReplyTo))
            {
                EmailHelper.SetReplyToEmailAddress(message, emailInfo.EmailReplyTo);
            }

            // Add additional headers
            message.Headers.Add(EmailMessage.GetHeaderFields(emailInfo.EmailHeaders));

            // Get attachment collection from EmailAttachmentInfoProvider
            var attachments = GetAttachments(emailInfo.EmailID);

            // Set email body depending on the email format (plain-text or html)
            EmailHelper.SetEmailBody(message, emailFormat, emailInfo.EmailBody, emailInfo.EmailPlainTextBody, siteName, attachments);

            return message;
        }


        /// <summary>
        /// Returns collection of <see cref="Attachment"/> objects based on email's attachments stored in DB.
        /// </summary>
        /// <param name="emailId">ID of the email which attachments should be returned</param>
        private static IEnumerable<Attachment> GetAttachments(int emailId)
        {
            DataSet ds = EmailAttachmentInfoProvider.GetEmailAttachmentInfos(emailId);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    var eai = new EmailAttachmentInfo(dr);
                    if (eai.AttachmentBinary != null)
                    {
                        var ms = new SystemIO.MemoryStream(eai.AttachmentBinary);
                        var at = new Attachment(ms, eai.AttachmentName);

                        if (!string.IsNullOrEmpty(eai.AttachmentContentID))
                        {
                            // Add inline attachment data
                            at.ContentDisposition.Inline = true;
                            at.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                            at.ContentId = eai.AttachmentContentID;
                            at.ContentType.MediaType = eai.AttachmentMimeType;
                            at.ContentType.Name = Path.GetFileName(eai.AttachmentName);
                            at.TransferEncoding = EmailHelper.TransferEncoding;
                        }

                        yield return at;
                    }
                }
            }
        }


        /// <summary>
        /// Fetches e-mails which should be sent and marks them as being sent.
        /// </summary>
        /// <param name="emailsToFetch">Determines which e-mails are to be fetched</param>
        /// <param name="firstEmailId">First email ID (only e-mails with the same or higher ID are fetched)</param>
        /// <param name="batchSize">Number of e-mails to retrieve; default value is 0 - in this case batch size is retrieved from Settings</param>
        /// <param name="failedEmailsNewerThan">Timestamp indicating that only failed emails that are younger than given date should be sent. Only works for <see cref="EmailMailoutEnum.Failed" />.</param>
        /// <returns>DataSet with fetched e-mails</returns>
        public static DataSet FetchEmails(EmailMailoutEnum emailsToFetch, int firstEmailId, int batchSize = 0, DateTime? failedEmailsNewerThan = null)
        {
            if ((emailsToFetch == EmailMailoutEnum.New) && (failedEmailsNewerThan != null))
            {
                throw new NotSupportedException("[EmailInfoProvider.FetchEmails]: Parameter newerThan affects only failed emails and this combination of parameters is not supported.");
            }

            bool fetchFailed = (emailsToFetch & EmailMailoutEnum.Failed) == EmailMailoutEnum.Failed;
            bool fetchNew = (emailsToFetch & EmailMailoutEnum.New) == EmailMailoutEnum.New;
            
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FetchFailed", fetchFailed);
            parameters.Add("@FetchNew", fetchNew);
            parameters.Add("@FirstEmailID", firstEmailId);
            parameters.Add("@BatchSize", batchSize == 0 ? BatchSize : batchSize);
            parameters.Add("@FailedEmailsNewerThan", failedEmailsNewerThan);

            return ConnectionHelper.ExecuteQuery("cms.email.fetchemailstosend", parameters);
        }


        /// <summary>
        /// Fetches mass e-mail which should be sent and marks it as being sent.
        /// </summary>
        /// <param name="emailsToFetch">Determines which e-mails are to be fetched</param>
        /// <param name="emailId">ID of the pattern e-mail</param>
        /// <param name="firstUserId">First user ID (only users with the same or higher ID are fetched)</param>
        /// <param name="batchSize">Number of users to retrieve; default value is 0 - in this case batch size is retrieved from Settings</param>
        /// <returns>DataSet with users whom the mass e-mail should be sent</returns>
        public static DataSet FetchMassEmail(EmailMailoutEnum emailsToFetch, int emailId, int firstUserId, int batchSize = 0)
        {
            bool fetchFailed = (emailsToFetch & EmailMailoutEnum.Failed) == EmailMailoutEnum.Failed;
            bool fetchNew = (emailsToFetch & EmailMailoutEnum.New) == EmailMailoutEnum.New;

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FetchFailed", fetchFailed);
            parameters.Add("@FetchNew", fetchNew);
            parameters.Add("@EmailID", emailId);
            parameters.Add("@FirstUserID", firstUserId);
            parameters.Add("@BatchSize", batchSize == 0 ? BatchSize : batchSize);

            return ConnectionHelper.ExecuteQuery("cms.email.fetchmassemail", parameters);
        }


        /// <summary>
        /// Deletes all EmailInfo objects which sending has failed.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static void DeleteAllFailed(int siteId)
        {
            DeleteAll(siteId, EmailStatusEnum.Waiting, true);
        }


        /// <summary>
        /// Deletes all EmailInfo objects with specific parameters.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="status">E-mail status</param>
        /// <param name="onlyFailed">If true failed e-mails are deleted, failed e-mail status is waiting</param>
        public static void DeleteAll(int siteId, EmailStatusEnum status, bool onlyFailed)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Status", Convert.ToInt32(status));
            parameters.Add("@SiteID", siteId);
            parameters.Add("@OnlyFailed", onlyFailed);

            ConnectionHelper.ExecuteQuery("cms.email.deleteoldemails", parameters);
        }


        /// <summary>
        /// Deletes expired archived e-mails.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="expirationDate">Every archived email older that expiration date will be deleted</param>
        /// <param name="batchSize">Number of emails to be deleted at once</param>
        public static void DeleteArchived(int siteId, DateTime expirationDate, int batchSize)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@ExpirationDate", expirationDate);
            parameters.Add("@BatchSize", batchSize);

            ConnectionHelper.ExecuteQuery("cms.email.deletearchived", parameters);
        }


        /// <summary>
        /// Gets number of e-mails due to where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static int GetEmailCount(string where)
        {
            // Get record from DB
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.email.selectcount", null, where);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get the count
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);
            }

            return 0;
        }


        /// <summary>
        /// Gets the number of archived emails that are older that expiration date for each site.
        /// </summary>
        /// <returns>Table of triplets containing site ID, expiration date and number of expired emails</returns>
        public static DataSet GetExpiredEmailCount()
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Now", DateTime.Now);

            return ConnectionHelper.ExecuteQuery("cms.email.expiredcount", parameters);
        }


        /// <summary>
        /// Returns the previous and next e-mails of the given e-mail in the order specified by ORDER BY parameter matching the WHERE criteria.
        /// </summary>
        /// <param name="emailId">ID of the e-mail relative to which the previous and next e-mails are returned</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        public static int[] GetPreviousNext(int emailId, string where, string orderBy)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@EmailID", emailId);

            DataSet ds = ConnectionHelper.ExecuteQuery("cms.email.selectpreviousnext", parameters, where, orderBy);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                int[] result = new int[2];
                if (ds.Tables[0].Rows.Count == 2)
                {
                    result[0] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EmailID"], 0);
                    result[1] = ValidationHelper.GetInteger(ds.Tables[0].Rows[1]["EmailID"], 0);
                }
                else
                {
                    int base_rn = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["BASE_RN"], 0);
                    int rn = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["RN"], 0);
                    if (rn > base_rn)
                    {
                        result[0] = 0;
                        result[1] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EmailID"], 0);
                    }
                    else
                    {
                        result[0] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EmailID"], 0);
                        result[1] = 0;
                    }
                }

                return result;
            }

            return null;
        }


        /// <summary>
        /// Gets dataset with two columns 'EmailSiteID' and 'EmailCount'. Dataset contains emails grouped by sites ID.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static DataSet GetEmailCountForSites(string where)
        {
            // Return dataset with two columns 'SiteName' and 'EmailCount'
            return ConnectionHelper.ExecuteQuery("cms.email.selectcountforsites", null, where);
        }

        #endregion


        #region "Protected methods - Basic"

        /// <summary>
        /// Returns all EmailInfo objects specified by where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Specifies number of records to return</param>
        /// <param name="columns">Data columns to return</param>
        [Obsolete("Use method GetEmails() instead")]
        protected virtual TypedDataSet GetEmailsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetObjectQuery().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Sets (updates or inserts) specified email.
        /// </summary>
        /// <param name="email">Email to set</param>
        protected virtual void SetEmailInfoInternal(EmailInfo email)
        {
            if (email != null)
            {
                if (email.EmailID == 0)
                {
                    email.EmailCreated = DateTime.Now;
                }
                email.Generalized.SetData();
            }
        }


        /// <summary>
        /// Deletes specified email.
        /// </summary>
        /// <param name="email">Email object</param>
        protected virtual void DeleteEmailInfoInternal(EmailInfo email)
        {
            if (email != null)
            {
                email.Generalized.DeleteData();
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Resets e-mail's status to 'waiting' anywhere the status is 'sending' for longer time.
        /// </summary>
        /// <remarks>
        /// The reset action is performed on those records which are in the 'sending' state more than 30 minutes. This preserves 'sending' state of emails which are currently sent by other servers.
        /// </remarks>
        internal static void ResetSendingStatus()
        {
            try
            {
                ConnectionHelper.ExecuteQuery("cms.email.resetstatus", null);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("EmailInfoProvider", "ResetSendingStatus", ex);
            }
        }


        /// <summary>
        /// Returns EmailInfo objects with the specified IDs.
        /// </summary>
        /// <param name="emailIds">Array of email IDs</param>        
        /// <returns>DataSet with emails that have one of the specified email IDs</returns>
        internal static TypedDataSet GetEmailInfos(int[] emailIds)
        {
            if (emailIds != null && emailIds.Length > 0)
            {
                string IDs = string.Join(", ", Array.ConvertAll(emailIds, emailId => emailId.ToString()));
                string where = string.Format("EmailID IN ({0})", IDs);
                return GetEmails().Where(where).TypedResult;
            }

            return null;
        }


        /// <summary>
        /// Sets status of EmailInfo objects specified by IDs to 'Sending'.
        /// </summary>
        /// <param name="emailIds">Array of email IDs</param>
        internal static void SetSendingStatus(int[] emailIds)
        {
            if (emailIds != null && emailIds.Length > 0)
            {
                string IDs = string.Join(", ", Array.ConvertAll(emailIds, emailId => emailId.ToString()));
                string where = string.Format("EmailID IN ({0})", IDs);
                ProviderObject.UpdateData(string.Format("EmailStatus={0}", (int)EmailStatusEnum.Sending), null, where);
            }
        }


        /// <summary>
        /// Archives the e-mail message.
        /// </summary>
        /// <param name="email">The e-mail message</param>        
        internal static void ArchiveEmail(EmailInfo email)
        {
            email.EmailLastSendResult = null;
            SaveEmailInternal(email, EmailStatusEnum.Archived);
        }


        /// <summary>
        /// Saves the failed e-mail message along with the reason for delivery failure.
        /// </summary>
        /// <param name="email">The e-mail message</param>
        /// <param name="lastSendResult">Reason for the delivery failure</param>
        internal static void SaveFailedEmail(EmailInfo email, string lastSendResult)
        {
            email.EmailLastSendResult = lastSendResult;
            SaveEmailInternal(email, EmailStatusEnum.Waiting);
        }


        /// <summary>
        /// Saves the e-mail message that was not sent along with the failure notification.
        /// </summary>
        /// <param name="email">The e-mail message</param>        
        internal static void SaveNotSentEmail(EmailInfo email)
        {
            SaveEmailInternal(email, EmailStatusEnum.Waiting);
        }


        private static void SaveEmailInternal(EmailInfo email, EmailStatusEnum status)
        {
            email.EmailLastSendAttempt = DateTime.Now;
            email.EmailStatus = status;
            SetEmailInfo(email);
        }

        #endregion
    }
}
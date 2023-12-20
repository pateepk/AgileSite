using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine.Extensions;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Sends e-mails from queue in a separate thread.
    /// </summary>
    internal class ThreadSender
    {
        #region "Constants"

        /// <summary>
        /// If the number of e-mails in the DB queue is higher than the threshold, 
        /// fetching of the next batch of e-mails is postponed until it drops below the threshold.
        /// </summary>
        private const int DBQUEUE_THRESHOLD = 100;

        #endregion


        #region "Variables"

        /// <summary>
        /// Singleton instance of the <c>ThreadSender</c>.
        /// </summary>
        private static ThreadSender instance;


        /// <summary>
        /// Lock over initialization of the singleton that guarantees that only one instance is created.
        /// </summary>
        private static object instanceLock = new object();


        /// <summary>
        /// Lock over the sender that guarantees that only one thread is sending messages at a time.
        /// </summary>
        private readonly object sendLock = new object();


        /// <summary>
        /// Lock over the callback that guarantees that only one callback may update queue at a time.
        /// </summary>
        private static object callbackLock = new object();


        /// <summary>
        /// Lock over the update queue that guarantees that only one thread is returning messages in the DB.
        /// </summary>
        private readonly object stopLock = new object();


        /// <summary>
        /// Windows identity.
        /// </summary>
        private WindowsIdentity mIdentity;


        /// <summary>
        /// Contains the list of e-mail IDs which should be sent.
        /// </summary>
        private int[] mEmailIds;


        /// <summary>
        /// Indicates which e-mail messages should be sent.
        /// </summary>
        private EmailMailoutEnum mEmailsToSend;


        /// <summary>
        /// Signals that sending was canceled and all pending operations should finish ASAP.
        /// </summary>
        private bool stopSending;


        /// <summary>
        /// Signals that sending is being canceled - pending messages from queue are returned to the DB.
        /// </summary>
        private bool stoppingSending;


        /// <summary>
        /// Wait handle that is used to wait until every callback from asynchronous send is processed.
        /// </summary>
        private readonly ManualResetEvent mailoutComplete = new ManualResetEvent(true);


        /// <summary>
        /// Wait handle that is used to limit the number of e-mails that are being fetched from the DB.
        /// </summary>
        private readonly ManualResetEvent fetchThrottle = new ManualResetEvent(true);


        /// <summary>
        /// In-memory queue of e-mail messages from the application that are waiting to be sent.
        /// </summary>
        private readonly Queue<EmailToken> emailQueueApp = new Queue<EmailToken>();


        /// <summary>
        /// In-memory queue of e-mail messages from the DB that are waiting to be sent.
        /// </summary>
        private readonly Queue<EmailToken> emailQueueDB = new Queue<EmailToken>();


        /// <summary>
        /// In-memory list of e-mail messages that are being sent.
        /// </summary>
        private readonly List<EmailToken> inTransitList = new List<EmailToken>();


        /// <summary>
        /// Contains mass e-mails that are pending finalization.
        /// </summary>
        /// <remarks>
        /// The mass e-mail must have its status set after all users have been sent their copy of 
        /// the message. Since it would be too memory and time consuming to check for x mass e-mails
        /// if they have been sent to n users, it's easier to run this finalization as one
        /// of the final steps of the mailout when we are sure there are no more messages to send 
        /// (just after all messages have been sent and before the sending is finished).
        /// </remarks>
        private readonly Queue<EmailInfo> pendingMassEmails = new Queue<EmailInfo>();


        /// <summary>
        /// Sends only failed emails, that are newer than this DateTime.
        /// </summary>
        private DateTime? mSendFailedEmailsNewerThan;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the singleton instance of a <see cref="ThreadSender" />.
        /// </summary>
        /// <value>The <c>ThreadSender</c> instance</value>
        internal static ThreadSender Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new ThreadSender();
                        }
                    }
                }

                return instance;
            }
        }


        /// <summary>
        /// Gets a value indicating whether sending is currently in progress.
        /// </summary>
        /// <value><c>true</c> if sending; otherwise, <c>false</c>.</value>
        internal bool Sending
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value indicating whether the number of e-mails in the DB queue is over the threshold.
        /// </summary>        
        private bool DBQueueOverThreshold
        {
            get
            {
                return emailQueueDB.Count > DBQUEUE_THRESHOLD;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the application and database queues are empty.
        /// </summary>
        /// <value><c>true</c> if both queues are empty, otherwise <c>false</c></value>
        private bool QueuesAreEmpty
        {
            get
            {
                return (emailQueueDB.Count == 0 && emailQueueApp.Count == 0) || stopSending;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSender"/> class.
        /// </summary>
        public ThreadSender()
        {
            EmailProvider.SendCompleted += SendCompleted;

            Init();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes this sender instance.
        /// </summary>
        private void Init()
        {
            mIdentity = null;
            mEmailIds = new int[0];
            mEmailsToSend = EmailMailoutEnum.None;

            stopSending = stoppingSending = false;

            mailoutComplete.Set();
            fetchThrottle.Set();

            // Ensure that all servers are available to send messages
            SMTPServerLookupTable.Instance.ReleaseAll();

            Sending = false;
        }


        /// <summary>
        /// Sends all failed emails for given site.
        /// </summary>
        internal void SendAllFailed(int siteId)
        {
            var emailIds = PrepareSiteWaitingEmails(siteId)
                .WhereNotNull(nameof(EmailInfo.EmailLastSendResult))
                .Column(nameof(EmailInfo.EmailID))
                .TypedResult.Select(m => m.EmailID).ToArray();

            Send(emailIds);
        }


        /// <summary>
        /// Sends all emails for given site.
        /// </summary>
        internal void SendAll(int siteId)
        {
            var emailIds = PrepareSiteWaitingEmails(siteId)
                .Column(nameof(EmailInfo.EmailID))
                .TypedResult.Select(m => m.EmailID).ToArray();

            Send(emailIds);
        }


        /// <summary>
        /// Sends the e-mail message with the specified ID.
        /// </summary>
        /// <param name="emailIds">List of IDs of the e-mail messages to send</param>
        internal void Send(int[] emailIds)
        {
            Send(emailIds, EmailMailoutEnum.All);
        }


        /// <summary>
        /// Sends the e-mail messages with the specified status.
        /// </summary>
        /// <param name="emailsToSend">Which e-mail messages should be sent</param>
        /// <param name="failedEmailsNewerThan">Timestamp indicating that only emails that are younger than given date should be sent. Only works for <see cref="EmailMailoutEnum.Failed" />.</param>
        internal void Send(EmailMailoutEnum emailsToSend, DateTime? failedEmailsNewerThan = null)
        {
            Send(new int[0], emailsToSend, failedEmailsNewerThan);
        }


        /// <summary>
        /// Sends the e-mail messages with the specified status.
        /// </summary>
        /// <param name="emailIds">List of IDs of the e-mail messages to send</param>
        /// <param name="emailsToSend">Which e-mail messages should be sent</param>
        /// <param name="failedEmailsNewerThan">Timestamp indicating that only emails that are younger than given date should be sent. Only works for <see cref="EmailMailoutEnum.Failed" />.</param>
        private void Send(int[] emailIds, EmailMailoutEnum emailsToSend, DateTime? failedEmailsNewerThan = null)
        {
            // If sending is not enabled cancel send
            if (!EmailHelper.IsAnySendingEnabled())
            {
                return;
            }

            // Acquire exclusive access to critical section using doubled-checked locking and 
            // run sender in a separate thread
            if (!Sending)
            {
                lock (sendLock)
                {
                    if (!Sending)
                    {
                        Sending = true;

                        mIdentity = WindowsIdentity.GetCurrent();
                        mEmailIds = emailIds;
                        mEmailsToSend = emailsToSend;
                        mSendFailedEmailsNewerThan = failedEmailsNewerThan;

                        try
                        {
                            using (new CMSActionContext { AllowAsyncActions = true })
                            {
                                CMSThread asyncEmailSend = new CMSThread(Run);
                                asyncEmailSend.Start();
                            }
                        }
                        catch
                        {
                            Sending = false;

                            throw;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Enqueues the e-mail message in the application's email queue.
        /// </summary>
        /// <param name="message">The e-mail message to send</param>
        /// <param name="siteName">Name of the site</param>
        /// <remarks>
        /// Enqueues the messages in the internal application queue from which the messages are 
        /// dispatched with precedence over the database queue.
        /// </remarks>
        internal void SendToQueue(EmailMessage message, string siteName)
        {
            EmailToken queueItem = new EmailToken(null, 0, siteName, message.ToMailMessage(siteName));
            emailQueueApp.SynchronizedEnqueue(queueItem);

            // Start resending messages from application queue
            Send(EmailMailoutEnum.OnlyAppQueue);
        }


        /// <summary>
        /// Stops current sending thread.
        /// </summary>
        internal void StopSend()
        {
            if (Sending)
            {
                stopSending = true;

                UpdateQueueState(true);
            }
        }


        /// <summary>
        /// Sends all e-mails one by one in a single thread.
        /// </summary>
        private void Run()
        {
            WindowsImpersonationContext ctx = null;

            try
            {
                ctx = mIdentity.Impersonate();

                // Reset 'Sending' state if set before 30 minutes
                EmailInfoProvider.ResetSendingStatus();

                bool sendAppQueue = (mEmailsToSend == EmailMailoutEnum.OnlyAppQueue);

                // Process application queue first
                if (emailQueueApp.Count > 0)
                {
                    ProcessQueue();
                }

                // Check if sending is enabled (does not affect messages in the application queue)                
                if (EmailHelper.IsAnySendingEnabled() && !sendAppQueue)
                {
                    // Process database queue next
                    if (mEmailIds.Length > 0)
                    {
                        SendSpecific();
                    }
                    else
                    {
                        SendFromQueue();
                    }
                }

                mailoutComplete.WaitOne();

                // Process mass e-mails pending finalization (at this moment all messages have been sent)
                ProcessPendingMassEmail();
            }
            catch (ThreadAbortException)
            {
                // No need to handle or log this (happens when thread is aborted)                
            }
            catch (Exception ex)
            {
                // This is a general catch-all block, exceptions caught here should typically signal
                // handling and API bugs, delivery failures are caught in the callback block
                EventLogProvider.LogException("EmailEngine", "ThreadSender", ex);
            }
            finally
            {
                Init();

                ctx.Undo();
            }
        }


        /// <summary>
        /// Sends the e-mails specified by in the array of e-mail IDs.
        /// </summary>
        private void SendSpecific()
        {
            DataSet emailsSet = EmailInfoProvider.GetEmailInfos(mEmailIds);
            if (!DataHelper.DataSourceIsEmpty(emailsSet))
            {
                // Sets status to 'Sending'
                EmailInfoProvider.SetSendingStatus(mEmailIds);

                SendSet(emailsSet, true);
            }
        }


        /// <summary>
        /// Sends e-mail messages waiting in the DB queue.
        /// </summary>
        private void SendFromQueue()
        {
            int lastEmailId = 0;
            bool hasEmails = true;

            while (!stopSending && hasEmails)
            {
                // Wait until the number of e-mails waiting in the DB queue is lower than the threshold
                fetchThrottle.WaitOne();

                if (!stopSending)
                {
                    // Fetch next set of e-mails to send
                    DataSet emailsSet = Execute(() => EmailInfoProvider.FetchEmails(mEmailsToSend, lastEmailId, failedEmailsNewerThan: mSendFailedEmailsNewerThan));

                    hasEmails = !DataHelper.DataSourceIsEmpty(emailsSet);

                    if (hasEmails)
                    {
                        lastEmailId = GetLastValue(emailsSet, "EmailID");

                        SendSet(emailsSet);
                    }
                }
            }
        }


        /// <summary>
        /// Sends the individual items in the set of e-mails.
        /// </summary>
        /// <param name="emailsSet">The e-mails set that should be mailed out</param>
        /// <param name="setInfoStatus">Indicates if 'sending' status should be set to the processed EmailInfo object to ensure proper update after successful sending</param>
        private void SendSet(DataSet emailsSet, bool setInfoStatus = false)
        {
            foreach (DataRow emailRow in emailsSet.Tables[0].Rows)
            {
                EmailInfo emailInfo = new EmailInfo(emailRow);
                if (setInfoStatus)
                {
                    // Set 'sending' status explicitly
                    emailInfo.EmailStatus = EmailStatusEnum.Sending;
                    emailInfo.Generalized.ResetChanges();
                }

                if (emailInfo.EmailIsMass)
                {
                    ProcessMassEmail(emailInfo, mEmailsToSend);
                }
                else
                {
                    Dispatch(emailInfo);
                }
            }
        }


        /// <summary>
        /// Retrieves e-mail from queue and mass sends it to the specified recipients.
        /// The recipients are fetched in the sets of 10.
        /// </summary>
        /// <param name="message">Email info object</param>        
        /// <param name="emailsToSend">Which e-mail messages should be sent</param>
        private void ProcessMassEmail(EmailInfo message, EmailMailoutEnum emailsToSend)
        {
            int lastMassUserId = 0;
            bool hasEmails = true;

            if (message.EmailStatus == EmailStatusEnum.Archived)
            {
                // Change which email should be sent
                emailsToSend = EmailMailoutEnum.Archived;
            }

            while (hasEmails)
            {
                // Fetch next set of users and send mass e-mail                
                DataSet usersSet = EmailInfoProvider.FetchMassEmail(emailsToSend, message.EmailID, lastMassUserId);
                hasEmails = !DataHelper.DataSourceIsEmpty(usersSet);

                if (hasEmails)
                {
                    // Get ID of last user in the set
                    lastMassUserId = GetLastValue(usersSet, "UserID");

                    foreach (DataRow dr in usersSet.Tables[0].Rows)
                    {
                        EmailInfo cloneMsg = message.Clone();
                        int userId = ValidationHelper.GetInteger(dr["UserID"], 0);
                        cloneMsg.EmailTo = dr["Email"].ToString();

                        Dispatch(cloneMsg, userId);
                    }
                }
            }

            pendingMassEmails.Enqueue(message);
        }


        /// <summary>
        /// Archives, deletes or marks as not sent all the pending mass e-mails.
        /// </summary>
        private void ProcessPendingMassEmail()
        {
            while (pendingMassEmails.Count > 0)
            {
                FinishMassEmail(pendingMassEmails.Dequeue());
            }
        }


        /// <summary>
        /// Dispatches e-mails from the database to the specified users.        
        /// </summary>
        /// <param name="message">E-mail info object</param>
        /// <param name="userId">User ID (used in mass e-mails)</param>
        private void Dispatch(EmailInfo message, int userId = 0)
        {
            string siteName = EmailHelper.GetSiteName(message.EmailSiteID);

            // Do not continue if e-mail sending is disabled for specific site (or global) or sending was stopped
            if (!EmailHelper.Settings.EmailsEnabled(siteName) || stopSending)
            {
                ReturnNotSent(message, userId);
                return;
            }

            try
            {
                // A conversion from EmailMessage to MailMessage may fail so we have to return the e-mail in that case
                MailMessage email = EmailInfoProvider.GetMailMessage(message, siteName);

                emailQueueDB.SynchronizedEnqueue(new EmailToken(message, userId, siteName, email));

                if (DBQueueOverThreshold)
                {
                    fetchThrottle.Reset();
                }

                ProcessQueue();
            }
            catch (Exception ex)
            {
                ReturnSendFailed(message, userId, ex);
            }
        }


        /// <summary>
        /// Processes the items in the e-mail queue.
        /// </summary>
        private void ProcessQueue()
        {
            if (stopSending)
            {
                UpdateQueueState();
                return;
            }

            EmailToken queueItem = Dequeue();
            if (queueItem == null || EmailHelper.DebugEmail(queueItem.Message))
            {
                // If debugging is turned on, just return (the test will simulate send)
                UpdateQueueState();
                return;
            }

            // Try to get available SMTP server
            SMTPServerLookupResult result = SMTPServerLookupTable.Instance.AcquireNext(queueItem.SiteName);

            // Get another SMTP server if the returned one was used during previous unsuccessful sending
            if ((queueItem.SMTPServer != null) && (result.SMTPServer != null) &&
                ((queueItem.SMTPServer.ServerID == result.SMTPServer.ServerID) && (SMTPServerLookupTable.Instance.GetSMTPServerCount(queueItem.SiteName) > 1)))
            {
                SMTPServerLookupResult oldResult = result;

                // Try to get next available SMTP server
                result = SMTPServerLookupTable.Instance.AcquireNext(queueItem.SiteName);

                // Release previous available SMTP server
                SMTPServerLookupTable.Instance.Release(oldResult.SMTPServer);
            }

            switch (result.Availability)
            {
                case SMTPServerAvailabilityEnum.Available:

                    queueItem.SMTPServer = result.SMTPServer;
                    queueItem.LastSendAttempt = DateTime.Now;

                    inTransitList.SynchronizedAdd(queueItem);

                    // There is something to send, we'll have to wait for the callbacks
                    mailoutComplete.Reset();

                    // Send message
                    EmailProvider.SendEmailAsync(queueItem.SiteName, queueItem.Message, queueItem.SMTPServer, queueItem);
                    break;

                case SMTPServerAvailabilityEnum.TemporarilyUnavailable:
                    if (queueItem.SendAttempts > 0)
                    {
                        // Return message to DB
                        EnsureExists(queueItem);
                        ReturnSendFailed(queueItem.Email, queueItem.UserId, new Exception(queueItem.Email.EmailLastSendResult));
                    }
                    else
                    {
                        // Enqueue message if temporarily unavailable
                        Enqueue(queueItem);
                    }

                    if (inTransitList.Count == 0)
                    {
                        // Mailout must go on (otherwise thread may get frozen)
                        UpdateQueueState();
                    }
                    break;

                case SMTPServerAvailabilityEnum.PermanentlyUnavailable:
                    // Return message to DB
                    EnsureExists(queueItem);
                    ReturnNotSent(queueItem.Email, queueItem.UserId);

                    if (inTransitList.Count == 0)
                    {
                        // Mailout must go on (otherwise thread may get frozen)
                        UpdateQueueState();
                    }
                    break;
            }
        }


        /// <summary>
        /// Handles the send completed event. 
        /// Determines if the message was sent successfully and marks the server as idle.
        /// </summary>
        /// <param name="sender">The object that invoked the event.</param>
        /// <param name="e">The <see cref="AsyncCompletedEventArgs"/> instance containing the event data.</param>
        private void SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                // Get email token
                EmailToken sentItem = (EmailToken)e.UserState;

                // Remove item from the list of e-mails that are being sent
                inTransitList.SynchronizedRemove(sentItem);

                // Release SMTP server
                SMTPServerLookupTable.Instance.Release(sentItem.SMTPServer);

                // Process e-mail after an error...
                if (e.Error != null || e.Cancelled)
                {
                    // Increase number of send attempts
                    sentItem.SendAttempts++;

                    if (sentItem.SendAttempts >= 3)
                    {
                        lock (callbackLock)
                        {
                            EnsureExists(sentItem);
                        }
                        // Return email to DB with error message
                        Execute(() => ReturnSendFailed(sentItem.Email, sentItem.UserId, e.Error));
                    }
                    else
                    {
                        if (sentItem.Email != null)
                        {
                            // Set error message
                            sentItem.Email.EmailLastSendResult = GetExceptionMessage(e.Error);
                        }

                        // Return email to app queue
                        Enqueue(sentItem);
                    }
                }
                // Process e-mail after successful sending...
                else
                {
                    if (EmailHelper.Settings.ArchiveEmails(sentItem.SiteName) > 0)
                    {
                        lock (callbackLock)
                        {
                            EnsureExists(sentItem);
                        }
                        // Archive e-mail
                        Execute(() => ArchiveEmail(sentItem.Email, sentItem.UserId));
                    }
                    else if (sentItem.Email != null)
                    {
                        // Delete e-mail
                        Execute(() => DeleteEmail(sentItem.Email, sentItem.UserId));
                    }

                    // Dispose the message
                    sentItem.Message.Dispose();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("EmailEngine", "ThreadSenderCallback", ex);
            }
            finally
            {
                // Mailout must go on (otherwise thread may get frozen)
                UpdateQueueState(true);
            }
        }


        /// <summary>
        /// Checks if there are more items in the queue and continues sending or signals the completion of mailout.
        /// </summary>
        /// <param name="allowProcessQueue">Indicates if further queue processing is enabled</param>
        private void UpdateQueueState(bool allowProcessQueue = false)
        {
            // If stop sending was signaled, then return every message in DB queue back to DB
            if (stopSending)
            {
                EnqueueAllToDB();
            }

            if (!DBQueueOverThreshold)
            {
                fetchThrottle.Set();
            }

            // Process queue if not empty and sending is not canceled
            if (QueuesAreEmpty && (inTransitList.Count == 0))
            {
                mailoutComplete.Set();
            }
            else if (allowProcessQueue)
            {
                ProcessQueue();
            }
        }


        /// <summary>
        /// Enqueues all messages from in-memory DB queue back to the DB.
        /// </summary>
        private void EnqueueAllToDB()
        {
            if (!stoppingSending)
            {
                lock (stopLock)
                {
                    if (!stoppingSending)
                    {
                        stoppingSending = true;

                        try
                        {
                            while (emailQueueDB.Count > 0)
                            {
                                EmailToken queueItem = emailQueueDB.Dequeue();
                                ReturnNotSent(queueItem.Email, queueItem.UserId);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("EmailEngine", "ThreadSenderEnqueueAllToDB", ex);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Enqueues the specified queue item.
        /// </summary>
        /// <param name="queueItem">The queue item</param>
        private void Enqueue(EmailToken queueItem)
        {
            if (queueItem.Email == null)
            {
                emailQueueApp.SynchronizedEnqueue(queueItem);
            }
            else
            {
                emailQueueDB.SynchronizedEnqueue(queueItem);
            }
        }


        /// <summary>
        /// Dequeues next queue item.
        /// </summary>
        /// <returns>Queue item represented by an e-mail token</returns>
        private EmailToken Dequeue()
        {
            if (mEmailsToSend == EmailMailoutEnum.OnlyAppQueue)
            {
                // Dequeue only app queue
                return emailQueueApp.SynchronizedDequeue();
            }

            // Try to dequeue app queue, if empty dequeue DB queue
            return emailQueueApp.SynchronizedDequeue() ?? emailQueueDB.SynchronizedDequeue();
        }


        /// <summary>
        /// Ensures that the e-mail message was saved to the database already.
        /// </summary>
        /// <param name="queueItem">Queue item represented by an e-mail token</param>
        private static void EnsureExists(EmailToken queueItem)
        {
            if (queueItem.Email == null)
            {
                int siteId = EmailHelper.GetSiteId(queueItem.SiteName);
                queueItem.Email = EmailInfoProvider.SetEmailInfo(new EmailMessage(queueItem.Message), siteId, true);
            }
        }


        /// <summary>
        /// Archives the e-mail message.
        /// </summary>
        /// <param name="message"><see cref="EmailInfo" /> object that specifies the email message in DB queue</param>
        /// <param name="userId">User ID</param>
        private static void ArchiveEmail(EmailInfo message, int userId)
        {
            // E-mail was successfully sent and should be archived
            if (message.EmailIsMass)
            {
                EmailUserInfo emailUser = EmailUserInfoProvider.GetEmailUserInfo(message.EmailID, userId);
                if (emailUser != null)
                {
                    EmailUserInfoProvider.ArchiveEmailUser(emailUser);
                }
            }
            else
            {
                EmailInfoProvider.ArchiveEmail(message);
            }
        }


        /// <summary>
        /// Deletes the e-mail message.
        /// </summary>
        /// <param name="message"><see cref="EmailInfo" /> object that specifies the email message in DB queue</param>
        /// <param name="userId">User ID</param>
        private static void DeleteEmail(EmailInfo message, int userId)
        {
            if (message.EmailIsMass)
            {
                EmailUserInfoProvider.DeleteEmailUserInfo(message.EmailID, userId);
            }
            else
            {
                EmailInfoProvider.DeleteEmailInfo(message);
            }
        }


        /// <summary>
        /// The e-mail message was not sent. Returns the e-mail message back to the DB.
        /// </summary>
        /// <param name="message"><see cref="EmailInfo" /> object that specifies the email message in DB queue</param>
        /// <param name="userId">User ID</param>
        private static void ReturnNotSent(EmailInfo message, int userId)
        {
            // E-mail functionality is disabled, no SMTP server exists for this site or send was canceled
            if (message.EmailIsMass)
            {
                EmailUserInfo emailUser = EmailUserInfoProvider.GetEmailUserInfo(message.EmailID, userId);
                if (emailUser != null)
                {
                    EmailUserInfoProvider.SaveNotSentEmailUser(emailUser);
                }
            }
            else
            {
                EmailInfoProvider.SaveNotSentEmail(message);
            }
        }


        /// <summary>
        /// The e-mail messages was sent, but the attempt failed. Returns the e-mail message back to the DB.
        /// </summary>
        /// <param name="message"><see cref="EmailInfo" /> object that specifies the email message in DB queue</param>
        /// <param name="userId">User ID</param>
        /// <param name="ex">The exception that occurred when sending the e-mail message.</param>
        private static void ReturnSendFailed(EmailInfo message, int userId, Exception ex)
        {
            string exception = GetExceptionMessage(ex);

            // Change e-mail status to 'Waiting' and set error message (last send result)
            if (message.EmailIsMass)
            {
                EmailUserInfo emailUser = EmailUserInfoProvider.GetEmailUserInfo(message.EmailID, userId);
                if (emailUser != null)
                {
                    EmailUserInfoProvider.SaveFailedEmailUser(emailUser, exception);
                }
            }
            else
            {
                EmailInfoProvider.SaveFailedEmail(message, exception);
            }
        }


        /// <summary>
        /// After the mass-email is sent, it is either archived, deleted or if sending failed, returned to queue.
        /// </summary>
        /// <param name="message"><see cref="EmailInfo" /> object that specifies the email message in DB queue</param>
        private static void FinishMassEmail(EmailInfo message)
        {
            // Check if there are any unsent messages
            if (!EmailUserInfoProvider.HasFailedEmailUsers(message.EmailID))
            {
                // Archive the pattern e-mail or try to delete it
                if (EmailHelper.Settings.ArchiveEmails(EmailHelper.GetSiteName(message.EmailSiteID)) > 0)
                {
                    EmailInfoProvider.ArchiveEmail(message);
                }
                else
                {
                    EmailInfoProvider.DeleteEmailInfo(message);
                }
            }
            else
            {
                // Preserve pattern e-mail if an error occurs for a binding record
                var errMessage = string.Format(CoreServices.Localization.GetString("emailqueue.queue.sendmassemailfailed"), CoreServices.Localization.GetString("emailqueue.queue.massdetails"));
                EmailInfoProvider.SaveFailedEmail(message, errMessage);
            }
        }


        /// <summary>
        /// Runs the query and in case of a deadlock, retries at a later time (up to 3 times).
        /// </summary>
        /// <param name="query">The SQL query (queries) represented by the <c>Action</c> delegate</param>
        /// <remarks>
        /// Used for non-queries that only perform an operation on DB (UPDATE, etc.) and do not return a result set.
        /// </remarks>        
        private static void Execute(Action query)
        {
            Execute(() =>
                        {
                            query();
                            return null;
                        });
        }


        /// <summary>
        /// Runs the query and in case of a deadlock, retries at a later time (up to 3 times).
        /// </summary>
        /// <param name="query">The SQL query (queries) represented by the <see cref="Func{T}" /> delegate</param>
        /// <returns>A <see cref="DataSet" /> with query's result set</returns>
        /// <remarks>
        /// Used for queries that perform data retrieval (SELECT) and return a result set.
        /// </remarks>
        private static DataSet Execute(Func<DataSet> query)
        {
            int retryCount = 3;
            Random random = new Random();
            SqlException sqlException = null;

            while (retryCount > 0)
            {
                try
                {
                    return query();
                }
                catch (Exception ex)
                {
                    retryCount--;

                    // Catch SQL deadlock exception only, re-throw other types
                    sqlException = ex.InnerException as SqlException;
                    if (sqlException.HasDeadlockOccured() && !CMSTransactionScope.IsInTransaction && (retryCount > 0))
                    {
                        Thread.Sleep(random.Next(500, 3000));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the value of the specified column for the last record in the DataSet.
        /// </summary>
        /// <param name="dataSet">The data set</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Value of the last record</returns>
        private static int GetLastValue(DataSet dataSet, string columnName)
        {
            DataRowCollection rows = dataSet.Tables[0].Rows;
            return DataHelper.GetIntValue(rows[rows.Count - 1], columnName);
        }


        /// <summary>
        /// Gets exception with message.
        /// </summary>
        /// <param name="ex">Exception</param>
        private static string GetExceptionMessage(Exception ex)
        {
            if (ex == null)
            {
                return null;
            }

            StringBuilder message = new StringBuilder();
            AppendException(message, ex);

            // Add inner exception stack trace
            Exception inner;
            while ((inner = ex.InnerException) != null)
            {
                AppendException(message, inner);

                ex = inner;
            }

            return message.ToString();
        }


        /// <summary>
        /// Appends the exception to the message.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="ex">Exception to append</param>
        private static void AppendException(StringBuilder message, Exception ex)
        {
            if (message.Length > 0)
            {
                message.AppendLine().AppendLine();
            }

            message.Append(ex.Message);
        }
        

        /// <summary>
        /// Returns query for all emails that are in waiting state for given site. 
        /// </summary>
        private ObjectQuery<EmailInfo> PrepareSiteWaitingEmails(int siteId)
        {
            return EmailInfoProvider.GetEmails()
                .WhereEquals(nameof(EmailInfo.EmailSiteID), siteId)
                .WhereEquals(nameof(EmailInfo.EmailStatus), EmailStatusEnum.Waiting)
                .Column(nameof(EmailInfo.EmailID));
        }

        #endregion
    }
}
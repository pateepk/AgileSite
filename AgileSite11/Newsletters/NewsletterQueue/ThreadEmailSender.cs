using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;

using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Newsletters.Filters;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Async e-mail sender. Gets e-mails from newsletter queue, prepares them and sends them to e-mail queue.
    /// </summary>    
    public class ThreadEmailSender
    {
        #region "Variables"

        /// <summary>
        /// Resend failed e-mails.
        /// </summary>
        protected bool mSendFailed = true;


        /// <summary>
        /// Send new e-mails.
        /// </summary>
        protected bool mSendNew = true;


        /// <summary>
        /// Issue ID.
        /// </summary>
        protected int mIssueID;


        /// <summary>
        /// Threads currently sending e-mails.
        /// </summary>
        private static int mSendingThreads;


        /// <summary>
        /// Windows identity.
        /// </summary>
        private WindowsIdentity mWindowsIdentity;


        /// <summary>
        /// Used to cancel the sending if set to 'false'.
        /// </summary>
        private static bool mAllowSending = true;


        /// <summary>
        /// Thread lock for scheduled sending.
        /// </summary>
        private static readonly object threadLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Number of threads that are currently sending issues.
        /// </summary>
        public static int SendingThreads
        {
            get
            {
                return mSendingThreads;
            }
        }


        /// <summary>
        /// Send failed e-mails?
        /// </summary>
        public bool SendFailed
        {
            get
            {
                return mSendFailed;
            }
            set
            {
                mSendFailed = value;
            }
        }


        /// <summary>
        /// Send new e-mails?
        /// </summary>
        public bool SendNew
        {
            get
            {
                return mSendNew;
            }
            set
            {
                mSendNew = value;
            }
        }


        /// <summary>
        /// IssueID, optional; only specific issue is send-out if it is set.
        /// </summary>
        public int IssueID
        {
            get
            {
                return mIssueID;
            }
            set
            {
                mIssueID = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Stops current sending process.
        /// </summary>
        public static void CancelSending()
        {
            if (mSendingThreads > 0)
            {
                mAllowSending = false;
            }
        }


        /// <summary>
        /// Runs the sender in an asynchronous thread.
        /// </summary>
        /// <param name="wi">Windows identity</param>
        public void RunAsync(WindowsIdentity wi)
        {
            if (mSendingThreads > 0)
            {
                return;
            }

            // Store windows identity
            mWindowsIdentity = wi;

            new CMSThread(Run).Start();
        }


        /// <summary>
        /// Sends all e-mails one by one in one thread.
        /// </summary>
        public void Run()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;

            if (mSendingThreads > 0)
            {
                return;
            }

            lock (threadLock)
            {
                if (mSendingThreads > 0)
                {
                    return;
                }

                try
                {
                    mSendingThreads = 1;

                    // Impersonate current thread
                    ctx = mWindowsIdentity.Impersonate();

                    if (SendFailed)
                    {
                        ResetSendingStatus();
                    }

                    if (IssueID <= 0)
                    {
                        // Get issue ID of the first item from newsletter queue which is not A/B test
                        IssueID = GetIssueIDToSend(SendNew, SendFailed);
                    }

                    var issue = IssueInfoProvider.GetIssueInfo(IssueID);
                    if (issue == null)
                    {
                        return;
                    }

                    // Create cloned object not to influence the original object
                    var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
                    var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);
                    string siteName = site.SiteName;

                    // Check if newsletter sending is enabled even if e-mail sending is disabled
                    if (!EmailHelper.Settings.EmailsEnabled(siteName) && !NewsletterHelper.GenerateEmailsEnabled(siteName))
                    {
                        return;
                    }

                    // Set 'Sending' status to current issue
                    IssueInfoProvider.SetIssueStatus(IssueID, IssueStatusEnum.Sending);

                    var resolver = ContentFilterResolvers.GetQueueResolver(issue, newsletter);
                    var macroFilter = new MacroResolverContentFilter(resolver);
                    var bodyFilter = new EmailQueueContentFilter(issue, newsletter, resolver, false);
                    var parts = new EmailParts(issue, newsletter);
                    parts.ApplyFilters(bodyFilter, macroFilter, macroFilter);

                    SendEmailsViaQueueManager(parts, site);

                    IssueInfoProvider.SetIssueStatus(IssueID, IssueStatusEnum.Finished);

                    if (issue.IssueIsVariant)
                    {
                        EnsureWinnerSelectionTask(issue.IssueVariantOfIssueID);
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Newsletter", "ThreadEmailSender", ex);
                }
                finally
                {
                    // Reset parameters
                    mSendingThreads = 0;
                    mAllowSending = true;

                    // Undo impersonation
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Returns IssueID of the top item in newsletter queue which is not A/B test or 0.
        /// </summary>
        /// <param name="getNew">Indicates if new issues can be used</param>
        /// <param name="getFailed">Indicates if failed issues can be used</param>
        private static int GetIssueIDToSend(bool getNew, bool getFailed)
        {
            int result = 0;

            string where = "(EmailSending IS NULL OR EmailSending = 0) AND (NOT EmailNewsletterIssueID IN (SELECT TestIssueID FROM Newsletter_ABTest WHERE TestWinnerSelected IS NULL)) AND (GetDate() > (SELECT IssueMailoutTime FROM Newsletter_NewsletterIssue WHERE IssueID = EmailNewsletterIssueID))";
            if (getNew && !getFailed)
            {
                @where = SqlHelper.AddWhereCondition(@where, "(EmailLastSendResult IS NULL OR EmailLastSendResult LIKE '')");
            }
            else if (!getNew && getFailed)
            {
                @where = SqlHelper.AddWhereCondition(@where, "NOT (EmailLastSendResult IS NULL OR EmailLastSendResult LIKE '')");
            }

            // Get TOP 1 item from newsletter queue that is in waiting status and wasn't sent before
            var emails = EmailQueueItemInfoProvider.GetEmailQueueItems().Where(@where).TopN(1);
            if (emails.Any())
            {
                result = ValidationHelper.GetInteger(emails.First().EmailNewsletterIssueID, 0);
            }

            return result;
        }


        private void SendEmailsViaQueueManager(EmailParts emailParts, SiteInfo site)
        {
            var issueIsVariant = emailParts.Issue.IssueIsVariant;

            // Remove all duplicates from newsletter queue (duplicate is row with same EmailAddress and EmailNewsletterIssueID as other row).
            ConnectionHelper.ExecuteQuery("newsletter.emails.removeduplicates", null);

            if (issueIsVariant)
            {
                SendIssueVariantsViaQueueManager(emailParts, site);
            }
            else
            {
                SendIssueViaQueueManager(emailParts, site);
            }
        }


        private void SendIssueVariantsViaQueueManager(EmailParts emailParts, SiteInfo site)
        {
            var totalVariantsSentCount = 0;
            var lastEmailGuid = Guid.Empty;
            var firstEmailGuid = Guid.Empty;

            while (mAllowSending)
            {
                try
                {
                    // Get e-mails for test variant (ordered by GUID)
                    var remainingVariantsToSendCount = GetMailsToSend(emailParts.Issue);
                    var topN = remainingVariantsToSendCount - totalVariantsSentCount > 10 ? 10 : remainingVariantsToSendCount - totalVariantsSentCount;
                    var data = EmailQueueManager.FetchVariantEmailsToSend(SendFailed, SendNew, lastEmailGuid, emailParts.Issue.IssueVariantOfIssueID, topN);

                    if (DataHelper.DataSourceIsEmpty(data))
                    {
                        // No e-mails to send -> finish sending
                        firstEmailGuid = Guid.Empty;
                        lastEmailGuid = Guid.Empty;
                        break;
                    }

                    firstEmailGuid = Guid.Empty;
                    var sentEmailsCountInCurrentBatch = 0;

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        var email = new EmailQueueItemInfo(dr);

                        if (firstEmailGuid == Guid.Empty)
                        {
                            firstEmailGuid = email.EmailGUID;
                        }

                        lastEmailGuid = email.EmailGUID;

                        if (EmailQueueManager.SendEmail(email, emailParts, site.SiteName))
                        {
                            sentEmailsCountInCurrentBatch++;
                        }

                        totalVariantsSentCount++;
                    }

                    if (sentEmailsCountInCurrentBatch > 0)
                    {
                        // Update number of sent e-mails of the issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueID, sentEmailsCountInCurrentBatch, DateTime.Now);
                        // Update number of sent e-mails of the parent issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueVariantOfIssueID, sentEmailsCountInCurrentBatch, DateTime.Now);
                    }
                }
                finally
                {
                    if (lastEmailGuid != Guid.Empty)
                    {
                        // The method requires the items to be ordered (by EmailGUID).
                        DeleteSentEmailMessageVariants(firstEmailGuid, lastEmailGuid, emailParts.Issue.IssueVariantOfIssueID);
                    }
                }
            }
        }


        private void SendIssueViaQueueManager(EmailParts emailParts, SiteInfo site)
        {
            const int TOP_N = 10;
            var lastEmailId = 0;
            var firstEmailId = 0;

            while (mAllowSending)
            {
                try
                {
                    // Get e-mails for normal issue (ordered by ID)
                    var data = EmailQueueManager.FetchEmailsToSend(SendFailed, SendNew, lastEmailId, IssueID, TOP_N);

                    if (DataHelper.DataSourceIsEmpty(data))
                    {
                        // No e-mails to send -> finish sending
                        firstEmailId = 0;
                        lastEmailId = 0;
                        break;
                    }

                    firstEmailId = 0;
                    var sentEmailsCountInCurrentBatch = 0;

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        var email = new EmailQueueItemInfo(dr);
                        if (firstEmailId == 0)
                        {
                            firstEmailId = email.EmailID;
                        }

                        lastEmailId = email.EmailID;

                        if (EmailQueueManager.SendEmail(email, emailParts, site.SiteName))
                        {
                            sentEmailsCountInCurrentBatch++;
                        }
                    }

                    if (sentEmailsCountInCurrentBatch > 0)
                    {
                        // Update number of sent e-mails of the issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueID, sentEmailsCountInCurrentBatch, DateTime.Now);
                    }
                }
                finally
                {
                    if (lastEmailId != 0)
                    {
                        // The method requires the items to be ordered (by EmailID).
                        DeleteSentEmailMessages(firstEmailId, lastEmailId);
                    }
                }
            }
        }


        private void DeleteSentEmailMessages(int firstEmailId, int lastEmailId)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailNewsletterIssueID", IssueID)
                .WhereTrue("EmailSending")
                .WhereGreaterOrEquals("EmailID", firstEmailId)
                .WhereLessOrEquals("EmailID", lastEmailId)
                .And(w => w.WhereNull("EmailLastSendResult").Or().WhereEmpty("EmailLastSendResult"));

            EmailQueueItemInfoProvider.DeleteEmailQueueItem(where);
        }


        private void DeleteSentEmailMessageVariants(Guid firstEmailGuid, Guid lastEmailGuid, int issueId)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailNewsletterIssueID", issueId)
                .WhereTrue("EmailSending")
                .WhereGreaterOrEquals("EmailGUID", firstEmailGuid)
                .WhereLessOrEquals("EmailGUID", lastEmailGuid)
                .And(w => w.WhereNull("EmailLastSendResult").Or().WhereEmpty("EmailLastSendResult"));

            EmailQueueItemInfoProvider.DeleteEmailQueueItem(where);
        }


        private void ResetSendingStatus()
        {
            try
            {
                ConnectionHelper.ExecuteQuery("newsletter.emails.resetstatus", null);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ThreadEmailSender", "ResetSendingStatus", ex);
            }
        }


        /// <summary>
        /// Returns number of emails that should be sent for current variant.
        /// Set 'Test phase' status to the parent issue.
        /// </summary>
        private static int GetMailsToSend(IssueInfo issue)
        {
            int mailsToSend = -1;
            var abTest = ABTestInfoProvider.GetABTestInfoForIssue(issue.IssueVariantOfIssueID);

            if (abTest != null)
            {
                // Set number of emails that should be sent for current variant
                mailsToSend = abTest.TestNumberPerVariantEmails - issue.IssueSentEmails;
            }

            // Do not update status if winner was selected before sending this variant
            if ((abTest == null) || (abTest.TestWinnerIssueID <= 0) && (abTest.TestWinnerSelected == DateTimeHelper.ZERO_TIME))
            {
                // Set 'Test phase' status to the parent issue
                IssueInfoProvider.SetIssueStatus(issue.IssueVariantOfIssueID, IssueStatusEnum.TestPhase);
            }
            return mailsToSend;
        }


        /// <summary>
        /// Creates/updates winner selection task and enable it if all variants were sent.
        /// </summary>
        private static void EnsureWinnerSelectionTask(int parentIssueId)
        {
            var abTest = ABTestInfoProvider.GetABTestInfoForIssue(parentIssueId);

            // Get list with variants that are not sent yet
            var notSentVariants = IssueHelper.GetIssueVariants(parentIssueId, string.Format("IssueStatus <> {0} OR IssueStatus IS NULL", (int)IssueStatusEnum.Finished));
            if ((notSentVariants == null) || !notSentVariants.Any())
            {
                // Create/update winner selection task and enable it if all variants were sent
                NewsletterTasksManager.EnsureWinnerSelectionTask(abTest, null, true, DateTime.Now);
            }
        }

        #endregion
    }
}
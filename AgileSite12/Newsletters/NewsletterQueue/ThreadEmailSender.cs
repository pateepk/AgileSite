using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Query;
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
        /// Issue ID.
        /// </summary>
        protected int mIssueID;


        internal ILicenseService mLicenseService;


        /// <summary>
        /// Threads currently sending e-mails.
        /// </summary>
        private static int mSendingThreads;


        /// <summary>
        /// Windows identity.
        /// </summary>
        private WindowsIdentity mWindowsIdentity;


        /// <summary>
        /// Used to signal that one of the settings affecting enabled state of email sending has changed.
        /// </summary>
        private volatile static bool mEmailsEnabledSettingChanged = false;


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


        internal ILicenseService LicenseService
        {
            get => mLicenseService ?? ObjectFactory<ILicenseService>.StaticSingleton();
            set => mLicenseService = value;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// <para>
        /// Notifies the sending process that one of the settings affecting whether email sending is allowed has changed.
        /// As a result the sending process re-evaluates whether emails can still be sent with respect to currently processed newsletter's site.
        /// The email sending process is driven by 'CMSEmailsEnabled' and 'CMSGenerateNewsletters' settings.
        /// </para>
        /// <para>
        /// A web farm task propagating the notification is optionally logged.
        /// </para>
        /// </summary>
        /// <seealso cref="EmailHelper.Settings.EmailsEnabled"/>
        /// <seealso cref="NewsletterHelper.GenerateEmailsEnabled"/>
        internal static void NotifyEmailsEnabledSettingChanged(bool logTask = true)
        {
            if (mSendingThreads > 0)
            {
                mEmailsEnabledSettingChanged = true;
            }

            if (logTask)
            {
                WebFarmHelper.CreateTask(new EmailsEnabledSettingChangedWebFarmTask());
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
                    // Impersonate current thread
                    ctx = mWindowsIdentity.Impersonate();

                    mSendingThreads = 1;

                    if (SendFailed)
                    {
                        NewsletterSendingStatusModifier.ResetFailedEmailsInQueue();
                    }

                    if (IssueID <= 0)
                    {
                        // Get issue ID of the first item from newsletter queue which is not A/B test
                        IssueID = GetIssueIDToSend(SendFailed);
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
                    if (!EmailsEnabled(siteName))
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
                    mEmailsEnabledSettingChanged = false;

                    // Undo impersonation
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Checks if newsletter sending is enabled even if e-mail sending is disabled.
        /// </summary>
        private bool EmailsEnabled(string siteName)
        {
            return EmailHelper.Settings.EmailsEnabled(siteName) || NewsletterHelper.GenerateEmailsEnabled(siteName);
        }


        /// <summary>
        /// Returns IssueID of the top item in newsletter queue which is not A/B test or 0.
        /// </summary>
        /// <param name="getFailed">Indicates if failed issues can be used</param>
        internal int GetIssueIDToSend(bool getFailed)
        {
            var dateTimeNowService = Service.Resolve<IDateTimeNowService>();
            var result = 0;
            
            var mailOutTime = IssueInfoProvider.GetIssues()
                                               .Column("IssueMailoutTime")
                                               .WhereEquals("IssueID", "EmailNewsletterIssueID".AsColumn());

            var condition = new WhereCondition()
                                       .Where(new WhereCondition()
                                              .WhereNull("EmailSending").Or().WhereFalse("EmailSending"))
                                       .WhereLessOrEquals(mailOutTime, dateTimeNowService.GetDateTimeNow());

            if (IsABTestingFeatureAvailable())
            {
                var abTests = ABTestInfoProvider.GetABTests()
                                                .Column("TestIssueID")
                                                .WhereNull("TestWinnerSelected");

                condition = condition.WhereNotIn("EmailNewsletterIssueID", abTests);
            }

            if (!getFailed)
            {
                condition = condition.WhereEmpty("EmailLastSendResult");
            }

            var emails = EmailQueueItemInfoProvider.GetEmailQueueItems().Where(condition).TopN(1);
            if (emails.Any())
            {
                result = ValidationHelper.GetInteger(emails.First().EmailNewsletterIssueID, 0);
            }

            return result;
        }


        private bool IsABTestingFeatureAvailable()
        {
            return LicenseService.IsFeatureAvailable(FeatureEnum.NewsletterABTesting, RequestContext.CurrentDomain);
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
            var sentEmailGuids = new List<Guid>();

            SetTestPhaseStatus(emailParts, site);
            var variantEmailsCount = GetVariantEmailsCount(emailParts.Issue);

            while (!mEmailsEnabledSettingChanged || EmailsEnabled(site.SiteName))
            {
                if (mEmailsEnabledSettingChanged)
                {
                    mEmailsEnabledSettingChanged = false;
                }

                try
                {                  
                    var topN = variantEmailsCount - totalVariantsSentCount > 10 ? 10 : variantEmailsCount - totalVariantsSentCount;
                    var data = EmailQueueManager.FetchVariantEmailsToSend(SendFailed, lastEmailGuid, emailParts.Issue.IssueVariantOfIssueID, topN);

                    if (DataHelper.DataSourceIsEmpty(data))
                    {
                        break;
                    }

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        var email = new EmailQueueItemInfo(dr);
                        if (EmailQueueManager.SendEmail(email, emailParts, site.SiteName))
                        {
                            totalVariantsSentCount++;
                            sentEmailGuids.Add(email.EmailGUID);
                        }
                    }
                }
                finally
                {
                    if (sentEmailGuids.Any())
                    {
                        // The method requires the items to be ordered (by EmailGUID).
                        DeleteSentEmailMessageVariants(sentEmailGuids, emailParts.Issue.IssueVariantOfIssueID);

                        // Update number of sent e-mails of the issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueID, sentEmailGuids.Count, DateTime.Now);
                        // Update number of sent e-mails of the parent issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueVariantOfIssueID, sentEmailGuids.Count, DateTime.Now);

                        sentEmailGuids.Clear();
                    }
                }
            }
        }

        private void SetTestPhaseStatus(EmailParts emailParts, SiteInfo site)
        {
            if (!EmailsEnabled(site.SiteName))
            {
                return;
            }

            if (!IsWinnerSelected(emailParts))
            {
                IssueInfoProvider.SetIssueStatus(emailParts.Issue.IssueVariantOfIssueID, IssueStatusEnum.TestPhase);
            }
        }


        private static bool IsWinnerSelected(EmailParts emailParts)
        {
            var abTest = ABTestInfoProvider.GetABTestInfoForIssue(emailParts.Issue.IssueVariantOfIssueID);
            return abTest != null && (abTest.TestWinnerIssueID > 0 || abTest.TestWinnerSelected != DateTimeHelper.ZERO_TIME);
        }


        private void SendIssueViaQueueManager(EmailParts emailParts, SiteInfo site)
        {
            const int TOP_N = 10;
            var lastEmailId = 0;
            var sentEmailIds = new List<int>();

            while (!mEmailsEnabledSettingChanged || EmailsEnabled(site.SiteName))
            {
                if (mEmailsEnabledSettingChanged)
                {
                    mEmailsEnabledSettingChanged = false;
                }

                try
                {
                    // Get e-mails for normal issue (ordered by ID)
                    var data = EmailQueueManager.FetchEmailsToSend(SendFailed, lastEmailId, IssueID, TOP_N);

                    if (DataHelper.DataSourceIsEmpty(data))
                    {
                        // No e-mails to send -> finish sending
                        break;
                    }

                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        var email = new EmailQueueItemInfo(dr);
                        if (EmailQueueManager.SendEmail(email, emailParts, site.SiteName))
                        {
                            sentEmailIds.Add(email.EmailID);
                            lastEmailId = email.EmailID;
                        }
                    }
                }
                finally
                {
                    if (sentEmailIds.Any())
                    {
                        // The method requires the items to be ordered (by EmailID).
                        DeleteSentEmailMessages(sentEmailIds);

                        // Update number of sent e-mails of the issue
                        IssueInfoProvider.AddSentEmails(emailParts.Issue.IssueID, sentEmailIds.Count, DateTime.Now);

                        sentEmailIds.Clear();
                    }
                }
            }
        }


        private void DeleteSentEmailMessages(ICollection<int> emailIdsToDelete)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailNewsletterIssueID", IssueID)
                .WhereTrue("EmailSending")
                .WhereIn("EmailID", emailIdsToDelete)
                .And(w => w.WhereNull("EmailLastSendResult").Or().WhereEmpty("EmailLastSendResult"));

            EmailQueueItemInfoProvider.DeleteEmailQueueItem(where);
        }


        private void DeleteSentEmailMessageVariants(ICollection<Guid> emailGuidsToDelete, int issueId)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailNewsletterIssueID", issueId)
                .WhereTrue("EmailSending")
                .WhereIn("EmailGUID", emailGuidsToDelete)
                .And(w => w.WhereNull("EmailLastSendResult").Or().WhereEmpty("EmailLastSendResult"));

            EmailQueueItemInfoProvider.DeleteEmailQueueItem(where);
        }


        /// <summary>
        /// Returns number of emails that should be sent for current variant.
        /// </summary>
        private int GetVariantEmailsCount(IssueInfo issue)
        {
            var abTest = ABTestInfoProvider.GetABTestInfoForIssue(issue.IssueVariantOfIssueID);
            return abTest != null ? abTest.TestNumberPerVariantEmails : -1;
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.Base;
using CMS.Newsletters.Filters;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.Newsletters
{
    /// <summary>
    /// Manages generation and sending of EmailQueueItems (newsletter queue).
    /// </summary>
    public static class EmailQueueManager
    {
        #region "Constants"

        /// <summary>
        /// Do not send constant.
        /// </summary>
        private const string DO_NOT_SEND = "##DONOTSEND##";

        #endregion


        #region "Variables"

        // Regular expression to match the page encoding
        private static Regex mEncodingRegExp;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets page encoding regular expression.
        /// </summary>
        private static Regex EncodingRegExp
        {
            get
            {
                return mEncodingRegExp ?? (mEncodingRegExp = RegexHelper.GetRegex("<meta[^>]+charset\\s*?=\\s*\"?([^\"]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline));
            }
        }

        #endregion


        #region "Sending methods"


        /// <summary>
        /// Tries to send all e-mails from the newsletter queue.
        /// </summary>
        /// <param name="sendFailed">Indicates if failed emails should be sent.</param>
        /// <param name="issueId">Context of an issue for which the emails should be sent. If not provided, context is gathered from the first item in the queue.</param>
        public static void SendAllEmails(bool sendFailed = true, int issueId = 0)
        {
            // Check license
            if (!String.IsNullOrEmpty(DataHelper.GetNotEmpty(RequestContext.CurrentDomain, String.Empty)))
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Newsletters);
            }

            var sender = new ThreadEmailSender
            {
                SendFailed = sendFailed,
                IssueID = issueId
            };

            sender.RunAsync(WindowsIdentity.GetCurrent());
        }


        /// <summary>
        /// Sends specified e-mail from the newsletter queue, failed e-mail is updated with ErrorMessage information.
        /// </summary>
        /// <param name="email">E-mail to be sent</param>
        /// <param name="emailParts">E-mail parts to be resolved and composed</param>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns true if e-mail has been successfully send</returns>
        internal static bool SendEmail(EmailQueueItemInfo email, EmailParts emailParts, string siteName)
        {
            // Save current thread's culture and temporarily switch it for site's default culture
            string culture = CultureHelper.GetDefaultCultureCode(siteName);
            CultureInfo oldUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureHelper.GetCultureInfo(culture);

            string lastError = null;
            bool sent = false;

            try
            {
                var subscriberMember = GetSubscriberMember(email);

                if (IsValidSubscriber(subscriberMember))
                {
                    var issue = emailParts.Issue;
                    var newsletter = emailParts.Newsletter;
                    var resolver = ContentFilterResolvers.GetSendResolver(issue, newsletter, subscriberMember);
                    var macroFilter = new MacroResolverContentFilter(resolver);

                    var baseUrl = emailParts.BaseUrl;
                    var bodyFilter = new EmailSendContentFilter(issue, newsletter, resolver, subscriberMember, baseUrl, false);

                    var subscriberEmailParts = emailParts.Clone();
                    subscriberEmailParts.ApplyFilters(bodyFilter, macroFilter, macroFilter);

                    var modifier = new EmailMessageModifier(issue, subscriberMember);
                    var message = new EmailMessageBuilder(subscriberEmailParts, modifier).Build();

                    EmailSender.SendEmail(siteName, message);
                    sent = true;
                }
            }
            catch (Exception ex)
            {
                lastError = EventLogProvider.GetExceptionLogMessage(ex);
                EventLogProvider.LogException("Newsletter", "EmailQueueManager", ex);
            }
            finally
            {
                // Set back to old culture
                Thread.CurrentThread.CurrentUICulture = oldUICulture;
            }

            if (lastError != null)
            {
                // Update the e-mail record if error occurred
                email.EmailLastSendResult = lastError;
                email.EmailLastSendAttempt = DateTime.Now;
                email.EmailSending = false;

                EmailQueueItemInfoProvider.SetEmailQueueItem(email);
            }

            return sent;
        }


        private static bool IsValidSubscriber(SubscriberInfo sbMember)
        {
            return sbMember != null && ValidationHelper.IsEmail(sbMember.SubscriberEmail);
        }


        private static SubscriberInfo GetSubscriberMember(EmailQueueItemInfo email)
        {
            var source = GetSubscriber(email) ?? new SubscriberInfo();

            var member = SubscriberInfoProvider.CreateSubscriberFromContact(email.EmailContactID, source);
            if (member != null)
            {
                member.SubscriberEmail = email.EmailAddress;
            }

            return member;
        }


        private static SubscriberInfo GetSubscriber(EmailQueueItemInfo email)
        {
            if (email.EmailSubscriberID <= 0)
            {
                return null;
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(email.EmailSubscriberID);

            // Check if subscriber belongs to newsletter's site
            if (subscriber == null || subscriber.SubscriberSiteID == email.EmailSiteID)
            {
                return subscriber;
            }

            EventLogProvider.LogEvent(EventType.WARNING, "Newsletter", "EmailQueueManager", $"Subscriber '{subscriber.SubscriberFullName}' does not belong to the site '{SiteInfoProvider.GetSiteName(email.EmailSiteID)}'.");
            return null;
        }


        /// <summary>
        /// Clears the sending status for the corrupted e-mails.
        /// </summary>
        public static void ClearEmailsSendingStatus()
        {
            // Create the thread and send the message
            CMSThread thread = new CMSThread(ClearEmailsSendingStatusInternal);

            thread.Start();
        }


        /// <summary>
        /// Clears the sending status for the corrupted e-mails.
        /// </summary>
        private static void ClearEmailsSendingStatusInternal()
        {
            // Impersonation context
            using (WindowsIdentity.GetCurrent().Impersonate())
            {
                ConnectionHelper.ExecuteQuery("newsletter.emails.clearsendingstatus", null);
            }

            // Ensure scheduled tasks of 'sending' A/B test variants
            var issues = IssueInfoProvider.GetIssues()
                                          .WhereTrue("IssueIsABTest")
                                          .Where("IssueVariantOfIssueID", QueryOperator.LargerThan, 0)
                                          .WhereEquals("IssueStatus", (int)IssueStatusEnum.Sending);

            foreach (var issue in issues)
            {
                // Return existing scheduled task or create new one
                TaskInfo task = NewsletterTasksManager.EnsureMailoutTask(issue, DateTime.Now, true);
                // Save task
                TaskInfoProvider.SetTaskInfo(task);

                if (issue.IssueScheduledTaskID != task.TaskID)
                {
                    // Update issue
                    issue.IssueScheduledTaskID = task.TaskID;
                    IssueInfoProvider.SetIssueInfo(issue);
                }
            }
        }

        #endregion //"Sending methods"


        #region "Fetching methods"

        /// <summary>
        /// Fetches the e-mails which should be sent from database and marks them as being sent.
        /// If enabled all e-mails with status 'sending' which have EmailID smaller then or equal to 'firstEmailId' will be deleted.
        /// </summary>
        /// <param name="fetchFailed">If true, failed e-mails are fetched</param>
        /// <param name="firstEmailId">First e-mail ID (all e-mails should have the same or larger ID)</param>
        /// <param name="issueId">If set, only e-mails of specified issue will be fetched</param>
        /// <param name="topN">Number of e-mails to fetch</param>
        internal static DataSet FetchEmailsToSend(bool fetchFailed, int firstEmailId, int issueId, int topN)
        {
            return FetchEmailsToSendInternal(fetchFailed, firstEmailId, Guid.Empty, issueId, topN);
        }


        /// <summary>
        /// Fetches the variant e-mails which should be sent from database and marks them as being sent.
        /// If enabled all e-mails with status 'sending' which have EmailGUID smaller then or equal to 'firstEmailGuid' will be deleted.
        /// </summary>
        /// <param name="fetchFailed">If true, failed e-mails are fetched</param>
        /// <param name="firstEmailGuid">First e-mail GUID (all e-mails should have the same or larger GUID)</param>
        /// <param name="issueId">If set, only e-mails of specified issue will be fetched</param>
        /// <param name="topN">Number of e-mails to fetch</param>
        internal static DataSet FetchVariantEmailsToSend(bool fetchFailed, Guid firstEmailGuid, int issueId, int topN)
        {
            return FetchEmailsToSendInternal(fetchFailed, -1, firstEmailGuid, issueId, topN);
        }


        /// <summary>
        /// Fetches the e-mails which should be sent from database and marks them as being sent.
        /// If enabled all e-mails with status 'sending' which have EmailID smaller then or equal to 'firstEmailId' will be deleted.
        /// </summary>
        /// <param name="fetchFailed">If true, failed e-mails are fetched</param>
        /// <param name="firstEmailId">First e-mail ID (all e-mails should have the same or larger ID)</param>
        /// <param name="firstEmailGuid">First e-mail GUID (all e-mails should have the same or larger GUID)</param>
        /// <param name="issueId">If set, only e-mails of specified issue will be fetched</param>
        /// <param name="topN">Number of e-mails to fetch</param>
        private static DataSet FetchEmailsToSendInternal(bool fetchFailed, int firstEmailId, Guid firstEmailGuid, int issueId, int topN)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@FetchFailed", fetchFailed);
            parameters.Add("@FirstEmailID", firstEmailId);
            parameters.Add("@FirstEmailGuid", firstEmailGuid);
            parameters.Add("@IssueID", issueId);
            parameters.Add("@TopN", topN >= 0 ? topN : 10);
            parameters.Add("@IsVariant", firstEmailId < 0);
            parameters.Add("@LastSendAttempt", DateTime.Now);

            // Get the data
            return ConnectionHelper.ExecuteQuery("newsletter.emails.fetchemailstosend", parameters);
        }

        #endregion //"Fetching methods"


        #region "Methods for generating newsletter queue"

        /// <summary>
        /// Generates emails of given issue into e-mail queue.
        /// </summary>
        /// <param name="issueID">Issue ID</param>
        /// <exception cref="Exception">More than 100 emails are generated with license not providing access to FullContactManagement</exception>
        public static void GenerateEmails(int issueID)
        {
            // Get the issue
            IssueInfo issue = IssueInfoProvider.GetIssueInfo(issueID);
            GenerateEmails(issue);
        }


        /// <summary>
        /// Generates e-mails of given issue into e-mail queue.
        /// </summary>
        /// <param name="issue">Issue object</param>
        /// <exception cref="Exception">More than 100 emails are generated with license not providing access to FullContactManagement</exception>
        public static void GenerateEmails(IssueInfo issue)
        {
            if (issue == null)
            {
                return;
            }

            SiteInfo site = SiteInfoProvider.GetSiteInfo(issue.IssueSiteID);
            if (site == null)
            {
                return;
            }

            // Check license version limitations of newsletters and subscribers
            // Return if counts are not fulfilled
            if (!NewsletterHelper.LicenseVersionCheck(site.DomainName, FeatureEnum.Newsletters, ObjectActionEnum.Edit, false))
            {
                // Log event
                string message = FeatureEnum.Newsletters.ToString();
                EventLogProvider.LogEvent(EventType.WARNING, message, LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, RequestContext.CurrentURL);

                return;
            }

            var emailFilter = new PreloadedUnsubscribedEmailsAddressBlocker(issue.IssueNewsletterID);

            try
            {
                using (var h = NewsletterEvents.GenerateQueueItems.Start(issue, null, emailFilter))
                {
                    if (h.CanContinue())
                    {
                        // Change issue's status to 'Preparing data'
                        IssueInfoProvider.SetIssueStatus(issue.IssueID, IssueStatusEnum.PreparingData);
                        if (issue.IssueIsVariant)
                        {
                            // Change also parent's status when sending A/B test
                            IssueInfoProvider.SetIssueStatus(issue.IssueVariantOfIssueID, IssueStatusEnum.PreparingData);
                        }

                        // Get info if bounced email monitoring is enabled and what the bounce limit is
                        bool monitorBounces = NewsletterHelper.MonitorBouncedEmails(site.SiteName);
                        int bounceLimit = NewsletterHelper.BouncedEmailsLimit(site.SiteName);

                        // Generate records for subscribed contact groups and contacts
                        GenerateContactEmails(issue, site, monitorBounces, bounceLimit, h.EventArguments.GeneratedEmails, emailFilter);

                        // Clear sending status so the sending may start
                        EmailQueueItemInfoProvider.ClearSendingStatus(issue);

                        if (issue.IssueIsVariant)
                        {
                            // Initialize number of e-mails that should be sent for each variant
                            IssueHelper.InitABTestNumbers(issue.IssueVariantOfIssueID);
                        }
                    }

                    h.FinishEvent();
                }
            }
            finally
            {
                // Remove all duplicates from newsletter queue (duplicate is row with same EmailAddress and EmailNewsletterIssueID as other row).
                ConnectionHelper.ExecuteQuery("newsletter.emails.removeduplicates", null);
            }
        }


        /// <summary>
        /// Generates e-mails of given issue into newsletter queue.
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="subscriber">Subscriber object</param>
        public static void GenerateEmails(IssueInfo issue, SubscriberInfo subscriber)
        {
            if (issue == null || subscriber == null)
            {
                return;
            }

            var isContactSubscriber = subscriber.SubscriberType.Equals(PredefinedObjectType.CONTACT, StringComparison.InvariantCultureIgnoreCase);
            var emailAddressBlocker = isContactSubscriber ? (IEmailAddressBlocker)
                new OnDemandUnsubscribedEmailsAddressBlocker(issue.IssueNewsletterID) :
                new PreloadedUnsubscribedEmailsAddressBlocker(issue.IssueNewsletterID);

            using (var h = NewsletterEvents.GenerateQueueItems.Start(issue, subscriber, emailAddressBlocker))
            {
                if (h.CanContinue())
                {
                    if (isContactSubscriber)
                    {
                        // Prepare newsletter queue record
                        var email = new EmailQueueItemInfo
                        {
                            EmailSubscriberID = subscriber.SubscriberID,
                            EmailSiteID = subscriber.SubscriberSiteID,
                            EmailNewsletterIssueID = issue.IssueID,
                            EmailContactID = subscriber.SubscriberRelatedID
                        };

                        // Get contact subscriber with email address
                        var subscriberMember = SubscriberInfoProvider.CreateSubscriberFromContact(subscriber.SubscriberRelatedID, subscriber);
                        if (subscriberMember != null)
                        {
                            email.EmailAddress = subscriberMember.SubscriberEmail;
                        }

                        if (emailAddressBlocker.IsBlocked(email.EmailAddress))
                        {
                            return;
                        }

                        // Save newsletter queue record
                        EmailQueueItemInfoProvider.SetEmailQueueItem(email);
                    }
                    else if (subscriber.SubscriberType.Equals(PredefinedObjectType.CONTACTGROUP, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Prepare data table that will be inserted into newsletter queue
                        var data = GetEmailQueueDataTable();

                        string siteName = SiteInfoProvider.GetSiteName(issue.IssueSiteID);
                        // Get info if bounced email monitoring is enabled and what the bounce limit is
                        bool monitorBounces = NewsletterHelper.MonitorBouncedEmails(siteName);
                        int bounceLimit = NewsletterHelper.BouncedEmailsLimit(siteName);

                        new EmailQueueRecipientCandidatesRetriever(emailAddressBlocker, issue, monitorBounces, bounceLimit).FillContactGroupTable(data, subscriber.SubscriberRelatedID, subscriber.SubscriberID, null, false);

                        if (DataHelper.DataSourceIsEmpty(data))
                        {
                            return;
                        }

                        BulkInsertDataToEmailQueue(data);
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Generates e-mails from subscribed contact groups and contacts of given issue into e-mail queue.
        /// </summary>
        /// <param name="issue">Issue object</param>
        /// <param name="site">Site the emails are generated for</param>
        /// <param name="monitorBounces">Indicates if bounced e-mails should be monitored</param>
        /// <param name="bounceLimit">Limit for bounced e-mails</param>
        /// <param name="generatedEmails">E-mails addresses that are to be ignored (that have already been generated)</param>
        /// <param name="emailAddressBlocker">Every email will be checked using this email address blocker before it is added to the email queue</param>
        private static void GenerateContactEmails(IssueInfo issue, SiteInfo site, bool monitorBounces, int bounceLimit, HashSet<string> generatedEmails, IEmailAddressBlocker emailAddressBlocker)
        {
            if (issue == null || (site.SiteID <= 0))
            {
                return;
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);

            // Prepare data table that will be inserted into newsletter queue
            var data = GetEmailQueueDataTable();


            // Fill data table with contacts' data
            if (newsletter.NewsletterType == EmailCommunicationTypeEnum.Newsletter)
            {
                new EmailQueueRecipientCandidatesRetriever(emailAddressBlocker, issue, monitorBounces, bounceLimit).FillContactTable(data, site.SiteID, generatedEmails);
            }
            else
            {
                Service.Resolve<IEmailCampaignEmailGenerator>().GenerateEmailsForIssue(data, issue);
            }

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            int maxNumberOfEmails = LicenseKeyInfoProvider.VersionLimitations(site.DomainName, FeatureEnum.SimpleContactManagement, false);
            if (maxNumberOfEmails > 0 && data.Rows.Count > maxNumberOfEmails)
            {
                throw new EmailQueueLimitException(maxNumberOfEmails, string.Format("The current license edition allows you to have only {0} unique recipients for a single email.", maxNumberOfEmails));
            }

            BulkInsertDataToEmailQueue(data);
        }


        private static void BulkInsertDataToEmailQueue(DataTable data)
        {
            var settings = new BulkInsertSettings
            {
                Options = SqlBulkCopyOptions.CheckConstraints,
                Mappings = new Dictionary<string, string>
                {
                    { "IssueID", "EmailNewsletterIssueID" },
                    { "SubscriberID", "EmailSubscriberID" },
                    { "SiteID", "EmailSiteID" },
                    { "Sending", "EmailSending" },
                    { "GUID", "EmailGUID" },
                    { "ContactID", "EmailContactID" },
                    { "Email", "EmailAddress" }
                }
            };

            ConnectionHelper.BulkInsert(data, "Newsletter_Emails", settings);
        }


        internal static DataTable GetEmailQueueDataTable()
        {
            var data = new DataTable();

            data.Columns.Add(new DataColumn("IssueID", typeof(int)));
            data.Columns.Add(new DataColumn("SubscriberID", typeof(int)));
            data.Columns.Add(new DataColumn("SiteID", typeof(int)));
            data.Columns.Add(new DataColumn("Sending", typeof(bool)));
            data.Columns.Add(new DataColumn("GUID", typeof(Guid)));
            data.Columns.Add(new DataColumn("ContactID", typeof(int)));
            data.Columns.Add(new DataColumn("Email", typeof(string)));

            return data;
        }

        #endregion //"Generating methods"


        #region "Dynamic newsletter methods"

        /// <summary>
        /// Generates issue of given dynamic newsletter based on the page defined by NewsletterDynamicURL.
        /// </summary>
        /// <param name="newsletterId">Newsletter ID</param>
        public static int GenerateDynamicIssue(int newsletterId)
        {
            // Get newsletter info object
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);
            if (newsletter == null)
            {
                return 0;
            }

            // Create new newsletter issue
            var newIssue = new IssueInfo
            {
                IssueNewsletterID = newsletter.NewsletterID,
                IssueSiteID = newsletter.NewsletterSiteID,
                IssueTemplateID = 0,
                IssueSentEmails = 0,
                IssueUnsubscribed = 0,
                IssueUseUTM = false
            };

            var resolvedUrl = Service.Resolve<IIssueUrlService>().GetDynamicNewsletterUrl(newsletter);

            try
            {
                newIssue.IssueText = GetIssueTextFromURL(resolvedUrl);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}(URL: {newsletter.NewsletterDynamicURL}, RESOLVED URL: {resolvedUrl})");
            }

            // Get subject from page title
            string pageTitle = GetTitleFromHTML(newIssue.IssueText).Trim();

            // Stop generating when page title or dynamic issue subject contains ##DONOTSEND## constant
            Func<string, bool> containsDoNotSend = s => (s != null) && s.Contains(DO_NOT_SEND);
            if (containsDoNotSend(pageTitle) || containsDoNotSend(newsletter.NewsletterDynamicSubject))
            {
                EventLogProvider.LogInformation("Dynamic newsletter issue", "Insert canceled", "Dynamic issue for newsletter with code name " + newsletter.NewsletterName + " wasn't created because page title or subject contains " + DO_NOT_SEND + ".");
                return 0;
            }

            // Get subject of the issue
            if (string.IsNullOrEmpty(newsletter.NewsletterDynamicSubject))
            {
                // Limit subject of issue to first 500 characters
                newIssue.IssueSubject = HTMLHelper.HTMLDecode(TextHelper.LimitLength(pageTitle, 500, wholeWords: true));

                // Limit issue name to first 200 characters
                newIssue.IssueDisplayName = HTMLHelper.HTMLDecode(TextHelper.LimitLength(pageTitle, 200, wholeWords: true));

                if (string.IsNullOrEmpty(newIssue.IssueSubject))
                {
                    newIssue.IssueSubject = newsletter.NewsletterDisplayName;
                    newIssue.IssueDisplayName = TextHelper.LimitLength(newsletter.NewsletterDisplayName, 200, wholeWords: true);
                }
            }
            else
            {
                newIssue.IssueSubject = newsletter.NewsletterDynamicSubject;
                newIssue.IssueDisplayName = newsletter.NewsletterDynamicSubject;
            }

            IssueInfoProvider.SetIssueInfo(newIssue);
            return newIssue.IssueID;
        }



        /// <summary>
        /// Returns title (text between 'title' tags) of given html page.
        /// </summary>
        /// <param name="htmlPage">Source code of html page</param>
        private static string GetTitleFromHTML(string htmlPage)
        {
            int start = htmlPage.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase);
            if (start > -1)
            {
                int end = htmlPage.IndexOf("</title>", StringComparison.InvariantCultureIgnoreCase);
                if (end > -1)
                {
                    return htmlPage.Substring(start + 7, end - start - 7);
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns source code of html page specified by newsletterDynamicURL.
        /// </summary>
        /// <param name="dynamicNewsletterUrl">Url of page to process</param>
        private static string GetIssueTextFromURL(string dynamicNewsletterUrl)
        {
            using (WebClient client = new WebClient())
            {
                byte[] pageContent = client.DownloadData(URLHelper.AddParameterToUrl(dynamicNewsletterUrl, URLHelper.SYSTEM_QUERY_PARAMETER, "1"));

                // Try to retrieve page encoding from the HTML code
                string pageEncodingString = null;
                Match match = EncodingRegExp.Match(new ASCIIEncoding().GetString(pageContent));
                if (match.Groups.Count > 1)
                {
                    pageEncodingString = match.Groups[1].Value;
                }

                Encoding pageEncoding;

                try
                {
                    pageEncoding = Encoding.GetEncoding(pageEncodingString);
                }
                catch
                {
                    try
                    {
                        // Retrieve the page encoding from the response header
                        ContentType contentType = new ContentType(client.ResponseHeaders[HttpResponseHeader.ContentType]);
                        pageEncoding = Encoding.GetEncoding(contentType.CharSet);
                    }
                    catch (Exception)
                    {
                        pageEncoding = Encoding.GetEncoding("utf-8");
                    }
                }

                return pageEncoding.GetString(pageContent);
            }
        }

        #endregion //"Dynamic newsletter methods"
    }
}
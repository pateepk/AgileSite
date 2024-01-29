using System;
using System.Data;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Default implementation of counters
    /// </summary>
    internal class DefaultCounters
    {
        /// <summary>
        /// Registers the performance counters
        /// </summary>
        public static void RegisterPerformanceCounters()
        {
            // Requests
            HealthMonitoringLogHelper.RegisterCounter(CounterName.VIEW_OF_CONTENT_PAGES_PER_SECOND, RequestHelper.TotalPageRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.PENDING_REQUESTS_PER_SECOND, RequestHelper.PendingRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.FILE_DOWNLOADS_AND_VIEWS_PER_SECOND, RequestHelper.TotalGetFileRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.NOT_FOUND_PAGES_PER_SECOND, RequestHelper.TotalPageNotFoundRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ROBOT_TXT_PER_SECOND, RequestHelper.TotalPageRobotsTxtRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.VIEW_OF_SYSTEM_PAGES_PER_SECOND, RequestHelper.TotalSystemPageRequests);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.NON_PAGES_REQUESTS_PER_SECOND, RequestHelper.TotalNonPageRequests);

            // Cache
            HealthMonitoringLogHelper.RegisterCounter(CounterName.CACHE_REMOVED_ITEMS_PER_SECOND, CacheHelper.Removed);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.CACHE_UNDERUSED_ITEMS_PER_SECOND, CacheHelper.Underused);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.CACHE_EXPIRED_ITEMS_PER_SECOND, CacheHelper.Expired);

            HealthMonitoringLogHelper.RegisterCounter(CounterName.RUNNING_THREADS, CMSThread.RunningThreads);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.RUNNING_SQL_QUERIES, SqlHelper.RunningQueries);

            HealthMonitoringLogHelper.RegisterCounter(CounterName.EVENTLOG_WARNINGS, EventLogProvider.Warnings);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.EVENTLOG_ERRORS, EventLogProvider.Errors);

            // E-mails
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ALL_EMAILS_IN_QUEUE, UpdateEmails);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ERROR_EMAILS_IN_QUEUE, UpdateEmails);

            // Memory
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ALLOCATED_MEMORY, UpdateAllocatedMemory);
        }


        /// <summary>
        /// Updates the allocated memory counter
        /// </summary>
        private static void UpdateAllocatedMemory(Counter counter)
        {
            var memory = GC.GetTotalMemory(false) / 1048576;
            counter.PerformanceCounter.SetValue(memory, null);
        }


        /// <summary>
        /// Updates the e-mail performance counter
        /// </summary>
        private static void UpdateEmails(Counter counter)
        {
            string where = null;

            string counterKey = counter.Key;
            if (counterKey == CounterName.ERROR_EMAILS_IN_QUEUE)
            {
                where = "EmailStatus = " + (int)(EmailStatusEnum.Waiting) + " AND (EmailLastSendResult <> '' OR EmailLastSendResult IS NOT NULL)";
            }
            else if (counterKey == CounterName.ALL_EMAILS_IN_QUEUE)
            {
                where = "EmailStatus = " + (int)(EmailStatusEnum.Waiting) + " AND (EmailLastSendResult = '' OR EmailLastSendResult IS NULL)";
            }

            var performanceCounter = counter.PerformanceCounter;
            
            // Get count of emails for sites.
            DataSet ds = EmailInfoProvider.GetEmailCountForSites(where);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                bool siteCountersEnabled = HealthMonitoringHelper.SiteCountersEnabled;

                // Reset counter values
                performanceCounter.Reset(siteCountersEnabled);

                // Set global value
                performanceCounter.SetValue(ValidationHelper.GetLong(ds.Tables[0].Compute("SUM(EmailCount)", null), 0), null);

                // Site counters
                if (siteCountersEnabled)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow row = ds.Tables[0].Rows[i];
                        string siteName = SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(row["EmailSiteID"], 0));

                        if (!string.IsNullOrEmpty(siteName))
                        {
                            long countEmails = ValidationHelper.GetLong(row["EmailCount"], 0);
                            // Set site value
                            performanceCounter.SetValue(countEmails, siteName);
                        }
                    }
                }
            }
            else
            {
                // Reset counter values
                performanceCounter.Reset(true);
            }
        }
    }
}

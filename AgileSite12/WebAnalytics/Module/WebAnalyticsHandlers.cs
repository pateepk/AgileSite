using System;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Web analytics events handlers
    /// </summary>
    public class WebAnalyticsHandlers
    {
        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                WebAnalyticsEvents.ProcessAnalyticsService.After += LogPageVisitActivities;

                // Logs web analytics and activities on every page request if JavaScript logging is disabled
                RequestEvents.PostAcquireRequestState.Execute += (sender, e) => LogBeginAnalytics();

                RequestEvents.RunEndRequestTasks.Execute += HandleEndAnalytics;

                AttachmentHandlerEvents.SendFile.Execute += LogAttachmentDownload;
            }

            SettingsKeyInfo.TYPEINFO.Events.Update.After += InvalidateExcludedIPs;
            SettingsKeyInfo.TYPEINFO.Events.Insert.After += InvalidateExcludedIPs;
            SettingsKeyInfo.TYPEINFO.Events.Delete.After += InvalidateExcludedIPs;
        }


        private static void LogAttachmentDownload(object sender, CMSEventArgs<AttachmentSendFileEventArgs> e)
        {
            var attachment = e.Parameter.Attachment;
            var siteName = e.Parameter.SiteName;
            var isMultipart = e.Parameter.IsMultipart;
            var isRangeRequest = e.Parameter.IsRangeRequest;
            var isLiveSite = e.Parameter.IsLiveSite;

            if (!isLiveSite || attachment?.FileNode == null || (attachment.Attachment == null) || !attachment.FileNode.IsFile())
            {
                return;
            }

            if (isMultipart || isRangeRequest)
            {
                return;
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            // Log analytics
            // Note that file extension is skipped because the method checks extension against NodeAliasPath. But attachment could have its own extension.
            if (AnalyticsHelper.IsLoggingEnabled(siteName, attachment.FileNode.NodeAliasPath, LogExcludingFlags.SkipFileExtensionCheck) &&
                !AnalyticsHelper.IsFileExtensionExcluded(siteName, attachment.Attachment.AttachmentExtension))
            {
                // Log analytics hit
                if (AnalyticsHelper.TrackFileDownloadsEnabled(siteName))
                {
                    HitLogProvider.LogHit(
                        HitLogProvider.FILE_DOWNLOADS,
                        siteName,
                        attachment.FileNode.DocumentCulture,
                        attachment.FileNode.NodeAliasPath,
                        attachment.FileNode.NodeID);
                }

                // Log analytics conversion
                if (!string.IsNullOrEmpty(attachment.FileNode.DocumentTrackConversionName))
                {
                    HitLogProvider.LogConversions(
                        SiteContext.CurrentSiteName,
                        attachment.FileNode.DocumentCulture,
                        attachment.FileNode.DocumentTrackConversionName,
                        attachment.FileNode.DocumentID,
                        ValidationHelper.GetDoubleSystem(attachment.FileNode.DocumentConversionValue, 0));
                }
            }

            // Log download activity
            if (LoggingActivityEnabled(attachment.Attachment) && LogFileDownload(attachment.FileNode))
            {
                var file = attachment.FileNode;
                var pagesActivityLogger = Service.Resolve<IPagesActivityLogger>();
                pagesActivityLogger.LogPageVisit(file.GetDocumentName(), file, attachment.Attachment.AttachmentName);
            }
        }


        /// <summary>
        /// Logs landing page and page visit activities.
        /// </summary>
        private static void LogPageVisitActivities(object sender, AnalyticsJSEventArgs e)
        {
            var referrer = e.QueryParameters["referrer"];
            var currentPage = DocumentContext.CurrentPageInfo;
            var currentUrl = CMSHttpContext.Current.Request.UrlReferrer != null ? CMSHttpContext.Current.Request.UrlReferrer.AbsoluteUri : null;
            var pagesActivityLogger = Service.Resolve<IPagesActivityLogger>();

            pagesActivityLogger.LogLandingPage(currentPage.GetDocumentName(), currentPage, currentUrl, referrer);
            pagesActivityLogger.LogPageVisit(currentPage.GetDocumentName(), currentPage, null, currentUrl, referrer);
            pagesActivityLogger.LogExternalSearch(AnalyticsHelper.Referrer, currentPage, currentUrl, referrer);
        }


        /// <summary>
        /// Logs the necessary analytics at the end of the current request
        /// </summary>
        private static void HandleEndAnalytics(object sender, EventArgs e)
        {
            LogEndAnalytics();
        }


        /// <summary>
        /// Logs analytics at the end of the request
        /// </summary>
        private static void LogEndAnalytics()
        {
            // Log page view
            if (RequestContext.LogPageHit && (!QueryHelper.GetBoolean(URLHelper.SYSTEM_QUERY_PARAMETER, false)) && (RequestContext.CurrentStatus != RequestStatusEnum.RESTService))
            {
                string siteName = SiteContext.CurrentSiteName;
                PageInfo currentPage = DocumentContext.CurrentPageInfo;

                if (currentPage != null)
                {
                    if (!RequestHelper.IsPostBack() && AnalyticsHelper.IsLoggingEnabled(siteName, currentPage.NodeAliasPath) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
                    {
                        if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
                        {
                            // Log page view
                            HitLogProvider.LogPageView(siteName, currentPage.DocumentCulture, currentPage.NodeAliasPath, currentPage.NodeID);
                        }

                        // Log aggregated view
                        if (QueryHelper.Contains("feed") && AnalyticsHelper.TrackAggregatedViewsEnabled(siteName))
                        {
                            HitLogProvider.LogHit(HitLogProvider.AGGREGATED_VIEWS, siteName, currentPage.DocumentCulture, currentPage.NodeAliasPath, currentPage.NodeID);
                        }
                    }

                    if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
                    {
                        var pagesActivityLogger = Service.Resolve<IPagesActivityLogger>();
                        pagesActivityLogger.LogPageVisit(currentPage.GetDocumentName(), currentPage);
                    }
                }
            }
        }


        /// <summary>
        /// Logs web analytics within the request
        /// </summary>
        private static void LogBeginAnalytics()
        {
            if (!RequestContext.IsContentPage)
            {
                return;
            }

            if (!Service.Resolve<ISiteService>().IsLiveSite)
            {
                return;
            }

            if (ConnectionHelper.ConnectionAvailable && !RequestHelper.IsPostBack() && !QueryHelper.GetBoolean(URLHelper.SYSTEM_QUERY_PARAMETER, false))
            {
                string siteName = SiteContext.CurrentSiteName;
                PageInfo currentPage = DocumentContext.CurrentPageInfo;

                if (currentPage != null)
                {
                    if (AnalyticsHelper.IsLoggingEnabled(siteName, currentPage.NodeAliasPath))
                    {
                        if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
                        {
                            // When JS logging is disabled visitor status is set to context in LogVisitor() method
                            AnalyticsMethods.LogVisitor(siteName);
                            // Log referring, search, landing and exit pages
                            AnalyticsMethods.LogAnalytics(currentPage, siteName);
                        }
                        else
                        {
                            // Visitor status must be set to context here because LogVisitor() method is called later in different request when using JS logging
                            bool idleExpired = false;
                            VisitorStatusEnum visitorStatus = AnalyticsHelper.GetContextStatus(SiteContext.CurrentSiteName, ref idleExpired);

                            if (visitorStatus == VisitorStatusEnum.Unknown)
                            {
                                visitorStatus = VisitorStatusEnum.FirstVisit;
                            }
                            else if (idleExpired)
                            {
                                visitorStatus = VisitorStatusEnum.MoreVisits;
                            }

                            AnalyticsContext.CurrentVisitStatus = visitorStatus;
                        }
                    }

                    // Log search crawler visit, check whether logging is enabled, do not check the excluded crawlers
                    if (AnalyticsHelper.IsLoggingEnabled(siteName, currentPage.NodeAliasPath, LogExcludingFlags.SkipCrawlerCheck))
                    {
                        AnalyticsMethods.LogSearchCrawler(siteName, currentPage);
                    }

                    if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
                    {
                        LogBeginActivities(siteName);
                    }
                }
            }
        }


        /// <summary>
        /// Logs activities for current request
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void LogBeginActivities(string siteName)
        {
            // Log activities
            if (ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(siteName) && ActivitySettingsHelper.ActivitiesEnabledForThisUser(MembershipContext.AuthenticatedUser))
            {
                int contactId = ModuleCommands.OnlineMarketingGetCurrentContactID();
                AnalyticsContext.RequestContactID = contactId;
                if (contactId > 0)
                {
                    var currentPage = DocumentContext.CurrentPageInfo;
                    var pagesActivityLogger = Service.Resolve<IPagesActivityLogger>();
                    pagesActivityLogger.LogLandingPage(currentPage.GetDocumentName(), currentPage);
                    pagesActivityLogger.LogExternalSearch(AnalyticsHelper.Referrer, currentPage);
                }
            }
        }


        /// <summary>
        /// Returns true if logging activities is enabled.
        /// </summary>
        private static bool LoggingActivityEnabled(IAttachment attachment)
        {
            if (attachment == null)
            {
                return false;
            }

            // Check if logging is enabled
            if (!ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(SiteContext.CurrentSiteName) ||
                !ActivitySettingsHelper.ActivitiesEnabledForThisUser(MembershipContext.AuthenticatedUser))
            {
                return false;
            }

            if (attachment.AttachmentExtension == null)
            {
                return true;
            }

            // Get allowed extensions (if not specified log everything)
            var trackedExtensions = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSActivityTrackedExtensions")
                                                       .ToLowerInvariant()
                                                       .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            if (!trackedExtensions.Any())
            {
                return true;
            }

            string attachmentExtension = attachment.AttachmentExtension.TrimStart('.').ToLowerInvariant();

            return trackedExtensions.Contains(attachmentExtension);
        }


        /// <summary>
        /// Checks if logging is enabled for current document.
        /// </summary>
        private static bool LogFileDownload(TreeNode fileNode)
        {
            if (fileNode == null)
            {
                return false;
            }

            return (fileNode.DocumentLogVisitActivity == true) ||
                   ((fileNode.DocumentLogVisitActivity == null) &&
                   ValidationHelper.GetBoolean(fileNode.GetInheritedValue("DocumentLogVisitActivity", false), false));
        }


        private static void InvalidateExcludedIPs(object sender, ObjectEventArgs args)
        {
            var setting = args.Object as SettingsKeyInfo;
            if (setting != null && setting.KeyName.Equals("CMSAnalyticsExcludedIPs", StringComparison.OrdinalIgnoreCase))
            {
                AnalyticsHelper.IPsRegExpTable.Clear();
            }
        }

        #endregion
    }
}
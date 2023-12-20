using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SocialMarketing.LinkedInInternal;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Contains methods for querying LinkedIn API to retrieve account level or post level insights and for collecting them for given application automatically.
    /// </summary>
    internal class LinkedInInsightsHelper
    {
        #region "Public constants"

        /// <summary>
        /// Prefix used for LinkedIn Insights code names.
        /// </summary>
        public const string INSIGHT_CODENAME_PREFIX = "LinkedIn.";

        #endregion


        #region "Protected constants"

        /// <summary>
        /// Array containing pairs (name, period) of scalar Insights for LinkedIn account updates (company profile).
        /// </summary>
        protected static readonly string[][] accountHistoricalUpdateInsights =
        { 
            new[] { "click-count", "day" },
            new[] { "comment-count", "day"},
            new[] { "impression-count", "day"},
            new[] { "like-count", "day"},
            new[] { "share-count", "day"},
            new[] { "unique-count", "day"}
        };


        /// <summary>
        /// Array containing pairs (name, period) of scalar Insights for LinkedIn account followers (company profile).
        /// </summary>
        protected static readonly string[][] accountHistoricalFollowerInsights =
        { 
            new[] { "total-follower-count", "day" },
            new[] { "organic-follower-count", "day"},
            new[] { "paid-follower-count", "day"}
        };


        /// <summary>
        /// Base URI part for LinkedIn stats collection.
        /// </summary>
        protected const string API_URI = "https://api.linkedin.com/v1";


        /// <summary>
        /// URI for account stats collection (followers).
        /// </summary>
        protected const string ACCOUNT_FOLLOWER_STATISTICS_URI_FORMAT = API_URI + "/companies/{0}/historical-follow-statistics?start-timestamp={1}&end-timestamp={2}&time-granularity=day";


        /// <summary>
        /// URI for account stats collection (status updates).
        /// </summary>
        protected const string ACCOUNT_STATUS_STATISTICS_URI_FORMAT = API_URI + "/companies/{0}/historical-status-update-statistics:(time,click-count,comment-count,engagement,impression-count,like-count,share-count,unique-count)?start-timestamp={1}&end-timestamp={2}&time-granularity=day";


        /// <summary>
        /// URI for post stats collection.
        /// </summary>
        private const string POST_STATISTICS_URI_FORMAT = API_URI + "/companies/{0}/historical-status-update-statistics:(time,click-count,comment-count,engagement,impression-count,like-count,share-count)?start-timestamp={1}&time-granularity=month&update-key={2}";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Processes LinkedIn stats for all company profiles within given application.
        /// Writes to log if failure during stats collection occurs.
        /// </summary>
        /// <param name="applicationId">ID of LinkedIn application of which the company profiles (accounts) will be processed</param>
        /// <param name="accountInsightsStates">Insights collection states for given application.</param>
        /// <returns>True on successful collection, false otherwise. Any errors are logged into event log.</returns>
        public static bool ProcessLinkedInAccountInsightsByApplication(int applicationId, List<LinkedInAccountInsightsState> accountInsightsStates)
        {
            // Get accounts within application
            ObjectQuery<LinkedInAccountInfo> accounts = LinkedInAccountInfoProvider.GetLinkedInAccounts().WhereEquals("LinkedInAccountId", applicationId);
            bool result = true;

            foreach (LinkedInAccountInfo account in accounts)
            {
                // Get state from the time the stats were last collected
                LinkedInAccountInsightsState accountInsightsState = accountInsightsStates.FirstOrDefault(it => account.LinkedInAccountID == it.AccountId);
                if (accountInsightsState == null)
                {
                    accountInsightsState = new LinkedInAccountInsightsState{AccountId = account.LinkedInAccountID};
                    accountInsightsStates.Add(accountInsightsState);
                }

                try
                {
                    // Prevent update of last modification date and time
                    using (CMSActionContext actionContext = new CMSActionContext())
                    {
                        // Collect stats for the account and then for all posts within the account
                        actionContext.UpdateTimeStamp = false;
                        result &= ProcessLinkedInAccountInsights(account, accountInsightsState);
                        result &= ProcessLinkedInPostsInsights(account);
                    }
                }
                catch (Exception ex)
                {
                    // Most likely some unexpected exception
                    EventLogProvider.LogWarning("Social marketing - LinkedIn Insights", "ACCOUNTINSIGHTS", ex, account.LinkedInAccountSiteID,
                            String.Format("Could not collect LinkedIn Insights data for account (company profile) with ID {0} ({1}), there was an unexpected error.", account.LinkedInAccountID, account.LinkedInAccountDisplayName));
                    result = false;
                }
            }

            return result;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Processes insights for given LinkedIn account (company profile).
        /// </summary>
        /// <param name="account">Account for which to process insights.</param>
        /// <param name="accountInsightsState">State of insights collection from the last time, gets updated after processing.</param>
        /// <returns>True on successful processing, false otherwise. Any errors are logged into event log.</returns>
        private static bool ProcessLinkedInAccountInsights(LinkedInAccountInfo account, LinkedInAccountInsightsState accountInsightsState)
        {
            if (account == null)
            {
                return true;
            }

            if (String.IsNullOrEmpty(account.LinkedInAccountAccessToken))
            {
                throw new ArgumentException($"LinkedInAccountLinkedInApplicationID of LinkedIn account with ID {account.LinkedInAccountID} does not seem to be valid.");
            }

            bool result = true;

            try
            {
                // Collect insights and update date time of last collection, if collection was successful
                DateTime? recentlyCollected = CollectLinkedInAccountInsights(account.LinkedInAccountAccessToken, account.LinkedInAccountProfileID, accountInsightsState.LastCollectedDateTime);
                if (recentlyCollected != null)
                {
                    accountInsightsState.LastCollectedDateTime = recentlyCollected;
                }
            }
            catch (LinkedInApiUnauthorizedException ex)
            {
                // Terminate collection of stats for given authorization tuple (company profile)
                EventLogProvider.LogWarning("Social marketing - LinkedIn account Insights", "ACCOUNTINSIGHTS", ex, account.LinkedInAccountSiteID,
                    $"LinkedIn API authorization (OAuth keys) invalid for account with ID {account.LinkedInAccountID} ({account.LinkedInAccountDisplayName}). Occurred while processing account insights.");

                return false;
            }
            catch (LinkedInApiThrottleLimitException ex)
            {
                // Terminate collection of stats for given authorization tuple (company profile)
                EventLogProvider.LogWarning("Social marketing - LinkedIn account Insights", "ACCOUNTINSIGHTS", ex, account.LinkedInAccountSiteID,
                    $"LinkedIn API throttle limit overdrown for account with ID {account.LinkedInAccountID} ({account.LinkedInAccountDisplayName}). Occurred while processing account insights.");

                return false;
            }

            catch (Exception ex)
            {
                EventLogProvider.LogWarning("Social marketing - LinkedIn account Insights", "ACCOUNTINSIGHTS", ex, account.LinkedInAccountSiteID,
                    $"LinkedIn account Insights data for account with ID {account.LinkedInAccountID} ({account.LinkedInAccountDisplayName}) couldn't be retrieved from LinkedIn API.");

                result = false;
            }

            return result;
        }


        /// <summary>
        /// Retrieves LinkedIn stats for all posts of given account (company profile).
        /// Stats are retrieved once a day.
        /// </summary>
        /// <param name="account">LinkedIn account (company profile).</param>
        /// <returns>True on successful processing, false otherwise. Any internal errors are logged into event log.</returns>
        private static bool ProcessLinkedInPostsInsights(LinkedInAccountInfo account)
        {
            if (account == null)
            {
                return true;
            }

            if (String.IsNullOrEmpty(account.LinkedInAccountAccessToken))
            {
                throw new ArgumentException($"LinkedInAccountLinkedInApplicationID of LinkedIn account with ID {account.LinkedInAccountID} does not seem to be valid.");
            }

            // Get posts with outdated Insights. Order them descending. When the throttle limit is exhausted, the most recent posts will always be collected.
            List<LinkedInPostInfo> postsToUpdate = LinkedInPostInfoProvider.GetLinkedInPostInfosWithOutdatedInsights(account.LinkedInAccountID).OrderByDescending("LinkedInPostPublishedDateTime").ToList();
            bool result = true;
            while (postsToUpdate.Any())
            {
                var postToUpdate = postsToUpdate.First();
                postsToUpdate = postsToUpdate.Skip(1).ToList();

                try
                {
                    UpdateLinkedInPostInsights(account.LinkedInAccountAccessToken, account.LinkedInAccountProfileID, postToUpdate);
                }
                catch (LinkedInApiUnauthorizedException ex)
                {
                    // Terminate collection of stats for given authorization tuple (company profile)
                    EventLogProvider.LogWarning("Social marketing - LinkedIn post Insights", "POSTINSIGHTS", ex, account.LinkedInAccountSiteID,
                        $"LinkedIn API authorization (OAuth keys) invalid for account with ID {account.LinkedInAccountID} ({account.LinkedInAccountDisplayName}). Occurred while processing post with ID {postToUpdate.LinkedInPostID}.");

                    return false;
                }
                catch (LinkedInApiThrottleLimitException ex)
                {
                    // Terminate collection of stats for given authorization tuple (company profile)
                    EventLogProvider.LogWarning("Social marketing - LinkedIn post Insights", "POSTINSIGHTS", ex, account.LinkedInAccountSiteID,
                        $"LinkedIn API throttle limit overdrown for account with ID {account.LinkedInAccountID} ({account.LinkedInAccountDisplayName}) while processing post with ID {postToUpdate.LinkedInPostID}.");

                    return false;
                }

                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - LinkedIn post Insights", "POSTINSIGHTS", ex, account.LinkedInAccountSiteID,
                        $"LinkedIn post Insights data for post with ID {postToUpdate.LinkedInPostID} couldn't be retrieved from LinkedIn API.");

                    result = false;
                }
            }

            return result;
        }


        /// <summary>
        /// Collects LinkedIn statistics for company identified by its LinkedIn identifier.
        /// The stats are collected once a day, by default for yesterday (in UTC).
        /// When lastCollectedDateTime is supplied (UTC date) the stats are collected from the day after lastCollectedDateTime till yesterday.
        /// Collects both status update stats and follower stats.
        /// </summary>
        /// <param name="accessToken">Access token for API requests.</param>
        /// <param name="companyId">ID of the company on LinkedIn</param>
        /// <param name="lastCollectedDateTime">UTC date (time is not needed) of the last collected stats (accordingly to LinkedIn's time stamps)</param>
        /// <returns>UTC date of the recently collected stats (accordinly to LinkedIn's time stamps). Null if nothing collected.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrown.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        private static DateTime? CollectLinkedInAccountInsights(string accessToken, string companyId, DateTime? lastCollectedDateTime)
        {
            DateTime? sinceWhen = null;
            if (lastCollectedDateTime.HasValue)
            {
                sinceWhen = lastCollectedDateTime.Value.AddDays(1);
            }

            DateTime? collectedStatusUpdateDateTime = null;
            DateTime? collectedFollowerDateTime = null;
            HistoricalStatusUpdateStatistics statusUpdateAccountStats = GetLinkedInHistoricalStatusUpdateStatistics(accessToken, companyId, sinceWhen);
            HistoricalFollowStatistics followerAccountStats = GetLinkedInHistoricalFollowerStatistics(accessToken, companyId, sinceWhen);

            if ((statusUpdateAccountStats != null) && (statusUpdateAccountStats.Statistics != null) && statusUpdateAccountStats.Statistics.Any())
            {
                collectedStatusUpdateDateTime = ProcessLinkedInAccountStatusUpdateInsights(companyId, statusUpdateAccountStats);
            }

            if ((followerAccountStats != null) && (followerAccountStats.Statistics != null) && followerAccountStats.Statistics.Any())
            {
                collectedFollowerDateTime = ProcessLinkedInAccountFollowerInsights(companyId, followerAccountStats);
            }

            if ((collectedStatusUpdateDateTime == null) || (collectedFollowerDateTime == null))
            {
                // Return null if any is null
                return null;
            }

            // Return the earlier date time
            return (collectedStatusUpdateDateTime > collectedFollowerDateTime) ? collectedFollowerDateTime : collectedStatusUpdateDateTime;
        }


        /// <summary>
        /// Stores historical status update statistics of given company.
        /// </summary>
        /// <param name="companyId">ID of the company on LinkedIn</param>
        /// <param name="statusUpdateAccountStats">Historical status update statistics to be stored.</param>
        /// <returns>UTC date of the latest stored stat (accordinly to LinkedIn's time stamps). Null if nothing to be stored (statusUpdateAccountStats are empty).</returns>
        private static DateTime? ProcessLinkedInAccountStatusUpdateInsights(string companyId, HistoricalStatusUpdateStatistics statusUpdateAccountStats)
        {
            DateTime? collectedDateTime = null;
            statusUpdateAccountStats.Statistics.OrderBy(it => it.Time).ToList().ForEach(it =>
            {
                // UTC date the stats relate to (time is set to midnight by LinkedIn)
                DateTime utcDate = UnixTimeStampToDateTime(it.Time / 1000);

                for (int i = 0; i < accountHistoricalUpdateInsights.Length; ++i)
                {
                    InsightInfo processedInsight = GetOrCreateInsight(INSIGHT_CODENAME_PREFIX + accountHistoricalUpdateInsights[i][0], companyId, accountHistoricalUpdateInsights[i][1]);
                    long value = 0;
                    switch (accountHistoricalUpdateInsights[i][0].ToLowerInvariant())
                    {
                        case "click-count":
                            value = it.ClickCount;
                            break;

                        case "comment-count":
                            value = it.CommentCount;
                            break;

                        case "impression-count":
                            value = it.ImpressionCount;
                            break;

                        case "like-count":
                            value = it.LikeCount;
                            break;

                        case "share-count":
                            value = it.ShareCount;
                            break;

                        case "unique-count":
                            value = it.UniqueCount;
                            break;
                    }
                    InsightInfoProvider.UpdateHits(processedInsight, utcDate, value);
                }

                collectedDateTime = utcDate;
            });

            return collectedDateTime;
        }


        /// <summary>
        /// Stores historical follower statistics of given company.
        /// </summary>
        /// <param name="companyId">ID of the company on LinkedIn</param>
        /// <param name="followerAccountStats">Historical follower statistics to be stored.</param>
        /// <returns>UTC date of the latest stored stat (accordinly to LinkedIn's time stamps). Null if nothing to be stored (followerAccountStats are empty).</returns>
        private static DateTime? ProcessLinkedInAccountFollowerInsights(string companyId, HistoricalFollowStatistics followerAccountStats)
        {
            DateTime? collectedDateTime = null;
            followerAccountStats.Statistics.OrderBy(it => it.Time).ToList().ForEach(it =>
            {
                // UTC date the stats relate to (time is set to midnight by LinkedIn)
                DateTime utcDate = UnixTimeStampToDateTime(it.Time / 1000);

                for (int i = 0; i < accountHistoricalFollowerInsights.Length; ++i)
                {
                    InsightInfo processedInsight = GetOrCreateInsight(INSIGHT_CODENAME_PREFIX + accountHistoricalFollowerInsights[i][0], companyId, accountHistoricalFollowerInsights[i][1]);
                    long value = 0;
                    switch (accountHistoricalFollowerInsights[i][0].ToLowerInvariant())
                    {
                        case "total-follower-count":
                            value = it.TotalFollowerCount;
                            break;

                        case "organic-follower-count":
                            value = it.OrganicFollowerCount;
                            break;

                        case "paid-follower-count":
                            value = it.PaidFollowerCount;
                            break;
                    }
                    InsightInfoProvider.UpdateHits(processedInsight, utcDate, value);
                }

                collectedDateTime = utcDate;
            });

            return collectedDateTime;
        }


        /// <summary>
        /// Retrieves LinkedIn Insights for given post. Insights are retrieved by one batch request for each type of insight.
        /// </summary>
        /// <param name="accessToken">Access token for posts.</param>
        /// <param name="companyId">ID of the company on LinkedIn</param>
        /// <param name="post">Post.</param>
        /// <returns>True on successful processing, false otherwise. Any internal errors are logged into event log.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrown.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        private static void UpdateLinkedInPostInsights(string accessToken, string companyId, LinkedInPostInfo post)
        {
            HistoricalStatusUpdateStatistics postStats = GetLinkedInHistoricalStatusUpdateStatistics(accessToken, companyId, post.LinkedInPostUpdateKey, post.LinkedInPostPublishedDateTime.Value);

            if (postStats?.Statistics != null && postStats.Statistics.Any())
            {
                int clickCount = 0;
                int commentCount = 0;
                double engagement = 0.0;
                int impressionCount = 0;
                int likeCount = 0;
                int shareCount = 0;

                postStats.Statistics.ForEach(it =>
                {
                    clickCount += it.ClickCount;
                    commentCount += it.CommentCount;
                    engagement += it.Engagement;
                    impressionCount += it.ImpressionCount;
                    likeCount += it.LikeCount;
                    shareCount += it.ShareCount;
                });

                post.LinkedInPostClickCount = clickCount;
                post.LinkedInPostCommentCount = commentCount;
                post.LinkedInPostEngagement = engagement;
                post.LinkedInPostImpressionCount = impressionCount;
                post.LinkedInPostLikeCount = likeCount;
                post.LinkedInPostShareCount = shareCount;

                post.LinkedInPostInsightsLastUpdated = DateTime.Now;

                LinkedInPostInfoProvider.SetLinkedInPostInfo(post);
            }
        }

        #endregion


        #region "Public methods for API querying"

        /// <summary>
        /// Gets historical follower statistics for given LinkedIn company profile (only stats for finished days can be retrieved).
        /// </summary>
        /// <param name="accessToken">Access token to be used for API querying.</param>
        /// <param name="companyId">Company profile ID on LinkedIn.</param>
        /// <param name="sinceWhen">Since when to get the statistics. Should be a UTC date from the past until yesterday. Today's stats can not be retrieved since they are not complete.</param>
        /// <returns>Historical follower stats for the company profile.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrown.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <remarks>
        /// Can be used to collect statistics for whole days only. This means the latest sinceWhen must represent UTC's yesterday (LinkedIn uses UTC).
        /// </remarks>
        public static HistoricalFollowStatistics GetLinkedInHistoricalFollowerStatistics(string accessToken, string companyId, DateTime? sinceWhen = null)
        {
            // Compute UNIX timestamp for yesterday. The company stats are always collected for whole days. Today's stats must be collected when the day ends.
            DateTime utcYesterday = UtcToday().AddDays(-1);
            DateTime startTime = (sinceWhen == null || sinceWhen > utcYesterday) ? utcYesterday : sinceWhen.Value;
            long startTimeMillis = (long)DateTimeToUnixTimeStamp(startTime) * 1000;
            long endTimeMillis = ((long)DateTimeToUnixTimeStamp(utcYesterday) + 1) * 1000;

            // Retrieve stats for given company profile
            Uri requestUri = new Uri(String.Format(ACCOUNT_FOLLOWER_STATISTICS_URI_FORMAT, companyId, startTimeMillis, endTimeMillis));
            ApiResponse response = LinkedInHelper.ProcessGetRequest(accessToken, requestUri);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                HistoricalFollowStatistics res = LinkedInXmlSerializer.Deserialize<HistoricalFollowStatistics>(response.ResponseBody);

                return res;
            }

            return null;
        }


        /// <summary>
        /// Gets historical status update statistics for given LinkedIn company profile (only stats for finished days can be retrieved).
        /// </summary>
        /// <param name="accessToken">Access token to be used for API querying.</param>
        /// <param name="companyId">Company profile ID on LinkedIn.</param>
        /// <param name="sinceWhen">Since when to get the statistics. Should be a UTC date from the past until yesterday. Today's stats can not be retrieved since they are not complete.</param>
        /// <returns>Historical status update stats for the company profile.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrown.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        /// <remarks>
        /// Can be used to collect statistics for whole days only. This means the latest sinceWhen must represent UTC's yesterday (LinkedIn uses UTC).
        /// </remarks>
        public static HistoricalStatusUpdateStatistics GetLinkedInHistoricalStatusUpdateStatistics(string accessToken, string companyId, DateTime? sinceWhen = null)
        {
            // Compute UNIX timestamp for yesterday. The company stats are always collected for whole days. Today's stats must be collected when the day ends.
            DateTime utcYesterday = UtcToday().AddDays(-1);
            DateTime startTime = (sinceWhen == null || sinceWhen > utcYesterday) ? utcYesterday : sinceWhen.Value;
            long startTimeMillis =  (long) DateTimeToUnixTimeStamp(startTime) * 1000;
            long endTimeMillis = ((long)DateTimeToUnixTimeStamp(utcYesterday) + 1) * 1000;

            // Retrieve stats for given company profile
            Uri requestUri = new Uri(String.Format(ACCOUNT_STATUS_STATISTICS_URI_FORMAT, companyId, startTimeMillis, endTimeMillis));
            ApiResponse response = LinkedInHelper.ProcessGetRequest(accessToken, requestUri);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                HistoricalStatusUpdateStatistics res = LinkedInXmlSerializer.Deserialize<HistoricalStatusUpdateStatistics>(response.ResponseBody);

                return res;
            }

            return null;
        }


        /// <summary>
        /// Gets historical status update statistics for given LinkedIn company update.
        /// The statistics are with month granularity starting at the post published datetime.
        /// </summary>
        /// <param name="accessToken">Access token to be used for API querying.</param>
        /// <param name="companyId">Company profile ID on LinkedIn.</param>
        /// <param name="updateKey">Update key of the company update to query.</param>
        /// <param name="postPublishedDateTime">Date time of post publishing.</param>
        /// <returns>Historical status update stats.</returns>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiThrottleLimitException">Thrown when protocol error occurs because the limit of API requests was overdrown.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        public static HistoricalStatusUpdateStatistics GetLinkedInHistoricalStatusUpdateStatistics(string accessToken, string companyId, string updateKey, DateTime postPublishedDateTime)
        {
            // Compute UNIX timestamp for publishing time of the post. Decrement the published time a bit since the actual posting via API occurs before the time stamp is updated and some midnight posts might not be counted correctly
            double publishedTimeStamp = DateTimeToUnixTimeStamp(postPublishedDateTime.AddMinutes(-1));
            long startTimeMillis = (long)publishedTimeStamp * 1000;

            // Retrieve stats for given update-key
            Uri requestUri = new Uri(String.Format(POST_STATISTICS_URI_FORMAT, companyId, startTimeMillis, updateKey));
            ApiResponse response = LinkedInHelper.ProcessGetRequest(accessToken, requestUri);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                HistoricalStatusUpdateStatistics res = LinkedInXmlSerializer.Deserialize<HistoricalStatusUpdateStatistics>(response.ResponseBody);

                return res;
            }

            return null;
        }

        #endregion


        #region "Utility methods"
        
        /// <summary>
        /// Converts UNIX time stamp to local time.
        /// </summary>
        /// <param name="timeStamp">Seconds since UNIX epoch</param>
        /// <returns>DateTime in local time.</returns>
        protected static DateTime UnixTimeStampToDateTime(double timeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timeStamp);

            return dt.ToLocalTime();
        }


        /// <summary>
        /// Converts any time (local or UTC) to UNIX time stamp.
        /// </summary>
        /// <param name="dt">DateTime</param>
        /// <returns>Seconds since UNIX epoch</returns>
        protected static double DateTimeToUnixTimeStamp(DateTime dt)
        {
            DateTime dtEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = dt.ToUniversalTime().Subtract(dtEpoch);

            return span.TotalSeconds;
        }


        /// <summary>
        /// Gets UTC today (today's date according to UTC, time set to midnight).
        /// </summary>
        /// <returns>UTC today</returns>
        protected static DateTime UtcToday()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcToday = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);

            return utcToday;
        }


        /// <summary>
        /// Gets the insight matching the specified criteria, and returns it. Creates a new one, if none found.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <param name="valueName">>The value name. Pass NULL, if not named.</param>
        /// <returns>The insight matching the specified criteria.</returns>
        protected static InsightInfo GetOrCreateInsight(string codeName, string externalId, string periodType, string valueName = null)
        {
            InsightInfo insight = InsightInfoProvider.GetInsight(codeName, externalId, periodType, valueName);
            if (insight == null)
            {
                insight = new InsightInfo()
                {
                    InsightCodeName = codeName,
                    InsightExternalID = externalId,
                    InsightPeriodType = periodType,
                    InsightValueName = valueName
                };
                InsightInfoProvider.SetInsight(insight);
            }
            return insight;
        }

        #endregion
    }
}

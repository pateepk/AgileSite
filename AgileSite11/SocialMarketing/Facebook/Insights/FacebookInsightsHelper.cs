using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Base;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides methods for processing Facebook Insights.
    /// </summary>
    internal class FacebookInsightsHelper
    {

        #region "Public constants"

        /// <summary>
        /// Prefix used for Facebook Insights code names.
        /// </summary>
        public const string INSIGHT_CODENAME_PREFIX = "Facebook.";

        #endregion


        #region "Protected constants"

        /// <summary>
        /// Array containing pairs (name, period) of scalar Insights for Facebook page.
        /// </summary>
        protected static readonly string[][] scalarPageInsights =
        {
            new[] { "page_stories", "day" },
            new[] { "page_impressions_unique", "day"},
            new[] { "page_impressions_paid_unique", "day"},
            new[] { "page_impressions_organic_unique", "day"},
            new[] { "page_impressions_viral_unique", "day"},
            new[] { "page_engaged_users", "day"},
            new[] { "page_consumptions_unique", "day"},
            new[] { "page_negative_feedback_unique", "day"},
            new[] { "page_fans", "lifetime"},
            new[] { "page_fan_adds_unique", "day"},
            new[] { "page_fan_removes_unique", "day"},
            new[] { "page_views_unique", "day"},
            new[] { "page_posts_impressions_unique", "day"},
            new[] { "page_posts_impressions_paid_unique", "day"},
            new[] { "page_posts_impressions_organic_unique", "day"},
            new[] { "page_posts_impressions_viral_unique", "day"}
        };


        /// <summary>
        /// Array containing pairs (name, period) of structured Insights for Facebook page.
        /// </summary>
        protected static readonly string[][] structuredPageInsights =
        {
            new[] { "page_stories_by_story_type", "day"},
            new[] { "page_storytellers_by_story_type", "day"},
            new[] { "page_storytellers_by_age_gender", "day"},
            new[] { "page_storytellers_by_country", "day"},
            new[] { "page_storytellers_by_locale", "day"},
            new[] { "page_consumptions_by_consumption_type_unique", "day"},
            new[] { "page_fans_locale", "lifetime"},
            new[] { "page_fans_city", "lifetime"},
            new[] { "page_fans_country", "lifetime"},
            new[] { "page_fans_gender_age", "lifetime"},
            new[] { "page_fans_by_like_source_unique", "day"}
        };


        /// <summary>
        /// Collection of metrics to be requested during posts retrieval.
        /// </summary>
        protected static readonly string[] POSTS_INSIGHTS_METRICS = new[] { "post_impressions_unique", "post_stories_by_action_type", "post_negative_feedback_by_type" };


        /// <summary>
        /// How long after successful Facebook Insights collection is not necessary to ask Facebook API for new Insights.
        /// </summary>
        protected static readonly TimeSpan SKIP_COLLECTION_TIME = new TimeSpan(20, 0, 0);


        /// <summary>
        /// Facebook response may contain structured values with zero length key
        /// (occured i.e. in page_fans_locale_city), which breaks the query
        /// that displays the graph (results in empty column name).
        /// </summary>
        protected const string EMPTY_STRUCTURED_NAME = "[unknown]";


        /// <summary>
        /// How many posts are processed by one Facebook Graph API request.
        /// </summary>
        protected const int POST_INSIGHTS_BATCH_SIZE = 30;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Processes Facebook Insights for all pages within given application.
        /// Skips Insights processing, if Facebook API response does not contain values newer than lastCollectedDay.
        /// Writes to log if failure during Insights collection occurs, but continues collecting Insights of other accounts (pages).
        /// </summary>
        /// <param name="applicationId">ID of Facebook application of which the pages will be processed</param>
        /// <param name="insightsState">Contains Insights collection state.</param>
        public static void ProcessFacebookAccountInsightsByApplication(int applicationId, FacebookInsightsState insightsState)
        {
            ObjectQuery<FacebookAccountInfo> accounts = FacebookAccountInfoProvider.GetFacebookAccountsByApplicationId(applicationId);

            foreach (FacebookAccountInfo account in accounts)
            {
                try
                {
                    // Prevent update of last modification date and time
                    using (CMSActionContext actionContext = new CMSActionContext())
                    {
                        actionContext.UpdateTimeStamp = false;
                        ProcessFacebookPostsInsights(account);
                        ProcessFacebookAccountInsights(account, insightsState);
                    }
                }
                catch (Exception ex)
                {
                    // Most likely a FacebookApiException
                    EventLogProvider.LogWarning("Social marketing - Facebook Insights", "ACCOUNTINSIGHTS", ex, account.FacebookAccountSiteID,
                            String.Format("Could not collect Facebook Insights data for account (page) with ID {0} ({1}), there was an unexpected error.", account.FacebookAccountID, account.FacebookAccountDisplayName));
                }
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Retrieves Facebook Insights for given account (page).
        /// After successful collection, the next collection time is not until (Now + SKIP_COLLECTION_TIME).
        /// Collection state is loaded from and stored to insightsState.
        /// </summary>
        /// <param name="account">Facebook account (page).</param>
        /// <param name="insightsState">Contains Insights collection state.</param>
        /// <exception cref="Facebook.FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        protected static void ProcessFacebookAccountInsights(FacebookAccountInfo account, FacebookInsightsState insightsState)
        {
            FacebookInsightsStateItem insightsStateItem = insightsState.GetOrCreateStateItem(account.FacebookAccountPageID);
            if (insightsStateItem.SkipCollectionUntil.HasValue && (insightsStateItem.SkipCollectionUntil > DateTime.Now))
            {
                return;
            }

            var appSecret = FacebookApplicationInfoProvider.GetFacebookApplicationInfo(account.FacebookAccountFacebookApplicationID).FacebookApplicationConsumerSecret;
            var insightMetrics = scalarPageInsights.Concat(structuredPageInsights);
            dynamic insights = FacebookHelper.RetrieveFacebookInsights(account.FacebookAccountPageID, insightMetrics.Select(a => a[0]), account.FacebookPageAccessToken.AccessToken, appSecret);

            FacebookInsightsParser insightsParser = new FacebookInsightsParser(insights);

            SortedSet<DateTime> availableDateTimes = insightsParser.GetDateTimes(insightMetrics.ToArray());

            if (!insightsParser.HasData() || !availableDateTimes.Any())
            {
                EventLogProvider.LogWarning("Social marketing - Facebook Insights", "ACCOUNTINSIGHTS", null, account.FacebookAccountSiteID,
                            String.Format("Could not collect Facebook Insights data for account (page) with ID {0} ({1}), the response does not contain any data.", account.FacebookAccountID, account.FacebookAccountDisplayName));

                return;
            }

            IEnumerable<DateTime> availableDateTimesToBeProcessed;
            if (!insightsStateItem.CollectedEndTime.HasValue)
            {
                availableDateTimesToBeProcessed = availableDateTimes;
            }
            else
            {
                availableDateTimesToBeProcessed = availableDateTimes.Where(it => it > insightsStateItem.CollectedEndTime).ToList();
            }

            if (!availableDateTimesToBeProcessed.Any())
            {
                return;
            }

            foreach (DateTime day in availableDateTimesToBeProcessed)
            {
                ProcessBothFacebookInsights(account, insightsParser, day);
            }

            insightsStateItem.CollectedEndTime = availableDateTimesToBeProcessed.Max();
            insightsStateItem.SkipCollectionUntil = DateTime.Now + SKIP_COLLECTION_TIME;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Processes scalar and structured Facebook page insights.
        /// </summary>
        /// <param name="account">Facebook account (page).</param>
        /// <param name="insightsParser">Insights parser with Facebook API response.</param>
        /// <param name="endTime">End time of values to process.</param>
        private static void ProcessBothFacebookInsights(FacebookAccountInfo account, FacebookInsightsParser insightsParser, DateTime endTime)
        {
            ProcessScalarFacebookInsights(account, insightsParser, endTime);
            ProcessStructuredFacebookInsights(account, insightsParser, endTime);
        }


        /// <summary>
        /// Processes scalar Facebook page insights.
        /// </summary>
        /// <param name="account">Facebook account (page).</param>
        /// <param name="insightsParser">Insights parser with Facebook API response.</param>
        /// <param name="endTime">End time of values to process.</param>
        private static void ProcessScalarFacebookInsights(FacebookAccountInfo account, FacebookInsightsParser insightsParser, DateTime endTime)
        {
            long? scalarResult;
            string missingInsights = "";
            InsightInfo processedInsight;

            // Process scalar Insights
            for (int i = 0; i < scalarPageInsights.Length; ++i)
            {
                scalarResult = insightsParser.GetLongValue(scalarPageInsights[i][0], scalarPageInsights[i][1], endTime);
                if (!scalarResult.HasValue)
                {
                    // Insight might be unexpectedly missing in response - probably change in API or some kind of error
                    missingInsights += scalarPageInsights[i][0] + " (" + scalarPageInsights[i][1] + "); ";
                }
                else
                {
                    // Put Insight in the database
                    processedInsight = GetOrCreateInsight(INSIGHT_CODENAME_PREFIX + scalarPageInsights[i][0], account.FacebookAccountPageID, scalarPageInsights[i][1], null);
                    InsightInfoProvider.UpdateHits(processedInsight, FacebookEndTimeToPeriodDateTime(endTime), scalarResult.Value);
                }
            }

            if (missingInsights.Length > 0)
            {
                EventLogProvider.LogInformation("Social marketing - Facebook Insights", "ACCOUNTINSIGHTS", String.Format("Some scalar Facebook Insights data is missing in Facebook API response for account (page) with ID {0} ({1}). " +
                                  "This means no such data is available at Facebook for this metric yet, but may be available later. Missing Insights ({2}): {3}",
                        account.FacebookAccountID, account.FacebookAccountDisplayName, endTime, missingInsights));
            }
        }


        /// <summary>
        /// Processes structured Facebook page insights.
        /// </summary>
        /// <param name="account">Facebook account (page).</param>
        /// <param name="insightsParser">Insights parser with Facebook API response.</param>
        /// <param name="endTime">End time of values to process.</param>
        private static void ProcessStructuredFacebookInsights(FacebookAccountInfo account, FacebookInsightsParser insightsParser, DateTime endTime)
        {
            string missingInsights = "";
            InsightInfo processedInsight;

            // Process structured Insights
            for (int i = 0; i < structuredPageInsights.Length; ++i)
            {
                IDictionary<string, object> structuredValues = insightsParser.GetStructuredValue(structuredPageInsights[i][0], structuredPageInsights[i][1], endTime);
                if (structuredValues == null)
                {
                    // Insight might be unexpectedly missing in response - probably change in API or some kind of error
                    missingInsights += structuredPageInsights[i][0] + " (" + structuredPageInsights[i][1] + "); ";
                }
                else
                {
                    foreach (KeyValuePair<string, object> pair in structuredValues)
                    {
                        processedInsight = GetOrCreateInsight(INSIGHT_CODENAME_PREFIX + structuredPageInsights[i][0], account.FacebookAccountPageID, structuredPageInsights[i][1], (pair.Key.Length == 0) ? EMPTY_STRUCTURED_NAME : pair.Key);
                        InsightInfoProvider.UpdateHits(processedInsight, FacebookEndTimeToPeriodDateTime(endTime), (long)pair.Value);
                    }
                }
            }

            if (missingInsights.Length > 0)
            {
                EventLogProvider.LogInformation("Social marketing - Facebook Insights", "ACCOUNTINSIGHTS", String.Format("Some structured Facebook Insights data is missing in Facebook API response for account (page) with ID {0} ({1}). " +
                                  "This means no such data is available at Facebook for this metric yet, but may be available later. Missing Insights ({2}): {3}",
                        account.FacebookAccountID, account.FacebookAccountDisplayName, endTime, missingInsights));
            }
        }


        /// <summary>
        /// Retrieves Facebook Insights for all posts of given account (page).
        /// Insights are retrieved once a day.
        /// </summary>
        /// <param name="account">Facebook account (page).</param>
        private static void ProcessFacebookPostsInsights(FacebookAccountInfo account)
        {
            List<FacebookPostInfo> postsToUpdate = FacebookPostInfoProvider.GetFacebookPostInfosWithOutdatedInsights(account.FacebookAccountID).ToList();

            while (postsToUpdate.Any())
            {
                try
                {
                    var secretKey = FacebookApplicationInfoProvider.GetFacebookApplicationInfo(account.FacebookAccountFacebookApplicationID).FacebookApplicationConsumerSecret;
                    UpdateFacebookPostsBatchInsights(postsToUpdate.Take(POST_INSIGHTS_BATCH_SIZE).ToList(), account.FacebookPageAccessToken.AccessToken, secretKey);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - Facebook post Insights", "POSTINSIGHTS", ex, account.FacebookAccountSiteID,
                            "Some Facebook post Insights data for batch of posts couldn't be retrieved from Facebook Graph API.");
                }
                finally
                {
                    // Prevent endless loop if exception is thrown
                    postsToUpdate = postsToUpdate.Skip(POST_INSIGHTS_BATCH_SIZE).ToList();
                }
            }
        }


        /// <summary>
        /// Retrieves Facebook Insights for given posts. Insights are retrieved by one batch request for each type of insight.
        /// </summary>
        /// <param name="postsBatch">Batch of posts.</param>
        /// <param name="accessToken">Access token for posts.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        private static void UpdateFacebookPostsBatchInsights(List<FacebookPostInfo> postsBatch, string accessToken, string appSecret)
        {
            List<string> ids = postsBatch.Select(x => x.FacebookPostExternalID).ToList();
            dynamic insightsData = FacebookHelper.RetrieveFacebookInsights(ids, POSTS_INSIGHTS_METRICS, accessToken, appSecret);
            dynamic likesData = FacebookHelper.RetrieveFacebookLikesSummary(ids, accessToken, appSecret);
            dynamic commentsData = FacebookHelper.RetrieveFacebookCommentsSummary(ids, accessToken, appSecret);
            dynamic postsData = FacebookHelper.RetrieveFacebookObjects(ids, accessToken, appSecret);

            foreach (FacebookPostInfo post in postsBatch)
            {
                try
                {
                    ICollection<dynamic> insights = (insightsData[post.FacebookPostExternalID].data as ICollection<dynamic>) ?? new Collection<dynamic>();
                    foreach (dynamic insight in insights)
                    {
                        if ((insight.name == "post_impressions_unique") && (insight.period == "lifetime"))
                        {
                            post.FacebookPostInsightPeopleReached = GetInsightIntegerValue(insight);
                        }
                        else if ((insight.name == "post_stories_by_action_type") && (insight.period == "lifetime"))
                        {
                            post.FacebookPostInsightLikesTotal = GetInsightIntegerValue(insight, "like");
                            post.FacebookPostInsightCommentsTotal = GetInsightIntegerValue(insight, "comment");
                        }
                        else if ((insight.name == "post_negative_feedback_by_type") && (insight.period == "lifetime"))
                        {
                            post.FacebookPostInsightNegativeHideAllPosts = GetInsightIntegerValue(insight, "hide_all");
                            post.FacebookPostInsightNegativeHidePost = GetInsightIntegerValue(insight, "hide");
                            post.FacebookPostInsightNegativeUnlikePage = GetInsightIntegerValue(insight, "unlike_page");
                            post.FacebookPostInsightNegativeReportSpam = GetInsightIntegerValue(insight, "report_spam");
                        }
                    }

                    post.FacebookPostInsightLikesFromPage = (int)likesData[post.FacebookPostExternalID].summary.total_count;
                    post.FacebookPostInsightCommentsFromPage = (int)commentsData[post.FacebookPostExternalID].summary.total_count;

                    dynamic shares = null;
                    if (postsData is IDictionary<string, object>)
                    {
                        shares = postsData[post.FacebookPostExternalID].shares;
                    }
                    post.FacebookPostInsightSharesFromPage = (int)((shares != null) ? shares.count : 0);

                    post.FacebookPostInsightsLastUpdated = DateTime.Now;
                    FacebookPostInfoProvider.SetFacebookPostInfo(post);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - Facebook post Insights", "POSTINSIGHTS", ex, post.FacebookPostSiteID,
                            String.Format("Some Facebook post Insights data have been missing in Facebook API response for post with ID {0}.", post.FacebookPostID));
                }
            }
        }


        /// <summary>
        /// Gets the insight matching the specified criteria, and returns it. Creates a new one, if none found.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <param name="valueName">>The value name. Pass NULL, if not named.</param>
        /// <returns>The insight matching the specified criteria.</returns>
        private static InsightInfo GetOrCreateInsight(string codeName, string externalId, string periodType, string valueName = null)
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


        /// <summary>
        /// Converts Facebook Insight's end time (midnight after Insight collection) to some time from the day 
        /// the Insight relates to.
        /// </summary>
        /// <param name="facebookEndTime">Facebook Insight's end time.</param>
        /// <returns>Date time from the day the Insight relates to.</returns>
        private static DateTime FacebookEndTimeToPeriodDateTime(DateTime facebookEndTime)
        {
            return facebookEndTime.ToUniversalTime().AddDays(-1);
        }


        /// <summary>
        /// Gets integer value from insight. Use parameter valueName when insight value is structured. Returns 0 if insight doesn't contain requested value.
        /// </summary>
        /// <param name="insight">Insight object.</param>
        /// <param name="valueName">Name of structured value. Value is considered as scalar if not given.</param>
        /// <returns>Returns insight's integer value or 0 if value couldn't be retrieved.</returns>
        private static int GetInsightIntegerValue(dynamic insight, string valueName = null)
        {
            int value = 0;
            if ((insight != null) && (insight.values != null) && (insight.values[0] != null) && (insight.values[0].value != null))
            {
                dynamic insightValue = insight.values[0].value;
                if (String.IsNullOrEmpty(valueName))
                {
                    value = (int)insightValue;
                }
                else if (insightValue is IDictionary<string, dynamic>)
                {
                    IDictionary<string, dynamic> structuredValues = insightValue;
                    if (structuredValues.ContainsKey(valueName))
                    {
                        value = (int)structuredValues[valueName];
                    }
                }
            }

            return value;
        }

        #endregion

    }
}

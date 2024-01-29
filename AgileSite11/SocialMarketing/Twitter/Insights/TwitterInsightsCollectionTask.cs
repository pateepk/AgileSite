using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.SiteProvider;

using LinqToTwitter;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Retrieves insights from Twitter.
    /// </summary>
    public sealed class TwitterInsightsCollectionTask : ITask
    {
        private readonly ITwitterRetryPolicyProvider mRetryPolicyProvider = Service.Resolve<ITwitterRetryPolicyProvider>();


        #region "Public methods"

        /// <summary>
        /// Retrieves insights from Twitter and stores them in the database.
        /// </summary>
        /// <param name="task">The task data.</param>
        /// <returns>An error message if there was an error; otherwise, an empty string.</returns>
        public string Execute(TaskInfo task)
        {
            if (!IsFeatureAvailable(task))
            {
                return "Feature Social Marketing Insights is not available in the Kentico edition you are using.";
            }

            DateTime currentDateTime = DateTime.Now;
            SiteInfoIdentifier siteId = task.TaskSiteID;
            try
            {
                ProcessAccounts(siteId, currentDateTime);
            }
            catch (AggregateException exception)
            {
                foreach (Exception innerException in exception.InnerExceptions)
                {
                    EventLogProvider.LogException("Twitter insights", "Process", innerException, siteId);
                }
                return String.IsNullOrEmpty(exception.Message) ? exception.ToString() : exception.Message;
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("Twitter insights", "Process", exception, siteId);
                return String.IsNullOrEmpty(exception.Message) ? exception.ToString() : exception.Message;
            }

            return String.Empty;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Collects insights for all Twitter accounts from the specified site.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        private void ProcessAccounts(SiteInfoIdentifier siteId, DateTime currentDateTime)
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (TwitterAccountInfo account in TwitterAccountInfoProvider.GetTwitterAccounts(siteId))
            {
                try
                {
                    ProcessAccount(account, currentDateTime);
                }
                catch (Exception exception)
                {
                    if (mRetryPolicyProvider.IsEligible(exception) && mRetryPolicyProvider.ApplyRetryPolicy())
                    {
                        // Give it another try, but do not proceed other accounts, since the connection problem probably persists
                        break;
                    }

                    exceptions.Add(exception);
                    mRetryPolicyProvider.Reset();
                }
            }
            if (exceptions.Count > 0)
            {
                throw new AggregateException("There were errors while processing Twitter insights, please check the event log for more information.", exceptions);
            }
        }


        /// <summary>
        /// Collects insights for the specified Twitter account.
        /// </summary>
        /// <param name="account">The Twitter account.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        private void ProcessAccount(TwitterAccountInfo account, DateTime currentDateTime)
        {
            if (!String.IsNullOrEmpty(account.TwitterAccountUserID))
            {
                TwitterContext context = CreateContext(account);
                Help help = context.Help.Where(x => x.Type == HelpType.RateLimits).Single();
                // Prevent update of last modification date and time
                using (CMSActionContext actionContext = new CMSActionContext())
                {
                    actionContext.UpdateTimeStamp = false;
                    // Insights and insight samples need to be synchronized.
                    using (CMSTransactionScope scope = new CMSTransactionScope())
                    {
                        ProcessFollowers(account, currentDateTime, context, help);
                        ProcessMentions(account, currentDateTime, context, help);
                        TwitterAccountInfoProvider.SetTwitterAccountInfo(account);
                        scope.Commit();
                    }
                    ProcessPosts(account, currentDateTime, context, help);
                }
            }
        }


        /// <summary>
        /// Collects the number of followers for the specified Twitter account, if the Twitter API request rate limits allow it.
        /// </summary>
        /// <param name="account">The Twitter account.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        /// <param name="context">The Twitter context.</param>
        /// <param name="help">The object with Twitter API request rate limits.</param>
        private void ProcessFollowers(TwitterAccountInfo account, DateTime currentDateTime, TwitterContext context, Help help)
        {
            TryPerformRequest("users", "/users/show/:id", help, () => ProcessFollowers(account, currentDateTime, context));
        }


        /// <summary>
        /// Collects the number of followers for the specified Twitter account.
        /// </summary>
        /// <param name="account">The Twitter account.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        /// <param name="context">The Twitter context.</param>
        private void ProcessFollowers(TwitterAccountInfo account, DateTime currentDateTime, TwitterContext context)
        {
            User user = context.User.Where(x => x.Type == UserType.Show && x.UserID == UInt64.Parse(account.TwitterAccountUserID)).Single();
            account.TwitterAccountFollowers = user.FollowersCount;
            InsightInfo insight = GetOrCreateInsight("Twitter.channel_followers", account.TwitterAccountUserID, "lifetime");
            InsightInfoProvider.UpdateHits(insight, currentDateTime.AddDays(-1), user.FollowersCount);
        }


        /// <summary>
        /// Collects the number of mentions for the specified Twitter account, if the Twitter API request rate limits allow it.
        /// </summary>
        /// <param name="account">The Twitter account.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        /// <param name="context">The Twitter context.</param>
        /// <param name="help">The object with Twitter API request rate limits.</param>
        /// <remarks>
        /// This method counts tweets on the mentions timeline.
        /// As the number of API calls is limited it uses a range of tweet identifiers to remember which part of the timeline has been already processed.
        /// </remarks>
        private void ProcessMentions(TwitterAccountInfo account, DateTime currentDateTime, TwitterContext context, Help help)
        {
            int mentionCount = account.TwitterAccountMentions;
            TwitterIdentifierRange? range = account.TwitterAccountMentionsRange;
            Status[] mentions = null;
            if (range == null)
            {
                // Establish the initial range of tweet identifiers on the mention timeline
                mentions = GetMentions(context, help).ToArray();
                if (mentions.Length == 0)
                {
                    return;
                }
                mentionCount = mentions.Length;
                range = new TwitterIdentifierRange(mentions.Last().StatusID, mentions.First().StatusID);
            }
            else
            {
                // Process new mentions
                ulong since = range.Value.To;
                mentions = GetMentions(context, help).Where(x => x.SinceID == since).ToArray();
                if (mentions.Length > 0)
                {
                    ulong max = UInt64.MaxValue;
                    range = new TwitterIdentifierRange(range.Value.From, mentions.First().StatusID);
                    while (mentions.Length > 0)
                    {
                        mentionCount = mentionCount + mentions.Length;
                        max = mentions.Last().StatusID - 1;
                        mentions = GetMentions(context, help).Where(x => x.SinceID == since && x.MaxID == max).ToArray();
                    }
                }
            }

            // Process old mentions
            mentions = GetMentions(context, help).Where(x => x.MaxID == (range.Value.From - 1)).ToArray();
            while (mentions.Length > 0)
            {
                mentionCount = mentionCount + mentions.Length;
                range = new TwitterIdentifierRange(mentions.Last().StatusID, range.Value.To);
                mentions = GetMentions(context, help).Where(x => x.MaxID == (range.Value.From - 1)).ToArray();
            }
            account.TwitterAccountMentions = mentionCount;
            account.TwitterAccountMentionsRange = range.Value;
            InsightInfo insight = GetOrCreateInsight("Twitter.channel_mentions", account.TwitterAccountUserID, "lifetime");
            InsightInfoProvider.UpdateHits(insight, currentDateTime.AddDays(-1), mentionCount);
        }


        /// <summary>
        /// Retrieves a queryable collection of mentions, and returns it.
        /// </summary>
        /// <param name="context">The Twitter context.</param>
        /// <param name="help">The object with Twitter API request rate limits.</param>
        /// <returns>A queryable collection of mentions.</returns>
        /// <remarks>
        /// If the request cannot be performed because the API limit of number of calls has been reached, an empty queryable collection is returned. 
        /// </remarks>
        private IQueryable<Status> GetMentions(TwitterContext context, Help help)
        {
            if (TryPerformRequest("statuses", "/statuses/mentions_timeline", help))
            {
                return context.Status.Where(x => x.Type == StatusType.Mentions && x.Count == 800 && x.TrimUser == true);
            }

            return Enumerable.Empty<Status>().AsQueryable();
        }


        /// <summary>
        /// Collects insights for posts that belong to the specified Twitter account, if the Twitter API request rate limits allow it.
        /// </summary>
        /// <param name="account">The Twitter account.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        /// <param name="context">The Twitter context.</param>
        /// <param name="help">The object with Twitter API request rate limits.</param>
        private void ProcessPosts(TwitterAccountInfo account, DateTime currentDateTime, TwitterContext context, Help help)
        {
            // Process tweets in order they were last modified so the most current ones are updated first
            foreach (TwitterPostInfo post in TwitterPostInfoProvider.GetTwitterPostInfoByAccountId(account.TwitterAccountID)
                .WhereNotNull("TwitterPostExternalID")                                                      // Skip tweets that are not available on the Twitter (they only exist in CMS)
                .Where(
                    new WhereCondition().WhereNull("TwitterPostInsightsUpdateDateTime").Or()                // Skip tweets that were not published or
                    .WhereLessOrEquals("TwitterPostInsightsUpdateDateTime", currentDateTime.AddDays(-1))    // that were updated less then a day ago to save Twitter API calls
                )
                .OrderByDescending("TwitterPostLastModified"))
            {
                if (!TryPerformRequest("statuses", "/statuses/show/:id", help, () => ProcessPost(post, currentDateTime, context)))
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Collects insights for the specified post.
        /// </summary>
        /// <param name="post">The Twitter post.</param>
        /// <param name="currentDateTime">The current date and time.</param>
        /// <param name="context">The Twitter context.</param>
        private void ProcessPost(TwitterPostInfo post, DateTime currentDateTime, TwitterContext context)
        {
            try
            {
                Status status = context.Status.Single(x => x.Type == StatusType.Show && x.ID == UInt64.Parse(post.TwitterPostExternalID));
                if (status.FavoriteCount.HasValue)
                {
                    post.TwitterPostFavorites = status.FavoriteCount.Value;
                }

                post.TwitterPostRetweets = status.RetweetCount;
                post.TwitterPostInsightsUpdateDateTime = currentDateTime;
                TwitterPostInfoProvider.SetTwitterPostInfo(post);
            }
            catch (TwitterQueryException ex) when (TwitterHelper.IsPostNotFoundErrorCode(ex.ErrorCode))
            {
                // In case of not existing post set the error code for the current twitter post in CMS DB, therefore the post will be marked as faulty and processing of this
                // post will be omitted in the future. According to the error code proper message will be displayed in the UI.
                post.TwitterPostErrorCode = ex.ErrorCode;

                // Set external ID to null to let the system know the post is not successfully posted
                post.TwitterPostExternalID = null;

                TwitterPostInfoProvider.SetTwitterPostInfo(post);
            }
        }


        /// <summary>
        /// Performs the specified action, if the current Twitter API request rate limits allow it.
        /// </summary>
        /// <param name="categoryName">The API resource category name.</param>
        /// <param name="resourceName">The API resource name.</param>
        /// <param name="help">The object with Twitter API request rate limits.</param>
        /// <param name="request">The optional action to perform, if the Twitter API request rate limits allow it.</param>
        /// <returns>A value indicating whether the specified action was performed.</returns>
        private bool TryPerformRequest(string categoryName, string resourceName, Help help, Action request = null)
        {
            RateLimits limits = help.RateLimits[categoryName].Single(x => x.Resource == resourceName);
            if (limits.Remaining > 0)
            {
                limits.Remaining--;
                if (request != null)
                {
                    request();
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Creates a new instance of the Twitter context, and returns it.
        /// </summary>
        /// <param name="channel">The Twitter channel to create the context for.</param>
        /// <returns>A new instance of the Twitter context.</returns>
        private TwitterContext CreateContext(TwitterAccountInfo channel)
        {
            TwitterApplicationInfo application = TwitterApplicationInfoProvider.GetTwitterApplicationInfo(channel.TwitterAccountTwitterApplicationID);
            IAuthorizer authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = application.TwitterApplicationConsumerKey,
                    ConsumerSecret = application.TwitterApplicationConsumerSecret,
                    AccessToken = channel.TwitterAccountAccessToken,
                    AccessTokenSecret = channel.TwitterAccountAccessTokenSecret
                }
            };

            return new TwitterContext(authorizer);
        }


        /// <summary>
        /// Gets or creates a Twitter insight, and returns it.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The insight channel external identifier.</param>
        /// <param name="periodType">The insight period type.</param>
        /// <returns>A Twitter insight.</returns>
        private InsightInfo GetOrCreateInsight(string codeName, string externalId, string periodType)
        {
            InsightInfo insight = InsightInfoProvider.GetInsight(codeName, externalId, periodType);
            if (insight == null)
            {
                insight = new InsightInfo
                {
                    InsightCodeName = codeName,
                    InsightExternalID = externalId,
                    InsightPeriodType = periodType
                };
                InsightInfoProvider.SetInsight(insight);
            }

            return insight;
        }


        /// <summary>
        /// Checks whether Insights collection is available.
        /// </summary>
        /// <param name="taskInfo">Task info.</param>
        /// <returns>True if Insights collection is available, false otherwise.</returns>
        private bool IsFeatureAvailable(TaskInfo taskInfo)
        {
            SiteInfo siteInfo = SiteInfoProvider.GetSiteInfo(taskInfo.TaskSiteID);
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(siteInfo.DomainName, FeatureEnum.SocialMarketingInsights))
            {
                return false;
            }
            return true;
        }

        #endregion

    }
}
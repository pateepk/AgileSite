using System;
using System.Linq;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Scheduler;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Handles scheduled post publishing when activated by the scheduler.
    /// </summary>
    public class SocialMarketingPostPublishingTask : ITask
    {
        #region "Constants - public"

        /// <summary>
        /// Scheduled task name format for Facebook.
        /// Usage: <code>var completeTaskCodeName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_FACEBOOK, {FacebookPostInfoID});</code>
        /// </summary>
        public const string TASK_CODENAME_FORMAT_FACEBOOK = TASK_CODENAME_PREFIX_FACEBOOK + "_{0}";


        /// <summary>
        /// Scheduled task name format for Facebook.
        /// Usage: <code>var completeTaskCodeName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_PREFIX_LINKEDIN, {LinkedInPostInfoID});</code>
        /// </summary>
        public const string TASK_CODENAME_FORMAT_LINKEDIN = TASK_CODENAME_PREFIX_LINKEDIN + "_{0}";


        /// <summary>
        /// Scheduled task name format for Twitter.
        /// Usage: <code>var completeTaskCodeName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_TWITTER, {TwitterPostInfoID});</code>
        /// </summary>
        public const string TASK_CODENAME_FORMAT_TWITTER = TASK_CODENAME_PREFIX_TWITTER + "_{0}";

        #endregion


        #region "Constants - private"

        /// <summary>
        /// Scheduled task name prefix for Facebook.
        /// </summary>
        private const string TASK_CODENAME_PREFIX_FACEBOOK = "FacebookPostPublishing";


        /// <summary>
        /// Scheduled task name prefix for LinkedIn.
        /// </summary>
        private const string TASK_CODENAME_PREFIX_LINKEDIN = "LinkedInPostPublishing";


        /// <summary>
        /// Scheduled task name prefix for Twitter.
        /// </summary>
        private const string TASK_CODENAME_PREFIX_TWITTER = "TwitterPostPublishing";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Executes scheduled post publishing.
        /// </summary>
        /// <param name="taskInfo">Contains information related to the post.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        public string Execute(TaskInfo taskInfo)
        {
            if (taskInfo.TaskName.StartsWithCSafe(TASK_CODENAME_PREFIX_FACEBOOK))
            {
                return Execute_Facebook(taskInfo);
            }
            if (taskInfo.TaskName.StartsWithCSafe(TASK_CODENAME_PREFIX_LINKEDIN))
            {
                return Execute_LinkedIn(taskInfo);
            }
            if (taskInfo.TaskName.StartsWithCSafe(TASK_CODENAME_PREFIX_TWITTER))
            {
                return Execute_Twitter(taskInfo);
            }
            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Executes scheduled Facebook post publishing.
        /// </summary>
        /// <param name="taskInfo">Contains information related to the post.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        private string Execute_Facebook(TaskInfo taskInfo)
        {
            FacebookPostInfo facebookPost = FacebookPostInfoProvider.GetFacebookPostInfo(ValidationHelper.GetInteger(taskInfo.TaskData, 0));
            if ((facebookPost != null) && String.IsNullOrWhiteSpace(facebookPost.FacebookPostExternalID))
            {
                try
                {
                    FacebookPostInfoProvider.PublishFacebookPostToFacebook(facebookPost.FacebookPostID);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - Facebook post", "PUBLISHSCHEDULEDPOST", ex, facebookPost.FacebookPostSiteID,
                        String.Format("An error occurred while publishing scheduled Facebook post with ID {0}.", facebookPost.FacebookPostID));
                    
                    return "Facebook scheduled post failed. See event log for details.";
                }
            }
            return null;
        }


        /// <summary>
        /// Executes scheduled LinkedIn post publishing.
        /// </summary>
        /// <param name="taskInfo">Contains information related to the post.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        private string Execute_LinkedIn(TaskInfo taskInfo)
        {
            LinkedInPostInfo post = LinkedInPostInfoProvider.GetLinkedInPostInfo(ValidationHelper.GetInteger(taskInfo.TaskData, 0));
            if ((post != null) && String.IsNullOrWhiteSpace(post.LinkedInPostUpdateKey))
            {
                try
                {
                    LinkedInPostInfoProvider.PublishLinkedInPostToLinkedIn(post.LinkedInPostID);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - LinkedIn post", "PUBLISHSCHEDULEDPOST", ex, post.LinkedInPostSiteID,
                        String.Format("An error occurred while publishing scheduled LinkedIn post with ID {0}.", post.LinkedInPostID));

                    return "LinkedIn scheduled post failed. See event log for details.";
                }
            }
            return null;
        }


        /// <summary>
        /// Executes scheduled Twitter post publishing.
        /// </summary>
        /// <param name="taskInfo">Contains information related to the post.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        private string Execute_Twitter(TaskInfo taskInfo)
        {
            TwitterPostInfo twitterPost = TwitterPostInfoProvider.GetTwitterPostInfo(ValidationHelper.GetInteger(taskInfo.TaskData, 0));
            if ((twitterPost != null) && String.IsNullOrWhiteSpace(twitterPost.TwitterPostExternalID))
            {
                try
                {
                    TwitterPostInfoProvider.PublishTwitterPostToTwitter(twitterPost.TwitterPostID);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogWarning("Social marketing - Twitter post", "PUBLISHSCHEDULEDPOST", ex, twitterPost.TwitterPostSiteID,
                        String.Format("An error occurred while publishing scheduled Twitter post with ID {0}.", twitterPost.TwitterPostID));

                    return "Twitter scheduled post failed. See event log for details.";
                }
            }
            return null;
        }


        #endregion
    }
}
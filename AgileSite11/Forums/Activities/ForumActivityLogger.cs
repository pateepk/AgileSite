using System;

using CMS.Activities;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.Forums
{
    /// <summary>
    /// Provides methods for forum activities logging.
    /// </summary>
    public class ForumActivityLogger
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();

        /// <summary>
        /// Logs forum subscription activity.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumSubscriptionInfo">Forum subscription info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or <paramref name="forumSubscriptionInfo"/> is <c>null</c>.</exception>
        public void LogForumSubscriptionActivity(ForumInfo forumInfo, ForumSubscriptionInfo forumSubscriptionInfo, TreeNode currentDocument)
        {
            if (forumInfo == null)
            {
                throw new ArgumentNullException("forumInfo");
            }

            if (forumSubscriptionInfo == null)
            {
                throw new ArgumentNullException("forumSubscriptionInfo");
            }

            var initializer = new ForumSubscriptionActivityInitializer(forumInfo, forumSubscriptionInfo, currentDocument);
            mActivityLogService.Log(initializer, CMSHttpContext.Current.Request);
        }


        /// <summary>
        /// Logs forum post activity.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumPostInfo">Forum post info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or 
        /// <paramref name="forumPostInfo"/> or <paramref name="currentDocument"/> is <c>null</c>.</exception>
        public void LogForumPostActivity(ForumInfo forumInfo, ForumPostInfo forumPostInfo, TreeNode currentDocument)
        {
            if (forumInfo == null)
            {
                throw new ArgumentNullException("forumInfo");
            }

            if (forumPostInfo == null)
            {
                throw new ArgumentNullException("forumPostInfo");
            }

            if (currentDocument == null)
            {
                throw new ArgumentNullException("currentDocument");
            }

            var initializer = new ForumPostActivityInitializer(forumInfo, forumPostInfo, currentDocument);
            mActivityLogService.Log(initializer, CMSHttpContext.Current.Request);
        }
    }
}
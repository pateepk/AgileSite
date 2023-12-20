using System;

using CMS.DocumentEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Provides possibility to log forum activities.
    /// </summary>
    public interface IForumActivityLogger
    {
        /// <summary>
        /// Logs forum subscription activity.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumSubscriptionInfo">Forum subscription info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or <paramref name="forumSubscriptionInfo"/> is <c>null</c>.</exception>
        void LogForumSubscriptionActivity(ForumInfo forumInfo, ForumSubscriptionInfo forumSubscriptionInfo, TreeNode currentDocument);


        /// <summary>
        /// Logs forum post activity.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumPostInfo">Forum post info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or 
        /// <paramref name="forumPostInfo"/> or <paramref name="currentDocument"/> is <c>null</c>.</exception>
        void LogForumPostActivity(ForumInfo forumInfo, ForumPostInfo forumPostInfo, TreeNode currentDocument);
    }
}
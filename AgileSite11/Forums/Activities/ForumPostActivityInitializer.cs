using System;

using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Represents initializer for forum post.
    /// </summary>
    internal class ForumPostActivityInitializer : IActivityInitializer
    {
        private readonly ForumPostInfo mForumPostInfo;
        private readonly ForumInfo mForumInfo;
        private readonly ITreeNode mCurrentDocument;
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="ForumPostActivityInitializer"/>.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumPostInfo">Forum post info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or 
        /// <paramref name="forumPostInfo"/> or <paramref name="currentDocument"/> is <c>null</c>.</exception>
        public ForumPostActivityInitializer(ForumInfo forumInfo, ForumPostInfo forumPostInfo, TreeNode currentDocument)
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

            mForumPostInfo = forumPostInfo;
            mForumInfo = forumInfo;
            mCurrentDocument = currentDocument;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is <c>null</c>.</exception>
        public void Initialize(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            activity.ActivityTitle = mActivityTitleBuilder.CreateTitle(ActivityType, mForumInfo.ForumName);
            activity.ActivityItemID = mForumInfo.ForumID;
            activity.ActivityItemDetailID = mForumPostInfo.PostId;
            activity.ActivityNodeID = mCurrentDocument.NodeID;
            activity.ActivityCulture = mCurrentDocument.DocumentCulture;
        }


        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.FORUM_POST;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMForumPosts";
            }
        }
    }
}

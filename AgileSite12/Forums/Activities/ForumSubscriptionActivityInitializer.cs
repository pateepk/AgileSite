using System;

using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Represents initializer for forum subscription.
    /// </summary>
    internal class ForumSubscriptionActivityInitializer : IActivityInitializer
    {
        private readonly ForumInfo mForumInfo;
        private readonly ForumSubscriptionInfo mForumSubscriptionInfo;
        private readonly ITreeNode mCurrentDocument;
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="ForumSubscriptionActivityInitializer"/>.
        /// </summary>
        /// <param name="forumInfo">Forum info</param>
        /// <param name="forumSubscriptionInfo">Forum subscription info</param>
        /// <param name="currentDocument">Current document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="forumInfo"/> or <paramref name="forumSubscriptionInfo"/> is <c>null</c>.</exception>
        public ForumSubscriptionActivityInitializer(ForumInfo forumInfo, ForumSubscriptionInfo forumSubscriptionInfo, TreeNode currentDocument)
        {
            if (forumInfo == null)
            {
                throw new ArgumentNullException("forumInfo");
            }

            if (forumSubscriptionInfo == null)
            {
                throw new ArgumentNullException("forumSubscriptionInfo");
            }

            mForumInfo = forumInfo;
            mForumSubscriptionInfo = forumSubscriptionInfo;
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
            activity.ActivityItemDetailID = mForumSubscriptionInfo.SubscriptionID;

            if (mCurrentDocument != null)
            {
                activity.ActivityNodeID = mCurrentDocument.NodeID;
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            }
        }


        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.SUBSCRIPTION_FORUM_POST;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMForumPostSubscription";
            }
        }
    }
}

using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.MessageBoards
{
    internal class MessageBoardSubscriptionActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly BoardInfo mBoardInfo;
        private readonly ITreeNode mCurrentDocument;
        private readonly int mSubscriptionId;


        /// <summary>
        /// Constructor of <see cref="MessageBoardSubscriptionActivityInitializer"/>.
        /// </summary>
        public MessageBoardSubscriptionActivityInitializer(BoardInfo boardInfo, TreeNode currentDocument, int subscriptionId)
        {
            mBoardInfo = boardInfo;
            mCurrentDocument = currentDocument;
            mSubscriptionId = subscriptionId;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mBoardInfo.BoardDisplayName);
            activity.ActivityItemID = mBoardInfo.BoardID;
            activity.ActivityItemDetailID = mSubscriptionId;

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
                return PredefinedActivityType.SUBSCRIPTION_MESSAGE_BOARD;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMMessageBoardSubscription";
            }
        }
    }
}

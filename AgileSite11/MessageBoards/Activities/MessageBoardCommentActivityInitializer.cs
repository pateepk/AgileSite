using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.MessageBoards
{
    internal class MessageBoardCommentActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly BoardMessageInfo mBoardMessageInfo;
        private readonly ITreeNode mCurrentDocument;
        private readonly string mBoardDisplayName;


        /// <summary>
        /// Constructor of <see cref="MessageBoardCommentActivityInitializer"/>.
        /// </summary>
        public MessageBoardCommentActivityInitializer(BoardMessageInfo boardMessageInfo, TreeNode currentDocument, string boardDisplayName)
        {
            mBoardMessageInfo = boardMessageInfo;
            mBoardDisplayName = boardDisplayName;
            mCurrentDocument = currentDocument;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mBoardDisplayName);
            activity.ActivityItemID = mBoardMessageInfo.MessageBoardID;
            activity.ActivityItemDetailID = mBoardMessageInfo.MessageID;

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
                return PredefinedActivityType.MESSAGE_BOARD_COMMENT;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMMessageBoardPosts";
            }
        }
    }
}

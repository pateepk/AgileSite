using System;
using System.Web;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Provides methods for logging message board activities.
    /// </summary>
    public class MessageBoardActivityLogger
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Logs activity for message board comment.
        /// </summary>
        /// <param name="boardMessageInfo"><see cref="BoardMessageInfo"/> to log activity for</param>
        /// <param name="boardInfo"><see cref="BoardInfo"/> comment occurred on</param>
        /// <param name="currentDocument">Current document node; activity is logged even though document is null</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="boardMessageInfo"/> or <paramref name="boardInfo"/> is <c>null</c>.</exception>
        public void LogMessageBoardCommentActivity(BoardMessageInfo boardMessageInfo, BoardInfo boardInfo, TreeNode currentDocument)
        {
            if (boardMessageInfo == null)
            {
                throw new ArgumentNullException("boardMessageInfo");
            }

            if (boardInfo == null)
            {
                throw new ArgumentNullException("boardInfo");
            }

            if (boardInfo.BoardLogActivity)
            {
                var activityInitializer = new MessageBoardCommentActivityInitializer(boardMessageInfo, currentDocument, boardInfo.BoardDisplayName);
                mActivityLogService.Log(activityInitializer, GetCurrentRequest());
            }
        }


        /// <summary>
        /// Logs activity for message board comment.
        /// </summary>
        /// <param name="boardInfo"><see cref="BoardInfo"/> subscription occurred on</param>
        /// <param name="boardSubscriptionInfo"><see cref="BoardSubscriptionInfo"/> to log activity for</param>
        /// <param name="currentDocument">Current document node; activity is logged even though document is null</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="boardInfo"/> or <paramref name="boardSubscriptionInfo"/> is <c>null</c>.</exception>
        public void LogMessageBoardSubscriptionActivity(BoardInfo boardInfo, BoardSubscriptionInfo boardSubscriptionInfo, TreeNode currentDocument)
        {
            if (boardInfo == null)
            {
                throw new ArgumentNullException("boardInfo");
            }

            if (boardSubscriptionInfo == null)
            {
                throw new ArgumentNullException("boardSubscriptionInfo");
            }

            if (boardInfo.BoardLogActivity)
            {
                var activityInitializer = new MessageBoardSubscriptionActivityInitializer(boardInfo, currentDocument, boardSubscriptionInfo.SubscriptionID);
                mActivityLogService.Log(activityInitializer, GetCurrentRequest());
            }
        }


        /// <summary>
        /// Returns current request.
        /// </summary>
        /// <returns>Current request.</returns>
        protected virtual HttpRequestBase GetCurrentRequest()
        {
            return CMSHttpContext.Current.Request;
        }
    }
}

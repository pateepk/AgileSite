using System;

using CMS.DocumentEngine;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Provides possibility to log message board activities.
    /// </summary>
    public interface IMessageBoardActivityLogger
    {
        /// <summary>
        /// Logs activity for message board comment.
        /// </summary>
        /// <param name="boardMessageInfo"><see cref="BoardMessageInfo"/> to log activity for</param>
        /// <param name="boardInfo"><see cref="BoardInfo"/> comment occurred on</param>
        /// <param name="currentDocument">Current document node; activity is logged even though document is null</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="boardMessageInfo"/> or <paramref name="boardInfo"/> is <c>null</c>.</exception>
        void LogMessageBoardCommentActivity(BoardMessageInfo boardMessageInfo, BoardInfo boardInfo, TreeNode currentDocument);


        /// <summary>
        /// Logs activity for message board comment.
        /// </summary>
        /// <param name="boardInfo"><see cref="BoardInfo"/> subscription occurred on</param>
        /// <param name="boardSubscriptionInfo"><see cref="BoardSubscriptionInfo"/> to log activity for</param>
        /// <param name="currentDocument">Current document node; activity is logged even though document is null</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="boardInfo"/> or <paramref name="boardSubscriptionInfo"/> is <c>null</c>.</exception>
        void LogMessageBoardSubscriptionActivity(BoardInfo boardInfo, BoardSubscriptionInfo boardSubscriptionInfo, TreeNode currentDocument);
    }
}
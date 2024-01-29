using System;
using System.Data;
using System.Linq;
using System.Security.Principal;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.Search;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Class providing BoardMessageInfo management.
    /// </summary>
    public class BoardMessageInfoProvider : AbstractInfoProvider<BoardMessageInfo, BoardMessageInfoProvider>
    {
        #region "Properties"

        private static bool mEnableEmails = true;

        /// <summary>
        /// Indicates if e-mails are allowed to be sent to subscribers and moderators, by default it is set to True.
        /// </summary>
        public static bool EnableEmails
        {
            get
            {
                return mEnableEmails && CMSActionContext.CurrentSendEmails;
            }
            set
            {
                mEnableEmails = value;
            }
        }

        #endregion


        #region "Public methods - Basic"

        

        /// <summary>
        /// Returns the BoardMessageInfo structure for the specified boardMessage.
        /// </summary>
        /// <param name="boardMessageId">BoardMessage id</param>
        public static BoardMessageInfo GetBoardMessageInfo(int boardMessageId)
        {
            return ProviderObject.GetInfoById(boardMessageId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified boardMessage.
        /// </summary>
        /// <param name="boardMessage">BoardMessage to set</param>
        public static void SetBoardMessageInfo(BoardMessageInfo boardMessage)
        {
            ProviderObject.SetBoardMessageInfoInternal(boardMessage);
        }


        /// <summary>
        /// Deletes specified boardMessage.
        /// </summary>
        /// <param name="infoObj">BoardMessage object</param>
        public static void DeleteBoardMessageInfo(BoardMessageInfo infoObj)
        {
            ProviderObject.DeleteBoardMessageInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified boardMessage.
        /// </summary>
        /// <param name="boardMessageId">BoardMessage id</param>
        public static void DeleteBoardMessageInfo(int boardMessageId)
        {
            BoardMessageInfo infoObj = GetBoardMessageInfo(boardMessageId);
            DeleteBoardMessageInfo(infoObj);
        }


        /// <summary>
        /// Returns messages object query.
        /// </summary>
        public static ObjectQuery<BoardMessageInfo> GetMessages()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all messages according to where and order by condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">OrderBy condition</param>
        public static DataSet GetMessages(string where, string orderBy)
        {
            return GetMessages()
                        .Where(where)
                        .OrderBy(orderBy);
        }


        /// <summary>
        /// Returns dataset of all messages for given board.
        /// </summary>
        /// <param name="boardId">Message board ID</param>
        public static DataSet GetMessages(int boardId)
        {
            return GetMessages().WhereEquals("MessageBoardID", boardId);
        }


        /// <summary>
        /// Returns count of messages under message board specified by name and document ID.
        /// </summary>
        /// <param name="boardId">ID of the message board.</param>
        /// <param name="approvedOnly">Allows to count approved messages only.</param>
        /// <param name="withoutSpam">Allows to count messages not marked as spam only.</param>
        public static int GetMessagesCount(int boardId, bool approvedOnly, bool withoutSpam)
        {
            var where = GetMessagesWhereCondition(boardId, approvedOnly, withoutSpam);

            return GetMessages()
                    .Where(where)
                    .Count;
        }
        
        #endregion


        #region "Public methods - Advanced"
        
        /// <summary>
        /// Returns messages for specified document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static DataSet GetDocumentMessages(int documentId)
        {
            var boards = BoardInfoProvider.GetMessageBoards()
                                    .Column("BoardID")
                                    .WhereEquals("BoardDocumentID", documentId)
                                    .WhereTrue("BoardEnabled")
                                    .WhereEquals("BoardAccess", 0);

            if (DataHelper.DataSourceIsEmpty(boards))
            {
                return null;
            }

            return GetMessages()
                    .WhereTrue("MessageApproved")
                    .WhereIn("MessageBoardID", boards);
        }


        /// <summary>
        /// Checks if the given user is owner of the given message.
        /// </summary>
        /// <param name="userId">Id of the user to check</param>
        /// <param name="messageId">ID of the message to check</param>        
        public static bool IsUserMessageOwner(int userId, int messageId)
        {
            var count = GetMessages()
                    .TopN(1)
                    .Column("MessageID")
                    .WhereEquals("MessageUserID", userId)
                    .WhereEquals("MessageID", messageId)
                    .Count;

            return count > 0;
        }


        /// <summary>
        /// Sends a notification e-mail to all board subscribers and to all board moderators.
        /// </summary>
        /// <param name="message">Board message data</param>
        /// <param name="toSubscribers">Indicates if notification email should be sent to board subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to board moderators</param>        
        public static void SendNewMessageNotification(BoardMessageInfo message, bool toSubscribers, bool toModerators)
        {
            if (!CMSActionContext.CurrentSendNotifications)
            {
                return;
            }

            if (toSubscribers || toModerators)
            {
                ThreadEmailSender sender = new ThreadEmailSender(message);
                sender.SendNewMessageNotification(WindowsIdentity.GetCurrent(), toSubscribers, toModerators);
            }
        }


        /// <summary>
        /// Updates document ratings according to old and new rating values.
        /// </summary>
        /// <param name="boardId">Board id</param>
        /// <param name="oldRating">Old rating value</param>
        /// <param name="newRating">New rating value</param>
        private static void UpdateDocumentRating(int boardId, double oldRating, double newRating)
        {
            // Do not update document rating when in context of import
            if (!CMSActionContext.CurrentUpdateRating)
            {
                return;
            }

            if ((oldRating > 0.0f) || (newRating > 0.0f))
            {
                // Get board info object
                BoardInfo bi = BoardInfoProvider.GetBoardInfo(boardId);
                if (bi == null)
                {
                    return;
                }

                // Get document where the message board is placed
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                TreeNode node = tree.SelectSingleDocument(bi.BoardDocumentID);

                if (node == null)
                {
                    return;
                }

                // Remove old rating
                if (oldRating > 0.0f)
                {
                    node.DocumentRatingValue -= oldRating;
                    node.DocumentRatings--;
                }
                // Add new rating
                if (newRating > 0.0f)
                {
                    node.DocumentRatingValue += newRating;
                    node.DocumentRatings++;
                }

                // Set rating of the given document
                TreeProvider.SetRating(node, node.DocumentRatingValue, node.DocumentRatings);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Updates message board messages count, last message time and last message user name.
        /// </summary>
        /// <param name="boardID">Board id</param>
        private static void UpdateMessageBoardCounts(int boardID)
        {
            if (boardID <= 0)
            {
                return;
            }

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@boardID", boardID);

            ConnectionHelper.ExecuteQuery("board.message.updatecounts", parameters);
        }


        /// <summary>
        /// Returns sitename for selected board.
        /// </summary>
        /// <param name="boardId">Board id</param>
        private static string GetBoardSiteName(int boardId)
        {
            BoardInfo bi = BoardInfoProvider.GetBoardInfo(boardId);
            if (bi == null)
            {
                return null;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(bi.BoardSiteID);
            if (si != null)
            {
                return si.SiteName;
            }

            return null;
        }


        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        internal static void UpdateData(string updateExpression, WhereCondition where)
        {
            string whereCondition = null;
            QueryDataParameters parameters = null;
            if (where != null)
            {
                whereCondition = where.WhereCondition;
                parameters = where.Parameters;
            }

            ProviderObject.UpdateData(updateExpression, parameters, whereCondition);
        }


        /// <summary>
        /// Gets where condition to filter messages
        /// </summary>
        /// <param name="boardId">ID of the message board.</param>
        /// <param name="approvedOnly">Allows to count approved messages only.</param>
        /// <param name="withoutSpam">Allows to count messages not marked as spam only.</param>
        internal static WhereCondition GetMessagesWhereCondition(int boardId, bool approvedOnly, bool withoutSpam)
        {
            var condition = new WhereCondition().WhereEquals("MessageBoardID", boardId);

            // Filter approved messages
            if (approvedOnly)
            {
                condition.WhereTrue("MessageApproved");
            }

            // Filter non-spam messages
            if (withoutSpam)
            {
                condition.WhereEquals("MessageIsSpam".AsColumn().IsNull(0), 0);
            }

            return condition;
        }


        #endregion


        #region "Internal methods - Basic"
        
        /// <summary>
        /// Sets (updates or inserts) specified boardMessage.
        /// </summary>
        /// <param name="boardMessage">BoardMessage to set</param>
        protected virtual void SetBoardMessageInfoInternal(BoardMessageInfo boardMessage)
        {
            // Check license for message boards
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MessageBoards);
            }

            if (boardMessage == null)
            {
                throw new NullReferenceException("BoardMessageInfo object is not specified.");
            }

            bool? addPoints = null;
            bool emailToSubscribers = false;
            bool emailToModerators = false;
            double oldRating = 0.0f;

            boardMessage.SetValue("MessageUserInfo", boardMessage.MessageUserInfo.GetData());

            if (boardMessage.MessageID > 0)
            {
                BoardMessageInfo oldBoarMess = GetBoardMessageInfo(boardMessage.MessageID);
                if (oldBoarMess != null)
                {
                    // Rejected -> Approved
                    if ((!oldBoarMess.MessageApproved) && (boardMessage.MessageApproved))
                    {
                        addPoints = true;
                        emailToSubscribers = true;
                    }
                        // Approved -> Rejected
                    else if ((oldBoarMess.MessageApproved) && (!boardMessage.MessageApproved))
                    {
                        addPoints = false;
                    }

                    // Get old rating value
                    oldRating = oldBoarMess.MessageRatingValue;
                }

                boardMessage.Generalized.UpdateData();
            }
            else
            {
                boardMessage.Generalized.InsertData();

                if (boardMessage.MessageApproved)
                {
                    addPoints = true;
                    emailToSubscribers = true;
                }
                else
                {
                    emailToModerators = true;
                }
            }

            // Send new message notification
            SendNewMessageNotification(boardMessage, emailToSubscribers, emailToModerators);

            // Update document rating
            UpdateDocumentRating(boardMessage, addPoints, oldRating);

            // Update message board
            UpdateMessageBoardCounts(boardMessage.MessageBoardID);

            if (addPoints.HasValue)
            {
                string siteName = GetBoardSiteName(boardMessage.MessageBoardID);
                if (!String.IsNullOrEmpty(siteName))
                {
                    // Add activity points
                    BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.MessageBoardPost, boardMessage.MessageUserID, siteName, (bool)addPoints);
                }
            }

            // Update search index
            if (SearchIndexInfoProvider.SearchEnabled)
            {
                CreateSearchTask(boardMessage);
            }
        }


        private static void CreateSearchTask(BoardMessageInfo messageInfo)
        {
            BoardInfo bi = BoardInfoProvider.GetBoardInfo(messageInfo.MessageBoardID);
            if (bi == null)
            {
                return;
            }

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode node = tree.SelectSingleDocument(bi.BoardDocumentID);

            // Update search index for given document
            if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
            }
        }


        private static void UpdateDocumentRating(BoardMessageInfo boardMessage, bool? addPoints, double oldRating)
        {
            // Update document ratings...
            if (addPoints != null)
            {
                if (addPoints.Value)
                {
                    // Message was approved
                    UpdateDocumentRating(boardMessage.MessageBoardID, 0.0f, boardMessage.MessageRatingValue);
                }
                else
                {
                    // Message was rejected
                    UpdateDocumentRating(boardMessage.MessageBoardID, oldRating, 0.0f);
                }
            }
            else if (boardMessage.MessageApproved && (oldRating != boardMessage.MessageRatingValue))
            {
                // Rating of approved message was changed
                UpdateDocumentRating(boardMessage.MessageBoardID, oldRating, boardMessage.MessageRatingValue);
            }
        }


        /// <summary>
        /// Deletes specified boardMessage.
        /// </summary>
        /// <param name="infoObj">BoardMessage object</param>
        protected virtual void DeleteBoardMessageInfoInternal(BoardMessageInfo infoObj)
        {
            if (infoObj == null)
            {
                return;
            }

            int userId = infoObj.MessageUserID;
            bool approved = infoObj.MessageApproved;
            int boardId = infoObj.MessageBoardID;

            infoObj.Generalized.DeleteData();

            if (infoObj.MessageApproved)
            {
                // Update document ratings - subtract rating value of approved message
                UpdateDocumentRating(boardId, infoObj.MessageRatingValue, 0.0f);
            }

            // Update message board
            UpdateMessageBoardCounts(infoObj.MessageBoardID);

            if ((userId > 0) && (approved))
            {
                string siteName = GetBoardSiteName(boardId);
                if (!String.IsNullOrEmpty(siteName))
                {
                    BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.MessageBoardPost, userId, siteName, false);
                }
            }

            // Update search index
            if (SearchIndexInfoProvider.SearchEnabled)
            {
                CreateSearchTask(infoObj);
            }
        }

        #endregion
    }
}
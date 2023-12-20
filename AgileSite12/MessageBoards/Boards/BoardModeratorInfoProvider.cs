using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Class providing BoardModeratorInfo management.
    /// </summary>
    public class BoardModeratorInfoProvider : AbstractInfoProvider<BoardModeratorInfo, BoardModeratorInfoProvider>
    {
        #region "Methods"
        
        /// <summary>
        /// Returns object query for board moderators
        /// </summary>
        public static ObjectQuery<BoardModeratorInfo> GetBoardModerators()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the BoardModeratorInfo structure for the specified boardModerator.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="boardId">BoardID</param>
        public static BoardModeratorInfo GetBoardModeratorInfo(int userId, int boardId)
        {
            if ((userId <= 0) || (boardId <= 0))
            {
                return null;
            }
            
            return GetBoardModerators()
                        .TopN(1)
                        .WhereEquals("BoardID", boardId)
                        .WhereEquals("UserID", userId)
                        .FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified boardModerator.
        /// </summary>
        /// <param name="boardModerator">BoardModerator to set</param>
        public static void SetBoardModeratorInfo(BoardModeratorInfo boardModerator)
        {
            if (boardModerator == null)
            {
                throw new ArgumentNullException("boardModerator");
            }

            // Check IDs
            if ((boardModerator.UserID <= 0) || (boardModerator.BoardID <= 0))
            {
                throw new InvalidOperationException("Object IDs are not set.");
            }

            // Get existing
            BoardModeratorInfo existing = GetBoardModeratorInfo(boardModerator.UserID, boardModerator.BoardID);
            if (existing != null)
            {
                boardModerator.Generalized.UpdateData();
            }
            else
            {
                boardModerator.Generalized.InsertData();
            }
        }


        /// <summary>
        /// Deletes specified boardModerator.
        /// </summary>
        /// <param name="infoObj">BoardModerator object</param>
        public static void DeleteBoardModeratorInfo(BoardModeratorInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified boardModerator.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="boardId">BoardID</param>
        public static void DeleteBoardModeratorInfo(int userId, int boardId)
        {
            BoardModeratorInfo infoObj = GetBoardModeratorInfo(userId, boardId);
            DeleteBoardModeratorInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified BoardModerator info object.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="boardId">Board id</param>
        public static void RemoveModeratorFromBoard(int userId, int boardId)
        {
            DeleteBoardModeratorInfo(userId, boardId);
        }


        /// <summary>
        /// Adds new BoardModeratorInfo object.
        /// </summary>        
        /// <param name="boardId">ID of the board moderator is being added to</param>
        /// <param name="userId">ID of the user representing board moderator</param>
        public static void AddModeratorToBoard(int userId, int boardId)
        {
            UserInfo user = UserInfoProvider.GetUserInfo(userId);
            BoardInfo board = BoardInfoProvider.GetBoardInfo(boardId);

            if ((user == null) || (board == null))
            {
                return;
            }

            // Create new binding
            BoardModeratorInfo infoObj = new BoardModeratorInfo();
            infoObj.UserID = userId;
            infoObj.BoardID = boardId;

            // Save to the database
            SetBoardModeratorInfo(infoObj);
        }


        /// <summary>
        /// Checks if the given user is moderator of the given board.
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="boardId">ID of the board to check</param>        
        public static bool IsUserBoardModerator(int userId, int boardId)
        {
            return GetBoardModeratorInfo(userId, boardId) != null;
        }

       
        /// <summary>
        /// Inserts moderators for the specified board.
        /// </summary>
        /// <param name="boardId">ID of the board moderators are inserted to</param>
        /// <param name="moderators">String containing user names separated by semicolon</param>
        public static void SetBoardModerators(int boardId, string moderators)
        {
            SetBoardModerators(boardId, moderators, false);
        }


        /// <summary>
        /// Inserts moderators for the specified board.
        /// </summary>
        /// <param name="boardId">ID of the board moderators are inserted to</param>
        /// <param name="moderators">String containing user names separated by semicolon</param>
        /// <param name="useIds">Indicates whether the string representing moderators contains user IDs rather than user names</param>
        public static void SetBoardModerators(int boardId, string moderators, bool useIds)
        {
            if (String.IsNullOrEmpty(moderators))
            {
                return;
            }

            // Go through the moderators and create record for each item
            string[] moderatorsArr = moderators.Split(';');
            foreach (string moderator in moderatorsArr)
            {
                // Reset moderator
                UserInfo ui = null;

                if (useIds)
                {
                    int moderatorId = ValidationHelper.GetInteger(moderator, 0);
                    if (moderatorId > 0)
                    {
                        ui = UserInfoProvider.GetUserInfo(moderatorId);
                    }
                }
                else
                {
                    ui = UserInfoProvider.GetUserInfo(moderator);
                }

                if (ui == null)
                {
                    continue;
                }

                // Set the moderator
                var bmi = new BoardModeratorInfo();
                bmi.BoardID = boardId;
                bmi.UserID = ui.UserID;
                SetBoardModeratorInfo(bmi);
            }
        }
        
        #endregion
    }
}
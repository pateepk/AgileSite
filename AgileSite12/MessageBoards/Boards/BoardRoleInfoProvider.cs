using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Class providing BoardRoleInfo management.
    /// </summary>
    public class BoardRoleInfoProvider : AbstractInfoProvider<BoardRoleInfo, BoardRoleInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the BoardRoleInfo structure for the specified boardRole.
        /// </summary>
        /// <param name="boardId">Board id</param>
        /// <param name="roleId">Role id</param>
        public static BoardRoleInfo GetBoardRoleInfo(int roleId, int boardId)
        {
            return GetBoardRoles()
                .TopN(1)
                .WhereEquals("BoardID", boardId)
                .WhereEquals("RoleID", roleId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified boardRole.
        /// </summary>
        /// <param name="boardRole">BoardRole to set</param>
        public static void SetBoardRoleInfo(BoardRoleInfo boardRole)
        {
            if (boardRole == null)
            {
                throw new ArgumentNullException("boardRole");
            }

            // Check IDs
            if ((boardRole.RoleID <= 0) || (boardRole.BoardID <= 0))
            {
                throw new InvalidOperationException("Object IDs are not set.");
            }

            // Get existing
            BoardRoleInfo existing = GetBoardRoleInfo(boardRole.RoleID, boardRole.BoardID);
            if (existing == null)
            {
                boardRole.Generalized.InsertData();
            }
        }


        /// <summary>
        /// Deletes specified boardRole.
        /// </summary>
        /// <param name="infoObj">BoardRole object</param>
        public static void DeleteBoardRoleInfo(BoardRoleInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified boardRole.
        /// </summary>
        /// <param name="roleId">ID of the role to delete</param>
        /// <param name="boardId">ID of the board role is deleted from</param>
        public static void DeleteBoardRoleInfo(int roleId, int boardId)
        {
            BoardRoleInfo infoObj = GetBoardRoleInfo(roleId, boardId);
            DeleteBoardRoleInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified boardRole.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="boardId">BoardID</param>
        public static void RemoveRoleFromBoard(int roleId, int boardId)
        {
            DeleteBoardRoleInfo(roleId, boardId);
        }


        /// <summary>
        /// Adds specified role to the board.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="boardId">BoardID</param>
        public static void AddRoleToBoard(int roleId, int boardId)
        {
            RoleInfo role = RoleInfoProvider.GetRoleInfo(roleId);
            BoardInfo board = BoardInfoProvider.GetBoardInfo(boardId);

            if ((role != null) && (board != null))
            {
                // Create new binding
                BoardRoleInfo infoObj = new BoardRoleInfo();
                infoObj.RoleID = roleId;
                infoObj.BoardID = boardId;

                // Save to the database
                SetBoardRoleInfo(infoObj);
            }
        }

        
        /// <summary>
        /// Inserts roles for the specified board.
        /// </summary>
        /// <param name="boardId">ID of the board roles should be added to</param>
        /// <param name="roles">String containing role names separated by semicolon</param>
        public static void SetBoardRoles(int boardId, string roles)
        {
            // Go through the roleas and inserted new record for every item
            string[] rolesArr = roles.Split(';');
            foreach (string role in rolesArr)
            {
                if (role == "")
                {
                    continue;
                }

                BoardRoleInfo bri = new BoardRoleInfo();
                String siteName = role.StartsWithCSafe(".") ? "" : SiteContext.CurrentSiteName;
                RoleInfo ri = RoleInfoProvider.GetRoleInfo(role, siteName);
                if (ri == null)
                {
                    continue;
                }

                // Set the relationship
                bri.RoleID = ri.RoleID;
                bri.BoardID = boardId;
                SetBoardRoleInfo(bri);
            }
        }


        /// <summary>
        /// Returns object query for board moderators
        /// </summary>
        public static ObjectQuery<BoardRoleInfo> GetBoardRoles()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion
    }
}
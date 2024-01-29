using System;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Functions for message boards module.
    /// </summary>
    public class MessageBoardTransformationFunctions
    {
        /// <summary>
        /// Returns group GUID for community group specified as an URL parameter of the current request.
        /// </summary>        
        private static Guid GetCurrentGroupGuid()
        {
            // Try to get the group info from the request items collection
            BaseInfo gi = (BaseInfo) RequestStockHelper.GetItem(RequestStockItemName.CurrentGroup, true);
            if (gi != null)
            {
                return gi.Generalized.ObjectGUID;
            }

            // Try to get group by its GroupID first
            int groupId = QueryHelper.GetInteger("groupid", 0);
            if (groupId > 0)
            {
                gi = ModuleCommands.CommunityGetGroupInfo(groupId);
            }

            // If group was not found by its GroupID
            if (gi == null)
            {
                // Try to get group by its GroupName
                string groupName = QueryHelper.GetString("groupname", "");
                if (groupName != "")
                {
                    gi = ModuleCommands.CommunityGetGroupInfoByName(groupName, SiteContext.CurrentSiteName);
                }
            }

            if (gi == null)
            {
                // Try to get group by its GroupName
                Guid groupGuid = QueryHelper.GetGuid("groupguid", Guid.Empty);
                if (groupGuid != Guid.Empty)
                {
                    return groupGuid;
                }

                if (DocumentContext.CurrentPageInfo != null)
                {
                    // Try to get group from current document
                    groupId = DocumentContext.CurrentPageInfo.NodeGroupID;
                    if (groupId > 0)
                    {
                        gi = ModuleCommands.CommunityGetGroupInfo(groupId);
                    }
                }
            }
            
            // Save the group to the request items if new group was loaded from DB
            RequestStockHelper.Add(RequestStockItemName.CurrentGroup, gi, true);

            return (gi != null) ? gi.Generalized.ObjectGUID : Guid.Empty;
        }


        /// <summary>
        /// Returns the count of messages in given message board.
        /// </summary>
        /// <param name="documentId">ID of the document.</param>
        /// <param name="boardWebpartName">Messageboard webpart name.</param>
        /// <param name="type">Type of messageboard: 'document', 'user' or 'group'.</param>
        public static int GetBoardMessagesCount(int documentId, string boardWebpartName, string type)
        {
            // Get board type
            BoardOwnerTypeEnum boardType = BoardInfoProvider.GetBoardOwnerTypeEnum(type);

            Guid identifier = Guid.Empty;

            // Get correct identifier by type
            switch (boardType)
            {
                case BoardOwnerTypeEnum.User:
                    identifier = MembershipContext.AuthenticatedUser.UserGUID;
                    break;

                case BoardOwnerTypeEnum.Group:
                    identifier = GetCurrentGroupGuid();
                    break;
            }

            // Get board name
            string boardName = BoardInfoProvider.GetMessageBoardName(boardWebpartName, boardType, identifier.ToString());

            // Get board info
            BoardInfo board = BoardInfoProvider.GetBoardInfo(boardName, documentId);

            if (board != null)
            {
                // Get messages count
                return BoardMessageInfoProvider.GetMessagesCount(board.BoardID, true, true);
            }

            return 0;
        }


        /// <summary>
        /// Returns the count of messages in given document related message board.
        /// </summary>
        /// <param name="documentId">ID of the document.</param>
        /// <param name="boardWebpartName">Messageboard webpart name.</param>
        public static int GetBoardMessagesCount(int documentId, string boardWebpartName)
        {
            return GetBoardMessagesCount(documentId, boardWebpartName, "document");
        }
    }
}

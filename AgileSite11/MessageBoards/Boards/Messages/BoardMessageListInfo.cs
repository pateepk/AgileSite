using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardMessageListInfo), "board.boardmessagelist")]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardListInfo virtual object.
    /// </summary>
    internal class BoardMessageListInfo : AbstractInfo<BoardMessageListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "board.boardmessagelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardMessageListInfo object.
        /// </summary>
        public BoardMessageListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardMessageListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public BoardMessageListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, BoardMessageInfo.TYPEINFO);
            typeInfo.AllowDataExport = true;

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "BoardID",
                "BoardName",
                "BoardDisplayName",
                "BoardDescription",
                "BoardOpenedFrom",
                "BoardOpened",
                "BoardOpenedTo",
                "BoardEnabled",
                "BoardModerated",
                "BoardAccess",
                "BoardUseCaptcha",
                "BoardLastModified",
                "BoardMessages",
                "BoardDocumentID",
                "BoardGUID",
                "BoardUserID",
                "BoardGroupID",
                "BoardLastMessageTime",
                "BoardLastMessageUserName",
                "BoardUnsubscriptionURL",
                "BoardRequireEmails",
                "BoardSiteID",
                "BoardEnableSubscriptions",
                "BoardBaseURL",
                "MessageID",
                "MessageUserName",
                "MessageText",
                "MessageEmail",
                "MessageURL",
                "MessageIsSpam",
                "MessageBoardID",
                "MessageApproved",
                "MessageUserID",
                "MessageApprovedByUserID",
                "MessageUserInfo",
                "MessageAvatarGUID",
                "MessageInserted",
                "MessageLastModified",
                "MessageGUID",
                "MessageRatingValue",
                "GroupID",
                "GroupGUID",
                "GroupLastModified",
                "GroupSiteID",
                "GroupDisplayName",
                "GroupName",
                "GroupDescription",
                "GroupNodeGUID",
                "GroupApproveMembers",
                "GroupAccess",
                "GroupCreatedByUserID",
                "GroupApprovedByUserID",
                "GroupAvatarID",
                "GroupApproved",
                "GroupCreatedWhen",
                "GroupSendJoinLeaveNotification",
                "GroupSendWaitingForApprovalNotification",
                "GroupSecurity"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("board.message", "selectallwithboard");
        }

        #endregion
    }
}
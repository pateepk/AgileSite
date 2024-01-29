using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.MessageBoards
{
    using TypedDataSet = InfoDataSet<BoardInfo>;


    /// <summary>
    /// Class providing BoardInfo management.
    /// </summary>
    public class BoardInfoProvider : AbstractInfoProvider<BoardInfo, BoardInfoProvider>, IFullNameInfoProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="BoardInfoProvider"/>.
        /// </summary>
        public BoardInfoProvider()
            : base(BoardInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                FullName = true,
                Load = LoadHashtableEnum.None
            })
        {
        }

        #region "Methods"

        /// <summary>
        /// Returns the BoardInfo structure for the specified board.
        /// </summary>
        /// <param name="boardId">Board id.</param>
        public static BoardInfo GetBoardInfo(int boardId)
        {
            return ProviderObject.GetInfoById(boardId);
        }


        /// <summary>
        /// Returns the BoardInfo structure for the specified board.
        /// </summary>
        /// <param name="boardName">Board code name.</param>
        /// <param name="documentId">Document ID.</param>
        public static BoardInfo GetBoardInfo(string boardName, int documentId)
        {
            return ProviderObject.GetBoardInfoInternal(boardName, documentId);
        }


        /// <summary>
        /// Returns the number of message boards related to the specified document.
        /// </summary>
        /// <param name="documentId">Document ID to return number of boards for.</param> 
        public static int GetBoardsCount(int documentId)
        {
            return ProviderObject.GetBoardsCountInternal(documentId);
        }


        /// <summary>
        /// Returns the info on board matching specified criteria.
        /// </summary>
        /// <param name="boardName">Name of the board.</param>
        /// <param name="userId">ID of the user board belongs to.</param>
        public static BoardInfo GetBoardInfoForUser(string boardName, int userId)
        {
            return ProviderObject.GetBoardInfoForUserInternal(boardName, userId);
        }


        /// <summary>
        /// Returns the info on board matching specified criteria.
        /// </summary>
        /// <param name="boardName">Name of the board.</param>
        /// <param name="groupId">ID of the group board belongs to.</param>
        public static BoardInfo GetBoardInfoForGroup(string boardName, int groupId)
        {
            return ProviderObject.GetBoardInfoForGroupInternal(boardName, groupId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified board.
        /// </summary>
        /// <param name="board">Board to set.</param>
        public static void SetBoardInfo(BoardInfo board)
        {
            ProviderObject.SetInfo(board);
        }


        /// <summary>
        /// Deletes specified board.
        /// </summary>
        /// <param name="infoObj">Board object.</param>
        public static void DeleteBoardInfo(BoardInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns message boards object query.
        /// </summary>
        public static ObjectQuery<BoardInfo> GetMessageBoards()
        {
            return ProviderObject.GetObjectQuery();
        }

        
        /// <summary>
        /// Deletes specified board.
        /// </summary>
        /// <param name="boardId">Board id.</param>
        public static void DeleteBoardInfo(int boardId)
        {
            BoardInfo infoObj = GetBoardInfo(boardId);
            DeleteBoardInfo(infoObj);
        }

        
        /// <summary>
        /// Determines whether the board is opened for the current user.
        /// </summary>
        /// <param name="boardOpen">Indicates whether the board is opened according settings.</param>
        /// <param name="openFrom">Date-time value of board opening.</param>
        /// <param name="openTo">Date-time value of board closing.</param>
        public static bool IsBoardOpened(bool boardOpen, DateTime openFrom, DateTime openTo)
        {
            // Check if default flag is set            
            if (!boardOpen)
            {
                return false;
            }

            // Get current date-time
            DateTime now = TimeZoneHelper.GetUserDateTime(MembershipContext.AuthenticatedUser);

            // Compare user's time with board open time
            if (((now >= openFrom) || (openFrom == DateTimeHelper.ZERO_TIME)) &&
                ((openTo >= now) || (openTo == DateTimeHelper.ZERO_TIME)))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets unsubscription page URL.
        /// </summary>
        /// <param name="boardUnsubscriptionUrl">Board unsubscription URL.</param>
        /// <param name="siteName">Site name.</param>
        public static string GetUnsubscriptionUrl(string boardUnsubscriptionUrl, string siteName)
        {
            // Get board unsubscribe URL
            boardUnsubscriptionUrl = (string.IsNullOrEmpty(boardUnsubscriptionUrl)) ? ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName + ".CMSBoardUnsubsriptionURL"), "") : boardUnsubscriptionUrl;

            // No URL set, use default unsubscribe page
            if (string.IsNullOrEmpty(boardUnsubscriptionUrl))
            {
                boardUnsubscriptionUrl = "~/CMSModules/MessageBoards/CMSPages/Unsubscribe.aspx";
            }
            return boardUnsubscriptionUrl;
        }


        /// <summary>
        /// Returns True if current user is authorized to add messages to the specified message board, otherwise returns False.
        /// </summary>
        /// <param name="boardProperties">Properties of the board.</param>
        public static bool IsUserAuthorizedToAddMessages(BoardProperties boardProperties)
        {
            // Check if the board is opened first
            bool isOpened = IsBoardOpened(boardProperties.BoardOpened, boardProperties.BoardOpenedFrom, boardProperties.BoardOpenedTo);
            if (isOpened)
            {
                bool result = false;
                CurrentUserInfo cu = MembershipContext.AuthenticatedUser;

                // If there is any reason to perform further check
                // Check if the current user apply to any of the following
                switch (boardProperties.BoardAccess)
                {
                    case SecurityAccessEnum.AllUsers:
                        // No further check is required unless user board is processed
                        result = true;
                        break;

                    case SecurityAccessEnum.Owner:
                        // Owner is available only for user board
                        if (boardProperties.BoardOwner == "user")
                        {
                            var currentUserProfile = MembershipContext.CurrentUserProfile;

                            result = (currentUserProfile != null && (cu.UserID == currentUserProfile.UserID));
                        }
                        break;

                    // Only authenticated can do so
                    case SecurityAccessEnum.AuthenticatedUsers:
                        result = AuthenticationHelper.IsAuthenticated();
                        break;

                    // Only users from specified roles can add
                    case SecurityAccessEnum.AuthorizedRoles:
                        // Check if the user belongs to the default role                        
                        string[] roles = boardProperties.BoardRoles.Split(';');
                        foreach (string role in roles)
                        {
                            if (cu.IsInRole(role, SiteContext.CurrentSiteName))
                            {
                                result = true;
                                break;
                            }
                        }
                        break;

                    // Only group members are allowed to add messages
                    case SecurityAccessEnum.GroupMembers:
                        int groupId = ModuleCommands.CommunityGetCurrentGroupID();
                        if (groupId > 0)
                        {
                            result = cu.IsGroupMember(groupId);
                        }
                        break;

                    case SecurityAccessEnum.GroupAdmin:
                        groupId = ModuleCommands.CommunityGetCurrentGroupID();
                        if (groupId > 0)
                        {
                            result = cu.IsGroupAdministrator(groupId);
                        }
                        break;
                }

                return result || cu.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
            }

            return false;
        }


        /// <summary>
        /// Returns True if current user is authorized to add messages to the specified message board, otherwise returns False.
        /// </summary>
        /// <param name="board">Board information.</param>
        public static bool IsUserAuthorizedToAddMessages(BoardInfo board)
        {
            // Check if the board is opened first
            bool isOpened = IsBoardOpened(board.BoardOpened, board.BoardOpenedFrom, board.BoardOpenedTo);
            if (!isOpened)
            {
                return false;
            }

            bool result = false;
            CurrentUserInfo cu = MembershipContext.AuthenticatedUser;

            // If there is any reason to perform further check
            // Check if the current user apply to any of the following
            switch (board.BoardAccess)
            {
                // Nobody, not even global admin
                case SecurityAccessEnum.Nobody:
                    return false;

                case SecurityAccessEnum.AllUsers:
                    // No further check is required unless user board is processed
                    result = true;
                    break;

                case SecurityAccessEnum.Owner:
                    // Owner is available only for user board
                    if (board.BoardUserID > 0)
                    {
                        var currentUserProfile = MembershipContext.CurrentUserProfile;

                        result = (currentUserProfile != null) && (cu.UserID == currentUserProfile.UserID) && (cu.UserID == board.BoardUserID);
                    }
                    break;

                // Only authenticated can do so
                case SecurityAccessEnum.AuthenticatedUsers:
                    result = AuthenticationHelper.IsAuthenticated();
                    break;

                // Only users from specified roles can add
                case SecurityAccessEnum.AuthorizedRoles:
                    result = IsUserAuthorizedPerBoard(cu, board.BoardID);
                    break;

                // Only group members are allowed to add messages
                case SecurityAccessEnum.GroupMembers:
                    result = cu.IsGroupMember(board.BoardGroupID);
                    break;

                // Group administrator
                case SecurityAccessEnum.GroupAdmin:
                    result = cu.IsGroupAdministrator(board.BoardGroupID);
                    break;
            }

            return result || cu.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
        }


        /// <summary>
        /// Checks if the user is authorized per board (if she belongs to the role authorized per board).
        /// </summary>
        /// <param name="userInfo">User info.</param>     
        /// <param name="boardId">ID of the board to check.</param>
        public static bool IsUserAuthorizedPerBoard(UserInfo userInfo, int boardId)
        {
            return ProviderObject.IsUserAuthorizedPerBoardInternal(userInfo, boardId);
        }


        /// <summary>
        /// Checks if the current user can manage messages of the specified board.
        /// </summary>
        /// <param name="board">Board object to check.</param>
        public static bool IsUserAuthorizedToManageMessages(BoardInfo board)
        {
            if (board == null)
            {
                return false;
            }

            CurrentUserInfo cui = MembershipContext.AuthenticatedUser;
            if (cui == null)
            {
                return false;
            }

            if (cui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            // Check if the current user is owner of user's blog
            bool isOwnerOrAdmin = false;
            if (board.BoardUserID > 0)
            {
                isOwnerOrAdmin = (board.BoardUserID == cui.UserID);
            }

            //  Check if the current user is admin of group's blog
            if (board.BoardGroupID > 0)
            {
                isOwnerOrAdmin = cui.IsGroupAdministrator(board.BoardGroupID);
            }

            // Check if the user is board moderator- if necessary
            bool isModerator = false;
            if (!isOwnerOrAdmin && board.BoardModerated)
            {
                isModerator = BoardModeratorInfoProvider.IsUserBoardModerator(cui.UserID, board.BoardID);
            }

            bool canModify = board.BoardGroupID > 0
                ? cui.IsAuthorizedPerResource("cms.groups", "Manage")
                : cui.IsAuthorizedPerResource("CMS.MessageBoards", "Modify");

            return (isOwnerOrAdmin || isModerator || canModify);
        }


        /// <summary>
        /// Returns all message boards matching specified criteria with associated document data.
        /// </summary>
        /// <param name="where">WHERE condition.</param>
        /// <param name="orderBy">ORDER BY expression.</param>
        /// <param name="topN">Top N condition.</param>
        /// <param name="columns">Columns to be selected.</param>
        /// <param name="documentData">Indicates if document data for each message board should be included in the result dataset.</param>
        public static TypedDataSet GetMessageBoards(string where, string orderBy, int topN = 0, string columns = null, bool documentData = false)
        {
            return ProviderObject.GetMessageBoardsInternal(where, orderBy, topN, columns, documentData);
        }


        /// <summary>
        /// Returns the name for messageboard according to its type, webpart name, and identifier dependent on type.
        /// </summary>
        /// <param name="boardWebpartName">Name of the webpart the board was created with.</param>
        /// <param name="type">Type of the message board.</param>
        /// <param name="identifier">User GUID in case of user message board type or Group GUID in case of user message board type.</param>
        public static string GetMessageBoardName(string boardWebpartName, BoardOwnerTypeEnum type, string identifier)
        {
            return ProviderObject.GetMessageBoardNameInternal(boardWebpartName, type, identifier);
        }


        /// <summary>
        /// Returns absolute URL to the document where the message board is placed on.
        /// </summary>        
        public static string GetMessageBoardUrl(BoardInfo board, string siteName)
        {
            return ProviderObject.GetMessageBoardUrlInternal(board, siteName);
        }


        /// <summary>
        /// Gets double opt-in interval for the site. It's time interval in hours within which user has to approve her subscription.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static int DoubleOptInInterval(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSBoardOptInInterval");
        }


        /// <summary>
        /// Gets setting value indicating if double opt-in confirmation emails should be sent.
        /// </summary>
        public static bool SendOptInConfirmation(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSBoardEnableOptInConfirmation");
        }


        /// <summary>
        /// Gets setting value indicating if double opt-in should be enabled.
        /// </summary>
        public static bool EnableDoubleOptIn(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSBoardEnableOptIn");
        }


        /// <summary>
        /// Returns BoardOwnerType specified by string.
        /// </summary>
        /// <param name="enumStr">'user', 'group' or 'document' (default) string to be converted.</param>
        public static BoardOwnerTypeEnum GetBoardOwnerTypeEnum(string enumStr)
        {
            switch (enumStr.ToLowerInvariant())
            {
                case "user":
                    return BoardOwnerTypeEnum.User;

                case "group":
                    return BoardOwnerTypeEnum.Group;

                default:
                    return BoardOwnerTypeEnum.Document;
            }
        }


        /// <summary>
        /// Returns 'USER', 'GROUP' or 'DOCUMENT' (default) string according to given owner type.
        /// </summary>
        /// <param name="boardOwnerTypeEnum">BoardOwnerType to be converted.</param>
        public static string GetBoardOwnerTypeEnumString(BoardOwnerTypeEnum boardOwnerTypeEnum)
        {
            switch (boardOwnerTypeEnum)
            {
                case BoardOwnerTypeEnum.User:
                    return "USER";

                case BoardOwnerTypeEnum.Group:
                    return "GROUP";

                default:
                    return "DOCUMENT";
            }
        }

        #endregion

        #region "Internal methods"

        /// <summary>
        /// Returns the BoardInfo structure for the specified board.
        /// </summary>
        /// <param name="boardName">Board code name.</param>
        /// <param name="documentId">Document ID.</param>
        protected virtual BoardInfo GetBoardInfoInternal(string boardName, int documentId)
        {
            return ProviderObject.GetInfoByFullName(GetBoardFullName(boardName, documentId, 0, 0));
        }


        /// <summary>
        /// Returns the number of message boards related to the specified document.
        /// </summary>
        /// <param name="documentId">Document ID to return number of boards for.</param> 
        protected virtual int GetBoardsCountInternal(int documentId)
        {
            if (documentId <= 0)
            {
                return 0;
            }

            return GetMessageBoards()
                            .WhereEquals("BoardDocumentID", documentId)
                            .Count;
        }


        /// <summary>
        /// Returns the info on board matching specified criteria.
        /// </summary>
        /// <param name="boardName">Name of the board.</param>
        /// <param name="userId">ID of the user board belongs to.</param>
        protected virtual BoardInfo GetBoardInfoForUserInternal(string boardName, int userId)
        {
            if (userId <= 0 || String.IsNullOrEmpty(boardName))
            {
                return null;
            }

            return ProviderObject.GetInfoByFullName(GetBoardFullName(boardName, 0, userId, 0));
        }


        /// <summary>
        /// Returns the info on board matching specified criteria.
        /// </summary>
        /// <param name="boardName">Name of the board.</param>
        /// <param name="groupId">ID of the group board belongs to.</param>
        protected virtual BoardInfo GetBoardInfoForGroupInternal(string boardName, int groupId)
        {
            if (groupId <= 0 || String.IsNullOrEmpty(boardName))
            {
                return null;
            }

            return ProviderObject.GetInfoByFullName(GetBoardFullName(boardName, 0, 0, groupId));
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete.</param>
        protected override void DeleteInfo(BoardInfo info)
        {
            if (info == null)
            {
                return;
            }

            base.DeleteInfo(info);

            // Update user counts
            UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.MessageBoardPost, 0, 0);

            // Update search index
            if (!SearchIndexInfoProvider.SearchEnabled)
            {
                return;
            }

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode node = tree.SelectSingleDocument(info.BoardDocumentID);

            // Update search index for given document
            if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
            }
        }


        /// <summary>
        /// Checks if the user is authorized per board (if she belongs to the role authorized per board).
        /// </summary>
        /// <param name="userInfo">User info.</param>     
        /// <param name="boardId">ID of the board to check.</param>
        protected virtual bool IsUserAuthorizedPerBoardInternal(UserInfo userInfo, int boardId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", userInfo.UserID);
            parameters.Add("@ValidTo", DateTime.Now);

            // Get user generic roles
            var genRoleWhere = UserInfoProvider.GetGenericRoles(userInfo);

            parameters.AddMacro("##GENERICROLES##", genRoleWhere);

            string where = null;
            if (boardId > 0)
            {
                where = "BoardID=" + boardId;
            }

            // Look for user role belonging to the roles group authorized per board
            DataSet ds = ConnectionHelper.ExecuteQuery("board.boardrole.isuserauthorized", parameters, where);

            return (!DataHelper.DataSourceIsEmpty(ds));
        }


        /// <summary>
        /// Returns all message boards matching specified criteria with associated document data.
        /// </summary>
        /// <param name="where">WHERE condition.</param>
        /// <param name="orderBy">ORDER BY expression.</param>
        /// <param name="topN">Top N condition.</param>
        /// <param name="columns">Columns to be selected.</param>
        /// <param name="documentData">Indicates if document data for each message board should be included in the result dataset.</param>
        protected virtual TypedDataSet GetMessageBoardsInternal(string where, string orderBy, int topN, string columns, bool documentData)
        {
            if (documentData)
            {
                var parameters = new QueryDataParameters();
                parameters.EnsureDataSet<BoardInfo>();

                return ConnectionHelper.ExecuteQuery("board.board.getboardlist", parameters, where, orderBy, topN, columns).As<BoardInfo>();
            }

            return GetObjectQuery().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns absolute URL to the document where the message board is placed on.
        /// <param name="board">Message board object to get document URL of.</param>
        /// <param name="siteName">Site name of the message board.</param>
        /// </summary>        
        protected virtual string GetMessageBoardUrlInternal(BoardInfo board, string siteName)
        {
            if (board == null)
            {
                return String.Empty;
            }

            // Get base URL
            string documentUrl = DataHelper.GetNotEmpty(board.BoardBaseURL, SettingsKeyInfoProvider.GetValue(siteName + ".CMSBoardBaseURL"));

            // Base URL not defined, get document URL
            if (string.IsNullOrEmpty(documentUrl))
            {
                TreeProvider tp = new TreeProvider();
                TreeNode documentNode = tp.SelectSingleDocument(board.BoardDocumentID);
                if (documentNode != null)
                {
                    documentUrl = DocumentURLProvider.GetUrl(documentNode);
                }
            }

            string param = string.Empty;

            // Add user name as query parameter if user is defined
            if (board.BoardUserID > 0)
            {
                UserInfo ui = UserInfoProvider.GetUserInfo(board.BoardUserID);
                if ((ui != null) && (!string.IsNullOrEmpty(ui.UserName)))
                {
                    param += "?username=" + HTMLHelper.HTMLEncode(ui.UserName);
                }
            }

            // Add group name as query parameter if group is defined
            if (board.BoardGroupID > 0)
            {
                if (!ModuleEntryManager.IsModuleLoaded(ModuleName.COMMUNITY))
                {
                    return URLHelper.GetAbsoluteUrl(documentUrl + param);
                }

                BaseInfo gi = ModuleCommands.CommunityGetGroupInfo(board.BoardGroupID);
                if (gi == null)
                {
                    return URLHelper.GetAbsoluteUrl(documentUrl + param);
                }

                string groupname = ValidationHelper.GetString(gi.GetValue("GroupName"), null);
                if (!string.IsNullOrEmpty(groupname))
                {
                    param = URLHelper.AddUrlParameter(param, "groupname", groupname);
                }
            }

            return URLHelper.GetAbsoluteUrl(documentUrl + param);
        }


        /// <summary>
        /// Returns the name for messageboard according to its type, webpart name, and identifier dependent on type.
        /// </summary>
        /// <param name="boardWebpartName">Name of the webpart the board was created with.</param>
        /// <param name="type">Type of the message board.</param>
        /// <param name="identifier">User GUID in case of user message board type or Group GUID in case of user message board type.</param>
        protected virtual string GetMessageBoardNameInternal(string boardWebpartName, BoardOwnerTypeEnum type, string identifier)
        {
            switch (type)
            {
                case BoardOwnerTypeEnum.User:
                    return boardWebpartName + "_user_" + identifier;

                case BoardOwnerTypeEnum.Group:
                    return boardWebpartName + "_group_" + identifier;

                default:
                    return boardWebpartName;
            }
        }


        /// <summary>
        /// Updates data for all records given by where condition.
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2."</param>
        /// <param name="where">Where condition.</param>
        /// <param name="parameters">Parameters.</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion

        #region "Full name methods"

        /// <summary>
        /// Creates a new dictionary for caching the objects by the full name.
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(BoardInfo.OBJECT_TYPE, "BoardName;BoardDocumentID;BoardUserID;BoardGroupID");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name.
        /// </summary>
        /// <param name="fullName">Board full name.</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string boardName;
            string type;

            if (!ObjectHelper.ParseFullName(fullName, out boardName, out type))
            {
                return null;
            }

            var parts = type.Split('|');
            if (parts.Length != 2)
            {
                return null;
            }

            var columnName = parts[0];
            var objectId = ValidationHelper.GetInteger(parts[1], 0);

            var where = new WhereCondition()
                .WhereEquals("BoardName", boardName)
                .WhereEquals(columnName, objectId);

            return where.ToString(true);
        }


        /// <summary>
        /// Gets board full name to be used within a hashtable.
        /// </summary>
        /// <param name="boardName">Board name.</param>
        /// <param name="documentId">Board document ID, if board represents a document ad-hoc board.</param>
        /// <param name="userId">Board user ID, if board represents user board.</param>
        /// <param name="groupId">Board group ID, if board represents group board.</param>
        internal static string GetBoardFullName(string boardName, int documentId, int userId, int groupId)
        {
            string suffix;
            if (userId > 0)
            {
                suffix = $"BoardUserID|{userId}";
            }
            else if (groupId > 0)
            {
                suffix = $"BoardGroupID|{groupId}";
            }
            else
            {
                suffix = $"BoardDocumentID|{documentId}";
            }

            return ObjectHelper.BuildFullName(boardName, suffix);
        }

        #endregion
    }
}
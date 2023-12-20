using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class specific for chat user.
    /// </summary>
    public static class ChatUserHelper
    {
        #region "Public methods"

        #region "Validation"

        /// <summary>
        /// Returns true if nickname is available. Otherwise false. Takes into account <see cref="ChatSettingsProvider.ForceAnonymUniqueNicknamesSetting"/>.
        /// </summary>
        /// <param name="nickname">Required nickname</param>
        /// <returns>True if nickname is available; otherwise false</returns>
        public static bool IsNicknameAvailable(string nickname)
        {
            return IsNicknameAvailable(nickname, ChatSettingsProvider.ForceAnonymUniqueNicknamesSetting);
        }


        /// <summary>
        /// Returns true if nickname is available. Otherwise false.
        /// 
        /// Current user is not counted.
        /// </summary>
        /// <param name="nickname">Required nickname</param>
        /// <param name="includeAnonymousUsers">If true, also ONLINE anonymous users are checked for duplications. Otherwise only CMS Users.</param>
        /// <returns>True if nickname is available; otherwise false</returns>
        public static bool IsNicknameAvailable(string nickname, bool includeAnonymousUsers)
        {
            if (nickname.Length == 0)
            {
                return false;
            }

            ChatUserInfo loggedInChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

            int chatUsersCount = ChatUserInfoProvider.GetCountOfChatUsers(nickname, includeAnonymousUsers, loggedInChatUser?.ChatUserID);

            return chatUsersCount == 0;
        }


        /// <summary>
        /// Checks if nickname is valid. Nickname is also trimmed.
        /// 
        /// Nickname cannot be empty
        /// Nickname cannot exceed 50 characters
        /// User can't manually change his nickname to guest nickname
        /// 
        /// Throws exception if not valid
        /// </summary>
        /// <param name="nickname">Nickname to check</param>
        public static void VerifyNicknameIsValid(ref string nickname)
        {
            if (nickname == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NicknameCantBeEmpty);
            }

            nickname = nickname.Trim();

            // Nickname cannot be empty
            // Nickname cannot exceed 50 characters
            // User can't manually change his nickname to guest nickname
            if (nickname.Length == 0)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NicknameCantBeEmpty);
            }

            if (nickname.Length > 50)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NicknameTooLong, string.Format(ChatResponseStatusEnum.NicknameTooLong.ToStringValue(), 50));
            }

            if (nickname.StartsWithCSafe(ChatSettingsProvider.GuestPrefixSetting, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NicknameCantBeginWith, string.Format(ChatResponseStatusEnum.NicknameCantBeginWith.ToStringValue(), ChatSettingsProvider.GuestPrefixSetting));
            }
        }

        #endregion


        #region "Registration and login"

        /// <summary>
        /// Registers guest chat user. His nickname is generated automatically. If CMS User is logged in, guest is not registered and this CMS User is logged in.
        /// </summary>
        /// <returns>New chat user</returns>
        public static void RegisterAndLoginChatUser()
        {
            RegisterAndLoginChatUser(false);
        }


        /// <summary>
        /// Registers guest chat user. His nickname is generated automatically. If CMS User is logged in, guest is not registered and this CMS User is logged in.
        /// </summary>
        /// <param name="isHidden">If false, this user will be shown in online users on live site. If user was logged in as hidden before and now is logged in as 'not hidden' value will be overridden.</param>
        /// <returns>New chat user</returns>
        public static void RegisterAndLoginChatUser(bool isHidden)
        {
            bool isCurrentUserHidden;

            ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser(out isCurrentUserHidden);

            // Do nothing if some chat user is logged in now and is not hidden (or is hidden but the new user will also be hidden).
            if ((currentChatUser == null) || (isCurrentUserHidden && !isHidden))
            {
                ChatUserInfo chatUser;

                UserInfo cmsUser = MembershipContext.AuthenticatedUser;

                if (cmsUser.IsPublic())
                {
                    // If a guest is already logged in - do nothing (just change his Hidden state if needed - it is handled by LogInChatUser method).
                    if (currentChatUser != null)
                    {
                        chatUser = currentChatUser;
                    }
                    // If guest is not logged in, create a new one
                    else
                    {
                        // First, register anonymous user with nickname set to new Guid
                        chatUser = RegisterAnonymousChatUser(Guid.NewGuid().ToString());

                        string guestPrefixSetting = ChatSettingsProvider.GuestPrefixSetting;

                        // Then change his nickname to <guest_prefix>_<users_id>
                        chatUser.ChatUserNickname = guestPrefixSetting + chatUser.ChatUserID.ToString();

                        ChatUserInfoProvider.SetChatUserInfo(chatUser);
                    }
                }
                else
                {
                    chatUser = ChatUserHelper.GetChatUserFromCMSUser(cmsUser);
                }

                ChatOnlineUserHelper.LogInChatUser(chatUser, isHidden);
            }
        }
        
        
        /// <summary>
        /// If CMS User is currently logged in, it logs him into chat. Parameter <paramref name="nickname"/> is ignored in that case.
        /// 
        /// If CMS User is not logged in, user is registered as anonymous chat user with nickname set to parameter <paramref name="nickname"/>.
        /// 
        /// Does nothing if chat user is currently logged in.
        /// </summary>
        /// <param name="nickname">If user is not logged in, he will be registered as anonymous user with this nickname.</param>
        public static void RegisterAndLoginChatUser(string nickname)
        {
            RegisterAndLoginChatUser(nickname, false);
        }


        /// <summary>
        /// If CMS User is currently logged in, it logs him into chat. Parameter <paramref name="nickname"/> is ignored in that case.
        /// 
        /// If CMS User is not logged in, user is registered as anonymous chat user with nickname set to parameter <paramref name="nickname"/>.
        /// 
        /// Does nothing if chat user is currently logged in.
        /// </summary>
        /// <param name="nickname">If user is not logged in, he will be registered as anonymous user with this nickname.</param>
        /// <param name="isHidden">If false, this user will be shown in online users on live site. If user was logged in as hidden before and now is logged in as 'not hidden' value will be overridden.</param>
        public static void RegisterAndLoginChatUser(string nickname, bool isHidden)
        {
            // Do nothing if user is logged in right now
            if (!ChatOnlineUserHelper.IsChatUserLoggedIn(true))
            {
                ChatUserInfo chatUser;
                CurrentUserInfo cmsUser = MembershipContext.AuthenticatedUser;

                if (cmsUser.IsPublic())
                {
                    chatUser = RegisterAnonymousChatUser(nickname);
                }
                else
                {
                    chatUser = GetChatUserFromCMSUser(cmsUser);
                }

                ChatOnlineUserHelper.LogInChatUser(chatUser, isHidden);
            }
        }


        /// <summary>
        /// Registers anonymous chat user with specified nickname.
        /// </summary>
        /// <param name="nickname">Nickname of chat user</param>
        /// <returns>New chat user</returns>
        public static ChatUserInfo RegisterAnonymousChatUser(string nickname)
        {
            VerifyNicknameIsValid(ref nickname);

            UserInfo cmsUser = MembershipContext.AuthenticatedUser;


            if (cmsUser.IsPublic() && !ChatSettingsProvider.AreAnonymsAllowedGloballySetting)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AnonymsDisallowedGlobally);
            }

            if (!IsNicknameAvailable(nickname))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NicknameNotAvailable);
            }

            ChatUserInfo chatUser = new ChatUserInfo();

            if (!cmsUser.IsPublic())
            {
                chatUser.ChatUserUserID = cmsUser.UserID;
            }
            chatUser.ChatUserNickname = nickname;

            ChatUserInfoProvider.SetChatUserInfo(chatUser);

            return chatUser;
        }

        #endregion


        #region "ChatUser rights"


        /// <summary>
        /// Verifies that currently logged in chat user has join rights for a room.
        /// 
        /// Throws exception with AccessDenied (or Kicked) type and message set to the cause of the denial in case of failure and does nothing in case of success.
        /// 
        /// Join rights:
        /// 
        ///  - user can't be kicked
        ///  - if room is public, user always has Join rights
        ///  - if he is admin, he also has join rights
        ///  - supporters always have Join rights in support rooms 
        /// </summary>
        /// <param name="roomID">Rights for this room will be checked</param>
        public static void VerifyChatUserHasJoinRoomRights(int roomID)
        {
            // Get logged in chat user (or null if he is not logged in)
            ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

            // If chat user is not logged in, he has no rights at all
            if (currentChatUser == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NotLoggedIn);
            }


            RoomState room;

            // Room wasn't found or is disabled
            if (!ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out room) || !room.RoomInfo.ChatRoomEnabled)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied, ChatResponseStatusEnum.RoomNotFound.ToStringValue());
            }

            // Check if room belongs to this site or is global
            if ((room.RoomInfo.ChatRoomSiteID != null) && (room.RoomInfo.ChatRoomSiteID.Value != SiteContext.CurrentSiteID))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            // Anonyms don't have any rights in rooms with disallowed anonymous access
            if (currentChatUser.IsAnonymous && !room.RoomInfo.ChatRoomAllowAnonym)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied, ChatResponseStatusEnum.AnonymsDisallowed.ToStringValue());
            }


            int forHowLong;

            // If user was kicked, he has no rights at all
            if (room.IsUserKicked(currentChatUser.ChatUserID, out forHowLong))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.KickedFromRoom, String.Format(ResHelper.GetString("chat.errormessage.kickedforanother"), forHowLong));
            }


            // Everybody (except anonyms) has join rights to public rooms
            if (!room.RoomInfo.ChatRoomPrivate)
            {
                return;
            }

            // In private room you must have Join or higher rights
            if (ChatGlobalData.Instance.UsersRoomAdminStates.GetAdminLevelInRoom(currentChatUser.ChatUserID, roomID) >= AdminLevelEnum.Join)
            {
                return;
            }

            // If room is whisper room and user don't have join rights (previous condintion), he can't access it. Even GlobalAdmin cant access whisper room where he is not joined.
            if (room.RoomInfo.IsWhisperRoom)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }
            
            // Global admin and Manage rooms rights
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.ManageRooms))
            {
                return;
            }

            // Supporters always have Join rights in support rooms
            if (room.RoomInfo.ChatRoomIsSupport && ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.EnterSupport))
            {
                return;
            }

            // If method has not returned before this line, user don't have sufficient rights
            throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
        }


        /// <summary>
        /// Check if current user has admin rights to room.
        /// 
        /// User have admin rights if one of the following conditions is valid:
        ///  - he is global admin
        ///  - he has permission ChatPermissionEnum.ManageRooms
        ///  - he is creator of this room
        ///  - he is associated as admin to this room in table ChatRoomAdmin
        ///  - room is support (ChatRoomIsSupport = true) and user has permission ChatPermissionEnum.EnterSupport
        /// </summary>
        /// <param name="roomID">Checks rights for this room</param>
        /// <returns>True if current user has admin rights to the specified room</returns>
        public static bool CheckAdminRoomRights(int roomID)
        {
            try
            {
                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                if (currentChatUser == null)
                {
                    return false;
                }


                var currentUser = MembershipContext.AuthenticatedUser;

                RoomState room;

                if (!ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out room))
                {
                    return false;
                }

                // Check if room belongs to this site or is global
                if ((room.RoomInfo.ChatRoomSiteID != null) && (room.RoomInfo.ChatRoomSiteID.Value != SiteContext.CurrentSiteID))
                {
                    return false;
                }

                // Nobody has admin rights in one to one not-support rooms - whisper rooms (you can't invite user to one to one, kick, etc.).
                if (room.RoomInfo.IsWhisperRoom)
                {
                    return false;
                }

                if (currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.ManageRooms))
                {
                    return true;
                }

                if (ChatGlobalData.Instance.UsersRoomAdminStates.IsUserAdmin(currentChatUser.ChatUserID, roomID))
                {
                    return true;
                }

                if (room.RoomInfo.ChatRoomIsSupport && ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.EnterSupport))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user has admin rights to room.
        /// 
        /// User have admin rights if one of the following conditions is valid:
        ///  - he is global admin
        ///  - he has permission ChatPermissionEnum.ManageRooms
        ///  - he is creator of this room
        ///  - he is associated as admin to this room in table ChatRoomAdmin
        ///  - room is support (ChatRoomIsSupport = true) and user has permission ChatPermissionEnum.EnterSupport
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUser">Chat user ID</param>
        /// <returns>True if current user has admin rights to the specified room</returns>
        public static bool CheckAdminRoomRights(int roomID, ChatUserInfo chatUser)
        {
            try
            {
                if (chatUser == null)
                {
                    return false;
                }

                RoomState room;

                if (!ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out room))
                {
                    return false;
                }

                // Check if room belongs to this site or is global
                if ((room.RoomInfo.ChatRoomSiteID != null) && (room.RoomInfo.ChatRoomSiteID.Value != SiteContext.CurrentSiteID))
                {
                    return false;
                }

                // Nobody has admin rights in one to one not-support rooms - whisper rooms (you can't invite user to one to one, kick, etc.).
                if (room.RoomInfo.IsWhisperRoom)
                {
                    return false;
                }

                if (ChatGlobalData.Instance.UsersRoomAdminStates.IsUserAdmin(chatUser.ChatUserID, roomID))
                {
                    return true;
                }

                if (!chatUser.IsAnonymous)
                {
                    UserInfo user = UserInfoProvider.GetUserInfo(chatUser.ChatUserUserID.Value);

                    if (ChatProtectionHelper.HasUserPermission(user, ChatPermissionEnum.ManageRooms))
                    {
                        return true;
                    }

                    if (room.RoomInfo.ChatRoomIsSupport && ChatProtectionHelper.HasUserPermission(user, ChatPermissionEnum.EnterSupport))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Checks if current chat user has Join rights to the specified room.
        /// 
        ///  - user can't be kicked
        ///  - if room is public, user always has Join rights
        ///  - if he is admin, he also has join rights
        ///  - supporters always have Join rights in support rooms 
        /// </summary>
        /// <param name="roomID">Rights for this room will be checked</param>
        /// <returns>True if current chat user has join permissions to specified room (join permissions means, that he can enter it, etc.).</returns>
        public static bool CheckJoinRoomRights(int roomID)
        {
            try
            {
                VerifyChatUserHasJoinRoomRights(roomID);
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion


        #region "CMS Permissions"

        /// <summary>
        /// Checks if chat user has permission.
        /// </summary>
        /// <param name="chatUser">Chat user to check</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permissions; otherwise false</returns>
        public static bool HasChatUserPermission(ChatUserInfo chatUser, ChatPermissionEnum permission)
        {
            if (!chatUser.IsAnonymous)
            {
                UserInfo user = UserInfoProvider.GetUserInfo(chatUser.ChatUserUserID.Value);

                return ChatProtectionHelper.HasUserPermission(user, ChatPermissionEnum.ManageRooms);
            }
            return false;
        }

        #endregion


        #region "Other"

        /// <summary>
        /// Changes nickname of passed chatUser.
        /// </summary>
        /// <param name="chatUser">Chat user whose nickname will be changed</param>
        /// <param name="newNickname">New nickname</param>
        public static void ChangeChatUserNickname(ChatUserInfo chatUser, string newNickname)
        {
            VerifyNicknameIsValid(ref newNickname);

            if (chatUser.IsAnonymous)
            {
                if (!IsNicknameAvailable(newNickname))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.NicknameNotAvailable);
                }
            }
            else
            {
                if (!IsNicknameAvailable(newNickname, false))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.NicknameNotAvailable);
                }

                // If CMS User changes his nickname to X, change all anonym nicknames X to something else (guest_X)
                // This is needed to ensure uniqueness of CMS User's chat nicknames.
                GuestifyChatUsers(chatUser.ChatUserID, newNickname);
            }


            string oldNickname = chatUser.ChatUserNickname;

            chatUser.ChatUserNickname = newNickname;
            ChatUserInfoProvider.SetChatUserInfo(chatUser);

            InsertChangeNicknameMessages(chatUser, newNickname, oldNickname);
        }


        /// <summary>
        /// Gets chat user assigned to CURRENT CMSUser. If chat user does not exist, it is created with nickname set to FullName of CMSUser.
        /// </summary>
        /// <returns>Chat user</returns>
        public static ChatUserInfo GetChatUserFromCMSUser()
        {
            return GetChatUserFromCMSUser(MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Gets chat user assigned to passed CMSUser. If chat user does not exist, it is created with nickname set to FullName of CMSUser.
        /// </summary>
        /// <param name="user">CMS User</param>
        /// <returns>Chat user</returns>
        public static ChatUserInfo GetChatUserFromCMSUser(UserInfo user)
        {
            if (user.IsPublic())
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            ChatUserInfo chatUserCurrent = ChatUserInfoProvider.GetChatUserByUserID(user.UserID);

            if (chatUserCurrent != null)
            {
                return chatUserCurrent;
            }
            else
            {
                ChatUserInfo newChatUser = new ChatUserInfo();
                
                newChatUser.ChatUserNickname = string.IsNullOrEmpty(user.UserNickName) ? user.FullName : user.UserNickName;

                // Trim to 44 character. There are 6 characters left to have space to add counter " (XXX)" if nicknames are not unique
                if (newChatUser.ChatUserNickname.Length > 44)
                {
                    newChatUser.ChatUserNickname = newChatUser.ChatUserNickname.Substring(0, 44);
                }

                // Ensure unique nickname among non-anonymous chat users
                newChatUser.EnsureUniqueNickname();

                newChatUser.ChatUserUserID = user.UserID;

                ChatUserInfoProvider.SetChatUserInfo(newChatUser);


                // If there are any anonymous chat users with this nickname - change their nicknames
                GuestifyChatUsers(newChatUser.ChatUserID, newChatUser.ChatUserNickname);

                return newChatUser;
            }
        }

        #endregion

        #endregion


        #region "Private methods"

        /// <summary>
        /// Changes name of all chat users with nickname <paramref name="nicknameToGuestify"/> to 'guestprefix' (from settings) and his old nickname.
        /// </summary>
        /// <param name="changedByChatUserId">Chat user with this ID initiated this operation (needed to insert notification)</param>
        /// <param name="nicknameToGuestify">Anonymous users with this nickname will be guestified.</param>
        private static void GuestifyChatUsers(int changedByChatUserId, string nicknameToGuestify)
        {
            string guestPrefix = ChatSettingsProvider.GuestPrefixSetting;
            string oldNickname;

            foreach (ChatUserInfo chatUser in ChatUserInfoProvider.GetAnonymousChatUsersByNickname(nicknameToGuestify))
            {
                oldNickname = chatUser.ChatUserNickname;

                chatUser.ChatUserNickname = guestPrefix + chatUser.ChatUserNickname;
                ChatUserInfoProvider.SetChatUserInfo(chatUser);

                ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.NicknameAutomaticallyChanged, changedByChatUserId, chatUser.ChatUserID, null, null);
            }
        }


        private static void InsertChangeNicknameMessages(ChatUserInfo chatUser, string newNickname, string oldNickname)
        {
            int chatUserID = chatUser.ChatUserID;

            string systemMessageText = ChatMessageHelper.GetSystemMessageText(ChatMessageTypeEnum.ChangeNickname, oldNickname, newNickname);

            foreach (int roomID in ChatRoomUserInfoProvider.GetRoomsWhereChatUserIsOnline(chatUserID))
            {
                ChatMessageInfoProvider.SetChatMessageInfo(ChatMessageHelper.BuildNewChatMessage(systemMessageText, roomID, null, ChatMessageTypeEnum.ChangeNickname));
            }
        }

        #endregion
    }
}

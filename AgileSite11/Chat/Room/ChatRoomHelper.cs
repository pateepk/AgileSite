using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class. Contains methods working only with chat rooms.
    /// </summary>
    public static class ChatRoomHelper
    {
        #region "Public methods"

        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo.
        /// 
        /// Property IsRemoved is set based on:
        /// - if room is disabled, IsRemoved is true
        /// - if user is anonymous and room does not allow anonymous users, IsRemoved is set to true
        /// - otherwise it is set to false
        /// </summary>
        /// <param name="roomState">Room state</param>
        /// <param name="isUserAnonymous">Anonymous state of current user</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToData(RoomState roomState, bool isUserAnonymous)
        {
            return ConvertRoomToData(roomState.RoomInfo, isUserAnonymous);
        }


        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo. User is considered as non anonymous here.
        /// 
        /// Property IsRemoved is set based on:
        /// - if room is disabled, IsRemoved is true
        /// - if user is anonymous and room does not allow anonymous users, IsRemoved is set to true
        /// - otherwise it is set to false
        /// </summary>
        /// <param name="roomState">Room state</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToData(RoomState roomState)
        {
            return ConvertRoomToData(roomState.RoomInfo);
        }


        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo. User is considered as non anonymous here.
        /// 
        /// Property IsRemoved is set based on:
        /// - if room is disabled, IsRemoved is true
        /// - if user is anonymous and room does not allow anonymous users, IsRemoved is set to true
        /// - otherwise it is set to false
        /// </summary>
        /// <param name="roomInfo">Room info</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToData(ChatRoomInfo roomInfo)
        {
            return ConvertRoomToData(roomInfo, false);
        }


        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo.
        /// 
        /// Property IsRemoved is set based on:
        /// - if room is disabled, IsRemoved is true
        /// - if user is anonymous and room does not allow anonymous users, IsRemoved is set to true
        /// - otherwise it is set to false
        /// 
        /// If room is removed, its values are not set. Only RoomID and IsRemoved = true are returned.
        /// </summary>
        /// <param name="roomInfo">Room info</param>
        /// <param name="isUserAnonymous">Anonymous state of current user</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToData(ChatRoomInfo roomInfo, bool isUserAnonymous)
        {
            bool isRemoved = isUserAnonymous && !roomInfo.ChatRoomAllowAnonym;

            return ConvertRoomToDataRemoved(roomInfo, isRemoved);
        }
        

        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo.
        /// 
        /// If room is removed, its values are not set. Only RoomID and IsRemoved = true are returned.
        /// 
        /// Returns null if roomState is null;
        /// </summary>
        /// <param name="roomState">Room state</param>
        /// <param name="userAdminLevel">If admin level is None and room is private, the room will be returned with IsRemoved = true</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToDataAdminLevel(RoomState roomState, AdminLevelEnum userAdminLevel)
        {
            if (roomState == null)
            {
                return null;
            }

            bool isRemoved = roomState.RoomInfo.ChatRoomPrivate && (userAdminLevel == AdminLevelEnum.None);

            return ConvertRoomToDataRemoved(roomState.RoomInfo, isRemoved);
        }


        /// <summary>
        /// Makes ChatRoomData from information contained in ChatRoomInfo.
        /// 
        /// If room is removed, its values are not set. Only RoomID and IsRemoved = true are returned.
        /// </summary>
        /// <param name="roomInfo">Room info</param>
        /// <param name="isRemoved">If true room will be returned with IsRemoved = true</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData ConvertRoomToDataRemoved(ChatRoomInfo roomInfo, bool isRemoved)
        {
            if (roomInfo == null)
            {
                return null;
            }

            isRemoved = isRemoved || !roomInfo.ChatRoomEnabled;

            if (isRemoved)
            {
                return new ChatRoomData()
                {
                    ChatRoomID = roomInfo.ChatRoomID,
                    IsRemoved = true,
                };
            }
            else
            {
                return new ChatRoomData()
                {
                    AllowAnonym = roomInfo.ChatRoomAllowAnonym,
                    CanManage = ChatUserHelper.CheckAdminRoomRights(roomInfo.ChatRoomID),
                    Description = roomInfo.ChatRoomDescription,
                    DisplayName = roomInfo.ChatRoomDisplayName,
                    HasPassword = roomInfo.HasPassword,
                    ChatRoomID = roomInfo.ChatRoomID,
                    IsPrivate = roomInfo.ChatRoomPrivate,
                    IsRemoved = false,
                };
            }
        }


        /// <summary>
        /// Creates one-to-one chat room and inserts it into the DB. If room already exists, the old one is returned.
        /// 
        /// Invitation is send to secondChatUser and both users get 'Join' rights to this room. 
        /// </summary>
        /// <param name="currentChatUser">Current chat user</param>
        /// <param name="secondChatUser">Chat user to whisper with</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData CreateOneToOneChatRoom(ChatUserInfo currentChatUser, ChatUserInfo secondChatUser)
        {
            string adhocRoomCodeName = ChatRoomHelper.MakeOneToOneCodeName(currentChatUser.ChatUserID, secondChatUser.ChatUserID);

            ChatRoomInfo oneToOneChatRoom = ChatRoomInfoProvider.GetOneToOneChatRoomInfo(adhocRoomCodeName);

            // If chat room doesn't exist, create a new one
            if (oneToOneChatRoom == null)
            {
                oneToOneChatRoom = ChatRoomHelper.BuildOneToOneRoom(currentChatUser, secondChatUser);

                ChatRoomInfoProvider.SetChatRoomInfo(oneToOneChatRoom);

                ChatRoomUserHelper.SetChatAdminLevel(oneToOneChatRoom.ChatRoomID, currentChatUser.ChatUserID, AdminLevelEnum.Join);
                ChatRoomUserHelper.SetChatAdminLevel(oneToOneChatRoom.ChatRoomID, secondChatUser.ChatUserID, AdminLevelEnum.Join);
            }

            ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.Invitation, currentChatUser.ChatUserID, secondChatUser.ChatUserID, oneToOneChatRoom.ChatRoomID, null);

            return ConvertRoomToData(oneToOneChatRoom);
        }


        /// <summary>
        /// Creates classic (not one-to-one and not support) chat room.
        /// </summary>
        /// <param name="currentChatUser">Current chat user</param>
        /// <param name="displayName">Display name</param>
        /// <param name="isPrivate">Is private</param>
        /// <param name="password">Password</param>
        /// <param name="allowAnonym">Allow anonym</param>
        /// <param name="description">Description of the chat room</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData CreateClassicChatRoom(ChatUserInfo currentChatUser, string displayName, bool isPrivate, string password, bool allowAnonym, string description)
        {
            VerifyChatRoomNameIsValid(ref displayName);
            VerifyChatRoomPasswordIsValid(ref password);
            VerifyChatRoomDescriptionIsValid(ref description);


            string codeName = string.Format("chatroom_{0}_{1}", currentChatUser.ChatUserID, Guid.NewGuid().ToString());

            ChatRoomInfo newChatRoom = ChatRoomHelper.BuildChatRoom(displayName, codeName, isPrivate, allowAnonym, currentChatUser.ChatUserID, password, false, false, description);

            // Hash password
            newChatRoom.ChatRoomPassword = ChatRoomHelper.GetRoomPasswordHash(password, newChatRoom.ChatRoomGUID);

            ChatRoomInfoProvider.SetChatRoomInfo(newChatRoom);

            ChatRoomUserHelper.SetChatAdminLevel(newChatRoom.ChatRoomID, currentChatUser.ChatUserID, AdminLevelEnum.Creator);

            return ConvertRoomToData(newChatRoom);
        }


        /// <summary>
        /// Creates support room. Support room is private, marked as IsSupport, so new messages will be send to online support engineers.
        /// 
        /// Support greeting system message is automatically inserted (the default one) if the room is newly created.
        /// </summary>
        /// <param name="currentChatUser">Current chat user. This parameter is needed to find chat room (if this user has been chatting before).</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData CreateSupportChatRoom(ChatUserInfo currentChatUser)
        {
            string displayName = string.Format("{0}", currentChatUser.ChatUserNickname);
            string codeName = string.Format("support_{0}_{1}", currentChatUser.ChatUserID, SiteContext.CurrentSiteID);

            ChatRoomInfo supportChatRoom = ChatRoomInfoProvider.GetSupportChatRoomInfo(codeName);

            // If chat room doesn't exist, create a new one
            if (supportChatRoom == null)
            {
                supportChatRoom = ChatRoomHelper.BuildChatRoom(displayName, codeName, true, true, currentChatUser.ChatUserID, "", true, true, String.Empty);

                ChatRoomInfoProvider.SetChatRoomInfo(supportChatRoom);

                // Set permissions to the creator of this room, so he can join it
                ChatRoomUserHelper.SetChatAdminLevel(supportChatRoom.ChatRoomID, currentChatUser.ChatUserID, AdminLevelEnum.Join);

                ChatMessageHelper.InsertSystemMessage(supportChatRoom.ChatRoomID, ChatMessageTypeEnum.SupportGreeting, currentChatUser.ChatUserNickname);
            }

            return ConvertRoomToData(supportChatRoom);
        }


        /// <summary>
        /// Creates support room. Support room is private, marked as IsSupport, so new messages will be send to online support engineers.
        /// 
        /// If parameter <paramref name="greetingMessages"/> is null, the default support greeting message (ChatMessageTypeEnum.SupportGreeting) will be inserted. But only if the room did not exist before.
        /// </summary>
        /// <param name="currentChatUser">Current chat user. This parameter is needed to find chat room (if this user has been chatting before).</param>
        /// <param name="greetingMessages">Messages which will be inserted to the room as a greeting. They will be inserted even if room was already created.</param>
        /// <returns>ChatRoomData</returns>
        public static ChatRoomData CreateSupportChatRoomManual(ChatUserInfo currentChatUser, IEnumerable<string> greetingMessages)
        {
            string displayName = Guid.NewGuid().ToString();
            string codeName = string.Format("autoinitiatedchat_{0}", Guid.NewGuid().ToString());

            ChatRoomInfo autoInitiatedChatRoom = ChatRoomHelper.BuildChatRoom(displayName, codeName, true, true, currentChatUser.ChatUserID, "", true, true, String.Empty);

            // Insert room with display name set to new GUID
            autoInitiatedChatRoom.Insert();

            // Update name to pattern and ID
            autoInitiatedChatRoom.ChatRoomDisplayName = currentChatUser.ChatUserNickname;
            autoInitiatedChatRoom.Update();
            
            // Set permissions to the creator of this room, so he can join it
            ChatRoomUserHelper.SetChatAdminLevel(autoInitiatedChatRoom.ChatRoomID, currentChatUser.ChatUserID, AdminLevelEnum.Join);

            // If manual greeting messages are set, insert them to the room one by one
            if (greetingMessages != null)
            {
                foreach (string messageText in greetingMessages)
                {
                    ChatMessageInfoProvider.SetChatMessageInfo(ChatMessageHelper.BuildNewChatMessage(messageText, autoInitiatedChatRoom.ChatRoomID, null, ChatMessageTypeEnum.SupportGreeting));
                }
            }

            return ConvertRoomToData(autoInitiatedChatRoom);
        }


        /// <summary>
        /// Creates new initiated chat room and inserts it into the database.
        /// </summary>
        /// <param name="namePattern">Name of the new room. {0} will be replaced with RoomID.</param>
        /// <param name="creator">Creator of this room.</param>
        public static ChatRoomInfo CreateIntiatedChatRoom(string namePattern, ChatUserInfo creator)
        {
            string displayName = Guid.NewGuid().ToString();
            string codeName = string.Format("initiatedchat_{0}", Guid.NewGuid().ToString());

            ChatRoomInfo initiatedChatRoom = ChatRoomHelper.BuildChatRoom(displayName, codeName, true, true, creator.ChatUserID, "", true, true, String.Empty);

            // Insert room with display name set to new GUID
            initiatedChatRoom.Insert();

            // Update name to pattern and ID
            initiatedChatRoom.ChatRoomDisplayName = string.Format(namePattern, initiatedChatRoom.ChatRoomID);
            initiatedChatRoom.Update();

            return initiatedChatRoom;
        }


        /// <summary>
        /// Changes chat room.
        /// 
        /// Properly handles changing from public to private state.
        /// 
        /// Invalidates rooms cache.
        /// 
        /// Properties ChatRoomIsSupport and ChatRoomName are not changed.
        /// </summary>
        /// <param name="roomID">Room ID to change</param>
        /// <param name="displayName">New display name</param>
        /// <param name="isPrivate">New is private state</param>
        /// <param name="password">New password. If it is null, password is not changed</param>
        /// <param name="allowAnonym">New allow anonym</param>
        /// <param name="description">New description</param>
        /// <param name="hashPassword">If true, password is not hashed yet and should be hashed automatically; if false, hashing was handled by caller</param>
        /// <returns>Changed chat room</returns>
        public static ChatRoomData ChangeChatRoom(int roomID, string displayName, bool isPrivate, string password, bool allowAnonym, string description, bool hashPassword)
        {
            return ChangeChatRoom(roomID, displayName, isPrivate, password, allowAnonym, description, null, null, hashPassword);
        }


        /// <summary>
        /// Changes chat room.
        /// 
        /// Properly handles changing from public to private state.
        /// 
        /// Invalidates rooms cache.
        /// </summary>
        /// <param name="roomID">Room ID to change</param>
        /// <param name="displayName">New display name</param>
        /// <param name="isPrivate">New is private state</param>
        /// <param name="password">New password. If it is null, password is not changed</param>
        /// <param name="allowAnonym">New allow anonym</param>
        /// <param name="description">New description</param>
        /// <param name="isSupport">New is support state (is not changed if null)</param>
        /// <param name="codeName">New code name (is not changed if null)</param>
        /// <param name="hashPassword">If true, password is not hashed yet and should be hashed automatically; if false, hashing was handled by caller</param>
        /// <returns>Changed chat room</returns>
        public static ChatRoomData ChangeChatRoom(int roomID, string displayName, bool isPrivate, string password, bool allowAnonym, string description, bool? isSupport, string codeName, bool hashPassword)
        {
            VerifyChatRoomNameIsValid(ref displayName);
            VerifyChatRoomPasswordIsValid(ref password);
            VerifyChatRoomDescriptionIsValid(ref description);


            ChatRoomInfo room = ChatRoomInfoProvider.GetChatRoomInfo(roomID);

            if (room == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
            }

            
            // Get hashed password. If password is null or empty it is not changed.
            string hashedPassword = hashPassword ? ChatRoomHelper.GetRoomPasswordHash(password, room.ChatRoomGUID) : password;


            // Change chat room - handles change private state
            room.ChatRoomDisplayName = displayName;
            room.ChatRoomPrivate = isPrivate;
            room.ChatRoomPassword = hashedPassword;
            room.ChatRoomAllowAnonym = allowAnonym;
            room.ChatRoomDescription = description;
            room.ChatRoomIsSupport = isSupport ?? room.ChatRoomIsSupport;
            room.ChatRoomName = codeName ?? room.ChatRoomName;
            room.Update();  

            SiteState siteState = ChatGlobalData.Instance.Sites.Current;

            if (siteState != null)
            {
                SiteRooms roomsOnCurrentSite = siteState.Rooms;

                return ConvertRoomToData(roomsOnCurrentSite.GetRoom(roomID));
            }

            return ConvertRoomToData(room);
        }


        /// <summary>
        /// Disables chat room and refreshes cache.
        /// </summary>
        /// <param name="roomID">Room ID to disable</param>
        public static void DisableChatRoom(int roomID)
        {
            ChatRoomInfoProvider.DisableRoom(roomID);

            ChatGlobalData.Instance.Sites.Current.Rooms.ForceUpdate();
        }


        /// <summary>
        /// Enables chat room and refreshes cache.
        /// </summary>
        /// <param name="roomID">Chat room to enabled</param>
        public static void EnableChatRoom(int roomID)
        {
            ChatRoomInfoProvider.EnableRoom(roomID);

            ChatGlobalData.Instance.Sites.Current.Rooms.ForceUpdate();
        }


        /// <summary>
        /// Combines plain text password with salt and returns its hash.
        /// 
        /// If plainText is null, it returns null. If it is empty string it retuns empty string.
        /// </summary>
        /// <param name="plainTextPassword">Password in plain text.</param>
        /// <param name="salt">Salt (ChatRoomGUID)</param>
        /// <returns>Hashed password</returns>
        public static string GetRoomPasswordHash(string plainTextPassword, string salt)
        {
            if (string.IsNullOrEmpty(plainTextPassword))
            {
                return plainTextPassword;
            }

            return SecurityHelper.GetSHA2Hash(plainTextPassword + salt);
        }


        /// <summary>
        /// Combines plain text password with salt (GUID converted to string) and returns its hash.
        /// 
        /// If plainText is null, it returns null. If it is empty string it retuns empty string.
        /// </summary>
        /// <param name="plainTextPassword">Password in plain text.</param>
        /// <param name="salt">Salt (ChatRoomGUID)</param>
        /// <returns>Hashed password</returns>
        public static string GetRoomPasswordHash(string plainTextPassword, Guid salt)
        {
            return GetRoomPasswordHash(plainTextPassword, salt.ToString());
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Truncates description if needed.
        /// </summary>
        /// <param name="description">Description</param>
        private static void VerifyChatRoomDescriptionIsValid(ref string description)
        {
            if (description.Length > 500)
            {
                description = description.Substring(0, 500);
            }
        }


        /// <summary>
        /// Validates chat room name. Name is also trimmed.
        /// 
        /// Name is valid if it is not empty and no longer than 100 characters.
        /// 
        /// Throw exception if not valid.
        /// </summary>
        /// <param name="roomName">Name to validate</param>
        private static void VerifyChatRoomNameIsValid(ref string roomName)
        {
            if (roomName == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNameCantBeEmpty);
            }

            roomName = roomName.Trim();

            if (roomName.Length == 0)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNameCantBeEmpty);
            }

            if (roomName.Length > 100)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNameTooLong, string.Format(ChatResponseStatusEnum.RoomNameTooLong.ToStringValue(), 100));
            }
        }


        /// <summary>
        /// Validates password.
        /// 
        /// Throw exception if not valid.
        /// </summary>
        /// <param name="password">Password to validate</param>
        private static void VerifyChatRoomPasswordIsValid(ref string password)
        {
            if ((password != null) && (password.Length > 100))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomPasswordTooLong, string.Format(ChatResponseStatusEnum.RoomPasswordTooLong.ToStringValue(), 100));
            }
        }


        /// <summary>
        /// Creates ChatRoomInfo from passed params. Room is assigned to the current site.
        /// </summary>
        /// <param name="displayName">displayName</param>
        /// <param name="codeName">codeName</param>
        /// <param name="isPrivate">isPrivate</param>
        /// <param name="allowAnonym">allowAnonym</param>
        /// <param name="createdByChatUserID">createdByChatUserID</param>
        /// <param name="password">password</param>
        /// <param name="isSupport">isSupport</param>
        /// <param name="isOneToOne">isOneToOne</param>
        /// <param name="description">Description of the chat room</param>
        /// <returns>ChatRoomInfo</returns>
        private static ChatRoomInfo BuildChatRoom(string displayName, string codeName, bool isPrivate, bool allowAnonym, int createdByChatUserID, string password, bool isSupport, bool isOneToOne, string description)
        {
            return BuildChatRoom(displayName, codeName, isPrivate, allowAnonym, createdByChatUserID, password, isSupport, isOneToOne, description, SiteContext.CurrentSiteID);
        }


        /// <summary>
        /// Creates ChatRoomInfo from passed params.
        /// </summary>
        /// <param name="displayName">displayName</param>
        /// <param name="codeName">codeName</param>
        /// <param name="isPrivate">isPrivate</param>
        /// <param name="allowAnonym">allowAnonym</param>
        /// <param name="createdByChatUserID">createdByChatUserID</param>
        /// <param name="password">password</param>
        /// <param name="isSupport">isSupport</param>
        /// <param name="isOneToOne">isOneToOne</param>
        /// <param name="description">Description of the chat room</param>
        /// <param name="siteID">SiteID of the chat room (null is global)</param>
        /// <returns>ChatRoomInfo</returns>
        private static ChatRoomInfo BuildChatRoom(string displayName, string codeName, bool isPrivate, bool allowAnonym, int createdByChatUserID, string password, bool isSupport, bool isOneToOne, string description, int? siteID)
        {
            return new ChatRoomInfo()
            {
                ChatRoomDisplayName = displayName,
                ChatRoomName = codeName,
                ChatRoomSiteID = siteID,
                ChatRoomEnabled = true,
                ChatRoomPrivate = isPrivate,
                ChatRoomAllowAnonym = allowAnonym,
                ChatRoomCreatedByChatUserID = createdByChatUserID,
                ChatRoomCreatedWhen = DateTime.Now, // GETDATE() will be used on SQL Server side
                ChatRoomPassword = password,
                ChatRoomIsSupport = isSupport,
                ChatRoomIsOneToOne = isOneToOne,
                ChatRoomDescription = (String.IsNullOrEmpty(description) ? null : description),
                ChatRoomGUID = Guid.NewGuid()
            };
        }


        /// <summary>
        /// Creates ChatRoomInfo which has properties set to be one-to-one room.
        /// </summary>
        /// <param name="createdByChatUser">The iniciator of one-to-one chat.</param>
        /// <param name="secondChatUser">The invited user.</param>
        /// <returns>ChatRoomInfo</returns>
        private static ChatRoomInfo BuildOneToOneRoom(ChatUserInfo createdByChatUser, ChatUserInfo secondChatUser)
        {
            return BuildChatRoom(
                MakeOneToOneDisplayName(createdByChatUser, secondChatUser),
                MakeOneToOneCodeName(createdByChatUser.ChatUserID, secondChatUser.ChatUserID),
                true, true, createdByChatUser.ChatUserID, "", false, true, String.Empty, null);
        }


        /// <summary>
        /// Creates code name for an one-to-one room. Code name looks like this: "adhoc_[lower_user_ID>]|[greater_user_ID]|[site_id]"
        /// </summary>
        /// <param name="chatUser1ID">ID of the first chat user</param>
        /// <param name="chatUser2ID">ID of the second chat user</param>
        /// <returns>Unique name of the whisper room between these two chat users</returns>
        private static string MakeOneToOneCodeName(int chatUser1ID, int chatUser2ID)
        {
            int lower, greater;

            // Chat user 1 ID is greater then chat user 2 ID
            if (chatUser1ID > chatUser2ID)
            {
                greater = chatUser1ID;
                lower = chatUser2ID;
            }
            else
            {
                greater = chatUser2ID;
                lower = chatUser1ID;
            }

            // First ID is always the lower one
            return string.Format("adhoc_{0}-{1}", lower, greater);
        }


        /// <summary>
        /// Creates display name for an one-to-one room
        /// </summary>
        /// <param name="chatUser1">First chat user</param>
        /// <param name="chatUser2">Second chat user</param>
        /// <returns>Name of the whisper room</returns>
        private static string MakeOneToOneDisplayName(ChatUserInfo chatUser1, ChatUserInfo chatUser2)
        {
            return string.Format("{0} - {1}", chatUser1.ChatUserNickname, chatUser2.ChatUserNickname); ;
        }

        #endregion
    }
}

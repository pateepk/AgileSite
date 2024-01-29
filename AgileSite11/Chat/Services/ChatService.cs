using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;

using CMS.EventLog;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Chat
{
    /// <summary>
    /// Implementation of chat service. All chat operations made from client goes through this service.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = false, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ChatService : ChatServiceBase, IChatService
    {
        #region "Verifying methods"

        /// <summary>
        /// Checks if notification is valid (belongs to current user, is invitation if should be, etc.).
        /// </summary>
        /// <param name="notification">Notification to validate</param>
        /// <param name="invitationExpected">Indicates if this notification has to be invitation otherwise it is not valid (security)</param>
        private void VerifyNotificationIsValid(ChatNotificationInfo notification, bool invitationExpected)
        {
            if ((notification == null)
                || (invitationExpected && (notification.ChatNotificationType != ChatNotificationTypeEnum.Invitation)))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }
            else if (!ChatOnlineUserHelper.IsChatUserLoggedIn())
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NotLoggedIn);
            }
            else if (notification.ChatNotificationReceiverID != ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }

            // Invitation cannot be processed more than once, hence the error if notification is already read
            if (invitationExpected && notification.ChatNotificationIsRead)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.InvitationAlreadyAnswered);
            }
        }


        /// <summary>
        /// Checks if initiated chat request belongs to current user (or contact).
        /// 
        /// Throws exception if fails.
        /// </summary>
        /// <param name="request">Initiated chat request to validate</param>
        private void VerifyInitiatedChatRequestIsValid(ChatInitiatedChatRequestInfo request)
        {
            if (request == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            // Check if request belongs to current contact
            if (request.InitiatedChatRequestContactID.HasValue && (request.InitiatedChatRequestContactID.Value == ModuleCommands.OnlineMarketingGetCurrentContactID()))
            {
                return;
            }

            // Check if request belongs to current user
            if (request.InitiatedChatRequestUserID.HasValue && (request.InitiatedChatRequestUserID.Value == MembershipContext.AuthenticatedUser.UserID))
            {
                return;
            }

            // Request does not belong to the current user - access denied
            throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
        }


        private void VerifyOperationIsNotFlooding(FloodOperationEnum operation)
        {
            if (!ChatProtectionHelper.CheckOperationForFlooding(operation))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.Flooding);
            }
        }

        #endregion


        #region "Service methods"

        /// <summary>
        /// <para>
        /// Keeping alive user in a room. User has to call this method every few seconds to let server know that he is still online.
        /// </para>
        /// <para>
        /// At the same time a new online users in a room and a new messages in a room are checked. Client sends the last time he made an update
        /// of online users (<paramref name="roomUsersLastChange"/>) and messages (<paramref name="roomMessagesLastChange"/>). If something new 
        /// has happened since that time, the changes are sent back.
        /// </para>
        /// </summary>
        /// <param name="roomID">ID of a chat room</param>
        /// <param name="roomUsersLastChange">Last update of members on this client (client gets this number in PingRoom().Users.LastChange) - if this param is null, client does not need online users. If it is 0, it is the first request and all users are returned.</param>
        /// <param name="roomMessagesLastChange">Last update of messages on this client (client gets this number in PingRoom().Messages.LastChange) - if this param is null, client does not need online users. If it is 0, it is the first request and all users are returned.</param>
        /// <param name="maxMessagesCount">Maximum number of messages returned</param>
        /// <returns>PingRoomsResultData</returns>
        public ChatGeneralResponse<PingRoomResponseData> PingRoom(int roomID, long? roomUsersLastChange, long? roomMessagesLastChange, int? maxMessagesCount)
        {
            try
            {
                VerifyIPIsNotBanned();
                ChatUserHelper.VerifyChatUserHasJoinRoomRights(roomID);
                VerifyChatUserIsOnlineInARoom(roomID);


                int chatUserID = ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID;

                RoomState room = Sites.Current.Rooms.GetRoom(roomID);

                // Store LastChecking to the DB
                if (!ChatRoomUserInfoProvider.UpdateLastChecking(chatUserID, roomID))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.NotJoinedInARoom);
                }


                PingRoomResponseData response = new PingRoomResponseData()
                {
                    RoomID = roomID,
                    IsCurrentUserAdmin = ChatUserHelper.CheckAdminRoomRights(roomID),
                };


                if (roomUsersLastChange.HasValue)
                {
                    DateTime? usersLastChange = roomUsersLastChange.Value == 0 ? (DateTime?)null : new DateTime(roomUsersLastChange.Value);

                    response.Users = room.GetOnlineUsers(usersLastChange);
                }


                if (roomMessagesLastChange.HasValue)
                {
                    bool isFirstRequest = roomMessagesLastChange.Value == 0;

                    DateTime? messagesLastChange = isFirstRequest ? (DateTime?)null : new DateTime(roomMessagesLastChange.Value);


                    response.Messages = room.GetMessages(isFirstRequest ? maxMessagesCount : null, messagesLastChange, isFirstRequest, chatUserID);
                }

                return GetOkChatResponse(response);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<PingRoomResponseData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "PingRoom", ex);

                return GetChatResponse<PingRoomResponseData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// /// <summary>
        /// Keeps user online in global list of chat online users (this method needs to be called periodically).
        /// 
        /// Checks if there were any changes in:
        /// - rooms (new room added, changed name, etc.) since <paramref name="lastRoomsChange"/>.
        /// - counts of users in rooms since <paramref name="lastUsersInRoomsChange"/>.
        /// - global online users since <paramref name="lastOnlineUsersChange"/>.
        /// - notifications for online user since <paramref name="lastNotificationChange"/>.
        /// 
        /// If any of this objects has changed, changes are send back to client.
        /// 
        /// If any of params is null, its objects are not checked. If param is 0, it is the first request and all objects are returned.
        /// 
        /// If user is not online on chat, only CurrentChatUserState will be returned and data (rooms, online users, etc.) will be null.
        /// </summary>
        /// <param name="lastRoomsChange">Time of last check for rooms list made by this client.</param>
        /// <param name="lastUsersInRoomsChange">Time of last check for counts of users in rooms made by this client.</param>
        /// <param name="lastOnlineUsersChange">Time of last check for global online users made by this client.</param>
        /// <param name="lastNotificationChange">Time of last check for notifications made by this client.</param>
        /// <returns>PingRoomsResultData</returns>
        public ChatGeneralResponse<PingResultData> Ping(long? lastRoomsChange, long? lastUsersInRoomsChange, long? lastOnlineUsersChange, long? lastNotificationChange)
        {
            try
            {
                VerifyIPIsNotBanned();

                SiteState site = Sites.Current;

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                // If chat user is logged in, set LastChecking to now
                if (currentChatUser != null)
                {
                    ChatOnlineUserInfoProvider.UpdateLastChecking(site.SiteID, currentChatUser.ChatUserID);
                }

                PingResultData response = new PingResultData()
                {
                    CurrentChatUserState = CurrentChatUserState,
                };

                // Return data only if user is logged in
                if (currentChatUser != null)
                {
                    // Rooms changes
                    if (lastRoomsChange.HasValue)
                    {
                        DateTime? roomsLastChange = lastRoomsChange.Value == 0 ? (DateTime?)null : new DateTime(lastRoomsChange.Value);

                        response.Rooms = site.Rooms.GetChangedRooms(roomsLastChange, currentChatUser);


                        // Online users change is send only if client is interested also in room changes (hence this if is nested in the upper if)
                        if (lastUsersInRoomsChange.HasValue)
                        {
                            DateTime? usersInRoomsLastChange = lastUsersInRoomsChange.Value == 0 ? (DateTime?)null : new DateTime(lastUsersInRoomsChange.Value);

                            IEnumerable<int> changedRoomsId = null;

                            if ((response.Rooms != null) && usersInRoomsLastChange.HasValue)
                            {
                                changedRoomsId = response.Rooms.List.Where(r => !r.IsRemoved).Select(r => r.ChatRoomID);
                            }

                            response.UsersInRooms = site.Rooms.GetUsersInRoomsCounts(usersInRoomsLastChange, currentChatUser, changedRoomsId);
                        }
                    }

                    // Online users changes
                    if (lastOnlineUsersChange.HasValue)
                    {
                        DateTime? onlineUsersLastChange = lastOnlineUsersChange.Value == 0 ? (DateTime?)null : new DateTime(lastOnlineUsersChange.Value);

                        response.OnlineUsers = site.OnlineUsers.GetOnlineUsers(onlineUsersLastChange);
                    }

                    // Notification changes
                    if (lastNotificationChange.HasValue)
                    {
                        DateTime? notificationsLastChange = lastNotificationChange.Value == 0 ? (DateTime?)null : new DateTime(lastNotificationChange.Value);

                        response.Notifications = site.OnlineUsers.GetNotifications(currentChatUser.ChatUserID, notificationsLastChange);
                    }
                }

                return GetOkChatResponse(response);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<PingResultData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "Ping", ex);

                return GetChatResponse<PingResultData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Joins a room.
        /// 
        /// Chat user has to be logged in before this operation.
        /// </summary>
        /// <param name="roomID">ID of a room.</param>
        /// <param name="password">Entrance password (if required).</param>
        /// <returns>JoinRoomData</returns>
        public ChatGeneralResponse<JoinRoomData> JoinRoom(int roomID, string password)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyOperationIsNotFlooding(FloodOperationEnum.JoinRoom);
                ChatUserHelper.VerifyChatUserHasJoinRoomRights(roomID);


                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                ChatRoomUserHelper.JoinUserToRoom(roomID, currentChatUser, password, true);

                ChatRoomInfo roomInfo = Sites.Current.Rooms.GetRoom(roomID).RoomInfo;

                return GetOkChatResponse(new JoinRoomData()
                {
                    ChatRoomID = roomInfo.ChatRoomID,
                    DisplayName = roomInfo.ChatRoomDisplayName,
                    HasPassword = roomInfo.HasPassword,
                    IsPrivate = roomInfo.ChatRoomPrivate,
                    AllowAnonym = roomInfo.ChatRoomAllowAnonym,
                    IsCurrentUserAdmin = ChatUserHelper.CheckAdminRoomRights(roomID),
                    IsOneToOne = roomInfo.ChatRoomIsOneToOne,
                    IsSupport = roomInfo.ChatRoomIsSupport,
                });
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<JoinRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "JoinRoom", ex);

                return GetChatResponse<JoinRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Leaves chat room.
        /// </summary>
        /// <param name="roomID">ID of a room.</param>
        /// <returns>ChatGeneralReponse</returns>
        public ChatGeneralResponse LeaveRoom(int roomID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();

                ChatRoomUserHelper.LeaveRoom(roomID, ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "LeaveRoom", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Posts a new message to a room.
        /// Chat user has to be logged in and joined this room. Message can not be empty.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="messageText">Message text</param>
        /// <returns>ChatGeneralReponse</returns>
        public ChatGeneralResponse<MessageData> PostMessage(int roomID, string messageText)
        {
            if (!ObjectFactory<ILicenseService>.StaticSingleton().IsFeatureAvailable(FeatureEnum.Chat))
            {
                return GetChatResponse<MessageData>(ChatResponseStatusEnum.BadRequest, "The feature 'Chat' is not supported in this edition. ");
            }

            try
            {
                VerifyIPIsNotBanned();
                VerifyOperationIsNotFlooding(FloodOperationEnum.PostMessage);
                ChatUserHelper.VerifyChatUserHasJoinRoomRights(roomID);
                VerifyChatUserIsOnlineInARoom(roomID);

                MessageData messageData = ChatMessageHelper.PostMessage(messageText, roomID, ChatOnlineUserHelper.GetLoggedInChatUser());

                return GetOkChatResponse(messageData);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<MessageData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "PostMessage", ex);

                return GetChatResponse<MessageData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Posts a whisper message to user. This message will be posted to a room, but only sender and receiver of this messages will be able to see it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="recipientChatUserID">Recepient chat user ID</param>
        /// <param name="messageText">Message text</param>
        /// <returns>ChatGeneralReponse</returns>
        public ChatGeneralResponse<MessageData> PostMessageToUser(int roomID, int recipientChatUserID, string messageText)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyOperationIsNotFlooding(FloodOperationEnum.PostMessage);
                ChatUserHelper.VerifyChatUserHasJoinRoomRights(roomID);
                VerifyChatUserIsOnlineInARoom(roomID);

                RoomState room;

                // Get room
                if (!Sites.Current.Rooms.ForceTryGetRoom(roomID, out room))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
                }

                RoomOnlineUserData onlineUser;
                ChatUserInfo recipientChatUser;

                // Try to get recepient from the list of online users
                if (room.TryGetUser(recipientChatUserID, out onlineUser))
                {
                    recipientChatUser = onlineUser.ChatUser;
                }
                // If he is not online - get him from DB
                else
                {
                    recipientChatUser = ChatUserInfoProvider.GetChatUserInfo(recipientChatUserID);
                }

                if (recipientChatUser == null)
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
                }

                MessageData message = ChatMessageHelper.PostMessageToUser(messageText, roomID, ChatOnlineUserHelper.GetLoggedInChatUser(), recipientChatUser);

                return GetOkChatResponse(message);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<MessageData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "PostMessageToUser", ex);

                return GetChatResponse<MessageData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Registers new anonymous chat user. This new user is automatically logged in.
        /// </summary>
        /// <param name="nickname">Nickname of a new user</param>
        /// <returns>ChatUserStateData of the new user.</returns>
        public ChatGeneralResponse<ChatUserStateData> Register(string nickname)
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatUserHelper.RegisterAndLoginChatUser(nickname);

                return GetOkChatResponse(CurrentChatUserState);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserStateData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "Register", ex);

                return GetChatResponse<ChatUserStateData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Registers new guest user (user with auto generated nickname).
        /// </summary>
        /// <returns>ChatUserStateData of the new user</returns>
        public ChatGeneralResponse<ChatUserStateData> RegisterGuest()
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatUserHelper.RegisterAndLoginChatUser();

                return GetOkChatResponse(CurrentChatUserState);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserStateData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "RegisterGuest", ex);

                return GetChatResponse<ChatUserStateData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Logouts chat user from chat (not CMSUser).
        /// 
        /// Leaves all rooms.
        /// 
        /// If CMSUser is logged in, this operation won't logout the chat user in reality.
        /// </summary>
        public ChatGeneralResponse<ChatUserStateData> Logout()
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatOnlineUserHelper.LogOutOfChatCurrentUser();

                return GetOkChatResponse(CurrentChatUserState);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserStateData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "Logout", ex);

                return GetChatResponse<ChatUserStateData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Gets info about currently logged in chat user.
        /// </summary>
        /// <returns>ChatUserStateData (id, nickname, etc.)</returns>
        public ChatGeneralResponse<ChatUserStateData> GetChatUserState()
        {
            try
            {
                VerifyIPIsNotBanned();

                return GetOkChatResponse(CurrentChatUserState);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserStateData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "GetChatUserState", ex);

                return GetChatResponse<ChatUserStateData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Creates new one-to-one chat room which will be accessible by current user and one more user (invited one).
        /// 
        /// Display name of new room will be: {nickname_current} - {nickname_invited}
        /// Code name will be: adhoc_{id_lower}-{id_upper} ({id_lower} is the lower id of the two chat users, {id_upper} is the upper one).
        /// 
        /// A current user have to manually join this room after creating it.
        /// 
        /// If those two users have chatted before, the previously created room is returned. Code name is used to find previously created chat room.
        /// </summary>
        /// <param name="invitedChatUserID">ID of invited chat user</param>
        /// <returns>Newly created chat room (or the old one if exists)</returns>
        public ChatGeneralResponse<ChatRoomData> CreateOneToOneChatRoom(int invitedChatUserID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                // Find second chat user
                ChatUserInfo secondChatUser = ChatUserInfoProvider.GetChatUserInfo(invitedChatUserID);

                if ((secondChatUser == null) || (secondChatUser.ChatUserID == currentChatUser.ChatUserID))
                {
                    return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.WrongSecondUser);
                }

                return GetOkChatResponse(ChatRoomHelper.CreateOneToOneChatRoom(currentChatUser, secondChatUser));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CreateOneToOneChatRoom", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Creates new support room. Support room is private room which can be accessed only by creator
        /// and support. When creator writes to this room, support is notified.
        /// </summary>
        /// <returns>ChatRoomData of new room</returns>
        public ChatGeneralResponse<ChatRoomData> CreateSupportChatRoom()
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatUserHelper.RegisterAndLoginChatUser(true);

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                return GetOkChatResponse(ChatRoomHelper.CreateSupportChatRoom(currentChatUser));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CreateSupportChatRoom", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Creates new support room. Support room is private room which can be accessed only by creator
        /// and support. When creator writes to this room, support is notified.
        /// 
        /// This room does not have default greeting message, but messages passed as parameter.
        /// </summary>
        /// <returns>ChatRoomData of new room</returns>
        public ChatGeneralResponse<ChatRoomData> CreateSupportChatRoomManual(IEnumerable<string> messages)
        {
            try
            {
                VerifyIPIsNotBanned();

                if ((messages == null) || (messages.Count() == 0))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
                }

                ChatUserHelper.RegisterAndLoginChatUser(true);

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                return GetOkChatResponse(ChatRoomHelper.CreateSupportChatRoomManual(currentChatUser, messages));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CreateSupportChatRoom", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Creates new chat room.
        /// 
        /// Current user have to join it manually after creating it.
        /// 
        /// Code name of this room will be: chatroom_{current_user_id}_{new_guid}
        /// </summary>
        /// <param name="displayName">Display name of a new room (cannot be empty)</param>
        /// <param name="isPrivate">Indicates if this room will be private</param>
        /// <param name="password">Optional password</param>
        /// <param name="allowAnonym">True if anonymous users can join this room (anonym is chat user without CMS.User assigned to it)</param>
        /// <param name="description">Description of the chat room</param>
        /// <returns>New chat room</returns>
        public ChatGeneralResponse<ChatRoomData> CreateChatRoom(string displayName, bool isPrivate, string password, bool allowAnonym, string description)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();
                VerifyOperationIsNotFlooding(FloodOperationEnum.CreateRoom);
                VerifyChatUserHasAnyPermission(ChatPermissionEnum.ManageRooms, ChatPermissionEnum.CreateRoomsFromLiveSite);

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                return GetOkChatResponse(ChatRoomHelper.CreateClassicChatRoom(currentChatUser, displayName, isPrivate, password, allowAnonym, description));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CreateChatRoom", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Kicks a chat user from a room. Currently logged user must be an admin to kick another user.
        /// </summary>
        /// <param name="roomID">User will be kicked from this room.</param>
        /// <param name="chatUserToKickID">ID of user to kick</param>
        /// <returns>ChatGeneralResponse (code OK if everything went fine)</returns>
        public ChatGeneralResponse KickUser(int roomID, int chatUserToKickID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                // Find chat user to kick
                ChatUserInfo chatUserToKick = ChatUserInfoProvider.GetChatUserInfo(chatUserToKickID);

                if (chatUserToKick == null)
                {
                    return GetChatResponse(ChatResponseStatusEnum.ChatUserNotFound);
                }

                if (ChatUserHelper.CheckAdminRoomRights(roomID, chatUserToKick))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.CanNotKickAdmin);
                }


                // Kick user
                ChatRoomUserHelper.KickUserFromRoom(roomID, chatUserToKick, ChatOnlineUserHelper.GetLoggedInChatUser());

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "KickUser", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Changes nickname of currently logged in user.
        /// </summary>
        /// <param name="newNickname">New nickname of current user.</param>
        /// <returns>Chat user state</returns>
        public ChatGeneralResponse<ChatUserStateData> ChangeMyNickname(string newNickname)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();
                VerifyOperationIsNotFlooding(FloodOperationEnum.ChangeNickname);

                // Change nickname (throws exception if fails).
                ChatUserHelper.ChangeChatUserNickname(ChatOnlineUserHelper.GetLoggedInChatUser(), newNickname);

                return GetOkChatResponse(CurrentChatUserState);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserStateData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "ChangeMyNickname", ex);

                return GetChatResponse<ChatUserStateData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Gets count of currently logged in support engineers.
        /// </summary>
        /// <returns>Count of online support engineers</returns>
        public ChatGeneralResponse<int> GetSupportEngineersOnlineCount()
        {
            try
            {
                VerifyIPIsNotBanned();

                return GetOkChatResponse(ChatGlobalData.Instance.Sites.Current.OnlineSupport.OnlineSupportersCount);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<int>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "GetSupportEngineersOnlineCount", ex);

                return GetChatResponse<int>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Invites user to the chat room. Invitation is inserted into the second user's notifications. Second user can enter the room after accepting the invitation.
        /// 
        /// Current user has to have admin rights to the room to invite.
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserID">Chat user id to invite</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse InviteToRoom(int roomID, int chatUserID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();

                RoomState room = Sites.Current.Rooms.GetRoom(roomID);

                if (room == null)
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
                }

                if (!room.RoomInfo.ChatRoomEnabled || (!room.RoomInfo.ChatRoomAllowAnonym && ChatOnlineUserHelper.GetLoggedInChatUser().IsAnonymous))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
                }

                if (room.RoomInfo.ChatRoomPrivate)
                {
                    // Verify if user is admin only in private rooms
                    VerifyChatUserHasAdminRoomRights(roomID);
                }

                ChatNotificationHelper.SendInvitationToRoom(room.RoomInfo, ChatOnlineUserHelper.GetLoggedInChatUser(), chatUserID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "InviteToRoom", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Search users who are online globally on a chat (they are performing Ping()) and meet condition (their nickname contains <paramref name="nickname"/>.
        /// </summary>
        /// <param name="nickname">Searches for users with this nickname</param>
        /// <param name="topN">Returns this number of found users</param>
        /// <param name="invitedToRoomID">If not null, only users who can be invited to this room will be returned</param>
        /// <returns>Online users</returns>
        public ChatGeneralResponse<IEnumerable<OnlineUserData>> SearchOnlineUsers(string nickname, int topN, int? invitedToRoomID)
        {
            try
            {
                VerifyIPIsNotBanned();

                return GetOkChatResponse(Sites.Current.OnlineUsers.SearchOnlineUsers(nickname, topN, invitedToRoomID));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<IEnumerable<OnlineUserData>>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "SearchOnlineUsers", ex);

                return GetChatResponse<IEnumerable<OnlineUserData>>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Accepts invitation:
        ///  - closes specified notification
        ///  - inserts user into room (joins)
        ///  - sends notification to sender about accepting invite
        /// </summary>
        /// <param name="notificationID">Notification (invitation) ID</param>
        /// <returns>Room which is led to by the accepted invitation (so client can show it immediatelly in a room list)</returns>
        public ChatGeneralResponse<ChatRoomData> AcceptInvitation(int notificationID)
        {
            try
            {
                VerifyIPIsNotBanned();

                // Find notification to accept
                ChatNotificationInfo notification = ChatNotificationInfoProvider.GetChatNotificationInfo(notificationID);

                VerifyNotificationIsValid(notification, true);

                ChatRoomData chatRoomData = ChatNotificationHelper.AcceptInvitation(notification);

                return GetOkChatResponse(chatRoomData);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "AcceptInvitation", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Declines invitation:
        ///  - closes specified notification
        ///  - sends notification to sender about declining invite
        /// </summary>
        /// <param name="notificationID">Notification (invitation) ID</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse DeclineInvitation(int notificationID)
        {
            try
            {
                VerifyIPIsNotBanned();

                // Find notification to decline
                ChatNotificationInfo notification = ChatNotificationInfoProvider.GetChatNotificationInfo(notificationID);

                VerifyNotificationIsValid(notification, true);

                ChatNotificationHelper.DeclineInvitation(notification);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "DeclineInvitation", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Closes (markes as read) specified notification. This can be used to close generic notifications like InvitationDeclined or InvitationAccepted.
        /// </summary>
        /// <param name="notificationID">Notification ID</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse CloseNotification(int notificationID)
        {
            try
            {
                VerifyIPIsNotBanned();

                // Find notification to close
                ChatNotificationInfo notification = ChatNotificationInfoProvider.GetChatNotificationInfo(notificationID);

                VerifyNotificationIsValid(notification, false);

                ChatNotificationHelper.MarkAsReadChatNotification(notification);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CloseNotification", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Closes (markes as read) all notifications which belongs to online users and which were sent until <paramref name="untilWhen"/>.
        /// </summary>
        /// <param name="untilWhen">Notifications received before this time (Ticks) will be closed. This time can be obtained in Ping().Notifications.LastChange</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse CloseAllNotifications(long untilWhen)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();

                ChatNotificationHelper.CloseAllChatNotifications(ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID, new DateTime(untilWhen));

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "CloseAllNotifications", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Gets permissions of currently logged in user.
        /// </summary>
        /// <returns>Permissions.</returns>
        public ChatGeneralResponse<ChatUserPermissionData> GetPermissions()
        {
            try
            {
                VerifyIPIsNotBanned();

                return GetOkChatResponse(
                    new ChatUserPermissionData()
                    {
                        ManageRooms = ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.ManageRooms),
                        CreateRoomsFromLiveSite = ChatProtectionHelper.HasCurrentUserPermission(ChatPermissionEnum.CreateRoomsFromLiveSite),
                    });
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserPermissionData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "GetPermissions", ex);

                return GetChatResponse<ChatUserPermissionData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Rejects the message.
        /// 
        /// Checks for permissions for a specified room (roomID can be get from messageID) and if current chat user has
        /// admin rights, it rejects the message.
        /// </summary>
        /// <param name="messageID">Message ID</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse RejectMessage(int messageID)
        {
            try
            {
                VerifyIPIsNotBanned();

                // Find message to reject
                ChatMessageInfo message = ChatMessageInfoProvider.GetChatMessageInfo(messageID);

                if (message == null)
                {
                    return GetChatResponse(ChatResponseStatusEnum.BadRequest);
                }

                VerifyChatUserHasAdminRoomRights(message.ChatMessageRoomID);

                // Reject message
                ChatMessageHelper.RejectMessage(message);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatUserPermissionData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "RejectMessage", ex);

                return GetChatResponse<ChatUserPermissionData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Permanently leaves current user from the private chat room (Join/Admin rights are removed from this room).
        /// 
        /// If room is not private, it acts as normale LeaveRoom().
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse LeaveRoomPermanently(int roomID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserIsLoggedIn();

                // Leave permanently from the room
                ChatRoomUserHelper.LeaveRoomPermanently(roomID, ChatOnlineUserHelper.GetLoggedInChatUser());

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "LeaveUserPermanently", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Kickes user permanently from a private room. If this room is public, it acts as a normal kick.
        /// 
        /// Kicked user won't be able to enter room again, unless he will be invited one more time.
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserToKickID">ID of chat user which should be kicked</param>
        /// <returns>General response</returns>
        public ChatGeneralResponse KickUserPermanently(int roomID, int chatUserToKickID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                // Find chat user to kick
                ChatUserInfo chatUserToKick = ChatUserInfoProvider.GetChatUserInfo(chatUserToKickID);

                if (chatUserToKick == null)
                {
                    return GetChatResponse(ChatResponseStatusEnum.ChatUserNotFound);
                }

                if (ChatUserHelper.CheckAdminRoomRights(roomID, chatUserToKick))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.CanNotKickAdmin);
                }

                // Kick user permanently
                ChatRoomUserHelper.KickUserPermanentlyFromRoom(roomID, chatUserToKick, ChatOnlineUserHelper.GetLoggedInChatUser());

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "KickUserPermanently", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Adds admin to the room. Only admin can do it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToAddID">Chat user who will become admin</param>
        /// <returns>ChatGeneralResponse</returns>
        public ChatGeneralResponse AddAdmin(int roomID, int chatUserToAddID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                RoomState room = ChatGlobalData.Instance.Sites.Current.Rooms.GetRoom(roomID);

                if (room == null)
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
                }

                ChatRoomUserHelper.IncreaseChatAdminLevel(roomID, chatUserToAddID, AdminLevelEnum.Admin);

                // Insert add admin notification
                ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.AdminAdded, ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID, chatUserToAddID, roomID, room.RoomInfo.ChatRoomSiteID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "AddAdmin", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Deletes admin from the room. Only admin can do it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToDeleteID">Chat user who will loose room admin permissions</param>
        /// <returns>ChatGeneralResponse</returns>
        public ChatGeneralResponse DeleteAdmin(int roomID, int chatUserToDeleteID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                RoomState room = ChatGlobalData.Instance.Sites.Current.Rooms.GetRoom(roomID);

                if (room == null)
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
                }

                ChatRoomUserHelper.SetChatAdminLevel(roomID, chatUserToDeleteID, AdminLevelEnum.Join);

                // Insert add admin notification
                ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.AdminDeleted, ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID, chatUserToDeleteID, roomID, room.RoomInfo.ChatRoomSiteID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "DeleteAdmin", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Changes existing chat room.
        /// </summary>
        /// <param name="roomID">Chat room to change</param>
        /// <param name="displayName">Display name of a new room (cannot be empty)</param>
        /// <param name="isPrivate">Indicates if this room will be private</param>
        /// <param name="hasPassword">Indicates if room will have password</param>
        /// <param name="password">Optional password</param>
        /// <param name="allowAnonym">True if anonymous users can join this room (anonym is chat user without CMS.User assigned to it)</param>
        /// <param name="description">Description of the chat room</param>
        /// <returns>Changed chat room</returns>
        public ChatGeneralResponse<ChatRoomData> ChangeChatRoom(int roomID, string displayName, bool isPrivate, bool hasPassword, string password, bool allowAnonym, string description)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                // If user wants to keep password, but didnt set one, password is set to null what means DON'T CHANGE
                if (hasPassword && (password.Length == 0))
                {
                    password = null;
                }
                // If user don't want to password protect room, set password to ""
                else if (!hasPassword)
                {
                    password = "";
                }

                ChatRoomData roomData = ChatRoomHelper.ChangeChatRoom(roomID, displayName, isPrivate, password, allowAnonym, description, true);

                return GetOkChatResponse(roomData);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<ChatRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "ChangeChatRoom", ex);

                return GetChatResponse<ChatRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Disables room. User has to be either Global admin, has ManageRooms permission or be creator/admin of this room.
        /// </summary>
        /// <param name="roomID">Chat room to delete</param>
        /// <returns>GeneralResponse</returns>
        public ChatGeneralResponse DeleteChatRoom(int roomID)
        {
            try
            {
                VerifyIPIsNotBanned();
                VerifyChatUserHasAdminRoomRights(roomID);

                ChatRoomHelper.DisableChatRoom(roomID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "DeleteChatRoom", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Checks if there is new initiated chat request pending for this user and returns it. Returns null if there is nothing.
        /// 
        /// Requests are searched by ContactID and UserID.
        /// </summary>
        /// <param name="lastChange">Last state on the client.</param>
        public ChatGeneralResponse<InitiateChatRequestData> PingInitiate(long? lastChange)
        {
            try
            {
                VerifyIPIsNotBanned();

                int? contactID;
                int? userID;

                contactID = ModuleCommands.OnlineMarketingGetCurrentContactID();
                if (contactID == 0)
                {
                    contactID = null;
                }

                var currentUser = MembershipContext.AuthenticatedUser;

                userID = currentUser.IsPublic() ? (int?)null : currentUser.UserID;

                DateTime? lastChangeDate = lastChange.HasValue ? new DateTime(lastChange.Value) : (DateTime?)null;

                return GetOkChatResponse(ChatGlobalData.Instance.InitiatedChats.GetInitiatedChatRequest(contactID, userID, lastChangeDate));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<InitiateChatRequestData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "PingInitiate", ex);

                return GetChatResponse<InitiateChatRequestData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Accepts initiated chat request.
        /// </summary>
        /// <param name="chatRoomID">Room used in the request</param>
        public ChatGeneralResponse AcceptChatRequest(int chatRoomID)
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatInitiatedChatRequestInfo request = ChatInitiatedChatRequestInfoProvider.GetInitiateRequest(chatRoomID);

                VerifyInitiatedChatRequestIsValid(request);

                // Return error if request was already accepted or is deleted
                if ((request.InitiatedChatRequestState == InitiatedChatRequestStateEnum.Accepted) || (request.InitiatedChatRequestState == InitiatedChatRequestStateEnum.Deleted))
                {
                    throw new ChatServiceException(ChatResponseStatusEnum.InitiatedChatRequestAlreadyAccepted);
                }

                ChatInitiatedChatRequestHelper.AcceptChatRequest(request);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "AcceptChatRequest", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Declines initiated chat request.
        /// </summary>
        /// <param name="chatRoomID">Room used in the request</param>
        public ChatGeneralResponse DeclineChatRequest(int chatRoomID)
        {
            try
            {
                VerifyIPIsNotBanned();

                ChatInitiatedChatRequestInfo request = ChatInitiatedChatRequestInfoProvider.GetInitiateRequest(chatRoomID);

                VerifyInitiatedChatRequestIsValid(request);

                // Do nothing if request is not in the New state
                if (request.InitiatedChatRequestState == InitiatedChatRequestStateEnum.New)
                {
                    ChatInitiatedChatRequestHelper.DeclineChatRequest(request);
                }

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatService", "DeclineChatRequest", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }

        #endregion
    }
}

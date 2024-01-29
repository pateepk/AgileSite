using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CMS.Chat
{
    /// <summary>
    /// Service contract for chat service.
    /// </summary>
    [ServiceContract(Namespace = "urn:chat")]
    public interface IChatService
    {
        #region "Operation contracts"

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
        [OperationContract]
        ChatGeneralResponse<PingRoomResponseData> PingRoom(int roomID, long? roomUsersLastChange, long? roomMessagesLastChange, int? maxMessagesCount);


        /// <summary>
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
        /// </summary>
        /// <param name="lastRoomsChange">Time of last check for rooms list made by this client.</param>
        /// <param name="lastUsersInRoomsChange">Time of last check for counts of users in rooms made by this client.</param>
        /// <param name="lastOnlineUsersChange">Time of last check for global online users made by this client.</param>
        /// <param name="lastNotificationChange">Time of last check for notifications made by this client.</param>
        /// <returns>PingRoomsResultData</returns>
        [OperationContract]
        ChatGeneralResponse<PingResultData> Ping(long? lastRoomsChange, long? lastUsersInRoomsChange, long? lastOnlineUsersChange, long? lastNotificationChange);


        /// <summary>
        /// Joins a room.
        /// 
        /// Chat user has to be logged in before this operation.
        /// </summary>
        /// <param name="roomID">ID of a room.</param>
        /// <param name="password">Entrance password (if required).</param>
        /// <returns>JoinRoomData</returns>
        [OperationContract]
        ChatGeneralResponse<JoinRoomData> JoinRoom(int roomID, string password);


        /// <summary>
        /// Leaves chat room.
        /// </summary>
        /// <param name="roomID">ID of a room.</param>
        /// <returns>ChatGeneralReponse</returns>
        [OperationContract]
        ChatGeneralResponse LeaveRoom(int roomID);


        /// <summary>
        /// Posts a new message to a room.
        /// 
        /// Chat user has to be logged in and joined this room. Message can not be empty.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="messageText">Message text</param>
        /// <returns>ChatGeneralReponse</returns>
        [OperationContract]
        ChatGeneralResponse<MessageData> PostMessage(int roomID, string messageText);


        /// <summary>
        /// Posts a whisper message to user. This message will be posted to a room, but only sender and receiver of this messages will be able to see it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="recipientChatUserID">Recepient chat user ID</param>
        /// <param name="messageText">Message text</param>
        /// <returns>ChatGeneralReponse</returns>
        [OperationContract]
        ChatGeneralResponse<MessageData> PostMessageToUser(int roomID, int recipientChatUserID, string messageText);


        /// <summary>
        /// Registers new anonymous chat user. This new user is automatically logged in.
        /// </summary>
        /// <param name="nickname">Nickname of a new user</param>
        /// <returns>ChatUserStateData of the new user.</returns>
        [OperationContract]
        ChatGeneralResponse<ChatUserStateData> Register(string nickname);


        /// <summary>
        /// Registers new guest user (user with auto generated nickname).
        /// </summary>
        /// <returns>ChatUserStateData of the new user</returns>
        [OperationContract]
        ChatGeneralResponse<ChatUserStateData> RegisterGuest();


        /// <summary>
        /// Logouts chat user from chat (not CMSUser).
        /// 
        /// Leaves all rooms.
        /// 
        /// If CMSUser is logged in, this operation won't logout the chat user in reality.
        /// </summary>
        [OperationContract]
        ChatGeneralResponse<ChatUserStateData> Logout();


        /// <summary>
        /// Gets info about currently logged in chat user.
        /// </summary>
        /// <returns>ChatUserStateData (id, nickname, etc.)</returns>
        [OperationContract]
        ChatGeneralResponse<ChatUserStateData> GetChatUserState();


        /// <summary>
        /// Creates new one-to-one chat room which will be accessible by current user and one more user (invited one).
        /// 
        /// Display name of new room will be: {nickname_current} - {nickname_invited}
        /// Code name will be: adhoc_{id_lower}-{id_upper} ({id_lower} is the lower id of the two chat users, {id_upper} is the upper one);
        /// 
        /// A current user have to manually join this room after creating it.
        /// 
        /// If those two users have chatted before, the previously created room is returned. Code name is used to find previously created chat room.
        /// </summary>
        /// <param name="invitedChatUserID">ID of invited chat user</param>
        /// <returns>Newly created chat room (or the old one if exists)</returns>
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> CreateOneToOneChatRoom(int invitedChatUserID);


        /// <summary>
        /// Creates new support room. Support room is private room which can be accessed only by creator
        /// and support. When creator writes to this room, support is notified.
        /// </summary>
        /// <returns>ChatRoomData of new room</returns>
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> CreateSupportChatRoom();


        /// <summary>
        /// Creates new support room. Support room is private room which can be accessed only by creator
        /// and support. When creator writes to this room, support is notified.
        /// 
        /// This room does not have default greeting message, but messages passed as parameter.
        /// </summary>
        /// <returns>ChatRoomData of new room</returns>
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> CreateSupportChatRoomManual(IEnumerable<string> messages);

        
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
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> CreateChatRoom(string displayName, bool isPrivate, string password, bool allowAnonym, string description);


        /// <summary>
        /// Kicks a chat user from a room. Currently logged user must be an admin to kick another user.
        /// </summary>
        /// <param name="roomID">User will be kicked from this room.</param>
        /// <param name="chatUserToKickID">ID of user to kick</param>
        /// <returns>ChatGeneralResponse (code OK if everything went fine)</returns>
        [OperationContract]
        ChatGeneralResponse KickUser(int roomID, int chatUserToKickID);


        /// <summary>
        /// Changes nickname of currently logged in user.
        /// </summary>
        /// <param name="newNickname">New nickname of current user.</param>
        /// <returns>Chat user state</returns>
        [OperationContract]
        ChatGeneralResponse<ChatUserStateData> ChangeMyNickname(string newNickname);


        /// <summary>
        /// Gets count of currently logged in support engineers.
        /// </summary>
        /// <returns>Count of online support engineers</returns>
        [OperationContract]
        ChatGeneralResponse<int> GetSupportEngineersOnlineCount();


        /// <summary>
        /// Invites user to the chat room. Invitation is inserted into the second user's notifications. Second user can enter the room after accepting the invitation.
        /// 
        /// Current user has to have admin rights to the room to invite.
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserID">Chat user id to invite</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse InviteToRoom(int roomID, int chatUserID);


        /// <summary>
        /// Search users who are online globally on a chat (they are performing Ping()) and meet condition (their nickname contains <paramref name="nickname"/>.
        /// </summary>
        /// <param name="nickname">Searches for users with this nickname</param>
        /// <param name="topN">Returns this number of found users</param>
        /// <param name="invitedToRoomID">If not null, only users who can be invited to this room will be returned</param>
        /// <returns>Online users</returns>
        [OperationContract]
        ChatGeneralResponse<IEnumerable<OnlineUserData>> SearchOnlineUsers(string nickname, int topN, int? invitedToRoomID);


        /// <summary>
        /// Accepts invitation:
        ///  - closes specified notification
        ///  - inserts user into room (joins)
        ///  - sends notification to sender about accepting invite
        /// </summary>
        /// <param name="notificationID">Notification (invitation) ID</param>
        /// <returns>Room which is led to by the accepted invitation (so client can show it immediatelly in a room list)</returns>
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> AcceptInvitation(int notificationID);


        /// <summary>
        /// Declines invitation:
        ///  - closes specified notification
        ///  - sends notification to sender about declining invite
        /// </summary>
        /// <param name="notificationID">Notification (invitation) ID</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse DeclineInvitation(int notificationID);


        /// <summary>
        /// Closes (markes as read) specified notification. This can be used to close generic notifications like InvitationDeclined or InvitationAccepted.
        /// </summary>
        /// <param name="notificationID">Notification ID</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse CloseNotification(int notificationID);


        /// <summary>
        /// Closes (markes as read) all notifications which belongs to online users and which were sent until <paramref name="untilWhen"/>.
        /// </summary>
        /// <param name="untilWhen">Notifications received before this time (Ticks) will be closed. This time can be obtained in Ping().Notifications.LastChange</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse CloseAllNotifications(long untilWhen);


        /// <summary>
        /// Gets permissions of currently logged in user.
        /// </summary>
        /// <returns>Permissions</returns>
        [OperationContract]
        ChatGeneralResponse<ChatUserPermissionData> GetPermissions();


        /// <summary>
        /// Rejects the message.
        /// 
        /// Checks for permissions for a specified room (roomID can be get from messageID) and if current chat user has
        /// admin rights, it rejects the message.
        /// </summary>
        /// <param name="messageID">Message ID</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse RejectMessage(int messageID);


        /// <summary>
        /// Permanently leaves current user from the private chat room (Join/Admin rights are removed from this room).
        /// 
        /// If room is not private, it acts as normale LeaveRoom().
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse LeaveRoomPermanently(int roomID);


        /// <summary>
        /// Kickes user permanently from a private room. If this room is public, it acts as a normal kick.
        /// 
        /// Kicked user won't be able to enter room again, unless he will be invited one more time.
        /// </summary>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserToKickID">ID of chat user which should be kicked</param>
        /// <returns>General response</returns>
        [OperationContract]
        ChatGeneralResponse KickUserPermanently(int roomID, int chatUserToKickID);


        /// <summary>
        /// Adds admin to the room. Only admin can do it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToAddID">Chat user who will become admin</param>
        /// <returns>ChatGeneralResponse</returns>
        [OperationContract]
        ChatGeneralResponse AddAdmin(int roomID, int chatUserToAddID);


        /// <summary>
        /// Deletes admin from the room. Only admin can do it.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToDeleteID">Chat user who will loose room admin permissions</param>
        /// <returns>ChatGeneralResponse</returns>
        [OperationContract]
        ChatGeneralResponse DeleteAdmin(int roomID, int chatUserToDeleteID);


        /// <summary>
        /// Changes existing chat room.
        /// </summary>
        /// <param name="roomID">Chat room to change</param>
        /// <param name="displayName">Display name of a new room (cannot be empty)</param>
        /// <param name="isPrivate">Indicates if this room will be private</param>
        /// <param name="hasPassword">Indicates if room has password</param>
        /// <param name="password">Optional password</param>
        /// <param name="allowAnonym">True if anonymous users can join this room (anonym is chat user without CMS.User assigned to it)</param>
        /// <param name="description">Description of the chat room</param>
        /// <returns>Changed chat room</returns>
        [OperationContract]
        ChatGeneralResponse<ChatRoomData> ChangeChatRoom(int roomID, string displayName, bool isPrivate, bool hasPassword, string password, bool allowAnonym, string description);


        /// <summary>
        /// Disables room. User has to be either Global admin, has ManageRooms permission or be creator/admin of this room.
        /// </summary>
        /// <param name="roomID">Chat room to delete</param>
        /// <returns>GeneralResponse</returns>
        [OperationContract]
        ChatGeneralResponse DeleteChatRoom(int roomID);


        /// <summary>
        /// Checks if there is new initiated chat request pending for this user and returns it. Returns null if there is nothing.
        /// 
        /// Requests are searched by ContactID and UserID.
        /// </summary>
        /// <param name="lastChange">Last state on the client.</param>
        [OperationContract]
        ChatGeneralResponse<InitiateChatRequestData> PingInitiate(long? lastChange);


        /// <summary>
        /// Accepts initiated chat request.
        /// </summary>
        /// <param name="chatRoomID">Room used in the request</param>
        [OperationContract]
        ChatGeneralResponse AcceptChatRequest(int chatRoomID);


        /// <summary>
        /// Declines initiated chat request.
        /// </summary>
        /// <param name="chatRoomID">Room used in the request</param>
        [OperationContract]
        ChatGeneralResponse DeclineChatRequest(int chatRoomID);
        
        #endregion
    }
}

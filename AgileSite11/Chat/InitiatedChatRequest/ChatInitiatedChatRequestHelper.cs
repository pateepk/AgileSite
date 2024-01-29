using System;
using System.Linq;
using System.Text;

using CMS.Membership;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for initiated chat.
    /// </summary>
    public static class ChatInitiatedChatRequestHelper
    {
        #region "Public static methods"

        /// <summary>
        /// Initiates chat with user. User can't be public user. 
        /// </summary>
        /// <param name="userID">ID of user to initiate chat with.</param>
        /// <returns>Room which was created for this conversation.</returns>
        public static SupportRoomData InitiateChatByUserID(int userID)
        {
            UserInfo user = UserInfoProvider.GetUserInfo(userID);

            if ((user == null) || user.IsPublic())
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            return InitiateChat(userID, null, user.FullName);
        }


        /// <summary>
        /// Initiates chat with Contact.
        /// </summary>
        /// <param name="contactID">ID of contact to initiate chat with.</param>
        /// <returns>Room created for this communication.</returns>
        public static SupportRoomData InitiateChatByContactID(int contactID)
        {
            return InitiateChat(null, contactID, "Contact_" + contactID);
        }


        /// <summary>
        /// Accepts request. Marks it as accepted, gives user join rights to this room and logs him into chat.
        /// </summary>
        /// <param name="request">Request to accept.</param>
        public static void AcceptChatRequest(ChatInitiatedChatRequestInfo request)
        {
            ChatUserHelper.RegisterAndLoginChatUser(true);

            ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

            request.InitiatedChatRequestState = InitiatedChatRequestStateEnum.Accepted;
            request.InitiatedChatRequestLastModification = DateTime.Now; // GETDATE() will be used on SQL Server side

            request.Update();

            ChatRoomUserHelper.IncreaseChatAdminLevel(request.InitiatedChatRequestRoomID, currentChatUser.ChatUserID, AdminLevelEnum.Join);
        }


        /// <summary>
        /// Declines request
        /// </summary>
        /// <param name="request">Initiated chat request to decline.</param>
        public static void DeclineChatRequest(ChatInitiatedChatRequestInfo request)
        {
            request.InitiatedChatRequestState = InitiatedChatRequestStateEnum.Declined;
            request.InitiatedChatRequestLastModification = DateTime.Now; // GETDATE() will be used on SQL Server side

            request.Update();

            ChatMessageHelper.InsertSystemMessage(request.InitiatedChatRequestRoomID, ChatMessageTypeEnum.ChatRequestDeclined);

            if (request.InitiatedChatRequestContactID.HasValue)
            {
                ChatGlobalData.Instance.InitiatedChats.InvalidateContactIDCache();
            }
            else
            {
                ChatGlobalData.Instance.InitiatedChats.InvalidateUserIDCache();
            }
        }

        #endregion


        #region "Private static methods"

        private static SupportRoomData InitiateChat(int? userID, int? contactID, string roomName)
        {
            ChatUserInfo currentChatUser = ChatOnlineSupportHelper.SupportChatUser;

            ChatInitiatedChatRequestInfo existingRequest = ChatInitiatedChatRequestInfoProvider.GetInitiateRequest(userID, contactID);

            ChatRoomInfo initiatedChatRoom = null;

            if (existingRequest == null)
            {
                initiatedChatRoom = ChatRoomHelper.CreateIntiatedChatRoom(roomName + "_{0}", currentChatUser);

                InsertRequest(userID, contactID, initiatedChatRoom.ChatRoomID, currentChatUser);
            }
            else
            {
                // State is new - return existing room
                if (existingRequest.InitiatedChatRequestState == InitiatedChatRequestStateEnum.New)
                {
                    initiatedChatRoom = ChatRoomInfoProvider.GetChatRoomInfo(existingRequest.InitiatedChatRequestRoomID);
                }
                else
                {
                    initiatedChatRoom = ChatRoomHelper.CreateIntiatedChatRoom(roomName + "_{0}", currentChatUser);

                    UpdateRequest(existingRequest, initiatedChatRoom.ChatRoomID, currentChatUser);
                }
            }

            ChatSupportTakenRoomHelper.TakeRoom(currentChatUser.ChatUserID, initiatedChatRoom.ChatRoomID);

            return new SupportRoomData
            {
                DisplayName = initiatedChatRoom.ChatRoomDisplayName,
                ChatRoomID = initiatedChatRoom.ChatRoomID,
                UnreadMessagesCount = 0,
                IsTaken = true,
                IsRemoved = false,
            };
        }


        private static void InsertRequest(int? userID, int? contactID, int roomID, ChatUserInfo initiator)
        {
            new ChatInitiatedChatRequestInfo()
            {
                InitiatedChatRequestUserID = userID,
                InitiatedChatRequestContactID = contactID,
                InitiatedChatRequestRoomID = roomID,
                InitiatedChatRequestInitiatorChatUserID = initiator.ChatUserID,
                InitiatedChatRequestInitiatorName = initiator.ChatUserNickname,
                InitiatedChatRequestLastModification = DateTime.Now, // GETDATE() will be used on SQL Server side
                InitiatedChatRequestState = InitiatedChatRequestStateEnum.New,
            }.Insert();
        }


        private static void UpdateRequest(ChatInitiatedChatRequestInfo existingRequest, int roomID, ChatUserInfo initiator)
        {
            existingRequest.InitiatedChatRequestRoomID = roomID;
            existingRequest.InitiatedChatRequestInitiatorChatUserID = initiator.ChatUserID;
            existingRequest.InitiatedChatRequestInitiatorName = initiator.ChatUserNickname;
            existingRequest.InitiatedChatRequestLastModification = DateTime.Now; // GETDATE() will be used on SQL Server side
            existingRequest.InitiatedChatRequestState = InitiatedChatRequestStateEnum.New;

            existingRequest.Update();
        }

        #endregion
    }
}

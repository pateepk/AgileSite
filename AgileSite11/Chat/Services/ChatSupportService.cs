using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;

using CMS.SiteProvider;
using CMS.Membership;
using CMS.EventLog;


namespace CMS.Chat
{
    /// <summary>
    /// Implementation of chat support service.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    public class ChatSupportService : ChatServiceBase, IChatSupportService
    {
        #region "Private methods"

        private void VerifySupportIsOnline()
        {
            if (!ChatOnlineSupportHelper.IsSupportOnline)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.SupportIsNotOnline);
            }
        }

        #endregion


        #region "Service methods"

        /// <summary>
        /// Checks for new rooms needing support.
        /// 
        /// Keeps alive (sets LastChecking to now) user on support and classic chat.
        /// </summary>
        /// <param name="lastChange">Time of last change which has this client. Only new rooms will be send. If null, all support rooms with pending messages will be send.</param>
        public ChatGeneralResponse<SupportPingResponseData> SupportPing(long? lastChange)
        {
            try
            {
                SupportPingResponseData response = new SupportPingResponseData();
                
                ChatUserInfo currentChatUser = ChatOnlineSupportHelper.SupportChatUser;

                if (currentChatUser == null)
                {
                    response.OnlineSupportChatUserID = null;

                    return GetOkChatResponse(response);
                }

                response.OnlineSupportChatUserID = currentChatUser.ChatUserID;

                
                // Update last checking in "classic" chat online users state, so user won't be logged out of chat
                ChatOnlineUserInfoProvider.UpdateLastChecking(SiteContext.CurrentSiteID, currentChatUser.ChatUserID);
                // Update last checkin in "support online state"
                ChatOnlineSupportInfoProvider.UpdateLastChecking(SiteContext.CurrentSiteID, currentChatUser.ChatUserID);

                DateTime? lastChangeDT = !lastChange.HasValue ? (DateTime?)null : new DateTime(lastChange.Value);

                response.Rooms = ChatGlobalData.Instance.Sites.Current.SupportRooms.GetChangedSupportRooms(currentChatUser.ChatUserID, lastChangeDT);

                return GetOkChatResponse(response);
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<SupportPingResponseData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "SupportPing", ex);

                return GetChatResponse<SupportPingResponseData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Joins current CMS user to classic chat as hidden and also sets him as online on support chat.
        /// </summary>
        public ChatGeneralResponse EnterSupport()
        {
            try
            {
                VerifyChatUserHasPermission(ChatPermissionEnum.EnterSupport);

                ChatOnlineSupportHelper.EnterSupport();

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "EnterSupport", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Leaves support chat.
        /// </summary>
        public ChatGeneralResponse LeaveSupport()
        {
            try
            {
                ChatOnlineSupportHelper.LeaveSupport();

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "LeaveSupport", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Takes support room. After this call, new messages in this room won't be send to other users - only to the current one.
        /// </summary>
        /// <param name="roomID">Room ID to take</param>
        public ChatGeneralResponse SupportTakeRoom(int roomID)
        {
            try
            {
                VerifySupportIsOnline();

                ChatSupportTakenRoomHelper.TakeRoom(ChatOnlineSupportHelper.SupportChatUser.ChatUserID, roomID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "SupportTakeRoom", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Leaves room. After this call, new messages in this room will be send to all online supporters.
        /// </summary>
        /// <param name="roomID">Room ID to leave</param>
        public ChatGeneralResponse SupportLeaveRoom(int roomID)
        {
            try
            {
                VerifySupportIsOnline();

                ChatSupportTakenRoomHelper.ResolveTakenRoom(ChatOnlineSupportHelper.SupportChatUser.ChatUserID, roomID);

                return GetOkChatResponse();
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "SupportLeaveRoom", ex);

                return GetChatResponse(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Initiates chat with user identified by his UserID.
        /// </summary>
        /// <param name="userID">UserID to start chat with</param>
        public ChatGeneralResponse<SupportRoomData> InitiateChatByUserID(int userID)
        {
            try
            {
                VerifySupportIsOnline();

                return GetOkChatResponse(ChatInitiatedChatRequestHelper.InitiateChatByUserID(userID));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<SupportRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "InitiateChatByUserID", ex);

                return GetChatResponse<SupportRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }


        /// <summary>
        /// Initiates chat with user identified by his ContactID.
        /// </summary>
        /// <param name="contactID">ContactID to start chat with</param>
        public ChatGeneralResponse<SupportRoomData> InitiateChatByContactID(int contactID)
        {
            try
            {
                VerifySupportIsOnline();

                return GetOkChatResponse(ChatInitiatedChatRequestHelper.InitiateChatByContactID(contactID));
            }
            catch (ChatServiceException cse)
            {
                return GetChatResponse<SupportRoomData>(cse.StatusCode, cse.StatusMessage);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ChatSupportService", "InitiateChatByUserID", ex);

                return GetChatResponse<SupportRoomData>(ChatResponseStatusEnum.UnknownError);
            }
        }

        #endregion
    }
}























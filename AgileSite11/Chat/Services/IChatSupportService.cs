using System;
using System.ServiceModel;

namespace CMS.Chat
{
    /// <summary>
    /// Service contract for ChatSupportService.
    /// </summary>
    [ServiceContract(Namespace = "urn:chat")]
    public interface IChatSupportService
    {
        #region "Operation contracts"

        /// <summary>
        /// Checks for new rooms needing support.
        /// 
        /// Keeps alive (sets LastChecking to now) user on support and classic chat.
        /// </summary>
        /// <param name="lastChange">Time of last change which has this client. Only new rooms will be send. If null, all support rooms with pending messages will be send.</param>
        [OperationContract]
        ChatGeneralResponse<SupportPingResponseData> SupportPing(long? lastChange);


        /// <summary>
        /// Joins current CMS user to classic chat as hidden and also sets him as online on support chat.
        /// </summary>
        [OperationContract]
        ChatGeneralResponse EnterSupport();


        /// <summary>
        /// Leaves support chat.
        /// </summary>
        [OperationContract]
        ChatGeneralResponse LeaveSupport();


        /// <summary>
        /// Takes support room. After this call, new messages in this room won't be send to other users - only to the current one.
        /// </summary>
        /// <param name="roomID">Room ID to take</param>
        [OperationContract]
        ChatGeneralResponse SupportTakeRoom(int roomID);


        /// <summary>
        /// Leaves room. After this call, new messages in this room will be send to all online supporters.
        /// </summary>
        /// <param name="roomID">Room ID to leave</param>
        [OperationContract]
        ChatGeneralResponse SupportLeaveRoom(int roomID);


        #region "Initiated chat"

        /// <summary>
        /// Initiates chat with user identified by his UserID.
        /// </summary>
        /// <param name="userID">UserID to start chat with</param>
        [OperationContract]
        ChatGeneralResponse<SupportRoomData> InitiateChatByUserID(int userID);


        /// <summary>
        /// Initiates chat with user identified by his ContactID.
        /// </summary>
        /// <param name="contactID">ContactID to start chat with</param>
        [OperationContract]
        ChatGeneralResponse<SupportRoomData> InitiateChatByContactID(int contactID);

        #endregion

        #endregion
    }
}

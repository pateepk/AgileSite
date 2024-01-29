using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for Chat_SupportTakenRoom.
    /// </summary>
    public static class ChatSupportTakenRoomHelper
    {
        /// <summary>
        /// Sets room as taken by chat user.
        /// </summary>
        /// <param name="chatUserID">Room will be taken by this chat user.</param>
        /// <param name="roomID">This room will be taken.</param>
        public static void TakeRoom(int chatUserID, int roomID)
        {
            if (!ChatSupportTakenRoomInfoProvider.IsRoomTaken(roomID, chatUserID))
            {
                ChatSupportTakenRoomInfoProvider.TakeRoom(chatUserID, roomID);
            }
            else
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomAlreadyTaken);
            }
        }


        /// <summary>
        /// Deletes taken room. This room will no longer be taken.
        /// </summary>
        /// <param name="chatUserID">Chat user ID who has this room taken.</param>
        /// <param name="roomID">Room ID.</param>
        public static void ResolveTakenRoom(int chatUserID, int roomID)
        {
            ChatSupportTakenRoomInfoProvider.ResolveTakenRoom(chatUserID, roomID);
        }
    }
}

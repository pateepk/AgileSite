using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data class containing information about chat request. This class is send to client.
    /// </summary>
    [DataContract]
    public class InitiateChatRequestData : IChatCacheableWithCurrentState<int>
    {
        #region "Data members"

        /// <summary>
        /// Nickname of chat user who initiated this chat.
        /// </summary>
        [DataMember]
        public string InitiatorName { get; set; }


        /// <summary>
        /// ID of room which was created for this conversation.
        /// </summary>
        [DataMember]
        public int RoomID { get; set; }


        /// <summary>
        /// Messages written by supporter.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Messages { get; set; }


        /// <summary>
        /// If true, request was accepted or rejected and should be hidden on client.
        /// 
        /// Request should be removed if state is something else than New.
        /// </summary>
        [DataMember]
        public bool IsRemoved 
        {
            get
            {
                return RequestState != InitiatedChatRequestStateEnum.New;
            }
            set // Only because [DataMember] requires it
            {
            }
        }


        /// <summary>
        /// Time when was this request changed for the last time. This time should be send back to server by client in next request.
        /// </summary>
        [DataMember]
        public long LastChange
        {
            get
            {
                return ChangeTime.Ticks;
            }
            set
            {
                ChangeTime = new DateTime(value);
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// State of this request.
        /// </summary>
        public InitiatedChatRequestStateEnum RequestState { get; set; }


        /// <summary>
        /// Type of change. Based on this value item will be removed from cache or just modified. Item should be removed only if it is in the Deleted state.
        /// </summary>
        public ChangeTypeEnum ChangeType
        {
            get
            {
                return RequestState == InitiatedChatRequestStateEnum.Deleted ? ChangeTypeEnum.Remove : ChangeTypeEnum.Modify;
            }
        }


        /// <summary>
        /// This request will be stored under this key in cache. It can be either UserID or ContactID (depending how was it retrieved from DB).
        /// </summary>
        public int PK { get; set; }


        /// <summary>
        /// When was this request changed for the last time (new message in room, changed state, etc.).
        /// </summary>
        public DateTime ChangeTime { get; set; }

        #endregion
    }
}

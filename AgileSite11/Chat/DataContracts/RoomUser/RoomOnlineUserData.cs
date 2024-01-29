using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about one online user in a room.
    /// </summary>
    [DataContract]
    public class RoomOnlineUserData : IChatCacheableWithCurrentState<int>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="chatUser">Chat user</param>
        /// <param name="isOnline">Is online</param>
        /// <param name="adminLevel">Admin level</param>
        /// <param name="lastChange">Last modification</param>
        /// <param name="isRemoved">True if this user should be removed from list on client</param>
        public RoomOnlineUserData(ChatUserInfo chatUser, bool isOnline, AdminLevelEnum adminLevel, DateTime lastChange, bool isRemoved)
        {
            ChatUser = chatUser;

            IsOnline = isOnline;

            AdminLevel = adminLevel;

            ChangeTime = lastChange;

            IsRemoved = isRemoved;

            if (!IsRemoved)
            {
                // check for permissions isn't done earlier because of the performance (it is not needed when user is removed)
                IsChatAdmin = ChatUserHelper.HasChatUserPermission(chatUser, ChatPermissionEnum.ManageRooms);
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Chat user.
        /// </summary>
        public ChatUserInfo ChatUser { get; private set; }

        #endregion


        #region "Data members"

        /// <summary>
        /// Gets nickname of this user.
        /// </summary>
        [DataMember]
        public string Nickname
        {
            get
            {
                return ChatUser.ChatUserNickname;
            }
            set
            {
                ChatUser.ChatUserNickname = value;
            }
        }


        /// <summary>
        /// Gets ID of this user.
        /// </summary>
        [DataMember]
        public int ChatUserID
        {
            get
            {
                return ChatUser.ChatUserID;
            }
            set
            {
                ChatUser.ChatUserID = value;
            }
        }


        /// <summary>
        /// Gets IsAnonymous state of this user. True if user is anonymous (has not cms user associated with it).
        /// </summary>
        [DataMember]
        public bool IsAnonymous
        {
            get
            {
                return ChatUser.IsAnonymous;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// True if user is online.
        /// </summary>
        [DataMember]
        public bool IsOnline { get; set; }


        /// <summary>
        /// Is admin in 'this' room.
        /// </summary>
        [DataMember]
        public AdminLevelEnum AdminLevel { get; set; }


        /// <summary>
        /// Is global chat admin (has ManageRooms).
        /// </summary>
        [DataMember]
        public bool IsChatAdmin { get; set; }


        /// <summary>
        /// True if this user should be removed from list on client
        /// </summary>
        [DataMember]
        public bool IsRemoved { get; set; }

        #endregion


        #region IChatCacheableWithCurrentState Members

        /// <summary>
        /// Change type.
        /// </summary>
        ChangeTypeEnum IChatCacheableWithCurrentState<int>.ChangeType
        {
            get
            {
                if (IsRemoved == true)
                {
                    return ChangeTypeEnum.Remove;
                }

                return ChangeTypeEnum.Modify;
            }
        }


        /// <summary>
        /// Primary key
        /// </summary>
        public int PK
        {
            get 
            {
                return ChatUserID;
            }
        }

        #endregion


        #region IChatIncrementalCacheable Members

        /// <summary>
        /// Last modification of this user.
        /// </summary>
        public DateTime ChangeTime { get; set; }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns string representation.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return string.Format("Nick: {0}, Is removed: {1}, LastChange: {2}, Is online: {3}, Admin level: {4}", Nickname, IsRemoved, ChangeTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), IsOnline, AdminLevel.ToString());
        }

        #endregion
    }
}

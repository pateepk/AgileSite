using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Online user data.
    /// </summary>
    [DataContract]
    public class OnlineUserData : IChatCacheableWithCurrentState<int>
    {
        #region "Data members"

        /// <summary>
        /// Gets or sets nickname of this user.
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
        /// Gets IsAnonymous state of this user. True if user is anonym (has not cms user associated with it).
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
        /// True if this online user should be removed from list on client.
        /// </summary>
        [DataMember]
        public bool IsRemoved { get; set; }

        #endregion


        #region "Public properties"

        /// <summary>
        /// ChatUserInfo of this online user.
        /// </summary>
        public ChatUserInfo ChatUser { get; private set; }


        /// <summary>
        /// If true, this record shouldn't be sent to client - it will be only stored on server side.
        /// </summary>
        public bool IsHidden { get; private set; }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructs OnlineUserData.
        /// </summary>
        /// <param name="chatUser">Info of this chat user.</param>
        /// <param name="lastChange">Last change</param>
        /// <param name="isRemoved">If true, this record should be removed from list on client</param>
        /// <param name="isHidden">If true, this record shouldn't be sent to client - it will be only stored on server side</param>
        public OnlineUserData(ChatUserInfo chatUser, DateTime lastChange, bool isRemoved, bool isHidden)
        {
            ChatUser = chatUser;

            IsRemoved = isRemoved;
            ChangeTime = lastChange;
            IsHidden = isHidden;
        }

        #endregion


        #region IChatCacheableWithCurrentState Members

        /// <summary>
        /// Change type of this record.
        /// </summary>
        ChangeTypeEnum IChatCacheableWithCurrentState<int>.ChangeType
        {
            get 
            {
                if (IsRemoved)
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
        /// Last change
        /// </summary>
        public DateTime ChangeTime { get; set; }

        #endregion
    }
}

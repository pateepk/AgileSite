using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatRoomUserInfo), ChatRoomUserInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatRoomUser data container class.
    /// </summary>
    public class ChatRoomUserInfo : AbstractInfo<ChatRoomUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.roomuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatRoomUserInfoProvider), OBJECT_TYPE, "Chat.RoomUser", "ChatRoomUserID", null, null, null, null, null, null, "ChatRoomUserRoomID", ChatRoomInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("ChatRoomUserChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            IsBinding = true,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Room ID.
        /// </summary>
        public virtual int ChatRoomUserRoomID
        {
            get
            {
                return GetIntegerValue("ChatRoomUserRoomID", 0);
            }
            set
            {         
                SetValue("ChatRoomUserRoomID", value);
            }
        }


        /// <summary>
        /// Datetime of last checking.
        /// </summary>
        public virtual DateTime? ChatRoomUserLastChecking
        {
            get
            {
                object value = GetValue("ChatRoomUserLastChecking");
                if (value != null)
                {
                    return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
                }
                return null;
            }
            set
            {         
                SetValue("ChatRoomUserLastChecking", value);
            }
        }


        /// <summary>
        /// ID.
        /// </summary>
        public virtual int ChatRoomUserID
        {
            get
            {
                return GetIntegerValue("ChatRoomUserID", 0);
            }
            set
            {         
                SetValue("ChatRoomUserID", value);
            }
        }


        /// <summary>
        /// Chat user ID.
        /// </summary>
        public virtual int ChatRoomUserChatUserID
        {
            get
            {
                return GetIntegerValue("ChatRoomUserChatUserID", 0);
            }
            set
            {         
                SetValue("ChatRoomUserChatUserID", value);
            }
        }


        /// <summary>
        /// DateTime when kick will expire.
        /// 
        /// If set to null, user was not kicked.
        /// </summary>
        public virtual DateTime? ChatRoomUserKickExpiration
        {
            get
            {
                object value = GetValue("ChatRoomUserKickExpiration");
                if (value != null)
                {
                    return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
                }
                return null;
            }
            set
            {
                SetValue("ChatRoomUserKickExpiration", value);
            }
        }

        
        /// <summary>
        /// DateTime when user left the room.
        /// 
        /// If set to null, user is online.
        /// </summary>
        public virtual DateTime? ChatRoomUserLeaveTime
        {
            get
            {
                object value = GetValue("ChatRoomUserLeaveTime");
                if (value != null)
                {
                    return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
                }
                return null;
            }
            set
            {
                SetValue("ChatRoomUserLeaveTime", value);
            }
        }


        /// <summary>
        /// DateTime when user joined room.
        /// 
        /// If set to null, user has left.
        /// </summary>
        public virtual DateTime? ChatRoomUserJoinTime
        {
            get
            {
                object value = GetValue("ChatRoomUserJoinTime");
                if (value != null)
                {
                    return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
                }
                return null;
            }
            set
            {
                SetValue("ChatRoomUserJoinTime", value);
            }
        }

        /// <summary>
        /// Chat admin level.
        /// </summary>
        public virtual AdminLevelEnum ChatRoomUserAdminLevel
        {
            get
            {
                return ChatHelper.GetEnum(GetIntegerValue("ChatRoomUserAdminLevel", -1), AdminLevelEnum.None);
            }
            set
            {
                SetValue("ChatRoomUserAdminLevel", (int)value);
            }
        }


        /// <summary>
        /// Last modification.
        /// </summary>
        public virtual DateTime ChatRoomUserLastModification
        {
            get
            {
                return GetDateTimeValue("ChatRoomUserLastModification", DateTime.MinValue);
            }
            set
            {
                SetValue("ChatRoomUserLastModification", value);
            }
        }


        /// <summary>
        /// Gets online state of this user. User is online if his JoinTime is not null.
        /// </summary>
        public bool IsOnline
        {
            get
            {
                return ChatRoomUserJoinTime.HasValue;
            }
        }


        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatRoomUserInfoProvider.DeleteChatRoomUser(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatRoomUserInfoProvider.SetChatRoomUser(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatRoomUser object.
        /// </summary>
        public ChatRoomUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatRoomUser object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatRoomUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

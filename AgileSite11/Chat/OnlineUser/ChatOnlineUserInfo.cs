using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatOnlineUserInfo), ChatOnlineUserInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatOnlineUser data container class.
    /// </summary>
    public class ChatOnlineUserInfo : AbstractInfo<ChatOnlineUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.onlineuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatOnlineUserInfoProvider), OBJECT_TYPE, "Chat.OnlineUser", "ChatOnlineUserID", null, null, null, null, null, "ChatOnlineUserSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("ChatOnlineUserChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            MacroCollectionName = "OnlineChatUser",
            ContainsMacros = false,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of chat online user.
        /// </summary>
        public virtual int ChatOnlineUserChatUserID
        {
            get
            {
                return GetIntegerValue("ChatOnlineUserChatUserID", 0);
            }
            set
            {
                SetValue("ChatOnlineUserChatUserID", value);
            }
        }


        /// <summary>
        /// Time of last ping.
        /// </summary>
        public virtual DateTime? ChatOnlineUserLastChecking
        {
            get
            {
                object value = GetValue("ChatOnlineUserLastChecking");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatOnlineUserLastChecking", value);
            }
        }


        /// <summary>
        /// ID of chat online user.
        /// </summary>
        public virtual int ChatOnlineUserID
        {
            get
            {
                return GetIntegerValue("ChatOnlineUserID", 0);
            }
            set
            {
                SetValue("ChatOnlineUserID", value);
            }
        }


        /// <summary>
        /// Site where user is online.
        /// </summary>
        public virtual int ChatOnlineUserSiteID
        {
            get
            {
                return GetIntegerValue("ChatOnlineUserSiteID", 0);
            }
            set
            {
                SetValue("ChatOnlineUserSiteID", value);
            }
        }


        /// <summary>
        /// Site where user is online.
        /// </summary>
        public virtual DateTime? ChatOnlineUserLeaveTime
        {
            get
            {
                object value = GetValue("ChatOnlineUserLeaveTime");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatOnlineUserLeaveTime", value);
            }
        }


        /// <summary>
        /// Site where user is online.
        /// </summary>
        public virtual DateTime? ChatOnlineUserJoinTime
        {
            get
            {
                object value = GetValue("ChatOnlineUserJoinTime");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatOnlineUserJoinTime", value);
            }
        }


        /// <summary>
        /// Unique token.
        /// </summary>
        public virtual string ChatOnlineUserToken
        {
            get
            {
                return GetStringValue("ChatOnlineUserToken", null);
            }
            set
            {
                SetValue("ChatOnlineUserToken", value);
            }
        }


        /// <summary>
        /// True if user shouldn't be displayed on live site in list of online users.
        /// </summary>
        public virtual bool ChatOnlineUserIsHidden
        {
            get
            {
                return GetBooleanValue("ChatOnlineUserIsHidden", false);
            }
            set
            {
                SetValue("ChatOnlineUserIsHidden", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatOnlineUserInfoProvider.DeleteChatOnlineUser(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatOnlineUserInfoProvider.SetChatOnlineUser(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatOnlineUser object.
        /// </summary>
        public ChatOnlineUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatOnlineUser object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatOnlineUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

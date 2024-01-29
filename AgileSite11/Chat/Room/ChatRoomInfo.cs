using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatRoomInfo), ChatRoomInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatRoomInfo data container class.
    /// </summary>
    public class ChatRoomInfo : AbstractInfo<ChatRoomInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.room";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatRoomInfoProvider), OBJECT_TYPE, "Chat.Room", "ChatRoomID", "ChatRoomLastModification", "ChatRoomGUID", "ChatRoomName", "ChatRoomDisplayName", null, "ChatRoomSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("ChatRoomCreatedByChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired) },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = false,
            SupportsVersioning = false,
            AllowRestore = false,
            SupportsGlobalObjects = true,
            ModuleName = ModuleName.CHAT,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                LogExport = true,
                IsExportable = true,
                WhereCondition = "(ChatRoomPrivate = 0 AND ChatRoomIsOneToOne = 0 AND ChatRoomScheduledToDelete IS NULL)",
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY),
                },
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "ChatRoomEnabled",
                    "ChatRoomLastSupportVisit",
                    "ChatRoomPrivateStateLastModification",
                    "ChatRoomCreatedWhen",
                    "ChatRoomScheduledToDelete",
                    "ChatRoomCreatedByChatUserID"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                FilterCondition = new WhereCondition().WhereEquals("ChatRoomIsOneToOne", 0).And().WhereEquals("ChatRoomPrivate", 0)
            },
            MacroCollectionName = "ChatRoom",
            EnabledColumn = "ChatRoomEnabled",
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Log export only if room meets export criteria. Only public rooms are meant to be exported.
        /// </summary>
        protected override bool LogExport
        {
            get
            {
                return base.LogExport && !ChatRoomPrivate && !ChatRoomIsOneToOne;
            }
            set
            {
                base.LogExport = value;
            }
        }


        /// <summary>
        /// System name.
        /// </summary>
        public virtual string ChatRoomName
        {
            get
            {
                return GetStringValue("ChatRoomName", "");
            }
            set
            {
                SetValue("ChatRoomName", value);
            }
        }


        /// <summary>
        /// True, if this room can be entered by anonyms (users which are not registered or logged in).
        /// </summary>
        public virtual bool ChatRoomAllowAnonym
        {
            get
            {
                return GetBooleanValue("ChatRoomAllowAnonym", false);
            }
            set
            {
                SetValue("ChatRoomAllowAnonym", value);
            }
        }

        /// <summary>
        /// True, if this room is created as one to one chat.
        /// </summary>
        public virtual bool ChatRoomIsOneToOne
        {
            get
            {
                return GetBooleanValue("ChatRoomIsOneToOne", false);
            }
            set
            {
                SetValue("ChatRoomIsOneToOne", value);
            }
        }


        /// <summary>
        /// If false, this room is not accessible.
        /// </summary>
        public virtual bool ChatRoomEnabled
        {
            get
            {
                return GetBooleanValue("ChatRoomEnabled", false);
            }
            set
            {
                SetValue("ChatRoomEnabled", value);
            }
        }


        /// <summary>
        /// If room is private, it is not visible in rooms list.
        /// </summary>
        public virtual bool ChatRoomPrivate
        {
            get
            {
                return GetBooleanValue("ChatRoomPrivate", false);
            }
            set
            {
                SetValue("ChatRoomPrivate", value);
            }
        }


        /// <summary>
        /// Site to which this room belongs (null = global).
        /// </summary>
        public virtual int? ChatRoomSiteID
        {
            get
            {
                object val = GetValue("ChatRoomSiteID");
                if (val == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(val, 0);
            }
            set
            {
                SetValue("ChatRoomSiteID", value);
            }
        }


        /// <summary>
        /// Password of a room.
        /// </summary>
        public virtual string ChatRoomPassword
        {
            get
            {
                return GetStringValue("ChatRoomPassword", "");
            }
            set
            {
                SetValue("ChatRoomPassword", value);
            }
        }


        /// <summary>
        /// Gets true if password is needed to enter this room.
        /// </summary>
        public bool HasPassword
        {
            get
            {
                return !string.IsNullOrEmpty(ChatRoomPassword);
            }
        }


        /// <summary>
        /// True if room is support and one to one. This aplies to rooms created by 'I need help' button on live site.
        /// </summary>
        public bool IsOneToOneSupport
        {
            get
            {
                return ChatRoomIsOneToOne && ChatRoomIsSupport;
            }
        }


        /// <summary>
        /// True if this room is for whispering (OneToOne is true and IsSupport is false).
        /// </summary>
        public bool IsWhisperRoom
        {
            get
            {
                return ChatRoomIsOneToOne && !ChatRoomIsSupport;
            }
        }


        /// <summary>
        /// Datetime when this chat room was created.
        /// </summary>
        public virtual DateTime ChatRoomCreatedWhen
        {
            get
            {
                return GetDateTimeValue("ChatRoomCreatedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatRoomCreatedWhen", value);
            }
        }

        /// <summary>
        /// ID of creator of this room.
        /// </summary>
        public virtual int? ChatRoomCreatedByChatUserID
        {
            get
            {
                object val = GetValue("ChatRoomCreatedByChatUserID");

                if (val == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(val, 0);
            }
            set
            {
                SetValue("ChatRoomCreatedByChatUserID", value);
            }
        }


        /// <summary>
        /// ID of chat room.
        /// </summary>
        public virtual int ChatRoomID
        {
            get
            {
                return GetIntegerValue("ChatRoomID", 0);
            }
            set
            {
                SetValue("ChatRoomID", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string ChatRoomDisplayName
        {
            get
            {
                return GetStringValue("ChatRoomDisplayName", "");
            }
            set
            {
                SetValue("ChatRoomDisplayName", value);
            }
        }



        /// <summary>
        /// True, if supporters will follow this room.
        /// </summary>
        public virtual bool ChatRoomIsSupport
        {
            get
            {
                return GetBooleanValue("ChatRoomIsSupport", false);
            }
            set
            {
                SetValue("ChatRoomIsSupport", value);
            }
        }


        /// <summary>
        /// True, if any supporter is already taking care of this room.
        /// </summary>
        public virtual DateTime? ChatRoomLastSupportVisit
        {
            get
            {
                object chatRoomLastSupportVisit = GetValue("ChatRoomLastSupportVisit");
                if (chatRoomLastSupportVisit != null)
                {
                    return ValidationHelper.GetDateTime(chatRoomLastSupportVisit, DateTime.Now);
                }
                return null;
            }
            set
            {
                SetValue("ChatRoomLastSupportVisit", value);
            }
        }


        /// <summary>
        /// Decription of the chat room.
        /// </summary>
        public virtual string ChatRoomDescription
        {
            get
            {
                return GetStringValue("ChatRoomDescription", "");
            }
            set
            {
                SetValue("ChatRoomDescription", value);
            }
        }


        /// <summary>
        /// Datetime when this chat room was created.
        /// </summary>
        public virtual DateTime ChatRoomLastModification
        {
            get
            {
                return GetDateTimeValue("ChatRoomLastModification", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatRoomLastModification", value);
            }
        }


        /// <summary>
        /// True, if any supporter is already taking care of this room.
        /// </summary>
        public virtual DateTime? ChatRoomScheduledToDelete
        {
            get
            {
                object value = GetValue("ChatRoomScheduledToDelete");
                if (value != null)
                {
                    return ValidationHelper.GetDateTime(value, DateTime.Now);
                }
                return null;
            }
            set
            {
                SetValue("ChatRoomScheduledToDelete", value);
            }
        }


        /// <summary>
        /// Datetime when this chat room was created.
        /// </summary>
        public virtual DateTime ChatRoomPrivateStateLastModification
        {
            get
            {
                return GetDateTimeValue("ChatRoomPrivateStateLastModification", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatRoomPrivateStateLastModification", value);
            }
        }


        /// <summary>
        /// Unique identifier of this chat orom.
        /// </summary>
        public virtual Guid ChatRoomGUID
        {
            get
            {
                return GetGuidValue("ChatRoomGUID", Guid.Empty);
            }
            set
            {
                SetValue("ChatRoomGUID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatRoomInfoProvider.DeleteChatRoomInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatRoomInfoProvider.SetChatRoomInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatRoomInfo object.
        /// </summary>
        public ChatRoomInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatRoomInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatRoomInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets CreatedWhen and LastModification to the current time.
        /// 
        /// This method is called on cloned object prepared in memory by InsertAsClone method. 
        /// Override if you need to do further actions before inserting actual object to DB (insert special objects, modify foreign keys, copy files, etc.).
        /// Calls Insert() by default.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            ChatRoomCreatedWhen = ChatRoomLastModification = DateTime.Now; // GETDATE() will be used on SQL Server side

            Insert();
        }


        /// <summary>
        /// Override use of classic query for updating data.
        /// 
        /// Properly handles changing from public to private state.
        /// 
        /// Invalidates rooms cache.
        /// </summary>
        protected override void UpdateData()
        {
            bool originalChatRoomPrivate = ValidationHelper.GetBoolean(GetOriginalValue("ChatRoomPrivate"), false);
            bool originalChatRoomAllowAnonym = ValidationHelper.GetBoolean(GetOriginalValue("ChatRoomAllowAnonym"), false);

            bool privateStateChanged = (originalChatRoomPrivate != ChatRoomPrivate);
            bool changedToDisallowAnonyms = (originalChatRoomAllowAnonym && !ChatRoomAllowAnonym);

            base.UpdateData();

            // Handle private state change
            if (privateStateChanged)
            {
                bool changedToPublic = !ChatRoomPrivate;
                bool changedToPrivate = ChatRoomPrivate;

                IEnumerable<ChatRoomUserInfo> usersInRoom = ChatRoomUserInfoProvider.GetChatRoomUsersByRoomID(ChatRoomID);

                foreach (ChatRoomUserInfo userInRoom in usersInRoom)
                {
                    // Changing room to public. Change permissions from Join to None (Join is not valid in public rooms).
                    if (changedToPublic)
                    {
                        if (userInRoom.ChatRoomUserAdminLevel == AdminLevelEnum.Join)
                        {
                            ChatRoomUserHelper.SetChatAdminLevel(ChatRoomID, userInRoom.ChatRoomUserChatUserID, AdminLevelEnum.None);
                        }
                    }
                    // Setting room to private. Give join rights to everyone who is online in this room right now
                    else
                    {
                        if ((userInRoom.ChatRoomUserAdminLevel == AdminLevelEnum.None) && userInRoom.IsOnline)
                        {
                            ChatRoomUserHelper.SetChatAdminLevel(ChatRoomID, userInRoom.ChatRoomUserChatUserID, AdminLevelEnum.Join);
                        }
                    }
                }


                // If room was changed to private, ensure that current user has at least join rights
                if (changedToPrivate && ChatOnlineUserHelper.IsChatUserLoggedIn())
                {
                    ChatRoomUserHelper.IncreaseChatAdminLevel(ChatRoomID, ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID, AdminLevelEnum.Join);
                }
            }

            // Handle change from allow to disallow anonymous users
            if (changedToDisallowAnonyms)
            {
                IEnumerable<ChatRoomUserInfo> usersInRoom = ChatRoomUserInfoProvider.GetAnonymousChatRoomUsersByRoomID(ChatRoomID);

                foreach (ChatRoomUserInfo userInRoom in usersInRoom)
                {
                    // Remove all rights from anonymous users
                    if (userInRoom.ChatRoomUserAdminLevel != AdminLevelEnum.None)
                    {
                        ChatRoomUserHelper.SetChatAdminLevel(ChatRoomID, userInRoom.ChatRoomUserChatUserID, AdminLevelEnum.None);
                    }

                    // 'Leave' all anonymous users who are online in this room
                    if (userInRoom.IsOnline)
                    {
                        ChatRoomUserHelper.LeaveRoom(ChatRoomID, userInRoom.ChatRoomUserChatUserID);
                    }
                }
            }

            ChatRoomInfoProvider.UpdateCachedRooms();
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ChatRoomEnabled = true;
        }

        #endregion
    }
}

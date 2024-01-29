using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatSupportTakenRoomInfo), ChatSupportTakenRoomInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatSupportTakenRoomsInfo data container class.
    /// </summary>
    public class ChatSupportTakenRoomInfo : AbstractInfo<ChatSupportTakenRoomInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.supporttakenroom";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatSupportTakenRoomInfoProvider), OBJECT_TYPE, "Chat.SupportTakenRoom", "ChatSupportTakenRoomID", "ChatSupportTakenRoomLastModification", null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>() { 
                new ObjectDependency("ChatSupportTakenRoomChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ChatSupportTakenRoomRoomID", ChatRoomInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            MacroCollectionName = "ChatSupportTakenRoom",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID.
        /// </summary>
        public virtual int ChatSupportTakenRoomID
        {
            get
            {
                return GetIntegerValue("ChatSupportTakenRoomID", 0);
            }
            set
            {         
                SetValue("ChatSupportTakenRoomID", value);
            }
        }


        /// <summary>
        /// ID of a chat room.
        /// </summary>
        public virtual int? ChatSupportTakenRoomRoomID
        {
            get
            {
                object value = GetValue("ChatSupportTakenRoomRoomID");

                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetInteger(value, 0);
            }
            set
            {         
                SetValue("ChatSupportTakenRoomRoomID", value);
            }
        }


        /// <summary>
        /// DateTime when support engineer took this room.
        /// </summary>
        public virtual DateTime ChatSupportTakenRoomLastModification
        {
            get
            {
                return GetDateTimeValue("ChatSupportTakenRoomLastModification", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatSupportTakenRoomLastModification", value);
            }
        }


        /// <summary>
        /// ID of a row in table Chat_OnlineSupport.
        /// </summary>
        public virtual int? ChatSupportTakenRoomChatUserID
        {
            get
            {
                object value = GetValue("ChatSupportTakenRoomChatUserID");

                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetInteger(value, 0);
            }
            set
            {
                SetValue("ChatSupportTakenRoomChatUserID", value);
            }
        }


        /// <summary>
        /// DateTime when this room was resolved for the last time. Room is resolved when support engineer leaves room.
        /// </summary>
        public virtual DateTime? ChatSupportTakenRoomResolvedDateTime
        {
            get
            {
                object value = GetValue("ChatSupportTakenRoomResolvedDateTime");

                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatSupportTakenRoomResolvedDateTime", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatSupportTakenRoomInfoProvider.DeleteChatSupportTakenRoomsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatSupportTakenRoomInfoProvider.SetChatSupportTakenRoomsInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatSupportTakenRoomsInfo object.
        /// </summary>
        public ChatSupportTakenRoomInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatSupportTakenRoomsInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatSupportTakenRoomInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

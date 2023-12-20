using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatNotificationInfo), ChatNotificationInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatNotificationInfo data container class.
    /// </summary>
    public class ChatNotificationInfo : AbstractInfo<ChatNotificationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.notification";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatNotificationInfoProvider), OBJECT_TYPE, "Chat.Notification", "ChatNotificationID", null, null, null, null, null, "ChatNotificationSiteID", "ChatNotificationReceiverID", ChatUserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { 
                new ObjectDependency("ChatNotificationRoomID", ChatRoomInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ChatNotificationSenderID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) 
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            SupportsGlobalObjects = true,
            ModuleName = ModuleName.CHAT,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of chat notification.
        /// </summary>
        public virtual int ChatNotificationID
        {
            get
            {
                return GetIntegerValue("ChatNotificationID", 0);
            }
            set
            {         
                SetValue("ChatNotificationID", value);
            }
        }


        /// <summary>
        /// Receiver of this notification. This one will see it.
        /// </summary>
        public virtual int ChatNotificationReceiverID
        {
            get
            {
                return GetIntegerValue("ChatNotificationReceiverID", 0);
            }
            set
            {         
                SetValue("ChatNotificationReceiverID", value);
            }
        }


        /// <summary>
        /// True if this notification was read, otherwise false.
        /// </summary>
        public virtual bool ChatNotificationIsRead
        {
            get
            {
                return GetBooleanValue("ChatNotificationIsRead", false);
            }
            set
            {         
                SetValue("ChatNotificationIsRead", value);
            }
        }


        /// <summary>
        /// Room of this notification (optional).
        /// </summary>
        public virtual int? ChatNotificationRoomID
        {
            get
            {
                object value = GetValue("ChatNotificationRoomID");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(value, 0);
            }
            set
            {         
                SetValue("ChatNotificationRoomID", value, (value > 0));
            }
        }


        /// <summary>
        /// Sender (issuer) of this notification.
        /// </summary>
        public virtual int ChatNotificationSenderID
        {
            get
            {
                return GetIntegerValue("ChatNotificationSenderID", 0);
            }
            set
            {         
                SetValue("ChatNotificationSenderID", value);
            }
        }


        /// <summary>
        /// Type of notification.
        /// </summary>
        public virtual ChatNotificationTypeEnum ChatNotificationType
        {
            get
            {
                return (ChatNotificationTypeEnum)GetIntegerValue("ChatNotificationType", 0);
            }
            set
            {         
                SetValue("ChatNotificationType", (int)value);
            }
        }


        /// <summary>
        /// DateTime of send.
        /// </summary>
        public virtual DateTime ChatNotificationSendDateTime
        {
            get
            {
                return GetDateTimeValue("ChatNotificationSendDateTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatNotificationSendDateTime", value);
            }
        }


        /// <summary>
        /// DateTime of send.
        /// </summary>
        public virtual DateTime? ChatNotificationReadDateTime
        {
            get
            {
                object value = GetValue("ChatNotificationReadDateTime");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ChatNotificationReadDateTime", value);
            }
        }


        /// <summary>
        /// SiteID of this notification. If null than global.
        /// </summary>
        public virtual int? ChatNotificationSiteID
        {
            get
            {
                object value = GetValue("ChatNotificationSiteID");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(value, 0);
            }
            set
            {
                SetValue("ChatNotificationSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatNotificationInfoProvider.DeleteChatNotificationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatNotificationInfoProvider.SetChatNotificationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatNotificationInfo object.
        /// </summary>
        public ChatNotificationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatNotificationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatNotificationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

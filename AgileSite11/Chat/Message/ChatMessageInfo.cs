using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatMessageInfo), ChatMessageInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatMessageInfo data container class.
    /// </summary>
    public class ChatMessageInfo : AbstractInfo<ChatMessageInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.message";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatMessageInfoProvider), OBJECT_TYPE, "Chat.Message", "ChatMessageID", "ChatMessageLastModified", null, null, "ChatMessageText", null, null, "ChatMessageRoomID", ChatRoomInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { 
                new ObjectDependency("ChatMessageUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
                new ObjectDependency("ChatMessageRecipientID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired) },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsCloning = false,
            AllowRestore = false,
            UpdateTimeStamp = true,
            ModuleName = ModuleName.CHAT,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Chat user Author's ID - foreign key.
        /// </summary>
        public virtual int? ChatMessageUserID
        {
            get
            {
                object value = GetValue("ChatMessageUserID");
                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }
                return null;
            }
            set
            {         
                SetValue("ChatMessageUserID", value);
            }
        }


        /// <summary>
        /// Recipient's ID.
        /// </summary>
        public virtual int? ChatMessageRecipientID
        {
            get
            {
                return GetIntegerValue("ChatMessageRecipientID", 0);
            }
            set
            {
                SetValue("ChatMessageRecipientID", value);
            }
        }


        /// <summary>
        /// Room's ID - foreign key.
        /// </summary>
        public virtual int ChatMessageRoomID
        {
            get
            {
                return GetIntegerValue("ChatMessageRoomID", 0);
            }
            set
            {         
                SetValue("ChatMessageRoomID", value);
            }
        }


        /// <summary>
        /// Chat message ID - Primary key.
        /// </summary>
        public virtual int ChatMessageID
        {
            get
            {
                return GetIntegerValue("ChatMessageID", 0);
            }
            set
            {         
                SetValue("ChatMessageID", value);
            }
        }


        /// <summary>
        /// Timestamp when was message created.
        /// </summary>
        public virtual DateTime ChatMessageCreatedWhen
        {
            get
            {
                return GetDateTimeValue("ChatMessageCreatedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("ChatMessageCreatedWhen", value);
            }
        }


        /// <summary>
        /// Sender's IP address.
        /// </summary>
        public virtual string ChatMessageIPAddress
        {
            get
            {
                return GetStringValue("ChatMessageIPAddress", "");
            }
            set
            {         
                SetValue("ChatMessageIPAddress", value);
            }
        }


        /// <summary>
        /// User's message.
        /// </summary>
        public virtual string ChatMessageText
        {
            get
            {
                return GetStringValue("ChatMessageText", "");
            }
            set
            {
                SetValue("ChatMessageText", value);
            }
        }


        /// <summary>
        /// Is message rejected.
        /// </summary>
        public virtual bool ChatMessageRejected
        {
            get
            {
                return GetBooleanValue("ChatMessageRejected", false);
            }
            set
            {
                SetValue("ChatMessageRejected", value);
            }
        }


        /// <summary>
        /// DateTime of last modification.
        /// </summary>
        public virtual DateTime ChatMessageLastModified
        {
            get
            {
                return GetDateTimeValue("ChatMessageLastModified", DateTime.Now);
            }
            set
            {
                SetValue("ChatMessageLastModified", value);
            }
        }

        
        /// <summary>
        /// Is message system.
        /// </summary>
        public virtual ChatMessageTypeEnum ChatMessageSystemMessageType
        {
            get
            {
                return ChatHelper.GetEnum(GetIntegerValue("ChatMessageSystemMessageType", 0), ChatMessageTypeEnum.ClassicMessage);
            }
            set
            {
                SetValue("ChatMessageSystemMessageType", (int)value);
            }
        }
        

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatMessageInfoProvider.DeleteChatMessageInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatMessageInfoProvider.SetChatMessageInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatMessageInfo object.
        /// </summary>
        public ChatMessageInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatMessageInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatMessageInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

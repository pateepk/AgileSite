using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatInitiatedChatRequestInfo), ChatInitiatedChatRequestInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatInitiatedChatRequestInfo data container class.
    /// </summary>
    public class ChatInitiatedChatRequestInfo : AbstractInfo<ChatInitiatedChatRequestInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.initiatedchatrequest";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatInitiatedChatRequestInfoProvider), OBJECT_TYPE, "Chat.InitiatedChatRequest", "InitiatedChatRequestID", "InitiatedChatRequestLastModification", null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>() 
            { 
                new ObjectDependency("InitiatedChatRequestContactID", PredefinedObjectType.CONTACT, ObjectDependencyEnum.NotRequired),
                new ObjectDependency("InitiatedChatRequestUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
                new ObjectDependency("InitiatedChatRequestRoomID", ChatRoomInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("InitiatedChatRequestInitiatorChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            ImportExportSettings = { LogExport = true }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Time of last modification of this request.
        /// </summary>
        public virtual DateTime InitiatedChatRequestLastModification
        {
            get
            {
                return GetDateTimeValue("InitiatedChatRequestLastModification", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("InitiatedChatRequestLastModification", value);
            }
        }


        /// <summary>
        /// Conversation will take place in this room.
        /// </summary>
        public virtual int InitiatedChatRequestRoomID
        {
            get
            {
                return GetIntegerValue("InitiatedChatRequestRoomID", 0);
            }
            set
            {
                SetValue("InitiatedChatRequestRoomID", value);
            }
        }


        /// <summary>
        /// Request is meant for user with this ID. If it is null, ContactID is used.
        /// </summary>
        public virtual int? InitiatedChatRequestUserID
        {
            get
            {
                object value = GetValue("InitiatedChatRequestUserID");

                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }

                return null;
            }
            set
            {
                SetValue("InitiatedChatRequestUserID", value, value.HasValue && (value.Value > 0));
            }
        }


        /// <summary>
        /// Unique identifier of this request.
        /// </summary>
        public virtual int InitiatedChatRequestID
        {
            get
            {
                return GetIntegerValue("InitiatedChatRequestID", 0);
            }
            set
            {
                SetValue("InitiatedChatRequestID", value);
            }
        }


        /// <summary>
        /// Friendly name of initiator of this chat (usually it will be just nickanme of user who started inserted this request).
        /// </summary>
        public virtual string InitiatedChatRequestInitiatorName
        {
            get
            {
                return GetStringValue("InitiatedChatRequestInitiatorName", "");
            }
            set
            {
                SetValue("InitiatedChatRequestInitiatorName", value);
            }
        }


        /// <summary>
        /// Chat user who inserted this request.
        /// </summary>
        public virtual int InitiatedChatRequestInitiatorChatUserID
        {
            get
            {
                return GetIntegerValue("InitiatedChatRequestInitiatorChatUserID", 0);
            }
            set
            {
                SetValue("InitiatedChatRequestInitiatorChatUserID", value);
            }
        }


        /// <summary>
        /// Request is meant for contact with this ID. If it is null, UserID is used.
        /// </summary>
        public virtual int? InitiatedChatRequestContactID
        {
            get
            {
                object value = GetValue("InitiatedChatRequestContactID");

                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }

                return null;
            }
            set
            {
                SetValue("InitiatedChatRequestContactID", value, value.HasValue && (value.Value > 0));
            }
        }


        /// <summary>
        /// State of this request.
        /// </summary>
        public virtual InitiatedChatRequestStateEnum InitiatedChatRequestState
        {
            get
            {
                return ChatHelper.GetEnum(GetIntegerValue("InitiatedChatRequestState", 0), InitiatedChatRequestStateEnum.New);
            }
            set
            {
                SetValue("InitiatedChatRequestState", (int)value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatInitiatedChatRequestInfoProvider.DeleteChatInitiatedChatRequestInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatInitiatedChatRequestInfoProvider.SetChatInitiatedChatRequestInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatInitiatedChatRequestInfo object.
        /// </summary>
        public ChatInitiatedChatRequestInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatInitiatedChatRequestInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatInitiatedChatRequestInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

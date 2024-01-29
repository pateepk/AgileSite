using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Messaging;

[assembly: RegisterObjectType(typeof(MessageInfo), MessageInfo.OBJECT_TYPE)]

namespace CMS.Messaging
{
    /// <summary>
    /// MessageInfo data container class.
    /// </summary>
    public class MessageInfo : AbstractInfo<MessageInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "messaging.message";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MessageInfoProvider), OBJECT_TYPE, "Messaging.Message", "MessageID", "MessageLastModified", "MessageGUID", null, "MessageSubject", null, null, null, null)
                                              {
                                                  TouchCacheDependencies = true,
                                                  DependsOn = new List<ObjectDependency>()
                                                  {
                                                      new ObjectDependency("MessageSenderUserID", UserInfo.OBJECT_TYPE), 
                                                      new ObjectDependency("MessageRecipientUserID", UserInfo.OBJECT_TYPE),
                                                  },
                                                  ModuleName = "cms.messaging",
                                                  SupportsCloning = false
                                              };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Message ID.
        /// </summary>
        public virtual int MessageID
        {
            get
            {
                return GetIntegerValue("MessageID", 0);
            }
            set
            {
                SetValue("MessageID", value);
            }
        }


        /// <summary>
        /// Date when the message was sent.
        /// </summary>
        public virtual DateTime MessageSent
        {
            get
            {
                return GetDateTimeValue("MessageSent", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MessageSent", value);
            }
        }


        /// <summary>
        /// Date when the message was read.
        /// </summary>
        public virtual DateTime MessageRead
        {
            get
            {
                return GetDateTimeValue("MessageRead", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MessageRead", value);
            }
        }


        /// <summary>
        /// True if sender deletes the message.
        /// </summary>
        public virtual bool MessageSenderDeleted
        {
            get
            {
                return GetBooleanValue("MessageSenderDeleted", false);
            }
            set
            {
                SetValue("MessageSenderDeleted", value);
            }
        }


        /// <summary>
        /// True if recipient deletes the message.
        /// </summary>
        public bool MessageRecipientDeleted
        {
            get
            {
                return GetBooleanValue("MessageRecipientDeleted", false);
            }
            set
            {
                SetValue("MessageRecipientDeleted", value);
            }
        }


        /// <summary>
        /// User ID of the sender.
        /// </summary>
        public virtual int MessageSenderUserID
        {
            get
            {
                return GetIntegerValue("MessageSenderUserID", 0);
            }
            set
            {
                SetValue("MessageSenderUserID", value);
            }
        }


        /// <summary>
        /// User ID of the recipient.
        /// </summary>
        public virtual int MessageRecipientUserID
        {
            get
            {
                return GetIntegerValue("MessageRecipientUserID", 0);
            }
            set
            {
                SetValue("MessageRecipientUserID", value);
            }
        }


        /// <summary>
        /// Sender nick name.
        /// </summary>
        public virtual string MessageSenderNickName
        {
            get
            {
                return GetStringValue("MessageSenderNickName", "");
            }
            set
            {
                SetValue("MessageSenderNickName", value);
            }
        }


        /// <summary>
        /// Recipient nick name.
        /// </summary>
        public virtual string MessageRecipientNickName
        {
            get
            {
                return GetStringValue("MessageRecipientNickName", "");
            }
            set
            {
                SetValue("MessageRecipientNickName", value);
            }
        }


        /// <summary>
        /// Subjet of the message.
        /// </summary>
        public virtual string MessageSubject
        {
            get
            {
                return GetStringValue("MessageSubject", "");
            }
            set
            {
                SetValue("MessageSubject", value);
            }
        }


        /// <summary>
        /// Body of the message.
        /// </summary>
        public virtual string MessageBody
        {
            get
            {
                return GetStringValue("MessageBody", "");
            }
            set
            {
                SetValue("MessageBody", value);
            }
        }


        /// <summary>
        /// Message GUID.
        /// </summary>
        public virtual Guid MessageGUID
        {
            get
            {
                return GetGuidValue("MessageGUID", Guid.Empty);
            }
            set
            {
                SetValue("MessageGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime MessageLastModified
        {
            get
            {
                return GetDateTimeValue("MessageLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MessageLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if message is read by the recipient.
        /// </summary>
        public virtual bool MessageIsRead
        {
            get
            {
                return GetBooleanValue("MessageIsRead", false);
            }
            set
            {
                SetValue("MessageIsRead", value, true);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MessageInfoProvider.DeleteMessageInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MessageInfoProvider.SetMessageInfo(this);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            bool allowed = false;
            switch (permission)
            {
                case PermissionsEnum.Read:
                case PermissionsEnum.Delete:
                    allowed = (MessageSenderUserID == userInfo.UserID) || (MessageRecipientUserID == userInfo.UserID);
                    break;

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    allowed = (MessageSenderUserID == userInfo.UserID);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MessageInfo object.
        /// </summary>
        public MessageInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MessageInfo object from the given DataRow.
        /// </summary>
        public MessageInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatSupportCannedResponseInfo), ChatSupportCannedResponseInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatSupportCannedResponseInfo data container class.
    /// </summary>
    public class ChatSupportCannedResponseInfo : AbstractInfo<ChatSupportCannedResponseInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.supportcannedresponse";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatSupportCannedResponseInfoProvider), OBJECT_TYPE, "Chat.SupportCannedResponse", "ChatSupportCannedResponseID", null, null, "ChatSupportCannedResponseName", "ChatSupportCannedResponseTagName", null, "ChatSupportCannedResponseSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ChatSupportCannedResponseChatUserID", ChatUserInfo.OBJECT_TYPE) },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default
            },
            LogEvents = false,
            TouchCacheDependencies = false,
            ModuleName = ModuleName.CHAT,
            SupportsVersioning = false,
            AllowRestore = false,
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                LogExport = true,
                IsExportable = true,
                WhereCondition = "ChatSupportCannedResponseChatUserID IS NULL",
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY),
                },
            },
            SupportsCloneToOtherSite = false,
            MacroCollectionName = "ChatSupportCannedResponse",
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                FilterCondition = new WhereCondition().WhereNull("ChatSupportCannedResponseChatUserID")
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Export only canned responses which do not belong to user (are site bound or global).
        /// </summary>
        protected override bool LogExport
        {
            get
            {
                return base.LogExport && (ChatSupportCannedResponseChatUserID == null);
            }
            set
            {
                base.LogExport = value;
            }
        }


        /// <summary>
        /// ID of this canned response
        /// </summary>
        public virtual int ChatSupportCannedResponseID
        {
            get
            {
                return GetIntegerValue("ChatSupportCannedResponseID", 0);
            }
            set
            {
                SetValue("ChatSupportCannedResponseID", value);
            }
        }


        /// <summary>
        /// Owner of this canned response
        /// </summary>
        public virtual int? ChatSupportCannedResponseChatUserID
        {
            get
            {
                object val = GetValue("ChatSupportCannedResponseChatUserID");
                if (val == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(val, 0);
            }
            set
            {
                SetValue("ChatSupportCannedResponseChatUserID", value);
            }
        }


        /// <summary>
        /// Tag of this canned response. This field will be used to find canned response.
        /// </summary>
        public virtual string ChatSupportCannedResponseTagName
        {
            get
            {
                return GetStringValue("ChatSupportCannedResponseTagName", "");
            }
            set
            {
                SetValue("ChatSupportCannedResponseTagName", value);
            }
        }


        /// <summary>
        /// Code name of this canned response. Used for exporting.
        /// </summary>
        public virtual string ChatSupportCannedResponseName
        {
            get
            {
                return GetStringValue("ChatSupportCannedResponseName", "");
            }
            set
            {
                SetValue("ChatSupportCannedResponseName", value);
            }
        }


        /// <summary>
        /// Text of this canned response. Code name will be replaced with this field.
        /// </summary>
        public virtual string ChatSupportCannedResponseText
        {
            get
            {
                return GetStringValue("ChatSupportCannedResponseText", "");
            }
            set
            {
                SetValue("ChatSupportCannedResponseText", value);
            }
        }


        /// <summary>
        /// Site to which this canned response belongs (null = global).
        /// </summary>
        public virtual int? ChatSupportCannedResponseSiteID
        {
            get
            {
                object val = GetValue("ChatSupportCannedResponseSiteID");
                if (val == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(val, 0);
            }
            set
            {
                SetValue("ChatSupportCannedResponseSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatSupportCannedResponseInfoProvider.DeleteChatSupportCannedResponseInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatSupportCannedResponseInfoProvider.SetChatSupportCannedResponseInfo(this);
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// Check takes into account the fact, that the canned response can be personal and its owner thus has all the permissions.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            var permissionToCheck = GetPermissionToCheck(permission);
            if (!ChatSupportCannedResponseChatUserID.HasValue)
            {
                return base.CheckPermissionsInternal(permissionToCheck, siteName, userInfo, exceptionOnFailure);
            }

            if (userInfo.IsAuthorizedPerResource(ModuleName.CHAT, "EnterSupport", siteName, exceptionOnFailure))
            {
                // Personal response permission check
                ChatUserInfo chatUser = ChatUserInfoProvider.GetChatUserInfo(ChatSupportCannedResponseChatUserID.Value);
                if (chatUser != null && chatUser.ChatUserUserID.HasValue)
                {
                    int userID = chatUser.ChatUserUserID.Value;

                    if (userInfo.UserID == userID)
                    {
                        return true;
                    }
                }

                // Throw an exception on failure
                if (exceptionOnFailure)
                {
                    PermissionCheckException(permission, siteName, false);
                }
            }

            return false;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatSupportCannedResponseInfo object.
        /// </summary>
        public ChatSupportCannedResponseInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatSupportCannedResponseInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatSupportCannedResponseInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion

    }
}

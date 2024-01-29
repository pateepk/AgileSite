using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatUserInfo), ChatUserInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatUserInfo data container class.
    /// </summary>
    public class ChatUserInfo : AbstractInfo<ChatUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.user";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatUserInfoProvider), OBJECT_TYPE, "Chat.User", "ChatUserID", "ChatUserLastModification", null, null, "ChatUserNickname", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency( "ChatUserUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
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
            MacroCollectionName = "ChatUser",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets Chat User ID - Foreign key. Null if user is anonymous (does not have any CMS User assigned).
        /// </summary>
        public virtual int? ChatUserUserID
        {
            get
            {
                object value = GetValue("ChatUserUserID");
                if (value == null)
                {
                    return null;
                }
                return ValidationHelper.GetInteger(value, 0);
            }
            set
            {         
                SetValue("ChatUserUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets User's Nickname.
        /// </summary>
        public virtual string ChatUserNickname
        {
            get
            {
                return GetStringValue("ChatUserNickname", "");
            }
            set
            {         
                SetValue("ChatUserNickname", value);
            }
        }


        /// <summary>
        /// Gets or sets Chat user ID - Primary key.
        /// </summary>
        public virtual int ChatUserID
        {
            get
            {
                return GetIntegerValue("ChatUserID", 0);
            }
            set
            {         
                SetValue("ChatUserID", value);
            }
        }


        /// <summary>
        /// Checks if this chat user is anonymous (he is anonymous if column ChatUserUserID is null).
        /// </summary>
        public bool IsAnonymous
        {
            get
            {
                return ChatUserUserID == null;
            }
        }


        /// <summary>
        /// Gets or sets the time of last modification made to this user.
        /// </summary>
        public DateTime ChatUserLastModification
        {
            get
            {
                return GetDateTimeValue("ChatUserLastModification", DateTime.Now);
            }
            set
            {
                SetValue("ChatUserLastModification", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatUserInfoProvider.DeleteChatUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatUserInfoProvider.SetChatUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatUserInfo object.
        /// </summary>
        public ChatUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Ensures that nickname of this user is unique among non-anonymous users.
        /// </summary>
        public void EnsureUniqueNickname()
        {
            ChatUserNickname = GetUniqueDisplayName(ChatUserNickname, ChatUserID);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Constructs base where condition for checking column value uniqueness.
        /// 
        /// If DisplayName column is being checked, it modifies where condition, so only non-anonymous users are checked.
        /// </summary>
        /// <param name="columnName">Name of the column in which the uniqueness should be preserved (CodeNameColumn/DisplayNameColumn)</param>
        /// <param name="searchName">Name which should be saved in the column (evenutally with suffix)</param>
        /// <param name="currentObjectId">ID of the current object (this object will be excluded from the search for duplicate names)</param>
        /// <param name="exactMatch">If true, the names must match exactly</param>
        /// <returns>Where condition used to check for unique name</returns>
        protected override WhereCondition GetUniqueNameWhereCondition(string columnName, string searchName, int currentObjectId, bool exactMatch)
        {
            var where = base.GetUniqueNameWhereCondition(columnName, searchName, currentObjectId, exactMatch);

            // If display name column (ChatUserNickname) is being checked, add condition to unique column value check.
            // So uniqueness will be checked only in non-anonymous users
            if (columnName == DisplayNameColumn)
            {
                where.WhereNotNull("ChatUserUserID");
            }

            return where;
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Custom permissions check. Chat user can be read with Read or GlobalRead permission
        /// and modified with Modify or GlobalModify permission.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.CHAT, "GlobalRead", siteName, (UserInfo)userInfo, false))
                    {
                        return true;
                    }

                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                case PermissionsEnum.Modify:
                    if (UserInfoProvider.IsAuthorizedPerResource(ModuleName.CHAT, "GlobalModify", siteName, (UserInfo)userInfo, false))
                    {
                        return true;
                    }

                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}

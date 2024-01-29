using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardMessageInfo), BoardMessageInfo.OBJECT_TYPE)]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardMessageInfo data container class.
    /// </summary>
    public class BoardMessageInfo : AbstractInfo<BoardMessageInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BOARDMESSAGE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BoardMessageInfoProvider), OBJECT_TYPE, "Board.Message", "MessageID", "MessageLastModified", "MessageGUID", null, null, null, null, "MessageBoardID", BoardInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MessageApprovedByUserID", UserInfo.OBJECT_TYPE),
                new ObjectDependency("MessageUserID", UserInfo.OBJECT_TYPE)
            },
            MacroCollectionName = "CMS.BoardMessage",
            ModuleName = ModuleName.MESSAGEBOARD,
            SupportsCloning = false,
            ImportExportSettings =
            {
                LogProgress = false,
                IncludeToExportParentDataSet = IncludeToParentEnum.None
            },
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("MessageUserInfo")
                }
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Post IP address, and agent values.
        /// </summary>
        protected UserDataInfo mMessageUserInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Message board ID.
        /// </summary>
        public virtual int MessageBoardID
        {
            get
            {
                return GetIntegerValue("MessageBoardID", 0);
            }
            set
            {
                SetValue("MessageBoardID", value);
            }
        }


        /// <summary>
        /// Message Is Spam.
        /// </summary>
        public virtual bool MessageIsSpam
        {
            get
            {
                return GetBooleanValue("MessageIsSpam", false);
            }
            set
            {
                SetValue("MessageIsSpam", value);
            }
        }


        /// <summary>
        /// Message text.
        /// </summary>
        public virtual string MessageText
        {
            get
            {
                return GetStringValue("MessageText", "");
            }
            set
            {
                SetValue("MessageText", value);
            }
        }


        /// <summary>
        /// Message last modified datetime.
        /// </summary>
        public virtual DateTime MessageLastModified
        {
            get
            {
                return GetDateTimeValue("MessageLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MessageLastModified", value);
            }
        }


        /// <summary>
        /// Message inserted datetime.
        /// </summary>
        public virtual DateTime MessageInserted
        {
            get
            {
                return GetDateTimeValue("MessageInserted", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MessageInserted", value);
            }
        }


        /// <summary>
        /// Additional user info.
        /// </summary>
        public virtual UserDataInfo MessageUserInfo
        {
            get
            {
                if (mMessageUserInfo == null)
                {
                    // Load the xml data
                    mMessageUserInfo = new UserDataInfo();
                    mMessageUserInfo.LoadData(ValidationHelper.GetString(GetValue("MessageUserInfo"), ""));
                }
                return mMessageUserInfo;
            }
        }


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
        /// Message Avatar GUID.
        /// </summary>
        public virtual Guid MessageAvatarGUID
        {
            get
            {
                return GetGuidValue("MessageAvatarGUID", Guid.Empty);
            }
            set
            {
                SetValue("MessageAvatarGUID", value);
            }
        }


        /// <summary>
        /// Id of the user who inserted message.
        /// </summary>
        public virtual int MessageUserID
        {
            get
            {
                return GetIntegerValue("MessageUserID", 0);
            }
            set
            {
                SetValue("MessageUserID", value);
            }
        }


        /// <summary>
        /// ID of the user who approved the message.
        /// </summary>
        public virtual int MessageApprovedByUserID
        {
            get
            {
                return GetIntegerValue("MessageApprovedByUserID", 0);
            }
            set
            {
                SetValue("MessageApprovedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Message email.
        /// </summary>
        public virtual string MessageEmail
        {
            get
            {
                return GetStringValue("MessageEmail", "");
            }
            set
            {
                SetValue("MessageEmail", value);
            }
        }


        /// <summary>
        /// Message user name.
        /// </summary>
        public virtual string MessageUserName
        {
            get
            {
                return GetStringValue("MessageUserName", "");
            }
            set
            {
                SetValue("MessageUserName", value);
            }
        }


        /// <summary>
        /// GUID of the message.
        /// </summary>
        public virtual Guid MessageGUID
        {
            get
            {
                return GetGuidValue("MessageGUID", Guid.Empty);
            }
            set
            {
                SetValue("MessageGUID", value);
            }
        }


        /// <summary>
        /// Rating of current message (float between 0..1).
        /// </summary>
        public virtual double MessageRatingValue
        {
            get
            {
                return GetDoubleValue("MessageRatingValue", 0.0f);
            }
            set
            {
                SetValue("MessageRatingValue", value, 0);
            }
        }


        /// <summary>
        /// Is message approved or not.
        /// </summary>
        public virtual bool MessageApproved
        {
            get
            {
                return GetBooleanValue("MessageApproved", false);
            }
            set
            {
                SetValue("MessageApproved", value);
            }
        }


        /// <summary>
        /// Message URL.
        /// </summary>
        public virtual string MessageURL
        {
            get
            {
                return GetStringValue("MessageURL", "");
            }
            set
            {
                SetValue("MessageURL", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BoardMessageInfoProvider.DeleteBoardMessageInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BoardMessageInfoProvider.SetBoardMessageInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardMessageInfo object.
        /// </summary>
        public BoardMessageInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardMessageInfo object from the given DataRow.
        /// </summary>
        public BoardMessageInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            BoardInfo board = BoardInfoProvider.GetBoardInfo(MessageBoardID);

            switch (permission)
            {
                case PermissionsEnum.Create:
                    return (board != null) && BoardInfoProvider.IsUserAuthorizedToAddMessages(board);

                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return (board != null) && BoardInfoProvider.IsUserAuthorizedToManageMessages(board);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}
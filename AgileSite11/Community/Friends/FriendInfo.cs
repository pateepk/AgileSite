using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Community;

[assembly: RegisterObjectType(typeof(FriendInfo), FriendInfo.OBJECT_TYPE)]

namespace CMS.Community
{
    /// <summary>
    /// FriendInfo data container class.
    /// </summary>
    public class FriendInfo : AbstractInfo<FriendInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.FRIEND;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FriendInfoProvider), OBJECT_TYPE, "Community.Friend", "FriendID", null, "FriendGUID", null, null, null, null, "FriendRequestedUserID", UserInfo.OBJECT_TYPE)
                                              {
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("FriendUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding), new ObjectDependency("FriendApprovedBy", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), new ObjectDependency("FriendRejectedBy", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired) },
                                                  IsBinding = true,
                                                  AllowRestore = false,
                                                  ModuleName = "cms.friends",
                                                  SupportsCloning = false,
                                                  RegisterAsChildToObjectTypes = new List<string>() { UserInfo.OBJECT_TYPE },
                                                  RegisterAsBindingToObjectTypes = new List<string>(),
                                                  RegisterAsOtherBindingToObjectTypes = new List<string>(),
                                                  TouchCacheDependencies = true,
                                                  SynchronizationSettings =
                                                  {
                                                      LogSynchronization = SynchronizationTypeEnum.None
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of requesting user.
        /// </summary>
        public virtual int FriendUserID
        {
            get
            {
                return GetIntegerValue("FriendUserID", 0);
            }
            set
            {
                SetValue("FriendUserID", value);
            }
        }


        /// <summary>
        /// ID of user being requested.
        /// </summary>
        public virtual int FriendRequestedUserID
        {
            get
            {
                return GetIntegerValue("FriendRequestedUserID", 0);
            }
            set
            {
                SetValue("FriendRequestedUserID", value);
            }
        }


        /// <summary>
        /// ID of friendship.
        /// </summary>
        public virtual int FriendID
        {
            get
            {
                return GetIntegerValue("FriendID", 0);
            }
            set
            {
                SetValue("FriendID", value);
            }
        }


        /// <summary>
        /// Friendship approved by.
        /// </summary>
        public virtual int FriendApprovedBy
        {
            get
            {
                return GetIntegerValue("FriendApprovedBy", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("FriendApprovedBy", null);
                }
                else
                {
                    SetValue("FriendApprovedBy", value);
                }
            }
        }


        /// <summary>
        /// Friendship rejected by.
        /// </summary>
        public virtual int FriendRejectedBy
        {
            get
            {
                return GetIntegerValue("FriendRejectedBy", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("FriendRejectedBy", null);
                }
                else
                {
                    SetValue("FriendRejectedBy", value);
                }
            }
        }


        /// <summary>
        /// GUID of friendship.
        /// </summary>
        public virtual Guid FriendGUID
        {
            get
            {
                return GetGuidValue("FriendGUID", Guid.Empty);
            }
            set
            {
                SetValue("FriendGUID", value);
            }
        }


        /// <summary>
        /// Friend Requested When.
        /// </summary>
        public virtual DateTime FriendRequestedWhen
        {
            get
            {
                return GetDateTimeValue("FriendRequestedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FriendRequestedWhen", value);
            }
        }


        /// <summary>
        /// Friend Rejected When.
        /// </summary>
        public virtual DateTime FriendRejectedWhen
        {
            get
            {
                return GetDateTimeValue("FriendRejectedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FriendRejectedWhen", value);
            }
        }


        /// <summary>
        /// Friend Approved When.
        /// </summary>
        public virtual DateTime FriendApprovedWhen
        {
            get
            {
                return GetDateTimeValue("FriendApprovedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FriendApprovedWhen", value);
            }
        }


        /// <summary>
        /// Comment of friendship.
        /// </summary>
        public virtual string FriendComment
        {
            get
            {
                return GetStringValue("FriendComment", "");
            }
            set
            {
                SetValue("FriendComment", value);
            }
        }


        /// <summary>
        /// Friendship status.
        /// </summary>
        public virtual FriendshipStatusEnum FriendStatus
        {
            get
            {
                object status = GetValue("FriendStatus");
                if (status != null)
                {
                    return (FriendshipStatusEnum)status;
                }
                return FriendshipStatusEnum.Waiting;
            }
            set
            {
                SetValue("FriendStatus", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            FriendInfoProvider.DeleteFriendInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            FriendInfoProvider.SetFriendInfo(this);
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
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = UserInfoProvider.IsAuthorizedPerResource(TypeInfo.ModuleName, "Manage", siteName, (UserInfo)userInfo);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Get existing object
        /// </summary>
        /// <returns>Existing friend object</returns>
        protected override BaseInfo GetExisting()
        {
            return FriendInfoProvider.GetFriendInfo(FriendUserID, FriendRequestedUserID);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty FriendInfo object.
        /// </summary>
        public FriendInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new FriendInfo object from the given DataRow.
        /// </summary>
        public FriendInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
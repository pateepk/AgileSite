using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Community;

[assembly: RegisterObjectType(typeof(GroupMemberInfo), GroupMemberInfo.OBJECT_TYPE)]

namespace CMS.Community
{
    /// <summary>
    /// GroupMemberInfo data container class.
    /// </summary>
    public class GroupMemberInfo : AbstractInfo<GroupMemberInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.GROUPMEMBER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GroupMemberInfoProvider), OBJECT_TYPE, "Community.GroupMember", "MemberID", null, "MemberGUID", null, null, null, null,
            "MemberGroupID", GroupInfo.OBJECT_TYPE)
                                              {
                                                  DependsOn = new List<ObjectDependency> { new ObjectDependency("MemberUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                                                      new ObjectDependency("MemberApprovedByUserID", UserInfo.OBJECT_TYPE), 
                                                      new ObjectDependency("MemberInvitedByUserID", UserInfo.OBJECT_TYPE) },
                                                  ModuleName = "cms.groups",
                                                  AllowRestore = false,
                                                  GroupIDColumn = "MemberGroupID",
                                                  SupportsCloning = false,
                                                  TouchCacheDependencies = true,
                                                  ImportExportSettings = { IsAutomaticallySelected = true, IncludeToExportParentDataSet = IncludeToParentEnum.Complete, },
                                                  SynchronizationSettings =
                                                  {
                                                      IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                      LogSynchronization = SynchronizationTypeEnum.None
                                                  },
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Member status.
        /// </summary>
        public virtual GroupMemberStatus MemberStatus
        {
            get
            {
                return (GroupMemberStatus)ValidationHelper.GetInteger(GetValue("MemberStatus"), 0);
            }
            set
            {
                SetValue("MemberStatus", Convert.ToInt32(value));
            }
        }


        /// <summary>
        /// Date when member joined.
        /// </summary>
        public virtual DateTime MemberJoined
        {
            get
            {
                return GetDateTimeValue("MemberJoined", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MemberJoined", value);
            }
        }


        /// <summary>
        /// Member group ID.
        /// </summary>
        public virtual int MemberGroupID
        {
            get
            {
                return GetIntegerValue("MemberGroupID", 0);
            }
            set
            {
                SetValue("MemberGroupID", value);
            }
        }


        /// <summary>
        /// Member ID.
        /// </summary>
        public virtual int MemberID
        {
            get
            {
                return GetIntegerValue("MemberID", 0);
            }
            set
            {
                SetValue("MemberID", value);
            }
        }


        /// <summary>
        /// Member approved by user with UserID.
        /// </summary>
        public virtual int MemberApprovedByUserID
        {
            get
            {
                return GetIntegerValue("MemberApprovedByUserID", 0);
            }
            set
            {
                SetValue("MemberApprovedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Member GUID.
        /// </summary>
        public virtual Guid MemberGUID
        {
            get
            {
                return GetGuidValue("MemberGUID", Guid.Empty);
            }
            set
            {
                SetValue("MemberGUID", value);
            }
        }


        /// <summary>
        /// Member UserID.
        /// </summary>
        public virtual int MemberUserID
        {
            get
            {
                return GetIntegerValue("MemberUserID", 0);
            }
            set
            {
                SetValue("MemberUserID", value);
            }
        }


        /// <summary>
        /// Member invited by user with UserID.
        /// </summary>
        public virtual int MemberInvitedByUserID
        {
            get
            {
                return GetIntegerValue("MemberInvitedByUserID", 0);
            }
            set
            {
                SetValue("MemberInvitedByUserID", value);
            }
        }


        /// <summary>
        /// Date when member was approved.
        /// </summary>
        public virtual DateTime MemberApprovedWhen
        {
            get
            {
                return GetDateTimeValue("MemberApprovedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MemberApprovedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Date when member is approved.
        /// </summary>
        public virtual DateTime MemberRejectedWhen
        {
            get
            {
                return GetDateTimeValue("MemberRejectedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MemberRejectedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Member comment.
        /// </summary>
        public virtual string MemberComment
        {
            get
            {
                return GetStringValue("MemberComment", "");
            }
            set
            {
                SetValue("MemberComment", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            GroupMemberInfoProvider.DeleteGroupMemberInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            GroupMemberInfoProvider.SetGroupMemberInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty GroupMemberInfo object.
        /// </summary>
        public GroupMemberInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new GroupMemberInfo object from the given DataRow.
        /// </summary>
        public GroupMemberInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Remove user from group roles
            UserInfo userInfo = UserInfoProvider.GetUserInfo(MemberUserID);
            DataTable dt = UserInfoProvider.GetUserRoles(userInfo, String.Empty, String.Empty, 0, String.Empty, false, false, false);
            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int groupId = ValidationHelper.GetInteger(dr["RoleGroupID"], 0);
                    if (groupId == MemberGroupID)
                    {
                        int roleId = ValidationHelper.GetInteger(dr["RoleID"], 0);
                        UserInfoProvider.RemoveUserFromRole(MemberUserID, roleId);
                    }
                }
            }

            // Updates groups hash table for CurrentUser
            var currentUser = MembershipContext.AuthenticatedUser;
            if ((currentUser != null) && (currentUser.Groups != null) && (MemberUserID == currentUser.UserID))
            {
                currentUser.Groups.Remove(MemberGroupID);
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion
    }
}
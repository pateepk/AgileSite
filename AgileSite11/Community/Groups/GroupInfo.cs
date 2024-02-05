using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Community;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.MessageBoards;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(GroupInfo), GroupInfo.OBJECT_TYPE)]

namespace CMS.Community
{
    /// <summary>
    /// GroupInfo data container class.
    /// </summary>
    public class GroupInfo : AbstractInfo<GroupInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.GROUP;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GroupInfoProvider), OBJECT_TYPE, "Community.Group", "GroupID", "GroupLastModified", "GroupGUID", "GroupName", "GroupDisplayName", null, "GroupSiteID", null, null)
        {
            ModuleName = ModuleName.GROUPS,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
            },
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("GroupCreatedByUserID", UserInfo.OBJECT_TYPE), 
                new ObjectDependency("GroupApprovedByUserID", UserInfo.OBJECT_TYPE), 
                new ObjectDependency("GroupAvatarID", AvatarInfo.OBJECT_TYPE) 
            },
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(PredefinedObjectType.NODE, "NodeGroupID", ObjectDependencyEnum.Required),
                new ExtraColumn(BoardInfo.OBJECT_TYPE, "BoardGroupID", ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "GroupCreatedWhen" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            LogEvents = true,
            TouchCacheDependencies = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Group display name.
        /// </summary>
        public virtual string GroupDisplayName
        {
            get
            {
                return GetStringValue("GroupDisplayName", "");
            }
            set
            {
                SetValue("GroupDisplayName", value);
            }
        }


        /// <summary>
        /// Group site ID.
        /// </summary>
        public virtual int GroupSiteID
        {
            get
            {
                return GetIntegerValue("GroupSiteID", 0);
            }
            set
            {
                SetValue("GroupSiteID", value);
            }
        }


        /// <summary>
        /// Group name.
        /// </summary>
        public virtual string GroupName
        {
            get
            {
                return GetStringValue("GroupName", "");
            }
            set
            {
                SetValue("GroupName", value);
            }
        }


        /// <summary>
        /// Group description.
        /// </summary>
        public virtual string GroupDescription
        {
            get
            {
                return GetStringValue("GroupDescription", "");
            }
            set
            {
                SetValue("GroupDescription", value);
            }
        }


        /// <summary>
        /// Group's last modification time.
        /// </summary>
        public virtual DateTime GroupLastModified
        {
            get
            {
                return GetDateTimeValue("GroupLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GroupLastModified", value);
            }
        }


        /// <summary>
        /// Group access
        /// 0 - Anybody can view the content
        /// 1 - Only site members can view the content
        /// 2 - Only specified roles can view the content (reserved)
        /// 3 - Only group members can view the content
        /// </summary>
        public virtual SecurityAccessEnum GroupAccess
        {
            get
            {
                return (SecurityAccessEnum)ValidationHelper.GetInteger(GetValue("GroupAccess"), 0);
            }
            set
            {
                SetValue("GroupAccess", (int)value);
            }
        }


        /// <summary>
        /// Group GUID.
        /// </summary>
        public virtual Guid GroupGUID
        {
            get
            {
                return GetGuidValue("GroupGUID", Guid.Empty);
            }
            set
            {
                SetValue("GroupGUID", value);
            }
        }


        /// <summary>
        /// Group ID.
        /// </summary>
        public virtual int GroupID
        {
            get
            {
                return GetIntegerValue("GroupID", 0);
            }
            set
            {
                SetValue("GroupID", value);
            }
        }


        /// <summary>
        /// Group approve members
        /// 0 - Any site member can join
        /// 1 - Only approved members can join
        /// 2 - Invited members can join without approval
        /// </summary>
        public virtual GroupApproveMembersEnum GroupApproveMembers
        {
            get
            {
                return (GroupApproveMembersEnum)ValidationHelper.GetInteger(GetValue("GroupApproveMembers"), 0);
            }
            set
            {
                SetValue("GroupApproveMembers", (int)value);
            }
        }


        /// <summary>
        /// GUID of the group's root document.
        /// </summary>
        public virtual Guid GroupNodeGUID
        {
            get
            {
                return GetGuidValue("GroupNodeGUID", Guid.Empty);
            }
            set
            {
                SetValue("GroupNodeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Group created by user with UserID.
        /// </summary>
        public virtual int GroupCreatedByUserID
        {
            get
            {
                return GetIntegerValue("GroupCreatedByUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("GroupCreatedByUserID", null);
                }
                else
                {
                    SetValue("GroupCreatedByUserID", value);
                }
            }
        }


        /// <summary>
        /// Group approved by user with UserID.
        /// </summary>
        public virtual int GroupApprovedByUserID
        {
            get
            {
                return GetIntegerValue("GroupApprovedByUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("GroupApprovedByUserID", null);
                }
                else
                {
                    SetValue("GroupApprovedByUserID", value);
                }
            }
        }


        /// <summary>
        /// Group avatar's ID.
        /// </summary>
        public virtual int GroupAvatarID
        {
            get
            {
                return GetIntegerValue("GroupAvatarID", 0);
            }
            set
            {
                SetValue("GroupAvatarID", value, 0);
            }
        }


        /// <summary>
        /// Indicates if group has been approved.
        /// </summary>
        public virtual bool GroupApproved
        {
            get
            {
                return GetBooleanValue("GroupApproved", false);
            }
            set
            {
                SetValue("GroupApproved", value);
            }
        }


        /// <summary>
        /// Group creation time.
        /// </summary>
        public virtual DateTime GroupCreatedWhen
        {
            get
            {
                return GetDateTimeValue("GroupCreatedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GroupCreatedWhen", value);
            }
        }


        /// <summary>
        /// Indicates if join/leave notification should be sent
        /// </summary>
        public virtual bool GroupSendJoinLeaveNotification
        {
            get
            {
                return GetBooleanValue("GroupSendJoinLeaveNotification", true);
            }
            set
            {
                SetValue("GroupSendJoinLeaveNotification", value);
            }
        }


        /// <summary>
        /// Indicates if 'waiting for approval' notification should be sent.
        /// </summary>
        public virtual bool GroupSendWaitingForApprovalNotification
        {
            get
            {
                return GetBooleanValue("GroupSendWaitingForApprovalNotification", true);
            }
            set
            {
                SetValue("GroupSendWaitingForApprovalNotification", value);
            }
        }


        /// <summary>
        /// Sets group security properties.
        /// </summary>
        public virtual int GroupSecurity
        {
            get
            {
                return GetIntegerValue("GroupSecurity", 444);
            }
            set
            {
                SetValue("GroupSecurity", value);
            }
        }


        /// <summary>
        /// Indicates whether creating group pages is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowCreate
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(GroupSecurity, 3);
            }
            set
            {
                GroupSecurity = SecurityHelper.SetSecurityAccessEnum(GroupSecurity, value, 3);
            }
        }


        /// <summary>
        /// Indicates whether editing group pages is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowModify
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(GroupSecurity, 2);
            }
            set
            {
                GroupSecurity = SecurityHelper.SetSecurityAccessEnum(GroupSecurity, value, 2);
            }
        }


        /// <summary>
        /// Indicates whether the deleting group pages is allowed.
        /// </summary>
        public virtual SecurityAccessEnum AllowDelete
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(GroupSecurity, 1);
            }
            set
            {
                GroupSecurity = SecurityHelper.SetSecurityAccessEnum(GroupSecurity, value, 1);
            }
        }


        /// <summary>
        /// Indicates if ativity logging is performed for this particular group.
        /// </summary>
        public virtual bool GroupLogActivity
        {
            get
            {
                return GetBooleanValue("GroupLogActivity", false);
            }
            set
            {
                SetValue("GroupLogActivity", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            GroupInfoProvider.DeleteGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            GroupInfoProvider.SetGroupInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Copy the document
            TreeProvider tree = new TreeProvider();
            TreeNode node = tree.SelectSingleNode(GroupNodeGUID, TreeProvider.ALL_CULTURES, ObjectSiteName);

            // Erase the link to original document
            SetValue("GroupNodeGUID", null);

            if (node != null)
            {
                var parent = DocumentHelper.GetDocument(node.NodeParentID, TreeProvider.ALL_CULTURES, tree);
                TreeNode clonedNode = DocumentHelper.CopyDocument(node, parent, true, tree);
                if (clonedNode != null)
                {
                    GroupNodeGUID = clonedNode.NodeGUID;
                }
            }

            // Clone Avatar
            if (GroupAvatarID > 0)
            {
                AvatarInfo avatar = AvatarInfoProvider.GetAvatarInfo(GroupAvatarID);
                if (avatar != null)
                {
                    BaseInfo avatarClone = avatar.Generalized.InsertAsClone(settings, result);
                    GroupAvatarID = avatarClone.Generalized.ObjectID;
                }
            }

            Insert();

            settings.ExcludedOtherBindingTypes.AddRange((BoardRoleInfo.OBJECT_TYPE + ";" + PredefinedObjectType.BIZFORMROLE + ";" + PredefinedObjectType.WORKFLOWSTEPROLE + ";" + GroupRolePermissionInfo.OBJECT_TYPE + ";" + ForumRoleInfo.OBJECT_TYPE + ";" + MediaLibraryRolePermissionInfo.OBJECT_TYPE + ";" + PollRoleInfo.OBJECT_TYPE).Split(';'));
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            GroupDescription = "";
            GroupApproveMembers = GroupApproveMembersEnum.AnyoneCanJoin;
            GroupAccess = SecurityAccessEnum.AllUsers;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty GroupInfo object.
        /// </summary>
        public GroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new GroupInfo object from the given DataRow.
        /// </summary>
        public GroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
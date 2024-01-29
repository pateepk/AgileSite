using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Base;
using CMS.Helpers;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(AclItemInfo), AclItemInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// AclItemInfo data container class.
    /// </summary>
    public class AclItemInfo : AbstractInfo<AclItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.aclitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AclItemInfoProvider), OBJECT_TYPE, "CMS.ACLItem", "ACLItemID", "LastModified", "ACLItemGUID", null, null, null, null, "ACLID", AclInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("LastModifiedByUserID", UserInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default,
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = "cms.content",
            ImportExportSettings = { LogExport = false },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("UserID", value);
                }
                else
                {
                    SetValue("UserID", null);
                }
            }
        }


        /// <summary>
        /// ACL item ID.
        /// </summary>
        [DatabaseField]
        public virtual int ACLItemID
        {
            get
            {
                return GetIntegerValue("ACLItemID", 0);
            }
            set
            {
                SetValue("ACLItemID", value);
            }
        }


        /// <summary>
        /// ACL item GUID.
        /// </summary>
         [DatabaseField]
        public virtual Guid ACLItemGUID
        {
            get
            {
                return GetGuidValue("ACLItemGUID", Guid.Empty);
            }
            set
            {
                SetValue("ACLItemGUID", value);
            }
        }


        /// <summary>
        /// ACL ID.
        /// </summary>
        [DatabaseField]
        public virtual int ACLID
        {
            get
            {
                return GetIntegerValue("ACLID", 0);
            }
            set
            {
                SetValue("ACLID", value);
            }
        }


        /// <summary>
        /// Role ID.
        /// </summary>
        [DatabaseField]
        public virtual int RoleID
        {
            get
            {
                return GetIntegerValue("RoleID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("RoleID", value);
                }
                else
                {
                    SetValue("RoleID", null);
                }
            }
        }


        /// <summary>
        /// Acl item last modified date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LastModified
        {
            get
            {
                return GetDateTimeValue("LastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LastModified", value);
            }
        }



        /// <summary>
        /// Last modified by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int LastModifiedByUserID
        {
            get
            {
                return GetIntegerValue("LastModifiedByUserID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("LastModifiedByUserID", value);
                }
                else
                {
                    SetValue("LastModifiedByUserID", null);
                }
            }
        }


        /// <summary>
        /// Denied value representing denied permissions.
        /// </summary>
        [DatabaseField]
        public virtual int Denied
        {
            get
            {
                return GetIntegerValue("Denied", 0);
            }
            set
            {
                SetValue("Denied", value);
            }
        }


        /// <summary>
        /// Allowed value representing allowed permissions.
        /// </summary>
        [DatabaseField]
        public virtual int Allowed
        {
            get
            {
                return GetIntegerValue("Allowed", 0);
            }
            set
            {
                SetValue("Allowed", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AclItemInfo object.
        /// </summary>
        public AclItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AclItemInfo object from the given DataRow.
        /// </summary>
        public AclItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Sets object
        /// </summary>
        protected override void SetObject()
        {
            AclItemInfoProvider.SetAclItemInfo(this);
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
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = UserInfoProvider.IsAuthorizedPerResource(TypeInfo.ModuleName, "ModifyPermissions", siteName, (UserInfo)userInfo);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            if (ACLID > 0)
            {
                var condition = new WhereCondition().WhereEquals("ACLID", ACLID);

                if (UserID > 0)
                {
                    return condition.WhereEquals("UserID", UserID);
                }

                if (RoleID > 0)
                {
                    return condition.WhereEquals("RoleID", RoleID);
                }
            }

            return base.GetExistingWhereCondition();
        }

        #endregion
    }
}
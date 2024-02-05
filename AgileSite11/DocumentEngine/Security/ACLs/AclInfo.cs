using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Base;
using CMS.Helpers;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(AclInfo), AclInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// AclInfo data container class.
    /// </summary>
    public class AclInfo : AbstractInfo<AclInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.acl";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AclInfoProvider), OBJECT_TYPE, "CMS.ACL", "ACLID", "ACLLastModified", "ACLGUID", null, null, null, "ACLSiteID", null, null)
        {
            ModuleName = "cms.content",
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ACL inherited ACLs.
        /// </summary>
        [DatabaseField]
        public virtual string ACLInheritedACLs
        {
            get
            {
                return GetStringValue("ACLInheritedACLs", "");
            }
            set
            {
                SetValue("ACLInheritedACLs", value ?? String.Empty);
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
        /// ACL site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ACLSiteID
        {
            get
            {
                return GetIntegerValue("ACLSiteID", 0);
            }
            set
            {
                SetValue("ACLSiteID", value);
            }
        }


        /// <summary>
        /// ACL GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ACLGUID
        {
            get
            {
                return GetGuidValue("ACLGUID", Guid.Empty);
            }
            set
            {
                SetValue("ACLGUID", value);
            }
        }


        /// <summary>
        /// ACL last modified date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ACLLastModified
        {
            get
            {
                return GetDateTimeValue("ACLLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ACLLastModified", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AclInfo object.
        /// </summary>
        public AclInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AclInfo object from the given DataRow.
        /// </summary>
        public AclInfo(DataRow dr)
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
            AclInfoProvider.SetAclInfo(this);
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
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ACLInheritedACLs = "";
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Messaging;

[assembly: RegisterObjectType(typeof(IgnoreListInfo), IgnoreListInfo.OBJECT_TYPE)]

namespace CMS.Messaging
{
    /// <summary>
    /// IgnoreListInfo data container class.
    /// </summary>
    public class IgnoreListInfo : AbstractInfo<IgnoreListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.IGNORELIST;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IgnoreListInfoProvider), OBJECT_TYPE, "Messaging.IgnoreList", null, null, null, null, null, null, null, "IgnoreListUserID", UserInfo.OBJECT_TYPE)
                                              {
                                                  TouchCacheDependencies = true,
                                                  DependsOn = new List<ObjectDependency>()
                                                  {
                                                      new ObjectDependency("IgnoreListIgnoredUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                                                  },
                                                  ModuleName = "cms.messaging",
                                                  SynchronizationSettings =
                                                    {
                                                        IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                        LogSynchronization = SynchronizationTypeEnum.None,
                                                    }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Ignore List User ID.
        /// </summary>
        public virtual int IgnoreListUserID
        {
            get
            {
                return GetIntegerValue("IgnoreListUserID", 0);
            }
            set
            {
                SetValue("IgnoreListUserID", value);
            }
        }


        /// <summary>
        /// Ignore List Ignored User ID.
        /// </summary>
        public virtual int IgnoreListIgnoredUserID
        {
            get
            {
                return GetIntegerValue("IgnoreListIgnoredUserID", 0);
            }
            set
            {
                SetValue("IgnoreListIgnoredUserID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IgnoreListInfoProvider.DeleteIgnoreListInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IgnoreListInfoProvider.SetIgnoreListInfo(this);
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
                case PermissionsEnum.Read:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = (IgnoreListUserID == userInfo.UserID);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IgnoreListInfo object.
        /// </summary>
        public IgnoreListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IgnoreListInfo object from the given DataRow.
        /// </summary>
        public IgnoreListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
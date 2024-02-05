using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserRoleInfo), UserRoleInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// UserRoleInfo data container class.
    /// </summary>
    public class UserRoleInfo : AbstractInfo<UserRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.userrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserRoleInfoProvider), OBJECT_TYPE, "CMS.UserRole", "UserRoleID", null, null, null, null, null, null, "RoleID", RoleInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            ModuleName = "cms.roles",
            AllowRestore = false,
            IsBinding = true,
            RegisterAsBindingToObjectTypes = new List<string> { RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP },
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User ID.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Role ID.
        /// </summary>
        public virtual int RoleID
        {
            get
            {
                return GetIntegerValue("RoleID", 0);
            }
            set
            {
                SetValue("RoleID", value);
            }
        }


        /// <summary>
        /// Date to membership is valid for given user.
        /// </summary>
        public virtual DateTime ValidTo
        {
            get
            {
                return GetDateTimeValue("ValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserRoleInfoProvider.DeleteUserRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserRoleInfoProvider.SetUserRoleInfo(this);
        }


        /// <summary>
        /// Clones user-role binding.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Do not clone group role's binding when main object of cloning is user
            if ((settings.CloneBase.TypeInfo.ObjectType != UserInfo.OBJECT_TYPE) || (ObjectParent.TypeInfo.ObjectType != RoleInfo.OBJECT_TYPE_GROUP))
            {
                base.InsertAsCloneInternal(settings, result, originalObject);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserRoleInfo object.
        /// </summary>
        public UserRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserRoleInfo object from the given DataRow.
        /// </summary>
        public UserRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
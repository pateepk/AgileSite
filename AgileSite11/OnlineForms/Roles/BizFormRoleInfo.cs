using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.OnlineForms;
using CMS.Membership;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(BizFormRoleInfo), BizFormRoleInfo.OBJECT_TYPE)]

namespace CMS.OnlineForms
{
    /// <summary>
    /// BizFormRoleInfo data container class.
    /// </summary>
    public class BizFormRoleInfo : AbstractInfo<BizFormRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BIZFORMROLE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BizFormRoleInfoProvider), OBJECT_TYPE, "CMS.FormRole", null, null, null, null, null, null, null, "FormID", BizFormInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            RegisterAsOtherBindingToObjectTypes = new List<string>
            {
                RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP
            },
            ContinuousIntegrationSettings = { Enabled = true }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the Form.
        /// </summary>
        public virtual int FormID
        {
            get
            {
                return GetIntegerValue("FormID", 0);
            }
            set
            {
                SetValue("FormID", value);
            }
        }


        /// <summary>
        /// ID of the Role.
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BizFormRoleInfoProvider.DeleteBizFormRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BizFormRoleInfoProvider.SetBizFormRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BizFormRoleInfo object.
        /// </summary>
        public BizFormRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BizFormRoleInfo object from the given DataRow.
        /// </summary>
        public BizFormRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
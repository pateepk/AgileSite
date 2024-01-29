using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(RoleApplicationInfo), RoleApplicationInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// RoleApplicationInfo data container class.
    /// </summary>
    public class RoleApplicationInfo : AbstractInfo<RoleApplicationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.roleapplication";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RoleApplicationInfoProvider), OBJECT_TYPE, "CMS.RoleApplication", null, null, null, null, null, null, null, "RoleID", PredefinedObjectType.ROLE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("ElementID", UIElementInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.UIPERSONALIZATION,
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Role ID.
        /// </summary>
        [DatabaseField]
        public virtual int RoleID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("RoleID"), 0);
            }
            set
            {
                SetValue("RoleID", value);
            }
        }


        /// <summary>
        /// Element ID.
        /// </summary>
        [DatabaseField]
        public virtual int ElementID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ElementID"), 0);
            }
            set
            {
                SetValue("ElementID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RoleApplicationInfoProvider.DeleteRoleApplicationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RoleApplicationInfoProvider.SetRoleApplicationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RoleApplicationInfo object.
        /// </summary>
        public RoleApplicationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RoleApplicationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public RoleApplicationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
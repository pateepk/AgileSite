using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(RoleUIElementInfo), RoleUIElementInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// RoleUIElementInfo data container class.
    /// </summary>
    public class RoleUIElementInfo : AbstractInfo<RoleUIElementInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.roleuielement";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RoleUIElementInfoProvider), OBJECT_TYPE, "CMS.RoleUIElement", null, null, null, null, null, null, null, "RoleID", PredefinedObjectType.ROLE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ElementID", UIElementInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            },
            ModuleName = ModuleName.UIPERSONALIZATION
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the UIElement.
        /// </summary>
        public virtual int ElementID
        {
            get
            {
                return GetIntegerValue("ElementID", 0);
            }
            set
            {
                SetValue("ElementID", value);
            }
        }


        /// <summary>
        /// ID of the role.
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
            RoleUIElementInfoProvider.DeleteRoleUIElementInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RoleUIElementInfoProvider.SetRoleUIElementInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RoleUIElementInfo object.
        /// </summary>
        public RoleUIElementInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RoleUIElementInfo object from the given DataRow.
        /// </summary>
        public RoleUIElementInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
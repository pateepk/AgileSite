using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WidgetRoleInfo), WidgetRoleInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WidgetRole info class.
    /// </summary>
    public class WidgetRoleInfo : AbstractInfo<WidgetRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.widgetrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WidgetRoleInfoProvider), OBJECT_TYPE, "CMS.WidgetRole", null, null, null, null, null, null, null, "RoleID", RoleInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("WidgetID", WidgetInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("PermissionID", PermissionNameInfo.OBJECT_TYPE_RESOURCE, ObjectDependencyEnum.Required)
            },
            ModuleName = ModuleName.WIDGETS,
            IsBinding = true,
            ImportExportSettings =
            {
                LogExport = false
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or set the widget ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetID
        {
            get
            {
                return GetIntegerValue("WidgetID", 0);
            }
            set
            {
                SetValue("WidgetID", value);
            }
        }


        /// <summary>
        /// Gets or set the role ID.
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
                SetValue("RoleID", value);
            }
        }


        /// <summary>
        /// Permission ID.
        /// </summary>
        [DatabaseField]
        public virtual int PermissionID
        {
            get
            {
                return GetIntegerValue("PermissionID", 0);
            }
            set
            {
                SetValue("PermissionID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WidgetRoleInfoProvider.DeleteWidgetRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WidgetRoleInfoProvider.SetWidgetRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WidgetRoleInfo object.
        /// </summary>
        public WidgetRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WidgetRoleInfo object from the given DataRow.
        /// </summary>
        public WidgetRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
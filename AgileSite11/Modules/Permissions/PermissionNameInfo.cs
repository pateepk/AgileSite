using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(PermissionNameInfo), PermissionNameInfo.OBJECT_TYPE_RESOURCE)]
[assembly: RegisterObjectType(typeof(PermissionNameInfo), PermissionNameInfo.OBJECT_TYPE_CLASS)]

namespace CMS.Modules
{
    /// <summary>
    /// Permission info data container.
    /// </summary>
    public class PermissionNameInfo : AbstractInfo<PermissionNameInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type for resource
        /// </summary>
        public const string OBJECT_TYPE_RESOURCE = PredefinedObjectType.PERMISSION;

        /// <summary>
        /// Object type for class
        /// </summary>
        public const string OBJECT_TYPE_CLASS = PredefinedObjectType.CLASSPERMISSION;


        /// <summary>
        /// Resource type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFORESOURCE = new ObjectTypeInfo(typeof(PermissionNameInfoProvider), OBJECT_TYPE_RESOURCE, "CMS.Permission", "PermissionID", "PermissionLastModified", "PermissionGUID", "PermissionName", "PermissionDisplayName", null, null, "ResourceID", ResourceInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Incremental
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OrderColumn = "PermissionOrder",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.Incremental },
            TypeCondition = new TypeCondition().WhereIsNotNull("ResourceID"),
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Class type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCLASS = new ObjectTypeInfo(typeof(PermissionNameInfoProvider), OBJECT_TYPE_CLASS, "CMS.Permission", "PermissionID", "PermissionLastModified", "PermissionGUID", "PermissionName", "PermissionDisplayName", null, null, "ClassID", DataClassInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Incremental
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFORESOURCE,
            OrderColumn = "PermissionOrder",
            RegisterAsChildToObjectTypes = new List<string> { PredefinedObjectType.DOCUMENTTYPE, PredefinedObjectType.CUSTOMTABLECLASS },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.Incremental },
            TypeCondition = new TypeCondition().WhereIsNotNull("ClassID"),
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the permission ID.
        /// </summary>
        public virtual int PermissionId
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


        /// <summary>
        /// Gets or sets the permission display name.
        /// </summary>
        public virtual string PermissionDisplayName
        {
            get
            {
                return GetStringValue("PermissionDisplayName", "");
            }
            set
            {
                SetValue("PermissionDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the permission name.
        /// </summary>
        public virtual string PermissionName
        {
            get
            {
                return GetStringValue("PermissionName", "");
            }
            set
            {
                SetValue("PermissionName", value);
            }
        }


        /// <summary>
        /// Gets or sets the class ID.
        /// </summary>
        public virtual int ClassId
        {
            get
            {
                return GetIntegerValue("ClassID", 0);
            }
            set
            {
                if (ValidationHelper.GetInteger(value, 0) == 0)
                {
                    SetValue("ClassID", null);
                }
                else
                {
                    SetValue("ClassID", value);
                }
            }
        }


        /// <summary>
        /// Gets or sets the resource ID.
        /// </summary>
        public virtual int ResourceId
        {
            get
            {
                return GetIntegerValue("ResourceID", 0);
            }
            set
            {
                if (ValidationHelper.GetInteger(value, 0) == 0)
                {
                    SetValue("ResourceID", null);
                }
                else
                {
                    SetValue("ResourceID", value);
                }
            }
        }


        /// <summary>
        /// Permission GUID.
        /// </summary>
        public virtual Guid PermissionGUID
        {
            get
            {
                return GetGuidValue("PermissionGUID", Guid.Empty);
            }
            set
            {
                SetValue("PermissionGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime PermissionLastModified
        {
            get
            {
                return GetDateTimeValue("PermissionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PermissionLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// The Permission Description.
        /// </summary>
        public string PermissionDescription
        {
            get
            {
                return GetStringValue("PermissionDescription", "");
            }
            set
            {
                SetValue("PermissionDescription", value);
            }
        }


        /// <summary>
        /// Indicates whether the permission should be displayed in the permission matrix as well.
        /// </summary>
        public bool PermissionDisplayInMatrix
        {
            get
            {
                return GetBooleanValue("PermissionDisplayInMatrix", true);
            }
            set
            {
                SetValue("PermissionDisplayInMatrix", value);
            }
        }


        /// <summary>
        /// Gets or sets permission order.
        /// </summary>
        public virtual int PermissionOrder
        {
            get
            {
                return GetIntegerValue("PermissionOrder", 0);
            }
            set
            {
                SetValue("PermissionOrder", value);
            }
        }


        /// <summary>
        /// Indicates whether the permission can be assigned only by global administrators.
        /// </summary>
        public bool PermissionEditableByGlobalAdmin
        {
            get
            {
                return GetBooleanValue("PermissionEditableByGlobalAdmin", false);
            }
            set
            {
                SetValue("PermissionEditableByGlobalAdmin", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type info.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (ClassId > 0)
                {
                    // Class permission
                    return TYPEINFOCLASS;
                }
                else
                {
                    // Resource permission
                    return TYPEINFORESOURCE;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PermissionNameInfoProvider.DeletePermissionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PermissionNameInfoProvider.SetPermissionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty PermissionNameInfo structure.
        /// </summary>
        public PermissionNameInfo()
            : base(TYPEINFORESOURCE)
        {
        }


        /// <summary>
        /// Constructor, creates an empty PermissionNameInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public PermissionNameInfo(DataRow dr)
            : base(TYPEINFORESOURCE, dr)
        {
        }

        #endregion
    }
}
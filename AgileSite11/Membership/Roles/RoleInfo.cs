using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(RoleInfo), RoleInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(RoleInfo), RoleInfo.OBJECT_TYPE_GROUP)]

namespace CMS.Membership
{
    /// <summary>
    /// Role info data container.
    /// </summary>
    public class RoleInfo : AbstractInfo<RoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Role object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ROLE;

        /// <summary>
        /// Group role object type
        /// </summary>
        public const string OBJECT_TYPE_GROUP = PredefinedObjectType.GROUPROLE;


        /// <summary>
        /// Type information for standard role.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RoleInfoProvider), OBJECT_TYPE, "CMS.Role", "RoleID", "RoleLastModified", "RoleGUID", "RoleName", "RoleDisplayName", null, "SiteID", "RoleGroupID", null)
        {
            ModuleName = "cms.roles",
            GroupIDColumn = "RoleGroupID",
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = true,
            TypeCondition = new TypeCondition().WhereIsNull("RoleGroupID"),
        };


        /// <summary>
        /// Type information for group role.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = new ObjectTypeInfo(typeof(RoleInfoProvider), OBJECT_TYPE_GROUP, "CMS.Role", "RoleID", "RoleLastModified", "RoleGUID", "RoleName", "RoleDisplayName", null, "SiteID", "RoleGroupID", PredefinedObjectType.GROUP)
        {
            OriginalTypeInfo = TYPEINFO,
            ModuleName = "cms.roles",
            GroupIDColumn = "RoleGroupID",
            ImportExportSettings =
            {
                AllowSingleExport = false,
                LogExport = true
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = true,
            TypeCondition = new TypeCondition().WhereIsNotNull("RoleGroupID"),
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value, 0);
            }
        }


        /// <summary>
        /// Role Display Name.
        /// </summary>
        [DatabaseField("RoleDisplayName")]
        public virtual string RoleDisplayName
        {
            get
            {
                return GetStringValue("RoleDisplayName", string.Empty);
            }
            set
            {
                SetValue("RoleDisplayName", value);
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
                SetValue("RoleID", value);
            }
        }


        /// <summary>
        /// Role Description.
        /// </summary>
        [DatabaseField("RoleDescription")]
        public virtual string RoleDescription
        {
            get
            {
                return GetStringValue("RoleDescription", string.Empty);
            }
            set
            {
                SetValue("RoleDescription", value);
            }
        }

        /// <summary>
        /// Role Name.
        /// </summary>
        [DatabaseField]
        public virtual string RoleName
        {
            get
            {
                return GetStringValue("RoleName", string.Empty);
            }
            set
            {
                SetValue("RoleName", value);
            }
        }


        /// <summary>
        /// Role GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid RoleGUID
        {
            get
            {
                return GetGuidValue("RoleGUID", Guid.Empty);
            }
            set
            {
                SetValue("RoleGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RoleLastModified
        {
            get
            {
                return GetDateTimeValue("RoleLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("RoleLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Role group ID.
        /// </summary>
        [DatabaseField]
        public virtual int RoleGroupID
        {
            get
            {
                return GetIntegerValue("RoleGroupID", 0);
            }
            set
            {
                SetValue("RoleGroupID", value);
            }
        }


        /// <summary>
        /// Indicates whether role is group admin.
        /// </summary>
        [DatabaseField]
        public virtual bool RoleIsGroupAdministrator
        {
            get
            {
                return GetBooleanValue("RoleIsGroupAdministrator", false);
            }
            set
            {
                SetValue("RoleIsGroupAdministrator", value);
            }
        }


        /// <summary>
        /// Indicates whether role is domain role.
        /// </summary>
        [DatabaseField]
        public virtual bool RoleIsDomain
        {
            get
            {
                return GetBooleanValue("RoleIsDomain", false);
            }
            set
            {
                SetValue("RoleIsDomain", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (RoleGroupID == 0)
                {
                    return TYPEINFO;
                }
                else
                {
                    return TYPEINFOGROUP;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RoleInfoProvider.DeleteRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RoleInfoProvider.SetRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RoleInfo object.
        /// </summary>
        public RoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RoleInfo object from the given DataRow.
        /// </summary>
        public RoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
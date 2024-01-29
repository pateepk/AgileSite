using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(DeviceProfileLayoutInfo), DeviceProfileLayoutInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// DeviceProfileLayoutInfo data container class.
    /// </summary>
    public class DeviceProfileLayoutInfo : AbstractInfo<DeviceProfileLayoutInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.deviceprofilelayout";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DeviceProfileLayoutInfoProvider), OBJECT_TYPE, "CMS.DeviceProfileLayout", "DeviceProfileLayoutID", "DeviceProfileLayoutLastModified", "DeviceProfileLayoutGUID", null, null, null, null, "DeviceProfileID", DeviceProfileInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SourceLayoutID", LayoutInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("TargetLayoutID", LayoutInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            IsBinding = true,
            ImportExportSettings =
            {
                LogExport = true
            },
            DefaultData = new DefaultDataSettings(),
            ModuleName = ModuleName.DEVICEPROFILES,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Source layout ID
        /// </summary>
        [DatabaseField]
        public virtual int SourceLayoutID
        {
            get
            {
                return GetIntegerValue("SourceLayoutID", 0);
            }
            set
            {
                SetValue("SourceLayoutID", value);
            }
        }


        /// <summary>
        /// Device profile ID
        /// </summary>
        [DatabaseField]
        public virtual int DeviceProfileID
        {
            get
            {
                return GetIntegerValue("DeviceProfileID", 0);
            }
            set
            {
                SetValue("DeviceProfileID", value);
            }
        }


        /// <summary>
        /// Device profile layout mapping GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid DeviceProfileLayoutGUID
        {
            get
            {
                return GetGuidValue("DeviceProfileLayoutGUID", Guid.Empty);
            }
            set
            {
                SetValue("DeviceProfileLayoutGUID", value);
            }
        }


        /// <summary>
        /// Device profile layout mapping ID
        /// </summary>
        [DatabaseField]
        public virtual int DeviceProfileLayoutID
        {
            get
            {
                return GetIntegerValue("DeviceProfileLayoutID", 0);
            }
            set
            {
                SetValue("DeviceProfileLayoutID", value);
            }
        }


        /// <summary>
        /// Target layout ID
        /// </summary>
        [DatabaseField]
        public virtual int TargetLayoutID
        {
            get
            {
                return GetIntegerValue("TargetLayoutID", 0);
            }
            set
            {
                SetValue("TargetLayoutID", value);
            }
        }


        /// <summary>
        /// Device profile layout mapping last modification timestamp
        /// </summary>
        [DatabaseField]
        public virtual DateTime DeviceProfileLayoutLastModified
        {
            get
            {
                return GetDateTimeValue("DeviceProfileLayoutLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("DeviceProfileLayoutLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DeviceProfileLayoutInfoProvider.DeleteDeviceProfileLayoutInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DeviceProfileLayoutInfoProvider.SetDeviceProfileLayoutInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DeviceProfileLayoutInfo object.
        /// </summary>
        public DeviceProfileLayoutInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DeviceProfileLayoutInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DeviceProfileLayoutInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

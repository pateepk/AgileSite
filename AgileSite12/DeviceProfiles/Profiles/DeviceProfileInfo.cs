using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;

[assembly: RegisterObjectType(typeof(DeviceProfileInfo), DeviceProfileInfo.OBJECT_TYPE)]

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// DeviceProfile data container class.
    /// </summary>
    public class DeviceProfileInfo : AbstractInfo<DeviceProfileInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.deviceprofile";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DeviceProfileInfoProvider), OBJECT_TYPE, "CMS.DeviceProfile", "ProfileID", "ProfileLastModified", "ProfileGUID", "ProfileName", "ProfileDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },

            Feature = FeatureEnum.DeviceProfiles,
            LogEvents = true,
            TouchCacheDependencies = true,
            DefaultOrderBy = "ProfileOrder, ProfileID",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            OrderColumn = "ProfileOrder",
            ModuleName = ModuleName.DEVICEPROFILES,
            EnabledColumn = "ProfileEnabled",
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Profile user agents.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileUserAgents
        {
            get
            {
                return GetStringValue("ProfileUserAgents", "");
            }
            set
            {
                SetValue("ProfileUserAgents", value);
            }
        }


        /// <summary>
        /// Device profile display name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileDisplayName
        {
            get
            {
                return GetStringValue("ProfileDisplayName", "");
            }
            set
            {
                SetValue("ProfileDisplayName", value);
            }
        }


        /// <summary>
        /// Profile macro.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileMacro
        {
            get
            {
                return GetStringValue("ProfileMacro", "");
            }
            set
            {
                SetValue("ProfileMacro", value);
            }
        }


        /// <summary>
        /// Device profile code name.
        /// </summary>
        [DatabaseField]
        public virtual string ProfileName
        {
            get
            {
                return GetStringValue("ProfileName", "");
            }
            set
            {
                SetValue("ProfileName", value);
            }
        }


        /// <summary>
        /// Device profile ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileID
        {
            get
            {
                return GetIntegerValue("ProfileID", 0);
            }
            set
            {
                SetValue("ProfileID", value);
            }
        }


        /// <summary>
        /// Device profile order.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileOrder
        {
            get
            {
                return GetIntegerValue("ProfileOrder", 0);
            }
            set
            {
                SetValue("ProfileOrder", value);
            }
        }


        /// <summary>
        /// Enables or disables the device profile.
        /// </summary>
        [DatabaseField]
        public virtual bool ProfileEnabled
        {
            get
            {
                return GetBooleanValue("ProfileEnabled", true);
            }
            set
            {
                SetValue("ProfileEnabled", value);
            }
        }


        /// <summary>
        /// Device profile preview width in pixels.
        /// </summary>
        [DatabaseField]
        public virtual int ProfilePreviewWidth
        {
            get
            {
                return GetIntegerValue("ProfilePreviewWidth", 0);
            }
            set
            {
                SetValue("ProfilePreviewWidth", value);
            }
        }


        /// <summary>
        /// Device profile preview height in pixels.
        /// </summary>
        [DatabaseField]
        public virtual int ProfilePreviewHeight
        {
            get
            {
                return GetIntegerValue("ProfilePreviewHeight", 0);
            }
            set
            {
                SetValue("ProfilePreviewHeight", value);
            }
        }


        /// <summary>
        /// Device profile Guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ProfileGUID
        {
            get
            {
                return GetGuidValue("ProfileGUID", Guid.Empty);
            }
            set
            {
                SetValue("ProfileGUID", value);
            }
        }


        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public bool UsesExternalStorage
        {
            get
            {
                return StorageHelper.IsExternalStorage(GetThemePath());
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DeviceProfileInfoProvider.DeleteDeviceProfileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DeviceProfileInfoProvider.SetDeviceProfileInfo(this);
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            DeviceProfileInfoProvider.ClearCache(ProfileID);
            DeviceProfileInfoProvider.ClearCurrentProfilesTable(true);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DeviceInfo object.
        /// </summary>
        public DeviceProfileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DeviceInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DeviceProfileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            return "~/App_Themes/Components/DeviceProfile/" + ValidationHelper.GetSafeFileName(ProfileName);
        }

        #endregion
    }
}

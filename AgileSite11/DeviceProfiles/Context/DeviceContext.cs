using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Context data for device profiles
    /// </summary>
    public class DeviceContext : AbstractContext<DeviceContext>
    {
        #region "Variables"

        private CurrentDevice mCurrentDevice;
        private DeviceProfileInfo mCurrentDeviceProfile;
        private List<DeviceProfileInfo> mCurrentDeviceProfiles;

        private string mCurrentDeviceProfileName;
        private string mOriginalCurrentDeviceProfileName;
        private bool? mDevicesAvailable;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns current device information.
        /// </summary>
        [RegisterProperty]
        public static CurrentDevice CurrentDevice
        {
            get
            {
                // Load if not available
                var c = Current;
                if (c.mCurrentDevice == null)
                {
                    // Get current device information
                    c.mCurrentDevice = Service.Resolve<ICurrentDeviceProvider>().GetCurrentDevice();
                }

                return c.mCurrentDevice;
            }
            set
            {
                Current.mCurrentDevice = value;
            }
        }


        /// <summary>
        /// Returns true if device profile are available in current domain license.
        /// </summary>
        private bool DevicesAvailable
        {
            get
            {
                if (!mDevicesAvailable.HasValue)
                {
                    mDevicesAvailable = false;

                    // Check the license
                    string currentDomain = RequestContext.CurrentDomain;
                    if (!String.IsNullOrEmpty(currentDomain))
                    {
                        mDevicesAvailable = LicenseHelper.CheckFeature(currentDomain, FeatureEnum.DeviceProfiles);
                    }
                }

                return mDevicesAvailable.Value;
            }
        }


        /// <summary>
        /// Returns current device profile info.
        /// </summary>
        [RegisterProperty]
        public static DeviceProfileInfo CurrentDeviceProfile
        {
            get
            {
                // Load if not available
                var c = Current;
                if (c.DevicesAvailable && (c.mCurrentDeviceProfile == null))
                {
                    // Get current device profile info
                    c.mCurrentDeviceProfile = DeviceProfileInfoProvider.GetCurrentDeviceProfileInfo(SiteContext.CurrentSiteName);
                }

                return c.mCurrentDeviceProfile;
            }
            set
            {
                Current.mCurrentDeviceProfile = value;
            }
        }


        /// <summary>
        /// Returns list of current device profiles.
        /// </summary>
        [RegisterProperty]
        public static List<DeviceProfileInfo> CurrentDeviceProfiles
        {
            get
            {
                // Load if not available
                var c = Current;
                if (c.DevicesAvailable && (c.mCurrentDeviceProfiles == null))
                {
                    // Get current device profile info
                    c.mCurrentDeviceProfiles = DeviceProfileInfoProvider.GetCurrentDevicProfiles(SiteContext.CurrentSiteName);
                }

                return c.mCurrentDeviceProfiles;
            }
            set
            {
                Current.mCurrentDeviceProfiles = value;
            }
        }


        /// <summary>
        /// Returns current device profile name.
        /// </summary>
        [RegisterColumn]
        public static string CurrentDeviceProfileName
        {
            get
            {
                var c = Current;
                if (c.DevicesAvailable && (c.mCurrentDeviceProfileName == null))
                {
                    c.mCurrentDeviceProfileName = (CurrentDeviceProfile != null ? CurrentDeviceProfile.ProfileName : String.Empty);
                }
                return c.mCurrentDeviceProfileName;
            }
            set
            {
                Current.mCurrentDeviceProfileName = value;
                DeviceProfileInfoProvider.SetCurrentDeviceProfileInfo(value);
            }
        }

        
        /// <summary>
        /// Gets or sets the current device profile name
        /// </summary>
        internal static string OriginalCurrentDeviceProfileName
        {
            get
            {
                return Current.mOriginalCurrentDeviceProfileName;
            }
            set
            {
                Current.mOriginalCurrentDeviceProfileName = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets all device profiles specific CSS class name.
        /// </summary>
        public static string GetDeviceProfilesClass()
        {
            string cssClass = null;

            List<DeviceProfileInfo> profiless = CurrentDeviceProfiles;
            if (profiless != null)
            {
                foreach (DeviceProfileInfo profile in profiless)
                {
                    cssClass += String.Format(" {0}", ValidationHelper.GetIdentifier(profile.ProfileName, ""));
                }

                if (cssClass != null)
                {
                    cssClass = cssClass.Trim();
                }
            }

            return cssClass;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.MacroEngine;

namespace CMS.DeviceProfiles
{
    using ProfilesDictionary = SafeDictionary<string, List<DeviceProfileInfo>>;

    /// <summary>
    /// Class providing Device profile management.
    /// </summary>
    public class DeviceProfileInfoProvider : AbstractInfoProvider<DeviceProfileInfo, DeviceProfileInfoProvider>
    {
        #region "Variables"

        private static CMSStatic<ProfilesDictionary> mCurrentProfilesTable = new CMSStatic<ProfilesDictionary>();
        private static int? mMaxDeviceProfilesEntries;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Table of the device profiles lists indexed by site user agent.
        /// </summary>
        private static ProfilesDictionary CurrentProfilesTable
        {
            get
            {
                return mCurrentProfilesTable.Value ?? (mCurrentProfilesTable.Value = new ProfilesDictionary());
            }
        }


        /// <summary>
        /// Maximum number of entries stored in CurrentProfilesTable.
        /// </summary>
        private static int MaxDeviceProfilesEntries
        {
            get
            {
                if (!mMaxDeviceProfilesEntries.HasValue)
                {
                    mMaxDeviceProfilesEntries = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSMaxDeviceProfilesEntries"], 1000);
                }

                return mMaxDeviceProfilesEntries.Value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current device profile name stored in cookie
        /// </summary>
        public static string CurrentDeviceProfileName
        {
            get
            {
                return (string)ContextHelper.GetItem(CookieName.CurrentDeviceProfileName, true, false, true);
            }
            set
            {
                SetCurrentDeviceProfileInfo(value);
            }
        }

        #endregion


        #region "Constants"

        /// <summary>
        /// Query parameter name for loading device profile from cookies.
        /// </summary>
        public const string DEVICES_QUERY_PARAM = "loaddevice";


        /// <summary>
        /// Query parameter name for loading device profile from query.
        /// </summary>
        public const string DEVICENAME_QUERY_PARAM = "devicename";


        /// <summary>
        /// Name of cache key for automatic image resizing for device profiles.
        /// </summary>
        public const string DEVICE_IMAGE_CACHE_KEY = "deviceimage";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public DeviceProfileInfoProvider()
            : base(DeviceProfileInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                GUID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all device profiles.
        /// </summary>
        public static ObjectQuery<DeviceProfileInfo> GetDeviceProfiles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns device info with specified ID.
        /// </summary>
        /// <param name="infoId">Device info ID.</param>        
        public static DeviceProfileInfo GetDeviceProfileInfo(int infoId)
        {
            return ProviderObject.GetInfoById(infoId);
        }


        /// <summary>
        /// Returns device info with specified GUID.
        /// </summary>
        /// <param name="deviceGuid">Device GUID</param>                
        /// <param name="siteName">Site name</param>                
        public static DeviceProfileInfo GetDeviceProfileInfo(Guid deviceGuid, string siteName)
        {
            return ProviderObject.GetInfoByGuid(deviceGuid, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns device info with specified name.
        /// </summary>
        /// <param name="infoName">Device info name.</param>                
        public static DeviceProfileInfo GetDeviceProfileInfo(string infoName)
        {
            return ProviderObject.GetInfoByCodeName(infoName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified device info.
        /// </summary>
        /// <param name="infoObj">Device info to be set.</param>
        public static void SetDeviceProfileInfo(DeviceProfileInfo infoObj)
        {
            ProviderObject.SetDeviceProfileInfoInternal(infoObj);

            ClearCache(infoObj.ProfileID);

            ClearCurrentProfilesTable(true);
        }


        /// <summary>
        /// Deletes specified device info.
        /// </summary>
        /// <param name="infoObj">Device info to be deleted.</param>
        public static void DeleteDeviceProfileInfo(DeviceProfileInfo infoObj)
        {
            ProviderObject.DeleteDeviceProfileInfoInternal(infoObj);
            if (infoObj != null)
            {
                ClearCache(infoObj.ProfileID);

                ClearCurrentProfilesTable(true);
            }
        }


        /// <summary>
        /// Deletes device info with specified ID.
        /// </summary>
        /// <param name="infoId">Device info ID.</param>
        public static void DeleteDeviceProfileInfo(int infoId)
        {
            DeviceProfileInfo infoObj = GetDeviceProfileInfo(infoId);

            ClearCache(infoObj.ProfileID);

            DeleteDeviceProfileInfo(infoObj);

            ClearCurrentProfilesTable(true);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Initialize profiles order.
        /// </summary>
        public static void InitProfilesOrder()
        {
            ProviderObject.InitProfilesOrderInternal();
        }

        /// <summary>
        /// Moves profile up.
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        public static void MoveProfileUp(int profileId)
        {
            ProviderObject.MoveProfileUpInternal(profileId);
        }


        /// <summary>
        /// Moves profile down.
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        public static void MoveProfileDown(int profileId)
        {
            ProviderObject.MoveProfileDownInternal(profileId);
        }


        /// <summary>
        /// Returns current device profile info.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static DeviceProfileInfo GetCurrentDeviceProfileInfo(string siteName)
        {
            return ProviderObject.GetCurrentDeviceProfileInfoInternal(siteName, false, true);
        }


        /// <summary>
        /// Returns current device profile info.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="loadFromCookies">Indicates if device profile is loaded from cookie instead of current browser device</param>
        public static DeviceProfileInfo GetCurrentDeviceProfileInfo(string siteName, bool loadFromCookies)
        {
            return ProviderObject.GetCurrentDeviceProfileInfoInternal(siteName, loadFromCookies, true);
        }


        /// <summary>
        /// Returns current original device profile name (cannot be overridden by URL parameter or cookie).
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetOriginalCurrentDevicProfileName(string siteName)
        {
            string profileName = DeviceContext.OriginalCurrentDeviceProfileName;
            if (profileName == null)
            {
                DeviceProfileInfo dpi = ProviderObject.GetCurrentDeviceProfileInfoInternal(siteName, false, false);
                profileName = (dpi != null) ? dpi.ProfileName : String.Empty;
                DeviceContext.OriginalCurrentDeviceProfileName = profileName;
            }
            return profileName;
        }


        /// <summary>
        /// Returns ordered list of device profiles matching current device (can be overridden by URL parameter or cookie).
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static List<DeviceProfileInfo> GetCurrentDevicProfiles(string siteName)
        {
            return ProviderObject.GetCurrentDeviceProfilesInternal(siteName, false, true);
        }


        /// <summary>
        /// Sets current device profile info.
        /// </summary>
        /// <param name="profileName">Device profile code name</param>
        public static void SetCurrentDeviceProfileInfo(string profileName)
        {
            ProviderObject.SetCurrentDeviceProfileInfoInternal(profileName);
        }


        /// <summary>
        /// Returns order of the last profile.
        /// </summary>
        public static int GetLastProfileOrder()
        {
            var lastOrder = GetDeviceProfiles()
                        .TopN(1)
                        .Column("ProfileOrder")
                        .OrderByDescending("ProfileOrder")
                        .GetScalarResult(0);

            return lastOrder;
        }


        /// <summary>
        /// Returns true if device profiles are enabled for given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool IsDeviceProfilesEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSDeviceProfilesEnable");
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Sets (updates or inserts) specified device info.
        /// </summary>
        /// <param name="infoObj">Device info to be set.</param>        
        protected virtual void SetDeviceProfileInfoInternal(DeviceProfileInfo infoObj)
        {
            SetInfo(infoObj);
            ClearCache(infoObj.ProfileID);
            CurrentProfilesTable.Clear();
        }


        /// <summary>
        /// Deletes specified device info.
        /// </summary>
        /// <param name="infoObj">Device info to be deleted.</param>        
        protected virtual void DeleteDeviceProfileInfoInternal(DeviceProfileInfo infoObj)
        {
            DeleteInfo(infoObj);
            ClearCache(infoObj.ProfileID);
            CurrentProfilesTable.Clear();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            ClearCurrentProfilesTable(logTasks);
            CacheHelper.TouchKey(DEVICE_IMAGE_CACHE_KEY, logTasks, false);
        }


        /// <summary>
        /// Initializes sorting order of profiles.
        /// </summary>
        protected virtual void InitProfilesOrderInternal()
        {
            new DeviceProfileInfo().Generalized.InitObjectsOrder();
        }


        /// <summary>
        /// Moves profile up.
        /// </summary>
        /// <param name="profileId">Profile ID</param> 
        protected virtual void MoveProfileUpInternal(int profileId)
        {
            DeviceProfileInfo dpi = GetDeviceProfileInfo(profileId);
            if (dpi != null)
            {
                dpi.Generalized.MoveObjectUp();
            }
        }


        /// <summary>
        /// Moves profile up.
        /// </summary>
        /// <param name="profileId">Profile ID</param> 
        protected virtual void MoveProfileDownInternal(int profileId)
        {
            DeviceProfileInfo dpi = GetDeviceProfileInfo(profileId);
            if (dpi != null)
            {
                dpi.Generalized.MoveObjectDown();
            }
        }


        /// <summary>
        /// Returns current device profile info based on current user agent.
        /// The value based on the user agent can be overwritten by query string parameter or by cookie (CMSCurrentDeviceInfo).
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="loadFromCookies">Indicates if device profile is loaded from cookie instead of current browser device</param>
        /// <param name="canBeOverridden">Indicates if device profile can be overridden by URL parameter or cookie at all.</param>
        protected virtual List<DeviceProfileInfo> GetCurrentDeviceProfilesInternal(string siteName, bool loadFromCookies, bool canBeOverridden)
        {
            if (!IsDeviceProfilesEnabled(siteName))
            {
                return new List<DeviceProfileInfo>();
            }

            List<DeviceProfileInfo> currentProfiles = new List<DeviceProfileInfo>();

            string currentProfileName = null;

            if (canBeOverridden)
            {
                // Try get device profile name from context
                currentProfileName = ContextHelper.GetItem(CookieName.CurrentDeviceProfileName, true, false, false) as String;

                // Try to load from query string if not found in context
                if (currentProfileName == null)
                {
                    currentProfileName = QueryHelper.GetString(DEVICENAME_QUERY_PARAM, null);
                    if (currentProfileName != null)
                    {
                        SetCurrentDeviceProfileInfo(currentProfileName);
                    }
                }

                // Load current device info from cookie if not live site mode or is on live site view tab
                if (currentProfileName == null)
                {
                    if (LoadFromCookies(loadFromCookies))
                    {
                        currentProfileName = CurrentDeviceProfileName;
                    }
                    else if (Service.Resolve<ISiteService>().IsLiveSite && (ValidationHelper.GetBoolean(CookieHelper.GetValue(CookieName.ShowDesktopVersion), false)))
                    {
                        currentProfileName = "";
                    }
                }
            }

            if (currentProfileName == null)
            {
                string currentUserAgent = BrowserHelper.GetUserAgent();
                if (!String.IsNullOrEmpty(currentUserAgent))
                {
                    string currentUserAgentLower = currentUserAgent.ToLowerCSafe();
                    if (CurrentProfilesTable.ContainsKey(currentUserAgentLower))
                    {
                        return CurrentProfilesTable[currentUserAgentLower];
                    }
                    else
                    {
                        var profiles = GetDeviceProfiles()
                           .WhereEquals("ProfileEnabled", 1)
                           .OrderBy("ProfileOrder");

                        foreach (var profile in profiles.ToList())
                        {
                            if ((profile != null) && IsCurrentProfile(currentUserAgent, profile))
                            {
                                currentProfiles.Add(profile);
                            }
                        }

                        AddToCurrentProfilesTable(currentUserAgentLower, currentProfiles);
                    }
                }
            }
            else
            {
                DeviceProfileInfo profile = GetInfoByCodeName(currentProfileName);
                if (profile != null)
                {
                    currentProfiles.Add(profile);
                }
            }

            return currentProfiles;
        }


        /// <summary>
        /// Adds list of current devices to dictionary and clears table if is too big.
        /// </summary>
        /// <param name="userAgent">User agent</param>
        /// <param name="profiles">List of profiles</param>
        private static void AddToCurrentProfilesTable(string userAgent, List<DeviceProfileInfo> profiles)
        {
            if (CurrentProfilesTable.Count >= MaxDeviceProfilesEntries)
            {
                CurrentProfilesTable.Clear();
            }

            CurrentProfilesTable[userAgent] = profiles;
        }


        /// <summary>
        /// Clear hash table CurrentProfilesTable.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        internal static void ClearCurrentProfilesTable(bool logTask)
        {
            CurrentProfilesTable.Clear();
            if (logTask)
            {
                ProviderObject.CreateWebFarmTask("ClearCurrentProfilesTable", null);
            }
        }


        /// <summary>
        /// Clears cache for device profile.
        /// </summary>
        /// <param name="profileId">Device profile ID</param>
        internal static void ClearCache(int profileId)
        {
            DeviceProfileInfo profileInfo = GetDeviceProfileInfo(profileId);
            if (profileInfo != null)
            {
                CacheHelper.TouchKey("cms.deviceprofile|byname|" + profileInfo.ProfileName, true, false);
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            switch (actionName)
            {
                case "ClearCurrentProfilesTable":
                    ClearCurrentProfilesTable(false);
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception(String.Format("[{0}.ProcessWebFarmTask] The action name '{1}' has no supporting code.", TypeInfo.ObjectType, actionName));
            }
        }


        /// <summary>
        /// Returns current device profile info.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="loadFromCookies">Indicates if device profile is loaded from cookie instead of current browser device</param>
        /// <param name="canBeOverridden">Indicates if device profile can be overridden by URL parameter or cookie at all.</param>
        protected virtual DeviceProfileInfo GetCurrentDeviceProfileInfoInternal(string siteName, bool loadFromCookies, bool canBeOverridden)
        {
            List<DeviceProfileInfo> deviceProfiles = GetCurrentDeviceProfilesInternal(siteName, loadFromCookies, canBeOverridden);
            if (deviceProfiles.Count > 0)
            {
                // Return first device profile
                return deviceProfiles[0];
            }

            return null;
        }


        /// <summary>
        /// Returns true if device info should be loaded from cookies.
        /// </summary>
        /// <param name="force">Force load</param>
        protected bool LoadFromCookies(bool force)
        {
            if (force)
            {
                return true;
            }

            if (!Service.Resolve<ISiteService>().IsLiveSite)
            {
                return true;
            }

            if (QueryHelper.GetBoolean(DEVICES_QUERY_PARAM, false))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Sets current device profile info.
        /// </summary>
        /// <param name="profileName">Profile code name</param>
        protected virtual void SetCurrentDeviceProfileInfoInternal(string profileName)
        {
            if (profileName != null)
            {
                // Set cookies only if ObjectLifeTime query parameter is not set to request
                ObjectLifeTimeEnum lifeTime = ObjectLifeTimeFunctions.GetCurrentObjectLifeTime("Device");
                bool sc = (lifeTime == ObjectLifeTimeEnum.Cookies);

                ContextHelper.Add(CookieName.CurrentDeviceProfileName, profileName, true, false, sc, DateTime.Now.AddYears(1));
            }
            else
            {
                // Remove all possibly stored keys
                ContextHelper.Remove(CookieName.CurrentDeviceProfileName, true, false, true);
            }
        }


        /// <summary>
        /// Detects if given profile match current device.
        /// </summary>
        /// <param name="userAgent">Current user agent</param>
        /// <param name="info">Device profile info</param>
        private bool IsCurrentProfile(String userAgent, DeviceProfileInfo info)
        {
            bool isCurrent = false;
            if (!String.IsNullOrEmpty(info.ProfileUserAgents))
            {
                isCurrent = IsCurrentProfileAgent(userAgent, info);
            }

            if (!isCurrent && !String.IsNullOrEmpty(info.ProfileMacro))
            {
                isCurrent = IsCurrentProfileMacro(info);
            }

            return isCurrent;
        }


        /// <summary>
        /// Returns true if device profile user agents list contains current user agent.
        /// </summary>
        /// <param name="userAgent">Current user agent string</param>
        /// <param name="info">Device profile information</param>
        private static bool IsCurrentProfileAgent(String userAgent, DeviceProfileInfo info)
        {
            String[] agents = info.ProfileUserAgents.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return agents.Any(agent => (userAgent.IndexOfCSafe(agent.Trim(), true) >= 0));
        }


        /// <summary>
        /// Returns true if given device profile macro is true.
        /// </summary>
        /// <param name="info">Device profile information</param>
        private static bool IsCurrentProfileMacro(DeviceProfileInfo info)
        {
            string macroValue = MacroResolver.Resolve(info.ProfileMacro);
            if (!String.IsNullOrEmpty(macroValue))
            {
                return ValidationHelper.GetBoolean(macroValue, false);
            }

            return false;
        }

        #endregion

    }
}

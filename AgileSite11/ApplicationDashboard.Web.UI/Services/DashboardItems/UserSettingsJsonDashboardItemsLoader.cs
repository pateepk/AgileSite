using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Membership;
using CMS.Modules;

using Newtonsoft.Json;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Handles loading of applications from the user settings. 
    /// </summary>
    internal class UserSettingsJsonDashboardItemsLoader : IUserSpecificDashboardItemsLoader
    {
        private readonly IDashboardItemProvider mDashboardItemProvider;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dashboardItemProvider">Provider containing methods for obtaining filtered lists of applications.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dashboardItemProvider"/> is null</exception>
        public UserSettingsJsonDashboardItemsLoader(IDashboardItemProvider dashboardItemProvider)
        {
            if (dashboardItemProvider == null)
            {
                throw new ArgumentNullException("dashboardItemProvider");
            }

            mDashboardItemProvider = dashboardItemProvider;
        }


        /// <summary>
        /// Gets dictionary containing user dashboard setting object and corresponding UI element for <paramref name="user"/> 
        /// based on their preferences.
        /// </summary>
        /// <param name="user">User with preferences</param>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> is null</exception>
        /// <returns>Dictionary of user dashboard setting and UI elements representing the user applications</returns>
        public Dictionary<UserDashboardSetting, DashboardItem> GetUserSpecificDashboardItems(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string jsonDashboardApplications = user.UserSettings.UserDashboardApplications;
            if (string.IsNullOrEmpty(jsonDashboardApplications))
            {
                return null;
            }

            var settings = JsonConvert.DeserializeObject<List<UserDashboardSetting>>(jsonDashboardApplications) ?? new List<UserDashboardSetting>();
            var applicationGUIDs = settings.Select(s => s.ApplicationGuid).ToList();
            var applications = mDashboardItemProvider.GetFilteredApplicationsByGuids(user, applicationGUIDs);

            return settings.ToDictionary(setting => setting, setting =>
            {
                var singleElement = setting.ElementGuid.HasValue ? UIElementInfoProvider.GetUIElementInfo(setting.ElementGuid.Value) : null;

                return new DashboardItem
                {
                    Application = applications.SingleOrDefault(application => application.ElementGUID == setting.ApplicationGuid) ?? 
                        UIElementInfoProvider.GetUIElementInfo(setting.ApplicationGuid),
                    SingleObject = singleElement,
                    IsVisible = applications.Any(application => (application.ElementGUID == setting.ApplicationGuid)) &&
                                (!setting.ElementGuid.HasValue || singleElement != null)
                };
            });
        }


        /// <summary>
        /// Saves given list of user dashboard settings to the user settings.
        /// </summary>
        /// <param name="user">User the applications will be saved to</param>
        /// <param name="userDashboardSettings">List of dashboard settings</param>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> is null -or- <paramref name="userDashboardSettings"/> is null</exception>
        public void SaveUserSpecificDashboardSettings(UserInfo user, List<UserDashboardSetting> userDashboardSettings)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (userDashboardSettings == null)
            {
                throw new ArgumentNullException("userDashboardSettings");
            }

            user.UserSettings.UserDashboardApplications = JsonConvert.SerializeObject(userDashboardSettings);
            user.UserSettings.Update();
        }


        /// <summary>
        /// Removes given <paramref name="userDashboardSetting"/> from current user dashboard settings.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="userDashboardSetting">Setting to be removed</param>
        public void RemoveSpecificUserDashboardSetting(UserInfo user, UserDashboardSetting userDashboardSetting)
        {
            var settings = GetUserSpecificDashboardItems(user).Keys.ToList();
            settings.RemoveAll(setting => 
                (setting.ElementGuid == userDashboardSetting.ElementGuid) && 
                (setting.ObjectName == userDashboardSetting.ObjectName) && 
                (setting.ApplicationGuid == userDashboardSetting.ApplicationGuid) &&
                (setting.ObjectSiteName == userDashboardSetting.ObjectSiteName)
            );

            SaveUserSpecificDashboardSettings(user, settings);
        }
    }
}
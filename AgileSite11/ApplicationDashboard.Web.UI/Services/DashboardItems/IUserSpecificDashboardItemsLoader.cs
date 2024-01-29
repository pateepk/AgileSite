using System.Collections.Generic;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(IUserSpecificDashboardItemsLoader), typeof(UserSettingsJsonDashboardItemsLoader), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Handles loading of applications from the user settings. 
    /// </summary>
    internal interface IUserSpecificDashboardItemsLoader
    {
        /// <summary>
        /// Gets dictionary containing user dashboard setting object and corresponding UI element for <paramref name="user"/> base on their preferences.
        /// </summary>
        /// <param name="user">User with preferences</param>
        /// <returns>Dictionary of user dashboard setting and UI elements representing the user applications</returns>
        Dictionary<UserDashboardSetting, DashboardItem> GetUserSpecificDashboardItems(UserInfo user);


        /// <summary>
        /// Saves given list of user dashboard settings to the user settings.
        /// </summary>
        /// <param name="user">User the applications will be saved to</param>
        /// <param name="userDashboardSettings">List of dashboard settings</param>
        void SaveUserSpecificDashboardSettings(UserInfo user, List<UserDashboardSetting> userDashboardSettings);


        /// <summary>
        /// Removes given <paramref name="userDashboardSetting"/> from current user dashboard settings.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="userDashboardSetting">Setting to be removed</param>
        void RemoveSpecificUserDashboardSetting(UserInfo user, UserDashboardSetting userDashboardSetting);
    }
}
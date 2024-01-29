using System;

using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to setting values.
    /// </summary>
    /// <pageTitle>Settings</pageTitle>
    internal class Settings
    {
        /// <heading>Getting the values of settings</heading>
        private void GetSettingValue()
        {
            // Note: To find the code names of setting keys, open the Modules application in Kentico, edit a module and view the Settings tab

            // Gets the value of the "Page not found URL" setting for the current site
            string pageNotFoundUrl = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSPageNotFoundUrl");

            // Gets the global value of the "Scheduled tasks enabled" setting 
            bool scheduledTasksEnabled = SettingsKeyInfoProvider.GetBoolValue("CMSSchedulerTasksEnabled");

            // Gets the value of the "Application scheduler interval" setting for the current site
            int schedulerInterval = SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSSchedulerInterval");
        }


        /// <heading>Getting the values of web.config keys</heading>
        private void GetWebConfigSetting()
        {
            // Gets the value of the "CMSApplicationName" key from the appSettings section of the web.config
            string webConfigSetting = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSApplicationName"], "");
        }


        /// <heading>Setting values for settings</heading>
        private void SetSettingValue()
        {
            // Sets the value of the "Page not found URL" setting to "~/System-pages/NotFound" for the current site
            SettingsKeyInfoProvider.SetValue("CMSPageNotFoundUrl", SiteContext.CurrentSiteName, "~/System-pages/NotFound");

            // Sets the value of the global "Scheduled tasks enabled" setting to false
            SettingsKeyInfoProvider.SetGlobalValue("CMSSchedulerTasksEnabled", false);

            // Sets the value of the "Application scheduler interval" setting to 30 for the current site
            SettingsKeyInfoProvider.SetValue("CMSSchedulerInterval", SiteContext.CurrentSiteName, 30);
        }
    }
}

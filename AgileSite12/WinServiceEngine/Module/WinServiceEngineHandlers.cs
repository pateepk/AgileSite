using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.HealthMonitoring;
using CMS.Scheduler;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Event handlers for the module WinServiceEngine
    /// </summary>
    internal class WinServiceEngineHandlers
    {
        /// <summary>
        /// Initialize the handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += ClearServiceSettings;
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        private static void ClearServiceSettings(object sender, SettingsKeyChangedEventArgs e)
        {
            string serviceDefinitionName = null;
            bool serviceEnabled = false;
            string key = e.KeyName;

            // Clear the cached translation service settings
            switch (key.ToLowerCSafe())
            {
                case "cmsscheduleruseexternalservice":
                case "cmsschedulerserviceinterval":
                case "cmsschedulertasksenabled":
                    serviceDefinitionName = WinServiceHelper.SCHEDULER_SERVICE_BASENAME;
                    serviceEnabled = SchedulingHelper.UseExternalService;

                    if (key.EqualsCSafe("cmsschedulertasksenabled", true))
                    {
                        // Clear the enabled flag
                        SchedulingHelper.Clear(true);
                    }
                    break;

                case "cmsuseexternalservice":
                case "cmsservicehealthmonitoringinterval":
                case "cmsenablehealthmonitoring":
                    serviceDefinitionName = WinServiceHelper.HM_SERVICE_BASENAME;
                    serviceEnabled = HealthMonitoringHelper.UseExternalService;

                    if (key.EqualsCSafe("cmsenablehealthmonitoring", true))
                    {
                        // Clear the enabled flag
                        HealthMonitoringHelper.Clear(true);
                    }
                    break;
            }

            // No service to handle
            if (serviceDefinitionName == null)
            {
                return;
            }

            try
            {
                if (serviceEnabled)
                {
                    WinServiceHelper.RestartService(serviceDefinitionName, true);
                }
                else
                {
                    WinServiceHelper.DeleteServiceFile(serviceDefinitionName, true);
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("Settings", "RestartService", ex);
            }
        }
    }
}

using System;
using System.Linq;
using System.Text;

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
            string serviceName = null;
            bool serviceEnabled = false;
            string key = e.KeyName;

            // Clear the cached translation service settings
            switch (key.ToLowerCSafe())
            {
                case "cmsscheduleruseexternalservice":
                case "cmsschedulerserviceinterval":
                case "cmsschedulertasksenabled":
                    serviceName = WinServiceHelper.SCHEDULER_SERVICE_BASENAME;
                    serviceEnabled = SchedulingHelper.UseExternalService;

                    if (key.EqualsCSafe("cmsschedulertasksenabled", true))
                    {
                        // Clear the enabled flag
                        SchedulingHelper.Clear();
                    }
                    break;

                case "cmsuseexternalservice":
                case "cmsservicehealthmonitoringinterval":
                case "cmsenablehealthmonitoring":
                    serviceName = WinServiceHelper.HM_SERVICE_BASENAME;
                    serviceEnabled = HealthMonitoringHelper.UseExternalService;

                    if (key.EqualsCSafe("cmsenablehealthmonitoring", true))
                    {
                        // Clear the enabled flag
                        HealthMonitoringHelper.Clear();
                    }
                    break;
            }

            // No service to handle
            if (serviceName == null)
            {
                return;
            }

            try
            {
                var def = WinServiceHelper.GetServiceDefinition(serviceName);
                if (def != null)
                {
                    if (serviceEnabled)
                    {
                        WinServiceHelper.RestartService(def.GetServiceName());
                    }
                    else
                    {
                        WinServiceHelper.DeleteServiceFile(def.GetServiceName());
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("Settings", "RestartService", ex);
            }
        }
    }
}

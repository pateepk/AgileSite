using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm sync handlers.
    /// </summary>
    internal class WebFarmSyncHandlers
    {
        /// <summary>
        /// Initializes the handlers.
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsWebSite)
            {
                ApplicationEvents.Initialized.Execute += InitializeWebFarm;
                ApplicationEvents.PostStart.Execute += (sender, args) => AnonymousTasksProcesor.RegisterWatchers();

                if (SystemContext.IsRunningOnAzure)
                {
                    ApplicationEvents.End.Execute += (sender, args) => WebFarmServerInfoProvider.DeleteDynamicServer();
                }

                RequestEvents.RunEndRequestTasks.Execute += RestartIfWebFarmReconfigured;
                RequestEvents.Begin.Execute += StartWebFarmThreads;
                SettingsKeyInfoProvider.OnSettingsKeyChanged += CheckWebFarmSettingsChange;
            }
        }


        /// <summary>
        /// Ensures running web farm task processing
        /// </summary>
        private static void StartWebFarmThreads(object sender, EventArgs e)
        {
            if (!WebFarmContext.WebFarmEnabled)
            {
                return;
            }

            WebFarmTaskProcessor.Current.EnsureRunningThread();
            WebFarmMonitor.Current.EnsureRunningThread();
            WebFarmTaskCreator.Current.EnsureRunningThread();
        }


        /// <summary>
        /// Restarts server if web farms were reconfigured
        /// </summary>
        private static void RestartIfWebFarmReconfigured(object sender, EventArgs e)
        {
            if (ReconfigurationRestartRequired())
            {
                SystemHelper.RestartApplication(SystemContext.WebApplicationPhysicalPath);
            }
        }


        /// <summary>
        /// Returns true when server should be restarted because of current server reconfiguration.
        /// </summary>
        private static bool ReconfigurationRestartRequired()
        {
            var restartRequired =  (WebFarmContext.WebFarmMode != WebFarmModeEnum.Disabled) &&
                   ((WebFarmContext.ServerId == 0) || WebFarmContext.EnabledServers.All(s => s.ServerID != WebFarmContext.ServerId)) &&
                   !DataHelper.DataSourceIsEmpty(WebFarmServerInfoProvider.GetWebFarmServers().WhereEquals("ServerName", WebFarmContext.ServerName).WhereTrue("ServerEnabled").Execute());

            if (!restartRequired && WebFarmContext.WebFarmMode == WebFarmModeEnum.Manual)
            {
                // Find a server with the name associated with this webfarm but with different server ID.
                // It means server ID has changed without clearing the current server's cache (WebFarmContext.ServerId) 
                var server = WebFarmServerInfoProvider.GetWebFarmServers().TopN(1)
                    .WhereNotEquals(nameof(WebFarmServerInfo.ServerID), WebFarmContext.ServerId)
                    .WhereEquals(nameof(WebFarmServerInfo.ServerName), WebFarmContext.ServerName)
                    .FirstObject;

                restartRequired = server != null;
            }

            return restartRequired;
        }


        /// <summary>
        /// Initializes web farm functionality after application initialized
        /// </summary>
        private static void InitializeWebFarm(object sender, EventArgs args)
        {
            if (!WebFarmContext.WebFarmEnabled)
            {
                return;
            }

            WebFarmServerInfoProvider.EnsureAutomaticServer();
            WebFarmTaskCleaner.DeleteOldMemorySynchronizationTasksAsync();
        }


        /// <summary>
        /// Handles the change of setting values that are important for proper web farm operation.
        /// </summary>
        private static void CheckWebFarmSettingsChange(object sender, SettingsKeyChangedEventArgs e)
        {
            if (e.KeyName.EqualsCSafe("CMSWebFarmMode", true) || e.KeyName.EqualsCSafe("CMSWebFarmMaxFileSize", true))
            {
                WebFarmContext.Clear();
            }
        }
    }
}
using CMS.Base;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Runs the web farm keep-alive mechanism.
    /// </summary>
    internal class WebFarmMonitor : ThreadWorker<WebFarmMonitor>
    {
        /// <summary>
        /// Gets the interval in milliseconds for the worker.
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return 20000;
            }
        }


        /// <summary>
        /// Indicates that system is running as external web application e.g. MVC.
        /// </summary>
        private static bool IsExternalWebApp
        {
            get
            {
                return SystemContext.IsWebSite && !SystemContext.IsCMSRunningAsMainApplication;
            }
        }


        /// <summary>
        /// Creates <see cref="ThreadSettings"/> object with empty context.
        /// </summary>
        protected override ThreadSettings CreateThreadSettings()
        {
            var originalSettings = base.CreateThreadSettings();
            originalSettings.UseEmptyContext = true;
            return originalSettings;
        }


        /// <summary>
        /// Method processing actions.
        /// </summary>
        protected override void Process()
        {
            WebFarmServerMonitoringInfoProvider.DoPing();

            UpdateExternalWebAppFlagOfCurrentServer();

            WebFarmServerLogInfoProvider.CheckServerStatusChanges();

            if (!WebFarmContext.WebFarmEnabled)
            {
                StopExecution();
            }
        }


        /// <summary>
        /// Removes history of current instance on start of monitoring.
        /// </summary>
        protected override void Initialize()
        {
            WebFarmServerMonitoringInfoProvider.ClearOldMonitoringHistory(WebFarmContext.ServerId);
        }


        /// <summary>
        /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
        /// </summary>
        protected override void Finish()
        {
        }


        /// <summary>
        /// Updates current server's <see cref="WebFarmServerInfo.IsExternalWebAppServer"/> flag to reflect current system configuration.
        /// The flag is inferred from <see cref="SystemContext.IsWebSite"/> and <see cref="SystemContext.IsCMSRunningAsMainApplication"/>.
        /// </summary>
        private static void UpdateExternalWebAppFlagOfCurrentServer()
        {
            var server = WebFarmServerInfoProvider.GetWebFarmServerInfo(WebFarmContext.ServerId);
            if (server == null)
            {
                return;
            }

            if (server.IsExternalWebAppServer != IsExternalWebApp)
            {
                server.IsExternalWebAppServer = IsExternalWebApp;
                server.Update();
            }
        }
    }
}

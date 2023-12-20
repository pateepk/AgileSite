using System;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Sets server to healthy status.
    /// </summary>
    internal class HealthyCommand : IStatusCommand
    {
        /// <summary>
        /// Code of the command.
        /// </summary>
        public WebFarmServerStatusEnum Status
        {
            get
            {
                return WebFarmServerStatusEnum.Healthy;
            }
        }


        /// <summary>
        /// Executes current command.
        /// </summary>
        /// <param name="server">Server over which the command should be executed.</param>
        public void Execute(WebFarmServerInfo server)
        {
            var eventInfo = EventLogProvider.LogEvent(EventType.INFORMATION, "WebFarmMonitor", String.Format("{0} IS HEALTHY", server.ServerName), ResHelper.GetStringFormat("WebFarmMonitor.ServerIsHealthy", server.ServerDisplayName));
            EventLogHelper.SendEmailNotification(eventInfo, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Sets server to not responding status.
    /// </summary>
    internal class NotRespondingCommand : IStatusCommand
    {
        /// <summary>
        /// Code of the command.
        /// </summary>
        public WebFarmServerStatusEnum Status
        {
            get
            {
                return WebFarmServerStatusEnum.NotResponding;
            }
        }


        /// <summary>
        /// Executes current command.
        /// </summary>
        /// <param name="server">Server over which the command should be executed.</param>
        public void Execute(WebFarmServerInfo server)
        {
            var eventInfo = EventLogProvider.LogEvent(EventType.WARNING, "WebFarmMonitor", String.Format("{0} NOT RESPONDING", server.ServerName), ResHelper.GetStringFormat("WebFarmMonitor.ServerNotResponding", server.ServerDisplayName));
            EventLogHelper.SendEmailNotification(eventInfo, null);
        }
    }
}

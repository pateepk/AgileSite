using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Sets server to auto disabled status.
    /// </summary>
    internal class AutoDisabledCommand : IStatusCommand
    {
        /// <summary>
        /// Code of the command.
        /// </summary>
        public WebFarmServerStatusEnum Status
        {
            get
            {
                return WebFarmServerStatusEnum.AutoDisabled;
            }
        }


        /// <summary>
        /// Executes current command.
        /// </summary>
        /// <param name="server">Server over which the command should be executed.</param>
        public void Execute(WebFarmServerInfo server)
        {
            if (WebFarmContext.WebFarmMode == WebFarmModeEnum.Automatic)
            {
                WebFarmServerInfoProvider.DeleteWebFarmServerInfo(server.ServerID);
            }
            else
            {
                WebFarmTaskInfoProvider.DeleteServerMemoryTasks(server.ServerID);
            }
        }
    }
}

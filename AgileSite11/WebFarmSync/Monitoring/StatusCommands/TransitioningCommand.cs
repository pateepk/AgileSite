using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Sets server to transitioning status.
    /// </summary>
    internal class TransitioningCommand : IStatusCommand
    {
        /// <summary>
        /// Code of the command.
        /// </summary>
        public WebFarmServerStatusEnum Status
        {
            get
            {
                return WebFarmServerStatusEnum.Transitioning;
            }
        }


        /// <summary>
        /// Executes current command.
        /// </summary>
        /// <param name="server">Server over which the command should be executed.</param>
        public void Execute(WebFarmServerInfo server)
        {
        }
    }
}

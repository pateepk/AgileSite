using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Encapsulates logic that is related to server log code.
    /// </summary>
    interface IStatusCommand
    {
        /// <summary>
        /// Code of the command.
        /// </summary>
        WebFarmServerStatusEnum Status
        {
            get;
        }


        /// <summary>
        /// Executes current command.
        /// </summary>
        /// <param name="server">Server over which the command should be executed.</param>
        void Execute(WebFarmServerInfo server);
    }
}

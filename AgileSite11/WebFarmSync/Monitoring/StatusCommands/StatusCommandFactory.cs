using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Factory for status commands of given server.
    /// </summary>
    internal class StatusCommandFactory
    {
        /// <summary>
        /// Creates new status command.
        /// </summary>
        /// <param name="server">Server to create new status command for.</param>
        public static IStatusCommand GetStatusCommand(WebFarmServerInfo server)
        {
            var pings = WebFarmServerMonitoringInfoProvider.GetPings(server);

            if (pings.Count > 0 && PingsCountInTimeSpan(pings, new TimeSpan(24, 0, 0)) == 0)
            {
                return new AutoDisabledCommand();
            }

            var pingsIn3min = PingsCountInTimeSpan(pings, new TimeSpan(0, 3, 0));
            if (pingsIn3min == 0 || !server.ServerEnabled)
            {
                return new NotRespondingCommand();
            }
            if (pingsIn3min <= 7)
            {
                return new TransitioningCommand();
            }
            return new HealthyCommand();
        }


        /// <summary>
        /// Returns number of pings in the given time span.
        /// </summary>
        /// <param name="pings">List of pings.</param>
        /// <param name="timeSpan">Time span.</param>
        internal static int PingsCountInTimeSpan(List<DateTime> pings, TimeSpan timeSpan)
        {
            //We will look to the future because of daylight saving time is switching & servers can be out of sync.
            var to = WebFarmContext.GetDateTime().AddMinutes(65);
            var from = WebFarmContext.GetDateTime().Subtract(timeSpan);

            return pings.Count(v => (v >= from) && (v <= to));
        }
    }
}

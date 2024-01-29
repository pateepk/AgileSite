using System;
using System.Linq;
using System.Text;

namespace CMS.Core
{
    /// <summary>
    /// Logging policy types.
    /// </summary>
    public enum LoggingPolicyEnum
    {
        /// <summary>
        /// Logs all events without any limits.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Logs the event only once per application lifetime.
        /// </summary>
        OnlyOnce = 1,

        /// <summary>
        /// Logs the event once per given period.
        /// </summary>
        OncePerPeriod = 2
    }
}

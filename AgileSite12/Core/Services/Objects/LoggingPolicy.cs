using System;

namespace CMS.Core
{
    /// <summary>
    /// Logging policy allows to limit the number of logged events.
    /// </summary>
    public class LoggingPolicy
    {
        /// <summary>
        /// Default logging policy ensures that events are always logged.
        /// </summary>
        public static readonly LoggingPolicy DEFAULT = new LoggingPolicy(LoggingPolicyEnum.Default);
        

        /// <summary>
        /// Policy ensures that events are logged only once per application lifetime.
        /// </summary>
        public static readonly LoggingPolicy ONLY_ONCE = new LoggingPolicy(LoggingPolicyEnum.OnlyOnce);

        
        /// <summary>
        /// Internal constructor used for non-period policy types.
        /// </summary>
        /// <param name="type">Policy type</param>
        /// <exception cref="NotSupportedException">Thrown when <see cref="LoggingPolicyEnum.OncePerPeriod"/> is used.</exception>
        private LoggingPolicy(LoggingPolicyEnum type)
        {
            if (type == LoggingPolicyEnum.OncePerPeriod)
            {
                throw new NotSupportedException("Once per period type cannot be used. Use constructor with timespan value.");
            }

            Type = type;
        }


        /// <summary>
        /// Creates new logging policy of type <see cref="LoggingPolicyEnum.OncePerPeriod"/>.
        /// </summary>
        /// <remarks>
        /// You can use <see cref="ONLY_ONCE"/> constant instead of <see cref="TimeSpan.Zero"/> for events that should be logged only once without period limit.
        /// </remarks>
        /// <example>
        /// <code>
        /// var policy = new LoggingPolicy(TimeSpan.FromMinutes(15));
        /// CoreServices.EventLog.LogException("Data", "UPDATE", ex, policy);
        /// </code>
        /// </example>
        /// <param name="period">Period value.</param>
        public LoggingPolicy(TimeSpan period)
        {
            if (period == TimeSpan.Zero)
            {
                Type = LoggingPolicyEnum.OnlyOnce;
            }
            else
            {
                Type = LoggingPolicyEnum.OncePerPeriod;
                Period = period;
            }
        }


        /// <summary>
        /// Logging policy type.
        /// </summary>
        public LoggingPolicyEnum Type
        {
            get;
            private set;
        }


        /// <summary>
        /// Logging period.
        /// </summary>
        /// <remarks>
        /// Applicable when policy type is <see cref="LoggingPolicyEnum.OncePerPeriod"/>.
        /// </remarks>
        public TimeSpan Period
        {
            get;
            private set;
        }
    }
}

using System;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IOffPeakService), typeof(DefaultOffPeakService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Interface for deciding when in the day is off peak time, meaning when is the best to run tasks with bigger load.
    /// Counts with off peak between 2am and 6am.
    /// </summary>
    internal class DefaultOffPeakService : IOffPeakService
    {
        /// <summary>
        /// Returns the exact <see cref="DateTime"/> of next off-peak period.
        /// Counts with off-peak between 2am and 6am.
        /// </summary>
        /// <param name="dateTime">Timestamp of from when to look for next off-peak period.</param>
        /// <returns><see cref="DateTime"/> of next off-peak period.</returns>
        public DateTime GetNextOffPeakPeriodStart(DateTime dateTime)
        {
            if(dateTime.TimeOfDay < new TimeSpan(2, 0, 0))
            {
                return dateTime.Date.AddHours(2);
            }

            var nextDay = dateTime.Date.AddDays(1);
            return nextDay.AddHours(2);
        }


        /// <summary>
        /// Checks whether given <see cref="DateTime"/> is in off-peak period.
        /// Counts with off-peak between 2am and 6am.
        /// </summary>
        /// <param name="dateTime">The date to check off-peak period for</param>
        /// <returns>True if <paramref name="dateTime"/> is in off-peak, false otherwise.</returns>
        public bool IsOffPeak(DateTime dateTime)
        {
            return (dateTime.TimeOfDay >= new TimeSpan(2, 0, 0)) && (dateTime.TimeOfDay <= new TimeSpan(6, 0, 0));
        }
    }
}

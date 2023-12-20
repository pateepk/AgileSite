using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Interface for deciding when in the day is off peak time, meaning when is the best to run tasks with bigger load.
    /// </summary>
    public interface IOffPeakService
    {
        /// <summary>
        /// Returns the exact <see cref="DateTime"/> of next off-peak period.
        /// </summary>
        /// <param name="dateTime">Timestamp of from when to look for next off-peak period.</param>
        /// <returns><see cref="DateTime"/> of next off-peak period.</returns>
        DateTime GetNextOffPeakPeriodStart(DateTime dateTime);


        /// <summary>
        /// Checks whether given <see cref="DateTime"/> is in off-peak period.
        /// </summary>
        /// <param name="dateTime">The date to check off-peak period for</param>
        /// <returns>True if <paramref name="dateTime"/> is in off-peak, false otherwise.</returns>
        bool IsOffPeak(DateTime dateTime);
    }
}

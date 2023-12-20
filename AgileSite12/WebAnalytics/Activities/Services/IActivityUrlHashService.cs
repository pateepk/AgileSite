using System;

using CMS;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(IActivityUrlHashService), typeof(ActivityUrlHashService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Provides hash computation for activity URLs.
    /// </summary>
    public interface IActivityUrlHashService
    {
        /// <summary>
        /// Computes a hash for the specified activity URL.
        /// </summary>
        /// <param name="activityUrl">Activity URL to compute hash for.</param>
        /// <returns>Hash corresponding to <paramref name="activityUrl"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activityUrl"/> is null.</exception>
        long GetActivityUrlHash(string activityUrl);
    }
}

using CMS;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(IActivityUrlHashService), typeof(ActivityUrlHashService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Computes hash for an activity URL
    /// </summary>
    public interface IActivityUrlHashService
    {
        /// <summary>
        /// Computes has for the activity URL
        /// </summary>
        /// <param name="activityUrl">Activity URL</param>
        /// <returns>Computed hash</returns>
        long GetActivityUrlHash(string activityUrl);
    }
}

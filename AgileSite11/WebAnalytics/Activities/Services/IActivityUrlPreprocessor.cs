using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IActivityUrlPreprocessor), typeof(ActivityUrlPreprocessor), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Prepares an activity URL for further processing.
    /// </summary>
    public interface IActivityUrlPreprocessor
    {
        /// <summary>
        /// Processes the activity URL for further processing (eg. hash computation)
        /// </summary>
        /// <param name="activityUrl">Activity URL</param>
        /// <returns>Processed activity URL</returns>
        string PreprocessActivityUrl(string activityUrl);
    }
}

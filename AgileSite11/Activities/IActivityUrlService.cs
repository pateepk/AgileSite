using CMS;
using CMS.Activities;

[assembly:RegisterImplementation(typeof(IActivityUrlService), typeof(ActivityUrlService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides methods to get correct URL and URL referrer to insert in <see cref="IActivityInfo" />.
    /// </summary>
    public interface IActivityUrlService
    {
        /// <summary>
        /// Gets URL of request the activity was logged for.
        /// </summary>
        /// <returns>URL</returns>
        string GetActivityUrl();


        /// <summary>
        /// Gets URL referrer of request the activity was referred from.
        /// </summary>
        /// <returns>URL referrer</returns>
        string GetActivityUrlReferrer();
    }
}

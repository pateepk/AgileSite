using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivityFactory), typeof(ActivityFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to create activities with basic data.
    /// </summary>
    public interface IActivityFactory
    {
        /// <summary>
        /// Creates new activity info, and initializes it using <paramref name="activityInitializer"/>.
        /// </summary>
        /// <returns>New <see cref="ActivityInfo"/></returns>
        IActivityInfo Create(IActivityInitializer activityInitializer);
    }
}
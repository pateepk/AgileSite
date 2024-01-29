using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivityRepository), typeof(ActivityQueueRepository), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Represents activity repository.
    /// </summary>
    internal interface IActivityRepository
    {
        /// <summary>
        /// Saves given activity info to repository.
        /// </summary>
        /// <param name="activity">Activity info</param>
        void Save(IActivityInfo activity);
    }
}
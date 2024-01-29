using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivityQueueProcessor), typeof(ActivityMemoryQueueProcessor), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides method for processing the activity queue.
    /// </summary>
    public interface IActivityQueueProcessor
    {
        /// <summary>
        /// Performs bulk insert of <see cref="IActivityInfo"/> from <see cref="ActivityMemoryQueue"/>.
        /// </summary>
        void InsertActivitiesFromQueueToDB();
    }
}
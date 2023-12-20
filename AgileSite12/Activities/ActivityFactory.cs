using System;

using CMS.Activities.Internal;
using CMS.Core;
using CMS.Core.Internal;


namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to create activities. Use <see cref="IActivityModifier"/> to extend factory.
    /// </summary>
    internal class ActivityFactory : IActivityFactory
    {
        /// <summary>
        /// Creates new activity info, and initializes it using <paramref name="activityInitializer"/>. 
        /// Afterwards all registered modifiers are applied to created <see cref="IActivityInfo"/>.
        /// </summary>
        /// <returns>New <see cref="IActivityInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="activityInitializer"/> is <c>null</c>.</exception>
        public IActivityInfo Create(IActivityInitializer activityInitializer)
        {
            if (activityInitializer == null)
            {
                throw new ArgumentNullException("activityInitializer");
            }

            var activity = CreateActivity(activityInitializer.ActivityType);
            activityInitializer.Initialize(activity);
            return activity;
        }


        private IActivityInfo CreateActivity(string activityType)
        {
            var dateTimeService = Service.Resolve<IDateTimeNowService>();

            var activity = new ActivityDto
            {
                ActivityType = activityType,
                ActivityCreated = dateTimeService.GetDateTimeNow()
            };
            return activity;
        }
    }
}
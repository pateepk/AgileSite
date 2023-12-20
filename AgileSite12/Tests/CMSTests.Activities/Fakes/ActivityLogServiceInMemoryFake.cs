using System.Collections.Generic;
using System.Web;

using CMS.Activities;
using CMS.Activities.Internal;

namespace Tests.Activities
{
    /// <summary>
    /// Logger saves activities to the memory so it can be used in unit tests without database access.
    /// This implementation only logs activities (doesn't use registered filteres, modifiers or validators).
    /// </summary>
    public class ActivityLogServiceInMemoryFake : IActivityLogService
    {
        /// <summary>
        /// Gets collection containing all activities logged using the service.
        /// </summary>
        public IList<IActivityInfo> LoggedActivities
        {
            get;
            private set;
        }


        /// <summary>
        /// Instantiates new instance of <see cref="ActivityLogServiceInMemoryFake"/>.
        /// </summary>
        public ActivityLogServiceInMemoryFake()
        {
            LoggedActivities = new List<IActivityInfo>();
        }


        /// <summary>
        /// Logs activity initialized by given <paramref name="activityInitializer"/> to the <see cref="LoggedActivities"/>. 
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity.</param>
        /// <param name="currentRequest">Not used in current implementation</param>
        /// <param name="loggingDisabledInAdministration">Not used in current implementation</param>
        public void Log(IActivityInitializer activityInitializer, HttpRequestBase currentRequest, bool loggingDisabledInAdministration = true)
        {
            LogActivityToList(activityInitializer);
        }


        /// <summary>
        /// Logs activity initialized by given <paramref name="activityInitializer"/> to the <see cref="LoggedActivities"/>. 
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity.</param>
        public void LogWithoutModifiersAndFilters(IActivityInitializer activityInitializer)
        {
            LogActivityToList(activityInitializer);
        }


        /// <summary>
        /// Does not do anything in current implementation.
        /// </summary>
        /// <param name="filter">Activity filter</param>
        public void RegisterFilter(IActivityLogFilter filter)
        {
        }


        /// <summary>
        /// Does not do anything in current implementation.
        /// </summary>
        /// <param name="activityModifier">Activity modifier</param>
        public void RegisterModifier(IActivityModifier activityModifier)
        {
        }


        /// <summary>
        /// Does not do anything in current implementation.
        /// </summary>
        /// <param name="activityLogValidator">Activity log validator</param>
        public void RegisterValidator(IActivityLogValidator activityLogValidator)
        {
        }


        /// <summary>
        /// Logs activity created with given <paramref name="activityInitializer"/> to the <see cref="LoggedActivities"/> collection.
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity</param>
        private void LogActivityToList(IActivityInitializer activityInitializer)
        {
            var activity = new ActivityDto();
            activityInitializer.Initialize(activity);
            activity.ActivityType = activityInitializer.ActivityType;
            
            ActivityEvents.ActivityProcessedInLogService.StartEvent(activity);

            LoggedActivities.Add(activity);
        }
    }
}
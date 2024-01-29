using System.Web;

using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivityLogService), typeof(ActivityLogService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to log activities.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Logs given activity. Registered <see cref="IActivityLogFilter"/> are checked, activity is further processed only if all filters allow it.
        /// Then <see cref="IActivityInfo"/> is created and <paramref name="activityInitializer"/> is called. 
        /// Afterwards all registered modifiers are applied to the <see cref="IActivityInfo"/>. Finally the <see cref="IActivityInfo"/> is validated by registered <see cref="IActivityLogValidator"/>.
        /// Activity is stored only if it is valid. After the activity is stored, two events are fired.
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity.</param>
        /// <param name="currentRequest">Current request</param>
        /// <param name="loggingDisabledInAdministration"><c>True</c> if activities should not be logged in administration.</param>
        void Log(IActivityInitializer activityInitializer, HttpRequestBase currentRequest, bool loggingDisabledInAdministration = true);


        /// <summary>
        /// Logs activity initialized by <paramref name="activityInitializer"/> directly to the database. Implementation does not check <see cref="IActivityLogFilter"/> 
        /// and does not use <see cref="IActivityModifier"/>. Final validation by <see cref="IActivityLogValidator"/> is performed. If you have <see cref="HttpRequestBase"/> available please 
        /// use <see cref="Log(IActivityInitializer, HttpRequestBase, bool)"/> instead.
        /// Method is designed to be used in environment without request e.g. scheduled tasks. After the activity is stored, two events are fired.
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity</param>
        void LogWithoutModifiersAndFilters(IActivityInitializer activityInitializer);


        /// <summary>
        /// Registers global filter which decide if activity should not be logged or otherwise. Filters are called before <see cref="IActivityInfo"/> is created.
        /// Activity is not logged if at least one filter denies activity. Filters are global, thus they are shared across all instances of the service.
        /// </summary>
        /// <param name="filter">Activity filter.</param>
        void RegisterFilter(IActivityLogFilter filter);


        /// <summary>
        /// Registers global activity modifier. Modifier is used to update every newly created <see cref="IActivityInfo"/>. Modifier is called after activity is created
        /// and successfully initialized. Modifiers are global, thus they are shared across all instances of the service.
        /// </summary>
        /// <param name="activityModifier"></param>
        void RegisterModifier(IActivityModifier activityModifier);


        /// <summary>
        /// Registers global activity validator. Validators are used to check whether the processed <see cref="IActivityInfo"/> is valid and can be logged.
        /// Validators are global, therefore they are shared across all instances of the service.
        /// </summary>
        /// <param name="activityLogValidator">Activity log validator to be registered</param>
        void RegisterValidator(IActivityLogValidator activityLogValidator);
    }
}
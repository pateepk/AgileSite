using CMS.Activities;
using CMS.Membership;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Filters out logging for users with disabled logging.
    /// </summary>
    internal class CurrentUserEnabledTrackingFilter : IActivityLogFilter
    {
        /// <summary>
        /// Filters out logging for users with disabled logging
        /// </summary>
        /// <returns>Returns <c>true</c> if logging should be filtered out, because current user has disabled logging. Otherwise returns <c>false</c>.</returns>
        public bool IsDenied()
        {
            return !MembershipContext.AuthenticatedUser.UserSettings.UserLogActivities;
        }
    }
}
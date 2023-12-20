using CMS.Activities;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class for updating MVT related properties in activities.
    /// </summary>
    internal class MVTActivityModifier : IActivityModifier
    {
        /// <summary>
        /// Updates <see cref="IActivityInfo.ActivityMVTCombinationName"/> with current MVT combination name.
        /// </summary>
        /// <remarks>Only activities of type <see cref="PredefinedActivityType.PAGE_VISIT"/> or <see cref="PredefinedActivityType.LANDING_PAGE"/> are updated.</remarks>
        /// <param name="activity">Activity which property to update.</param>
        public void Modify(IActivityInfo activity)
        {
            if (activity.ActivityType == PredefinedActivityType.PAGE_VISIT || activity.ActivityType == PredefinedActivityType.LANDING_PAGE)
            {
                activity.ActivityMVTCombinationName = MVTContext.CurrentMVTCombinationName;
            }
        }
    }
}

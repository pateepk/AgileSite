using System.Collections.Generic;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service that provides method to work with the <see cref="ActivityTypeViewModel"/> objects.
    /// </summary>
    public interface IActivityTypeService
    {
        /// <summary>
        /// Returns view models for all allowed activity types.
        /// </summary>
        /// <param name="allowedActivities">List of allowed activity types.</param>
        /// <param name="isSiteContentOnly">Flag if site is context only.</param>
        /// <remarks>Some activities are not allowed on contento only sites.</remarks>
        IEnumerable<ActivityTypeViewModel> GetActivityTypeViewModels(ICollection<string> allowedActivities, bool isSiteContentOnly);
    }
}

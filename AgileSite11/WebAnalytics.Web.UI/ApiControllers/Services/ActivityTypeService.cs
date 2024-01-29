using System.Linq;
using System.Collections.Generic;

using CMS;
using CMS.Activities;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(IActivityTypeService), typeof(ActivityTypeService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Logic connected to <see cref="ActivityTypeInfo"/> modelling.
    /// </summary>
    internal class ActivityTypeService : IActivityTypeService
    {
        /// <summary>
        /// Returns view models for all allowed activity types.
        /// </summary>
        /// <param name="allowedActivities">List of allowed activity types.</param>
        /// <param name="isSiteContentOnly">Flag if site is context only.</param>
        /// <remarks>Some activities are not allowed on contento only sites.</remarks>
        public IEnumerable<ActivityTypeViewModel> GetActivityTypeViewModels(ICollection<string> allowedActivities, bool isSiteContentOnly)
        {
            var activities = ActivityTypeInfoProvider.GetActivityTypes()
                                                .WhereTrue("ActivityTypeEnabled")
                                                .WhereIn("ActivityTypeName", allowedActivities);

            if (isSiteContentOnly)
            {
                activities = activities.WhereFalse("ActivityTypeIsHiddenInContentOnly");
            }

            return activities.ToList()
                             .Select(type => new ActivityTypeViewModel
                             {
                                 Type = type.ActivityTypeName,
                                 DisplayName = type.ActivityTypeDisplayName
                             }
            );
        }
    }
}

using CMS;
using CMS.Activities;

[assembly: RegisterImplementation(typeof(IActivitySettings), typeof(ActivitySettings), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Activities
{
    /// <summary>
    /// Provides access to activities settings.
    /// </summary>
    internal interface IActivitySettings
    {
        /// <summary>
        /// Checks whether activities logging is endabled for given site name and online marketing module is available.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns <c>True</c> if activities are enabled and online marketing module is available, otherwise <c>false</c>.</returns>
        bool ActivitiesEnabledAndModuleLoadedForSite(string siteName);
    }
}
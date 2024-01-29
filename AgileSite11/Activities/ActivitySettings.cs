namespace CMS.Activities
{
    /// <summary>
    /// Provides access to activities settings.
    /// </summary>
    internal class ActivitySettings : IActivitySettings
    {
        /// <summary>
        /// Checks whether activities logging is enabled for given site name and online marketing module is available.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns <c>True</c> if activities are enabled and online marketing module is available, otherwise <c>false</c>.</returns>
        public bool ActivitiesEnabledAndModuleLoadedForSite(string siteName)
        {
            return ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(siteName);
        }
    }
}
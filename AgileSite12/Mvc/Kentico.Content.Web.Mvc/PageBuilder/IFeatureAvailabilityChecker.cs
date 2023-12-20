namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides methods to check availibility of a feature.
    /// </summary>
    internal interface IFeatureAvailabilityChecker
    {
        /// <summary>
        /// Indicates if feature is enabled.
        /// </summary>
        bool IsFeatureEnabled();


        /// <summary>
        /// Indicates if license requirements for feature are met.
        /// </summary>
        bool IsFeatureAvailable();
    }
}
namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides builders routes and constants.
    /// </summary>
    internal static class BuilderRoutes
    {
        /// <summary>
        /// Name of the localization script route.
        /// </summary>
        public const string LOCALIZATION_SCRIPT_ROUTE_NAME = "Kentico.PageBuilder.LocalizationScript";


        /// <summary>
        /// Name of the controller for localization script.
        /// </summary>
        public const string LOCALIZATION_SCRIPT_CONTROLLER_NAME = "KenticoLocalizationScript";


        /// <summary>
        /// Default route for Page builder localization script retrieval.
        /// </summary>
        public const string LOCALIZATION_SCRIPT_ROUTE = "Kentico.Resource/Builder/Scripts/Localization/{cultureCode}.js";
    }
}
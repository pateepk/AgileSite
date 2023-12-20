namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides localization script for Page builder feature.
    /// </summary>
    internal interface ILocalizationScriptProvider
    {
        /// <summary>
        /// Gets the script with localization of resource strings.
        /// </summary>
        /// <param name="cultureCode">Culture code of the localization.</param>
        string Get(string cultureCode);
    }
}
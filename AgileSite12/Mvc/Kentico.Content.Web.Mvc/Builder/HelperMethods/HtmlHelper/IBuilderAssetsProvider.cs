namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides interface for assets registration for a general builder.
    /// </summary>
    internal interface IBuilderAssetsProvider
    {
        /// <summary>
        /// Gets the builder localization script file.
        /// </summary>
        /// <param name="cultureCode">Culture code of the localization.</param>
        /// <param name="defaultCultureCode">Default culture code used as a fall-back.</param>
        string GetLocalizationScriptTags(string cultureCode, string defaultCultureCode);
    }
}
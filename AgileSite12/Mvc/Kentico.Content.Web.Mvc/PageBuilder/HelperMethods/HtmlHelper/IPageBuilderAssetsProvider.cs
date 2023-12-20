using System.Collections.Generic;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for assets registration for Page builder.
    /// </summary>
    internal interface IPageBuilderAssetsProvider : IBuilderAssetsProvider
    {
        /// <summary>
        /// Gets link tag for the bundle of page components' styles on a live site.
        /// </summary>
        /// <returns>Returns link tag with source pointed to page components styles bundle.</returns>
        string GetPageComponentsStyleBundleTag();


        /// <summary>
        /// Gets link tag for the bundle of page components' styles in administration.
        /// </summary>
        /// <returns>Returns link tag with source pointed to page components styles bundle.</returns>
        string GetPageComponentsAdminStyleBundleTag();


        /// <summary>
        /// Gets script tag for the bundle of page components' scripts in administration.
        /// </summary>
        /// <returns>Returns script tag with source pointed to page components JavaScript bundle.</returns>
        string GetPageComponentsAdminScriptBundleTag();


        /// <summary>
        /// Gets script tag for the bundle of page components' scripts on a live site.
        /// </summary>
        /// <returns>Returns script tag with source pointed to page components JavaScript bundle.</returns>
        string GetPageComponentsScriptBundleTag();


        /// <summary>
        /// Gets script tags for the Page builder scripts.
        /// </summary>
        /// <returns>Returns script tags with sources pointed to Page builder JavaScript files.</returns>
        string GetScriptTags();


        /// <summary>
        /// Gets the Page builder initialization script.
        /// </summary>
        /// <param name="configuration">Page builder initial configuration.</param>
        /// <param name="decorator">Decorates the paths which need preview context to be initialized.</param>
        /// <param name="featureAvailabilityCheckers">Checkers for features availability.</param>
        /// <returns>Returns an initialization Page builder script with a defined configuration object.</returns>
        string GetStartupScriptTag(PageBuilderScriptConfiguration configuration, IPathDecorator decorator, IDictionary<string, IFeatureAvailabilityChecker> featureAvailabilityCheckers);


        /// <summary>
        /// Gets the Page builder style sheet link tag.
        /// </summary>
        /// <returns>Returns link tags with Page builder styles.</returns>
        string GetStylesheetLinkTag();
    }
}
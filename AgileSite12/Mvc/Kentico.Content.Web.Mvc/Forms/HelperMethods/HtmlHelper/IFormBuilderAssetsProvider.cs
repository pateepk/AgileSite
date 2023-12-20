using CMS.Helpers;
using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface for assets registration for Form builder.
    /// </summary>
    internal interface IFormBuilderAssetsProvider : IBuilderAssetsProvider
    {
        /// <summary>
        /// Gets link tag for Form builder administration styles.
        /// </summary>
        /// <returns>Returns link tags with Form builder administration styles.</returns>
        string GetAdminStylesheetLinkTag();


        /// <summary>
        /// Gets link tag for Form builder's components administration styles,
        /// if their bundle is registered.
        /// </summary>
        /// <returns>Returns link tags with Form builder components' administration styles, or an empty string if the bundle is not registered.</returns>
        string GetFormComponentsAdminStylesheetLinkTag();


        /// <summary>
        /// Gets script tag for the bundle of forms scripts.
        /// </summary>
        /// <returns>Returns script tag with source pointed to forms JavaScript bundle,
        /// which includes scripts for form components, sections and additional forms scripts.</returns>
        string GetFormsScriptBundleTag();


        /// <summary>
        /// Gets link tag for Form builder's components live site styles,
        /// if their bundle is registered.
        /// </summary>
        /// <returns>Returns link tags with Form builder components' live site styles, or an empty string if the bundle is not registered.</returns>
        string GetFormComponentsStylesheetLinkTag();


        /// <summary>
        /// Gets script tags for the bundle of system jQuery and jQuery Unobtrusive Ajax script.
        /// </summary>
        /// <param name="renderCustomized">Indicates if customized Jquery bundle should be rendered.</param>
        /// <returns>Returns script tag with source pointed to jQuery JavaScript bundle.</returns>
        string GetJQueryScriptBundleTags(bool renderCustomized);


        /// <summary>
        /// Gets script tags for the Form builder scripts.
        /// </summary>
        /// <returns>Renders script tags with sources pointed to Form builder JavaScript files.</returns>
        string GetScriptTags();


        /// <summary>
        /// Gets the Form builder initialization script.
        /// </summary>
        /// <param name="configuration">Form builder initial configuration.</param>
        /// <param name="decorator">Decorates the paths which need <see cref="VirtualContext"/> to be initialized.</param>
        /// <returns>Returns an initialization Page builder script with a defined configuration object.</returns>
        string GetStartupScriptTag(FormBuilderScriptConfiguration configuration, IPathDecorator decorator);
    }
}
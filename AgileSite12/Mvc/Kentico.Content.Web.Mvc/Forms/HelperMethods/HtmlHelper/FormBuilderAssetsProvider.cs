using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;

using CMS.Helpers;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides assets registration for Form builder.
    /// </summary>
    internal class FormBuilderAssetsProvider : BuilderAssetsProvider, IFormBuilderAssetsProvider
    {
        private const string FORM_BUILDER_SCRIPT_FILE = "form-builder.js";
        private const string FORM_BUILDER_STYLE_FILE = "form-builder.css";


        /// <summary>
        /// Instantiates the Form builder assets provider.
        /// </summary>
        /// <param name="requestContext">Request context used when providing assets.</param>
        public FormBuilderAssetsProvider(System.Web.Routing.RequestContext requestContext)
            : base(requestContext) { }


        /// <summary>
        /// Gets script tags for the Form builder scripts.
        /// </summary>
        /// <returns>Renders script tags with sources pointed to Form builder JavaScript files.</returns>
        public string GetScriptTags()
        {
            return GetPolyfillScriptTags() + GetScriptTags(FORM_BUILDER_SCRIPT_FILE);
        }


        /// <summary>
        /// Gets the Form builder initialization script.
        /// </summary>
        /// <param name="configuration">Form builder initial configuration.</param>
        /// <param name="decorator">Decorates the paths which need <see cref="VirtualContext"/> to be initialized.</param>
        /// <returns>Returns an initialization Page builder script with a defined configuration object.</returns>
        public string GetStartupScriptTag(FormBuilderScriptConfiguration configuration, IPathDecorator decorator)
        {
            var initParameter = GetFormBuilderInitializationParameter(configuration, decorator);

            var initParameterJson = JsonConvert.SerializeObject(initParameter);

            // Create tag builder
            var tagBuilder = new TagBuilder("script")
            {
                InnerHtml = $"window.kentico.formBuilder.init({initParameterJson});"
            };

            return tagBuilder.ToString();
        }


        /// <summary>
        /// Gets script tags for the bundle of system jQuery and jQuery Unobtrusive Ajax script.
        /// </summary>
        /// <param name="renderCustomized">Indicates if customized Jquery bundle should be rendered.</param>
        /// <returns>Returns script tag with source pointed to jQuery JavaScript bundle.</returns>
        public string GetJQueryScriptBundleTags(bool renderCustomized)
        {
            return GetJQueryScriptBundleTag(renderCustomized) + GetJQueryUnobtrusiveAjaxScriptBundleTag(renderCustomized);
        }


        private string GetJQueryScriptBundleTag(bool renderCustomized)
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(ScriptsBundleCollectionUtils.JQUERY_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH))
            {
                return Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_SCRIPTS_BUNDLE_VIRTUAL_PATH).ToString();
            }

            if (renderCustomized)
            {
                return Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH).ToString();
            }

            return String.Empty;

        }


        private string GetJQueryUnobtrusiveAjaxScriptBundleTag(bool renderCustomized)
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(ScriptsBundleCollectionUtils.JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH))
            {
                return Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_BUNDLE_VIRTUAL_PATH).ToString();
            }

            if (renderCustomized)
            {
                return Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH).ToString();
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets script tag for the bundle of forms scripts.
        /// </summary>
        /// <returns>Returns script tag with source pointed to forms JavaScript bundle,
        /// which includes scripts for form components, sections and additional forms scripts.</returns>
        public string GetFormsScriptBundleTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(ScriptsBundleCollectionUtils.BUNDLE_VIRTUAL_PATH))
            {
                return string.Empty;
            }

            return Scripts.Render(ScriptsBundleCollectionUtils.BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets link tag for Form builder administration styles.
        /// </summary>
        /// <returns>Returns link tags with Form builder administration styles.</returns>
        public string GetAdminStylesheetLinkTag()
        {
            return GetStyleLinkTag(FORM_BUILDER_STYLE_FILE);
        }


        /// <summary>
        /// Gets link tag for Form builder's components live site styles,
        /// if their bundle is registered.
        /// </summary>
        /// <returns>Returns link tags with Form builder components' live site styles, or an empty string if the bundle is not registered.</returns>
        public string GetFormComponentsStylesheetLinkTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(StylesBundleCollectionUtils.FORM_COMPONENTS_LIVE_SITE_BUNDLE_VIRTUAL_PATH))
            {
                return "";
            }

            return Styles.Render(StylesBundleCollectionUtils.FORM_COMPONENTS_LIVE_SITE_BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets link tag for Form builder's components administration styles,
        /// if their bundle is registered.
        /// </summary>
        /// <returns>Returns link tags with Form builder components' administration styles, or an empty string if the bundle is not registered.</returns>
        public string GetFormComponentsAdminStylesheetLinkTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(StylesBundleCollectionUtils.FORM_COMPONENTS_ADMIN_BUNDLE_VIRTUAL_PATH))
            {
                return "";
            }

            return Styles.Render(StylesBundleCollectionUtils.FORM_COMPONENTS_ADMIN_BUNDLE_VIRTUAL_PATH).ToString();
        }


        private IDictionary<string, object> GetFormBuilderInitializationParameter(FormBuilderScriptConfiguration configuration, IPathDecorator decorator)
        {
            var initizaliationParameter = GetBuilderInitializationParameter(configuration, decorator);

            initizaliationParameter.Add("formIdentifier", configuration.FormIdentifier);
            initizaliationParameter.Add("propertiesEditorClientId", configuration.PropertiesEditorClientId);
            initizaliationParameter.Add("saveMessageClientId", configuration.SaveMessageClientId);
            initizaliationParameter.Add("propertiesEditorEndpoint", TryDecoratePath(configuration.PropertiesEditorEndpoint, decorator));
            initizaliationParameter.Add("validationRuleMetadataEndpoint", TryDecoratePath(configuration.ValidationRuleMetadataEndpoint, decorator));
            initizaliationParameter.Add("validationRuleMarkupEndpoint", TryDecoratePath(configuration.ValidationRuleMarkupEndpoint, decorator));
            initizaliationParameter.Add("visibilityConditionMarkupEndpoint", TryDecoratePath(configuration.VisibilityConditionMarkupEndpoint, decorator));

            return initizaliationParameter;
        }
    }
}

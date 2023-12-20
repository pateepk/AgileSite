using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides assets registration for Page builder.
    /// </summary>
    internal sealed class PageBuilderAssetsProvider : BuilderAssetsProvider, IPageBuilderAssetsProvider
    {
        private const string PAGE_BUILDER_SCRIPT_FILE = "page-builder.js";
        private const string PAGE_BUILDER_STYLE_FILE = "page-builder.css";

        /// <summary>
        /// Instantiates the Page builder assets provider.
        /// </summary>
        /// <param name="requestContext">Request context used when providing assets.</param>
        public PageBuilderAssetsProvider(RequestContext requestContext)
            : base(requestContext) { }


        /// <summary>
        /// Gets script tags for the Page builder scripts.
        /// </summary>
        /// <returns>Returns script tags with sources pointed to Page builder JavaScript files.</returns>
        public string GetScriptTags()
        {
            return GetPolyfillScriptTags() + GetScriptTags(PAGE_BUILDER_SCRIPT_FILE);
        }


        /// <summary>
        /// Gets the Page builder style sheet link tag.
        /// </summary>
        /// <returns>Returns link tags with Page builder styles.</returns>
        public string GetStylesheetLinkTag()
        {
            return GetStyleLinkTag(PAGE_BUILDER_STYLE_FILE);
        }


        /// <summary>
        /// Gets script tag for the bundle of page components' scripts on a live site.
        /// </summary>
        /// <returns>Returns script tag with source pointed to page components JavaScript bundle.</returns>
        public string GetPageComponentsScriptBundleTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(ScriptsBundleCollectionUtils.BUNDLE_VIRTUAL_PATH))
            {
                return string.Empty;
            }

            return Scripts.Render(ScriptsBundleCollectionUtils.BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets script tag for the bundle of page components' scripts in administration.
        /// </summary>
        /// <returns>Returns script tag with source pointed to page components JavaScript bundle.</returns>
        public string GetPageComponentsAdminScriptBundleTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(ScriptsBundleCollectionUtils.ADMIN_BUNDLE_VIRTUAL_PATH))
            {
                return string.Empty;
            }

            return Scripts.Render(ScriptsBundleCollectionUtils.ADMIN_BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets link tag for the bundle of page components' styles on a live site.
        /// </summary>
        /// <returns>Returns link tag with source pointed to page components styles bundle.</returns>
        public string GetPageComponentsStyleBundleTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(StylesBundleCollectionUtils.BUNDLE_VIRTUAL_PATH))
            {
                return string.Empty;
            }

            return Styles.Render(StylesBundleCollectionUtils.BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets link tag for the bundle of page components' styles in administration.
        /// </summary>
        /// <returns>Returns link tag with source pointed to page components styles bundle.</returns>
        public string GetPageComponentsAdminStyleBundleTag()
        {
            if (!BundleResolver.Current.IsBundleVirtualPath(StylesBundleCollectionUtils.ADMIN_BUNDLE_VIRTUAL_PATH))
            {
                return string.Empty;
            }

            return Styles.Render(StylesBundleCollectionUtils.ADMIN_BUNDLE_VIRTUAL_PATH).ToString();
        }


        /// <summary>
        /// Gets the Page builder initialization script.
        /// </summary>
        /// <param name="configuration">Page builder initial configuration.</param>
        /// <param name="decorator">Decorates the paths which need preview context to be initialized.</param>
        /// <param name="featureAvailabilityCheckers">Checkers for features availability.</param>
        /// <returns>Returns an initialization Page builder script with a defined configuration object.</returns>
        public string GetStartupScriptTag(PageBuilderScriptConfiguration configuration, IPathDecorator decorator, IDictionary<string, IFeatureAvailabilityChecker> featureAvailabilityCheckers)
        {
            var initParameter = GetPageBuilderInitializationParameter(configuration, decorator, featureAvailabilityCheckers);

            var initParameterJson = JsonConvert.SerializeObject(initParameter);

            // Create tag builder
            var tagBuilder = new TagBuilder("script")
            {
                InnerHtml = $"window.kentico.pageBuilder.init({initParameterJson});"
            };

            return tagBuilder.ToString();
        }


        private IDictionary<string, object> GetPageBuilderInitializationParameter(PageBuilderScriptConfiguration configuration, IPathDecorator decorator, IDictionary<string, IFeatureAvailabilityChecker> featureAvailabilityCheckers)
        {
            var initizaliationParameter = GetBuilderInitializationParameter(configuration, decorator);

            initizaliationParameter.Add("pageIdentifier", configuration.PageIdentifier);
            initizaliationParameter.Add("featureSet", GetFeatureSet(featureAvailabilityCheckers));
            initizaliationParameter.Add("pageTemplate", configuration.PageTemplate.GetBuilderInitializationParameter(decorator));
            initizaliationParameter.Add("selectors", configuration.Selectors.GetBuilderInitializationParameter(decorator));
            
            return initizaliationParameter;
        }


        private static IDictionary<string, bool> GetFeatureSet(IDictionary<string, IFeatureAvailabilityChecker> featureAvailabilityCheckers)
        {
            var featureSet = new Dictionary<string, bool>();

            foreach (var checker in featureAvailabilityCheckers)
            {
                featureSet.Add(checker.Key, IsFeatureAvailable(checker.Value));
            }

            return featureSet;
        }


        private static bool IsFeatureAvailable(IFeatureAvailabilityChecker checker)
        {
            return checker.IsFeatureAvailable() && checker.IsFeatureEnabled();
        }
    }
}

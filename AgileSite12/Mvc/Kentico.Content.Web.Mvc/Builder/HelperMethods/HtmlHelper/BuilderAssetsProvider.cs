using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.Base;
using CMS.Helpers;

using Kentico.Builder.Web.Mvc.Internal;
using Kentico.Content.Web.Mvc;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides assets registration for a general builder.
    /// </summary>
    internal abstract class BuilderAssetsProvider : IBuilderAssetsProvider
    {
        private const string DEFAULT_BASE_BUILDER_SCRIPT_FILE = "builder.js";
        private const string DEFAULT_BASE_BUILDER_STYLE_FILE = "builder.css";
        private const string DEFAULT_POLYFILL_SCRIPT_FILE = "shim.min.js";
        private const string DEFAULT_VENDORS_SCRIPT_FILE = "vendors.js";        

        protected const string DEFAULT_SCRIPT_PATH = "~/Kentico/Scripts/";
        protected const string DEFAULT_BUILDER_SCRIPT_PATH = DEFAULT_SCRIPT_PATH + "builders/";
        protected readonly System.Web.Routing.RequestContext requestContext;


        // Instantiate a UrlHelper to get absolute file path from relative one
        protected UrlHelper UrlHelper => new UrlHelper(requestContext);

        // Indicates whether the script file should be provided by the "Hot-module-reloading" module to enable live script debug.
        private bool DebugBuilderScripts => ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugBuilderScripts"], false);


        /// <summary>
        /// Instantiates the builder assets provider.
        /// </summary>
        /// <param name="requestContext">Request context used when providing assets.</param>
        protected BuilderAssetsProvider(System.Web.Routing.RequestContext requestContext)
        {
            this.requestContext = requestContext;
        }


        /// <summary>
        /// Returns all the required builder script file tags.
        /// </summary>
        /// <param name="filename">Name of the main script file.</param>
        protected string GetScriptTags(string filename)
        {
            string filePath = DEFAULT_BUILDER_SCRIPT_PATH + filename;
            string tag = GetVendorsScriptTag() + GetBaseBuilderScriptTag() + GetScriptTag(UrlHelper.Content(filePath));

#if DEBUG
            // Use script provided by the "Hot-module-reloading" module if enabled
            if (DebugBuilderScripts)
            {
                string debugFilePath = "http://localhost:3000/dist/" + filename;

                // Replace the "release" script tags with its single "debug" variant
                tag = GetScriptTag(UrlHelper.Content(debugFilePath));
            }
#endif

            return tag;
        }

        /// <summary>
        /// Returns all the polyfill script tags required by the builder.
        /// </summary>
        protected string GetPolyfillScriptTags()
        {
            return GetScriptTag(UrlHelper.Content(DEFAULT_SCRIPT_PATH + DEFAULT_POLYFILL_SCRIPT_FILE));
        }


        /// <summary>
        /// Gets a dictionary representing the initialization parameter of a builder.
        /// </summary>
        /// <param name="configuration">Builder script initial configuration.</param>
        /// <param name="decorator">Decorates the paths which need <see cref="VirtualContext"/> to be initialized.</param>
        /// <returns>Returns an initialization parameter created from the <paramref name="configuration"/> given.</returns>
        protected IDictionary<string, object> GetBuilderInitializationParameter(BuilderScriptConfiguration configuration, IPathDecorator decorator)
        {
            var allowedOrigins = configuration.AllowedOrigins?.Select(origin => origin.ToLowerInvariant());
            var metadataPath = TryDecoratePath(configuration.MetadataEndpoint, decorator);
            var configurationLoadPath = TryDecoratePath(configuration.ConfigurationLoadEndpoint, decorator);
            var configurationStorePath = TryDecoratePath(configuration.ConfigurationStoreEndpoint, decorator);

            var parameter = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "applicationPath", configuration.ApplicationPath?.Replace("'", "\\'") },
                { "configurationEndpoints", new { load = configurationLoadPath, store = configurationStorePath } },
                { "metadataEndpoint", metadataPath },
                { "allowedOrigins", allowedOrigins },
                { "constants",
                    new {
                        editingInstanceHeader = BuilderConstants.INSTANCE_HEADER_NAME
                    }
                },
                { "developmentMode", configuration.DevelopmentMode }
            };

            return parameter;
        }


        /// <summary>
        /// Returns all the required builder style sheet link tags.
        /// </summary>
        /// <param name="filename">Name of the specific builder script file.</param>
        protected string GetStyleLinkTag(string filename)
        {
            string filePath = DEFAULT_BUILDER_SCRIPT_PATH + filename;
            string tags = GetBaseBuilderStyleTag() + GetStylesheetLinkTag(UrlHelper.Content(filePath));

#if DEBUG
            // Use script provided by the "Hot-module-reloading" module if enabled
            if (DebugBuilderScripts)
            {
                // Styles are included in the main JS file in DEV mode => no need to include them explicitly.
                tags = String.Empty;
            }
#endif

            return tags;
        }


        /// <summary>
        /// Gets the builder localization script file.
        /// </summary>
        /// <param name="cultureCode">Culture code of the localization.</param>
        /// <param name="defaultCultureCode">Default culture code used as a fall-back.</param>
        public string GetLocalizationScriptTags(string cultureCode, string defaultCultureCode)
        {
            var currentCultureUrl = GetLocalizationScriptUrl(cultureCode);
            var currentCultureTag = GetScriptTag(currentCultureUrl);

            if (String.Equals(cultureCode, defaultCultureCode, StringComparison.OrdinalIgnoreCase))
            {
                return currentCultureTag;
            }

            var defaultCultureUrl = GetLocalizationScriptUrl(defaultCultureCode);
            var defaultCultureTag = GetScriptTag(defaultCultureUrl);

            return defaultCultureTag + currentCultureTag;
        }

        /// <summary>
        /// Returns the HTML 'script' tag with <paramref name="scriptPath"/>.
        /// </summary>
        /// <param name="scriptPath">Path to the script.</param>
        protected string GetScriptTag(string scriptPath)
        {
            var tagBuilder = new TagBuilder("script");

            tagBuilder.Attributes.Add("src", scriptPath);

            return tagBuilder.ToString();
        }


        /// <summary>
        /// Returns the HTML style sheet tag with <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Path to the style sheet.</param>
        protected string GetStylesheetLinkTag(string path)
        {
            var tagBuilder = new TagBuilder("link");

            tagBuilder.Attributes.Add("href", path);
            tagBuilder.Attributes.Add("rel", "stylesheet");
            tagBuilder.Attributes.Add("type", "text/css");

            return tagBuilder.ToString(TagRenderMode.SelfClosing);
        }


        /// <summary>
        /// Decorates the <paramref name="path"/> with <paramref name="pathDecorator"/>.
        /// </summary>
        /// <param name="path">Path to decorate.</param>
        /// <param name="pathDecorator">Decorates path with <see cref="VirtualContext"/>.</param>
        protected string TryDecoratePath(string path, IPathDecorator pathDecorator)
        {
            var decoratedPath = path.Replace("'", "\\'");

            if (pathDecorator != null)
            {
                decoratedPath = pathDecorator.Decorate(decoratedPath);
            }

            return decoratedPath;
        }


        private string GetLocalizationScriptUrl(string cultureCode)
        {
            return UrlHelper.GenerateUrl(BuilderRoutes.LOCALIZATION_SCRIPT_ROUTE_NAME, "Script", BuilderRoutes.LOCALIZATION_SCRIPT_CONTROLLER_NAME, new RouteValueDictionary(new { cultureCode }), RouteTable.Routes, requestContext, false);
        }


        private string GetBaseBuilderScriptTag()
        {
            string filePath = DEFAULT_BUILDER_SCRIPT_PATH + DEFAULT_BASE_BUILDER_SCRIPT_FILE;
            return GetScriptTag(UrlHelper.Content(filePath));
        }


        private string GetVendorsScriptTag()
        {
            string filePath = DEFAULT_BUILDER_SCRIPT_PATH + DEFAULT_VENDORS_SCRIPT_FILE;
            return GetScriptTag(UrlHelper.Content(filePath));
        }


        private string GetBaseBuilderStyleTag()
        {
            string filePath = DEFAULT_BUILDER_SCRIPT_PATH + DEFAULT_BASE_BUILDER_STYLE_FILE;
            return GetStylesheetLinkTag(UrlHelper.Content(filePath));
        }
    }
}

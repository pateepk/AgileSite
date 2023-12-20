using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CMS.Core;
using CMS.SiteProvider;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;
using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for extension point <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/>.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders necessary scripts for Page builder feature.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML markup with script tags for scripts required by Page builder feature.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the Page builder has not been initialized yet.</exception>
        public static IHtmlString PageBuilderScripts(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.PageBuilderScriptsInternal(new AllowedDomainsRetriever(SiteContext.CurrentSite), Service.Resolve<IComponentLocalizationService>());
        }


        /// <summary>
        /// Renders necessary stylesheet link tags for Page builder feature.
        /// In edit mode, both page builder (with inline editors) related styles and form widget styles are included.
        /// Otherwise, both page builder (without inline editors) and form widget styles are included.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML markup with link tags for stylesheets.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString PageBuilderStyles(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var requestContext = instance.Target.ViewContext.RequestContext;
            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var formBuilderAssetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(requestContext);
            var pageBuilderAssetsProvider = builderAssetsProviderFactory.Get<PageBuilderAssetsProvider, IPageBuilderAssetsProvider>(requestContext);

            if (!instance.Target.ViewContext.HttpContext.Kentico().PageBuilder().EditMode)
            {
                return new HtmlString(
                    pageBuilderAssetsProvider.GetPageComponentsStyleBundleTag() +
                    formBuilderAssetsProvider.GetFormComponentsStylesheetLinkTag()
                    );
            }

            return new HtmlString(
                pageBuilderAssetsProvider.GetStylesheetLinkTag() +
                pageBuilderAssetsProvider.GetPageComponentsAdminStyleBundleTag() +
                formBuilderAssetsProvider.GetFormComponentsStylesheetLinkTag()
            );
        }


        /// <summary>
        /// Renders necessary scripts for Page builder feature.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="retriever">Retriever of allowed domains to check origin of post messages on client.</param>
        /// <param name="localizationService">Localization service to provide context of a culture.</param>
        /// <returns>Returns HTML markup with script tags for scripts required by Page builder feature.</returns>
        internal static IHtmlString PageBuilderScriptsInternal(this ExtensionPoint<HtmlHelper> instance, IAllowedDomainsRetriever retriever, IComponentLocalizationService localizationService)
        {
            var httpContext = instance.Target.ViewContext.HttpContext;
            var pageBuilder = httpContext.Kentico().PageBuilder();

            if (pageBuilder.PageIdentifier == 0)
            {
                throw new InvalidOperationException("Page builder is not initialized. Call 'HttpContext.Kentico().PageBuilder().Initialize(<PageIdentifier>)' method first in you code before Page builder scripts registration.");
            }

            var requestContext = instance.Target.ViewContext.RequestContext;
            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var formBuilderAssetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(requestContext);
            var pageBuilderAssetsProvider = builderAssetsProviderFactory.Get<PageBuilderAssetsProvider, IPageBuilderAssetsProvider>(requestContext);

            if (!pageBuilder.EditMode)
            {
                return new HtmlString(
                    formBuilderAssetsProvider.GetJQueryScriptBundleTags(false) +
                    pageBuilderAssetsProvider.GetPageComponentsScriptBundleTag() +
                    formBuilderAssetsProvider.GetFormsScriptBundleTag()
                );
            }

            var applicationPath = httpContext.Request.ApplicationPath;

            var scriptBlock = pageBuilderAssetsProvider.GetScriptTags() + pageBuilderAssetsProvider.GetLocalizationScriptTags(localizationService.GetCultureCode(), localizationService.GetDefaultCultureCode());
            var featureSet = new Dictionary<string, IFeatureAvailabilityChecker>
            {
                { "personalizationEnabled", new PersonalizationAvailabilityChecker() }
            };

            var pageBuilderScriptConfiguration = PageBuilderScriptConfigurationSource.Instance.Get(pageBuilder, applicationPath, retriever);
            var startupScript = pageBuilderAssetsProvider.GetStartupScriptTag(pageBuilderScriptConfiguration, new PreviewPathDecorator(false), featureSet);

            return new HtmlString(
                scriptBlock +
                formBuilderAssetsProvider.GetJQueryScriptBundleTags(false) +
                pageBuilderAssetsProvider.GetPageComponentsAdminScriptBundleTag() +
                formBuilderAssetsProvider.GetFormsScriptBundleTag() +
                startupScript
            );
        }


        /// <summary>
        /// Renders widget zone markup.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML markup for widget zone container for edit mode.</returns>
        /// <remarks>
        /// For the live site, <c>null</c> value is returned. The HTML markup of widgets included in the zone is rendered directly to the response.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString WidgetZone(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var context = instance.Target.ViewContext.HttpContext;
            var feature = context.Kentico().PageBuilder();
            var dataContext = feature.GetDataContext();
            var widgetDefinitionProvider = new ComponentDefinitionProvider<WidgetDefinition>();
            var conditionTypeDefinitionProvider = new ComponentDefinitionProvider<ConditionTypeDefinition>();
            var evaluator = new PersonalizationVariantEvaluator(conditionTypeDefinitionProvider, new PersonalizationAvailabilityChecker());
            var renderer = new WidgetZoneRenderer();

            return WidgetZoneInternal(instance, feature, renderer, dataContext, widgetDefinitionProvider, evaluator);
        }


        /// <summary>
        /// Renders widget zone markup.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="feature">Page builder feature.</param>
        /// <param name="renderer">Widgets zone renderer.</param>
        /// <param name="dataContext">The Page builder data context.</param>
        /// <param name="provider">Provider for component definitions.</param>
        /// <param name="evaluator">Evaluator to retrieve widget variant to be used for rendering.</param>
        internal static IHtmlString WidgetZoneInternal(this ExtensionPoint<HtmlHelper> instance, IPageBuilderFeature feature, IWidgetZoneRenderer renderer, IPageBuilderDataContext dataContext, IComponentDefinitionProvider<WidgetDefinition> provider, IWidgetVariantEvaluator evaluator)
        {
            if (feature.EditMode)
            {
                return renderer.RenderZoneContainer();
            }

            var configuration = dataContext.Configuration;
            renderer.RenderWidgets(instance.Target, configuration.Page, provider, evaluator);

            return null;
        }


        /// <summary>
        /// Renders editable area markup.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="areaIdentifier">Area identifier.</param>
        /// <param name="defaultSectionIdentifier">Identifier of a section to be used as an initial one for the area.</param>
        /// <param name="allowedWidgets">Identifiers of allowed widgets in the area. If none provided all widgets are allowed.</param>
        /// <returns>Returns HTML markup for editable area container for edit mode.</returns>
        /// <remarks>
        /// For the live site, <c>null</c> value is returned. The HTML markup of area and widgets included in the zones is rendered directly to the response.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="areaIdentifier"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="areaIdentifier"/> is empty or whitespace.</exception>
        public static IHtmlString EditableArea(this ExtensionPoint<HtmlHelper> instance, string areaIdentifier, string defaultSectionIdentifier = null, params string[] allowedWidgets)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (areaIdentifier == null)
            {
                throw new ArgumentNullException(nameof(areaIdentifier));
            }

            if (string.IsNullOrWhiteSpace(areaIdentifier))
            {
                throw new ArgumentException("The area identifier cannot be an empty string.", nameof(areaIdentifier));
            }

            var context = instance.Target.ViewContext.HttpContext;
            var feature = context.Kentico().PageBuilder();
            var dataContext = feature.GetDataContext();
            var sectionDefinitionProvider = new ComponentDefinitionProvider<SectionDefinition>();
            var renderer = new EditableAreaRenderer(areaIdentifier);

            return EditableAreaInternal(instance, feature, renderer, dataContext, sectionDefinitionProvider, defaultSectionIdentifier, allowedWidgets);
        }


        /// <summary>
        /// Renders editable area markup.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="feature">Page builder feature.</param>
        /// <param name="renderer">Editable area renderer.</param>
        /// <param name="dataContext">The Page builder data context.</param>
        /// <param name="provider">Provider for component definitions.</param>
        /// <param name="defaultSectionIdentifier">Identifier of a section to be used as an initial one for the area.</param>
        /// <param name="allowedWidgets">Identifiers of allowed widgets in the area. If none provided all widgets are allowed.</param>
        internal static IHtmlString EditableAreaInternal(this ExtensionPoint<HtmlHelper> instance, IPageBuilderFeature feature, IEditableAreaRenderer renderer, IPageBuilderDataContext dataContext, IComponentDefinitionProvider<SectionDefinition> provider, string defaultSectionIdentifier, IEnumerable<string> allowedWidgets)
        {
            if (feature.EditMode)
            {
                var defaultSection = GetDefaultSectionIdentifier(feature, defaultSectionIdentifier);
                if (!provider.GetAll().Any(section => section.Identifier.Equals(defaultSection, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new NotSupportedException($"The '{defaultSection}' default section identifier doesn't represent registered section.");
                }

                return renderer.RenderAreaContainer(defaultSection, allowedWidgets);
            }

            var configuration = dataContext.Configuration;
            renderer.RenderSections(instance.Target, configuration.Page, provider);

            return null;
        }


        private static string GetDefaultSectionIdentifier(IPageBuilderFeature feature, string defaultSectionIdentifier)
        {
            if (string.IsNullOrEmpty(defaultSectionIdentifier))
            {
                return feature.Options.DefaultSectionIdentifier;
            }

            return defaultSectionIdentifier;
        }
    }
}

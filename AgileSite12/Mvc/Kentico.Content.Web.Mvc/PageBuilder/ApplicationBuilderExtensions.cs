using System;
using System.Web.Optimization;

using Kentico.Components.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Enables the Page builder feature to compose page content based on registered widgets.
        /// </para>
        /// <para>
        /// The preview functionality must be enabled using <see cref="Kentico.Content.Web.Mvc.ApplicationBuilderExtensions.UsePreview"/>
        /// in order to use the Page builder.
        /// </para>
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="options">Page builder options.</param>
        /// <remarks>
        /// Enabling the Page builder feature also registers components' bundles to <see cref="BundleTable.Bundles"/>
        /// and prepares routes to be mapped once <see cref="RouteCollectionAddRoutesMethods.MapRoutes"/> is called.
        /// Inherently this method must be called prior to system routes mapping.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RouteCollectionAddRoutesMethods.MapRoutes"/> has already been called.</exception>
        public static void UsePageBuilder(this IApplicationBuilder builder, PageBuilderOptions options = null)
        {
            options = options ?? new PageBuilderOptions();

            PreviewResponseRedirectionModule.Initialize();

            PreviewUrlProcessor.Instance.Register(context =>
            {
                var feature = new PageBuilderFeature(context, options);
                context.Kentico().SetFeature<IPageBuilderFeature>(feature);
            });

            PreviewPathDecorator.OnDecorate.Execute += PageBuilderPreviewPathDecoratorHandlers.AddEditModeParameters;

            KenticoDefaultSection.Register(options);

            ScriptsBundleCollectionUtils.RegisterPageComponentsScriptBundle(BundleTable.Bundles);
            ScriptsBundleCollectionUtils.RegisterPageComponentsAdminScriptBundle(BundleTable.Bundles);

            StylesBundleCollectionUtils.RegisterPageComponentsStyleBundle(BundleTable.Bundles);
            StylesBundleCollectionUtils.RegisterPageComponentsAdminStyleBundle(BundleTable.Bundles);

            RouteRegistration.Instance.Add(routes => routes.Kentico().MapPageBuilderRoutes());
            RouteRegistration.Instance.Add(routes => routes.Kentico().MapPageTemplateRoutes());
            RouteRegistration.Instance.Add(routes => routes.Kentico().MapSelectorDialogsRoutes());
        }
    }
}

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Transparently handles preview URLs and also disables output caching in preview mode.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void UsePreview(this IApplicationBuilder builder)
        {
            PreviewResponseRedirectionModule.Initialize();

            VirtualContextUrlProcessorsRegister.Instance.Add(PreviewUrlProcessor.Instance);

            PreviewUrlProcessor.Instance.Register(context =>
            {
                var previewFeature = new PreviewFeature();
                context.Kentico().SetFeature<IPreviewFeature>(previewFeature);
            });
        }
    }
}

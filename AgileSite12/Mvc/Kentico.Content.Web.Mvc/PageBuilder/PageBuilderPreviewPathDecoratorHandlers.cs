using CMS.Base;
using CMS.DocumentEngine.PageBuilder;
using CMS.Helpers;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Page builder handlers for the PreviewPathDecorator.
    /// </summary>
    internal static class PageBuilderPreviewPathDecoratorHandlers
    {
        /// <summary>
        /// Adds the edit mode parameters to the given path.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CMSEventArgs{PreviewPathDecoratorEventArguments}"/> instance containing the event data.</param>
        public static void AddEditModeParameters(object sender, CMSEventArgs<PreviewPathDecoratorEventArguments> e)
        {
            var feature = CMSHttpContext.Current.Kentico().PageBuilder();
            if (feature.EditMode)
            {
                // Propagate the edit mode and editing instance parameters from the current context into the generated preview URLs
                var url = PageBuilderHelper.AddEditModeParameter(e.Parameter.Path);
                var featureProperties = feature.GetProperties();
                url = URLHelper.AddParameterToUrl(url, PageBuilderHelper.EDITING_INSTANCE_QUERY_NAME, featureProperties.EditingInstanceIdentifier.ToString());

                e.Parameter.Path = url;
            }
        }
    }
}

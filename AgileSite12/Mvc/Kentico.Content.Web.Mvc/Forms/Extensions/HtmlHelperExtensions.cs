using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.Core;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;
using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides system extension methods for <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/> extension point.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders necessary stylesheet link tags for using Form builder based forms on a live site.
        /// If no form component specific stylesheet is available, an empty string is returned.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <returns>Returns stylesheet link tags for form components, or an empty string.</returns>
        /// <remarks>
        /// Form components' additional stylesheets are expected to reside within the '~/Content/FormComponents' directory.
        /// CSS files in that directory are bundled and this method renders a link tag referencing the bundle
        /// in its <c>href</c> attribute. The bundle exists only if the aforementioned directory does.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="htmlHelper"/> is <c>null</c>.</exception>
        public static IHtmlString FormComponentsStyles(this ExtensionPoint<HtmlHelper> htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var assetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(htmlHelper.Target.ViewContext.RequestContext);

            return new HtmlString(
                assetsProvider.GetFormComponentsStylesheetLinkTag()
            );
        }


        /// <summary>
        /// Renders form zone container.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML markup representing form zone container.</returns>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public static IHtmlString FormZone(this ExtensionPoint<HtmlHelper> instance)
        {
            var sectionModel = instance.GetSectionModel();
            if (sectionModel == null)
            {
                return instance.RenderZone();
            }

            var zoneIndex = GetCurrentAndIncrementZoneIndex(instance.Target.ViewContext.RouteData.Values);
            if (sectionModel.ZonesContent.Count <= zoneIndex)
            {
                return MvcHtmlString.Empty;
            }

            var zoneFormComponents = sectionModel.ZonesContent[zoneIndex];

            return instance.RenderZone(zoneFormComponents, sectionModel.RenderingConfiguration);
        }
        

        /// <summary>
        /// Renders form zone container for form builder.
        /// </summary>
        /// <returns>Returns HTML markup representing form zone container.</returns>
        private static IHtmlString RenderZone(this ExtensionPoint<HtmlHelper> instance)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.Attributes.Add("data-kentico-widget-zone", "");

            return new HtmlString(tagBuilder.ToString());
        }


        /// <summary>
        /// Renders content of form zone container.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formComponents">Ordered list of form components to render the markup for.</param>
        /// <param name="renderingConfiguration">Configuration for the form fields rendering.</param>
        /// <returns>Returns HTML markup representing form zone container.</returns>
        private static IHtmlString RenderZone(this ExtensionPoint<HtmlHelper> instance, IList<FormComponent> formComponents, FormFieldRenderingConfiguration renderingConfiguration)
        {
            if ((formComponents == null) || (!formComponents.Any()))
            {
                return MvcHtmlString.Empty;
            }

            var parentViewData = instance.GetParentViewData();

            StringBuilder html = new StringBuilder();

            foreach (var formComponent in formComponents)
            {
                var formFieldHtml = instance.FormField(formComponent, parentViewData, renderingConfiguration, true);
                html.Append(formFieldHtml.ToHtmlString());
            }

            return new MvcHtmlString(html.ToString());
        }


        private static ViewDataDictionary GetParentViewData(this ExtensionPoint<HtmlHelper> instance)
        {
            return instance.Target.ViewContext.ParentActionViewContext?.ViewData;
        }


        private static SectionModel GetSectionModel(this ExtensionPoint<HtmlHelper> instance)
        {
            var parentViewData = instance.GetParentViewData();
            if (parentViewData == null)
            {
                return null;
            }

            if (!parentViewData.TryGetValue(FormBuilderConstants.SECTION_MODEL_DATA_KEY, out object sectionModel))
            {
                return null;
            }

            return sectionModel as SectionModel;
        }


        private static int GetCurrentAndIncrementZoneIndex(RouteValueDictionary routeData)
        {
            int zoneIndex = (int?)routeData[FormBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY] ?? default(int);
            routeData[FormBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY] = zoneIndex + 1;

            return zoneIndex;
        }
    }
}

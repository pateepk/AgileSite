using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retriever of a section markup URL.
    /// </summary>
    internal class SectionMarkupUrlRetriever : ISectionMarkupUrlRetriever
    {
        private readonly ICurrentHttpContextProvider currentHttpContextProvider;
        private readonly IPathDecorator pathDecorator;


        /// <summary>
        /// Initializes a new instance of the <see cref="SectionMarkupUrlRetriever"/> class using the specified HTTP context.
        /// </summary>
        /// <param name="currentHttpContextProvider">Provider of current HTTP context to be used by the URL retriever.</param>
        /// <param name="pathDecorator">Decorates paths with virtual context prefix to include Form builder context. If null then <see cref="FormBuilderPathDecorator"/> is used.</param>
        public SectionMarkupUrlRetriever(ICurrentHttpContextProvider currentHttpContextProvider, IPathDecorator pathDecorator = null)
        {
            this.currentHttpContextProvider = currentHttpContextProvider ?? throw new ArgumentNullException(nameof(currentHttpContextProvider));
            this.pathDecorator = pathDecorator ?? new FormBuilderPathDecorator();
        }


        /// <summary>
        /// Gets URL providing markup of a section.
        /// </summary>
        /// <param name="sectionDefinition">Section to retrieve URL for.</param>
        /// <returns>Returns URL providing the markup.</returns>
        /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
        public string GetUrl(SectionDefinition sectionDefinition)
        {
            if (sectionDefinition == null)
            {
                throw new ArgumentNullException(nameof(sectionDefinition));
            }

            var httpContext = currentHttpContextProvider.Get();
            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.SECTION_MARKUP_ROUTE_NAME, FormBuilderRoutes.DEFAULT_ACTION_NAME, sectionDefinition.ControllerName, null, RouteTable.Routes, httpContext.Request.RequestContext, false);
            if (String.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Default Form builder section markup URL cannot be retrieved. Make sure controller of the section '{sectionDefinition.Name}' is registered.");
            }

            return pathDecorator.Decorate(url);
        }
    }
}

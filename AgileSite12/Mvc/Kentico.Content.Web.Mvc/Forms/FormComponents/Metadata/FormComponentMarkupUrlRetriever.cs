using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Default retriever of a form component markup URL.
    /// </summary>
    internal class FormComponentMarkupUrlRetriever : IFormComponentMarkupUrlRetriever
    {
        private readonly ICurrentHttpContextProvider currentHttpContextProvider;
        private readonly IPathDecorator pathDecorator;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentMarkupUrlRetriever"/> class using the specified HTTP context.
        /// </summary>
        /// <param name="currentHttpContextProvider">Provider of current HTTP context to be used by the URL retriever.</param>
        /// <param name="pathDecorator">Decorates paths with virtual context prefix to include Form builder context. If null then <see cref="FormBuilderPathDecorator"/> is used.</param>
        public FormComponentMarkupUrlRetriever(ICurrentHttpContextProvider currentHttpContextProvider, IPathDecorator pathDecorator = null)
        {
            this.currentHttpContextProvider = currentHttpContextProvider ?? throw new ArgumentNullException(nameof(currentHttpContextProvider));
            this.pathDecorator = pathDecorator ?? new FormBuilderPathDecorator();
        }


        /// <summary>
        /// Gets URL providing markup of a form component.
        /// </summary>
        /// <param name="formComponentDefinition">Form component to retrieve URL for.</param>
        /// <param name="formId">Identifier of biz form the component belongs to.</param>
        /// <returns>Returns URL providing the markup.</returns>
        /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
        public string GetUrl(FormComponentDefinition formComponentDefinition, int formId)
        {
            if (formComponentDefinition == null)
            {
                throw new ArgumentNullException(nameof(formComponentDefinition));
            }

            var httpContext = currentHttpContextProvider.Get();
            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.MARKUP_ROUTE_NAME, nameof(KenticoFormComponentMarkupController.EditorRow), "KenticoFormComponentMarkup", new RouteValueDictionary(new { identifier = formComponentDefinition.Identifier, formId }), RouteTable.Routes, httpContext.Request.RequestContext, false);
            if (String.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Form component markup URL cannot be retrieved. Make sure the '{typeof(KenticoFormComponentMarkupController).FullName}' is registered.");
            }

            return pathDecorator.Decorate(url);
        }
    }
}

using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Builder.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Default retriever of a form component default properties URL.
    /// </summary>
    internal class FormComponentDefaultPropertiesUrlRetriever : IFormComponentDefaultPropertiesUrlRetriever
    {
        private readonly ICurrentHttpContextProvider currentHttpContextProvider;
        private readonly IPathDecorator pathDecorator;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentDefaultPropertiesUrlRetriever"/> class using the specified HTTP context.
        /// </summary>
        /// <param name="currentHttpContextProvider">Provider of current HTTP context to be used by the URL retriever.</param>
        /// <param name="pathDecorator">Decorates paths with virtual context prefix to include Form builder context. If null then <see cref="FormBuilderPathDecorator"/> is used.</param>
        public FormComponentDefaultPropertiesUrlRetriever(ICurrentHttpContextProvider currentHttpContextProvider, IPathDecorator pathDecorator = null)
        {
            this.currentHttpContextProvider = currentHttpContextProvider ?? throw new ArgumentNullException(nameof(currentHttpContextProvider));
            this.pathDecorator = pathDecorator ?? new FormBuilderPathDecorator();
        }


        /// <summary>
        /// Gets URL providing default properties of a form component.
        /// </summary>
        /// <param name="formComponentDefinition">Form component to retrieve URL for.</param>
        /// <param name="formId">Id of a BizFormInfo where form component will be used.</param>
        /// <returns>Returns URL providing the properties.</returns>
        /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
        public string GetUrl(FormComponentDefinition formComponentDefinition, int formId)
        {
            if (formComponentDefinition == null)
            {
                throw new ArgumentNullException(nameof(formComponentDefinition));
            }

            if (formId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(formId));
            }

            var httpContext = currentHttpContextProvider.Get();
            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.PROPERTIES_ROUTE_NAME, nameof(KenticoFormComponentConfigurationController.GetDefaultProperties), "KenticoFormComponentConfiguration", new RouteValueDictionary(new { httproute = true, identifier = formComponentDefinition.Identifier, formId = formId }), RouteTable.Routes, httpContext.Request.RequestContext, false);
            if (String.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Form component default properties URL cannot be retrieved. Make sure the '{typeof(KenticoFormComponentConfigurationController).FullName}' is registered.");
            }

            return pathDecorator.Decorate(url);
        }
    }
}

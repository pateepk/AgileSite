using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Base class for Page builder page template controller with custom properties of type <typeparamref name="TPropertiesType"/>.
    /// </summary>
    public abstract class PageTemplateController<TPropertiesType> : ComponentController<TPropertiesType>
        where TPropertiesType : class, IPageTemplateProperties, new()
    {
        /// <summary>
        /// Gets key to route data tokens to retrieve the component definition.
        /// </summary>
        internal override string ComponentDefinitionRouteDataKey { get; } = PageBuilderConstants.PAGE_TEMPLATE_DEFINITION_ROUTE_DATA_KEY;


        /// <summary>
        /// Creates an instance of <see cref="PageTemplateController{TPropertiesType}"/> class.
        /// </summary>
        protected PageTemplateController()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="PageTemplateController{TPropertiesType}"/> class.
        /// </summary>
        /// <param name="propertiesRetriever">Retriever for page template properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where the page template is used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected PageTemplateController(IComponentPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) :
            base(propertiesRetriever, currentPageRetriever)
        {
        }


        /// <summary>
        /// Gets retriever for page template properties.
        /// </summary>
        internal override IComponentPropertiesRetriever GetPropertiesRetriever()
        {
            var dataContext = HttpContext.Kentico().PageBuilder().GetDataContext();
            return new PageTemplatePropertiesRetriever(dataContext);
        }
    }


    /// <summary>
    /// Base class for Page builder page template controller without properties.
    /// </summary>
    public abstract class PageTemplateController : ComponentController
    {
        /// <summary>
        /// Gets key to route data tokens to retrieve the component definition.
        /// </summary>
        internal override string ComponentDefinitionRouteDataKey { get; } = PageBuilderConstants.PAGE_TEMPLATE_DEFINITION_ROUTE_DATA_KEY;


        /// <summary>
        /// Creates an instance of <see cref="PageTemplateController"/> class.
        /// </summary>
        protected PageTemplateController()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="PageTemplateController"/> class.
        /// </summary>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected PageTemplateController(ICurrentPageRetriever currentPageRetriever) :
            base(currentPageRetriever)
        {
        }


        /// <summary>
        /// Gets retriever for page template properties.
        /// </summary>
        internal override IComponentPropertiesRetriever GetPropertiesRetriever()
        {
            var dataContext = HttpContext.Kentico().PageBuilder().GetDataContext();
            return new PageTemplatePropertiesRetriever(dataContext);
        }
    }
}

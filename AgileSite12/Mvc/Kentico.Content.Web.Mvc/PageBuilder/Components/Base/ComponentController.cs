using System;
using System.Web.Mvc;

using CMS.DocumentEngine;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Base class for Page builder component controller.
    /// </summary>
    public abstract class ComponentController : Controller
    {
        private readonly ICurrentPageRetriever currentPageRetriever;

        /// <summary>
        /// Gets key to route data tokens to retrieve the component definition.
        /// </summary>
        internal virtual string ComponentDefinitionRouteDataKey { get; } = PageBuilderConstants.COMPONENT_DEFINITION_ROUTE_DATA_KEY;


        /// <summary>
        /// Creates an instance of <see cref="ComponentController"/> class.
        /// </summary>
        protected ComponentController() : this(new CurrentPageRetriever())
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="ComponentController"/> class.
        /// </summary>
        protected ComponentController(ICurrentPageRetriever currentPageRetriever)
        {
            this.currentPageRetriever = currentPageRetriever;
        }


        /// <summary>
        /// Gets the page where the component is placed.
        /// </summary>
        protected internal TreeNode GetPage()
        {
            var feature = HttpContext.Kentico().PageBuilder();
            return currentPageRetriever.Retrieve(feature);
        }


        /// <summary>
        /// Gets the page of given type where the component is placed.
        /// </summary>
        /// <typeparam name="TPageType">The type of the page to return.</typeparam>
        /// <exception cref="InvalidCastException">Is thrown when the page is not of given type.</exception>
        protected internal TPageType GetPage<TPageType>() where TPageType : TreeNode
        {
            if (GetPage() is TPageType page)
            {
                return page;
            }

            throw new InvalidCastException($"The current page is not of '{nameof(TPageType)}' type.");
        }


        /// <summary>
        /// Gets the retriever for component properties.
        /// </summary>
        internal virtual IComponentPropertiesRetriever GetPropertiesRetriever()
        {
            var securityChecker = new PageSecurityChecker(new VirtualContextPageRetriever());
            var postDataRetriever = new PageBuilderPostDataRetriever<PageBuilderPostData>(HttpContext, securityChecker);

            var routeDataRetriever = new PageBuilderRouteDataRetriever(RouteData);

            var serializer = new ComponentPropertiesSerializer();

            bool isInEditMode = HttpContext.Kentico().PageBuilder().EditMode;
            return new ComponentPropertiesRetriever(routeDataRetriever, postDataRetriever, serializer, isInEditMode);
        }


        /// <summary>
        /// Retrieves component definition.
        /// </summary>
        internal IPropertiesComponentDefinition GetComponentDefinition()
        {
            return RouteData.Values[ComponentDefinitionRouteDataKey] as IPropertiesComponentDefinition;
        }
    }


    /// <summary>
    /// Base class for Page builder component controller with custom properties of type <typeparamref name="TPropertiesType"/>.
    /// </summary>
    /// <typeparam name="TPropertiesType">The properties type of the component.</typeparam>
    public abstract class ComponentController<TPropertiesType> : ComponentController
        where TPropertiesType : class, IComponentProperties, new()
    {
        private readonly IComponentPropertiesRetriever<TPropertiesType> externalPropertiesRetriever;
        private IComponentPropertiesRetriever mPropertiesRetriever;


        private IComponentPropertiesRetriever PropertiesRetriever
        {
            get
            {
                if (mPropertiesRetriever == null)
                {
                    mPropertiesRetriever = GetPropertiesRetriever();
                }

                return mPropertiesRetriever;
            }
        }


        /// <summary>
        /// Creates an instance of <see cref="ComponentController{TPropertiesType}"/> class.
        /// </summary>
        protected ComponentController() : base(new CurrentPageRetriever())
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="ComponentController{TPropertiesType}"/> class.
        /// </summary>
        /// <param name="propertiesRetriever">Retriever for component properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where is the component used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected ComponentController(IComponentPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) :
            base(currentPageRetriever)
        {
            externalPropertiesRetriever = propertiesRetriever;
        }


        /// <summary>
        /// Gets properties of a component based on <typeparamref name="TPropertiesType"/> type.
        /// </summary>
        protected internal TPropertiesType GetProperties()
        {
            return GetPropertiesInternal();
        }


        internal virtual TPropertiesType GetPropertiesInternal()
        {
            return externalPropertiesRetriever != null ? externalPropertiesRetriever.Retrieve() : PropertiesRetriever.Retrieve<TPropertiesType>();
        }
    }
}
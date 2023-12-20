using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Base class for Page builder widget controller with custom properties of type <typeparamref name="TPropertiesType"/>.
    /// </summary>
    /// <typeparam name="TPropertiesType">The properties type of the widget.</typeparam>
    public abstract class WidgetController<TPropertiesType> : ComponentController<TPropertiesType>
        where TPropertiesType : class, IWidgetProperties, new()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IWidgetPropertiesRetriever<TPropertiesType> externalWidgetPropertiesRetriever;
#pragma warning restore CS0618 // Type or member is obsolete


        /// <summary>
        /// Creates an instance of <see cref="WidgetController{TPropertiesType}"/> class.
        /// </summary>
        protected WidgetController() : base()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetController{TPropertiesType}"/> class.
        /// </summary>
        /// <param name="propertiesRetriever">Retriever for widget properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where is the widget used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        [Obsolete("Use WidgetController(IComponentPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) constructor instead.")]
        protected WidgetController(IWidgetPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) :
            base(null, currentPageRetriever)
        {
            externalWidgetPropertiesRetriever = propertiesRetriever;
        }

        
        /// <summary>
        /// Creates an instance of <see cref="WidgetController{TPropertiesType}"/> class.
        /// </summary>
        /// <param name="propertiesRetriever">Retriever for widget properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where is the widget used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected WidgetController(IComponentPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) :
            base(propertiesRetriever, currentPageRetriever)
        {
        }


        internal override TPropertiesType GetPropertiesInternal()
        {
            // Temporary solution to support obsolete constructor for testability
            return externalWidgetPropertiesRetriever != null ? externalWidgetPropertiesRetriever.Retrieve() : base.GetPropertiesInternal();
        }
    }


    /// <summary>
    /// Base class for Page builder widget controller without properties.
    /// </summary>
    public abstract class WidgetController : ComponentController
    {
        /// <summary>
        /// Creates an instance of <see cref="WidgetController"/> class.
        /// </summary>
        protected WidgetController() : base()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetController"/> class.
        /// </summary>
        /// <param name="currentPageRetriever">Retriever for current page where is the widget used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected WidgetController(ICurrentPageRetriever currentPageRetriever) : base(currentPageRetriever)
        {
        }
    }
}
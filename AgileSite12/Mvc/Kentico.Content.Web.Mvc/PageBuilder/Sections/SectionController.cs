namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Base class for Page builder section controller with custom properties of type <typeparamref name="TPropertiesType"/>.
    /// </summary>
    /// <typeparam name="TPropertiesType">The properties type of the section.</typeparam>
    public abstract class SectionController<TPropertiesType> : ComponentController<TPropertiesType>
        where TPropertiesType : class, ISectionProperties, new()
    {
        /// <summary>
        /// Creates an instance of <see cref="SectionController{TPropertiesType}"/> class.
        /// </summary>
        protected SectionController() : base()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="SectionController{TPropertiesType}"/> class.
        /// </summary>
        /// <param name="propertiesRetriever">Retriever for section properties.</param>
        /// <param name="currentPageRetriever">Retriever for current page where is the section used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected SectionController(IComponentPropertiesRetriever<TPropertiesType> propertiesRetriever, ICurrentPageRetriever currentPageRetriever) :
            base(propertiesRetriever, currentPageRetriever)
        {
        }
    }


    /// <summary>
    /// Base class for Page builder section controller without properties.
    /// </summary>
    public abstract class SectionController : ComponentController
    {
        /// <summary>
        /// Creates an instance of <see cref="SectionController"/> class.
        /// </summary>
        protected SectionController() : base()
        {
        }


        /// <summary>
        /// Creates an instance of <see cref="SectionController"/> class.
        /// </summary>
        /// <param name="currentPageRetriever">Retriever for current page where is the section used.</param>
        /// <remarks>Use this constructor for tests to handle dependencies.</remarks>
        protected SectionController(ICurrentPageRetriever currentPageRetriever) : base(currentPageRetriever)
        {
        }
    }
}
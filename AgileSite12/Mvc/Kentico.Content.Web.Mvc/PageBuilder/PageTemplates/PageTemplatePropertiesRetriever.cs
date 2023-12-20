using System;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Retrieves properties for a page template controller from request body or route data.
    /// </summary>
    internal class PageTemplatePropertiesRetriever : IComponentPropertiesRetriever
    {
        private readonly IPageBuilderDataContext dataContext;


        /// <summary>
        /// Creates an instance of <see cref="PageTemplatePropertiesRetriever"/> class.
        /// </summary>
        /// <param name="dataContext">The Page builder data context.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="dataContext"/> is <c>null</c>.</exception>
        public PageTemplatePropertiesRetriever(IPageBuilderDataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }


        /// <summary>
        /// Retrieves page template properties.
        /// </summary>
        public IComponentProperties Retrieve(Type propertiesType)
        {
            var configuration = dataContext.Configuration.PageTemplate;
            if (configuration == null)
            {
                throw new InvalidOperationException("The page doesn't use page templates.");
            }

            return configuration.Properties;
        }


        /// <summary>
        /// Retrieves page template properties.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the page template properties.</typeparam>
        public TPropertiesType Retrieve<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new()
        {
            var configuration = dataContext.Configuration.PageTemplate;
            if (configuration == null)
            {
                throw new InvalidOperationException("The page doesn't use page templates.");
            }

            return (TPropertiesType)configuration.Properties;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    internal class PageTemplateDefinitionProvider
    {
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> mPageTemplatesProvider;
        private readonly IEnumerable<IPageTemplateFilter> mFilterCollection;


        /// <summary>
        /// Creates a new instance of <see cref="PageTemplateDefinitionProvider"/>.
        /// </summary>
        public PageTemplateDefinitionProvider()
            : this(new ComponentDefinitionProvider<PageTemplateDefinition>(), PageBuilderFilters.PageTemplates)
        {
        }


        internal PageTemplateDefinitionProvider(IComponentDefinitionProvider<PageTemplateDefinition> pageTemplatesProvider, IEnumerable<IPageTemplateFilter> filterCollection)
        {
            mPageTemplatesProvider = pageTemplatesProvider ?? throw new ArgumentNullException(nameof(pageTemplatesProvider));
            mFilterCollection = filterCollection ?? throw new ArgumentNullException(nameof(filterCollection));
        }


        /// <summary>
        /// Applies registered page template filters on a collection of all templates and returns a result collection.
        /// </summary>
        /// <param name="context">Filter context which is passed to each filter.</param>
        /// <returns>Filtered template collection.</returns>
        public virtual IEnumerable<PageTemplateDefinition> GetFilteredTemplates(PageTemplateFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IEnumerable<PageTemplateDefinition> filteredPageTemplates = mPageTemplatesProvider.GetAll().ToList();

            if (filteredPageTemplates.Any())
            {
                foreach (IPageTemplateFilter filter in mFilterCollection)
                {
                    filteredPageTemplates = filter.Filter(filteredPageTemplates, context);
                }
            }

            return filteredPageTemplates;
        }
    }
}

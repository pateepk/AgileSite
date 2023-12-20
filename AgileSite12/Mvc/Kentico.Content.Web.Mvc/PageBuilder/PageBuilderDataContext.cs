using System;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.Helpers;

using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// The data context for the Page builder feature.
    /// </summary>
    internal sealed class PageBuilderDataContext : IPageBuilderDataContext
    {
        private readonly int pageIdentifier;

        private readonly Lazy<TreeNode> mPage;
        private readonly Lazy<PageBuilderConfiguration> mConfiguration;

        private readonly IPageRetriever pageRetriever;
        private readonly IPageBuilderConfigurationManager manager;


        /// <summary>
        /// Gets the page where the Page builder stores and loads data from.
        /// </summary>
        public TreeNode Page => mPage.Value;


        /// <summary>
        /// Gets the Page builder configuration.
        /// </summary>
        public PageBuilderConfiguration Configuration => mConfiguration.Value;


        /// <summary>
        /// Creates an instance of <see cref="PageBuilderDataContext"/> class.
        /// </summary>
        /// <param name="pageIdentifier">Identifier of page where the Page builder is initialized.</param>
        /// <param name="editingInstanceIdentifier">The editing instance identifier.</param>
        public PageBuilderDataContext(int pageIdentifier, Guid editingInstanceIdentifier) :
            this(pageIdentifier, new PageRetriever())
        {
            var editableAreasSerializer = new EditableAreasConfigurationSerializer();
            var templateSerializer = new PageTemplateConfigurationSerializer();
            var loader = new PageBuilderConfigurationSourceLoader(editingInstanceIdentifier, Service.Resolve<IPageBuilderConfigurationSourceLoader>());
            var widgetDefinitionProvider = new ComponentDefinitionProvider<WidgetDefinition>();
            var sectionDefinitionProvider = new ComponentDefinitionProvider<SectionDefinition>();
            var pageTemplateDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();
            var conditionTypeDefinitionProvider = new ComponentDefinitionProvider<ConditionTypeDefinition>();
            var previewLinksConverter = new PreviewLinksConverter(SystemContext.ApplicationPath);

            manager = new PageBuilderConfigurationManager(editableAreasSerializer, templateSerializer, loader, widgetDefinitionProvider, sectionDefinitionProvider, pageTemplateDefinitionProvider, conditionTypeDefinitionProvider, previewLinksConverter);
        }


        /// <summary>
        /// Creates an instance of <see cref="PageBuilderDataContext"/> class.
        /// </summary>
        /// <param name="pageIdentifier">Identifier of page where the Page builder is initialized.</param>
        /// <param name="manager">Page builder configuration manager.</param>
        /// <param name="pageRetriever">Retriever for a page initialized from the Page builder context.</param>
        public PageBuilderDataContext(int pageIdentifier, IPageBuilderConfigurationManager manager, IPageRetriever pageRetriever) :
            this(pageIdentifier, pageRetriever)
        {
            this.manager = manager;
        }


        private PageBuilderDataContext(int pageIdentifier, IPageRetriever pageRetriever)
        {
            if (pageIdentifier <= 0)
            {
                throw new ArgumentException("The Page builder feature needs to be initialized with identifier of an existing page.", nameof(pageIdentifier));
            }

            this.pageIdentifier = pageIdentifier;
            this.pageRetriever = pageRetriever;

            mPage = new Lazy<TreeNode>(GetPage);
            mConfiguration = new Lazy<PageBuilderConfiguration>(GetConfiguration);
        }


        private TreeNode GetPage()
        {
            return pageRetriever.Retrieve(pageIdentifier, VirtualContext.IsPreviewLinkInitialized);
        }


        private PageBuilderConfiguration GetConfiguration()
        {
            return manager.Load(Page);
        }
    }
}
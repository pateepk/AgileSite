using System;
using System.Linq;

using CMS.DataEngine;

using Kentico.Builder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Builds page templates view model.
    /// </summary>
    internal class PageTemplatesViewModelBuilder : IPageTemplatesViewModelBuilder
    {
        private readonly IComponentLocalizationService localizationService;
        private readonly PageTemplateDefinitionProvider defaultTemplatesProvider;
        private readonly CustomPageTemplateProvider customTemplatesProvider;


        /// <summary>
        /// Creates an instance of the <see cref="PageTemplatesViewModelBuilder"/> class.
        /// </summary>
        /// <param name="localizationService">Localization service to localize page template metadata.</param>
        /// <param name="defaultTemplatesProvider">Provider for default page templates retrieval.</param>
        /// <param name="customTemplatesProvider">Provider for custom page templates retrieval.</param>
        public PageTemplatesViewModelBuilder(IComponentLocalizationService localizationService, PageTemplateDefinitionProvider defaultTemplatesProvider,
            CustomPageTemplateProvider customTemplatesProvider)
        {
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.defaultTemplatesProvider = defaultTemplatesProvider ?? throw new ArgumentNullException(nameof(defaultTemplatesProvider));
            this.customTemplatesProvider = customTemplatesProvider ?? throw new ArgumentNullException(nameof(customTemplatesProvider));
        }


        /// <summary>
        /// Builds page templates view mode.
        /// </summary>
        /// <param name="filterContext">Filter context which is passed to each filter.</param>
        public PageTemplatesViewModel Build(PageTemplateFilterContext filterContext)
        {
            var defaultTemplates = defaultTemplatesProvider.GetFilteredTemplates(filterContext);
            var customTemplates = customTemplatesProvider.GetTemplates(defaultTemplates);

            return new PageTemplatesViewModel
            {
                DefaultPageTemplates = defaultTemplates?
                    .Select(definition => new DefaultPageTemplateViewModel
                    {
                        Name = localizationService.LocalizeString(definition.Name),
                        Identifier = definition.Identifier,
                        Description = localizationService.LocalizeString(definition.Description),
                        IconClass = definition.IconClass
                    })
                    .ToList() ?? Enumerable.Empty<DefaultPageTemplateViewModel>(),

                CustomPageTemplates = customTemplates?
                    .Select(configuration => new CustomPageTemplateViewModel
                    {
                        Name = localizationService.LocalizeString(configuration.PageTemplateConfigurationName),
                        Identifier = configuration.PageTemplateConfigurationGUID.ToString(),
                        Description = localizationService.LocalizeString(configuration.PageTemplateConfigurationDescription),
                        ImagePath = configuration.PageTemplateConfigurationThumbnailGUID != Guid.Empty ? MetaFileURLProvider.GetMetaFileUrl(configuration.PageTemplateConfigurationThumbnailGUID, string.Empty) : null,
                    })
                    .ToList() ?? Enumerable.Empty<CustomPageTemplateViewModel>(),
            };
        }
    }
}

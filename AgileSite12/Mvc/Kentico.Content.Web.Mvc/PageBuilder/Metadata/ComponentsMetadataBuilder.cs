using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Builds components metadata.
    /// </summary>
    internal sealed class ComponentsMetadataBuilder : IComponentsMetadataBuilder
    {
        private readonly IComponentUrlRetriever<IMarkupComponentDefinition> markupUrlRetriever;
        private readonly IComponentUrlRetriever<IMarkupComponentDefinition> administrationMarkupUrlRetriever;
        private readonly IComponentUrlRetriever<IPropertiesComponentDefinition> defaultPropertiesUrlRetriever;
        private readonly IComponentUrlRetriever<IPropertiesComponentDefinition> propertiesFormMarkupUrlRetriever;
        private readonly IComponentDefinitionProvider<WidgetDefinition> widgetsProvider;
        private readonly IComponentDefinitionProvider<SectionDefinition> sectionsProvider;
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> pageTemplatesProvider;
        private readonly IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypesProvider;
        private readonly IComponentLocalizationService localizationService;
        private readonly IEditablePropertiesCollector editablePropertiesCollector;


        /// <summary>
        /// Creates an instance of the <see cref="ComponentsMetadataBuilder"/> class.
        /// </summary>
        /// <param name="widgetsProvider">Provider to retrieve list of registered widgets.</param>
        /// <param name="sectionsProvider">Provider to retrieve list of registered sections.</param>
        /// <param name="pageTemplatesProvider">Provider to retrieve list of registered page templates.</param>
        /// <param name="conditionTypesProvider">Provider to retrieve list of registered personalization condition types.</param>
        /// <param name="markupUrlRetriever">Retriever to get the component markup URL.</param>
        /// <param name="administrationMarkupUrlRetriever">Retriever to get the markup of a component used for the administration UI.</param>
        /// <param name="defaultPropertiesUrlRetriever">Retriever to get the component default properties URL.</param>
        /// <param name="propertiesFormMarkupUrlRetriever">Retriever to get the component properties form markup.</param>
        /// <param name="localizationService">Localization service to localize component metadata.</param>
        /// <param name="editablePropertiesCollector">Collector of component's editable properties.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="widgetsProvider"/>, <paramref name="sectionsProvider"/>, <paramref name="conditionTypesProvider"/>, <paramref name="markupUrlRetriever"/>, 
        /// <paramref name="administrationMarkupUrlRetriever" />, <paramref name="defaultPropertiesUrlRetriever"/>, <paramref name="propertiesFormMarkupUrlRetriever"/>, <paramref name="localizationService"/> or <paramref name="editablePropertiesCollector" /> is <c>null</c>.
        /// </exception>
        public ComponentsMetadataBuilder(IComponentDefinitionProvider<WidgetDefinition> widgetsProvider, 
                                         IComponentDefinitionProvider<SectionDefinition> sectionsProvider,
                                         IComponentDefinitionProvider<PageTemplateDefinition> pageTemplatesProvider,
                                         IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypesProvider,
                                         IComponentUrlRetriever<IMarkupComponentDefinition> markupUrlRetriever,
                                         IComponentUrlRetriever<IMarkupComponentDefinition> administrationMarkupUrlRetriever,
                                         IComponentUrlRetriever<IPropertiesComponentDefinition> defaultPropertiesUrlRetriever, 
                                         IComponentUrlRetriever<IPropertiesComponentDefinition> propertiesFormMarkupUrlRetriever, 
                                         IComponentLocalizationService localizationService,
                                         IEditablePropertiesCollector editablePropertiesCollector)
        {
            this.widgetsProvider = widgetsProvider ?? throw new ArgumentNullException(nameof(widgetsProvider));
            this.sectionsProvider = sectionsProvider ?? throw new ArgumentNullException(nameof(sectionsProvider));
            this.pageTemplatesProvider = pageTemplatesProvider ?? throw new ArgumentNullException(nameof(pageTemplatesProvider));
            this.conditionTypesProvider = conditionTypesProvider ?? throw new ArgumentNullException(nameof(conditionTypesProvider));
            this.markupUrlRetriever = markupUrlRetriever ?? throw new ArgumentNullException(nameof(markupUrlRetriever));
            this.administrationMarkupUrlRetriever = administrationMarkupUrlRetriever ?? throw new ArgumentNullException(nameof(administrationMarkupUrlRetriever));
            this.propertiesFormMarkupUrlRetriever = propertiesFormMarkupUrlRetriever ?? throw new ArgumentNullException(nameof(propertiesFormMarkupUrlRetriever));
            this.defaultPropertiesUrlRetriever = defaultPropertiesUrlRetriever ?? throw new ArgumentNullException(nameof(defaultPropertiesUrlRetriever));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.editablePropertiesCollector = editablePropertiesCollector ?? throw new ArgumentNullException(nameof(editablePropertiesCollector));
        }


        /// <summary>
        /// Gets metadata for all registered components.
        /// </summary>
        public ComponentsMetadata GetAll()
        {
            var registeredWidgets = widgetsProvider.GetAll();
            var registeredSections = sectionsProvider.GetAll();
            var registeredConditionTypes = conditionTypesProvider.GetAll();
            var registeredPageTemplates = pageTemplatesProvider.GetAll();

            return GetComponentsMetadata(registeredWidgets, registeredSections, registeredPageTemplates, registeredConditionTypes);
        }


        private ComponentsMetadata GetComponentsMetadata(IEnumerable<WidgetDefinition> widgets, IEnumerable<SectionDefinition> sections, IEnumerable<PageTemplateDefinition> pageTemplates, IEnumerable<ConditionTypeDefinition> conditionTypes)
        {
            return new ComponentsMetadata
            {
                Widgets = widgets.Select(CreateWidgetMetadata).OrderBy(m => m.Name).ToList(),
                Sections = sections.Select(CreateSectionMetadata).OrderBy(m => m.Name).ToList(),
                PageTemplates = pageTemplates.Select(CreatePageTemplateMetadata).OrderBy(m => m.Name).ToList(),
                PersonalizationConditionTypes = conditionTypes.Select(CreatePersonalizationConditionMetadata).OrderBy(m => m.Name).ToList()
            };
        }


        private WidgetMetadata CreateWidgetMetadata(WidgetDefinition widget)
        {
            return new WidgetMetadata
            {
                TypeIdentifier = widget.Identifier,
                MarkupUrl = markupUrlRetriever.GetUrl(widget),
                DefaultPropertiesUrl = defaultPropertiesUrlRetriever.GetUrl(widget),
                PropertiesFormMarkupUrl = propertiesFormMarkupUrlRetriever.GetUrl(widget),
                Name = localizationService.LocalizeString(widget.Name),
                Description = localizationService.LocalizeString(widget.Description),
                IconClass = widget.IconClass,
                HasProperties = !(widget.PropertiesType is null),
                HasEditableProperties = (widget.PropertiesType != null) && editablePropertiesCollector.GetEditableProperties(Activator.CreateInstance(widget.PropertiesType)).Any(),
            };
        }


        private SectionMetadata CreateSectionMetadata(SectionDefinition section)
        {
            return new SectionMetadata
            {
                TypeIdentifier = section.Identifier,
                MarkupUrl = markupUrlRetriever.GetUrl(section),
                DefaultPropertiesUrl = defaultPropertiesUrlRetriever.GetUrl(section),
                PropertiesFormMarkupUrl = propertiesFormMarkupUrlRetriever.GetUrl(section),
                Name = localizationService.LocalizeString(section.Name),
                Description = localizationService.LocalizeString(section.Description),
                IconClass = section.IconClass,
                HasProperties = !(section.PropertiesType is null) && editablePropertiesCollector.GetEditableProperties(Activator.CreateInstance(section.PropertiesType)).Any(),
            };
        }


        private PageTemplateMetadata CreatePageTemplateMetadata(PageTemplateDefinition template)
        {
            return new PageTemplateMetadata
            {
                TypeIdentifier = template.Identifier,
                DefaultPropertiesUrl = defaultPropertiesUrlRetriever.GetUrl(template),
                PropertiesFormMarkupUrl = propertiesFormMarkupUrlRetriever.GetUrl(template),
                Name = localizationService.LocalizeString(template.Name),
                Description = localizationService.LocalizeString(template.Description),
                IconClass = template.IconClass,
                HasProperties = !(template.PropertiesType is null) && editablePropertiesCollector.GetEditableProperties(Activator.CreateInstance(template.PropertiesType)).Any(),
            };
        }


        private ConditionTypeMetadata CreatePersonalizationConditionMetadata(ConditionTypeDefinition condition)
        {
            return new ConditionTypeMetadata
            {
                TypeIdentifier = condition.Identifier,
                MarkupUrl = administrationMarkupUrlRetriever.GetUrl(condition),
                Name = localizationService.LocalizeString(condition.Name),
                Description = localizationService.LocalizeString(condition.Description),
                IconClass = condition.IconClass,
                Hint = localizationService.LocalizeString(condition.Hint)
            };
        }
    }
}
